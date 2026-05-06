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
        private Label lblToplamGelir, lblTamamlanan, lblAktif, lblIptal;

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
            this.Text = "📊 Finans & Muhasebe Raporları";
            this.Size = new Size(1050, 680);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 10);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // ===== TOP HEADER =====
            Panel pnlHeader = new Panel() {
                Dock = DockStyle.Top, Height = 80,
                BackColor = Color.FromArgb(180, 15, 23, 42)
            };
            pnlHeader.Controls.Add(new Label() {
                Text = "📊  FİNANS & MUHASEBE RAPORLARI",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.White, BackColor = Color.Transparent,
                Location = new Point(30, 15), AutoSize = true
            });
            pnlHeader.Controls.Add(new Label() {
                Text = "Kiralama geçmişi, finansal özet ve gelir analizi.",
                Font = new Font("Segoe UI", 9), ForeColor = UIHelper.TextSecondary,
                BackColor = Color.Transparent, Location = new Point(32, 50), AutoSize = true
            });

            // ===== STAT CARDS =====
            int cardW = 235, cardH = 110, cardY = 100, gapX = 20;
            int startX = 20;

            CreateStatCard("💰  TOPLAM GELİR", "0,00 TL", Color.FromArgb(16, 185, 129), new Point(startX, cardY), out lblToplamGelir);
            CreateStatCard("✅  TAMAMLANAN", "0", Color.FromArgb(37, 99, 235), new Point(startX + (cardW + gapX), cardY), out lblTamamlanan);
            CreateStatCard("🔄  AKTİF KİRALAMA", "0", Color.FromArgb(245, 158, 11), new Point(startX + (cardW + gapX) * 2, cardY), out lblAktif);
            CreateStatCard("❌  İPTAL EDİLEN", "0", Color.FromArgb(220, 38, 38), new Point(startX + (cardW + gapX) * 3, cardY), out lblIptal);

            // ===== EXPORT BUTTON =====
            Button btnYenile = new Button() {
                Text = "🔄  Yenile",
                Location = new Point(940, 110),
                Size = new Size(90, 40),
                BackColor = Color.FromArgb(30, 41, 59),
                ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand
            };
            btnYenile.FlatAppearance.BorderSize = 0;
            btnYenile.Click += (s, e) => RaporuYukle();
            btnYenile.MouseEnter += (s, e) => btnYenile.BackColor = Color.FromArgb(51, 65, 85);
            btnYenile.MouseLeave += (s, e) => btnYenile.BackColor = Color.FromArgb(30, 41, 59);

            // ===== GRID PANEL =====
            Panel pnlGrid = new Panel() {
                Location = new Point(20, 230),
                Size = new Size(1005, 410),
                BackColor = Color.FromArgb(140, 15, 23, 42)
            };
            UIHelper.ApplyShadow(pnlGrid);

            pnlGrid.Controls.Add(new Label() {
                Text = "📋  Kiralama Geçmişi & Sözleşme Arşivi",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = UIHelper.AccentColor, BackColor = Color.Transparent,
                Location = new Point(20, 15), AutoSize = true
            });

            dgvRapor = new DataGridView() {
                Location = new Point(10, 50),
                Size = new Size(985, 350),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false
            };
            UIHelper.StyleDataGridView(dgvRapor);
            pnlGrid.Controls.Add(dgvRapor);

            this.Controls.Add(pnlHeader);
            this.Controls.Add(pnlGrid);
            this.Controls.Add(btnYenile);
        }

        private void CreateStatCard(string title, string initialValue, Color accentColor, Point location, out Label valueLabel)
        {
            Panel card = new Panel() {
                Location = location, Size = new Size(235, 110),
                BackColor = Color.FromArgb(160, 15, 23, 42)
            };
            UIHelper.ApplyShadow(card);

            // Left color bar
            Panel colorBar = new Panel() {
                Location = new Point(0, 0), Size = new Size(5, 110),
                BackColor = accentColor
            };

            Label lTitle = new Label() {
                Text = title,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = UIHelper.TextSecondary,
                BackColor = Color.Transparent,
                Location = new Point(20, 18), AutoSize = true
            };

            valueLabel = new Label() {
                Text = initialValue,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Location = new Point(18, 52),
                Size = new Size(200, 40)
            };

            card.Controls.AddRange(new Control[] { colorBar, lTitle, valueLabel });
            this.Controls.Add(card);
        }

        private Label CreateStatLabel(string text, Point loc, Color c, Control parent)
        {
            Label lbl = new Label() {
                Text = text, Location = loc, AutoSize = true,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = c, BackColor = Color.Transparent
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
                    double toplamGelir = Convert.ToDouble(new SQLiteCommand("SELECT SUM(ToplamTutar) FROM Kiralamalar WHERE Durum='Tamamlandı'", con).ExecuteScalar() ?? 0);
                    long tamamlananCount = (long)new SQLiteCommand("SELECT COUNT(*) FROM Kiralamalar WHERE Durum='Tamamlandı'", con).ExecuteScalar();
                    long aktifCount = (long)new SQLiteCommand("SELECT COUNT(*) FROM Kiralamalar WHERE Durum='Aktif'", con).ExecuteScalar();
                    long iptalCount = (long)new SQLiteCommand("SELECT COUNT(*) FROM Kiralamalar WHERE Durum='İptal Edildi'", con).ExecuteScalar();

                    lblToplamGelir.Text = toplamGelir.ToString("C2");
                    lblTamamlanan.Text = tamamlananCount.ToString();
                    lblAktif.Text = aktifCount.ToString();
                    lblIptal.Text = iptalCount.ToString();

                    string query = @"SELECT k.Id, m.AdSoyad as [Müşteri], m.Telefon, a.Plaka, 
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

                    // Color-code rows by status
                    foreach (DataGridViewRow row in dgvRapor.Rows)
                    {
                        string durum = row.Cells["Durum"]?.Value?.ToString() ?? "";
                        if (durum == "Aktif")
                            row.DefaultCellStyle.ForeColor = UIHelper.AccentColor;
                        else if (durum == "Tamamlandı")
                            row.DefaultCellStyle.ForeColor = UIHelper.SuccessColor;
                        else if (durum == "İptal Edildi")
                            row.DefaultCellStyle.ForeColor = UIHelper.DangerColor;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Rapor yüklenirken hata: " + ex.Message); }
        }
    }
}
