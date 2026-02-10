using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using SchoolAdBlocker.Core.Filtering;
using SchoolAdBlocker.Core.Models;
using Titanium.Web.Proxy;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Models;

namespace SchoolAdBlocker.Core.Proxy
{
    public sealed class ProxyHost
    {
        public event Action<LogEvent> OnLog;
        public event Action<ProxyStats> OnStatsChanged;

        private ProxyServer _server;
        private ExplicitProxyEndPoint _endPoint;
        private AsyncEventHandler<SessionEventArgs> _beforeRequestHandler;
        private readonly object _sync = new object();

        private long _total;
        private long _blocked;
        private long _allowed;

        public void Start(int port, RuleEngine rules, CancellationToken ct)
        {
            lock (_sync)
            {
                if (_server != null) return;

                _server = new ProxyServer();
                _server.ForwardToUpstreamGateway = true;

                _endPoint = new ExplicitProxyEndPoint(IPAddress.Loopback, port, false);
                _server.AddEndPoint(_endPoint);

                _beforeRequestHandler = async (s, e) =>
                {
                    var host = e.HttpClient.Request.Host;
                    var isConnect = string.Equals(e.HttpClient.Request.Method, "CONNECT", StringComparison.OrdinalIgnoreCase);
                    HandleRequest(host, rules, isConnect: isConnect, e);
                    await Task.CompletedTask;
                };

                _server.BeforeRequest += _beforeRequestHandler;
            }

            _server.Start();

            Task.Run(async () =>
            {
                try
                {
                    while (!ct.IsCancellationRequested)
                        await Task.Delay(500, ct);
                }
                catch { }
                Stop();
            }, ct);

            Log("Info", string.Format("Proxy started on 127.0.0.1:{0}", port));
        }

        public void Stop()
        {
            ProxyServer server;
            AsyncEventHandler<SessionEventArgs> handler;

            lock (_sync)
            {
                if (_server == null) return;
                server = _server;
                handler = _beforeRequestHandler;
                _server = null;
                _endPoint = null;
                _beforeRequestHandler = null;
            }

            try
            {
                if (handler != null)
                    server.BeforeRequest -= handler;

                server.Stop();
                server.Dispose();
                Log("Info", "Proxy stopped");
            }
            catch (ObjectDisposedException)
            {
                Log("Info", "Proxy stopped");
            }
        }

        private void HandleRequest(string host, RuleEngine rules, bool isConnect, SessionEventArgs e)
        {
            host = NormalizeHost(host);
            Interlocked.Increment(ref _total);

            if (rules.ShouldBlockHost(host))
            {
                Interlocked.Increment(ref _blocked);

                if (!TryHardDrop(e))
                {
                    e.Ok("Blocked");
                    e.HttpClient.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                }

                PublishStats();
                Log("Blocked", string.Format("{0} {1} (dropped){2}", isConnect ? "CONNECT" : "HTTP", host, BuildRequestInfo(e)));
                return;
            }

            Interlocked.Increment(ref _allowed);
            PublishStats();
            Log("Allowed", string.Format("{0} {1}{2}", isConnect ? "CONNECT" : "HTTP", host, BuildRequestInfo(e)));
        }

        private static string NormalizeHost(string host)
        {
            if (string.IsNullOrWhiteSpace(host)) return string.Empty;

            host = host.Trim();
            var colonIndex = host.IndexOf(':');
            if (colonIndex > 0)
                host = host.Substring(0, colonIndex);

            return host.Trim().TrimEnd('.');
        }

        private static bool TryHardDrop(SessionEventArgs e)
        {
            try
            {
                var client = e.HttpClient;
                if (client == null) return false;

                var type = client.GetType();
                var method = type.GetMethod("Close")
                             ?? type.GetMethod("CloseConnection")
                             ?? type.GetMethod("CloseClientConnection")
                             ?? type.GetMethod("Disconnect");

                if (method != null)
                {
                    method.Invoke(client, null);
                    return true;
                }
            }
            catch { }

            return false;
        }

        private static string BuildRequestInfo(SessionEventArgs e)
        {
            try
            {
                var req = e.HttpClient != null ? e.HttpClient.Request : null;
                if (req == null) return string.Empty;

                var url = req.Url;
                if (string.IsNullOrWhiteSpace(url)) return string.Empty;

                return string.Format(" | {0}", url);
            }
            catch
            {
                return string.Empty;
            }
        }

        private void PublishStats()
        {
            var handler = OnStatsChanged;
            if (handler != null)
            {
                handler(new ProxyStats
                {
                    Total = Interlocked.Read(ref _total),
                    Blocked = Interlocked.Read(ref _blocked),
                    Allowed = Interlocked.Read(ref _allowed)
                });
            }
        }

        private void Log(string level, string message)
        {
            var handler = OnLog;
            if (handler != null)
                handler(new LogEvent { Level = level, Message = message });
        }
    }
}