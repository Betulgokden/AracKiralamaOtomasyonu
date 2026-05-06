using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Data.SQLite;

namespace AracKiralamaOtomasyonu
{
    public class GirisForm : Form
    {
        private TextBox txtKullaniciAdi;
        private TextBox txtSifre;
        private Button btnGiris;
        private Button btnGeri;
        private Label lblBaslik;
        private Label lblKullanici;
        private Label lblSifre;
        private Timer pulseTimer;
        private float pulseValue = 0f;
        private bool pulseDir = true;

        public GirisForm()
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer | ControlStyles.OptimizedDoubleBuffer, true);
            InitializeComponents();
            UIHelper.ApplyModernBackground(this);
        }

        private void InitializeComponents()
        {
            this.Text = "ARTVİN ARAÇ KİRALAMA - Giriş";
            this.Size = new Size(460, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 10);
            this.FormBorderStyle = FormBorderStyle.None;
            this.MaximizeBox = false;

            // Pulse timer for glow animation
            pulseTimer = new Timer() { Interval = 30 };
            pulseTimer.Tick += (s, e) => {
                if (pulseDir) { pulseValue += 0.03f; if (pulseValue >= 1f) pulseDir = false; }
                else { pulseValue -= 0.03f; if (pulseValue <= 0f) pulseDir = true; }
                this.Invalidate();
            };
            pulseTimer.Start();

            // Drag support (borderless form)
            bool dragging = false;
            Point dragStart = Point.Empty;
            this.MouseDown += (s, e) => { dragging = true; dragStart = e.Location; };
            this.MouseMove += (s, e) => { if (dragging) this.Location = new Point(this.Left + e.X - dragStart.X, this.Top + e.Y - dragStart.Y); };
            this.MouseUp += (s, e) => dragging = false;

            // Close button top right
            Button btnX = new Button() {
                Text = "✕", Location = new Point(415, 12), Size = new Size(32, 32),
                BackColor = Color.Transparent, ForeColor = Color.FromArgb(148, 163, 184),
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11, FontStyle.Bold), Cursor = Cursors.Hand
            };
            btnX.FlatAppearance.BorderSize = 0;
            btnX.Click += (s, e) => this.Close();
            btnX.MouseEnter += (s, e) => btnX.ForeColor = Color.White;
            btnX.MouseLeave += (s, e) => btnX.ForeColor = Color.FromArgb(148, 163, 184);

            // Glass card panel
            Panel pnlGlass = new Panel()
            {
                Size = new Size(400, 520),
                Location = new Point(30, 40),
                BackColor = Color.FromArgb(200, 15, 23, 42)
            };
            UIHelper.ApplyShadow(pnlGlass);

            // Lock icon label
            Label lblIcon = new Label() {
                Text = "🔐",
                Font = new Font("Segoe UI", 36),
                Location = new Point(0, 20),
                Size = new Size(400, 60),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            lblBaslik = new Label()
            {
                Text = "YÖNETİCİ GİRİŞİ",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                Location = new Point(0, 85),
                Size = new Size(400, 40),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };

            Label lblSubtitle = new Label() {
                Text = "Artvin Araç Kiralama Yönetim Sistemi",
                Font = new Font("Segoe UI", 9),
                Location = new Point(0, 125),
                Size = new Size(400, 25),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = UIHelper.TextSecondary,
                BackColor = Color.Transparent
            };

            // Divider
            Panel divider = new Panel() { Location = new Point(40, 160), Size = new Size(320, 1), BackColor = Color.FromArgb(40, 255, 255, 255) };

            lblKullanici = new Label()
            {
                Text = "KULLANICI ADI",
                Location = new Point(40, 180),
                AutoSize = true,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = UIHelper.AccentColor,
                BackColor = Color.Transparent
            };

            // Input wrapper
            Panel pnlUser = new Panel() { Location = new Point(40, 200), Size = new Size(320, 45), BackColor = Color.FromArgb(30, 41, 59) };
            txtKullaniciAdi = new TextBox() {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 12),
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(30, 41, 59),
                ForeColor = Color.White,
                Padding = new Padding(10, 10, 10, 10)
            };
            pnlUser.Padding = new Padding(12, 8, 12, 8);
            pnlUser.Controls.Add(txtKullaniciAdi);

            lblSifre = new Label()
            {
                Text = "ŞİFRE",
                Location = new Point(40, 265),
                AutoSize = true,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = UIHelper.AccentColor,
                BackColor = Color.Transparent
            };

            Panel pnlPass = new Panel() { Location = new Point(40, 285), Size = new Size(320, 45), BackColor = Color.FromArgb(30, 41, 59) };
            txtSifre = new TextBox() {
                Dock = DockStyle.Fill,
                PasswordChar = '●',
                Font = new Font("Segoe UI", 12),
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(30, 41, 59),
                ForeColor = Color.White
            };
            pnlPass.Padding = new Padding(12, 8, 12, 8);
            pnlPass.Controls.Add(txtSifre);

            // Focus effects
            pnlUser.Click += (s, e) => txtKullaniciAdi.Focus();
            pnlPass.Click += (s, e) => txtSifre.Focus();
            txtKullaniciAdi.GotFocus += (s, e) => pnlUser.BackColor = Color.FromArgb(40, 56, 80);
            txtKullaniciAdi.LostFocus += (s, e) => pnlUser.BackColor = Color.FromArgb(30, 41, 59);
            txtSifre.GotFocus += (s, e) => pnlPass.BackColor = Color.FromArgb(40, 56, 80);
            txtSifre.LostFocus += (s, e) => pnlPass.BackColor = Color.FromArgb(30, 41, 59);
            txtKullaniciAdi.BackColor = Color.FromArgb(30, 41, 59);
            txtSifre.BackColor = Color.FromArgb(30, 41, 59);

            // Enter key support
            txtSifre.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) BtnGiris_Click(null, null); };

            btnGiris = new Button()
            {
                Text = "GİRİŞ YAP  →",
                Location = new Point(40, 360),
                Width = 320,
                Height = 50,
                BackColor = UIHelper.AccentColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnGiris.FlatAppearance.BorderSize = 0;
            btnGiris.Click += BtnGiris_Click;
            btnGiris.MouseEnter += (s, e) => btnGiris.BackColor = Color.FromArgb(14, 165, 233);
            btnGiris.MouseLeave += (s, e) => btnGiris.BackColor = UIHelper.AccentColor;

            btnGeri = new Button()
            {
                Text = "← Ana Menüye Dön",
                Location = new Point(40, 425),
                Width = 320,
                Height = 45,
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(100, 116, 139),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10),
                Cursor = Cursors.Hand
            };
            btnGeri.FlatAppearance.BorderSize = 0;
            btnGeri.Click += (s, e) => this.Close();
            btnGeri.MouseEnter += (s, e) => btnGeri.ForeColor = Color.White;
            btnGeri.MouseLeave += (s, e) => btnGeri.ForeColor = Color.FromArgb(100, 116, 139);

            Label lblVersion = new Label() {
                Text = "v2.5 © Betül GÖKDEN",
                Font = new Font("Segoe UI", 8),
                Location = new Point(0, 488),
                Size = new Size(400, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(60, 255, 255, 255),
                BackColor = Color.Transparent
            };

            pnlGlass.Controls.AddRange(new Control[] { lblIcon, lblBaslik, lblSubtitle, divider, lblKullanici, pnlUser, lblSifre, pnlPass, btnGiris, btnGeri, lblVersion });

            this.Controls.Add(pnlGlass);
            this.Controls.Add(btnX);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            // Ambient glow behind the card
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int alpha = (int)(40 + 40 * pulseValue);
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddEllipse(100, 150, 260, 260);
                using (PathGradientBrush pgb = new PathGradientBrush(path))
                {
                    pgb.CenterColor = Color.FromArgb(alpha, UIHelper.AccentColor);
                    pgb.SurroundColors = new Color[] { Color.Transparent };
                    g.FillPath(pgb, path);
                }
            }
        }

        private void BtnGiris_Click(object sender, EventArgs e)
        {
            string kAdi = txtKullaniciAdi.Text.Trim();
            string sifre = txtSifre.Text.Trim();

            if (string.IsNullOrEmpty(kAdi) || string.IsNullOrEmpty(sifre))
            {
                MessageBox.Show("Lütfen kullanıcı adı ve şifreyi boş bırakmayınız.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    string query = "SELECT COUNT(*) FROM Kullanicilar WHERE KullaniciAdi = @kadi AND Sifre = @sifre";
                    using (var cmd = new SQLiteCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@kadi", kAdi);
                        cmd.Parameters.AddWithValue("@sifre", sifre);

                        long count = (long)cmd.ExecuteScalar();
                        if (count > 0)
                        {
                            SessionManager.CurrentUser = kAdi;
                            SessionManager.UserRole = "Yonetici";

                            pulseTimer?.Stop();
                            this.Hide();
                            using (AnaForm anaForm = new AnaForm())
                            {
                                anaForm.ShowDialog();
                            }
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Hatalı kullanıcı adı veya şifre!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Giriş sırasında hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
