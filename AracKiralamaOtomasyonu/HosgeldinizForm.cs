using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace AracKiralamaOtomasyonu
{
    public class HosgeldinizForm : Form
    {
        private Button btnMusteriPaneli;
        private Button btnYoneticiPaneli;
        private Label lblArtvin;
        private Label lblBaslik;
        private Label lblAltBaslik;
        private Panel pnlContent;

        public HosgeldinizForm()
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer | ControlStyles.OptimizedDoubleBuffer, true);
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Text = "ARTVİN ARAÇ KİRALAMA - Hoşgeldiniz";
            this.Size = new Size(1000, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // Premium Background Logic
            UIHelper.ApplyModernBackground(this);

            // Glassmorphism Centered Panel
            pnlContent = new Panel()
            {
                Size = new Size(800, 450),
                Location = new Point(100, 100),
                BackColor = Color.FromArgb(160, 15, 23, 42), // Dark transparent
            };
            UIHelper.ApplyShadow(pnlContent);

            pnlContent.Paint += (s, e) => {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    // Küçük ve şık bir arka plan ışığı (Glow effect)
                    path.AddEllipse(new Rectangle(250, -30, 300, 150));
                    using (System.Drawing.Drawing2D.PathGradientBrush pgb = new System.Drawing.Drawing2D.PathGradientBrush(path))
                    {
                        pgb.CenterColor = Color.FromArgb(70, 255, 215, 0); // Gold Glow
                        Color[] surr = { Color.FromArgb(0, 255, 215, 0) };
                        pgb.SurroundColors = surr;
                        e.Graphics.FillPath(pgb, path);
                    }
                }
            };

            lblArtvin = new Label()
            {
                Text = "ARTVİN ARAÇ KİRALAMA",
                Font = new Font("Century Gothic", 22, FontStyle.Bold | FontStyle.Italic),
                ForeColor = Color.FromArgb(255, 215, 0), // Gold
                BackColor = Color.Transparent,
                Location = new Point(0, 20),
                Size = new Size(800, 45),
                TextAlign = ContentAlignment.MiddleCenter
            };

            lblBaslik = new Label()
            {
                Text = "HOŞGELDİNİZ",
                Font = new Font("Segoe UI", 36, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Location = new Point(0, 65),
                Size = new Size(800, 80),
                TextAlign = ContentAlignment.MiddleCenter
            };

            lblAltBaslik = new Label()
            {
                Text = "Kusursuz bir sürüş deneyimi için doğru yerdesiniz.",
                Font = new Font("Segoe UI", 16, FontStyle.Italic),
                ForeColor = UIHelper.TextSecondary,
                BackColor = Color.Transparent,
                Location = new Point(0, 145),
                Size = new Size(800, 40),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Enhanced buttons
            btnMusteriPaneli = CreateEliteButton("GALERİYİ KEŞFET", new Point(150, 240), UIHelper.AccentColor);
            btnMusteriPaneli.Click += (s, e) => {
                new MusteriKiralamaForm().ShowDialog();
            };

            btnYoneticiPaneli = CreateEliteButton("YÖNETİCİ GİRİŞİ", new Point(450, 240), UIHelper.BackgroundColor);
            btnYoneticiPaneli.Click += (s, e) => {
                this.Hide();
                GirisForm gf = new GirisForm();
                gf.FormClosed += (sender, args) => this.Show();
                gf.Show();
            };

            Label lblFooter = new Label() {
                Text = "Premium Hizmet | 7/24 Destek | VIP Teslimat",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = UIHelper.TextSecondary,
                Dock = DockStyle.Bottom,
                Height = 40,
                TextAlign = ContentAlignment.MiddleCenter
            };

            pnlContent.Controls.Add(lblArtvin);
            pnlContent.Controls.Add(lblBaslik);
            pnlContent.Controls.Add(lblAltBaslik);
            pnlContent.Controls.Add(btnMusteriPaneli);
            pnlContent.Controls.Add(btnYoneticiPaneli);
            pnlContent.Controls.Add(lblFooter);

            this.Controls.Add(pnlContent);
        }

        private Button CreateEliteButton(string text, Point loc, Color backColor)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Location = loc;
            btn.Size = new Size(220, 85);
            btn.BackColor = backColor;
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            btn.Cursor = Cursors.Hand;
            
            // Interaction feedback
            btn.MouseEnter += (s, e) => btn.BackColor = Color.FromArgb(20, backColor); // Highlight
            btn.MouseLeave += (s, e) => btn.BackColor = backColor;

            return btn;
        }
    }
}
