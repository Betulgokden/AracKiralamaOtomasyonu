using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Data.SQLite;

namespace AracKiralamaOtomasyonu
{
    public class AracForm : Form
    {
        private DataGridView dgvAraclar;
        private TextBox txtPlaka, txtMarka, txtModel, txtYil, txtKilometre, txtFiyat, txtAra;
        private TextBox txtGuc, txtHizlanma, txtKoltuk;
        private ComboBox cmbDurum, cmbVites, cmbYakit;
        private Button btnEkle, btnGuncelle, btnSil, btnTemizle;
        private string seciliAracId = "";

        public AracForm()
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer | ControlStyles.OptimizedDoubleBuffer, true);
            InitializeComponents();
            UIHelper.ApplyModernBackground(this);
            Listele();
        }

        private void InitializeComponents()
        {
            this.Text = "🚗 Filo Yönetim Merkezi";
            this.Size = new Size(1100, 720);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 10);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // ===== TOP HEADER PANEL =====
            Panel pnlHeader = new Panel() {
                Dock = DockStyle.Top, Height = 80,
                BackColor = Color.FromArgb(180, 15, 23, 42)
            };
            Label lblTitle = new Label() {
                Text = "🚗  FİLO YÖNETİM MERKEZİ",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.White, BackColor = Color.Transparent,
                Location = new Point(30, 15), AutoSize = true
            };
            Label lblSub = new Label() {
                Text = "Araç ekle, güncelle, sil ve mevcut filoyu yönet.",
                Font = new Font("Segoe UI", 9), ForeColor = UIHelper.TextSecondary,
                BackColor = Color.Transparent, Location = new Point(32, 50), AutoSize = true
            };
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblSub);

            // ===== FORM PANEL (left-ish area) =====
            Panel pnlForm = new Panel() {
                Location = new Point(20, 100),
                Size = new Size(680, 320),
                BackColor = Color.FromArgb(160, 15, 23, 42)
            };
            UIHelper.ApplyShadow(pnlForm);

            Label lblFormTitle = new Label() {
                Text = "📝  Araç Bilgileri",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = UIHelper.AccentColor, BackColor = Color.Transparent,
                Location = new Point(20, 15), AutoSize = true
            };
            pnlForm.Controls.Add(lblFormTitle);

            // Grid layout inside form panel
            int lblX = 20, txtX = 160, startY = 50, gapY = 42;
            int lblX2 = 370, txtX2 = 510;

            AddLabel(pnlForm, "Plaka:", new Point(lblX, startY));
            txtPlaka = AddInput(pnlForm, new Point(txtX, startY), 160);

            AddLabel(pnlForm, "Marka:", new Point(lblX, startY + gapY));
            txtMarka = AddInput(pnlForm, new Point(txtX, startY + gapY), 160);

            AddLabel(pnlForm, "Model:", new Point(lblX, startY + gapY * 2));
            txtModel = AddInput(pnlForm, new Point(txtX, startY + gapY * 2), 160);

            AddLabel(pnlForm, "Yıl:", new Point(lblX, startY + gapY * 3));
            txtYil = AddInput(pnlForm, new Point(txtX, startY + gapY * 3), 160);

            AddLabel(pnlForm, "0-100 (sn):", new Point(lblX, startY + gapY * 4));
            txtHizlanma = AddInput(pnlForm, new Point(txtX, startY + gapY * 4), 160);

            AddLabel(pnlForm, "Koltuk:", new Point(lblX, startY + gapY * 5));
            txtKoltuk = AddInput(pnlForm, new Point(txtX, startY + gapY * 5), 160);

            // Right column
            AddLabel(pnlForm, "Vites Tipi:", new Point(lblX2, startY));
            cmbVites = new ComboBox() { Location = new Point(txtX2, startY), Width = 140, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbVites.Items.AddRange(new string[] { "Manuel", "Otomatik", "Yarı Otomatik" });
            cmbVites.SelectedIndex = 0;
            UIHelper.StyleModernInput(cmbVites);
            pnlForm.Controls.Add(cmbVites);

            AddLabel(pnlForm, "Yakıt Türü:", new Point(lblX2, startY + gapY));
            cmbYakit = new ComboBox() { Location = new Point(txtX2, startY + gapY), Width = 140, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbYakit.Items.AddRange(new string[] { "Benzin", "Dizel", "Elektrik", "Hibrid", "LPG" });
            cmbYakit.SelectedIndex = 0;
            UIHelper.StyleModernInput(cmbYakit);
            pnlForm.Controls.Add(cmbYakit);

            AddLabel(pnlForm, "Kilometre:", new Point(lblX2, startY + gapY * 2));
            txtKilometre = AddInput(pnlForm, new Point(txtX2, startY + gapY * 2), 140);

            AddLabel(pnlForm, "Günlük Fiyat:", new Point(lblX2, startY + gapY * 3));
            txtFiyat = AddInput(pnlForm, new Point(txtX2, startY + gapY * 3), 140);

            AddLabel(pnlForm, "Güç (HP):", new Point(lblX2, startY + gapY * 4));
            txtGuc = AddInput(pnlForm, new Point(txtX2, startY + gapY * 4), 140);

            AddLabel(pnlForm, "Durum:", new Point(lblX2, startY + gapY * 5));
            cmbDurum = new ComboBox() { Location = new Point(txtX2, startY + gapY * 5), Width = 140, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbDurum.Items.AddRange(new string[] { "Boş", "Dolu" });
            cmbDurum.SelectedIndex = 0;
            UIHelper.StyleModernInput(cmbDurum);
            pnlForm.Controls.Add(cmbDurum);

            // ===== ACTION BUTTONS PANEL (right) =====
            Panel pnlActions = new Panel() {
                Location = new Point(715, 100),
                Size = new Size(360, 320),
                BackColor = Color.FromArgb(160, 15, 23, 42)
            };
            UIHelper.ApplyShadow(pnlActions);

            Label lblActTitle = new Label() {
                Text = "⚡  İşlemler",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = UIHelper.AccentColor, BackColor = Color.Transparent,
                Location = new Point(20, 15), AutoSize = true
            };
            pnlActions.Controls.Add(lblActTitle);

            btnEkle = CreateActionButton("➕  Araç Ekle", new Point(20, 55), Color.FromArgb(16, 185, 129));
            btnEkle.Click += BtnEkle_Click;

            btnGuncelle = CreateActionButton("✏️  Güncelle", new Point(20, 115), Color.FromArgb(37, 99, 235));
            btnGuncelle.Click += BtnGuncelle_Click;

            btnSil = CreateActionButton("🗑️  Aracı Sil", new Point(20, 175), Color.FromArgb(220, 38, 38));
            btnSil.Click += BtnSil_Click;

            btnTemizle = CreateActionButton("🔄  Formu Temizle", new Point(20, 235), Color.FromArgb(71, 85, 105));
            btnTemizle.Click += (s, e) => Temizle();

            pnlActions.Controls.AddRange(new Control[] { btnEkle, btnGuncelle, btnSil, btnTemizle });

            // ===== SEARCH + GRID PANEL =====
            Panel pnlGrid = new Panel() {
                Location = new Point(20, 435),
                Size = new Size(1055, 240),
                BackColor = Color.FromArgb(140, 15, 23, 42)
            };
            UIHelper.ApplyShadow(pnlGrid);

            Label lblSearch = new Label() {
                Text = "🔍  Araç Listesi",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = UIHelper.TextSecondary, BackColor = Color.Transparent,
                Location = new Point(15, 10), AutoSize = true
            };
            pnlGrid.Controls.Add(lblSearch);

            Panel pnlSearchBox = new Panel() { Location = new Point(600, 8), Size = new Size(440, 36), BackColor = Color.FromArgb(30, 41, 59) };
            txtAra = new TextBox() {
                Text = "Plaka içinde ara...", ForeColor = UIHelper.TextSecondary,
                BackColor = Color.FromArgb(30, 41, 59), BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 10), Dock = DockStyle.Fill
            };
            txtAra.Enter += (s, e) => { if (txtAra.Text.Contains("ara")) { txtAra.Text = ""; txtAra.ForeColor = Color.White; } };
            txtAra.Leave += (s, e) => { if (string.IsNullOrWhiteSpace(txtAra.Text)) { txtAra.Text = "Plaka içinde ara..."; txtAra.ForeColor = UIHelper.TextSecondary; } };
            txtAra.TextChanged += (s, e) => Listele();
            pnlSearchBox.Padding = new Padding(10, 6, 10, 5);
            pnlSearchBox.Controls.Add(txtAra);
            pnlGrid.Controls.Add(pnlSearchBox);

            dgvAraclar = new DataGridView() {
                Location = new Point(10, 48),
                Size = new Size(1035, 182),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false
            };
            UIHelper.StyleDataGridView(dgvAraclar);
            dgvAraclar.CellClick += DgvAraclar_CellClick;
            pnlGrid.Controls.Add(dgvAraclar);

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

        private TextBox AddInput(Panel parent, Point loc, int width)
        {
            TextBox txt = new TextBox() { Location = loc, Width = width };
            UIHelper.StyleModernInput(txt);
            parent.Controls.Add(txt);
            return txt;
        }

        private Button CreateActionButton(string text, Point loc, Color backColor)
        {
            Button btn = new Button() {
                Text = text, Location = loc, Width = 310, Height = 50,
                BackColor = backColor, ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 0, 0),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.MouseEnter += (s, e) => btn.BackColor = ControlPaint.Light(backColor, 0.15f);
            btn.MouseLeave += (s, e) => btn.BackColor = backColor;
            return btn;
        }

        private bool Validasyon()
        {
            if (string.IsNullOrWhiteSpace(txtPlaka.Text)) { MessageBox.Show("Plaka alanı boş bırakılamaz.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false; }
            if (!int.TryParse(txtYil.Text, out int yil) || yil < 1950 || yil > DateTime.Now.Year + 1) { MessageBox.Show("Lütfen geçerli bir üretim yılı giriniz.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false; }
            if (!double.TryParse(txtFiyat.Text, out _)) { MessageBox.Show("Lütfen geçerli bir fiyat giriniz.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false; }
            if (!int.TryParse(txtKilometre.Text, out _)) { MessageBox.Show("Lütfen geçerli bir kilometre giriniz.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false; }
            return true;
        }

        private void Listele()
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    string query = "SELECT * FROM Araclar";
                    string searchKeyword = txtAra?.Text.Trim() ?? "";
                    if (searchKeyword != "Plaka içinde ara..." && !string.IsNullOrEmpty(searchKeyword))
                        query += " WHERE Plaka LIKE '%' || @arama || '%'";

                    using (var cmd = new SQLiteCommand(query, connection))
                    {
                        if (query.Contains("@arama")) cmd.Parameters.AddWithValue("@arama", searchKeyword);
                        using (SQLiteDataAdapter da = new SQLiteDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            da.Fill(dt);
                            dgvAraclar.DataSource = dt;
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Listeleme hatası: " + ex.Message); }
        }

        private void Temizle()
        {
            txtPlaka.Clear(); txtMarka.Clear(); txtModel.Clear(); txtYil.Clear();
            txtKilometre.Clear(); txtFiyat.Clear(); txtGuc.Clear();
            txtHizlanma.Clear(); txtKoltuk.Clear();
            cmbVites.SelectedIndex = 0; cmbYakit.SelectedIndex = 0; cmbDurum.SelectedIndex = 0;
            seciliAracId = "";
        }

        private void BtnEkle_Click(object sender, EventArgs e)
        {
            if (!Validasyon()) return;
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    string query = "INSERT INTO Araclar (Plaka, Marka, Model, Yil, Vites, Yakit, Kilometre, GunlukFiyat, Durum, Guc, Hizlanma, KoltukSayisi) VALUES (@p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10, @p11, @p12)";
                    using (var cmd = new SQLiteCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@p1", txtPlaka.Text.Trim().ToUpper());
                        cmd.Parameters.AddWithValue("@p2", txtMarka.Text.Trim());
                        cmd.Parameters.AddWithValue("@p3", txtModel.Text.Trim());
                        cmd.Parameters.AddWithValue("@p4", int.Parse(txtYil.Text));
                        cmd.Parameters.AddWithValue("@p5", cmbVites.Text);
                        cmd.Parameters.AddWithValue("@p6", cmbYakit.Text);
                        cmd.Parameters.AddWithValue("@p7", int.Parse(txtKilometre.Text));
                        cmd.Parameters.AddWithValue("@p8", double.Parse(txtFiyat.Text));
                        cmd.Parameters.AddWithValue("@p9", cmbDurum.Text);
                        cmd.Parameters.AddWithValue("@p10", int.TryParse(txtGuc.Text, out int g) ? g : 150);
                        cmd.Parameters.AddWithValue("@p11", double.TryParse(txtHizlanma.Text, out double h) ? h : 8.5);
                        cmd.Parameters.AddWithValue("@p12", int.TryParse(txtKoltuk.Text, out int k) ? k : 5);
                        cmd.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("✅ Araç başarıyla eklendi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Listele(); Temizle();
            }
            catch (Exception ex) { MessageBox.Show("Hata: " + ex.Message); }
        }

        private void DgvAraclar_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvAraclar.Rows[e.RowIndex];
                seciliAracId = row.Cells["Id"].Value.ToString();
                txtPlaka.Text = row.Cells["Plaka"].Value.ToString();
                txtMarka.Text = row.Cells["Marka"].Value.ToString();
                txtModel.Text = row.Cells["Model"].Value.ToString();
                txtYil.Text = row.Cells["Yil"].Value.ToString();
                cmbVites.Text = row.Cells["Vites"].Value.ToString();
                cmbYakit.Text = row.Cells["Yakit"].Value.ToString();
                txtKilometre.Text = row.Cells["Kilometre"].Value.ToString();
                txtFiyat.Text = row.Cells["GunlukFiyat"].Value.ToString();
                cmbDurum.Text = row.Cells["Durum"].Value.ToString();
                txtGuc.Text = row.Cells["Guc"].Value.ToString();
                txtHizlanma.Text = row.Cells["Hizlanma"].Value.ToString();
                txtKoltuk.Text = row.Cells["KoltukSayisi"].Value.ToString();
            }
        }

        private void BtnGuncelle_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(seciliAracId)) { MessageBox.Show("Lütfen güncellenecek aracı seçin."); return; }
            if (!Validasyon()) return;
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    string query = "UPDATE Araclar SET Plaka=@p1, Marka=@p2, Model=@p3, Yil=@p4, Vites=@p5, Yakit=@p6, Kilometre=@p7, GunlukFiyat=@p8, Durum=@p9, Guc=@p10, Hizlanma=@p11, KoltukSayisi=@p12 WHERE Id=@id";
                    using (var cmd = new SQLiteCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@p1", txtPlaka.Text.Trim().ToUpper());
                        cmd.Parameters.AddWithValue("@p2", txtMarka.Text.Trim());
                        cmd.Parameters.AddWithValue("@p3", txtModel.Text.Trim());
                        cmd.Parameters.AddWithValue("@p4", int.Parse(txtYil.Text));
                        cmd.Parameters.AddWithValue("@p5", cmbVites.Text);
                        cmd.Parameters.AddWithValue("@p6", cmbYakit.Text);
                        cmd.Parameters.AddWithValue("@p7", int.Parse(txtKilometre.Text));
                        cmd.Parameters.AddWithValue("@p8", double.Parse(txtFiyat.Text));
                        cmd.Parameters.AddWithValue("@p9", cmbDurum.Text);
                        cmd.Parameters.AddWithValue("@p10", int.TryParse(txtGuc.Text, out int g) ? g : 150);
                        cmd.Parameters.AddWithValue("@p11", double.TryParse(txtHizlanma.Text, out double h) ? h : 8.5);
                        cmd.Parameters.AddWithValue("@p12", int.TryParse(txtKoltuk.Text, out int k) ? k : 5);
                        cmd.Parameters.AddWithValue("@id", seciliAracId);
                        cmd.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("✅ Araç bilgileri güncellendi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Listele(); Temizle();
            }
            catch (Exception ex) { MessageBox.Show("Hata: " + ex.Message); }
        }

        private void BtnSil_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(seciliAracId)) { MessageBox.Show("Lütfen silinecek aracı seçin."); return; }
            var confirm = MessageBox.Show("Bu aracı silmek istediğinize emin misiniz?", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm == DialogResult.Yes)
            {
                try
                {
                    using (var connection = DatabaseHelper.GetConnection())
                    using (var cmd = new SQLiteCommand("DELETE FROM Araclar WHERE Id=@id", connection))
                    {
                        cmd.Parameters.AddWithValue("@id", seciliAracId);
                        cmd.ExecuteNonQuery();
                    }
                    MessageBox.Show("✅ Araç silindi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Listele(); Temizle();
                }
                catch (Exception ex) { MessageBox.Show("Hata: " + ex.Message); }
            }
        }
    }
}
