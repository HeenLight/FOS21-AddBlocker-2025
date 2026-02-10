using System;
using System.IO;
using System.Drawing;
using System.Media;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SchoolAdBlocker.Core.Blocklists;
using SchoolAdBlocker.Core.Filtering;
using SchoolAdBlocker.Core.Models;
using SchoolAdBlocker.Core.Proxy;
using SchoolAdBlocker.Core.Storage;
using SchoolAdBlocker.Core.Utils;

namespace AddBlocker
{
    public partial class Form1 : Form
    {
        private static SystemProxyManager _lastSystemProxy;
        private readonly string _baseDir;
        private readonly SettingsStore _settingsStore;
        private readonly BlocklistCacheStore _cache;
        private readonly HostsParser _parser;
        private readonly RemoteListFetcher _fetcher;
        private readonly BlocklistUpdater _updater;
        private readonly RuleEngine _rules;
        private readonly ProxyHost _proxy;
        private readonly SystemProxyManager _systemProxy;
        private readonly HttpClient _http;
        private readonly ThemeWaveOverlay _themeOverlay;
        private readonly string _logFilePath;
        private readonly SoundPlayer _soundPlayer;

        private AppSettings _settings;
        private CancellationTokenSource _cts;
        private bool _isRunning;
        private bool _darkTheme;

        public Form1()
        {
            InitializeComponent();
            _baseDir = AppContext.BaseDirectory;

            Guard.EnsureDirectory(System.IO.Path.Combine(_baseDir, "cache"));
            Guard.EnsureDirectory(System.IO.Path.Combine(_baseDir, "logs"));

            _settingsStore = new SettingsStore();
            _settings = _settingsStore.LoadOrCreateDefault(_baseDir);

            _cache = new BlocklistCacheStore(_baseDir);
            _parser = new HostsParser();
            _http = new HttpClient();
            _fetcher = new RemoteListFetcher(_http);
            _updater = new BlocklistUpdater(_cache, _parser, _fetcher);

            _rules = new RuleEngine();
            _rules.UpdateDomains(_updater.LoadFromCacheOrEmpty());

            _proxy = new ProxyHost();
            _proxy.OnLog += ProxyOnLog;
            _proxy.OnStatsChanged += ProxyOnStatsChanged;

            _systemProxy = new SystemProxyManager();
            _lastSystemProxy = _systemProxy;

            _logFilePath = Path.Combine(_baseDir, "logs", "app.log");

            _soundPlayer = LoadSoundPlayer();

            _themeOverlay = new ThemeWaveOverlay();
            _themeOverlay.Dock = DockStyle.Fill;
            _themeOverlay.Visible = false;
            _themeOverlay.Enabled = false;
            Controls.Add(_themeOverlay);
            _themeOverlay.BringToFront();

            txtPort.Text = _settings.Port.ToString();
            UpdateUiState();
            ApplyTheme();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            StopProxy();
            RestoreProxySafe();
            base.OnFormClosing(e);
        }

        public static void TryRestoreProxyOnExit()
        {
            try
            {
                if (_lastSystemProxy != null)
                    _lastSystemProxy.Restore();
            }
            catch { }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            int port;
            if (!int.TryParse(txtPort.Text, out port) || port <= 0 || port > 65535)
            {
                MessageBox.Show("Некорректный порт", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            StartProxy(port);
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            StopProxy();
        }

        private async void btnUpdate_Click(object sender, EventArgs e)
        {
            await UpdateRulesAsync();
        }

        private void btnTheme_Click(object sender, EventArgs e)
        {
            StartThemeTransition();
        }

        private void btnAddDomain_Click(object sender, EventArgs e)
        {
            var domain = NormalizeDomainInput(txtCustomDomain.Text);
            if (string.IsNullOrWhiteSpace(domain))
            {
                MessageBox.Show("Введите домен", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                var path = _cache.CustomPath;
                File.AppendAllText(path, domain + Environment.NewLine);
                _rules.UpdateDomains(_updater.LoadFromCacheOrEmpty());
                AddLog("Info", string.Format("Custom blocked: {0}", domain));
                txtCustomDomain.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnEaster_Click(object sender, EventArgs e)
        {
            PlaySound();
        }

        private void StartProxy(int port)
        {
            if (_isRunning) return;

            _cts = new CancellationTokenSource();

            try
            {
                _systemProxy.SaveCurrent();
                _systemProxy.ApplyLocalProxy(string.Format("127.0.0.1:{0}", port));
                AddLog("Info", _systemProxy.GetCurrentProxyInfo());

                _proxy.Start(port, _rules, _cts.Token);
                _isRunning = true;

                _settings.Port = port;
                _settingsStore.Save(_baseDir, _settings);

                UpdateUiState();
            }
            catch (Exception ex)
            {
                try { _systemProxy.Restore(); } catch { }
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void StopProxy()
        {
            if (!_isRunning) return;

            try
            {
                if (_cts != null) _cts.Cancel();
                _proxy.Stop();
            }
            finally
            {
                RestoreProxySafe();
                _isRunning = false;
                UpdateUiState();
            }
        }

        private void RestoreProxySafe()
        {
            try { _systemProxy.Restore(); } catch { }
        }

        private async Task UpdateRulesAsync()
        {
            btnUpdate.Enabled = false;
            try
            {
                var domains = await _updater.UpdateNowAsync(_settings.Sources, CancellationToken.None).ConfigureAwait(false);
                _rules.UpdateDomains(domains);
                SafeUi(() => AddLog("Info", "Blocklist updated (custom.txt applied)"));
            }
            catch (Exception ex)
            {
                SafeUi(() => AddLog("Error", ex.Message));
            }
            finally
            {
                SafeUi(() => btnUpdate.Enabled = true);
            }
        }

        private void ProxyOnLog(LogEvent e)
        {
            SafeUi(() => AddLog(e.Level, e.Message));
            if (string.Equals(e.Level, "Blocked", StringComparison.OrdinalIgnoreCase))
                PlaySound();
        }

        private void ProxyOnStatsChanged(ProxyStats s)
        {
            SafeUi(() =>
            {
                lblTotal.Text = string.Format("Total: {0}", s.Total);
                lblBlocked.Text = string.Format("Blocked: {0}", s.Blocked);
                lblAllowed.Text = string.Format("Allowed: {0}", s.Allowed);
            });
        }

        private void AddLog(string level, string message)
        {
            var line = string.Format("[{0}] {1}: {2}", DateTime.Now.ToString("HH:mm:ss"), level, message);
            listLog.Items.Add(line);
            if (listLog.Items.Count > 1000)
                listLog.Items.RemoveAt(0);
            listLog.TopIndex = listLog.Items.Count - 1;

            try
            {
                File.AppendAllText(_logFilePath, line + Environment.NewLine);
            }
            catch { }
        }

        private void UpdateUiState()
        {
            lblStatus.Text = _isRunning ? "Status: Running" : "Status: Stopped";
            btnStart.Enabled = !_isRunning;
            btnStop.Enabled = _isRunning;
            btnAddDomain.Enabled = !_isRunning;
        }

        private void ApplyTheme()
        {
            Color back;
            Color fore;
            Color panelBack;
            GetThemeColors(_darkTheme, out back, out fore, out panelBack);

            BackColor = back;
            ForeColor = fore;
            Font = new System.Drawing.Font("Segoe UI", 9F);

            txtPort.BackColor = panelBack;
            txtPort.ForeColor = fore;
            listLog.BackColor = panelBack;
            listLog.ForeColor = fore;
            txtCustomDomain.BackColor = panelBack;
            txtCustomDomain.ForeColor = fore;

            var buttons = new[] { btnStart, btnStop, btnUpdate, btnTheme, btnAddDomain };
            foreach (var b in buttons)
            {
                b.FlatStyle = FlatStyle.Flat;
                b.FlatAppearance.BorderSize = 0;
                b.BackColor = panelBack;
                b.ForeColor = fore;
            }

            var labels = new[] { lblPort, lblStatus, lblTotal, lblBlocked, lblAllowed, lblCustom };
            foreach (var l in labels)
            {
                l.ForeColor = fore;
            }
        }

        private static void GetThemeColors(bool dark, out Color back, out Color fore, out Color panelBack)
        {
            back = dark ? Color.FromArgb(32, 32, 32) : Color.White;
            fore = dark ? Color.Gainsboro : Color.Black;
            panelBack = dark ? Color.FromArgb(45, 45, 45) : Color.WhiteSmoke;
        }

        private static string NormalizeDomainInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            input = input.Trim();

            if (input.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                input.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                Uri uri;
                if (Uri.TryCreate(input, UriKind.Absolute, out uri))
                    return uri.Host.Trim().TrimEnd('.').ToLowerInvariant();
            }

            var slashIndex = input.IndexOf('/');
            if (slashIndex >= 0)
                input = input.Substring(0, slashIndex);

            var colonIndex = input.IndexOf(':');
            if (colonIndex >= 0)
                input = input.Substring(0, colonIndex);

            return input.Trim().TrimEnd('.').ToLowerInvariant();
        }

        private void StartThemeTransition()
        {
            var targetDark = !_darkTheme;

            Color back;
            Color fore;
            Color panelBack;
            GetThemeColors(targetDark, out back, out fore, out panelBack);

            var center = new Point(btnTheme.Left + btnTheme.Width / 2, btnTheme.Top + btnTheme.Height / 2);
            var maxRadius = CalculateMaxRadius(center, ClientSize);

            _themeOverlay.Start(this, center, BackColor, back, maxRadius, 300, () =>
            {
                _darkTheme = targetDark;
                ApplyTheme();
            });
        }

        private static int CalculateMaxRadius(Point center, Size size)
        {
            var dx = Math.Max(center.X, size.Width - center.X);
            var dy = Math.Max(center.Y, size.Height - center.Y);
            return (int)Math.Ceiling(Math.Sqrt(dx * dx + dy * dy));
        }

        private SoundPlayer LoadSoundPlayer()
        {
            try
            {
                var asm = Assembly.GetExecutingAssembly();
                var name = Array.Find(asm.GetManifestResourceNames(),
                    n => n.EndsWith("sound1.wav", StringComparison.OrdinalIgnoreCase));
                if (string.IsNullOrEmpty(name)) return null;

                using (var stream = asm.GetManifestResourceStream(name))
                {
                    if (stream == null) return null;
                    var ms = new MemoryStream();
                    stream.CopyTo(ms);
                    ms.Position = 0;
                    var player = new SoundPlayer(ms);
                    player.Load();
                    return player;
                }
            }
            catch
            {
                return null;
            }
        }

        private void PlaySound()
        {
            try
            {
                if (_soundPlayer != null)
                    _soundPlayer.Play();
            }
            catch
            {
            }
        }


        private void SafeUi(Action action)
        {
            if (InvokeRequired)
                BeginInvoke(action);
            else
                action();
        }

        private sealed class ThemeWaveOverlay : Control
        {
            private readonly System.Windows.Forms.Timer _timer;
            private Bitmap _background;
            private Point _center;
            private int _radius;
            private int _maxRadius;
            private int _durationMs;
            private int _startTick;
            private Color _startColor;
            private Color _endColor;
            private Action _onComplete;

            public ThemeWaveOverlay()
            {
                SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
                _timer = new System.Windows.Forms.Timer();
                _timer.Interval = 15;
                _timer.Tick += OnTick;
            }

            public void Start(Control owner, Point center, Color startColor, Color endColor, int maxRadius, int durationMs, Action onComplete)
            {
                if (_background != null)
                {
                    _background.Dispose();
                    _background = null;
                }

                _background = new Bitmap(owner.ClientSize.Width, owner.ClientSize.Height);
                using (var g = Graphics.FromImage(_background))
                {
                    var screenPoint = owner.PointToScreen(Point.Empty);
                    g.CopyFromScreen(screenPoint, Point.Empty, owner.ClientSize);
                }

                _center = center;
                _startColor = startColor;
                _endColor = endColor;
                _maxRadius = maxRadius;
                _durationMs = durationMs;
                _startTick = Environment.TickCount;
                _radius = 0;
                _onComplete = onComplete;
                Visible = true;
                BringToFront();
                _timer.Start();
                Invalidate();
            }

            protected override void OnPaintBackground(PaintEventArgs e)
            {
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                if (_background != null)
                    e.Graphics.DrawImageUnscaled(_background, 0, 0);
                else
                    e.Graphics.Clear(_startColor);
                using (var brush = new SolidBrush(_endColor))
                {
                    var d = _radius * 2;
                    e.Graphics.FillEllipse(brush, _center.X - _radius, _center.Y - _radius, d, d);
                }
            }

            private void OnTick(object sender, EventArgs e)
            {
                var elapsed = Environment.TickCount - _startTick;
                var progress = Math.Min(1.0, elapsed / (double)_durationMs);
                _radius = (int)(_maxRadius * progress);
                Invalidate();

                if (progress >= 1.0)
                {
                    _timer.Stop();
                    Visible = false;
                    if (_background != null)
                    {
                        _background.Dispose();
                        _background = null;
                    }
                    if (_onComplete != null)
                        _onComplete();
                }
            }
        }
    }
}
