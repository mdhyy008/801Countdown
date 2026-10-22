using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace _801CountdownApp1
{
    public enum CountdownStyleKind
    {
        Default = 0,
        Rounded = 1,
        Pill = 2,
        Neon = 3,
        Gradient = 4,
        Outline = 5,
        Transparent = 6,
    }

    public static class CountdownStyles
    {
        public static readonly CountdownStyleKind[] All =
        {
            CountdownStyleKind.Default,
            CountdownStyleKind.Rounded,
            CountdownStyleKind.Pill,
            CountdownStyleKind.Neon,
            CountdownStyleKind.Gradient,
            CountdownStyleKind.Outline,
            CountdownStyleKind.Transparent,
        };

        public static string GetName(CountdownStyleKind kind)
        {
            switch (kind)
            {
                case CountdownStyleKind.Rounded: return "圆角";
                case CountdownStyleKind.Pill: return "胶囊";
                case CountdownStyleKind.Neon: return "霓虹";
                case CountdownStyleKind.Gradient: return "渐变";
                case CountdownStyleKind.Outline: return "描边";
                case CountdownStyleKind.Transparent: return "透明";
                default: return "默认";
            }
        }

        public static float GetRadius(CountdownStyleKind kind, Size size)
        {
            switch (kind)
            {
                case CountdownStyleKind.Rounded:
                    return Math.Min(16f, Math.Min(size.Width, size.Height) / 3f);
                case CountdownStyleKind.Pill:
                    return size.Height / 2f;
                case CountdownStyleKind.Neon:
                    return Math.Min(14f, size.Height / 3f);
                case CountdownStyleKind.Gradient:
                    return Math.Min(12f, size.Height / 3f);
                default:
                    return 0f;
            }
        }

        public static GraphicsPath CreateRoundedPath(RectangleF rect, float radius)
        {
            var path = new GraphicsPath();
            if (radius <= 0.5f)
            {
                path.AddRectangle(rect);
                return path;
            }
            float d = radius * 2f;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        // 主窗口背景。Default 走原有平铺，其余按样式绘制。
        public static void DrawBackground(Graphics g, Rectangle bounds, CountdownStyleKind kind, Color fore, Color back)
        {
            if (kind == CountdownStyleKind.Transparent)
                return;

            if (kind == CountdownStyleKind.Default)
            {
                using (var brush = new SolidBrush(back))
                    g.FillRectangle(brush, bounds);
                return;
            }

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            var rect = new RectangleF(bounds.X, bounds.Y, bounds.Width, bounds.Height);
            float fullRadius = GetRadius(kind, bounds.Size);
            var inner = RectangleF.Inflate(rect, -1.2f, -1.2f);
            float radius = Math.Max(fullRadius - 1.2f, 0.5f);

            switch (kind)
            {
                case CountdownStyleKind.Rounded:
                {
                    using (var path = CreateRoundedPath(rect, fullRadius))
                    using (var fill = new SolidBrush(back))
                        g.FillPath(fill, path);
                    break;
                }

                case CountdownStyleKind.Pill:
                {
                    using (var path = CreateRoundedPath(rect, bounds.Height / 2f))
                    using (var fill = new SolidBrush(back))
                        g.FillPath(fill, path);
                    break;
                }

                case CountdownStyleKind.Neon:
                {
                    Color dark = Mix(back, Color.Black, 0.62f);
                    // 外圈辉光：由内向外逐层变淡
                    for (int layer = 4; layer >= 1; layer--)
                    {
                        float offset = 1.5f + layer * 1.6f;
                        var glowRect = RectangleF.Inflate(rect, -offset, -offset);
                        float glowRadius = Math.Max(fullRadius - offset, 0.5f);
                        using (var path = CreateRoundedPath(glowRect, glowRadius))
                        using (var pen = new Pen(Color.FromArgb(16 * layer, fore), 1f + layer * 1.4f))
                        {
                            g.DrawPath(pen, path);
                        }
                    }
                    using (var path = CreateRoundedPath(inner, radius))
                    {
                        using (var fill = new SolidBrush(dark))
                            g.FillPath(fill, path);
                        // 扫描线
                        using (Region clip = g.Clip)
                        {
                            g.SetClip(path, CombineMode.Intersect);
                            using (var scan = new Pen(Color.FromArgb(20, Color.Black), 1f))
                            {
                                for (float y = rect.Y + 2f; y < rect.Bottom; y += 4f)
                                    g.DrawLine(scan, rect.Left, y, rect.Right, y);
                            }
                            g.Clip = clip;
                        }
                        using (var pen = new Pen(fore, 1.6f))
                            g.DrawPath(pen, path);
                    }
                    break;
                }

                case CountdownStyleKind.Gradient:
                {
                    using (var path = CreateRoundedPath(rect, fullRadius))
                    using (var brush = new LinearGradientBrush(
                        new RectangleF(rect.X, rect.Y, rect.Width, rect.Height),
                        Shade(back, 38), Shade(back, -42), 45f))
                    {
                        g.FillPath(brush, path);
                    }
                    break;
                }

                case CountdownStyleKind.Outline:
                {
                    using (var fill = new SolidBrush(back))
                        g.FillRectangle(fill, rect);
                    using (var outer = new Pen(fore, 3f))
                        g.DrawRectangle(outer, rect.X + 2f, rect.Y + 2f, rect.Width - 5f, rect.Height - 5f);
                    using (var innerPen = new Pen(Color.FromArgb(150, fore), 1f))
                        g.DrawRectangle(innerPen, rect.X + 8f, rect.Y + 8f, rect.Width - 17f, rect.Height - 17f);
                    break;
                }
            }
        }

        // 霓虹样式的文字辉光：把文字墨迹位图按不同透明度、不同方向叠几层
        public static void DrawTextGlow(Graphics g, Bitmap ink, int x, int y)
        {
            if (ink == null) return;

            g.SmoothingMode = SmoothingMode.AntiAlias;

            float[][] directions =
            {
                new[] { -1f, 0f }, new[] { 1f, 0f }, new[] { 0f, -1f }, new[] { 0f, 1f },
                new[] { -0.7f, -0.7f }, new[] { 0.7f, -0.7f }, new[] { -0.7f, 0.7f }, new[] { 0.7f, 0.7f },
            };
            float[] steps = { 3f, 2f, 1f };

            using (var attributes = new System.Drawing.Imaging.ImageAttributes())
            {
                for (int layer = 0; layer < steps.Length; layer++)
                {
                    int alpha = 42 + layer * 26;
                    var matrix = new System.Drawing.Imaging.ColorMatrix { Matrix33 = alpha / 255f };
                    attributes.SetColorMatrix(matrix,
                        System.Drawing.Imaging.ColorMatrixFlag.Default,
                        System.Drawing.Imaging.ColorAdjustType.Bitmap);
                    foreach (var dir in directions)
                    {
                        float dx = dir[0] * steps[layer];
                        float dy = dir[1] * steps[layer];
                        var dest = new Rectangle(
                            (int)Math.Round(x + dx), (int)Math.Round(y + dy),
                            ink.Width, ink.Height);
                        g.DrawImage(ink, dest,
                            0, 0, ink.Width, ink.Height,
                            GraphicsUnit.Pixel, attributes);
                    }
                }
            }
        }

        public static Color Shade(Color c, int delta)
        {
            return Color.FromArgb(c.A,
                Math.Max(0, Math.Min(255, c.R + delta)),
                Math.Max(0, Math.Min(255, c.G + delta)),
                Math.Max(0, Math.Min(255, c.B + delta)));
        }

        public static Color Mix(Color a, Color b, float t)
        {
            t = Math.Max(0f, Math.Min(1f, t));
            return Color.FromArgb(
                (int)(a.R + (b.R - a.R) * t),
                (int)(a.G + (b.G - a.G) * t),
                (int)(a.B + (b.B - a.B) * t));
        }
    }
}
