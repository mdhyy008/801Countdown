using System;
using System.Collections.Generic;
using System.Drawing;
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
        private int? cachedTotalSlides;
        private bool slideHeightAdjusted;
        private float originalFontSize = 16f;
        private bool allowNegative;
        private bool alarmTriggered;
        private bool pureSlideMode;
        private ToolStripMenuItem screenMenuItem;

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
        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
        [StructLayout(LayoutKind.Sequential)]
        private struct RECT { public int Left; public int Top; public int Right; public int Bottom; }
        public Form1()
        {
            RegisterUrlAcl();
            AddFirewallRule();
            InitializeComponent();
            this.DoubleBuffered = true;
            this.TopMost = true;
            UpdateDisplay();
            centerLabel.MouseDoubleClick += CenterLabel_MouseDoubleClick;
            centerLabel.MouseUp += CenterLabel_MouseUp;
            centerLabel.MouseDown += Form_MouseDown;
            centerLabel.MouseMove += Form_MouseMove;
            centerLabel.MouseUp += Form_MouseUp;
            this.MouseDown += Form_MouseDown;
            this.MouseMove += Form_MouseMove;
            this.MouseUp += Form1_MouseUp;
            timer1.Tick += Timer1_Tick;
            // 右键菜单事件
            resetMenuItem.Click += ResetMenuItem_Click;
            topCenterMenuItem.Click += TopCenterMenuItem_Click;
            topRightMenuItem.Click += TopRightMenuItem_Click;
            closeMenuItem.Click += CloseMenuItem_Click;
            settingsMenuItem.Click += SettingsMenuItem_Click;
            // 启动Web控制服务器
            webServer = new WebControlServer(this);
            webServer.Start();
            // PPT全屏检测定时器
            pptTimer = new System.Windows.Forms.Timer(components);
            pptTimer.Interval = 500;
            pptTimer.Tick += PptTimer_Tick;
            this.FormClosing += Form1_FormClosing;
        }

        private void RegisterUrlAcl()
        {
            try
            {
                string url = "http://+:9051/";
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
                MessageBox.Show("注册URL ACL失败，请用管理员身份运行或手动执行：\r\nnetsh http add urlacl url=http://+:9051/ user=Everyone\r\n错误信息：" + ex.Message, "端口权限错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void AddFirewallRule()
        {
            try
            {
                string ruleName = "801CountdownApp1_9051";
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
                    string addArgs = $"advfirewall firewall add rule name={ruleName} dir=in action=allow protocol=TCP localport=9051";
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
                MessageBox.Show("添加防火墙规则失败，请用管理员身份运行或手动执行：\r\nnetsh advfirewall firewall add rule name=801CountdownApp1_9051 dir=in action=allow protocol=TCP localport=9051\r\n错误信息：" + ex.Message, "防火墙规则错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            if (remainingSeconds > 0 || allowNegative)
            {
                remainingSeconds--;
                UpdateDisplay();
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

        public async Task FlashWindowAsync()
        {
            Color origBack = this.BackColor;
            Color origFore = centerLabel.ForeColor;
            for (int i = 0; i < 3; i++)
            {
                this.BackColor = Color.White;
                centerLabel.ForeColor = Color.Red;
                await Task.Delay(300);
                this.BackColor = origBack;
                centerLabel.ForeColor = origFore;
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
            // 多屏切换菜单项（每次打开右键菜单时检测屏幕数量）
            screenMenuItem = new ToolStripMenuItem("切换屏幕");
            screenMenuItem.Click += ScreenMenuItem_Click;
            contextMenuStrip1.Items.Insert(contextMenuStrip1.Items.Count - 2, screenMenuItem);
            contextMenuStrip1.Opening += (_, __) => UpdateScreenMenuItemVisibility();
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

        private void CenterLabel_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                contextMenuStrip1.Show(centerLabel, e.Location);
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
            };
            form2.OpacityChanged += (s, value) =>
            {
                this.Opacity = value / 250.0;
            };
            form2.FontSizeChanged += (size) =>
            {
                originalFontSize = size;
                centerLabel.Font = new Font(centerLabel.Font.FontFamily, size, centerLabel.Font.Style);
                AutoSizeForm();
            };
            form2.ColorChanged += (fore, back) =>
            {
                centerLabel.ForeColor = fore;
                this.BackColor = back;
            };
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
                    UpdateDisplay();
                    timer1.Stop();
                    isCountingDown = false;
                }
                if (opacity != null)
                {
                    this.Opacity = (int)opacity / 250.0;
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
                key.Close();
                UpdateDisplay();
            }
            else
            {
                centerLabel.ForeColor = Color.White;
                this.BackColor = Color.Red;
            }
        }

        private void PlayEndSound()
        {
            var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\\801CountdownApp1");
            bool soundEnabled = key != null && key.GetValue("Sound") is int s && s == 1;
            key?.Close();
            if (!soundEnabled) return;

            try
            {
                string path = System.IO.Path.Combine(Application.StartupPath, "src", "微波炉.mp3");
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

            bool isPptFullscreen = IsPptFullscreen();

            if (isPptFullscreen)
            {
                if (pptAutoEnabled && !isCountingDown && remainingSeconds > 0)
                {
                    isCountingDown = true;
                    timer1.Start();
                }
                // 非全屏 → 全屏切换时清除页数缓存
                if (!pptWasFullscreen) cachedTotalSlides = null;
                string pageInfo = GetSlidePageInfo();
                if (pageInfo != lastSlideInfo)
                {
                    lastSlideInfo = pageInfo;
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
                    lastSlideInfo = null;
                    UpdateDisplay();
                }
                pptTimer.Interval = 2000; // 空闲中：低频轮询
            }
        }

        private static readonly string[] PresentationProcs = { "powerpnt", "wpp", "soffice", "focusky" };
        private static readonly string[] PdfViewerProcs = { "msedge", "chrome", "acroRd32", "foxitreader", "sumatrapdf", "nitropdf", "pdfxedit" };
        private static readonly string[] MediaPlayerProcs = { "vlc", "potplayermini", "mpc-hc", "mpc-be" };

        // 枚举所有窗口，检测各类全屏展示
        private bool IsPptFullscreen()
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
                    if (MatchesAny(fgProcName, PresentationProcs)) return true;
                    if (MatchesAny(fgProcName, PdfViewerProcs) && IsWindowTitlePdf(fgHwnd)) return true;
                    if (MatchesAny(fgProcName, MediaPlayerProcs)) return true;
                }
            }

            // 2. 后台路径：收集所有目标进程 PID
            var presPids = new HashSet<uint>();
            var pdfPids = new HashSet<uint>();
            var mediaPids = new HashSet<uint>();
            foreach (var name in PresentationProcs) CollectPids(name, presPids);
            foreach (var name in PdfViewerProcs) CollectPids(name, pdfPids);
            foreach (var name in MediaPlayerProcs) CollectPids(name, mediaPids);
            if (presPids.Count == 0 && pdfPids.Count == 0 && mediaPids.Count == 0) return false;

            // 3. 枚举窗口
            bool found = false;
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

                if (presPids.Contains(pid)) { found = true; return false; }
                if (pdfPids.Contains(pid) && IsWindowTitlePdf(hWnd)) { found = true; return false; }
                if (mediaPids.Contains(pid)) { found = true; return false; }
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
            string r;
            // 尝试 Microsoft PowerPoint（两级 fallback）
            r = TryGetSlideInfo("PowerPoint.Application");
            if (r != null) return r;
            // 尝试 WPS 演示
            r = TryGetSlideInfo("Ket.Application");
            if (r != null) return r;
            return null;
        }

        private string TryGetSlideInfo(string progId)
        {
            if (progId == "PowerPoint.Application")
            {
                return TryGetPptSlideInfoStrong();
            }
            else
            {
                return TryGetWpsSlideInfo();
            }
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
        private string TryGetWpsSlideInfo()
        {
            string progId = "Ket.Application";
            object app = null;
            try { app = Marshal.GetActiveObject(progId); }
            catch { }
            if (app == null)
            {
                try
                {
                    Type t = Type.GetTypeFromProgID(progId);
                    if (t != null) app = Activator.CreateInstance(t);
                }
                catch { }
            }
            if (app == null) return null;

            try
            {
                dynamic com = app;
                dynamic ssWindows = com.SlideShowWindows;
                if (ssWindows.Count > 0)
                {
                    dynamic ssWindow = ssWindows[1];
                    dynamic view = ssWindow.View;
                    dynamic slide = view.Slide;
                    int current = (int)slide.SlideIndex;
                    if (cachedTotalSlides == null)
                    {
                        cachedTotalSlides = (int)ssWindow.Presentation.Slides.Count;
                    }
                    return $"{current}/{cachedTotalSlides}";
                }
            }
            catch { }
            return null;
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
                return;
            }
            string text = FormatTime(remainingSeconds);
            bool showSlide = showSlideInfoEnabled && !string.IsNullOrEmpty(lastSlideInfo);
            if (showSlide && this.Height >= 50)
                text += "\nP" + lastSlideInfo;
            centerLabel.Text = text;
            AutoSizeForm();
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
            var textSize = TextRenderer.MeasureText(centerLabel.Text, centerLabel.Font);
            int newW = textSize.Width + 20;
            int newH = textSize.Height + 10;
            if (this.ClientSize.Width != newW || this.ClientSize.Height != newH)
            {
                this.ClientSize = new Size(newW, newH);
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
                isCountingDown = true;
                timer1.Start();
            }
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
            this.Invoke(new Action(() => { this.Visible = timerVisible; }));
            SaveSwitchStates();
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
            PlayEndSound();
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
            AutoSizeForm();
        }
        public int GetOpacityValue() => (int)(this.Opacity * 250);
        public void SetOpacity(int value)
        {
            this.Opacity = value / 250.0;
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
            lastSlideInfo = null;
            UpdateDisplay();
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
}
