using System;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace SchoolAdBlocker.Core.Proxy
{
    public sealed class SystemProxyManager
    {
        private const string InternetSettingsKey = @"Software\Microsoft\Windows\CurrentVersion\Internet Settings";
        private int? _proxyEnable;
        private string _proxyServer;
        private string _proxyOverride;
        private int? _autoDetect;
        private string _autoConfigUrl;

        public void SaveCurrent()
        {
            using (var key = Registry.CurrentUser.OpenSubKey(InternetSettingsKey, false))
            {
                _proxyEnable = (int?)key?.GetValue("ProxyEnable", 0);
                _proxyServer = (string)key?.GetValue("ProxyServer", string.Empty);
                _proxyOverride = (string)key?.GetValue("ProxyOverride", string.Empty);
                _autoDetect = (int?)key?.GetValue("AutoDetect", 0);
                _autoConfigUrl = (string)key?.GetValue("AutoConfigURL", string.Empty);
            }
        }

        public void ApplyLocalProxy(string proxyServer)
        {
            using (var key = Registry.CurrentUser.OpenSubKey(InternetSettingsKey, true))
            {
                if (key == null) return;

                key.SetValue("ProxyEnable", 1, RegistryValueKind.DWord);
                key.SetValue("ProxyServer", proxyServer, RegistryValueKind.String);
                key.SetValue("ProxyOverride", "<local>", RegistryValueKind.String);
                key.SetValue("AutoDetect", 0, RegistryValueKind.DWord);
                key.SetValue("AutoConfigURL", string.Empty, RegistryValueKind.String);
            }

            ApplyWinInetProxyOptions(true, proxyServer, "<local>");
            RefreshWinInet();
        }

        public void Restore()
        {
            using (var key = Registry.CurrentUser.OpenSubKey(InternetSettingsKey, true))
            {
                if (key == null) return;

                if (_proxyEnable.HasValue)
                    key.SetValue("ProxyEnable", _proxyEnable.Value, RegistryValueKind.DWord);

                key.SetValue("ProxyServer", _proxyServer ?? string.Empty, RegistryValueKind.String);
                key.SetValue("ProxyOverride", _proxyOverride ?? string.Empty, RegistryValueKind.String);
                if (_autoDetect.HasValue)
                    key.SetValue("AutoDetect", _autoDetect.Value, RegistryValueKind.DWord);
                key.SetValue("AutoConfigURL", _autoConfigUrl ?? string.Empty, RegistryValueKind.String);
            }

            var enable = _proxyEnable.HasValue && _proxyEnable.Value != 0;
            ApplyWinInetProxyOptions(enable, _proxyServer ?? string.Empty, _proxyOverride ?? string.Empty);
            RefreshWinInet();
        }

        private static void ApplyWinInetProxyOptions(bool enable, string proxyServer, string proxyBypass)
        {
            IntPtr proxyServerPtr = IntPtr.Zero;
            IntPtr proxyBypassPtr = IntPtr.Zero;
            IntPtr optionsPtr = IntPtr.Zero;
            IntPtr listPtr = IntPtr.Zero;

            try
            {
                proxyServerPtr = Marshal.StringToHGlobalUni(proxyServer ?? string.Empty);
                proxyBypassPtr = Marshal.StringToHGlobalUni(proxyBypass ?? string.Empty);

                var options = new INTERNET_PER_CONN_OPTION[3];
                options[0] = new INTERNET_PER_CONN_OPTION
                {
                    Option = INTERNET_PER_CONN_FLAGS,
                    Value = new INTERNET_PER_CONN_OPTION_VALUE
                    {
                        dwValue = enable ? (PROXY_TYPE_DIRECT | PROXY_TYPE_PROXY) : PROXY_TYPE_DIRECT
                    }
                };

                options[1] = new INTERNET_PER_CONN_OPTION
                {
                    Option = INTERNET_PER_CONN_PROXY_SERVER,
                    Value = new INTERNET_PER_CONN_OPTION_VALUE
                    {
                        pszValue = proxyServerPtr
                    }
                };

                options[2] = new INTERNET_PER_CONN_OPTION
                {
                    Option = INTERNET_PER_CONN_PROXY_BYPASS,
                    Value = new INTERNET_PER_CONN_OPTION_VALUE
                    {
                        pszValue = proxyBypassPtr
                    }
                };

                var optionSize = Marshal.SizeOf(typeof(INTERNET_PER_CONN_OPTION));
                optionsPtr = Marshal.AllocCoTaskMem(optionSize * options.Length);

                for (var i = 0; i < options.Length; i++)
                {
                    var ptr = IntPtr.Add(optionsPtr, i * optionSize);
                    Marshal.StructureToPtr(options[i], ptr, false);
                }

                var list = new INTERNET_PER_CONN_OPTION_LIST
                {
                    Size = Marshal.SizeOf(typeof(INTERNET_PER_CONN_OPTION_LIST)),
                    Connection = IntPtr.Zero,
                    OptionCount = options.Length,
                    OptionError = 0,
                    Options = optionsPtr
                };

                listPtr = Marshal.AllocCoTaskMem(list.Size);
                Marshal.StructureToPtr(list, listPtr, false);

                InternetSetOption(IntPtr.Zero, INTERNET_OPTION_PER_CONNECTION_OPTION, listPtr, list.Size);
            }
            finally
            {
                if (listPtr != IntPtr.Zero) Marshal.FreeCoTaskMem(listPtr);
                if (optionsPtr != IntPtr.Zero) Marshal.FreeCoTaskMem(optionsPtr);
                if (proxyServerPtr != IntPtr.Zero) Marshal.FreeHGlobal(proxyServerPtr);
                if (proxyBypassPtr != IntPtr.Zero) Marshal.FreeHGlobal(proxyBypassPtr);
            }
        }

        private static void RefreshWinInet()
        {
            InternetSetOption(IntPtr.Zero, INTERNET_OPTION_SETTINGS_CHANGED, IntPtr.Zero, 0);
            InternetSetOption(IntPtr.Zero, INTERNET_OPTION_REFRESH, IntPtr.Zero, 0);
        }

        private const int INTERNET_OPTION_SETTINGS_CHANGED = 39;
        private const int INTERNET_OPTION_REFRESH = 37;
        private const int INTERNET_OPTION_PER_CONNECTION_OPTION = 75;

        private const int INTERNET_PER_CONN_FLAGS = 1;
        private const int INTERNET_PER_CONN_PROXY_SERVER = 2;
        private const int INTERNET_PER_CONN_PROXY_BYPASS = 3;

        private const int PROXY_TYPE_DIRECT = 0x00000001;
        private const int PROXY_TYPE_PROXY = 0x00000002;

        [StructLayout(LayoutKind.Sequential)]
        private struct INTERNET_PER_CONN_OPTION_LIST
        {
            public int Size;
            public IntPtr Connection;
            public int OptionCount;
            public int OptionError;
            public IntPtr Options;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct INTERNET_PER_CONN_OPTION
        {
            public int Option;
            public INTERNET_PER_CONN_OPTION_VALUE Value;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct INTERNET_PER_CONN_OPTION_VALUE
        {
            [FieldOffset(0)]
            public int dwValue;
            [FieldOffset(0)]
            public IntPtr pszValue;
        }

        public string GetCurrentProxyInfo()
        {
            using (var key = Registry.CurrentUser.OpenSubKey(InternetSettingsKey, false))
            {
                var enabled = (int?)key?.GetValue("ProxyEnable", 0) ?? 0;
                var server = (string)key?.GetValue("ProxyServer", string.Empty) ?? string.Empty;
                var autoDetect = (int?)key?.GetValue("AutoDetect", 0) ?? 0;
                var autoConfig = (string)key?.GetValue("AutoConfigURL", string.Empty) ?? string.Empty;

                return string.Format("ProxyEnable={0}; ProxyServer={1}; AutoDetect={2}; AutoConfigURL={3}", enabled, server, autoDetect, autoConfig);
            }
        }

        [DllImport("wininet.dll", SetLastError = true)]
        private static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int dwBufferLength);
    }
}