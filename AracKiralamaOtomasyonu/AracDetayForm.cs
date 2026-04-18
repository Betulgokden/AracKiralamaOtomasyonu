using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace AracKiralamaOtomasyonu
{
    public class AracDetayForm : Form
    {
        private string aracId;
        private double gunlukFiyat;
        private string aracAdi;
        private Image aracImage;

        // Sol panel kontrolleri
        private PictureBox pbArac;
        private Label lblAracAdi, lblSegment;

        // Sağ panel kontrolleri
        private TextBox txtTC, txtAdSoyad, txtTelefon;
        private DateTimePicker dtpBaslangic, dtpBitis;
        private RadioButton rbStandart, rbFullKasko;
        private CheckBox cbCocukKoltugu, cbGPS, cbEkSurucu;
        private ComboBox cmbOdeme;
        private Label lblToplamFiyat;
        private Button btnKirala;

        public AracDetayForm(IDataRecord row)
        {
            aracId = row["Id"].ToString();
            gunlukFiyat = Convert.ToDouble(row["GunlukFiyat"]);
            aracAdi = $"{row["Marka"]} {row["Model"]}";
            aracImage = ImageService.GetImage(row["Marka"].ToString(), row["ResimYolu"]?.ToString());

            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer | ControlStyles.OptimizedDoubleBuffer, true);
            
            InitializeComponents(row);
            UIHelper.ApplyModernBackground(this);
        }

        private void InitializeComponents(IDataRecord row)
        {
            this.Text = $"{aracAdi} - Araç Detay & Kiralama";
            this.Size = new Size(1100, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 10);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // ========== SOL PANEL - ARAÇ BİLGİLERİ ==========
            Panel pnlSol = new Panel() {
                Dock = DockStyle.Left,
                Width = 550,
                BackColor = Color.FromArgb(160, 15, 23, 42),
                Padding = new Padding(30)
            };

            // Araç görseli
            pbArac = new PictureBox() {
                Location = new Point(30, 20),
                Size = new Size(490, 280),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent,
                Image = aracImage
            };

            // Araç adı
            lblAracAdi = new Label() {
                Text = aracAdi,
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Location = new Point(30, 310),
                Size = new Size(490, 40)
            };

            // Segment bilgisi
            lblSegment = new Label() {
                Text = GetSegmentText(row),
                Font = new Font("Segoe UI", 10),
                ForeColor = UIHelper.AccentColor,
                BackColor = Color.Transparent,
                Location = new Point(30, 355),
                Size = new Size(490, 25)
            };

            // Bilgi kartları
            Panel pnlBilgiler = new Panel() {
                Location = new Point(30, 395),
                Size = new Size(490, 200),
                BackColor = Color.FromArgb(100, 255, 255, 255)
            };

            int yil = 0;
            try { yil = Convert.ToInt32(row["Yil"]); } catch { yil = 2024; }
            string vites = row["Vites"]?.ToString() ?? "Otomatik";
            string yakit = row["Yakit"]?.ToString() ?? "Benzin";
            int km = 0;
            try { km = Convert.ToInt32(row["Kilometre"]); } catch { }
            int guc = 0;
            try { guc = Convert.ToInt32(row["Guc"]); } catch { guc = 100; }
            int koltuk = 5;
            try { koltuk = Convert.ToInt32(row["KoltukSayisi"]); } catch { }

            // Bilgi satırları
            AddInfoRow(pnlBilgiler, "📅 Yıl:", yil.ToString(), 15);
            AddInfoRow(pnlBilgiler, "⚙️ Vites:", vites, 50);
            AddInfoRow(pnlBilgiler, "⛽ Yakıt:", yakit, 85);
            AddInfoRow(pnlBilgiler, "🛣️ Kilometre:", km.ToString("N0") + " km", 120);
            AddInfoRow(pnlBilgiler, "🐎 Güç:", guc + " HP", 155);

            // Fiyat etiketi
            Label lblFiyatBaslik = new Label() {
                Text = "Günlük Kiralama Fiyatı:",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(100, 116, 139),
                Location = new Point(30, 610),
                AutoSize = true
            };
            Label lblFiyat = new Label() {
                Text = $"{gunlukFiyat:N0} TL / Gün",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.FromArgb(16, 185, 129),
                Location = new Point(250, 600),
                AutoSize = true
            };

            pnlSol.Controls.Add(pbArac);
            pnlSol.Controls.Add(lblAracAdi);
            pnlSol.Controls.Add(lblSegment);
            pnlSol.Controls.Add(pnlBilgiler);
            pnlSol.Controls.Add(lblFiyatBaslik);
            pnlSol.Controls.Add(lblFiyat);

            // ========== SAĞ PANEL - KİRALAMA FORMU ==========
            Panel pnlSag = new Panel() {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(140, 15, 23, 42),
                Padding = new Padding(30, 20, 30, 20)
            };

            Label lblFormBaslik = new Label() {
                Text = "📋 KİRALAMA BİLGİLERİ",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Dock = DockStyle.Top,
                Height = 45,
                TextAlign = ContentAlignment.MiddleLeft
            };

            FlowLayoutPanel flowForm = new FlowLayoutPanel() {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(0, 10, 0, 0)
            };

            // Müşteri bilgileri
            AddSectionLabel(flowForm, "👤 Müşteri Bilgileri");
            txtTC = AddFormInput(flowForm, "TC Kimlik No:", 11);
            txtTC.TextChanged += TxtTC_TextChanged;
            txtAdSoyad = AddFormInput(flowForm, "Ad Soyad:");
            txtTelefon = AddFormInput(flowForm, "Telefon:");

            // Tarih
            AddSectionLabel(flowForm, "📅 Kiralama Dönemi");
            Label lblBas = new Label() { Text = "Başlangıç Tarihi:", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = UIHelper.TextSecondary, BackColor = Color.Transparent, AutoSize = true, Margin = new Padding(0, 5, 0, 2) };
            dtpBaslangic = new DateTimePicker() { Width = 430, Font = new Font("Segoe UI", 11), Format = DateTimePickerFormat.Short };
            Label lblBit = new Label() { Text = "Bitiş Tarihi:", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = UIHelper.TextSecondary, BackColor = Color.Transparent, AutoSize = true, Margin = new Padding(0, 5, 0, 2) };
            dtpBitis = new DateTimePicker() { Width = 430, Font = new Font("Segoe UI", 11), Format = DateTimePickerFormat.Short, Value = DateTime.Now.AddDays(3) };
            dtpBaslangic.ValueChanged += (s, e) => HesaplaTutar();
            dtpBitis.ValueChanged += (s, e) => HesaplaTutar();
            UIHelper.StyleModernInput(dtpBaslangic);
            UIHelper.StyleModernInput(dtpBitis);
            flowForm.Controls.AddRange(new Control[] { lblBas, dtpBaslangic, lblBit, dtpBitis });

            // Sigorta
            AddSectionLabel(flowForm, "🛡️ Güvenlik Paketi");
            rbStandart = new RadioButton() { Text = "Standart Sigorta (Ücretsiz)", Checked = true, AutoSize = true, Font = new Font("Segoe UI", 10), ForeColor = Color.White, BackColor = Color.Transparent };
            rbFullKasko = new RadioButton() { Text = "Full Kasko (+500 TL/Gün)", AutoSize = true, Font = new Font("Segoe UI", 10), ForeColor = Color.White, BackColor = Color.Transparent };
            rbStandart.CheckedChanged += (s, e) => HesaplaTutar();
            rbFullKasko.CheckedChanged += (s, e) => HesaplaTutar();
            flowForm.Controls.Add(rbStandart);
            flowForm.Controls.Add(rbFullKasko);

            // Ekstra hizmetler
            AddSectionLabel(flowForm, "✨ Ek Hizmetler");
            cbCocukKoltugu = new CheckBox() { Text = "Çocuk Koltuğu (+200 TL)", AutoSize = true, Font = new Font("Segoe UI", 10), ForeColor = Color.White, BackColor = Color.Transparent };
            cbGPS = new CheckBox() { Text = "Navigasyon / GPS (+150 TL)", AutoSize = true, Font = new Font("Segoe UI", 10), ForeColor = Color.White, BackColor = Color.Transparent };
            cbEkSurucu = new CheckBox() { Text = "Ek Sürücü (+300 TL)", AutoSize = true, Font = new Font("Segoe UI", 10), ForeColor = Color.White, BackColor = Color.Transparent };
            cbCocukKoltugu.CheckedChanged += (s, e) => HesaplaTutar();
            cbGPS.CheckedChanged += (s, e) => HesaplaTutar();
            cbEkSurucu.CheckedChanged += (s, e) => HesaplaTutar();
            flowForm.Controls.Add(cbCocukKoltugu);
            flowForm.Controls.Add(cbGPS);
            flowForm.Controls.Add(cbEkSurucu);

            // Ödeme
            AddSectionLabel(flowForm, "💳 Ödeme Yöntemi");
            cmbOdeme = new ComboBox() { Width = 430, Font = new Font("Segoe UI", 11), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbOdeme.Items.AddRange(new string[] { "Kredi Kartı", "Nakit (Ofiste)", "Banka Havalesi" });
            cmbOdeme.SelectedIndex = 0;
            UIHelper.StyleModernInput(cmbOdeme);
            flowForm.Controls.Add(cmbOdeme);

            // Alt panel - Toplam fiyat ve buton
            Panel pnlAlt = new Panel() {
                Dock = DockStyle.Bottom,
                Height = 130,
                BackColor = Color.Transparent,
                Padding = new Padding(30, 10, 30, 10)
            };

            lblToplamFiyat = new Label() {
                Text = "0,00 TL",
                Font = new Font("Segoe UI", 28, FontStyle.Bold),
                ForeColor = Color.FromArgb(220, 38, 38),
                Dock = DockStyle.Top,
                Height = 50,
                TextAlign = ContentAlignment.MiddleCenter
            };

            btnKirala = new Button() {
                Text = "✅ KİRALAMAYI ONAYLA",
                Dock = DockStyle.Bottom,
                Height = 55,
                BackColor = Color.FromArgb(16, 185, 129),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnKirala.FlatAppearance.BorderSize = 0;
            btnKirala.Click += BtnKirala_Click;

            pnlAlt.Controls.Add(lblToplamFiyat);
            pnlAlt.Controls.Add(btnKirala);

            pnlSag.Controls.Add(flowForm);
            pnlSag.Controls.Add(lblFormBaslik);
            pnlSag.Controls.Add(pnlAlt);

            this.Controls.Add(pnlSag);
            this.Controls.Add(pnlSol);

            HesaplaTutar();
        }

        private string GetSegmentText(IDataRecord row)
        {
            string yakit = row["Yakit"]?.ToString() ?? "";
            string vites = row["Vites"]?.ToString() ?? "";
            if (yakit == "Elektrik") return "🔋 Elektrikli Araç • " + vites;
            if (yakit == "Dizel") return "⛽ Dizel • " + vites;
            return "⛽ " + yakit + " • " + vites;
        }

        private void AddInfoRow(Panel parent, string label, string value, int y)
        {
            Label lbl = new Label() {
                Text = label,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(71, 85, 105),
                BackColor = Color.Transparent,
                Location = new Point(20, y),
                AutoSize = true
            };
            Label val = new Label() {
                Text = value,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Location = new Point(250, y),
                AutoSize = true
            };
            parent.Controls.Add(lbl);
            parent.Controls.Add(val);
        }

        private void AddSectionLabel(FlowLayoutPanel parent, string text)
        {
            Label lbl = new Label() {
                Text = text,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = UIHelper.AccentColor,
                BackColor = Color.Transparent,
                AutoSize = true,
                Margin = new Padding(0, 15, 0, 5)
            };
            parent.Controls.Add(lbl);
        }

        private TextBox AddFormInput(FlowLayoutPanel parent, string labelText, int maxLen = 100)
        {
            Label lbl = new Label() {
                Text = labelText,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = UIHelper.TextSecondary,
                BackColor = Color.Transparent,
                AutoSize = true,
                Margin = new Padding(0, 5, 0, 2)
            };
            TextBox txt = new TextBox() {
                Width = 430,
                MaxLength = maxLen,
                Font = new Font("Segoe UI", 12),
                BorderStyle = BorderStyle.FixedSingle
            };
            UIHelper.StyleModernInput(txt);
            parent.Controls.Add(lbl);
            parent.Controls.Add(txt);
            return txt;
        }

        private void TxtTC_TextChanged(object sender, EventArgs e)
        {
            if (txtTC.Text.Length == 11)
            {
                using (var con = DatabaseHelper.GetConnection())
                using (var cmd = new SQLiteCommand("SELECT * FROM Musteriler WHERE TCNo=@tc", con))
                {
                    cmd.Parameters.AddWithValue("@tc", txtTC.Text);
                    using (var r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            txtAdSoyad.Text = r["AdSoyad"].ToString();
                            txtTelefon.Text = r["Telefon"].ToString();
                        }
                    }
                }
            }
        }

        private void HesaplaTutar()
        {
            int gunler = (dtpBitis.Value.Date - dtpBaslangic.Value.Date).Days;
            if (gunler <= 0) gunler = 1;

            double toplam = gunlukFiyat * gunler;
            if (rbFullKasko.Checked) toplam += (500 * gunler);
            if (cbCocukKoltugu.Checked) toplam += 200;
            if (cbGPS.Checked) toplam += 150;
            if (cbEkSurucu.Checked) toplam += 300;

            lblToplamFiyat.Text = toplam.ToString("N0") + " TL";
            lblToplamFiyat.Tag = toplam;
        }

        private void BtnKirala_Click(object sender, EventArgs e)
        {
            if (txtTC.Text.Length != 11) { MessageBox.Show("Lütfen geçerli bir TC Kimlik No girin (11 haneli).", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (string.IsNullOrWhiteSpace(txtAdSoyad.Text)) { MessageBox.Show("Lütfen Ad Soyad alanını doldurun.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (string.IsNullOrWhiteSpace(txtTelefon.Text)) { MessageBox.Show("Lütfen Telefon alanını doldurun.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (dtpBitis.Value.Date <= dtpBaslangic.Value.Date) { MessageBox.Show("Bitiş tarihi, başlangıç tarihinden sonra olmalıdır.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            try
            {
                using (var con = DatabaseHelper.GetConnection())
                {
                    // Müşteri bul veya oluştur
                    long musteriId = 0;
                    using (var cmd = new SQLiteCommand("SELECT Id FROM Musteriler WHERE TCNo=@t", con))
                    {
                        cmd.Parameters.AddWithValue("@t", txtTC.Text);
                        object res = cmd.ExecuteScalar();
                        if (res == null)
                        {
                            using (var ins = new SQLiteCommand("INSERT INTO Musteriler (TCNo, AdSoyad, Telefon, EhliyetNo, Adres) VALUES (@1,@2,@3,'','')", con))
                            {
                                ins.Parameters.AddWithValue("@1", txtTC.Text);
                                ins.Parameters.AddWithValue("@2", txtAdSoyad.Text);
                                ins.Parameters.AddWithValue("@3", txtTelefon.Text);
                                ins.ExecuteNonQuery();
                            }
                            musteriId = (long)new SQLiteCommand("SELECT last_insert_rowid()", con).ExecuteScalar();
                        }
                        else musteriId = Convert.ToInt64(res);
                    }

                    // Extras & Sigorta
                    string extras = "";
                    if (cbCocukKoltugu.Checked) extras += "Çocuk Koltuğu, ";
                    if (cbGPS.Checked) extras += "GPS, ";
                    if (cbEkSurucu.Checked) extras += "Ek Sürücü, ";
                    extras = extras.TrimEnd(' ', ',');
                    string sigorta = rbFullKasko.Checked ? "Full Kasko" : "Standart";

                    // Kiralama kaydı
                    using (var cmdK = new SQLiteCommand(@"INSERT INTO Kiralamalar 
                        (MusteriId, AracId, BaslangicTarihi, BitisTarihi, ToplamTutar, Durum, SigortaTipi, EkstraHizmetler, OdemeYontemi) 
                        VALUES (@a,@b,@c,@d,@e,'Aktif',@f,@g,@h); 
                        UPDATE Araclar SET Durum='Dolu' WHERE Id=@b;", con))
                    {
                        cmdK.Parameters.AddWithValue("@a", musteriId);
                        cmdK.Parameters.AddWithValue("@b", aracId);
                        cmdK.Parameters.AddWithValue("@c", dtpBaslangic.Value.ToString("yyyy-MM-dd"));
                        cmdK.Parameters.AddWithValue("@d", dtpBitis.Value.ToString("yyyy-MM-dd"));
                        cmdK.Parameters.AddWithValue("@e", lblToplamFiyat.Tag ?? 0);
                        cmdK.Parameters.AddWithValue("@f", sigorta);
                        cmdK.Parameters.AddWithValue("@g", extras);
                        cmdK.Parameters.AddWithValue("@h", cmbOdeme.SelectedItem.ToString());
                        cmdK.ExecuteNonQuery();
                    }
                }

                MessageBox.Show(
                    $"✅ Rezervasyon başarıyla oluşturuldu!\n\n" +
                    $"Araç: {aracAdi}\n" +
                    $"Tarih: {dtpBaslangic.Value:dd.MM.yyyy} - {dtpBitis.Value:dd.MM.yyyy}\n" +
                    $"Toplam: {lblToplamFiyat.Text}\n\n" +
                    $"Keyifli sürüşler dileriz! 🚗",
                    "Kiralama Onayı",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kiralama sırasında bir hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
