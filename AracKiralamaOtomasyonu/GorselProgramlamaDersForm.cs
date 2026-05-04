using System;
using System.Drawing;
using System.Windows.Forms;

namespace AracKiralamaOtomasyonu
{
    /// <summary>
    /// Görsel Programlama dersi için hazırlanan, modern tasarıma sahip 
    /// form bileşenleri rehber formu.
    /// </summary>
    public class GorselProgramlamaDersForm : Form
    {
        private FlowLayoutPanel pnlMain;

        public GorselProgramlamaDersForm()
        {
            this.Text = "Görsel Programlama - Bileşen Tasarım Rehberi";
            this.Size = new Size(1200, 850);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 242, 245); // Modern açık gri arka plan
            this.Font = new Font("Segoe UI", 10);
            this.DoubleBuffered = true;

            InitializeComponents();
        }

        private void InitializeComponents()
        {
            // --- ANA PANEL ---
            pnlMain = new FlowLayoutPanel()
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(40),
                BackColor = Color.Transparent
            };

            // --- 1. ETİKETLER (LABELS) ---
            AddSection("1. Etiketler (Labels)", "Bilgi vermek veya metin göstermek için kullanılır.");
            
            pnlMain.Controls.Add(new Label() { Text = "Ana Başlık Etiketi", Font = new Font("Segoe UI", 24, FontStyle.Bold), ForeColor = Color.FromArgb(30, 41, 59), AutoSize = true, Margin = new Padding(0, 0, 30, 20) });
            pnlMain.Controls.Add(new Label() { Text = "Vurgulu Alt Başlık", Font = new Font("Segoe UI", 14, FontStyle.Italic), ForeColor = Color.FromArgb(71, 85, 105), AutoSize = true, Margin = new Padding(0, 10, 30, 20) });
            
            Label lblLink = new Label() { Text = "Tıklanabilir Link Etiketi", ForeColor = Color.Blue, Font = new Font("Segoe UI", 10, FontStyle.Underline), Cursor = Cursors.Hand, AutoSize = true };
            lblLink.Click += (s, e) => MessageBox.Show("Linke tıklandı!");
            pnlMain.Controls.Add(lblLink);

            // --- 2. BUTONLAR (BUTTONS) ---
            AddSection("2. Butonlar (Buttons)", "Kullanıcı etkileşimi ve komut çalıştırmak için kullanılır.");

            Button btnNormal = CreateButton("Standart Buton", Color.FromArgb(100, 116, 139));
            Button btnAction = CreateButton("İşlem Butonu", Color.FromArgb(37, 99, 235));
            Button btnWarning = CreateButton("Uyarı Butonu", Color.FromArgb(220, 38, 38));
            
            pnlMain.Controls.Add(btnNormal);
            pnlMain.Controls.Add(btnAction);
            pnlMain.Controls.Add(btnWarning);

            // --- 3. GİRİŞ ALANLARI (INPUTS) ---
            AddSection("3. Giriş Alanları (Inputs)", "Kullanıcıdan veri almak için kullanılır.");

            TextBox txtBasic = new TextBox() { Width = 250, Text = "Metin kutusu...", Margin = new Padding(0, 0, 20, 10) };
            pnlMain.Controls.Add(txtBasic);

            ComboBox cmb = new ComboBox() { Width = 250, Margin = new Padding(0, 0, 20, 10), DropDownStyle = ComboBoxStyle.DropDownList };
            cmb.Items.AddRange(new string[] { "Seçenek A", "Seçenek B", "Seçenek C" });
            cmb.SelectedIndex = 0;
            pnlMain.Controls.Add(cmb);

            // --- 4. SEÇİM ARAÇLARI (SELECTION) ---
            AddSection("4. Seçim Araçları", "Evet/Hayır veya çoklu seçenek sunmak için kullanılır.");

            CheckBox chk = new CheckBox() { Text = "Beni Hatırla", Checked = true, AutoSize = true, Margin = new Padding(0, 0, 20, 10) };
            pnlMain.Controls.Add(chk);

            RadioButton rb1 = new RadioButton() { Text = "Erkek", Checked = true, AutoSize = true };
            RadioButton rb2 = new RadioButton() { Text = "Kadın", AutoSize = true };
            pnlMain.Controls.Add(rb1);
            pnlMain.Controls.Add(rb2);

            // --- 5. İLERLEME VE DURUM (PROGRESS) ---
            AddSection("5. İlerleme ve Durum", "İşlemlerin yüzdesini göstermek için kullanılır.");

            ProgressBar pb = new ProgressBar() { Width = 500, Height = 25, Value = 75, Margin = new Padding(0, 10, 0, 10) };
            pnlMain.Controls.Add(pb);

            // --- 6. GRUPLAMA (GROUPING) ---
            AddSection("6. Gruplama (GroupBox)", "Benzer bileşenleri bir araya toplar.");

            GroupBox gb = new GroupBox() { Text = "Kullanıcı Bilgileri", Size = new Size(400, 150), Margin = new Padding(0, 10, 0, 10) };
            gb.Controls.Add(new Label() { Text = "Ad:", Location = new Point(20, 40), AutoSize = true });
            gb.Controls.Add(new TextBox() { Location = new Point(60, 37), Width = 200 });
            gb.Controls.Add(new Label() { Text = "Soyad:", Location = new Point(20, 80), AutoSize = true });
            gb.Controls.Add(new TextBox() { Location = new Point(60, 77), Width = 200 });
            pnlMain.Controls.Add(gb);

            this.Controls.Add(pnlMain);
        }

        private void AddSection(string title, string description)
        {
            Panel p = new Panel() { Width = 1000, Height = 80, Margin = new Padding(0, 30, 0, 10) };
            
            Label lblTitle = new Label()
            {
                Text = title,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                AutoSize = true,
                Location = new Point(0, 0)
            };
            
            Label lblDesc = new Label()
            {
                Text = description,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(100, 116, 139),
                AutoSize = true,
                Location = new Point(0, 35)
            };

            p.Controls.Add(lblTitle);
            p.Controls.Add(lblDesc);
            
            pnlMain.SetFlowBreak(p, true);
            pnlMain.Controls.Add(p);
        }

        private Button CreateButton(string text, Color backColor)
        {
            Button btn = new Button()
            {
                Text = text,
                Size = new Size(180, 50),
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 20, 10)
            };
            btn.FlatAppearance.BorderSize = 0;
            
            // Hover efekti
            btn.MouseEnter += (s, e) => btn.BackColor = ControlPaint.Light(backColor, 0.2f);
            btn.MouseLeave += (s, e) => btn.BackColor = backColor;

            return btn;
        }
    }
}
