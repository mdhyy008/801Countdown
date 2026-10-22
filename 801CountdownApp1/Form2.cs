using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using Microsoft.Win32;
using QRCoder;

namespace _801CountdownApp1
{
    public partial class Form2 : Form
    {
        public event EventHandler<Font> FontSelected;
        public event EventHandler<int> OpacityChanged;
        public event Action<int> FontSizeChanged;
        public event Action<int> PaddingValueChanged;
        public event Action<Color, Color> ColorChanged;
        public event Action<CountdownStyleKind> CountdownStyleChanged;
        public Font CurrentLabelFont { get; set; }
        private const int EM_SETCUEBANNER = 0x1501;
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, string lParam);

        private static readonly string[] soundFiles = { "微波炉.mp3", "马里奥金币.mp3" };
        private CheckBox checkBoxShowSlideInfo;
        private CheckBox checkBoxAllowNegative;
        private CheckBox checkBoxPureSlide;
        private CheckBox checkBoxSlideToolTip;
        private Label labelPresenterMode;
        private FlowLayoutPanel presenterModePanel;
        private Button buttonPresenterCom;
        private Button buttonPresenterKeys;
        private bool presenterKeysMode;
        private Image qrImage;
        private SidebarNavButton[] navButtons;
        private Control[] sections;

        public Form2()
        {
            InitializeComponent();
            buttonFont.Click += ButtonFont_Click;
            trackBarOpacity.Scroll += TrackBarOpacity_Scroll;
            trackBarFontSize.Scroll += TrackBarFontSize_Scroll;
            trackBarPadding.Scroll += TrackBarPadding_Scroll;
        }

        private void ButtonFont_Click(object sender, EventArgs e)
        {
            using (FontDialog fontDialog = new FontDialog())
            {
                if (CurrentLabelFont != null)
                {
                    fontDialog.Font = CurrentLabelFont;
                }
                if (fontDialog.ShowDialog() == DialogResult.OK)
                {
                    FontSelected?.Invoke(this, fontDialog.Font);
                    CurrentLabelFont = fontDialog.Font;
                    // 同步滑块到选中的字号
                    int fs = (int)Math.Round(fontDialog.Font.Size);
                    if (fs < trackBarFontSize.Minimum) fs = trackBarFontSize.Minimum;
                    if (fs > trackBarFontSize.Maximum) fs = trackBarFontSize.Maximum;
                    trackBarFontSize.Value = fs;
                    labelFontSizeVal.Text = fs.ToString();
                }
            }
        }

        private void TrackBarOpacity_Scroll(object sender, EventArgs e)
        {
            UpdateOpacityLabel();
            OpacityChanged?.Invoke(this, trackBarOpacity.Value);
        }

        private void UpdateOpacityLabel()
        {
            labelOpacityVal.Text = Math.Round(trackBarOpacity.Value * 100.0 / 250.0).ToString();
        }

        private void TrackBarFontSize_Scroll(object sender, EventArgs e)
        {
            labelFontSizeVal.Text = trackBarFontSize.Value.ToString();
            if (CurrentLabelFont != null)
            {
                CurrentLabelFont = new Font(CurrentLabelFont.FontFamily, trackBarFontSize.Value, CurrentLabelFont.Style);
            }
            FontSizeChanged?.Invoke(trackBarFontSize.Value);
        }

        private void TrackBarPadding_Scroll(object sender, EventArgs e)
        {
            labelPaddingVal.Text = trackBarPadding.Value.ToString();
            PaddingValueChanged?.Invoke(trackBarPadding.Value);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Close();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            textBoxIP.Text = GetLocalIPAddress();
        }

        private string GetLocalIPAddress()
        {
            string ip = "";
            foreach (var host in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (host.AddressFamily == AddressFamily.InterNetwork)
                {
                    ip = host.ToString();
                    break;
                }
            }
            return ip;
        }

        private void SetPresenterMode(bool keys)
        {
            presenterKeysMode = keys;
            if (buttonPresenterCom == null || buttonPresenterKeys == null) return;
            buttonPresenterCom.BackColor = keys ? Color.White : Color.Black;
            buttonPresenterCom.ForeColor = keys ? Color.Black : Color.White;
            buttonPresenterKeys.BackColor = keys ? Color.Black : Color.White;
            buttonPresenterKeys.ForeColor = keys ? Color.White : Color.Black;
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            // 验证颜色代码
            if (!string.IsNullOrEmpty(textBoxForeColor.Text))
            {
                try { var c = ColorTranslator.FromHtml(textBoxForeColor.Text); }
                catch { MessageBox.Show("字体色格式不正确，请输入如#RRGGBB或常见颜色名！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            }
            if (!string.IsNullOrEmpty(textBoxBackColor.Text))
            {
                try { var c = ColorTranslator.FromHtml(textBoxBackColor.Text); }
                catch { MessageBox.Show("背景色格式不正确，请输入如#RRGGBB或常见颜色名！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            }
            // 验证远控端口
            if (!int.TryParse(textBoxPort.Text, out int webPortVal) || webPortVal < 1 || webPortVal > 65535)
            {
                MessageBox.Show("访问端口必须为 1~65535 之间的整数！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            // 验证总时间是否为有效数字
            if (!int.TryParse(textBoxTotalTime.Text, out int totalTimeVal) || totalTimeVal < 1)
            {
                MessageBox.Show("总时间必须为大于0的整数！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            // 防止分钟模式下 totalTimeVal * 60 溢出
            if (comboBoxUnit.SelectedIndex == 0 && totalTimeVal > int.MaxValue / 60)
            {
                MessageBox.Show("分钟数值过大，请设置在 " + (int.MaxValue / 60) + " 以内！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\\801CountdownApp1");
            key.SetValue("TotalTime", textBoxTotalTime.Text);
            key.SetValue("TimeUnit", comboBoxUnit.SelectedIndex);
            key.SetValue("Opacity", trackBarOpacity.Value);
            key.SetValue("PPTAuto", checkBoxPPTAuto.Checked ? 1 : 0);
            key.SetValue("ShowSlideInfo", checkBoxShowSlideInfo.Checked ? 1 : 0);
            key.SetValue("Sound", checkBoxSound.Checked ? 1 : 0);
            key.SetValue("Flash", checkBox2.Checked ? 1 : 0);
            key.SetValue("AllowNegative", checkBoxAllowNegative.Checked ? 1 : 0);
            key.SetValue("PureSlideMode", checkBoxPureSlide.Checked ? 1 : 0);
            key.SetValue("PreAlert", checkBoxPreAlert.Checked ? 1 : 0);
            key.SetValue("PreAlertSeconds", (int)numericPreAlert.Value);
            int soundIndex = Math.Max(0, Math.Min(soundFiles.Length - 1, comboBoxSound.SelectedIndex));
            key.SetValue("SoundFile", soundFiles[soundIndex]);
            key.SetValue("WebPort", webPortVal);
            key.SetValue("WebPassword", textBoxWebPassword.Text);
            key.SetValue("PresenterMode", presenterKeysMode ? 1 : 0);
            key.SetValue("RemotePresentation", checkBoxPresentation.Checked ? 1 : 0);
            key.SetValue("RemoteDocPreview", checkBoxDocPreview.Checked ? 1 : 0);
            // 保存颜色
            if (!string.IsNullOrEmpty(textBoxForeColor.Text))
                key.SetValue("ForeColor", ColorTranslator.FromHtml(textBoxForeColor.Text).ToArgb());
            if (!string.IsNullOrEmpty(textBoxBackColor.Text))
                key.SetValue("BackColor", ColorTranslator.FromHtml(textBoxBackColor.Text).ToArgb());
            // 保存checkBox3、checkBox4
            key.SetValue("SetTime", checkBox3.Checked ? 1 : 0);
            key.SetValue("StartPauseReset", checkBox4.Checked ? 1 : 0);
            key.SetValue("RemoteWindowPosition", checkBoxWin.Checked ? 1 : 0);
            key.SetValue("RemoteAppearance", checkBoxAppearance.Checked ? 1 : 0);
            key.SetValue("RemoteFlashSound", checkBoxFlashSound.Checked ? 1 : 0);
            key.SetValue("RemoteSettings", checkBoxSettings.Checked ? 1 : 0);
            key.SetValue("ShowToolTip", checkBoxSlideToolTip.Checked ? 1 : 0);
            // 保存字体
            key.SetValue("FontSizeSlider", trackBarFontSize.Value);
            key.SetValue("Padding", trackBarPadding.Value);
            if (CurrentLabelFont != null)
            {
                key.SetValue("FontFamily", CurrentLabelFont.FontFamily.Name);
                key.SetValue("FontSize", CurrentLabelFont.Size);
                key.SetValue("FontStyle", (int)CurrentLabelFont.Style);
            }
            key.Close();
            // 立即通知Form1应用颜色
            if (!string.IsNullOrEmpty(textBoxForeColor.Text) && !string.IsNullOrEmpty(textBoxBackColor.Text))
            {
                ColorChanged?.Invoke(ColorTranslator.FromHtml(textBoxForeColor.Text), ColorTranslator.FromHtml(textBoxBackColor.Text));
            }
            Close();
        }

        private void buttonColorSelect_Click(object sender, EventArgs e)
        {
            using (var form3 = new Form3())
            {
                form3.ColorSelected += (fore, back) =>
                {
                    textBoxForeColor.Text = ColorTranslator.ToHtml(fore);
                    textBoxBackColor.Text = ColorTranslator.ToHtml(back);
                    ColorChanged?.Invoke(fore, back);
                };
                form3.ShowDialog();
            }
        }

        private void buttonStyleSelect_Click(object sender, EventArgs e)
        {
            using (var form4 = new Form4())
            {
                form4.StyleSelected += style => CountdownStyleChanged?.Invoke(style);
                form4.ShowDialog();
            }
        }

        private static Bitmap CreateQrIcon()
        {
            bool[,] pattern =
            {
                { true, true, true, false, true, true, true },
                { true, false, true, false, true, false, true },
                { true, true, true, false, true, true, true },
                { false, false, false, true, false, false, false },
                { true, true, true, false, true, false, true },
                { true, false, true, true, false, true, false },
                { true, true, true, false, true, false, true }
            };
            var bmp = new Bitmap(21, 21);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
                using (var brush = new SolidBrush(Color.Black))
                {
                    for (int y = 0; y < 7; y++)
                        for (int x = 0; x < 7; x++)
                            if (pattern[y, x]) g.FillRectangle(brush, x * 3, y * 3, 3, 3);
                }
            }
            return bmp;
        }

        private void buttonQr_Click(object sender, EventArgs e)
        {
            using (var dialog = new Form())
            {
                dialog.Text = "内网控制地址二维码";
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.MaximizeBox = false;
                dialog.MinimizeBox = false;
                dialog.ShowInTaskbar = false;
                dialog.ClientSize = new Size(320, 372);
                var qr = new PictureBox
                {
                    Image = qrImage,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Location = new Point(20, 16),
                    Size = new Size(280, 280)
                };
                var close = new Button
                {
                    Text = "关闭",
                    Size = new Size(100, 32),
                    Location = new Point(110, 308)
                };
                close.Click += (s, args) => dialog.Close();
                dialog.Controls.Add(qr);
                dialog.Controls.Add(close);
                dialog.AcceptButton = close;
                dialog.CancelButton = close;
                dialog.ShowDialog(this);
            }
        }

        private void LoadConfigFromRegistry()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\\801CountdownApp1");
            if (key != null)
            {
                object totalTime = key.GetValue("TotalTime");
                object timeUnit = key.GetValue("TimeUnit");
                object opacity = key.GetValue("Opacity");
                object pptAuto = key.GetValue("PPTAuto");
                object showSlideInfo = key.GetValue("ShowSlideInfo");
                object sound = key.GetValue("Sound");
                object foreColor = key.GetValue("ForeColor");
                object backColor = key.GetValue("BackColor");
                object flash = key.GetValue("Flash");
                object fontFamily = key.GetValue("FontFamily");
                object fontSize = key.GetValue("FontSize");
                object fontStyle = key.GetValue("FontStyle");
                object fontSizeSlider = key.GetValue("FontSizeSlider");
                if (totalTime != null) textBoxTotalTime.Text = totalTime.ToString();
                if (timeUnit != null) comboBoxUnit.SelectedIndex = (int)timeUnit;
                if (opacity != null) trackBarOpacity.Value = (int)opacity;
                if (pptAuto != null) checkBoxPPTAuto.Checked = ((int)pptAuto) == 1;
                if (showSlideInfo != null) checkBoxShowSlideInfo.Checked = ((int)showSlideInfo) == 1;
                if (sound != null) checkBoxSound.Checked = ((int)sound) == 1;
                if (foreColor != null) textBoxForeColor.Text = ColorTranslator.ToHtml(Color.FromArgb((int)foreColor));
                if (backColor != null) textBoxBackColor.Text = ColorTranslator.ToHtml(Color.FromArgb((int)backColor));
                if (flash != null) checkBox2.Checked = ((int)flash) == 1;
                if (fontFamily != null && fontSize != null && fontStyle != null)
                {
                    try
                    {
                        CurrentLabelFont = new Font(fontFamily.ToString(), float.Parse(fontSize.ToString()), (FontStyle)(int)fontStyle);
                    }
                    catch { }
                }
                if (fontSizeSlider != null) { trackBarFontSize.Value = (int)fontSizeSlider; labelFontSizeVal.Text = trackBarFontSize.Value.ToString(); }
                else if (CurrentLabelFont != null)
                {
                    int fs = (int)Math.Round(CurrentLabelFont.Size);
                    fs = Math.Max(trackBarFontSize.Minimum, Math.Min(trackBarFontSize.Maximum, fs));
                    trackBarFontSize.Value = fs;
                    labelFontSizeVal.Text = fs.ToString();
                }
                // 读取内边距
                object paddingValue = key.GetValue("Padding");
                if (paddingValue != null)
                {
                    int value = (int)paddingValue;
                    value = Math.Max(trackBarPadding.Minimum, Math.Min(trackBarPadding.Maximum, value));
                    trackBarPadding.Value = value;
                }
                labelPaddingVal.Text = trackBarPadding.Value.ToString();
                // 读取负数超时
                object allowNeg = key.GetValue("AllowNegative");
                checkBoxAllowNegative.Checked = allowNeg != null && (int)allowNeg == 1;
                // 读取纯PPT页数模式
                object pureSlide = key.GetValue("PureSlideMode");
                checkBoxPureSlide.Checked = pureSlide != null && (int)pureSlide == 1;
                // 结束前提醒
                object preAlert = key.GetValue("PreAlert");
                checkBoxPreAlert.Checked = preAlert != null && (int)preAlert == 1;
                object preAlertSec = key.GetValue("PreAlertSeconds");
                int preSec = preAlertSec != null ? (int)preAlertSec : 60;
                numericPreAlert.Value = Math.Max(numericPreAlert.Minimum, Math.Min(numericPreAlert.Maximum, preSec));
                // 音效
                object soundFile = key.GetValue("SoundFile");
                int soundIndex = Array.IndexOf(soundFiles, soundFile as string);
                comboBoxSound.SelectedIndex = soundIndex >= 0 ? soundIndex : 0;
                // 远控端口与密码
                object webPort = key.GetValue("WebPort");
                textBoxPort.Text = webPort != null ? ((int)webPort).ToString() : "9051";
                object webPassword = key.GetValue("WebPassword");
                textBoxWebPassword.Text = webPassword as string ?? "";
                object presenterMode = key.GetValue("PresenterMode");
                SetPresenterMode(presenterMode is int mode && mode == 1);
                object remotePresentation = key.GetValue("RemotePresentation");
                checkBoxPresentation.Checked = remotePresentation == null || (int)remotePresentation == 1;
                object remoteDocPreview = key.GetValue("RemoteDocPreview");
                checkBoxDocPreview.Checked = remoteDocPreview == null || (int)remoteDocPreview == 1;
                // 读取checkBox3、checkBox4
                object setTime = key.GetValue("SetTime");
                object startPauseReset = key.GetValue("StartPauseReset");
                checkBox3.Checked = (setTime == null) ? true : ((int)setTime == 1);
                checkBox4.Checked = (startPauseReset == null) ? true : ((int)startPauseReset == 1);
                object remoteWindow = key.GetValue("RemoteWindowPosition");
                object remoteAppearance = key.GetValue("RemoteAppearance");
                object remoteFlashSound = key.GetValue("RemoteFlashSound");
                object remoteSettings = key.GetValue("RemoteSettings");
                checkBoxWin.Checked = remoteWindow == null || (int)remoteWindow == 1;
                checkBoxAppearance.Checked = remoteAppearance == null || (int)remoteAppearance == 1;
                checkBoxFlashSound.Checked = remoteFlashSound == null || (int)remoteFlashSound == 1;
                checkBoxSettings.Checked = remoteSettings == null || (int)remoteSettings == 1;
                // 鼠标悬浮提示
                object showToolTip = key.GetValue("ShowToolTip");
                checkBoxSlideToolTip.Checked = showToolTip == null || (int)showToolTip == 1;
                key.Close();
            }
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            comboBoxUnit.SelectedIndex = 0;
            labelFontSizeVal.Text = trackBarFontSize.Value.ToString();
            // 页数显示勾选框
            checkBoxShowSlideInfo = new CheckBox();
            checkBoxShowSlideInfo.Text = "PPT全屏时：显示幻灯片页数";
            checkBoxShowSlideInfo.AutoSize = true;
            checkBoxShowSlideInfo.Margin = new Padding(4);
            checkBoxShowSlideInfo.Font = new Font("微软雅黑", 9F, FontStyle.Bold);
            basicLayout.Controls.Add(checkBoxShowSlideInfo, 0, 2);
            basicLayout.SetColumnSpan(checkBoxShowSlideInfo, 7);
            // 允许超时负数
            checkBoxAllowNegative = new CheckBox();
            checkBoxAllowNegative.Text = "计时结束：显示负数";
            checkBoxAllowNegative.AutoSize = true;
            checkBoxAllowNegative.Margin = new Padding(4);
            checkBoxAllowNegative.Font = new Font("微软雅黑", 9F, FontStyle.Bold);
            basicLayout.Controls.Add(checkBoxAllowNegative, 0, 3);
            basicLayout.SetColumnSpan(checkBoxAllowNegative, 7);
            // 纯PPT页数模式
            checkBoxPureSlide = new CheckBox();
            checkBoxPureSlide.Text = "仅显示文件页数";
            checkBoxPureSlide.AutoSize = true;
            checkBoxPureSlide.Margin = new Padding(4);
            checkBoxPureSlide.Font = new Font("微软雅黑", 9F, FontStyle.Bold);
            basicLayout.Controls.Add(checkBoxPureSlide, 0, 4);
            basicLayout.SetColumnSpan(checkBoxPureSlide, 7);
            // 鼠标悬浮提示
            checkBoxSlideToolTip = new CheckBox();
            checkBoxSlideToolTip.Text = "鼠标悬浮提示";
            checkBoxSlideToolTip.AutoSize = true;
            checkBoxSlideToolTip.Margin = new Padding(4);
            checkBoxSlideToolTip.Font = new Font("微软雅黑", 9F, FontStyle.Bold);
            basicLayout.Controls.Add(checkBoxSlideToolTip, 0, 5);
            basicLayout.SetColumnSpan(checkBoxSlideToolTip, 7);
            // 翻页笔模式（两段式）
            labelPresenterMode = new Label();
            labelPresenterMode.AutoSize = true;
            labelPresenterMode.Anchor = AnchorStyles.Left;
            labelPresenterMode.Margin = new Padding(4, 0, 4, 0);
            labelPresenterMode.Text = "翻页笔模式";

            buttonPresenterCom = new Button();
            buttonPresenterCom.Text = "COM";
            buttonPresenterCom.AutoSize = true;
            buttonPresenterCom.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            buttonPresenterCom.Padding = new Padding(16, 5, 16, 5);
            buttonPresenterCom.Margin = new Padding(0);
            buttonPresenterCom.FlatStyle = FlatStyle.Flat;
            buttonPresenterCom.FlatAppearance.BorderSize = 1;
            buttonPresenterCom.FlatAppearance.BorderColor = Color.Black;
            buttonPresenterCom.UseVisualStyleBackColor = false;
            buttonPresenterCom.Font = new Font("微软雅黑", 9F, FontStyle.Bold);
            buttonPresenterCom.Click += (s2, e2) => SetPresenterMode(false);

            buttonPresenterKeys = new Button();
            buttonPresenterKeys.Text = "模拟按键";
            buttonPresenterKeys.AutoSize = true;
            buttonPresenterKeys.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            buttonPresenterKeys.Padding = new Padding(16, 5, 16, 5);
            buttonPresenterKeys.Margin = new Padding(0);
            buttonPresenterKeys.FlatStyle = FlatStyle.Flat;
            buttonPresenterKeys.FlatAppearance.BorderSize = 1;
            buttonPresenterKeys.FlatAppearance.BorderColor = Color.Black;
            buttonPresenterKeys.UseVisualStyleBackColor = false;
            buttonPresenterKeys.Font = new Font("微软雅黑", 9F, FontStyle.Bold);
            buttonPresenterKeys.Click += (s2, e2) => SetPresenterMode(true);

            presenterModePanel = new FlowLayoutPanel();
            presenterModePanel.AutoSize = true;
            presenterModePanel.WrapContents = false;
            presenterModePanel.FlowDirection = FlowDirection.LeftToRight;
            presenterModePanel.Margin = new Padding(4);
            presenterModePanel.Controls.Add(buttonPresenterCom);
            presenterModePanel.Controls.Add(buttonPresenterKeys);

            remoteLayout.Controls.Add(labelPresenterMode, 0, 3);
            remoteLayout.Controls.Add(presenterModePanel, 1, 3);
            remoteLayout.SetColumnSpan(presenterModePanel, 3);
            LoadConfigFromRegistry();
            SendMessage(textBoxWebPassword.Handle, EM_SETCUEBANNER, IntPtr.Zero, "无密码");
            UpdateOpacityLabel();
            textBoxIP.Text = GetLocalIPAddress();
            buttonColorSelect.Click += buttonColorSelect_Click;
            buttonStyleSelect.Click += buttonStyleSelect_Click;
            // 显示Web控制地址和二维码
            string ip = GetLocalIPAddress();
            int configuredPort = 9051;
            if (!int.TryParse(textBoxPort.Text, out configuredPort) || configuredPort < 1 || configuredPort > 65535)
                configuredPort = 9051;
            string url = $"http://{ip}:{configuredPort}/";
            linkLabel1.Text = url;
            linkLabel1.Links.Clear();
            linkLabel1.Links.Add(0, url.Length, url);
            // 生成二维码
            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q))
            using (QRCode qrCode = new QRCode(qrCodeData))
            {
                qrImage = qrCode.GetGraphic(8);
            }
            buttonQr.Image = CreateQrIcon();
            // 侧边栏导航
            labelAboutVersionVal.Text = GetBuildVersion();
            navButtons = new[] { navBasic, navAlert, navAppearance, navRemote, navAbout };
            sections = new Control[] { groupBoxBasic, groupBox2, groupBoxAppearance, groupBox1, groupBoxAbout };
            for (int i = 0; i < navButtons.Length; i++)
            {
                int index = i;
                navButtons[i].Click += (s, args) => ShowSection(index);
            }
            // 侧边栏宽度按导航文字实际占用兜底，避免 DPI 缩放后文字被裁
            int navNeed = 0;
            foreach (var b in navButtons)
                navNeed = Math.Max(navNeed, TextRenderer.MeasureText(b.Text, b.Font).Width);
            navNeed += sidebarPanel.Padding.Horizontal + 36;
            if (rootLayout.ColumnStyles[0].Width < navNeed)
            {
                int delta = navNeed - (int)rootLayout.ColumnStyles[0].Width;
                rootLayout.ColumnStyles[0].Width = navNeed;
                this.ClientSize = new Size(this.ClientSize.Width + delta, this.ClientSize.Height);
            }
            ShowSection(0);
            // 窗口不超出工作区，内容装不下时由内容区滚动
            Rectangle work = Screen.FromControl(this).WorkingArea;
            int maxWidth = Math.Max(work.Width - (this.Width - this.ClientSize.Width) - 10, 400);
            int maxHeight = Math.Max(work.Height - (this.Height - this.ClientSize.Height) - 10, 300);
            this.ClientSize = new Size(
                Math.Min(this.ClientSize.Width, maxWidth),
                Math.Min(this.ClientSize.Height, maxHeight));
            this.CenterToScreen();
        }

        private static string GetBuildVersion()
        {
            try
            {
                string path = Path.Combine(Application.StartupPath, "version.txt");
                if (File.Exists(path))
                {
                    string text = File.ReadAllText(path).Trim();
                    if (text.Length > 0) return text;
                }
            }
            catch { }
            return Application.ProductVersion;
        }

        private void ShowSection(int index)
        {
            for (int i = 0; i < sections.Length; i++)
            {
                bool active = i == index;
                sections[i].Visible = active;
                navButtons[i].Selected = active;
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Link.LinkData.ToString());
        }

        private void linkLabelOfficial_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://mdhyy.cn/tools/page/801CountdownApp1/");
        }

        private void linkLabelHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://help.web.mdhyy.cn/?doc=801countdown");
        }

        private void linkLabelAboutSite_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://mdhyy.cn");
        }
    }
}
