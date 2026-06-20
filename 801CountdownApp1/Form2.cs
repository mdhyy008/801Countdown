using System;
using System.Drawing;
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
        public event Action<Color, Color> ColorChanged;
        public Font CurrentLabelFont { get; set; }
        private CheckBox checkBoxShowSlideInfo;
        private CheckBox checkBoxAllowNegative;
        private CheckBox checkBoxPureSlide;

        public Form2()
        {
            InitializeComponent();
            buttonFont.Click += ButtonFont_Click;
            trackBarOpacity.Scroll += TrackBarOpacity_Scroll;
            trackBarFontSize.Scroll += TrackBarFontSize_Scroll;
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
            OpacityChanged?.Invoke(this, trackBarOpacity.Value);
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
            // 保存颜色
            if (!string.IsNullOrEmpty(textBoxForeColor.Text))
                key.SetValue("ForeColor", ColorTranslator.FromHtml(textBoxForeColor.Text).ToArgb());
            if (!string.IsNullOrEmpty(textBoxBackColor.Text))
                key.SetValue("BackColor", ColorTranslator.FromHtml(textBoxBackColor.Text).ToArgb());
            // 保存checkBox3、checkBox4
            key.SetValue("SetTime", checkBox3.Checked ? 1 : 0);
            key.SetValue("StartPauseReset", checkBox4.Checked ? 1 : 0);
            // 保存字体
            key.SetValue("FontSizeSlider", trackBarFontSize.Value);
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
                // 读取负数超时
                object allowNeg = key.GetValue("AllowNegative");
                checkBoxAllowNegative.Checked = allowNeg != null && (int)allowNeg == 1;
                // 读取纯PPT页数模式
                object pureSlide = key.GetValue("PureSlideMode");
                checkBoxPureSlide.Checked = pureSlide != null && (int)pureSlide == 1;
                // 读取checkBox3、checkBox4
                object setTime = key.GetValue("SetTime");
                object startPauseReset = key.GetValue("StartPauseReset");
                checkBox3.Checked = (setTime == null) ? true : ((int)setTime == 1);
                checkBox4.Checked = (startPauseReset == null) ? true : ((int)startPauseReset == 1);
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
            basicLayout.Controls.Add(checkBoxShowSlideInfo, 0, 3);
            basicLayout.SetColumnSpan(checkBoxShowSlideInfo, 7);
            // 允许超时负数
            checkBoxAllowNegative = new CheckBox();
            checkBoxAllowNegative.Text = "允许超时继续倒计为负数";
            checkBoxAllowNegative.AutoSize = true;
            checkBoxAllowNegative.Margin = new Padding(4);
            checkBoxAllowNegative.Font = new Font("微软雅黑", 9F, FontStyle.Bold);
            basicLayout.Controls.Add(checkBoxAllowNegative, 0, 4);
            basicLayout.SetColumnSpan(checkBoxAllowNegative, 7);
            // 纯PPT页数模式
            checkBoxPureSlide = new CheckBox();
            checkBoxPureSlide.Text = "纯PPT页数显示模式（隐藏时间，仅显示页数）";
            checkBoxPureSlide.AutoSize = true;
            checkBoxPureSlide.Margin = new Padding(4);
            checkBoxPureSlide.Font = new Font("微软雅黑", 9F, FontStyle.Bold);
            basicLayout.Controls.Add(checkBoxPureSlide, 0, 5);
            basicLayout.SetColumnSpan(checkBoxPureSlide, 7);
            LoadConfigFromRegistry();
            textBoxIP.Text = GetLocalIPAddress();
            buttonColorSelect.Click += buttonColorSelect_Click;
            // 显示Web控制地址和二维码
            string ip = GetLocalIPAddress();
            string url = $"http://{ip}:9051/";
            linkLabel1.Text = url;
            linkLabel1.Links.Clear();
            linkLabel1.Links.Add(0, url.Length, url);
            // 生成二维码
            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q))
            using (QRCode qrCode = new QRCode(qrCodeData))
            {
                pictureBox1.Image = qrCode.GetGraphic(8);
            }
            // 自动适配窗口高度
            this.PerformLayout();
            this.ClientSize = new Size(640, buttonPanel.Bottom + mainLayout.Padding.Bottom + 30);
            this.CenterToScreen();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Link.LinkData.ToString());
        }

        private void linkLabelOfficial_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://mdhyy.cn/tools/page/801CountdownApp1/");
        }
    }
}
