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
            this.Text = "Müşteri Yönetimi";
            this.Size = new Size(850, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 10);

            int lblX = 20, txtX = 140, startY = 20, gapY = 40;

            this.Controls.Add(new Label() { Text = "TC No (11 Hane):", Location = new Point(lblX, startY), AutoSize = true, ForeColor = Color.White, BackColor = Color.Transparent });
            txtTCNo = new TextBox() { Location = new Point(txtX, startY), Width = 150, MaxLength = 11 };

            this.Controls.Add(new Label() { Text = "Ad Soyad:", Location = new Point(lblX, startY + gapY * 1), AutoSize = true, ForeColor = Color.White, BackColor = Color.Transparent });
            txtAdSoyad = new TextBox() { Location = new Point(txtX, startY + gapY * 1), Width = 200 };

            this.Controls.Add(new Label() { Text = "Telefon:", Location = new Point(lblX, startY + gapY * 2), AutoSize = true, ForeColor = Color.White, BackColor = Color.Transparent });
            txtTelefon = new TextBox() { Location = new Point(txtX, startY + gapY * 2), Width = 150 };

            this.Controls.Add(new Label() { Text = "Ehliyet No:", Location = new Point(lblX, startY + gapY * 3), AutoSize = true, ForeColor = Color.White, BackColor = Color.Transparent });
            txtEhliyet = new TextBox() { Location = new Point(txtX, startY + gapY * 3), Width = 150 };

            this.Controls.Add(new Label() { Text = "Adres:", Location = new Point(lblX, startY + gapY * 4), AutoSize = true, ForeColor = Color.White, BackColor = Color.Transparent });
            txtAdres = new TextBox() { Location = new Point(txtX, startY + gapY * 4), Width = 200, Multiline = true, Height = 60 };

            this.Controls.AddRange(new Control[] { txtTCNo, txtAdSoyad, txtTelefon, txtEhliyet, txtAdres });

            foreach (var ctrl in new Control[] { txtTCNo, txtAdSoyad, txtTelefon, txtEhliyet, txtAdres })
            {
                UIHelper.StyleModernInput(ctrl);
            }

            int btnX = 380;
            btnEkle = CreateActionButton("Ekle", new Point(btnX, startY), Color.MediumSeaGreen);
            btnEkle.Click += BtnEkle_Click;

            btnGuncelle = CreateActionButton("Güncelle", new Point(btnX, startY + gapY + 10), Color.DodgerBlue);
            btnGuncelle.Click += BtnGuncelle_Click;

            btnSil = CreateActionButton("Sil", new Point(btnX, startY + gapY * 2 + 20), Color.IndianRed);
            btnSil.Click += BtnSil_Click;

            btnTemizle = CreateActionButton("Temizle", new Point(btnX, startY + gapY * 3 + 30), Color.Gray);
            btnTemizle.Click += (s, e) => Temizle();

            this.Controls.AddRange(new Control[] { btnEkle, btnGuncelle, btnSil, btnTemizle });

            txtAra = UIHelper.CreateSearchBox("TC veya Ad içinde ara...", this, new Point(20, 310));
            txtAra.TextChanged += (s, e) => Listele();
            this.Controls.Add(txtAra);

            dgvMusteriler = new DataGridView()
            {
                Location = new Point(20, 350),
                Size = new Size(790, 290),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false
            };
            UIHelper.StyleDataGridView(dgvMusteriler);
            dgvMusteriler.CellClick += DgvMusteriler_CellClick;
            this.Controls.Add(dgvMusteriler);
        }

        private Button CreateActionButton(string text, Point loc, Color backColor)
        {
            return new Button()
            {
                Text = text,
                Location = loc,
                Width = 120,
                Height = 40,
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatAppearance = { BorderSize = 0 }
            };
        }

        private bool Validasyon()
        {
            if (txtTCNo.Text.Length != 11 || !txtTCNo.Text.All(char.IsDigit))
            {
                MessageBox.Show("TC Kimlik Numarası 11 haneli ve sadece rakam olmalıdır.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtAdSoyad.Text))
            {
                MessageBox.Show("Ad Soyad alanı boş bırakılamaz.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtEhliyet.Text))
            {
                MessageBox.Show("Ehliyet No alanı boş bırakılamaz.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private void Listele()
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    string query = "SELECT * FROM Musteriler";
                    string searchKeyword = txtAra.Text.Trim();
                    if (searchKeyword != "TC veya Ad içinde ara..." && !string.IsNullOrEmpty(searchKeyword))
                    {
                        query += " WHERE TCNo LIKE '%' || @arama || '%' OR AdSoyad LIKE '%' || @arama || '%'";
                    }

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
            txtTCNo.Clear();
            txtAdSoyad.Clear();
            txtTelefon.Clear();
            txtEhliyet.Clear();
            txtAdres.Clear();
            seciliMusteriId = "";
        }

        private void BtnEkle_Click(object sender, EventArgs e)
        {
            if (!Validasyon()) return;

            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    string query = "INSERT INTO Musteriler (TCNo, AdSoyad, Telefon, EhliyetNo, Adres) VALUES (@p1, @p2, @p3, @p4, @p5)";
                    using (var cmd = new SQLiteCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@p1", txtTCNo.Text.Trim());
                        cmd.Parameters.AddWithValue("@p2", txtAdSoyad.Text.Trim());
                        cmd.Parameters.AddWithValue("@p3", txtTelefon.Text.Trim());
                        cmd.Parameters.AddWithValue("@p4", txtEhliyet.Text.Trim());
                        cmd.Parameters.AddWithValue("@p5", txtAdres.Text.Trim());
                        cmd.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("Müşteri başarıyla eklendi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Listele();
                Temizle();
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
                {
                    string query = "UPDATE Musteriler SET TCNo=@p1, AdSoyad=@p2, Telefon=@p3, EhliyetNo=@p4, Adres=@p5 WHERE Id=@id";
                    using (var cmd = new SQLiteCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@p1", txtTCNo.Text.Trim());
                        cmd.Parameters.AddWithValue("@p2", txtAdSoyad.Text.Trim());
                        cmd.Parameters.AddWithValue("@p3", txtTelefon.Text.Trim());
                        cmd.Parameters.AddWithValue("@p4", txtEhliyet.Text.Trim());
                        cmd.Parameters.AddWithValue("@p5", txtAdres.Text.Trim());
                        cmd.Parameters.AddWithValue("@id", seciliMusteriId);
                        cmd.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("Müşteri güncellendi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Listele();
                Temizle();
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
                    {
                        string query = "DELETE FROM Musteriler WHERE Id=@id";
                        using (var cmd = new SQLiteCommand(query, connection))
                        {
                            cmd.Parameters.AddWithValue("@id", seciliMusteriId);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    MessageBox.Show("Müşteri silindi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Listele();
                    Temizle();
                }
                catch (Exception ex) { MessageBox.Show("Hata: " + ex.Message); }
            }
        }
    }
}
