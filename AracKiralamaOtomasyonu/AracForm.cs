using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

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
            this.Text = "Araç Yönetimi";
            this.Size = new Size(1000, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 10);

            int lblX = 20, txtX = 140, startY = 20, gapY = 40;
            int col2X = 350, txt2X = 450;

            this.Controls.Add(new Label() { Text = "Plaka:", Location = new Point(lblX, startY), AutoSize = true, ForeColor = Color.White, BackColor = Color.Transparent });
            txtPlaka = new TextBox() { Location = new Point(txtX, startY), Width = 150 };

            this.Controls.Add(new Label() { Text = "Marka:", Location = new Point(lblX, startY + gapY * 1), AutoSize = true, ForeColor = Color.White, BackColor = Color.Transparent });
            txtMarka = new TextBox() { Location = new Point(txtX, startY + gapY * 1), Width = 150 };

            this.Controls.Add(new Label() { Text = "Model:", Location = new Point(lblX, startY + gapY * 2), AutoSize = true, ForeColor = Color.White, BackColor = Color.Transparent });
            txtModel = new TextBox() { Location = new Point(txtX, startY + gapY * 2), Width = 150 };

            this.Controls.Add(new Label() { Text = "Yıl:", Location = new Point(lblX, startY + gapY * 3), AutoSize = true, ForeColor = Color.White, BackColor = Color.Transparent });
            txtYil = new TextBox() { Location = new Point(txtX, startY + gapY * 3), Width = 150 };

            this.Controls.Add(new Label() { Text = "Vites Tipi:", Location = new Point(col2X, startY), AutoSize = true, ForeColor = Color.White, BackColor = Color.Transparent });
            cmbVites = new ComboBox() { Location = new Point(txt2X, startY), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbVites.Items.AddRange(new string[] { "Manuel", "Otomatik", "Yarı Otomatik" });
            cmbVites.SelectedIndex = 0;

            this.Controls.Add(new Label() { Text = "Yakıt Türü:", Location = new Point(col2X, startY + gapY * 1), AutoSize = true, ForeColor = Color.White, BackColor = Color.Transparent });
            cmbYakit = new ComboBox() { Location = new Point(txt2X, startY + gapY * 1), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbYakit.Items.AddRange(new string[] { "Benzin", "Dizel", "Elektrik", "Hibrid", "LPG" });
            cmbYakit.SelectedIndex = 0;

            this.Controls.Add(new Label() { Text = "Kilometre:", Location = new Point(col2X, startY + gapY * 2), AutoSize = true, ForeColor = Color.White, BackColor = Color.Transparent });
            txtKilometre = new TextBox() { Location = new Point(txt2X, startY + gapY * 2), Width = 150 };

            this.Controls.Add(new Label() { Text = "Günlük Fiyat:", Location = new Point(col2X, startY + gapY * 3), AutoSize = true, ForeColor = Color.White, BackColor = Color.Transparent });
            txtFiyat = new TextBox() { Location = new Point(txt2X, startY + gapY * 3), Width = 150 };

            this.Controls.Add(new Label() { Text = "Guc (HP):", Location = new Point(col2X, startY + gapY * 4), AutoSize = true, ForeColor = Color.White, BackColor = Color.Transparent });
            txtGuc = new TextBox() { Location = new Point(txt2X, startY + gapY * 4), Width = 150 };

            this.Controls.Add(new Label() { Text = "0-100 (sn):", Location = new Point(lblX, startY + gapY * 4), AutoSize = true, ForeColor = Color.White, BackColor = Color.Transparent });
            txtHizlanma = new TextBox() { Location = new Point(txtX, startY + gapY * 4), Width = 150 };

            this.Controls.Add(new Label() { Text = "Koltuk:", Location = new Point(lblX, startY + gapY * 5), AutoSize = true, ForeColor = Color.White, BackColor = Color.Transparent });
            txtKoltuk = new TextBox() { Location = new Point(txtX, startY + gapY * 5), Width = 150 };

            this.Controls.Add(new Label() { Text = "Durum:", Location = new Point(col2X, startY + gapY * 5), AutoSize = true, ForeColor = Color.White, BackColor = Color.Transparent });
            cmbDurum = new ComboBox() { Location = new Point(txt2X, startY + gapY * 5), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbDurum.Items.AddRange(new string[] { "Boş", "Dolu" });
            cmbDurum.SelectedIndex = 0;

            this.Controls.AddRange(new Control[] { txtPlaka, txtMarka, txtModel, txtYil, cmbVites, cmbYakit, txtKilometre, txtFiyat, cmbDurum, txtGuc, txtHizlanma, txtKoltuk });

            foreach (var ctrl in new Control[] { txtPlaka, txtMarka, txtModel, txtYil, cmbVites, cmbYakit, txtKilometre, txtFiyat, cmbDurum, txtGuc, txtHizlanma, txtKoltuk })
            {
                UIHelper.StyleModernInput(ctrl);
            }

            int btnX = 650;
            btnEkle = CreateActionButton("Ekle", new Point(btnX, startY), Color.MediumSeaGreen);
            btnEkle.Click += BtnEkle_Click;

            btnGuncelle = CreateActionButton("Güncelle", new Point(btnX, startY + gapY + 10), Color.DodgerBlue);
            btnGuncelle.Click += BtnGuncelle_Click;

            btnSil = CreateActionButton("Sil", new Point(btnX, startY + gapY * 2 + 20), Color.IndianRed);
            btnSil.Click += BtnSil_Click;

            btnTemizle = CreateActionButton("Temizle", new Point(btnX, startY + gapY * 3 + 30), Color.Gray);
            btnTemizle.Click += (s, e) => Temizle();

            this.Controls.AddRange(new Control[] { btnEkle, btnGuncelle, btnSil, btnTemizle });

            txtAra = UIHelper.CreateSearchBox("Plaka içinde ara...", this, new Point(20, 270));
            txtAra.TextChanged += (s, e) => Listele();
            this.Controls.Add(txtAra);

            dgvAraclar = new DataGridView()
            {
                Location = new Point(20, 310),
                Size = new Size(940, 280),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false
            };
            UIHelper.StyleDataGridView(dgvAraclar);
            dgvAraclar.CellClick += DgvAraclar_CellClick;
            this.Controls.Add(dgvAraclar);
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
            if (string.IsNullOrWhiteSpace(txtPlaka.Text))
            {
                MessageBox.Show("Plaka alanı boş bırakılamaz.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (!int.TryParse(txtYil.Text, out int yil) || yil < 1950 || yil > DateTime.Now.Year + 1)
            {
                MessageBox.Show("Lütfen geçerli bir üretim yılı giriniz.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (!double.TryParse(txtFiyat.Text, out _))
            {
                MessageBox.Show("Lütfen geçerli bir fiyat giriniz.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (!int.TryParse(txtKilometre.Text, out _))
            {
                MessageBox.Show("Lütfen geçerli bir kilometre giriniz.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    string query = "SELECT * FROM Araclar";
                    string searchKeyword = txtAra.Text.Trim();
                    if (searchKeyword != "Plaka içinde ara..." && !string.IsNullOrEmpty(searchKeyword))
                    {
                        query += " WHERE Plaka LIKE '%' || @arama || '%'";
                    }

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
            txtPlaka.Clear();
            txtMarka.Clear();
            txtModel.Clear();
            txtYil.Clear();
            txtKilometre.Clear();
            txtFiyat.Clear();
            cmbVites.SelectedIndex = 0;
            cmbYakit.SelectedIndex = 0;
            cmbDurum.SelectedIndex = 0;
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
                MessageBox.Show("Araç başarıyla eklendi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Listele();
                Temizle();
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
                MessageBox.Show("Araç bilgileri güncellendi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Listele();
                Temizle();
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
                    {
                        string query = "DELETE FROM Araclar WHERE Id=@id";
                        using (var cmd = new SQLiteCommand(query, connection))
                        {
                            cmd.Parameters.AddWithValue("@id", seciliAracId);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    MessageBox.Show("Araç silindi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Listele();
                    Temizle();
                }
                catch (Exception ex) { MessageBox.Show("Hata: " + ex.Message); }
            }
        }
    }
}
