using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace AracKiralamaOtomasyonu
{
    public class MusteriKiralamaForm : Form
    {
        private ModernFlowLayoutPanel flowAraclar;
        private Label lblHeader, lblSubHeader;
        private TextBox txtAra;

        public MusteriKiralamaForm()
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer | ControlStyles.OptimizedDoubleBuffer, true);
            InitializeComponents();
            UIHelper.SetDoubleBuffered(flowAraclar);
            ListeyiYukle();
        }

        private void InitializeComponents()
        {
            this.Text = "Araç Galerisi - Kiralık Araçlar";
            this.Size = new Size(1300, 850);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(15, 23, 42);
            this.Font = new Font("Segoe UI", 10);

            // Background
            UIHelper.ApplyModernBackground(this);

            // Top Header Panel
            Panel pnlTop = new Panel() {
                Dock = DockStyle.Top,
                Height = 120,
                BackColor = Color.FromArgb(200, 15, 23, 42)
            };

            lblHeader = new Label() {
                Text = "🚗 ARAÇ GALERİSİ",
                Font = new Font("Segoe UI", 28, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Location = new Point(40, 15),
                AutoSize = true
            };

            lblSubHeader = new Label() {
                Text = "Beğendiğiniz aracı seçerek detaylarını inceleyin ve kiralama işleminizi başlatın.",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.FromArgb(148, 163, 184),
                BackColor = Color.Transparent,
                Location = new Point(42, 70),
                AutoSize = true
            };

            // Search box
            Panel pnlSearch = new Panel() {
                Location = new Point(900, 35),
                Size = new Size(340, 45),
                BackColor = Color.FromArgb(30, 41, 59)
            };
            txtAra = new TextBox() {
                Text = "🔍 Marka veya model ara...",
                ForeColor = Color.Gray,
                BackColor = Color.FromArgb(30, 41, 59),
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 12),
                Dock = DockStyle.Fill
            };
            txtAra.Enter += (s, e) => { if (txtAra.Text.Contains("ara")) { txtAra.Text = ""; txtAra.ForeColor = Color.White; } };
            txtAra.Leave += (s, e) => { if (string.IsNullOrWhiteSpace(txtAra.Text)) { txtAra.Text = "🔍 Marka veya model ara..."; txtAra.ForeColor = Color.Gray; } };
            txtAra.TextChanged += (s, e) => ListeyiYukle();
            pnlSearch.Padding = new Padding(12, 10, 12, 5);
            pnlSearch.Controls.Add(txtAra);

            pnlTop.Controls.Add(lblHeader);
            pnlTop.Controls.Add(lblSubHeader);
            pnlTop.Controls.Add(pnlSearch);

            // Gallery area - full width
            flowAraclar = new ModernFlowLayoutPanel() {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.Transparent, // Görseli görmek için tekrar saydam yapıldı ama ModernFlowLayoutPanel sayesinde kasmayacak
                Padding = new Padding(20, 20, 20, 20),
                WrapContents = true
            };

            this.Controls.Add(flowAraclar);
            this.Controls.Add(pnlTop);
        }

        private void ListeyiYukle()
        {
            flowAraclar.SuspendLayout();
            flowAraclar.Controls.Clear();

            string searchTerm = "";
            if (txtAra != null && !txtAra.Text.Contains("ara") && !string.IsNullOrWhiteSpace(txtAra.Text))
                searchTerm = txtAra.Text.Trim();

            using (var con = DatabaseHelper.GetConnection())
            {
                string query = "SELECT * FROM Araclar WHERE Durum='Boş'";
                if (!string.IsNullOrEmpty(searchTerm))
                    query += " AND (Marka LIKE @s OR Model LIKE @s)";

                using (var cmd = new SQLiteCommand(query, con))
                {
                    if (!string.IsNullOrEmpty(searchTerm))
                        cmd.Parameters.AddWithValue("@s", "%" + searchTerm + "%");

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            flowAraclar.Controls.Add(CreateCarCard(reader));
                    }
                }
            }
            flowAraclar.ResumeLayout();
        }

        private ModernCarCard CreateCarCard(IDataRecord row)
        {
            string id = row["Id"].ToString();
            string marka = row["Marka"].ToString();
            string model = row["Model"].ToString();
            string name = $"{marka} {model}";
            double fiyat = Convert.ToDouble(row["GunlukFiyat"]);
            string vites = row["Vites"]?.ToString() ?? "Otomatik";
            string yakit = row["Yakit"]?.ToString() ?? "Benzin";
            int yil = Convert.ToInt32(row["Yil"]);
            string statsText = $"{yil} • {vites} • {yakit}";

            ModernCarCard card = new ModernCarCard()
            {
                Title = name,
                Price = $"{fiyat:N0} TL / Gün",
                Stats = statsText,
                CarImage = ImageService.GetImage(marka, row["ResimYolu"]?.ToString()),
                Tag = id
            };

            card.CardClick = () => {
                // Open the detail form for this car
                using (var con = DatabaseHelper.GetConnection())
                using (var cmd = new SQLiteCommand("SELECT * FROM Araclar WHERE Id=@id", con))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var detayForm = new AracDetayForm(reader);
                            detayForm.ShowDialog();
                            // Refresh gallery after booking
                            ListeyiYukle();
                        }
                    }
                }
            };

            return card;
        }
    }
}
