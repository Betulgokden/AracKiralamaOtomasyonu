using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;

namespace AracKiralamaOtomasyonu
{
    public class KiralamaForm : Form
    {
        private ComboBox cmbMusteri, cmbArac;
        private DateTimePicker dtpBaslangic, dtpBitis;
        private TextBox txtAra;
        private Label lblToplamTutar;
        private Button btnKirala, btnGeriAlindi, btnIptalEt;
        private DataGridView dgvKiralamalar;

        private DataTable dtAraclar;
        private string seciliKiralamaId = "";
        private string seciliKiralikAracId = "";

        public KiralamaForm()
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer | ControlStyles.OptimizedDoubleBuffer, true);
            InitializeComponents();
            UIHelper.ApplyModernBackground(this);
            FormYukle();
        }

        private void InitializeComponents()
        {
            this.Text = "Kiralama İşlemleri";
            this.Size = new Size(850, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 10);

            int lblX = 20, txtX = 140, startY = 20, gapY = 40;

            this.Controls.Add(new Label() { Text = "Müşteri:", Location = new Point(lblX, startY), AutoSize = true, ForeColor = Color.White, BackColor = Color.Transparent });
            cmbMusteri = new ComboBox() { Location = new Point(txtX, startY), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };

            this.Controls.Add(new Label() { Text = "Araç (Müsait):", Location = new Point(lblX, startY + gapY * 1), AutoSize = true, ForeColor = Color.White, BackColor = Color.Transparent });
            cmbArac = new ComboBox() { Location = new Point(txtX, startY + gapY * 1), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbArac.SelectedIndexChanged += HesaplaTutar;

            this.Controls.Add(new Label() { Text = "Başlangıç Tarihi:", Location = new Point(lblX, startY + gapY * 2), AutoSize = true, ForeColor = Color.White, BackColor = Color.Transparent });
            dtpBaslangic = new DateTimePicker() { Location = new Point(txtX, startY + gapY * 2), Width = 200, Format = DateTimePickerFormat.Short };
            dtpBaslangic.ValueChanged += HesaplaTutar;

            this.Controls.Add(new Label() { Text = "Bitiş Tarihi:", Location = new Point(lblX, startY + gapY * 3), AutoSize = true, ForeColor = Color.White, BackColor = Color.Transparent });
            dtpBitis = new DateTimePicker() { Location = new Point(txtX, startY + gapY * 3), Width = 200, Format = DateTimePickerFormat.Short };
            dtpBitis.ValueChanged += HesaplaTutar;

            this.Controls.Add(new Label() { Text = "Toplam Tutar:", Location = new Point(lblX, startY + gapY * 4), AutoSize = true, ForeColor = Color.White, BackColor = Color.Transparent });
            lblToplamTutar = new Label() { Text = "0 TL", Location = new Point(txtX, startY + gapY * 4), AutoSize = true, Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = UIHelper.AccentColor, BackColor = Color.Transparent };

            UIHelper.StyleModernInput(cmbMusteri);
            UIHelper.StyleModernInput(cmbArac);
            UIHelper.StyleModernInput(dtpBaslangic);
            UIHelper.StyleModernInput(dtpBitis);

            this.Controls.AddRange(new Control[] { cmbMusteri, cmbArac, dtpBaslangic, dtpBitis, lblToplamTutar });

            // Butonlar
            btnKirala = new Button() { Text = "Kirala", Location = new Point(360, startY), Width = 140, Height = 50, BackColor = Color.MediumSeaGreen, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11, FontStyle.Bold) };
            btnKirala.Click += BtnKirala_Click;

            btnGeriAlindi = new Button() { Text = "Geri Alındı", Location = new Point(510, startY), Width = 140, Height = 50, BackColor = Color.DodgerBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11, FontStyle.Bold) };
            btnGeriAlindi.Click += BtnGeriAlindi_Click;

            btnIptalEt = new Button() { Text = "İptal Edildi", Location = new Point(660, startY), Width = 140, Height = 50, BackColor = Color.IndianRed, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11, FontStyle.Bold) };
            btnIptalEt.Click += BtnIptalEt_Click;

            this.Controls.Add(btnKirala);
            this.Controls.Add(btnGeriAlindi);
            this.Controls.Add(btnIptalEt);

            txtAra = UIHelper.CreateSearchBox("Müşteri veya Plaka ara...", this, new Point(20, 210));
            txtAra.Width = 790;
            txtAra.TextChanged += (s, e) => KiralamalariGetir();

            // DataGridView
            dgvKiralamalar = new DataGridView()
            {
                Location = new Point(20, 260),
                Size = new Size(790, 280),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false
            };
            UIHelper.StyleDataGridView(dgvKiralamalar);
            dgvKiralamalar.CellClick += DgvKiralamalar_CellClick;
            this.Controls.Add(dgvKiralamalar);
        }

        private void FormYukle()
        {
            MusterileriGetir();
            MusaitAraclariGetir();
            KiralamalariGetir();
        }

        private void MusterileriGetir()
        {
            using (var con = DatabaseHelper.GetConnection())
            {
                using (SQLiteDataAdapter da = new SQLiteDataAdapter("SELECT Id, AdSoyad FROM Musteriler", con))
                {
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    cmbMusteri.DisplayMember = "AdSoyad";
                    cmbMusteri.ValueMember = "Id";
                    cmbMusteri.DataSource = dt;
                }
            }
        }

        private void MusaitAraclariGetir()
        {
            using (var con = DatabaseHelper.GetConnection())
            {
                string query = "SELECT Id, (Plaka || ' - ' || Marka || ' ' || Model) as AracBilgi, GunlukFiyat FROM Araclar WHERE Durum='Boş'";
                using (SQLiteDataAdapter da = new SQLiteDataAdapter(query, con))
                {
                    dtAraclar = new DataTable();
                    da.Fill(dtAraclar);
                    cmbArac.DisplayMember = "AracBilgi";
                    cmbArac.ValueMember = "Id";
                    cmbArac.DataSource = dtAraclar;
                }
            }
        }

        private void KiralamalariGetir()
        {
            using (var con = DatabaseHelper.GetConnection())
            {
                string searchTerm = (txtAra != null && txtAra.Text != "Müşteri veya Plaka ara...") ? txtAra.Text.Trim() : "";
                
                string query = @"
                    SELECT k.Id, m.AdSoyad as [Müşteri], m.Telefon, a.Plaka, 
                           k.BaslangicTarihi as [Başlangıç], k.BitisTarihi as [Bitiş], 
                           k.ToplamTutar as [Tutar], k.Durum, a.Id as AracId
                    FROM Kiralamalar k
                    INNER JOIN Musteriler m ON k.MusteriId = m.Id
                    INNER JOIN Araclar a ON k.AracId = a.Id";

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query += " WHERE m.AdSoyad LIKE @s OR a.Plaka LIKE @s";
                }

                query += " ORDER BY k.Id DESC";

                using (var cmd = new SQLiteCommand(query, con))
                {
                    if (!string.IsNullOrEmpty(searchTerm))
                        cmd.Parameters.AddWithValue("@s", "%" + searchTerm + "%");

                    using (SQLiteDataAdapter da = new SQLiteDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        
                        // Kalan gün hesaplama (Tracking enhancement)
                        dt.Columns.Add("Kalan Gün");
                        foreach (DataRow row in dt.Rows)
                        {
                            if (row["Durum"].ToString() == "Aktif")
                            {
                                DateTime bTarih = DateTime.Parse(row["Bitiş"].ToString());
                                int kalan = (bTarih.Date - DateTime.Now.Date).Days;
                                row["Kalan Gün"] = kalan < 0 ? $"Gecikti ({Math.Abs(kalan)} gün)" : $"{kalan} gün";
                            }
                            else
                            {
                                row["Kalan Gün"] = "-";
                            }
                        }

                        dgvKiralamalar.DataSource = dt;
                        if (dgvKiralamalar.Columns.Contains("AracId"))
                            dgvKiralamalar.Columns["AracId"].Visible = false;
                        
                        // Renklendirme ve stil
                        dgvKiralamalar.Columns["Kalan Gün"].DisplayIndex = 7; // Durumdan önce göster
                    }
                }
            }
        }

        private void HesaplaTutar(object sender, EventArgs e)
        {
            if (cmbArac.SelectedValue != null && dtAraclar != null)
            {
                int gunSayisi = (dtpBitis.Value.Date - dtpBaslangic.Value.Date).Days;
                if (gunSayisi <= 0) gunSayisi = 1;

                DataRow[] rows = dtAraclar.Select($"Id = {cmbArac.SelectedValue}");
                if (rows.Length > 0)
                {
                    double gunlukFiyat = Convert.ToDouble(rows[0]["GunlukFiyat"]);
                    double toplam = gunSayisi * gunlukFiyat;
                    lblToplamTutar.Text = toplam.ToString("C2");
                    lblToplamTutar.Tag = toplam; // Değeri tutmak için
                }
            }
        }

        private void BtnKirala_Click(object sender, EventArgs e)
        {
            if (cmbMusteri.SelectedValue == null) { MessageBox.Show("Lütfen müşteri seçin."); return; }
            if (cmbArac.SelectedValue == null) { MessageBox.Show("Lütfen araç seçin."); return; }

            try
            {
                using (var con = DatabaseHelper.GetConnection())
                {
                    using (var cmd = new SQLiteCommand("INSERT INTO Kiralamalar (MusteriId, AracId, BaslangicTarihi, BitisTarihi, ToplamTutar, Durum) VALUES (@1, @2, @3, @4, @5, 'Aktif'); UPDATE Araclar SET Durum='Dolu' WHERE Id=@2;", con))
                    {
                        cmd.Parameters.AddWithValue("@1", cmbMusteri.SelectedValue);
                        cmd.Parameters.AddWithValue("@2", cmbArac.SelectedValue);
                        cmd.Parameters.AddWithValue("@3", dtpBaslangic.Value.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@4", dtpBitis.Value.ToString("yyyy-MM-dd"));
                        object tutarT = lblToplamTutar.Tag ?? 0;
                        cmd.Parameters.AddWithValue("@5", Convert.ToDouble(tutarT));
                        cmd.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("Kiralama işlemi başarıyla kaydedildi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                FormYukle();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
            }
        }

        private void DgvKiralamalar_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvKiralamalar.Rows[e.RowIndex];
                seciliKiralamaId = row.Cells["Id"].Value.ToString();
                seciliKiralikAracId = row.Cells["AracId"].Value.ToString();
            }
        }

        private void BtnGeriAlindi_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(seciliKiralamaId))
            {
                MessageBox.Show("Lütfen işlem yapılacak kiralama kaydını tablodan seçin.");
                return;
            }

            try
            {
                using (var con = DatabaseHelper.GetConnection())
                {
                    using (var cmd = new SQLiteCommand("UPDATE Kiralamalar SET Durum='Tamamlandı' WHERE Id=@1; UPDATE Araclar SET Durum='Boş' WHERE Id=@2;", con))
                    {
                        cmd.Parameters.AddWithValue("@1", seciliKiralamaId);
                        cmd.Parameters.AddWithValue("@2", seciliKiralikAracId);
                        cmd.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("Araç geri alındı ve müsait duruma getirildi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                seciliKiralamaId = "";
                seciliKiralikAracId = "";
                FormYukle();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
            }
        }

        private void BtnIptalEt_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(seciliKiralamaId))
            {
                MessageBox.Show("Lütfen iptal edilecek kiralama kaydını tablodan seçin.");
                return;
            }

            var confirm = MessageBox.Show("Bu kiralama işlemini iptal etmek istediğinize emin misiniz?", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            try
            {
                using (var con = DatabaseHelper.GetConnection())
                {
                    using (var cmd = new SQLiteCommand("UPDATE Kiralamalar SET Durum='İptal Edildi' WHERE Id=@1; UPDATE Araclar SET Durum='Boş' WHERE Id=@2;", con))
                    {
                        cmd.Parameters.AddWithValue("@1", seciliKiralamaId);
                        cmd.Parameters.AddWithValue("@2", seciliKiralikAracId);
                        cmd.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("Kiralama işlemi iptal edildi ve araç müsait duruma getirildi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                seciliKiralamaId = "";
                seciliKiralikAracId = "";
                FormYukle();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
            }
        }
    }
}
