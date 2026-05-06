using System;
using System.Drawing;
using System.Windows.Forms;

namespace AracKiralamaOtomasyonu
{
    public class GorselProgramlamaDersForm : Form
    {
        private FlowLayoutPanel pnlMain;

        /// <summary>
        /// Görsel Programlama dersi için hazırlanan, modern Glassmorphism tasarıma sahip 
        /// form bileşenleri rehber formu.
        /// </summary>
        public GorselProgramlamaDersForm()
        {
            this.Text = "Görsel Programlama - Bileşen Tasarım Rehberi";
            this.Size = new Size(1200, 850);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 10);
            this.DoubleBuffered = true;
            UIHelper.ApplyModernBackground(this);

            InitializeComponents();
        }

        private void InitializeComponents()
        {
            // ===== TOP HEADER =====
            Panel pnlHeader = new Panel() {
                Dock = DockStyle.Top, Height = 90,
                BackColor = Color.FromArgb(200, 15, 23, 42)
            };
            UIHelper.ApplyShadow(pnlHeader);

            Label lblIcon = new Label() {
                Text = "🖥️", Font = new Font("Segoe UI", 26),
                ForeColor = UIHelper.AccentColor, BackColor = Color.Transparent,
                Location = new Point(30, 15), AutoSize = true
            };
            Label lblTitle = new Label() {
                Text = "GÖRSEL PROGRAMLAMA — WinForms Bileşen Rehberi",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White, BackColor = Color.Transparent,
                Location = new Point(80, 18), AutoSize = true
            };
            Label lblSub = new Label() {
                Text = "Tüm temel UI bileşenlerinin modern dark-theme görünümü ve kullanım örnekleri.",
                Font = new Font("Segoe UI", 9), ForeColor = UIHelper.TextSecondary,
                BackColor = Color.Transparent, Location = new Point(82, 55), AutoSize = true
            };
            pnlHeader.Controls.AddRange(new Control[] { lblIcon, lblTitle, lblSub });

            // ===== SCROLLABLE CONTENT =====
            pnlMain = new FlowLayoutPanel() {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(40, 30, 40, 40),
                BackColor = Color.Transparent
            };

            // --- 1. ETİKETLER ---
            AddSection("1. Etiketler (Labels)", "Bilgi vermek veya metin göstermek için kullanılır.");
            pnlMain.Controls.Add(CreateDemoCard(() => {
                var p = new Panel() { Size = new Size(1050, 90), BackColor = Color.Transparent };
                p.Controls.Add(new Label() { Text = "Ana Başlık Etiketi", Font = new Font("Segoe UI", 22, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, Location = new Point(0, 5) });
                p.Controls.Add(new Label() { Text = "Vurgulu Alt Başlık (Italic)", Font = new Font("Segoe UI", 13, FontStyle.Italic), ForeColor = UIHelper.TextSecondary, AutoSize = true, Location = new Point(0, 45) });
                Label lblLink = new Label() { Text = "🔗 Tıklanabilir Link Etiketi", ForeColor = UIHelper.AccentColor, Font = new Font("Segoe UI", 10, FontStyle.Underline), Cursor = Cursors.Hand, AutoSize = true, Location = new Point(400, 50) };
                lblLink.Click += (s, e) => MessageBox.Show("Linke tıklandı!");
                p.Controls.Add(lblLink);
                return p;
            }));

            // --- 2. BUTONLAR ---
            AddSection("2. Butonlar (Buttons)", "Kullanıcı etkileşimi ve komut çalıştırmak için kullanılır.");
            pnlMain.Controls.Add(CreateDemoCard(() => {
                var p = new Panel() { Size = new Size(1050, 75), BackColor = Color.Transparent };
                int x = 0;
                foreach (var (txt, col) in new[] {
                    ("✅  Başarılı İşlem", Color.FromArgb(16, 185, 129)),
                    ("🔵  Primer Buton", Color.FromArgb(37, 99, 235)),
                    ("⚠️  Uyarı", Color.FromArgb(245, 158, 11)),
                    ("❌  Kritik Hata", Color.FromArgb(220, 38, 38)),
                    ("⚙️  Standart", Color.FromArgb(71, 85, 105))
                }) {
                    Button btn = CreateButton(txt, col);
                    btn.Location = new Point(x, 0);
                    p.Controls.Add(btn);
                    x += 205;
                }
                return p;
            }));

            // --- 3. GİRİŞ ALANLARI ---
            AddSection("3. Giriş Alanları (Inputs)", "Kullanıcıdan veri almak için kullanılır.");
            pnlMain.Controls.Add(CreateDemoCard(() => {
                var p = new Panel() { Size = new Size(1050, 105), BackColor = Color.Transparent };
                AddFieldLabel(p, "TextBox:", new Point(0, 10));
                TextBox txt = new TextBox() { Width = 220, Location = new Point(95, 7), Text = "Örnek metin..." };
                UIHelper.StyleModernInput(txt);
                p.Controls.Add(txt);

                AddFieldLabel(p, "ComboBox:", new Point(340, 10));
                ComboBox cmb = new ComboBox() { Width = 200, Location = new Point(435, 7), DropDownStyle = ComboBoxStyle.DropDownList };
                cmb.Items.AddRange(new string[] { "Seçenek A", "Seçenek B", "Seçenek C" });
                cmb.SelectedIndex = 0;
                UIHelper.StyleModernInput(cmb);
                p.Controls.Add(cmb);

                AddFieldLabel(p, "Şifre Kutusu:", new Point(660, 10));
                TextBox txtPass = new TextBox() { Width = 200, Location = new Point(775, 7), PasswordChar = '●' };
                UIHelper.StyleModernInput(txtPass);
                p.Controls.Add(txtPass);

                AddFieldLabel(p, "DatePicker:", new Point(0, 65));
                DateTimePicker dtp = new DateTimePicker() { Width = 220, Location = new Point(95, 62), Format = DateTimePickerFormat.Short };
                UIHelper.StyleModernInput(dtp);
                p.Controls.Add(dtp);

                return p;
            }));

            // --- 4. SEÇİM ARAÇLARI ---
            AddSection("4. Seçim Araçları", "Evet/Hayır veya çoklu seçenek sunmak için kullanılır.");
            pnlMain.Controls.Add(CreateDemoCard(() => {
                var p = new Panel() { Size = new Size(1050, 70), BackColor = Color.Transparent };
                CheckBox chk = new CheckBox() { Text = "✅ Beni Hatırla", Checked = true, AutoSize = true, Font = new Font("Segoe UI", 11), ForeColor = Color.White, BackColor = Color.Transparent, Location = new Point(0, 20) };
                RadioButton rb1 = new RadioButton() { Text = "Seçenek 1", Checked = true, AutoSize = true, Font = new Font("Segoe UI", 11), ForeColor = Color.White, BackColor = Color.Transparent, Location = new Point(200, 20) };
                RadioButton rb2 = new RadioButton() { Text = "Seçenek 2", AutoSize = true, Font = new Font("Segoe UI", 11), ForeColor = Color.White, BackColor = Color.Transparent, Location = new Point(360, 20) };
                RadioButton rb3 = new RadioButton() { Text = "Seçenek 3", AutoSize = true, Font = new Font("Segoe UI", 11), ForeColor = Color.White, BackColor = Color.Transparent, Location = new Point(520, 20) };
                p.Controls.AddRange(new Control[] { chk, rb1, rb2, rb3 });
                return p;
            }));

            // --- 5. İLERLEME =====
            AddSection("5. İlerleme ve Durum (Progress)", "İşlemlerin yüzdesini göstermek için kullanılır.");
            pnlMain.Controls.Add(CreateDemoCard(() => {
                var p = new Panel() { Size = new Size(1050, 80), BackColor = Color.Transparent };
                for (int i = 0; i < 3; i++) {
                    int val = (i + 1) * 33;
                    Color barColor = i == 0 ? UIHelper.SuccessColor : i == 1 ? UIHelper.AccentColor : Color.FromArgb(245, 158, 11);
                    AddFieldLabel(p, $"{val}%", new Point(0, 5 + i * 28));
                    ProgressBar pb = new ProgressBar() { Width = 700, Height = 18, Value = val, Location = new Point(60, 7 + i * 28) };
                    p.Controls.Add(pb);
                }
                return p;
            }));

            // --- 6. GRUPLAMA ---
            AddSection("6. Gruplama (GroupBox & Panel)", "Benzer bileşenleri bir arada tutar.");
            pnlMain.Controls.Add(CreateDemoCard(() => {
                var p = new Panel() { Size = new Size(1050, 130), BackColor = Color.Transparent };
                Panel innerCard = new Panel() { Location = new Point(0, 5), Size = new Size(500, 110), BackColor = Color.FromArgb(100, 30, 41, 59) };
                UIHelper.ApplyShadow(innerCard);
                innerCard.Controls.Add(new Label() { Text = "👤  Kullanıcı Grubu Panel Örneği", Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = UIHelper.AccentColor, AutoSize = true, Location = new Point(15, 12), BackColor = Color.Transparent });
                AddFieldLabel(innerCard, "Ad:", new Point(15, 50));
                TextBox tName = new TextBox() { Location = new Point(55, 47), Width = 150 };
                UIHelper.StyleModernInput(tName);
                AddFieldLabel(innerCard, "Soyad:", new Point(225, 50));
                TextBox tLast = new TextBox() { Location = new Point(280, 47), Width = 150 };
                UIHelper.StyleModernInput(tLast);
                innerCard.Controls.AddRange(new Control[] { tName, tLast });
                p.Controls.Add(innerCard);
                return p;
            }));

            this.Controls.Add(pnlMain);
            this.Controls.Add(pnlHeader);
        }

        private void AddSection(string title, string description)
        {
            Panel p = new Panel() { Width = 1080, Height = 70, Margin = new Padding(0, 30, 0, 8), BackColor = Color.Transparent };
            pnlMain.SetFlowBreak(p, true);

            // Accent line
            Panel line = new Panel() { Location = new Point(0, 0), Size = new Size(4, 60), BackColor = UIHelper.AccentColor };
            Label lblTitle = new Label() { Text = title, Font = new Font("Segoe UI", 15, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, Location = new Point(18, 0) };
            Label lblDesc = new Label() { Text = description, Font = new Font("Segoe UI", 10), ForeColor = UIHelper.TextSecondary, AutoSize = true, Location = new Point(18, 35) };

            p.Controls.AddRange(new Control[] { line, lblTitle, lblDesc });
            pnlMain.Controls.Add(p);
        }

        private Panel CreateDemoCard(Func<Panel> contentFactory)
        {
            Panel wrap = new Panel() { Width = 1080, BackColor = Color.FromArgb(140, 15, 23, 42), Margin = new Padding(0, 0, 0, 20), Padding = new Padding(25, 20, 25, 20) };
            UIHelper.ApplyShadow(wrap);
            pnlMain.SetFlowBreak(wrap, true);
            Panel content = contentFactory();
            wrap.Height = content.Height + 45;
            content.Location = new Point(25, 15);
            wrap.Controls.Add(content);
            return wrap;
        }

        private static void AddFieldLabel(Panel parent, string text, Point loc)
        {
            parent.Controls.Add(new Label() { Text = text, Location = loc, AutoSize = true, ForeColor = UIHelper.TextSecondary, BackColor = Color.Transparent, Font = new Font("Segoe UI", 10) });
        }

        private Button CreateButton(string text, Color backColor)
        {
            Button btn = new Button() {
                Text = text, Size = new Size(190, 48),
                BackColor = backColor, ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.MouseEnter += (s, e) => btn.BackColor = ControlPaint.Light(backColor, 0.15f);
            btn.MouseLeave += (s, e) => btn.BackColor = backColor;
            return btn;
        }
    }
}
