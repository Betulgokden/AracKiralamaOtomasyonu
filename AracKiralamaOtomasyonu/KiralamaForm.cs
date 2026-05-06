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
            this.Text = "📚 Operasyon & Sözleşmeler";
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
                Text = "📚  OPERASYON & SÖZLEŞMELER",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.White, BackColor = Color.Transparent,
                Location = new Point(30, 15), AutoSize = true
            });
            pnlHeader.Controls.Add(new Label() {
                Text = "Yeni kiralama oluştur, araç teslim al ve sözleşmeleri yönet.",
                Font = new Font("Segoe UI", 9), ForeColor = UIHelper.TextSecondary,
                BackColor = Color.Transparent, Location = new Point(32, 50), AutoSize = true
            });

            // ===== NEW RENTAL PANEL =====
            Panel pnlForm = new Panel() {
                Location = new Point(20, 100),
                Size = new Size(680, 230),
                BackColor = Color.FromArgb(160, 15, 23, 42)
            };
            UIHelper.ApplyShadow(pnlForm);

            pnlForm.Controls.Add(new Label() {
                Text = "➕  Yeni Kiralama",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = UIHelper.AccentColor, BackColor = Color.Transparent,
                Location = new Point(20, 15), AutoSize = true
            });

            int lblX = 20, txtX = 160, startY = 50, gapY = 44;

            AddLabel(pnlForm, "Müşteri:", new Point(lblX, startY));
            cmbMusteri = new ComboBox() { Location = new Point(txtX, startY), Width = 240, DropDownStyle = ComboBoxStyle.DropDownList };
            UIHelper.StyleModernInput(cmbMusteri);
            pnlForm.Controls.Add(cmbMusteri);

            AddLabel(pnlForm, "Araç (Müsait):", new Point(lblX, startY + gapY));
            cmbArac = new ComboBox() { Location = new Point(txtX, startY + gapY), Width = 240, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbArac.SelectedIndexChanged += HesaplaTutar;
            UIHelper.StyleModernInput(cmbArac);
            pnlForm.Controls.Add(cmbArac);

            AddLabel(pnlForm, "Başlangıç:", new Point(lblX, startY + gapY * 2));
            dtpBaslangic = new DateTimePicker() { Location = new Point(txtX, startY + gapY * 2), Width = 200, Format = DateTimePickerFormat.Short };
            dtpBaslangic.ValueChanged += HesaplaTutar;
            UIHelper.StyleModernInput(dtpBaslangic);
            pnlForm.Controls.Add(dtpBaslangic);

            AddLabel(pnlForm, "Bitiş:", new Point(420, startY + gapY * 2));
            dtpBitis = new DateTimePicker() { Location = new Point(540, startY + gapY * 2), Width = 120, Format = DateTimePickerFormat.Short };
            dtpBitis.ValueChanged += HesaplaTutar;
            UIHelper.StyleModernInput(dtpBitis);
            pnlForm.Controls.Add(dtpBitis);

            AddLabel(pnlForm, "Toplam Tutar:", new Point(lblX, startY + gapY * 3 + 5));
            lblToplamTutar = new Label() {
                Text = "0,00 TL",
                Location = new Point(txtX, startY + gapY * 3),
                AutoSize = true,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = UIHelper.SuccessColor,
                BackColor = Color.Transparent
            };
            pnlForm.Controls.Add(lblToplamTutar);

            // ===== ACTIONS PANEL =====
            Panel pnlActions = new Panel() {
                Location = new Point(715, 100),
                Size = new Size(310, 230),
                BackColor = Color.FromArgb(160, 15, 23, 42)
            };
            UIHelper.ApplyShadow(pnlActions);

            pnlActions.Controls.Add(new Label() {
                Text = "⚡  Sözleşme İşlemleri",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = UIHelper.AccentColor, BackColor = Color.Transparent,
                Location = new Point(15, 15), AutoSize = true
            });

            btnKirala = CreateActionButton("✅  Kirala", new Point(15, 55), Color.FromArgb(16, 185, 129));
            btnKirala.Click += BtnKirala_Click;

            btnGeriAlindi = CreateActionButton("🔄  Araç Geri Alındı", new Point(15, 115), Color.FromArgb(37, 99, 235));
            btnGeriAlindi.Click += BtnGeriAlindi_Click;

            btnIptalEt = CreateActionButton("❌  İptal Et", new Point(15, 175), Color.FromArgb(220, 38, 38));
            btnIptalEt.Click += BtnIptalEt_Click;

            pnlActions.Controls.AddRange(new Control[] { btnKirala, btnGeriAlindi, btnIptalEt });

            // ===== GRID PANEL =====
            Panel pnlGrid = new Panel() {
                Location = new Point(20, 345),
                Size = new Size(1005, 295),
                BackColor = Color.FromArgb(140, 15, 23, 42)
            };
            UIHelper.ApplyShadow(pnlGrid);

            pnlGrid.Controls.Add(new Label() {
                Text = "📋  Tüm Kiralamalar",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = UIHelper.TextSecondary, BackColor = Color.Transparent,
                Location = new Point(15, 10), AutoSize = true
            });

            Panel pnlSearchBox = new Panel() { Location = new Point(520, 8), Size = new Size(470, 36), BackColor = Color.FromArgb(30, 41, 59) };
            txtAra = new TextBox() {
                Text = "Müşteri veya Plaka ara...", ForeColor = UIHelper.TextSecondary,
                BackColor = Color.FromArgb(30, 41, 59), BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 10), Dock = DockStyle.Fill
            };
            txtAra.Enter += (s, e) => { if (txtAra.Text.Contains("ara")) { txtAra.Text = ""; txtAra.ForeColor = Color.White; } };
            txtAra.Leave += (s, e) => { if (string.IsNullOrWhiteSpace(txtAra.Text)) { txtAra.Text = "Müşteri veya Plaka ara..."; txtAra.ForeColor = UIHelper.TextSecondary; } };
            txtAra.TextChanged += (s, e) => KiralamalariGetir();
            pnlSearchBox.Padding = new Padding(10, 6, 10, 5);
            pnlSearchBox.Controls.Add(txtAra);
            pnlGrid.Controls.Add(pnlSearchBox);

            dgvKiralamalar = new DataGridView() {
                Location = new Point(10, 50),
                Size = new Size(985, 235),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false, ReadOnly = true, AllowUserToAddRows = false
            };
            UIHelper.StyleDataGridView(dgvKiralamalar);
            dgvKiralamalar.CellClick += DgvKiralamalar_CellClick;
            pnlGrid.Controls.Add(dgvKiralamalar);

            this.Controls.Add(pnlHeader);
            this.Controls.Add(pnlForm);
            this.Controls.Add(pnlActions);
            this.Controls.Add(pnlGrid);
        }

        private Label AddLabel(Panel parent, string text, Point loc)
        {
            Label lbl = new Label() { Text = text, Location = loc, AutoSize = true, ForeColor = UIHelper.TextSecondary, BackColor = Color.Transparent };
            parent.Controls.Add(lbl);
            return lbl;
        }

        private Button CreateActionButton(string text, Point loc, Color backColor)
        {
            Button btn = new Button() {
                Text = text, Location = loc, Width = 275, Height = 50,
                BackColor = backColor, ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(12, 0, 0, 0), Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.MouseEnter += (s, e) => btn.BackColor = ControlPaint.Light(backColor, 0.15f);
            btn.MouseLeave += (s, e) => btn.BackColor = backColor;
            return btn;
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
            using (SQLiteDataAdapter da = new SQLiteDataAdapter("SELECT Id, AdSoyad FROM Musteriler", con))
            {
                DataTable dt = new DataTable();
                da.Fill(dt);
                cmbMusteri.DisplayMember = "AdSoyad";
                cmbMusteri.ValueMember = "Id";
                cmbMusteri.DataSource = dt;
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
                string query = @"SELECT k.Id, m.AdSoyad as [Müşteri], m.Telefon, a.Plaka, 
                                       k.BaslangicTarihi as [Başlangıç], k.BitisTarihi as [Bitiş], 
                                       k.ToplamTutar as [Tutar], k.Durum, a.Id as AracId
                                FROM Kiralamalar k
                                INNER JOIN Musteriler m ON k.MusteriId = m.Id
                                INNER JOIN Araclar a ON k.AracId = a.Id";
                if (!string.IsNullOrEmpty(searchTerm))
                    query += " WHERE m.AdSoyad LIKE @s OR a.Plaka LIKE @s";
                query += " ORDER BY k.Id DESC";

                using (var cmd = new SQLiteCommand(query, con))
                {
                    if (!string.IsNullOrEmpty(searchTerm))
                        cmd.Parameters.AddWithValue("@s", "%" + searchTerm + "%");
                    using (SQLiteDataAdapter da = new SQLiteDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        dt.Columns.Add("Kalan Gün");
                        foreach (DataRow row in dt.Rows)
                        {
                            if (row["Durum"].ToString() == "Aktif")
                            {
                                DateTime bTarih = DateTime.Parse(row["Bitiş"].ToString());
                                int kalan = (bTarih.Date - DateTime.Now.Date).Days;
                                row["Kalan Gün"] = kalan < 0 ? $"Gecikti ({Math.Abs(kalan)} gün)" : $"{kalan} gün";
                            }
                            else row["Kalan Gün"] = "-";
                        }
                        dgvKiralamalar.DataSource = dt;
                        if (dgvKiralamalar.Columns.Contains("AracId"))
                            dgvKiralamalar.Columns["AracId"].Visible = false;
                        if (dgvKiralamalar.Columns.Contains("Kalan Gün"))
                            dgvKiralamalar.Columns["Kalan Gün"].DisplayIndex = 7;
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
                    double toplam = gunSayisi * Convert.ToDouble(rows[0]["GunlukFiyat"]);
                    lblToplamTutar.Text = toplam.ToString("C2");
                    lblToplamTutar.Tag = toplam;
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
                using (var cmd = new SQLiteCommand("INSERT INTO Kiralamalar (MusteriId, AracId, BaslangicTarihi, BitisTarihi, ToplamTutar, Durum) VALUES (@1, @2, @3, @4, @5, 'Aktif'); UPDATE Araclar SET Durum='Dolu' WHERE Id=@2;", con))
                {
                    cmd.Parameters.AddWithValue("@1", cmbMusteri.SelectedValue);
                    cmd.Parameters.AddWithValue("@2", cmbArac.SelectedValue);
                    cmd.Parameters.AddWithValue("@3", dtpBaslangic.Value.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@4", dtpBitis.Value.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@5", Convert.ToDouble(lblToplamTutar.Tag ?? 0));
                    cmd.ExecuteNonQuery();
                }
                MessageBox.Show("✅ Kiralama işlemi başarıyla kaydedildi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                FormYukle();
            }
            catch (Exception ex) { MessageBox.Show("Hata: " + ex.Message); }
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
            if (string.IsNullOrEmpty(seciliKiralamaId)) { MessageBox.Show("Lütfen işlem yapılacak kiralama kaydını tablodan seçin."); return; }
            try
            {
                using (var con = DatabaseHelper.GetConnection())
                using (var cmd = new SQLiteCommand("UPDATE Kiralamalar SET Durum='Tamamlandı' WHERE Id=@1; UPDATE Araclar SET Durum='Boş' WHERE Id=@2;", con))
                {
                    cmd.Parameters.AddWithValue("@1", seciliKiralamaId);
                    cmd.Parameters.AddWithValue("@2", seciliKiralikAracId);
                    cmd.ExecuteNonQuery();
                }
                MessageBox.Show("✅ Araç geri alındı ve müsait duruma getirildi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                seciliKiralamaId = ""; seciliKiralikAracId = "";
                FormYukle();
            }
            catch (Exception ex) { MessageBox.Show("Hata: " + ex.Message); }
        }

        private void BtnIptalEt_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(seciliKiralamaId)) { MessageBox.Show("Lütfen iptal edilecek kiralama kaydını tablodan seçin."); return; }
            var confirm = MessageBox.Show("Bu kiralama işlemini iptal etmek istediğinize emin misiniz?", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;
            try
            {
                using (var con = DatabaseHelper.GetConnection())
                using (var cmd = new SQLiteCommand("UPDATE Kiralamalar SET Durum='İptal Edildi' WHERE Id=@1; UPDATE Araclar SET Durum='Boş' WHERE Id=@2;", con))
                {
                    cmd.Parameters.AddWithValue("@1", seciliKiralamaId);
                    cmd.Parameters.AddWithValue("@2", seciliKiralikAracId);
                    cmd.ExecuteNonQuery();
                }
                MessageBox.Show("Kiralama işlemi iptal edildi ve araç müsait duruma getirildi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                seciliKiralamaId = ""; seciliKiralikAracId = "";
                FormYukle();
            }
            catch (Exception ex) { MessageBox.Show("Hata: " + ex.Message); }
        }
    }
}
