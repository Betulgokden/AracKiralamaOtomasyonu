using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace AracKiralamaOtomasyonu
{
    public class MusteriForm : Form
    {
        private DataGridView dgvMusteriler;
        private TextBox txtTCNo, txtAdSoyad, txtTelefon, txtEhliyet, txtAdres, txtAra;
        private Button btnEkle, btnGuncelle, btnSil, btnTemizle;
        private string seciliMusteriId = "";

        public MusteriForm()
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer | ControlStyles.OptimizedDoubleBuffer, true);
            InitializeComponents();
            UIHelper.ApplyModernBackground(this);
            Listele();
        }

        private void InitializeComponents()
        {
            this.Text = "👥 CRM & Müşteri İlişkileri";
            this.Size = new Size(1050, 700);
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
                Text = "👥  CRM & MÜŞTERİ İLİŞKİLERİ",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.White, BackColor = Color.Transparent,
                Location = new Point(30, 15), AutoSize = true
            });
            pnlHeader.Controls.Add(new Label() {
                Text = "Müşteri kaydı ekle, güncelle, sil ve CRM veritabanını yönet.",
                Font = new Font("Segoe UI", 9), ForeColor = UIHelper.TextSecondary,
                BackColor = Color.Transparent, Location = new Point(32, 50), AutoSize = true
            });

            // ===== FORM PANEL =====
            Panel pnlForm = new Panel() {
                Location = new Point(20, 100),
                Size = new Size(660, 300),
                BackColor = Color.FromArgb(160, 15, 23, 42)
            };
            UIHelper.ApplyShadow(pnlForm);

            pnlForm.Controls.Add(new Label() {
                Text = "📋  Müşteri Bilgileri",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = UIHelper.AccentColor, BackColor = Color.Transparent,
                Location = new Point(20, 15), AutoSize = true
            });

            int lblX = 20, txtX = 170, startY = 50, gapY = 44;

            AddLabel(pnlForm, "TC Kimlik No:", new Point(lblX, startY));
            txtTCNo = AddInput(pnlForm, new Point(txtX, startY), 180, 11);

            AddLabel(pnlForm, "Ad Soyad:", new Point(lblX, startY + gapY));
            txtAdSoyad = AddInput(pnlForm, new Point(txtX, startY + gapY), 220);

            AddLabel(pnlForm, "Telefon:", new Point(lblX, startY + gapY * 2));
            txtTelefon = AddInput(pnlForm, new Point(txtX, startY + gapY * 2), 180);

            AddLabel(pnlForm, "Ehliyet No:", new Point(lblX, startY + gapY * 3));
            txtEhliyet = AddInput(pnlForm, new Point(txtX, startY + gapY * 3), 180);

            AddLabel(pnlForm, "Adres:", new Point(lblX, startY + gapY * 4));
            txtAdres = new TextBox() {
                Location = new Point(txtX, startY + gapY * 4), Width = 450,
                Multiline = true, Height = 60
            };
            UIHelper.StyleModernInput(txtAdres);
            pnlForm.Controls.Add(txtAdres);

            // ===== ACTIONS PANEL =====
            Panel pnlActions = new Panel() {
                Location = new Point(695, 100),
                Size = new Size(330, 300),
                BackColor = Color.FromArgb(160, 15, 23, 42)
            };
            UIHelper.ApplyShadow(pnlActions);

            pnlActions.Controls.Add(new Label() {
                Text = "⚡  İşlemler",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = UIHelper.AccentColor, BackColor = Color.Transparent,
                Location = new Point(20, 15), AutoSize = true
            });

            btnEkle = CreateActionButton("➕  Müşteri Ekle", new Point(20, 55), Color.FromArgb(16, 185, 129));
            btnEkle.Click += BtnEkle_Click;

            btnGuncelle = CreateActionButton("✏️  Güncelle", new Point(20, 115), Color.FromArgb(37, 99, 235));
            btnGuncelle.Click += BtnGuncelle_Click;

            btnSil = CreateActionButton("🗑️  Müşteri Sil", new Point(20, 175), Color.FromArgb(220, 38, 38));
            btnSil.Click += BtnSil_Click;

            btnTemizle = CreateActionButton("🔄  Formu Temizle", new Point(20, 235), Color.FromArgb(71, 85, 105));
            btnTemizle.Click += (s, e) => Temizle();

            pnlActions.Controls.AddRange(new Control[] { btnEkle, btnGuncelle, btnSil, btnTemizle });

            // ===== GRID PANEL =====
            Panel pnlGrid = new Panel() {
                Location = new Point(20, 415),
                Size = new Size(1005, 245),
                BackColor = Color.FromArgb(140, 15, 23, 42)
            };
            UIHelper.ApplyShadow(pnlGrid);

            pnlGrid.Controls.Add(new Label() {
                Text = "📊  Müşteri Listesi",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = UIHelper.TextSecondary, BackColor = Color.Transparent,
                Location = new Point(15, 10), AutoSize = true
            });

            Panel pnlSearchBox = new Panel() { Location = new Point(550, 8), Size = new Size(440, 36), BackColor = Color.FromArgb(30, 41, 59) };
            txtAra = new TextBox() {
                Text = "TC veya Ad içinde ara...", ForeColor = UIHelper.TextSecondary,
                BackColor = Color.FromArgb(30, 41, 59), BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 10), Dock = DockStyle.Fill
            };
            txtAra.Enter += (s, e) => { if (txtAra.Text.Contains("ara")) { txtAra.Text = ""; txtAra.ForeColor = Color.White; } };
            txtAra.Leave += (s, e) => { if (string.IsNullOrWhiteSpace(txtAra.Text)) { txtAra.Text = "TC veya Ad içinde ara..."; txtAra.ForeColor = UIHelper.TextSecondary; } };
            txtAra.TextChanged += (s, e) => Listele();
            pnlSearchBox.Padding = new Padding(10, 6, 10, 5);
            pnlSearchBox.Controls.Add(txtAra);
            pnlGrid.Controls.Add(pnlSearchBox);

            dgvMusteriler = new DataGridView() {
                Location = new Point(10, 48),
                Size = new Size(985, 187),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false, ReadOnly = true, AllowUserToAddRows = false
            };
            UIHelper.StyleDataGridView(dgvMusteriler);
            dgvMusteriler.CellClick += DgvMusteriler_CellClick;
            pnlGrid.Controls.Add(dgvMusteriler);

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

        private TextBox AddInput(Panel parent, Point loc, int width, int maxLen = 100)
        {
            TextBox txt = new TextBox() { Location = loc, Width = width, MaxLength = maxLen };
            UIHelper.StyleModernInput(txt);
            parent.Controls.Add(txt);
            return txt;
        }

        private Button CreateActionButton(string text, Point loc, Color backColor)
        {
            Button btn = new Button() {
                Text = text, Location = loc, Width = 285, Height = 50,
                BackColor = backColor, ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(15, 0, 0, 0), Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.MouseEnter += (s, e) => btn.BackColor = ControlPaint.Light(backColor, 0.15f);
            btn.MouseLeave += (s, e) => btn.BackColor = backColor;
            return btn;
        }

        private bool Validasyon()
        {
            if (txtTCNo.Text.Length != 11 || !txtTCNo.Text.All(char.IsDigit)) { MessageBox.Show("TC Kimlik Numarası 11 haneli ve sadece rakam olmalıdır.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false; }
            if (string.IsNullOrWhiteSpace(txtAdSoyad.Text)) { MessageBox.Show("Ad Soyad alanı boş bırakılamaz.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false; }
            if (string.IsNullOrWhiteSpace(txtEhliyet.Text)) { MessageBox.Show("Ehliyet No alanı boş bırakılamaz.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false; }
            return true;
        }

        private void Listele()
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    string query = "SELECT * FROM Musteriler";
                    string searchKeyword = txtAra?.Text.Trim() ?? "";
                    if (searchKeyword != "TC veya Ad içinde ara..." && !string.IsNullOrEmpty(searchKeyword))
                        query += " WHERE TCNo LIKE '%' || @arama || '%' OR AdSoyad LIKE '%' || @arama || '%'";
                    using (var cmd = new SQLiteCommand(query, connection))
                    {
                        if (query.Contains("@arama")) cmd.Parameters.AddWithValue("@arama", searchKeyword);
                        using (SQLiteDataAdapter da = new SQLiteDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            da.Fill(dt);
                            dgvMusteriler.DataSource = dt;
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Listeleme hatası: " + ex.Message); }
        }

        private void Temizle()
        {
            txtTCNo.Clear(); txtAdSoyad.Clear(); txtTelefon.Clear();
            txtEhliyet.Clear(); txtAdres.Clear();
            seciliMusteriId = "";
        }

        private void BtnEkle_Click(object sender, EventArgs e)
        {
            if (!Validasyon()) return;
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                using (var cmd = new SQLiteCommand("INSERT INTO Musteriler (TCNo, AdSoyad, Telefon, EhliyetNo, Adres) VALUES (@p1, @p2, @p3, @p4, @p5)", connection))
                {
                    cmd.Parameters.AddWithValue("@p1", txtTCNo.Text.Trim());
                    cmd.Parameters.AddWithValue("@p2", txtAdSoyad.Text.Trim());
                    cmd.Parameters.AddWithValue("@p3", txtTelefon.Text.Trim());
                    cmd.Parameters.AddWithValue("@p4", txtEhliyet.Text.Trim());
                    cmd.Parameters.AddWithValue("@p5", txtAdres.Text.Trim());
                    cmd.ExecuteNonQuery();
                }
                MessageBox.Show("✅ Müşteri başarıyla eklendi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Listele(); Temizle();
            }
            catch (Exception ex) { MessageBox.Show("Hata: " + ex.Message); }
        }

        private void DgvMusteriler_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvMusteriler.Rows[e.RowIndex];
                seciliMusteriId = row.Cells["Id"].Value.ToString();
                txtTCNo.Text = row.Cells["TCNo"].Value.ToString();
                txtAdSoyad.Text = row.Cells["AdSoyad"].Value.ToString();
                txtTelefon.Text = row.Cells["Telefon"].Value.ToString();
                txtEhliyet.Text = row.Cells["EhliyetNo"].Value.ToString();
                txtAdres.Text = row.Cells["Adres"].Value.ToString();
            }
        }

        private void BtnGuncelle_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(seciliMusteriId)) { MessageBox.Show("Lütfen güncellenecek müşteriyi seçin."); return; }
            if (!Validasyon()) return;
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                using (var cmd = new SQLiteCommand("UPDATE Musteriler SET TCNo=@p1, AdSoyad=@p2, Telefon=@p3, EhliyetNo=@p4, Adres=@p5 WHERE Id=@id", connection))
                {
                    cmd.Parameters.AddWithValue("@p1", txtTCNo.Text.Trim());
                    cmd.Parameters.AddWithValue("@p2", txtAdSoyad.Text.Trim());
                    cmd.Parameters.AddWithValue("@p3", txtTelefon.Text.Trim());
                    cmd.Parameters.AddWithValue("@p4", txtEhliyet.Text.Trim());
                    cmd.Parameters.AddWithValue("@p5", txtAdres.Text.Trim());
                    cmd.Parameters.AddWithValue("@id", seciliMusteriId);
                    cmd.ExecuteNonQuery();
                }
                MessageBox.Show("✅ Müşteri güncellendi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Listele(); Temizle();
            }
            catch (Exception ex) { MessageBox.Show("Hata: " + ex.Message); }
        }

        private void BtnSil_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(seciliMusteriId)) { MessageBox.Show("Lütfen silinecek müşteriyi seçin."); return; }
            var confirm = MessageBox.Show("Bu müşteriyi silmek istediğinize emin misiniz?", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm == DialogResult.Yes)
            {
                try
                {
                    using (var connection = DatabaseHelper.GetConnection())
                    using (var cmd = new SQLiteCommand("DELETE FROM Musteriler WHERE Id=@id", connection))
                    {
                        cmd.Parameters.AddWithValue("@id", seciliMusteriId);
                        cmd.ExecuteNonQuery();
                    }
                    MessageBox.Show("✅ Müşteri silindi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Listele(); Temizle();
                }
                catch (Exception ex) { MessageBox.Show("Hata: " + ex.Message); }
            }
        }
    }
}
