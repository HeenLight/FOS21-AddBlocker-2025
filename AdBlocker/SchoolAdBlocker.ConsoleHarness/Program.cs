using System;
using System.Net.Http;
using System.Threading;
using SchoolAdBlocker.Core.Blocklists;
using SchoolAdBlocker.Core.Filtering;
using SchoolAdBlocker.Core.Proxy;
using SchoolAdBlocker.Core.Storage;
using SchoolAdBlocker.Core.Utils;

namespace SchoolAdBlocker.ConsoleHarness
{
    internal static class ConsoleHarness
    {
        public static void Run()
        {
            var baseDir = AppContext.BaseDirectory;
            Guard.EnsureDirectory(System.IO.Path.Combine(baseDir, "cache"));
            Guard.EnsureDirectory(System.IO.Path.Combine(baseDir, "logs"));

            var settings = new SettingsStore().LoadOrCreateDefault(baseDir);

            var cache = new BlocklistCacheStore(baseDir);
            var parser = new HostsParser();
            var fetcher = new RemoteListFetcher(new HttpClient());
            var updater = new BlocklistUpdater(cache, parser, fetcher);

            var rules = new RuleEngine();
            rules.UpdateDomains(updater.LoadFromCacheOrEmpty());

            var proxy = new ProxyHost();
            var sysProxy = new SystemProxyManager();

            using (var cts = new CancellationTokenSource())
            {
                try
                {
                    sysProxy.SaveCurrent();
                    sysProxy.ApplyLocalProxy(string.Format("127.0.0.1:{0}", settings.Port));

                    updater.UpdateNowAsync(settings.Sources, cts.Token)
                        .ContinueWith(t => { if (t.Result != null) rules.UpdateDomains(t.Result); });

                    proxy.Start(settings.Port, rules, cts.Token);

                    Console.WriteLine("Proxy running. Press Enter to stop.");
                    Console.ReadLine();
                }
                finally
                {
                    cts.Cancel();
                    proxy.Stop();
                    try { sysProxy.Restore(); } catch { }
                }
            }
        }
    }
}