using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Media;
using Color = System.Drawing.Color;
using Ppt = Microsoft.Office.Interop.PowerPoint;

namespace _801CountdownApp1
{
    public partial class Form1 : Form
    {
        private int remainingSeconds = 800;
        private int totalSeconds = 800;
        private bool isCountingDown = false;
        private Point mouseOffset;
        private bool isMouseDown = false;
        private bool timerVisible = true;
        private WebControlServer webServer;
        private bool flash3Enabled;
        private MediaPlayer endSoundPlayer;
        private System.Windows.Forms.Timer pptTimer;
        private bool pptAutoEnabled = false;
        private bool showSlideInfoEnabled = false;
        private bool pptWasFullscreen = false;
        private string lastSlideInfo;
        private volatile int lastSlideIndex;
        private int? cachedTotalSlides;
        private bool slideHeightAdjusted;
        private float originalFontSize = 16f;
        private bool allowNegative;
        private bool alarmTriggered;
        private bool pureSlideMode;
        private bool preAlertEnabled;
        private int preAlertSeconds = 60;
        private bool preAlertTriggered;
        private int webPort = 9051;
        private static readonly object previewExportLock = new object();
        private static int previewExportActive;
        private ToolStripMenuItem screenMenuItem;
        private ToolStripMenuItem opacityMenuItem;
        private ToolStripMenuItem fontSizeMenuItem;
        private CountdownStyleKind styleKind = CountdownStyleKind.Default;
        private double layerOpacity = 1.0;
        private int paddingValue = 16;
        private bool drawingLayered;
        private bool mouseInsideWindow;
        private bool tooltipEnabled = true;
        private readonly ToolTip slideToolTip = new ToolTip
        {
            InitialDelay = 300,
            AutoPopDelay = 10000,
            ReshowDelay = 100,
            ShowAlways = true
        };

        // Win32 API for PPT/WPS fullscreen detection
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int GetClassName(IntPtr hWnd, System.Text.StringBuilder className, int maxCount);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder text, int maxCount);
        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);
        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);
        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);
        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);
        private const uint KEYEVENTF_KEYUP = 0x0002;
        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOACTIVATE = 0x0010;
        private const uint SWP_SHOWWINDOW = 0x0040;
        [StructLayout(LayoutKind.Sequential)]
        private struct RECT { public int Left; public int Top; public int Right; public int Bottom; }

        // 分层窗口：内容绘制到 32 位 ARGB 位图后整块提交，边缘抗锯齿且背景可透明
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetDC(IntPtr hWnd);
        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleDC(IntPtr hdc);
        [DllImport("gdi32.dll")]
        private static extern bool DeleteDC(IntPtr hdc);
        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);
        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst, IntPtr pptDst,
            ref Size psize, IntPtr hdcSrc, ref Point pprSrc, int crKey, ref BLENDFUNCTION pblend, int dwFlags);
        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_LAYERED = 0x80000;
        private const int ULW_ALPHA = 2;
        private const byte AC_SRC_OVER = 0;
        private const byte AC_SRC_ALPHA = 1;
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct BLENDFUNCTION
        {
            public byte BlendOp;
            public byte BlendFlags;
            public byte SourceConstantAlpha;
            public byte AlphaFormat;
        }
        public Form1()
        {
            webPort = LoadWebPort();
            RegisterUrlAcl(webPort);
            AddFirewallRule(webPort);
            InitializeComponent();
            this.DoubleBuffered = true;
            this.TopMost = true;
            // 界面改由分层窗口整块自绘，Label 只用于保存文字、字体和测量
            centerLabel.Visible = false;
            SetStyle(ControlStyles.StandardClick | ControlStyles.StandardDoubleClick, true);
            UpdateDisplay();
            this.MouseDoubleClick += CenterLabel_MouseDoubleClick;
            this.MouseEnter += (s, e) => { mouseInsideWindow = true; UpdateSlideToolTip(); };
            this.MouseLeave += (s, e) => { mouseInsideWindow = false; };
            this.MouseDown += Form_MouseDown;
            this.MouseMove += Form_MouseMove;
            this.MouseUp += Form_MouseUp;
            this.MouseUp += Form1_MouseUp;
            timer1.Tick += Timer1_Tick;
            // 右键菜单事件
            resetMenuItem.Click += ResetMenuItem_Click;
            topCenterMenuItem.Click += TopCenterMenuItem_Click;
            topRightMenuItem.Click += TopRightMenuItem_Click;
            closeMenuItem.Click += CloseMenuItem_Click;
            settingsMenuItem.Click += SettingsMenuItem_Click;
            // 启动Web控制服务器
            webServer = new WebControlServer(this, webPort);
            webServer.Start();
            // PPT全屏检测定时器
            pptTimer = new System.Windows.Forms.Timer(components);
            pptTimer.Interval = 500;
            pptTimer.Tick += PptTimer_Tick;
            this.FormClosing += Form1_FormClosing;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            int exStyle = GetWindowLong(Handle, GWL_EXSTYLE);
            SetWindowLong(Handle, GWL_EXSTYLE, exStyle | WS_EX_LAYERED);
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (Visible) RedrawLayered();
        }

        private static int LoadWebPort()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\\801CountdownApp1"))
                {
                    if (key?.GetValue("WebPort") is int port && port >= 1 && port <= 65535)
                        return port;
                }
            }
            catch { }
            return 9051;
        }

        private void RestartWebServer(int port)
        {
            webServer?.Stop();
            webServer = null;
            webPort = port;
            RegisterUrlAcl(port);
            AddFirewallRule(port);
            var server = new WebControlServer(this, port);
            if (!server.Start())
            {
                MessageBox.Show(
                    $"网页控制器端口 {port} 启动失败，可能已被占用或没有监听权限。\r\n错误信息：{server.LastError}",
                    "远控启动失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            webServer = server;
        }

        private void RegisterUrlAcl(int port)
        {
            try
            {
                string url = $"http://+:{port}/";
                string user = "Everyone";
                if (!IsUrlAclExists(url))
                {
                    var psi = new System.Diagnostics.ProcessStartInfo("netsh", $"http add urlacl url={url} user={user}")
                    {
                        Verb = "runas",
                        CreateNoWindow = true,
                        UseShellExecute = true
                    };
                    var proc = System.Diagnostics.Process.Start(psi);
                    proc.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"注册URL ACL失败，请用管理员身份运行或手动执行：\r\nnetsh http add urlacl url=http://+:{port}/ user=Everyone\r\n错误信息：" + ex.Message, "端口权限错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool IsUrlAclExists(string url)
        {
            try
            {
                var psi = new System.Diagnostics.ProcessStartInfo("netsh", "http show urlacl")
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                var proc = System.Diagnostics.Process.Start(psi);
                string output = proc.StandardOutput.ReadToEnd();
                proc.WaitForExit();
                return output.Contains(url);
            }
            catch { return false; }
        }

        private void AddFirewallRule(int port)
        {
            try
            {
                string ruleName = "801CountdownApp1_" + port;
                string arguments = $"advfirewall firewall show rule name={ruleName}";
                var psi = new System.Diagnostics.ProcessStartInfo("netsh", arguments)
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                var proc = System.Diagnostics.Process.Start(psi);
                string output = proc.StandardOutput.ReadToEnd();
                proc.WaitForExit();
                if (!output.Contains(ruleName))
                {
                    // 添加规则
                    string addArgs = $"advfirewall firewall add rule name={ruleName} dir=in action=allow protocol=TCP localport={port}";
                    var addPsi = new System.Diagnostics.ProcessStartInfo("netsh", addArgs)
                    {
                        Verb = "runas",
                        CreateNoWindow = true,
                        UseShellExecute = true
                    };
                    var addProc = System.Diagnostics.Process.Start(addPsi);
                    addProc.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"添加防火墙规则失败，请用管理员身份运行或手动执行：\r\nnetsh advfirewall firewall add rule name=801CountdownApp1_{port} dir=in action=allow protocol=TCP localport={port}\r\n错误信息：" + ex.Message, "防火墙规则错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CenterLabel_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (!isCountingDown)
                {
                    isCountingDown = true;
                    timer1.Start();
                }
                else
                {
                    isCountingDown = false;
                    timer1.Stop();
                }
            }
        }

        private async void Timer1_Tick(object sender, EventArgs e)
        {
            int before = remainingSeconds;
            if (remainingSeconds > 0 || allowNegative)
            {
                remainingSeconds--;
                UpdateDisplay();
                if (mouseInsideWindow) UpdateSlideToolTip();
            }

            // 结束前提醒：跨过阈值的那一秒触发一次，起步时已低于阈值则不补触发
            if (preAlertEnabled && !preAlertTriggered && remainingSeconds > 0
                && before > preAlertSeconds && remainingSeconds <= preAlertSeconds)
            {
                preAlertTriggered = true;
                // 结束前提醒是独立开关，勾选即响，不受「提示音」总开关影响
                PlayEndSound(force: true);
                if (flash3Enabled) await FlashWindowOnceAsync();
            }

            // 归零时触发提示音和闪烁（仅一次）
            if (remainingSeconds <= 0 && !alarmTriggered)
            {
                alarmTriggered = true;
                PlayEndSound();
                if (flash3Enabled)
                {
                    await FlashWindowAsync();
                }
            }

            // 非负数模式：归零后停止计时
            if (!allowNegative && remainingSeconds <= 0 && isCountingDown)
            {
                timer1.Stop();
                isCountingDown = false;
            }
        }

        private async Task FlashWindowOnceAsync()
        {
            Color origBack = this.BackColor;
            Color origFore = centerLabel.ForeColor;
            this.BackColor = Color.White;
            centerLabel.ForeColor = Color.Red;
            RedrawLayered();
            await Task.Delay(300);
            this.BackColor = origBack;
            centerLabel.ForeColor = origFore;
            RedrawLayered();
            await Task.Delay(300);
        }

        public async Task FlashWindowAsync()
        {
            Color origBack = this.BackColor;
            Color origFore = centerLabel.ForeColor;
            for (int i = 0; i < 3; i++)
            {
                this.BackColor = Color.White;
                centerLabel.ForeColor = Color.Red;
                RedrawLayered();
                await Task.Delay(300);
                this.BackColor = origBack;
                centerLabel.ForeColor = origFore;
                RedrawLayered();
                await Task.Delay(300);
            }
        }

        private string FormatTime(int totalSeconds)
        {
            bool neg = totalSeconds < 0;
            if (neg) totalSeconds = -totalSeconds;
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;
            return (neg ? "-" : "") + string.Format("{0:D2}:{1:D2}", minutes, seconds);
        }

        private void Form_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                mouseOffset = new Point(-e.X, -e.Y);
                isMouseDown = true;
            }
        }

        private void Form_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMouseDown)
            {
                Point mousePos = Control.MousePosition;
                mousePos.Offset(mouseOffset.X, mouseOffset.Y);
                this.Location = mousePos;
                SaveWindowLocation();
            }
        }

        private void Form_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isMouseDown = false;
            }
        }

        private void SaveWindowLocation()
        {
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\\801CountdownApp1");
            key.SetValue("Left", this.Left);
            key.SetValue("Top", this.Top);
            key.Close();
        }

        private void LoadWindowLocation()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\\801CountdownApp1");
            if (key != null)
            {
                object left = key.GetValue("Left");
                object top = key.GetValue("Top");
                if (left != null && top != null)
                {
                    this.Left = (int)left;
                    this.Top = (int)top;
                }
                key.Close();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadWindowLocation();
            ApplyConfigFromRegistry();
            LoadSwitchStates();
            // 透明度 / 字号快捷子菜单
            opacityMenuItem = BuildQuickMenuItem("透明度",
                new[] { "100%", "80%", "60%", "40%" },
                new[] { 250, 200, 150, 100 },
                SetOpacity);
            fontSizeMenuItem = BuildQuickMenuItem("字号",
                new[] { "16", "24", "32", "48" },
                new[] { 16, 24, 32, 48 },
                SetFontSize);
            contextMenuStrip1.Items.Insert(2, opacityMenuItem);
            contextMenuStrip1.Items.Insert(3, fontSizeMenuItem);
            // 多屏切换菜单项（每次打开右键菜单时检测屏幕数量）
            screenMenuItem = new ToolStripMenuItem("切换屏幕");
            screenMenuItem.Click += ScreenMenuItem_Click;
            contextMenuStrip1.Items.Insert(contextMenuStrip1.Items.Count - 2, screenMenuItem);
            contextMenuStrip1.Opening += (_, __) =>
            {
                UpdateScreenMenuItemVisibility();
                UpdateQuickMenuItemChecks();
            };
        }

        private static ToolStripMenuItem BuildQuickMenuItem(string title, string[] labels, int[] values, Action<int> apply)
        {
            var root = new ToolStripMenuItem(title);
            for (int i = 0; i < labels.Length; i++)
            {
                int value = values[i];
                var item = new ToolStripMenuItem(labels[i]) { Tag = value };
                item.Click += (s, args) => apply(value);
                root.DropDownItems.Add(item);
            }
            return root;
        }

        private void UpdateQuickMenuItemChecks()
        {
            if (opacityMenuItem == null || fontSizeMenuItem == null) return;
            int currentOpacity = GetOpacityValue();
            foreach (ToolStripItem item in opacityMenuItem.DropDownItems)
            {
                if (item is ToolStripMenuItem mi && mi.Tag is int v) mi.Checked = v == currentOpacity;
            }
            int currentFontSize = GetFontSize();
            foreach (ToolStripItem item in fontSizeMenuItem.DropDownItems)
            {
                if (item is ToolStripMenuItem mi && mi.Tag is int v) mi.Checked = v == currentFontSize;
            }
        }

        private void ScreenMenuItem_Click(object sender, EventArgs e)
        {
            CycleScreen();
        }

        public void CycleScreen()
        {
            var screens = Screen.AllScreens;
            if (screens.Length < 2) return;
            var current = Screen.FromControl(this);
            int idx = Array.IndexOf(screens, current);
            var next = screens[(idx + 1) % screens.Length];
            this.Location = new Point(next.WorkingArea.Left + 50, next.WorkingArea.Top + 50);
        }

        private void UpdateScreenMenuItemVisibility()
        {
            if (screenMenuItem != null)
                screenMenuItem.Visible = Screen.AllScreens.Length > 1;
        }

        private void ResetMenuItem_Click(object sender, EventArgs e)
        {
            timer1.Stop();
            isCountingDown = false;
            ResetTimeFromRegistry();
        }

        public void ResetTimeFromRegistry()
        {
            alarmTriggered = false;
            preAlertTriggered = false;
            var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\\801CountdownApp1");
            if (key != null)
            {
                object totalTime = key.GetValue("TotalTime");
                object timeUnit = key.GetValue("TimeUnit");
                if (totalTime != null && timeUnit != null)
                {
                    int value = 0;
                    int.TryParse(totalTime.ToString(), out value);
                    int unit = (int)timeUnit;
                    if (unit == 0) // 分
                        remainingSeconds = value > int.MaxValue / 60 ? int.MaxValue : value * 60;
                    else // 秒
                        remainingSeconds = value;
                    totalSeconds = remainingSeconds;
                }
                key.Close();
            }
            UpdateDisplay();
            UpdateSlideToolTip();
            timer1.Stop();
            isCountingDown = false;
        }

        private void TopCenterMenuItem_Click(object sender, EventArgs e)
        {
            MoveToTopCenter();
        }

        private void TopRightMenuItem_Click(object sender, EventArgs e)
        {
            MoveToRightTop();
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                contextMenuStrip1.Show(this, e.Location);
            }
        }

        private void CloseMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            pptTimer?.Stop();
            webServer?.Stop();
            slideToolTip.Dispose();
        }

        private void SettingsMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Form f in Application.OpenForms)
            {
                if (f is Form2) { f.BringToFront(); return; }
            }
            var form2 = new Form2();
            form2.CurrentLabelFont = centerLabel.Font;
            form2.FontSelected += (s, font) =>
            {
                centerLabel.Font = font;
                originalFontSize = font.Size;
                AutoSizeForm();
                RedrawLayered();
            };
            form2.OpacityChanged += (s, value) =>
            {
                layerOpacity = value / 250.0;
                RedrawLayered();
            };
            form2.FontSizeChanged += (size) =>
            {
                originalFontSize = size;
                centerLabel.Font = new Font(centerLabel.Font.FontFamily, size, centerLabel.Font.Style);
                AutoSizeForm();
                RedrawLayered();
            };
            form2.PaddingValueChanged += SetPadding;
            form2.ColorChanged += (fore, back) =>
            {
                centerLabel.ForeColor = fore;
                this.BackColor = back;
                RedrawLayered();
            };
            form2.CountdownStyleChanged += style => ApplyStyle(style);
            form2.FormClosed += (s, args) =>
            {
                ApplyConfigFromRegistry(false);
            };
            form2.Show();
        }

        private void ApplyConfigFromRegistry(bool applyTime = true)
        {
            var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\\801CountdownApp1");
            if (key != null)
            {
                object totalTime = key.GetValue("TotalTime");
                object timeUnit = key.GetValue("TimeUnit");
                object opacity = key.GetValue("Opacity");
                object foreColor = key.GetValue("ForeColor");
                object backColor = key.GetValue("BackColor");
                object fontFamily = key.GetValue("FontFamily");
                object fontSize = key.GetValue("FontSize");
                object fontStyle = key.GetValue("FontStyle");
                object fontSizeSliderVal = key.GetValue("FontSizeSlider");
                object paddingVal = key.GetValue("Padding");
                object styleVal = key.GetValue("Style");
                if (applyTime && totalTime != null && timeUnit != null)
                {
                    int value = 0;
                    int.TryParse(totalTime.ToString(), out value);
                    int unit = (int)timeUnit;
                    if (unit == 0) // 分
                        remainingSeconds = value > int.MaxValue / 60 ? int.MaxValue : value * 60;
                    else // 秒
                        remainingSeconds = value;
                    totalSeconds = remainingSeconds;
                    alarmTriggered = false;
                    preAlertTriggered = false;
                    UpdateDisplay();
                    timer1.Stop();
                    isCountingDown = false;
                }
                if (opacity != null)
                {
                    layerOpacity = (int)opacity / 250.0;
                }
                if (foreColor != null)
                {
                    centerLabel.ForeColor = Color.FromArgb((int)foreColor);
                }
                else
                {
                    centerLabel.ForeColor = Color.White;
                }
                if (backColor != null)
                {
                    this.BackColor = Color.FromArgb((int)backColor);
                }
                else
                {
                    this.BackColor = Color.Red;
                }
                if (fontFamily != null && fontSize != null && fontStyle != null)
                {
                    try
                    {
                        float fs = float.Parse(fontSize.ToString());
                        centerLabel.Font = new Font(fontFamily.ToString(), fs, (FontStyle)(int)fontStyle);
                        originalFontSize = fs;
                    }
                    catch { }
                }
                if (fontSizeSliderVal != null)
                {
                    int fs = (int)fontSizeSliderVal;
                    centerLabel.Font = new Font(centerLabel.Font.FontFamily, fs, centerLabel.Font.Style);
                    originalFontSize = fs;
                }
                paddingValue = paddingVal != null
                    ? Math.Max(0, Math.Min(20, (int)paddingVal))
                    : 16;
                // PPT/WPS 全屏自动计时
                object pptAuto = key.GetValue("PPTAuto");
                pptAutoEnabled = pptAuto != null && (int)pptAuto == 1;
                object showSlide = key.GetValue("ShowSlideInfo");
                showSlideInfoEnabled = showSlide != null && (int)showSlide == 1;
                // 读取纯PPT页数模式（需在 pptTimer 条件前）
                object pureSlide = key.GetValue("PureSlideMode");
                pureSlideMode = pureSlide != null && (int)pureSlide == 1;
                if (pptAutoEnabled || showSlideInfoEnabled || pureSlideMode) pptTimer.Start(); else pptTimer.Stop();
                // 读取负数超时设置
                object allowNeg = key.GetValue("AllowNegative");
                allowNegative = allowNeg != null && (int)allowNeg == 1;
                // 结束前提醒
                object preAlert = key.GetValue("PreAlert");
                preAlertEnabled = preAlert != null && (int)preAlert == 1;
                object preAlertSec = key.GetValue("PreAlertSeconds");
                preAlertSeconds = preAlertSec != null ? Math.Max(1, Math.Min(3600, (int)preAlertSec)) : 60;
                // 鼠标悬浮提示
                object showToolTip = key.GetValue("ShowToolTip");
                tooltipEnabled = showToolTip == null || (int)showToolTip == 1;
                // 远控端口（与当前不同则重建监听）
                object webPortVal = key.GetValue("WebPort");
                int wantedPort = webPortVal != null ? (int)webPortVal : 9051;
                if (wantedPort < 1 || wantedPort > 65535) wantedPort = 9051;
                key.Close();
                if (wantedPort != webPort) RestartWebServer(wantedPort);
                int styleValue = styleVal != null ? (int)styleVal : 0;
                if (styleValue < 0 || styleValue > (int)CountdownStyleKind.Transparent) styleValue = 0;
                ApplyStyle((CountdownStyleKind)styleValue);
                UpdateDisplay();
                UpdateSlideToolTip();
            }
            else
            {
                centerLabel.ForeColor = Color.White;
                this.BackColor = Color.Red;
                paddingValue = 16;
                ApplyStyle(CountdownStyleKind.Default);
                UpdateSlideToolTip();
            }
        }

        private void PlayEndSound(bool force = false)
        {
            if (!force)
            {
                var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\\801CountdownApp1");
                bool soundEnabled = key != null && key.GetValue("Sound") is int s && s == 1;
                key?.Close();
                if (!soundEnabled) return;
            }

            try
            {
                string fileName = "微波炉.mp3";
                var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\\801CountdownApp1");
                string savedName = key?.GetValue("SoundFile") as string;
                key?.Close();
                if (!string.IsNullOrEmpty(savedName)
                    && savedName.IndexOfAny(new[] { '\\', '/', ':' }) < 0
                    && savedName.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
                {
                    fileName = savedName;
                }
                string path = System.IO.Path.Combine(Application.StartupPath, "src", fileName);
                if (!System.IO.File.Exists(path))
                    path = System.IO.Path.Combine(Application.StartupPath, "src", "微波炉.mp3");
                if (System.IO.File.Exists(path))
                {
                    if (endSoundPlayer != null)
                    {
                        endSoundPlayer.Stop();
                        endSoundPlayer.Close();
                    }
                    endSoundPlayer = new MediaPlayer();
                    endSoundPlayer.MediaEnded += (s_, e_) => endSoundPlayer.Close();
                    endSoundPlayer.Open(new Uri(path, UriKind.Absolute));
                    endSoundPlayer.Play();
                }
            }
            catch { }
        }

        // PPT/WPS 全屏自动检测 + 页数显示
        private void PptTimer_Tick(object sender, EventArgs e)
        {
            if (this.IsDisposed) return;
            if (!pptAutoEnabled && !showSlideInfoEnabled && !pureSlideMode) return;

            FullscreenKind kind = DetectFullscreenKind();
            bool isPptFullscreen = kind != FullscreenKind.None;

            if (isPptFullscreen)
            {
                if (pptAutoEnabled && !isCountingDown && remainingSeconds > 0)
                {
                    isCountingDown = true;
                    timer1.Start();
                }
                // 非全屏 → 全屏切换时清除页数缓存
                if (!pptWasFullscreen)
                {
                    cachedTotalSlides = null;
                    // 仅放映类（可读页数）显示占位，PDF/视频等取不到页数
                    if (showSlideInfoEnabled && kind == FullscreenKind.SlideShow && string.IsNullOrEmpty(lastSlideInfo))
                    {
                        lastSlideInfo = "0/...";
                        UpdateSlideToolTip();
                        UpdateDisplay();
                    }
                }
                if (kind == FullscreenKind.SlideShow)
                {
                    string pageInfo = null;
                    // 预览导出期间不再额外调用 COM，避免和导出抢放映软件的界面线程
                    bool lockTaken = false;
                    try
                    {
                        lockTaken = System.Threading.Monitor.TryEnter(previewExportLock);
                        if (lockTaken) pageInfo = GetSlidePageInfo();
                    }
                    finally
                    {
                        if (lockTaken) System.Threading.Monitor.Exit(previewExportLock);
                    }
                    if (pageInfo != null)
                    {
                        int slash = pageInfo.IndexOf('/');
                        int parsed;
                        if (slash > 0 && int.TryParse(pageInfo.Substring(0, slash), out parsed)) lastSlideIndex = parsed;
                    }
                    if (pageInfo != null && pageInfo != lastSlideInfo)
                    {
                        lastSlideInfo = pageInfo;
                        UpdateSlideToolTip();
                        UpdateDisplay();
                    }
                }
                else if (lastSlideInfo != null)
                {
                    // 非放映类全屏：清掉上一次残留的页数
                    ClearSlideInfo();
                    UpdateDisplay();
                }
                pptWasFullscreen = true;
                pptTimer.Interval = 500; // 放映中：高频更新
            }
            else
            {
                if (pptWasFullscreen)
                {
                    pptWasFullscreen = false;
                    cachedTotalSlides = null;
                    if (pptAutoEnabled) StopAndResetTimer();
                }
                if (lastSlideInfo != null)
                {
                    ClearSlideInfo();
                    UpdateDisplay();
                }
                pptTimer.Interval = 2000; // 空闲中：低频轮询
            }
        }

        // 全屏检测结果：None=无全屏；Playing=全屏播放（触发自动计时，但读不到页数）；SlideShow=放映中且可通过 COM 读页数
        private enum FullscreenKind { None, Playing, SlideShow }

        // 可通过 PowerPoint/WPS COM 读取页数的放映进程
        private static readonly string[] SlideReadableProcs = { "powerpnt", "wpp" };
        // 其余全屏播放类（不读页数，仅触发自动计时）
        private static readonly string[] OtherPlaybackProcs = { "soffice", "focusky" };
        private static readonly string[] PdfViewerProcs = { "msedge", "chrome", "acroRd32", "foxitreader", "sumatrapdf", "nitropdf", "pdfxedit" };
        private static readonly string[] MediaPlayerProcs = { "vlc", "potplayermini", "mpc-hc", "mpc-be" };

        // 枚举所有窗口，检测各类全屏展示
        private FullscreenKind DetectFullscreenKind()
        {
            // 1. 快速路径：检查前台窗口
            IntPtr fgHwnd = GetForegroundWindow();
            if (fgHwnd != IntPtr.Zero)
            {
                GetWindowThreadProcessId(fgHwnd, out uint fgPid);
                string fgProcName = null;
                try { using (var p = System.Diagnostics.Process.GetProcessById((int)fgPid)) fgProcName = p.ProcessName; } catch { }
                if (fgProcName != null && IsWindowVisible(fgHwnd) && IsWindowFullscreen(fgHwnd))
                {
                    if (MatchesAny(fgProcName, SlideReadableProcs)) return FullscreenKind.SlideShow;
                    if (MatchesAny(fgProcName, OtherPlaybackProcs) || MatchesAny(fgProcName, MediaPlayerProcs)) return FullscreenKind.Playing;
                    // PDF 查看器需标题确认，浏览器全屏看网页时继续走后台枚举，避免漏判其它屏幕上的放映
                    if (MatchesAny(fgProcName, PdfViewerProcs) && IsWindowTitlePdf(fgHwnd)) return FullscreenKind.Playing;
                }
            }

            // 2. 后台路径：收集所有目标进程 PID
            var slidePids = new HashSet<uint>();
            var otherPids = new HashSet<uint>();
            var pdfPids = new HashSet<uint>();
            foreach (var name in SlideReadableProcs) CollectPids(name, slidePids);
            foreach (var name in OtherPlaybackProcs) CollectPids(name, otherPids);
            foreach (var name in MediaPlayerProcs) CollectPids(name, otherPids);
            foreach (var name in PdfViewerProcs) CollectPids(name, pdfPids);
            if (slidePids.Count == 0 && otherPids.Count == 0 && pdfPids.Count == 0) return FullscreenKind.None;

            // 3. 枚举窗口
            FullscreenKind found = FullscreenKind.None;
            EnumWindows((hWnd, lParam) =>
            {
                GetWindowThreadProcessId(hWnd, out uint pid);
                GetWindowRect(hWnd, out RECT rect);
                var screen = Screen.FromHandle(hWnd);
                bool fullscreen = rect.Left <= screen.Bounds.Left + 5
                    && rect.Top <= screen.Bounds.Top + 5
                    && rect.Right >= screen.Bounds.Right - 5
                    && rect.Bottom >= screen.Bounds.Bottom - 5;
                if (!fullscreen || !IsWindowVisible(hWnd)) return true;

                // 找到可读页数的放映优先，继续枚举以免被其它全屏窗口（PDF/视频）抢先
                if (slidePids.Contains(pid)) { found = FullscreenKind.SlideShow; return false; }
                if (found == FullscreenKind.None)
                {
                    if (otherPids.Contains(pid)) found = FullscreenKind.Playing;
                    else if (pdfPids.Contains(pid) && IsWindowTitlePdf(hWnd)) found = FullscreenKind.Playing;
                }
                return true;
            }, IntPtr.Zero);

            return found;
        }

        private static bool IsWindowFullscreen(IntPtr hWnd)
        {
            GetWindowRect(hWnd, out RECT rect);
            var screen = Screen.FromHandle(hWnd);
            return rect.Left <= screen.Bounds.Left + 5
                && rect.Top <= screen.Bounds.Top + 5
                && rect.Right >= screen.Bounds.Right - 5
                && rect.Bottom >= screen.Bounds.Bottom - 5;
        }

        private static bool IsWindowTitlePdf(IntPtr hWnd)
        {
            var sb = new System.Text.StringBuilder(512);
            return GetWindowText(hWnd, sb, sb.Capacity) > 0
                && sb.ToString().IndexOf(".pdf", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool MatchesAny(string procName, string[] names)
        {
            foreach (var n in names)
                if (procName.Equals(n, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        private static void CollectPids(string procName, HashSet<uint> set)
        {
            try
            {
                foreach (var p in System.Diagnostics.Process.GetProcessesByName(procName))
                    set.Add((uint)p.Id);
            }
            catch { }
        }

        private void StopAndResetTimer()
        {
            timer1.Stop();
            isCountingDown = false;
            ResetTimeFromRegistry();
        }

        // 通过 COM 获取幻灯片当前页/总页数（缓存总页数）
        private string GetSlidePageInfo()
        {
            string r = TryGetPptSlideInfoStrong();      // Microsoft PowerPoint
            if (r != null) return r;
            // WPS 演示：KWPP.Application（Ket.Application 是 WPS 表格，旧版本注册名为 WPP.Application）
            r = TryGetWpsSlideInfo("KWPP.Application");
            if (r != null) return r;
            r = TryGetWpsSlideInfo("WPP.Application");
            if (r != null) return r;
            return null;
        }

        private void UpdateSlideToolTip()
        {
            if (!tooltipEnabled)
            {
                slideToolTip.SetToolTip(this, null);
                return;
            }
            var sb = new System.Text.StringBuilder();
            sb.Append("状态：").Append(isCountingDown ? "运行中" : "已暂停");
            sb.Append("\n剩余：").Append(FormatTime(remainingSeconds))
              .Append(" / 总时长：").Append(FormatTime(totalSeconds));
            if (totalSeconds > 0)
            {
                int percent = (int)Math.Round(Math.Max(0, remainingSeconds) * 100.0 / totalSeconds);
                sb.Append("（").Append(percent).Append("%）");
            }
            if (!string.IsNullOrEmpty(lastSlideInfo))
            {
                sb.Append("\n\n页数：").Append(lastSlideInfo);
                string[] parts = lastSlideInfo.Split('/');
                if (parts.Length == 2
                    && int.TryParse(parts[0], out int current)
                    && int.TryParse(parts[1], out int total)
                    && total > 0)
                {
                    sb.Append("（").Append(Math.Round(current * 100.0 / total)).Append("%）");
                }
            }
            slideToolTip.SetToolTip(this, sb.ToString());
        }

        private void ClearSlideInfo()
        {
            lastSlideInfo = null;
            lastSlideIndex = 0;
            UpdateSlideToolTip();
        }

        // 强类型 Microsoft.Office.Interop.PowerPoint
        private string TryGetPptSlideInfoStrong()
        {
            Ppt.Application pptApp = null;
            Ppt.SlideShowWindows ssWindows = null;
            Ppt.SlideShowWindow ssWindow = null;
            Ppt.Slide slide = null;
            Ppt.Presentation presentation = null;
            try
            {
                pptApp = Marshal.GetActiveObject("PowerPoint.Application") as Ppt.Application;
                if (pptApp == null) return null;
                ssWindows = pptApp.SlideShowWindows;
                if (ssWindows == null || ssWindows.Count < 1) return null;

                ssWindow = ssWindows[1];
                slide = ssWindow.View.Slide;
                if (slide == null) return null;

                int current = slide.SlideIndex;
                if (cachedTotalSlides == null)
                {
                    presentation = ssWindow.Presentation;
                    cachedTotalSlides = presentation.Slides.Count;
                }
                return $"{current}/{cachedTotalSlides}";
            }
            catch
            {
                return null;
            }
            finally
            {
                if (presentation != null) Marshal.ReleaseComObject(presentation);
                if (slide != null) Marshal.ReleaseComObject(slide);
                if (ssWindow != null) Marshal.ReleaseComObject(ssWindow);
                if (ssWindows != null) Marshal.ReleaseComObject(ssWindows);
                if (pptApp != null) Marshal.ReleaseComObject(pptApp);
            }
        }

        // WPS 演示（动态调用，WPS 没有官方 PIA）
        private string TryGetWpsSlideInfo(string progId)
        {
            object app = null;
            object ssWindows = null;
            object ssWindow = null;
            object view = null;
            object slide = null;
            object presentation = null;
            try
            {
                // 只连接已运行的放映实例；此处不能再新建实例，否则每次取页数失败都会拉起一个后台 WPS 进程
                try { app = Marshal.GetActiveObject(progId); }
                catch { }
                if (app == null) return null;

                ssWindows = ((dynamic)app).SlideShowWindows;
                if (ssWindows == null || (int)((dynamic)ssWindows).Count < 1) return null;

                ssWindow = ((dynamic)ssWindows)[1];
                view = ((dynamic)ssWindow).View;
                slide = ((dynamic)view).Slide;
                int current = (int)((dynamic)slide).SlideIndex;
                if (cachedTotalSlides == null)
                {
                    presentation = ((dynamic)ssWindow).Presentation;
                    cachedTotalSlides = (int)((dynamic)presentation).Slides.Count;
                }
                return $"{current}/{cachedTotalSlides}";
            }
            catch { return null; }
            finally
            {
                ReleaseComObject(slide);
                ReleaseComObject(view);
                ReleaseComObject(presentation);
                ReleaseComObject(ssWindow);
                ReleaseComObject(ssWindows);
                ReleaseComObject(app);
            }
        }

        private static void ReleaseComObject(object o)
        {
            if (o == null) return;
            try { Marshal.ReleaseComObject(o); } catch { }
        }

        // 合并显示倒计时和幻灯片页数
        private void UpdateDisplay()
        {
            AdjustHeightForSlideInfo();
            if (pureSlideMode)
            {
                string info = !string.IsNullOrEmpty(lastSlideInfo) ? lastSlideInfo : "0/0";
                centerLabel.Text = "P" + info;
                AutoSizeForm();
                RedrawLayered();
                return;
            }
            string text = FormatTime(remainingSeconds);
            bool showSlide = showSlideInfoEnabled && !string.IsNullOrEmpty(lastSlideInfo);
            if (showSlide)
                text += "\nP" + lastSlideInfo;
            centerLabel.Text = text;
            AutoSizeForm();
            RedrawLayered();
        }

        // 显示页数时缩小字号，页数隐藏后恢复
        private void AdjustHeightForSlideInfo()
        {
            bool showSlide = showSlideInfoEnabled && !string.IsNullOrEmpty(lastSlideInfo);
            if (showSlide && !slideHeightAdjusted)
            {
                centerLabel.Font = new Font(centerLabel.Font.FontFamily, originalFontSize - 2, centerLabel.Font.Style);
                slideHeightAdjusted = true;
            }
            else if (!showSlide && slideHeightAdjusted)
            {
                centerLabel.Font = new Font(centerLabel.Font.FontFamily, originalFontSize, centerLabel.Font.Style);
                slideHeightAdjusted = false;
            }
        }

        private void AutoSizeForm()
        {
            Padding pad = EffectivePadding();
            int newW, newH;
            using (var ink = RenderTextInk())
            {
                newW = Math.Max(1, ink.Width) + pad.Horizontal;
                newH = Math.Max(1, ink.Height) + pad.Vertical;
            }

            if (this.ClientSize.Width != newW || this.ClientSize.Height != newH)
            {
                this.ClientSize = new Size(newW, newH);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            RedrawLayered();
        }

        // 上下左右统一的内边距，0 表示文字贴合边缘
        private Padding EffectivePadding()
        {
            return new Padding(Math.Max(0, Math.Min(20, paddingValue)));
        }

        public void SetPadding(int value)
        {
            paddingValue = Math.Max(0, Math.Min(20, value));
            centerLabel.Padding = EffectivePadding();
            AutoSizeForm();
            RedrawLayered();
        }

        public void ApplyStyle(CountdownStyleKind kind)
        {
            styleKind = kind;
            centerLabel.Padding = EffectivePadding();
            AutoSizeForm();
            RedrawLayered();
        }

        // 整个窗口画进 ARGB 位图再提交给系统，圆角边缘平滑、背景可完全透明
        private void RedrawLayered()
        {
            if (drawingLayered || IsDisposed || !IsHandleCreated) return;
            int width = ClientSize.Width;
            int height = ClientSize.Height;
            if (width <= 0 || height <= 0) return;

            drawingLayered = true;
            try
            {
                using (var bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                {
                    using (var g = Graphics.FromImage(bitmap))
                    {
                        // 透明样式下若整块 alpha=0，系统会把鼠标放行给下层窗口；
                        // 填一层肉眼不可见的 alpha=1，让整个窗口区域都能接收点击
                        g.Clear(styleKind == CountdownStyleKind.Transparent
                            ? Color.FromArgb(1, 0, 0, 0)
                            : Color.Transparent);
                        g.SmoothingMode = SmoothingMode.AntiAlias;
                        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                        var bounds = new Rectangle(0, 0, width, height);
                        CountdownStyles.DrawBackground(g, bounds, styleKind, centerLabel.ForeColor, this.BackColor);
                        using (var ink = RenderTextInk())
                        {
                            int textX = (width - ink.Width) / 2;
                            int textY = (height - ink.Height) / 2;
                            if (styleKind == CountdownStyleKind.Neon)
                                CountdownStyles.DrawTextGlow(g, ink, textX, textY);
                            g.InterpolationMode = InterpolationMode.NearestNeighbor;
                            g.DrawImage(ink, textX, textY, ink.Width, ink.Height);
                        }
                    }
                    PremultiplyAlpha(bitmap, layerOpacity);
                    UpdateLayer(bitmap);
                }
            }
            finally
            {
                drawingLayered = false;
            }
        }

        // 以与实际绘制一致的字体设置渲染文字，裁剪到真实墨迹范围。
        // 窗口尺寸与文字位置都以墨迹为准，四边可见留白才完全相等
        private Bitmap RenderTextInk()
        {
            string text = centerLabel.Text ?? string.Empty;
            Font font = centerLabel.Font;
            if (text.Length == 0)
                return new Bitmap(1, 1, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            int cellW = 0, cellH = 0;
            foreach (var line in text.Split('\n'))
            {
                var size = TextRenderer.MeasureText(line.Length == 0 ? " " : line, font);
                cellW = Math.Max(cellW, size.Width);
                cellH += size.Height;
            }
            int w = Math.Max(1, cellW + 40);
            int h = Math.Max(1, cellH + 40);

            var canvas = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(canvas))
            {
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                using (var format = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center,
                    FormatFlags = StringFormatFlags.NoWrap
                })
                using (var brush = new SolidBrush(centerLabel.ForeColor))
                {
                    g.DrawString(text, font, brush, new RectangleF(0, 0, w, h), format);
                }
            }

            Rectangle ink = FindInkBounds(canvas);
            if (ink.IsEmpty)
            {
                canvas.Dispose();
                return new Bitmap(1, 1, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            }
            var tight = canvas.Clone(ink, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            canvas.Dispose();
            return tight;
        }

        private static Rectangle FindInkBounds(Bitmap bitmap)
        {
            var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            var data = bitmap.LockBits(rect,
                System.Drawing.Imaging.ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            try
            {
                int stride = data.Stride;
                var buffer = new byte[stride * bitmap.Height];
                Marshal.Copy(data.Scan0, buffer, 0, buffer.Length);
                int minX = bitmap.Width, minY = bitmap.Height, maxX = -1, maxY = -1;
                for (int y = 0; y < bitmap.Height; y++)
                {
                    int row = y * stride;
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        if (buffer[row + x * 4 + 3] <= 8) continue;
                        if (x < minX) minX = x;
                        if (x > maxX) maxX = x;
                        if (y < minY) minY = y;
                        if (y > maxY) maxY = y;
                    }
                }
                return maxX < 0 ? Rectangle.Empty : Rectangle.FromLTRB(minX, minY, maxX + 1, maxY + 1);
            }
            finally
            {
                bitmap.UnlockBits(data);
            }
        }

        // UpdateLayeredWindow 要求预乘 alpha，否则半透明边缘会发白。
        // 透明度在这里乘进位图，可见像素即使乘成 0 也至少保留 alpha=1，
        // 否则窗口会在低透明度时整体变成鼠标穿透
        private static void PremultiplyAlpha(Bitmap bitmap, double opacity)
        {
            int opacity255 = Math.Max(0, Math.Min(255, (int)Math.Round(opacity * 255)));
            var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            var data = bitmap.LockBits(rect,
                System.Drawing.Imaging.ImageLockMode.ReadWrite,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            try
            {
                int bytes = Math.Abs(data.Stride) * data.Height;
                byte[] buffer = new byte[bytes];
                Marshal.Copy(data.Scan0, buffer, 0, bytes);
                for (int i = 0; i < bytes; i += 4)
                {
                    byte alpha = buffer[i + 3];
                    if (alpha == 0)
                    {
                        buffer[i] = 0;
                        buffer[i + 1] = 0;
                        buffer[i + 2] = 0;
                        continue;
                    }
                    int finalAlpha = alpha * opacity255 / 255;
                    if (finalAlpha == 0) finalAlpha = 1;
                    buffer[i + 3] = (byte)finalAlpha;
                    if (finalAlpha == 255) continue;
                    buffer[i] = (byte)(buffer[i] * finalAlpha / 255);
                    buffer[i + 1] = (byte)(buffer[i + 1] * finalAlpha / 255);
                    buffer[i + 2] = (byte)(buffer[i + 2] * finalAlpha / 255);
                }
                Marshal.Copy(buffer, 0, data.Scan0, bytes);
            }
            finally
            {
                bitmap.UnlockBits(data);
            }
        }

        private void UpdateLayer(Bitmap bitmap)
        {
            // 窗口显示或属性变化后，WinForms 会按 CreateParams 重写扩展样式，这里每帧补回
            int exStyle = GetWindowLong(Handle, GWL_EXSTYLE);
            if ((exStyle & WS_EX_LAYERED) == 0)
                SetWindowLong(Handle, GWL_EXSTYLE, exStyle | WS_EX_LAYERED);

            IntPtr screenDc = GetDC(IntPtr.Zero);
            IntPtr memDc = CreateCompatibleDC(screenDc);
            IntPtr hBitmap = bitmap.GetHbitmap(Color.FromArgb(0));
            IntPtr oldBitmap = SelectObject(memDc, hBitmap);
            try
            {
                var size = new Size(bitmap.Width, bitmap.Height);
                var source = new Point(0, 0);
                var blend = new BLENDFUNCTION
                {
                    BlendOp = AC_SRC_OVER,
                    BlendFlags = 0,
                    SourceConstantAlpha = 255,
                    AlphaFormat = AC_SRC_ALPHA
                };
                UpdateLayeredWindow(Handle, screenDc, IntPtr.Zero, ref size,
                    memDc, ref source, 0, ref blend, ULW_ALPHA);
            }
            finally
            {
                SelectObject(memDc, oldBitmap);
                DeleteObject(hBitmap);
                DeleteDC(memDc);
                ReleaseDC(IntPtr.Zero, screenDc);
            }
        }

        // WebControlServer配套方法
        public void ToggleCountdown()
        {
            if (isCountingDown)
            {
                isCountingDown = false;
                timer1.Stop();
            }
            else
            {
                alarmTriggered = false;
                preAlertTriggered = false;
                isCountingDown = true;
                timer1.Start();
            }
            UpdateSlideToolTip();
        }
        public void SetRemainingSeconds(int seconds)
        {
            remainingSeconds = seconds;
            UpdateDisplay();
        }
        public string GetCountdownState()
        {
            if (isCountingDown) return "运行中";
            return "已暂停";
        }
        public int GetRemainingSeconds() => remainingSeconds;
        public bool IsCountingDown() => isCountingDown;
        public void MoveToRightTop()
        {
            var screen = Screen.FromControl(this).WorkingArea;
            this.Location = new Point(screen.Right - this.Width - 50, screen.Top + 50);
            SaveWindowLocation();
        }
        public void MoveToLeftTop()
        {
            var screen = Screen.FromControl(this).WorkingArea;
            this.Location = new Point(screen.Left + 50, screen.Top + 50);
            SaveWindowLocation();
        }
        public void MoveToTopCenter()
        {
            var screen = Screen.FromControl(this).WorkingArea;
            int x = (screen.Width - this.Width) / 2 + screen.Left;
            this.Location = new Point(x, screen.Top + 50);
            SaveWindowLocation();
        }
        public void MoveBy(int dx, int dy)
        {
            this.Location = new Point(this.Left + dx, this.Top + dy);
            SaveWindowLocation();
        }
        public bool IsTimerVisible() => timerVisible;
        public void ToggleTimerVisible()
        {
            timerVisible = !timerVisible;
            this.Invoke(new Action(() =>
            {
                this.Visible = timerVisible;
                if (timerVisible) RestoreTopMost();
            }));
            SaveSwitchStates();
        }

        // 隐藏后再显示，窗口会丢掉 TopMost 层级，需主动置顶一次（不抢占焦点）
        private void RestoreTopMost()
        {
            if (!IsHandleCreated) return;
            SetWindowPos(this.Handle, HWND_TOPMOST, 0, 0, 0, 0,
                SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE | SWP_SHOWWINDOW);
        }
        public bool IsFlash3Enabled() => flash3Enabled;
        public void ToggleFlash3()
        {
            flash3Enabled = !flash3Enabled;
            SaveSwitchStates();
        }
        public bool IsPptAutoEnabled() => pptAutoEnabled;
        public void TogglePptAuto()
        {
            pptAutoEnabled = !pptAutoEnabled;
            var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"SOFTWARE\\801CountdownApp1");
            key.SetValue("PPTAuto", pptAutoEnabled ? 1 : 0);
            key.Close();
            if (pptAutoEnabled || showSlideInfoEnabled) pptTimer.Start(); else pptTimer.Stop();
        }
        public bool IsShowSlideInfoEnabled() => showSlideInfoEnabled;
        public void ToggleShowSlideInfo()
        {
            showSlideInfoEnabled = !showSlideInfoEnabled;
            var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"SOFTWARE\\801CountdownApp1");
            key.SetValue("ShowSlideInfo", showSlideInfoEnabled ? 1 : 0);
            key.Close();
            if (pptAutoEnabled || showSlideInfoEnabled) pptTimer.Start(); else pptTimer.Stop();
        }
        public int GetTotalSeconds() => totalSeconds;
        public bool IsSoundEnabled()
        {
            var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\\801CountdownApp1");
            bool result = key?.GetValue("Sound") is int s && s == 1;
            key?.Close();
            return result;
        }
        public void TestSound()
        {
            PlayEndSound(force: true);
        }
        public void ToggleSound()
        {
            var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"SOFTWARE\\801CountdownApp1");
            bool current = key.GetValue("Sound") is int sv && sv == 1;
            key.SetValue("Sound", current ? 0 : 1);
            key.Close();
        }
        public int GetFontSize() => (int)Math.Round(originalFontSize);
        public void SetFontSize(int size)
        {
            size = Math.Max(8, Math.Min(72, size));
            centerLabel.Font = new Font(centerLabel.Font.FontFamily, size, centerLabel.Font.Style);
            originalFontSize = size;
            var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"SOFTWARE\\801CountdownApp1");
            key.SetValue("FontSizeSlider", size);
            key.SetValue("FontSize", (float)size);
            key.Close();
            AutoSizeForm();
            RedrawLayered();
        }
        public int GetOpacityValue() => (int)Math.Round(layerOpacity * 250);
        public void SetOpacity(int value)
        {
            layerOpacity = value / 250.0;
            RedrawLayered();
            var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"SOFTWARE\\801CountdownApp1");
            key.SetValue("Opacity", value);
            key.Close();
        }
        private void SaveSwitchStates()
        {
            var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"SOFTWARE\\801CountdownApp1");
            key.SetValue("TimerVisible", timerVisible ? 1 : 0);
            key.SetValue("Flash", flash3Enabled ? 1 : 0);
            key.Close();
        }
        private void LoadSwitchStates()
        {
            var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\\801CountdownApp1");
            if (key != null)
            {
                object tv = key.GetValue("TimerVisible");
                object fl = key.GetValue("Flash");
                timerVisible = (tv == null) ? true : ((int)tv == 1);
                flash3Enabled = (fl == null) ? true : ((int)fl == 1);
                key.Close();
            }
            else
            {
                timerVisible = true;
                flash3Enabled = true;
            }
            this.Visible = timerVisible;
        }
        public bool IsAllowNegativeEnabled() => allowNegative;
        public void ToggleAllowNegative()
        {
            allowNegative = !allowNegative;
            var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"SOFTWARE\\801CountdownApp1");
            key.SetValue("AllowNegative", allowNegative ? 1 : 0);
            key.Close();
        }
        public bool IsPureSlideModeEnabled() => pureSlideMode;
        public string GetLastSlideInfo() => lastSlideInfo;
        public void TogglePureSlideMode()
        {
            pureSlideMode = !pureSlideMode;
            var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"SOFTWARE\\801CountdownApp1");
            key.SetValue("PureSlideMode", pureSlideMode ? 1 : 0);
            key.Close();
            if (pureSlideMode) pptTimer.Start();
            else if (!pptAutoEnabled && !showSlideInfoEnabled) pptTimer.Stop();
            ClearSlideInfo();
            UpdateDisplay();
        }
        // 网页端「全屏放映」：优先 PowerPoint，再试 WPS 演示
        public void StartSlideShow()
        {
            Ppt.Application pptApp = null;
            try
            {
                pptApp = Marshal.GetActiveObject("PowerPoint.Application") as Ppt.Application;
                if (pptApp != null)
                {
                    pptApp.ActivePresentation.SlideShowSettings.Run();
                    return;
                }
            }
            catch { }
            finally
            {
                ReleaseComObject(pptApp);
            }

            foreach (var progId in new[] { "KWPP.Application", "WPP.Application" })
            {
                object app = null;
                try
                {
                    app = Marshal.GetActiveObject(progId);
                    if (app == null) continue;
                    ((dynamic)app).ActivePresentation.SlideShowSettings.Run();
                    return;
                }
                catch { }
                finally
                {
                    ReleaseComObject(app);
                }
            }
        }

        // 网页端「翻页笔」：next/prev/goto/black/white/end
        public bool SlideShowControl(string action, int page, out string error)
        {
            error = null;
            if (LoadPresenterMode() == 1) return SendPresenterKeys(action, page, out error);
            string err = null;
            bool ok = false;
            if (!System.Threading.Monitor.TryEnter(previewExportLock, 1000))
            {
                error = "预览正在生成，请稍后再试";
                return false;
            }
            try
            {
                var thread = new System.Threading.Thread(() =>
                {
                    object app = null, ssWindows = null, ssWindow = null, view = null;
                    try
                    {
                        try { app = Marshal.GetActiveObject("PowerPoint.Application"); } catch { }
                        if (app == null) { try { app = Marshal.GetActiveObject("KWPP.Application"); } catch { } }
                        if (app == null) { try { app = Marshal.GetActiveObject("WPP.Application"); } catch { } }
                        if (app == null) { err = "未检测到正在运行的 PowerPoint 或 WPS 演示"; return; }

                        dynamic dynApp = app;
                        ssWindows = dynApp.SlideShowWindows;
                        if (ssWindows == null || (int)((dynamic)ssWindows).Count < 1) { err = "当前没有全屏放映"; return; }
                        ssWindow = ((dynamic)ssWindows)[1];
                        view = ((dynamic)ssWindow).View;
                        dynamic v = view;
                        switch (action)
                        {
                            case "next": v.Next(); break;
                            case "prev": v.Previous(); break;
                            case "goto":
                                if (page < 1) { err = "页码无效"; return; }
                                v.GotoSlide(page, true);
                                break;
                            case "end":
                                v.Exit();
                                break;
                            default:
                                err = "未知操作";
                                return;
                        }
                        ok = true;
                    }
                    catch (Exception ex) { err = "操作失败：" + ex.Message; }
                    finally
                    {
                        ReleaseComObject(view);
                        ReleaseComObject(ssWindow);
                        ReleaseComObject(ssWindows);
                        ReleaseComObject(app);
                    }
                });
                thread.IsBackground = true;
                thread.SetApartmentState(System.Threading.ApartmentState.STA);
                thread.Start();
                thread.Join();
            }
            finally
            {
                System.Threading.Monitor.Exit(previewExportLock);
            }
            error = err;
            return ok;
        }

        private static int LoadPresenterMode()
        {
            try
            {
                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\\801CountdownApp1"))
                {
                    if (key?.GetValue("PresenterMode") is int mode && mode == 1) return 1;
                }
            }
            catch { }
            return 0;
        }

        public int GetPresenterMode()
        {
            return LoadPresenterMode();
        }

        public int GetPodiumMode()
        {
            try
            {
                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\\801CountdownApp1"))
                {
                    if (key?.GetValue("PodiumMode") is int mode && mode == 1) return 1;
                }
            }
            catch { }
            return 0;
        }

        public void SetPodiumMode(int mode)
        {
            try
            {
                using (var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"SOFTWARE\\801CountdownApp1"))
                {
                    key.SetValue("PodiumMode", mode == 1 ? 1 : 0);
                }
            }
            catch { }
        }

        public void SetPresenterMode(int mode)
        {
            try
            {
                using (var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"SOFTWARE\\801CountdownApp1"))
                {
                    key.SetValue("PresenterMode", mode == 1 ? 1 : 0);
                }
            }
            catch { }
        }

        private static void PressVirtualKey(byte vk)
        {
            keybd_event(vk, 0, 0, UIntPtr.Zero);
            System.Threading.Thread.Sleep(20);
            keybd_event(vk, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        }

        // 模拟按键模式：和实体翻页笔一样发 PageUp/PageDown 等按键，放映窗口必须在前台
        private static bool SendPresenterKeys(string action, int page, out string error)
        {
            error = null;
            string procName = "";
            try
            {
                uint pid;
                GetWindowThreadProcessId(GetForegroundWindow(), out pid);
                procName = Process.GetProcessById((int)pid).ProcessName;
            }
            catch { }
            if (!MatchesAny(procName, SlideReadableProcs))
            {
                error = "请先把放映窗口切到前台";
                return false;
            }
            switch (action)
            {
                case "next": PressVirtualKey(0x22); break;   // PageDown
                case "prev": PressVirtualKey(0x21); break;   // PageUp
                case "goto":
                    if (page < 1) { error = "页码无效"; return false; }
                    foreach (char c in page.ToString())
                    {
                        PressVirtualKey((byte)(0x30 + (c - '0')));
                        System.Threading.Thread.Sleep(30);
                    }
                    PressVirtualKey(0x0D);                   // Enter
                    break;
                case "end": PressVirtualKey(0x1B); break;    // Esc
                default: error = "未知操作"; return false;
            }
            return true;
        }

        // 当前正在运行的放映软件名称，供网页端确认弹窗展示
        public string GetPresentationAppName()
        {
            bool hasPpt = false, hasWps = false;
            try { hasPpt = Process.GetProcessesByName("powerpnt").Length > 0; } catch { }
            try { hasWps = Process.GetProcessesByName("wpp").Length > 0; } catch { }
            if (hasPpt && hasWps) return "Microsoft PowerPoint 和 WPS 演示";
            if (hasPpt) return "Microsoft PowerPoint";
            if (hasWps) return "WPS 演示";
            return "";
        }

        // 网页预览轮询用：返回当前放映页，导出进行中或未放映时返回 0
        public int GetCurrentSlideIndex()
        {
            // 页数检测定时器本来就在轮询，优先复用它的结果，少一次 COM 调用
            int known = lastSlideIndex;
            if (known > 0) return known;
            if (!System.Threading.Monitor.TryEnter(previewExportLock)) return 0;
            try
            {
                int result = 0;
                var thread = new System.Threading.Thread(() =>
                {
                    object app = null, ssWindows = null, ssWindow = null, view = null, slide = null;
                    try
                    {
                        try { app = Marshal.GetActiveObject("PowerPoint.Application"); } catch { }
                        if (app == null) { try { app = Marshal.GetActiveObject("KWPP.Application"); } catch { } }
                        if (app == null) { try { app = Marshal.GetActiveObject("WPP.Application"); } catch { } }
                        if (app == null) return;
                        dynamic dynApp = app;
                        ssWindows = dynApp.SlideShowWindows;
                        if (ssWindows == null || (int)((dynamic)ssWindows).Count < 1) return;
                        ssWindow = ((dynamic)ssWindows)[1];
                        view = ((dynamic)ssWindow).View;
                        slide = ((dynamic)view).Slide;
                        result = (int)((dynamic)slide).SlideIndex;
                    }
                    catch { }
                    finally
                    {
                        ReleaseComObject(slide);
                        ReleaseComObject(view);
                        ReleaseComObject(ssWindow);
                        ReleaseComObject(ssWindows);
                        ReleaseComObject(app);
                    }
                });
                thread.IsBackground = true;
                thread.SetApartmentState(System.Threading.ApartmentState.STA);
                thread.Start();
                thread.Join();
                return result;
            }
            finally
            {
                System.Threading.Monitor.Exit(previewExportLock);
            }
        }

        // 网页端「文档预览」：每导出一页就回调一页；文档未变时直接复用缓存
        public void StreamPresentationPreview(Action<PreviewProgress> onStart, Action<int, string> onPage, Action<string> onError)
        {
            if (System.Threading.Interlocked.CompareExchange(ref previewExportActive, 1, 0) != 0)
            {
                onError?.Invoke("预览正在生成，请稍后再试");
                return;
            }
            try
            {
                var thread = new System.Threading.Thread(() =>
                {
                    try { ExportPresentationSlides(onStart, onPage, onError); }
                    catch (Exception ex) { onError?.Invoke("生成预览失败：" + ex.Message); }
                });
                thread.IsBackground = true;
                thread.SetApartmentState(System.Threading.ApartmentState.STA);
                thread.Start();
                thread.Join();
            }
            finally
            {
                System.Threading.Volatile.Write(ref previewExportActive, 0);
            }
        }

        private void ExportPresentationSlides(Action<PreviewProgress> onStart, Action<int, string> onPage, Action<string> onError)
        {
            object app = null, presentation = null, slides = null;
            object ssWindows = null, ssWindow = null, view = null, currentSlide = null;
            try
            {
                try { app = Marshal.GetActiveObject("PowerPoint.Application"); } catch { }
                if (app == null) { try { app = Marshal.GetActiveObject("KWPP.Application"); } catch { } }
                if (app == null) { try { app = Marshal.GetActiveObject("WPP.Application"); } catch { } }
                if (app == null)
                {
                    onError?.Invoke("未检测到正在运行的 PowerPoint 或 WPS 演示");
                    return;
                }

                dynamic dynApp = app;
                int current = 0;
                try
                {
                    ssWindows = dynApp.SlideShowWindows;
                    if (ssWindows != null && (int)((dynamic)ssWindows).Count > 0)
                    {
                        ssWindow = ((dynamic)ssWindows)[1];
                        presentation = ((dynamic)ssWindow).Presentation;
                        view = ((dynamic)ssWindow).View;
                        currentSlide = ((dynamic)view).Slide;
                        current = (int)((dynamic)currentSlide).SlideIndex;
                    }
                }
                catch { }
                if (presentation == null)
                {
                    try { presentation = dynApp.ActivePresentation; } catch { }
                }
                if (presentation == null)
                {
                    onError?.Invoke("未找到正在编辑或放映的演示文稿");
                    return;
                }

                slides = ((dynamic)presentation).Slides;
                int total = (int)((dynamic)slides).Count;
                if (total < 1)
                {
                    onError?.Invoke("演示文稿没有页面");
                    return;
                }
                bool presenting = current > 0;
                if (current < 1) current = 1;
                if (current > total) current = total;

                string root = System.IO.Path.Combine(Application.StartupPath, "preview");
                System.IO.Directory.CreateDirectory(root);
                string session = BuildPreviewSessionId((dynamic)presentation, total);
                string dir = System.IO.Path.Combine(root, session);
                System.IO.Directory.CreateDirectory(dir);
                TouchPreviewCache(root, dir);
                int thumbW, thumbH;
                GetSlideExportSize((dynamic)presentation, 640, out thumbW, out thumbH);
                // 记录文档来源，高清下载时校验是不是同一份文档
                try
                {
                    string docName = "";
                    try { docName = ((dynamic)presentation).FullName as string ?? ""; } catch { }
                    System.IO.File.WriteAllText(System.IO.Path.Combine(dir, "meta.txt"), docName + "\n" + total);
                }
                catch { }

                onStart?.Invoke(new PreviewProgress { Total = total, Current = current, Session = session });

                // 当前页优先导出，前端可以边导边显示
                var order = new List<int>();
                for (int i = current; i <= total; i++) order.Add(i);
                for (int i = 1; i < current; i++) order.Add(i);

                int exported = 0;
                foreach (int i in order)
                {
                    string file = System.IO.Path.Combine(dir, i + ".png");
                    if (!System.IO.File.Exists(file))
                    {
                        object slideObj = null;
                        string temp = file + ".tmp";
                        bool lockTaken = false;
                        try
                        {
                            System.Threading.Monitor.Enter(previewExportLock);
                            lockTaken = true;
                            slideObj = ((dynamic)slides)[i];
                            ((dynamic)slideObj).Export(temp, "PNG", thumbW, thumbH);
                            if (System.IO.File.Exists(temp)) System.IO.File.Move(temp, file);
                        }
                        catch
                        {
                            try { if (System.IO.File.Exists(temp)) System.IO.File.Delete(temp); } catch { }
                        }
                        finally
                        {
                            ReleaseComObject(slideObj);
                            if (lockTaken) System.Threading.Monitor.Exit(previewExportLock);
                        }
                        // 放映中导出时留一点空档，让放映软件能及时处理翻页按键
                        if (presenting) System.Threading.Thread.Sleep(60);
                    }
                    if (System.IO.File.Exists(file))
                    {
                        exported++;
                        onPage?.Invoke(i, $"/previewfile/{session}/{i}.png");
                    }
                }
                if (exported == 0) onError?.Invoke("当前放映软件不支持导出页面预览");
            }
            finally
            {
                ReleaseComObject(currentSlide);
                ReleaseComObject(view);
                ReleaseComObject(ssWindow);
                ReleaseComObject(ssWindows);
                ReleaseComObject(slides);
                ReleaseComObject(presentation);
                ReleaseComObject(app);
            }
        }

        // 网页端「下载图片」：按需重新导出这一页的高清图
        public string ExportHighResSlide(string session, int page, out string error)
        {
            error = null;
            if (!IsPreviewSessionId(session) || page < 1)
            {
                error = "参数错误";
                return null;
            }
            lock (previewExportLock)
            {
                string file = null;
                string err = null;
                var thread = new System.Threading.Thread(() =>
                {
                    object app = null, presentation = null, slides = null, slide = null;
                    try
                    {
                        try { app = Marshal.GetActiveObject("PowerPoint.Application"); } catch { }
                        if (app == null) { try { app = Marshal.GetActiveObject("KWPP.Application"); } catch { } }
                        if (app == null) { try { app = Marshal.GetActiveObject("WPP.Application"); } catch { } }
                        if (app == null) { err = "未检测到正在运行的 PowerPoint 或 WPS 演示"; return; }

                        dynamic dynApp = app;
                        object ssWindows = null, ssWindow = null;
                        try
                        {
                            ssWindows = dynApp.SlideShowWindows;
                            if (ssWindows != null && (int)((dynamic)ssWindows).Count > 0)
                            {
                                ssWindow = ((dynamic)ssWindows)[1];
                                presentation = ((dynamic)ssWindow).Presentation;
                            }
                        }
                        catch { }
                        finally
                        {
                            ReleaseComObject(ssWindow);
                            ReleaseComObject(ssWindows);
                        }
                        if (presentation == null)
                        {
                            try { presentation = dynApp.ActivePresentation; } catch { }
                        }
                        if (presentation == null) { err = "未找到正在编辑或放映的演示文稿"; return; }

                        slides = ((dynamic)presentation).Slides;
                        int total = (int)((dynamic)slides).Count;
                        if (page > total) { err = "页码超出范围"; return; }
                        string currentSession = BuildPreviewSessionId((dynamic)presentation, total);
                        if (!string.Equals(currentSession, session, StringComparison.OrdinalIgnoreCase))
                        {
                            err = "文档已变化，请重新打开预览";
                            return;
                        }

                        string dir = System.IO.Path.Combine(Application.StartupPath, "preview", session);
                        System.IO.Directory.CreateDirectory(dir);
                        string target = System.IO.Path.Combine(dir, page + "_hd.png");
                        string temp = target + ".tmp";
                        int hdW, hdH;
                        GetSlideExportSize((dynamic)presentation, 1920, out hdW, out hdH);
                        try
                        {
                            slide = ((dynamic)slides)[page];
                            ((dynamic)slide).Export(temp, "PNG", hdW, hdH);
                            if (!System.IO.File.Exists(temp)) { err = "导出失败"; return; }
                            try { if (System.IO.File.Exists(target)) System.IO.File.Delete(target); } catch { }
                            System.IO.File.Move(temp, target);
                            file = target;
                        }
                        catch (Exception ex) { err = "导出失败：" + ex.Message; }
                        finally
                        {
                            try { if (System.IO.File.Exists(temp)) System.IO.File.Delete(temp); } catch { }
                        }
                    }
                    catch (Exception ex) { err = "导出失败：" + ex.Message; }
                    finally
                    {
                        ReleaseComObject(slide);
                        ReleaseComObject(slides);
                        ReleaseComObject(presentation);
                        ReleaseComObject(app);
                    }
                });
                thread.IsBackground = true;
                thread.SetApartmentState(System.Threading.ApartmentState.STA);
                thread.Start();
                thread.Join();
                error = err ?? (file == null ? "导出失败" : null);
                return file;
            }
        }

        // 网页端「翻页笔」：当前页低分辨率预览，缓存有就直接读，没有才导出这一页
        public string ExportPresenterSlide(int page, out string error)
        {
            error = null;
            if (page < 1) { error = "参数错误"; return null; }
            if (!System.Threading.Monitor.TryEnter(previewExportLock, 1500))
            {
                error = "预览正在生成，请稍后再试";
                return null;
            }
            try
            {
                string file = null;
                string err = null;
                var thread = new System.Threading.Thread(() =>
                {
                    object app = null, presentation = null, slides = null, slide = null;
                    object ssWindows = null, ssWindow = null;
                    try
                    {
                        try { app = Marshal.GetActiveObject("PowerPoint.Application"); } catch { }
                        if (app == null) { try { app = Marshal.GetActiveObject("KWPP.Application"); } catch { } }
                        if (app == null) { try { app = Marshal.GetActiveObject("WPP.Application"); } catch { } }
                        if (app == null) { err = "未检测到正在运行的 PowerPoint 或 WPS 演示"; return; }

                        dynamic dynApp = app;
                        try
                        {
                            ssWindows = dynApp.SlideShowWindows;
                            if (ssWindows != null && (int)((dynamic)ssWindows).Count > 0)
                            {
                                ssWindow = ((dynamic)ssWindows)[1];
                                presentation = ((dynamic)ssWindow).Presentation;
                            }
                        }
                        catch { }
                        finally
                        {
                            ReleaseComObject(ssWindow);
                            ReleaseComObject(ssWindows);
                        }
                        if (presentation == null)
                        {
                            try { presentation = dynApp.ActivePresentation; } catch { }
                        }
                        if (presentation == null) { err = "未找到正在放映的演示文稿"; return; }

                        slides = ((dynamic)presentation).Slides;
                        int total = (int)((dynamic)slides).Count;
                        if (page > total) { err = "页码超出范围"; return; }

                        string session = BuildPreviewSessionId((dynamic)presentation, total);
                        string root = System.IO.Path.Combine(Application.StartupPath, "preview");
                        string dir = System.IO.Path.Combine(root, session);
                        System.IO.Directory.CreateDirectory(dir);
                        TouchPreviewCache(root, dir);
                        string target = System.IO.Path.Combine(dir, page + ".png");
                        if (!System.IO.File.Exists(target))
                        {
                            string temp = target + ".tmp";
                            int thumbW, thumbH;
                            GetSlideExportSize((dynamic)presentation, 640, out thumbW, out thumbH);
                            try
                            {
                                slide = ((dynamic)slides)[page];
                                ((dynamic)slide).Export(temp, "PNG", thumbW, thumbH);
                                if (System.IO.File.Exists(temp)) System.IO.File.Move(temp, target);
                            }
                            catch { }
                            finally
                            {
                                ReleaseComObject(slide);
                                try { if (System.IO.File.Exists(temp)) System.IO.File.Delete(temp); } catch { }
                            }
                        }
                        if (System.IO.File.Exists(target)) file = target;
                        else err = "导出失败";
                    }
                    catch (Exception ex) { err = "导出失败：" + ex.Message; }
                    finally
                    {
                        ReleaseComObject(slides);
                        ReleaseComObject(presentation);
                        ReleaseComObject(app);
                    }
                });
                thread.IsBackground = true;
                thread.SetApartmentState(System.Threading.ApartmentState.STA);
                thread.Start();
                thread.Join();
                error = err ?? (file == null ? "导出失败" : null);
                return file;
            }
            finally
            {
                System.Threading.Monitor.Exit(previewExportLock);
            }
        }

        private static bool IsPreviewSessionId(string s)
        {
            if (string.IsNullOrEmpty(s) || s.Length != 16) return false;
            foreach (char c in s)
            {
                bool hex = (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
                if (!hex) return false;
            }
            return true;
        }

        // 读取文档页面比例，4:3、A4 等非 16:9 文档保持原比例导出，避免拉伸变形
        private static double GetSlideRatio(dynamic presentation)
        {
            double ratio = 16.0 / 9.0;
            try
            {
                double w = Convert.ToDouble(((dynamic)presentation).PageSetup.SlideWidth);
                double h = Convert.ToDouble(((dynamic)presentation).PageSetup.SlideHeight);
                if (w > 0 && h > 0) ratio = w / h;
            }
            catch { }
            if (ratio < 0.2 || ratio > 5.0) ratio = 16.0 / 9.0;
            return ratio;
        }

        private static void GetSlideExportSize(dynamic presentation, int longSide, out int width, out int height)
        {
            double ratio = GetSlideRatio(presentation);
            if (ratio >= 1.0)
            {
                width = longSide;
                height = Math.Max(1, (int)Math.Round(longSide / ratio));
            }
            else
            {
                height = longSide;
                width = Math.Max(1, (int)Math.Round(longSide * ratio));
            }
        }

        // 缓存键 = 文档路径 + 大小 + 修改时间 + 页数 + 比例；未保存的文档每次生成新缓存
        private static string BuildPreviewSessionId(dynamic presentation, int total)
        {
            string key = null;
            try
            {
                string fullName = ((dynamic)presentation).FullName as string;
                if (!string.IsNullOrEmpty(fullName) && System.IO.File.Exists(fullName))
                {
                    var info = new System.IO.FileInfo(fullName);
                    key = fullName + "|" + info.Length + "|" + info.LastWriteTimeUtc.Ticks + "|" + total + "|thumb640|"
                        + GetSlideRatio(presentation).ToString("F3", System.Globalization.CultureInfo.InvariantCulture);
                }
            }
            catch { }
            if (key == null) key = Guid.NewGuid().ToString("N");
            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(key));
                var sb = new System.Text.StringBuilder(16);
                for (int i = 0; i < 8; i++) sb.Append(hash[i].ToString("x2"));
                return sb.ToString();
            }
        }

        // 保留最近使用的 10 份文档缓存，超出时淘汰最久未使用的一份
        private static void TouchPreviewCache(string root, string current)
        {
            const int maxCaches = 10;
            try { System.IO.Directory.SetLastWriteTimeUtc(current, DateTime.UtcNow); } catch { }
            try
            {
                string[] dirs = System.IO.Directory.GetDirectories(root);
                if (dirs.Length <= maxCaches) return;
                Array.Sort(dirs, (a, b) => System.IO.Directory.GetLastWriteTimeUtc(a)
                    .CompareTo(System.IO.Directory.GetLastWriteTimeUtc(b)));
                int remaining = dirs.Length;
                foreach (string dir in dirs)
                {
                    if (remaining <= maxCaches) break;
                    if (string.Equals(dir, current, StringComparison.OrdinalIgnoreCase)) continue;
                    try
                    {
                        System.IO.Directory.Delete(dir, true);
                        remaining--;
                    }
                    catch { }
                }
            }
            catch { }
        }

        // 网页端「关闭放映软件」：强制结束进程，未保存内容会丢失
        public void CloseSlideShowApp()
        {
            foreach (var name in new[] { "powerpnt", "wpp" })
            {
                try
                {
                    foreach (var p in Process.GetProcessesByName(name))
                    {
                        try { p.Kill(); } catch { }
                    }
                }
                catch { }
            }
        }

        public void SetTotalTimeMinutes(int minutes)
        {
            if (minutes < 1) minutes = 1;
            // 只写入注册表，不立即生效，等待下次重置
            var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"SOFTWARE\\801CountdownApp1");
            key.SetValue("TotalTime", minutes.ToString());
            key.SetValue("TimeUnit", 0); // 0=分
            key.Close();
        }
        public string GetForegroundAppInfo()
        {
            IntPtr hWnd = GetForegroundWindow();
            if (hWnd == IntPtr.Zero) return "";
            var sb = new System.Text.StringBuilder(512);
            GetWindowText(hWnd, sb, sb.Capacity);
            string title = sb.ToString().Trim();
            GetWindowThreadProcessId(hWnd, out uint pid);
            string procName = "";
            try { using (var p = System.Diagnostics.Process.GetProcessById((int)pid)) procName = p.ProcessName; } catch { }
            if (string.IsNullOrEmpty(title) && string.IsNullOrEmpty(procName)) return "";
            return $"{procName} | {title}";
        }
    }

    public class PreviewProgress
    {
        public int Total;
        public int Current;
        public string Session = "";
    }
}
