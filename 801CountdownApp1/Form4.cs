using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Microsoft.Win32;

namespace _801CountdownApp1
{
    public partial class Form4 : Form
    {
        public event Action<CountdownStyleKind> StyleSelected;

        private CountdownStyleKind currentStyle;
        private Color foreColor = Color.White;
        private Color backColor = Color.Red;

        public Form4()
        {
            InitializeComponent();
            LoadCurrentFromRegistry();
            for (int i = 0; i < stylePanels.Length; i++)
            {
                int idx = i;
                stylePanels[i].Paint += (s, e) => DrawPreview(e.Graphics, stylePanels[idx].ClientRectangle, CountdownStyles.All[idx]);
                stylePanels[i].Click += (s, e) => SelectStyle(CountdownStyles.All[idx]);
            }
        }

        private void LoadCurrentFromRegistry()
        {
            var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\\801CountdownApp1");
            if (key != null)
            {
                object fore = key.GetValue("ForeColor");
                object back = key.GetValue("BackColor");
                object style = key.GetValue("Style");
                if (fore != null) foreColor = Color.FromArgb((int)fore);
                if (back != null) backColor = Color.FromArgb((int)back);
                if (style != null)
                {
                    int value = (int)style;
                    if (value >= 0 && value <= (int)CountdownStyleKind.Transparent)
                        currentStyle = (CountdownStyleKind)value;
                }
                key.Close();
            }
        }

        private void SelectStyle(CountdownStyleKind kind)
        {
            currentStyle = kind;
            var key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\\801CountdownApp1");
            key.SetValue("Style", (int)kind);
            key.Close();
            StyleSelected?.Invoke(kind);
            Close();
        }

        private void DrawPreview(Graphics g, Rectangle rect, CountdownStyleKind kind)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.White);

            var preview = new Rectangle(3, 3, rect.Width - 6, rect.Height - 26);
            if (kind == CountdownStyleKind.Transparent)
                DrawCheckerboard(g, preview);
            CountdownStyles.DrawBackground(g, preview, kind, foreColor, backColor);

            using (var titleFont = new Font("微软雅黑", 12f, FontStyle.Bold))
            using (var titleBrush = new SolidBrush(foreColor))
            using (var center = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            })
            {
                g.DrawString("12:34", titleFont, titleBrush, preview, center);

                bool selected = kind == currentStyle;
                using (var nameFont = new Font("微软雅黑", 9f, selected ? FontStyle.Bold : FontStyle.Regular))
                using (var nameBrush = new SolidBrush(Color.FromArgb(45, 45, 45)))
                {
                    var nameRect = new RectangleF(0, rect.Height - 21, rect.Width, 20);
                    g.DrawString(CountdownStyles.GetName(kind), nameFont, nameBrush, nameRect, center);
                }
            }

            if (kind == currentStyle)
            {
                using (var pen = new Pen(Color.FromArgb(0, 120, 215), 2f))
                    g.DrawRectangle(pen, 1f, 1f, rect.Width - 2.5f, rect.Height - 2.5f);
            }
        }

        private static void DrawCheckerboard(Graphics g, Rectangle rect)
        {
            const int cell = 8;
            using (var light = new SolidBrush(Color.FromArgb(0xF7, 0xF7, 0xF7)))
            using (var dark = new SolidBrush(Color.FromArgb(0xE6, 0xE6, 0xE6)))
            {
                g.FillRectangle(light, rect);
                for (int y = 0; y * cell < rect.Height; y++)
                {
                    for (int x = 0; x * cell < rect.Width; x++)
                    {
                        if ((x + y) % 2 == 0) continue;
                        var cellRect = new Rectangle(
                            rect.X + x * cell, rect.Y + y * cell,
                            Math.Min(cell, rect.Right - (rect.X + x * cell)),
                            Math.Min(cell, rect.Bottom - (rect.Y + y * cell)));
                        g.FillRectangle(dark, cellRect);
                    }
                }
            }
        }
    }
}
