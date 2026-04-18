using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;

namespace AracKiralamaOtomasyonu
{
    public class RaporForm : Form
    {
        private DataGridView dgvRapor;
        private Label lblToplamGelir, lblTamamlanan, lblAktif;

        public RaporForm()
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer | ControlStyles.OptimizedDoubleBuffer, true);
            InitializeComponents();
            UIHelper.ApplyModernBackground(this);
            RaporuYukle();
        }

        private void InitializeComponents()
        {
            this.Text = "Kiralama Raporları ve İstatistikler";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 10);

            Label lblBaslik = new Label()
            {
                Text = "Kiralama Geçmişi ve Finansal Özet",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true,
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };

            // Özet Paneli
            Panel pnlOzet = new Panel()
            {
                Location = new Point(20, 70),
                Size = new Size(840, 100),
                BackColor = UIHelper.CardColor
            };

            lblToplamGelir = CreateStatLabel("Toplam Gelir: 0 TL", new Point(20, 30), UIHelper.SuccessColor, pnlOzet);
            lblTamamlanan = CreateStatLabel("Tamamlanan Kiralamalar: 0", new Point(300, 30), UIHelper.AccentColor, pnlOzet);
            lblAktif = CreateStatLabel("Şu An Aktif Kiralamalar: 0", new Point(600, 30), Color.Orange, pnlOzet);

            dgvRapor = new DataGridView()
            {
                Location = new Point(20, 190),
                Size = new Size(840, 350),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false
            };
            UIHelper.StyleDataGridView(dgvRapor);

            this.Controls.Add(lblBaslik);
            this.Controls.Add(pnlOzet);
            this.Controls.Add(dgvRapor);
        }

        private Label CreateStatLabel(string text, Point loc, Color c, Control parent)
        {
            Label lbl = new Label()
            {
                Text = text,
                Location = loc,
                AutoSize = true,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = c,
                BackColor = Color.Transparent
            };
            parent.Controls.Add(lbl);
            return lbl;
        }

        private void RaporuYukle()
        {
            try
            {
                using (var con = DatabaseHelper.GetConnection())
                {
                    // İstatistikler
                    double toplamGelir = Convert.ToDouble(new SQLiteCommand("SELECT SUM(ToplamTutar) FROM Kiralamalar WHERE Durum='Tamamlandı'", con).ExecuteScalar() ?? 0);
                    long tamamlananCount = (long)new SQLiteCommand("SELECT COUNT(*) FROM Kiralamalar WHERE Durum='Tamamlandı'", con).ExecuteScalar();
                    long aktifCount = (long)new SQLiteCommand("SELECT COUNT(*) FROM Kiralamalar WHERE Durum='Aktif'", con).ExecuteScalar();

                    lblToplamGelir.Text = $"Toplam Gelir: {toplamGelir:C2}";
                    lblTamamlanan.Text = $"Tamamlanan: {tamamlananCount}";
                    lblAktif.Text = $"Aktif: {aktifCount}";

                    // Tablo Listesi
                    string query = @"
                        SELECT k.Id, m.AdSoyad as [Müşteri], m.Telefon, a.Plaka, 
                               k.BaslangicTarihi as [Başlangıç], k.BitisTarihi as [Bitiş], 
                               k.ToplamTutar as [Toplam Tutar], k.Durum
                        FROM Kiralamalar k
                        INNER JOIN Musteriler m ON k.MusteriId = m.Id
                        INNER JOIN Araclar a ON k.AracId = a.Id
                        ORDER BY k.Id DESC";
                    
                    using (SQLiteDataAdapter da = new SQLiteDataAdapter(query, con))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        dgvRapor.DataSource = dt;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Rapor yüklenirken hata: " + ex.Message);
            }
        }
    }
}
