using System;
using System.Drawing;
using System.Windows.Forms;

namespace _801CountdownApp1
{
    public partial class Form3 : Form
    {
        public event Action<Color, Color> ColorSelected;
        private readonly (Color fore, Color back)[] colorPairs = new (Color, Color)[]
        {
            // 高对比度经典
            (Color.White, Color.Black),
            (Color.Black, Color.White),
            // Material Design 深色底 + 白字
            (Color.White, Color.FromArgb(0xF4, 0x43, 0x36)),      // Red
            (Color.White, Color.FromArgb(0xE9, 0x1E, 0x63)),      // Pink
            (Color.White, Color.FromArgb(0x9C, 0x27, 0xB0)),      // Purple
            (Color.White, Color.FromArgb(0x67, 0x3A, 0xB7)),      // Deep Purple
            (Color.White, Color.FromArgb(0x3F, 0x51, 0xB5)),      // Indigo
            (Color.White, Color.FromArgb(0x21, 0x96, 0xF3)),      // Blue
            (Color.White, Color.FromArgb(0x03, 0xA9, 0xF4)),      // Light Blue
            (Color.White, Color.FromArgb(0x00, 0xBC, 0xD4)),      // Cyan
            (Color.White, Color.FromArgb(0x00, 0x96, 0x88)),      // Teal
            (Color.White, Color.FromArgb(0x4C, 0xAF, 0x50)),      // Green
            (Color.White, Color.FromArgb(0xFF, 0x57, 0x22)),      // Deep Orange
            (Color.White, Color.FromArgb(0x79, 0x54, 0x8B)),      // Brown
            (Color.White, Color.FromArgb(0x60, 0x7D, 0x8B)),      // Blue Grey
            // Material Design 深色调 + 白字
            (Color.White, Color.FromArgb(0x37, 0x47, 0x4F)),      // Dark Blue Grey
            (Color.White, Color.FromArgb(0x1B, 0x5E, 0x20)),      // Dark Green
            (Color.White, Color.FromArgb(0xB7, 0x1C, 0x1C)),      // Dark Red
            (Color.White, Color.FromArgb(0x1A, 0x23, 0x7E)),      // Dark Indigo
            (Color.White, Color.FromArgb(0x00, 0x4D, 0x40)),      // Dark Teal
            (Color.White, Color.FromArgb(0x4A, 0x14, 0x8C)),      // Dark Purple
            (Color.White, Color.FromArgb(0xE6, 0x51, 0x00)),      // Dark Orange
            // Material Design 浅色底 + 深色字
            (Color.FromArgb(0x21, 0x21, 0x21), Color.FromArgb(0xFF, 0xC1, 0x07)),  // Amber
            (Color.FromArgb(0x21, 0x21, 0x21), Color.FromArgb(0xCD, 0xDC, 0x39)),  // Lime
            (Color.FromArgb(0x2E, 0x7D, 0x32), Color.FromArgb(0xFF, 0xF9, 0xC4)),  // Light Yellow
            (Color.FromArgb(0x21, 0x21, 0x21), Color.FromArgb(0x80, 0xCB, 0xC4)),  // Teal Light
            (Color.FromArgb(0x21, 0x21, 0x21), Color.FromArgb(0x90, 0xCA, 0xF9)),  // Blue Light
            (Color.FromArgb(0x21, 0x21, 0x21), Color.FromArgb(0xF4, 0x8F, 0xB1)),  // Pink Light
            (Color.FromArgb(0x21, 0x21, 0x21), Color.FromArgb(0xCE, 0x93, 0xD8)),  // Purple Light
            (Color.FromArgb(0x21, 0x21, 0x21), Color.FromArgb(0xFF, 0xCC, 0x80)),  // Orange Light
        };
        public Form3()
        {
            InitializeComponent();
            for (int i = 0; i < colorPanels.Length; i++)
            {
                int idx = i;
                colorPanels[i].Paint += (s, e) => DrawColorPanel(e.Graphics, colorPairs[idx]);
                colorPanels[i].Click += (s, e) =>
                {
                    ColorSelected?.Invoke(colorPairs[idx].fore, colorPairs[idx].back);
                    SaveColorToRegistry(colorPairs[idx].fore, colorPairs[idx].back);
                    this.Close();
                };
            }
        }
        private void DrawColorPanel(Graphics g, (Color fore, Color back) pair)
        {
            g.FillRectangle(new SolidBrush(pair.fore), new Rectangle(0, 0, 60, 30));
            g.FillRectangle(new SolidBrush(pair.back), new Rectangle(0, 30, 60, 30));
        }
        private void SaveColorToRegistry(Color fore, Color back)
        {
            var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"SOFTWARE\\801CountdownApp1");
            key.SetValue("ForeColor", fore.ToArgb());
            key.SetValue("BackColor", back.ToArgb());
            key.Close();
        }
    }
}
