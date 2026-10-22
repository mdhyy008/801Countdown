using System;
using System.Drawing;
using System.Windows.Forms;

namespace _801CountdownApp1
{
    public class SidebarNavButton : Button
    {
        private static readonly Color Accent = Color.FromArgb(0, 102, 204);
        private static readonly Color IdleFore = Color.FromArgb(55, 58, 64);
        private static readonly Color HoverBack = Color.FromArgb(232, 238, 247);
        private static readonly Color PressBack = Color.FromArgb(219, 229, 243);

        private bool _hover;
        private bool _pressed;
        private bool _selected;
        private Font _selectedFont;

        public SidebarNavButton()
        {
            SetStyle(ControlStyles.UserPaint
                | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.ResizeRedraw, true);
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            TextAlign = ContentAlignment.MiddleLeft;
            Cursor = Cursors.Hand;
            Font = new Font("微软雅黑", 10F, FontStyle.Regular);
            Padding = new Padding(14, 0, 8, 0);
        }

        public bool Selected
        {
            get { return _selected; }
            set
            {
                if (_selected == value) return;
                _selected = value;
                Invalidate();
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            _hover = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _hover = false;
            _pressed = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            _pressed = true;
            Invalidate();
            base.OnMouseDown(mevent);
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            _pressed = false;
            Invalidate();
            base.OnMouseUp(mevent);
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            Graphics g = pevent.Graphics;
            g.Clear(Parent != null ? Parent.BackColor : BackColor);

            Color back = _selected ? Accent
                       : _pressed ? PressBack
                       : _hover ? HoverBack
                       : Color.Empty;
            if (back != Color.Empty)
                using (SolidBrush brush = new SolidBrush(back))
                    g.FillRectangle(brush, ClientRectangle);

            Color fore = _selected ? Color.White : (_hover || _pressed ? Accent : IdleFore);
            Rectangle textRect = new Rectangle(Padding.Left, 0, Width - Padding.Horizontal, Height);
            TextRenderer.DrawText(g, Text, _selected ? SelectedFont : Font, textRect, fore,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter
                | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        }

        private Font SelectedFont
        {
            get
            {
                if (_selectedFont == null
                    || _selectedFont.FontFamily != Font.FontFamily
                    || _selectedFont.Size != Font.Size)
                {
                    if (_selectedFont != null) _selectedFont.Dispose();
                    _selectedFont = new Font(Font, FontStyle.Bold);
                }
                return _selectedFont;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _selectedFont != null)
            {
                _selectedFont.Dispose();
                _selectedFont = null;
            }
            base.Dispose(disposing);
        }
    }
}
