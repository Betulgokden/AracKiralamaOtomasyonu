using System;
using System.Drawing;
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
            this.Size = new Size(400, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 10);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            Panel pnlGlass = new Panel()
            {
                Size = new Size(340, 420),
                Location = new Point(22, 20),
                BackColor = Color.FromArgb(180, 15, 23, 42)
            };
            UIHelper.ApplyShadow(pnlGlass);

            lblBaslik = new Label()
            {
                Text = "Betül GÖKDEN",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(0, 30),
                Size = new Size(340, 40),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };

            lblKullanici = new Label()
            {
                Text = "Kullanıcı Adı:",
                Location = new Point(30, 90),
                AutoSize = true,
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };

            txtKullaniciAdi = new TextBox()
            {
                Location = new Point(30, 120),
                Width = 280,
                Font = new Font("Segoe UI", 12)
            };

            lblSifre = new Label()
            {
                Text = "Şifre:",
                Location = new Point(30, 170),
                AutoSize = true,
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };

            txtSifre = new TextBox()
            {
                Location = new Point(30, 200),
                Width = 280,
                PasswordChar = '*',
                Font = new Font("Segoe UI", 12)
            };

            UIHelper.StyleModernInput(txtKullaniciAdi);
            UIHelper.StyleModernInput(txtSifre);

            btnGiris = new Button()
            {
                Text = "Giriş Yap",
                Location = new Point(30, 260),
                Width = 280,
                Height = 45,
                BackColor = UIHelper.SuccessColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnGiris.FlatAppearance.BorderSize = 0;
            btnGiris.Click += BtnGiris_Click;

            btnGeri = new Button()
            {
                Text = "Ana Menüye Dön",
                Location = new Point(30, 320),
                Width = 280,
                Height = 45,
                BackColor = Color.FromArgb(100, 30, 41, 59),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnGeri.FlatAppearance.BorderSize = 0;
            btnGeri.Click += (s, e) => this.Close();

            pnlGlass.Controls.Add(lblBaslik);
            pnlGlass.Controls.Add(lblKullanici);
            pnlGlass.Controls.Add(txtKullaniciAdi);
            pnlGlass.Controls.Add(lblSifre);
            pnlGlass.Controls.Add(txtSifre);
            pnlGlass.Controls.Add(btnGiris);
            pnlGlass.Controls.Add(btnGeri);

            this.Controls.Add(pnlGlass);
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
                            SessionManager.UserRole = "Yonetici"; // Su anki veritabaninda sadece yoneticiler var

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
