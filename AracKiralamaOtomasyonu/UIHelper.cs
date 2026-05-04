using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AracKiralamaOtomasyonu
{
    public static class UIHelper
    {
        // Renk Paleti (Luxury Dark/Glass Theme)
        public static Color BackgroundColor = Color.FromArgb(15, 23, 42); // Navy/Slate
        public static Color SidebarColor = Color.FromArgb(180, 20, 30, 48); // Semi-transparent for Glass effect
        public static Color AccentColor = Color.FromArgb(56, 189, 248); // Sky Blue
        public static Color SuccessColor = Color.FromArgb(16, 185, 129); // Emerald
        public static Color DangerColor = Color.FromArgb(220, 38, 38); // Red
        public static Color CardColor = Color.FromArgb(160, 15, 23, 42); // Glass Card Color
        public static Color TextPrimary = Color.White; // Switched to white for dark theme
        public static Color TextSecondary = Color.FromArgb(148, 163, 184); // Light grey/blue

        private static Image _cachedBg;

        public static void ApplyModernBackground(Form form)
        {
            try
            {
                if (_cachedBg == null)
                {
                    string bgPath = System.IO.Path.Combine(Application.StartupPath, "Assets", "bg_luxury.png");
                    if (System.IO.File.Exists(bgPath))
                    {
                        _cachedBg = Image.FromFile(bgPath);
                    }
                }
                
                if (_cachedBg != null)
                {
                    form.BackgroundImage = _cachedBg;
                    form.BackgroundImageLayout = ImageLayout.Stretch;
                }
            }
            catch { }
            form.BackColor = BackgroundColor; 
        }

        public static void AnimateControlLift(Control control, bool entering)
        {
            if (entering)
            {
                control.Top -= 5;
                control.Height += 5;
                control.BackColor = Color.FromArgb(180, 30, 41, 59);
            }
            else
            {
                control.Top += 5;
                control.Height -= 5;
                control.BackColor = CardColor;
            }
        }

        public static void SetDoubleBuffered(Control control)
        {
            if (SystemInformation.TerminalServerSession) return;
            System.Reflection.PropertyInfo propertyInfo = typeof(Control).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (propertyInfo != null) propertyInfo.SetValue(control, true, null);
        }

        public static void StyleDataGridView(DataGridView dgv)
        {
            dgv.BackgroundColor = Color.FromArgb(15, 23, 42); // Solid background, DGV doesn't support transparent BackgroundColor
            dgv.BorderStyle = BorderStyle.None;
            dgv.EnableHeadersVisualStyles = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.GridColor = Color.FromArgb(40, 50, 70); // Solid faint divider
            dgv.RowTemplate.Height = 40;

            // Header Styling
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 41, 59); // Solid header
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgv.ColumnHeadersHeight = 45;

            // Row Styling
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(56, 189, 248); // Solid accent
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 10);
            dgv.DefaultCellStyle.BackColor = Color.FromArgb(20, 30, 48); // Solid dark
            dgv.DefaultCellStyle.ForeColor = Color.White;
            dgv.DefaultCellStyle.Padding = new Padding(5, 0, 5, 0);

            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(25, 35, 53); // Solid alternating

            // Row Header Styling (The leftmost column with the arrow)
            dgv.RowHeadersDefaultCellStyle.BackColor = Color.FromArgb(20, 30, 48); // Matching row color
            dgv.RowHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.RowHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(56, 189, 248); // Accent
            dgv.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;

            // Top Left Corner Styling
            dgv.TopLeftHeaderCell.Style.BackColor = Color.FromArgb(30, 41, 59); // Matching header color
            dgv.TopLeftHeaderCell.Style.SelectionBackColor = Color.FromArgb(30, 41, 59);
        }

        public static void SetRoundedRegion(Control control, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddArc(0, 0, radius, radius, 180, 90);
            path.AddArc(control.Width - radius, 0, radius, radius, 270, 90);
            path.AddArc(control.Width - radius, control.Height - radius, radius, radius, 0, 90);
            path.AddArc(0, control.Height - radius, radius, radius, 90, 90);
            path.CloseAllFigures();
            control.Region = new Region(path);
        }

        public static void ApplyShadow(Panel panel)
        {
            // Panel gölge simülasyonu için border ekleyebiliriz
            panel.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(Color.FromArgb(226, 232, 240), 1))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 1, panel.Height - 1);
                }
            };
        }

        public static TextBox CreateSearchBox(string placeholder, Control parent, Point loc)
        {
            Panel p = new Panel() { Location = loc, Size = new Size(250, 40), BackColor = Color.FromArgb(150, 15, 23, 42), Padding = new Padding(10, 5, 10, 5) };
            TextBox txt = new TextBox() { Text = placeholder, ForeColor = TextSecondary, BackColor = Color.FromArgb(15, 23, 42), BorderStyle = BorderStyle.None, Width = 230, Font = new Font("Segoe UI", 10), Dock = DockStyle.Fill };
            
            txt.Enter += (s, e) => { if (txt.Text == placeholder) { txt.Text = ""; txt.ForeColor = Color.White; } };
            txt.Leave += (s, e) => { if (string.IsNullOrWhiteSpace(txt.Text)) { txt.Text = placeholder; txt.ForeColor = TextSecondary; } };
            
            p.Controls.Add(txt);
            ApplyShadow(p);
            parent.Controls.Add(p);
            return txt;
        }

        public static void StyleModernInput(Control ctrl)
        {
            ctrl.BackColor = Color.FromArgb(30, 41, 59); // Slate-800
            ctrl.ForeColor = Color.White;
            ctrl.Font = new Font("Segoe UI", 10);
            
            if (ctrl is TextBox txt)
            {
                txt.BorderStyle = BorderStyle.FixedSingle;
            }
            else if (ctrl is ComboBox cmb)
            {
                cmb.FlatStyle = FlatStyle.Flat;
            }
            else if (ctrl is DateTimePicker dtp)
            {
                // DateTimePicker colors are tricky in WinForms, but we can set Font and BackColor
                dtp.CalendarMonthBackground = Color.FromArgb(30, 41, 59);
                dtp.CalendarTitleBackColor = AccentColor;
                dtp.CalendarTitleForeColor = Color.White;
                dtp.CalendarForeColor = Color.White;
            }
        }
    }

    public class ModernCarCard : Control
    {
        public Image CarImage { get; set; }
        public string Title { get; set; }
        public string Price { get; set; }
        public string Stats { get; set; }
        public bool IsSelected { get; set; }
        public Action CardClick { get; set; }

        private float _animationValue = 0;

        public ModernCarCard()
        {
            this.Size = new Size(250, 360);
            this.Cursor = Cursors.Hand;
            UIHelper.SetDoubleBuffered(this);
            this.Margin = new Padding(12);
        }

        protected override void OnMouseEnter(EventArgs e) { _animationValue = 1f; this.Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _animationValue = 0f; this.Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { CardClick?.Invoke(); base.OnMouseDown(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            float lift = _animationValue * 8;
            Rectangle rect = new Rectangle(5, (int)(10 - lift), this.Width - 11, this.Height - 15);

            // Shadow simulation
            using (SolidBrush shadowBrush = new SolidBrush(Color.FromArgb((int)(100 * _animationValue), 0, 0, 0)))
            {
                g.FillRectangle(shadowBrush, rect.X + 2, rect.Y + 4, rect.Width, rect.Height);
            }

            // Card Body - Glassmorphism style
            using (LinearGradientBrush b = new LinearGradientBrush(rect, IsSelected ? Color.FromArgb(200, 30, 50, 80) : Color.FromArgb(160, 15, 23, 42), IsSelected ? Color.FromArgb(180, 20, 40, 70) : Color.FromArgb(140, 10, 15, 30), 45f))
            {
                g.FillPath(b, GetRoundedPath(rect, 15));
            }
            
            using (Pen p = new Pen(IsSelected ? UIHelper.AccentColor : Color.FromArgb(100, 255, 255, 255), IsSelected ? 2 : 1))
            {
                g.DrawPath(p, GetRoundedPath(rect, 15));
            }

            // Image
            if (CarImage != null)
            {
                Rectangle imgRect = new Rectangle(rect.X + 15, rect.Y + 15, rect.Width - 30, 160);
                g.DrawImage(CarImage, imgRect);
            }

            // Texts
            using (Font fTitle = new Font("Segoe UI", 12, FontStyle.Bold))
            using (Font fPrice = new Font("Segoe UI", 12, FontStyle.Bold))
            using (Font fStats = new Font("Segoe UI", 9))
            {
                StringFormat sf = new StringFormat() { Alignment = StringAlignment.Center };
                
                g.DrawString(Title ?? "Premium Car", fTitle, new SolidBrush(UIHelper.TextPrimary), new Rectangle(rect.X, rect.Y + 185, rect.Width, 30), sf);
                
                // Content Stats
                g.DrawString(Stats ?? "", fStats, new SolidBrush(UIHelper.TextSecondary), new Rectangle(rect.X + 20, rect.Y + 225, rect.Width - 40, 60), sf);

                // Bottom Price Bar
                Rectangle priceRect = new Rectangle(rect.X, rect.Y + rect.Height - 50, rect.Width, 50);
                GraphicsPath priceBarPath = GetRoundedPath(priceRect, 0); // Straight or slight round bottom
                // Just use a simple rect for the bar at bottom
                using (SolidBrush b = new SolidBrush(IsSelected ? UIHelper.AccentColor : Color.FromArgb(30, 41, 59)))
                {
                    // Draw only bottom part rounded or just rectangle
                    g.FillRectangle(b, new Rectangle(rect.X + 1, rect.Y + rect.Height - 50, rect.Width - 1, 49));
                }
                g.DrawString(Price ?? "0 TL", fPrice, Brushes.White, new Rectangle(rect.X, rect.Y + rect.Height - 50, rect.Width, 50), new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
            }
        }

        private GraphicsPath GetRoundedPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            if (radius <= 0) { path.AddRectangle(rect); return path; }
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    // Yırtılmaları ve kasmaları önleyen donanım hızlandırmalı panel
    public class ModernFlowLayoutPanel : FlowLayoutPanel
    {
        public ModernFlowLayoutPanel()
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000; // WS_EX_COMPOSITED (Tüm alt kontrolleri ve saydamlığı donanım seviyesinde double-buffer yapar)
                return cp;
            }
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            // WM_VSCROLL (0x115), WM_HSCROLL (0x114), WM_MOUSEWHEEL (0x20A)
            if (m.Msg == 0x115 || m.Msg == 0x114 || m.Msg == 0x20A)
            {
                this.Invalidate(true); // Kaydırma anında eski piksellerin ekranda kalmasını zorla engeller
            }
        }
    }
}
