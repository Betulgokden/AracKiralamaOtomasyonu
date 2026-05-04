using System;
using System.Drawing;
using System.Windows.Forms;

namespace AracKiralamaOtomasyonu
{
    public class TasarimRehberiForm : Form
    {
        private Panel pnlHeader;
        private FlowLayoutPanel pnlContent;

        public TasarimRehberiForm()
        {
            this.Text = "Tasarım Rehberi - Modern WinForms";
            this.Size = new Size(1100, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = UIHelper.BackgroundColor;
            this.DoubleBuffered = true;

            InitializeComponents();
        }

        private void InitializeComponents()
        {
            // --- HEADER ---
            pnlHeader = new Panel()
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.FromArgb(20, 30, 48),
                Padding = new Padding(20)
            };

            Label lblTitle = new Label()
            {
                Text = "FORM BİLEŞENLERİ REHBERİ",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 15)
            };
            pnlHeader.Controls.Add(lblTitle);

            // --- CONTENT AREA ---
            pnlContent = new FlowLayoutPanel()
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(30),
                BackColor = Color.Transparent
            };

            // 1. ETIKETLER (LABELS)
            AddSectionTitle("1. ETİKETLER (LABELS)");
            
            pnlContent.Controls.Add(new Label() { Text = "Ana Başlık Etiketi", Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = UIHelper.TextPrimary, AutoSize = true, Margin = new Padding(0, 0, 20, 10) });
            pnlContent.Controls.Add(new Label() { Text = "Alt Başlık Etiketi", Font = new Font("Segoe UI", 12, FontStyle.Italic), ForeColor = UIHelper.TextSecondary, AutoSize = true, Margin = new Padding(0, 0, 20, 10) });
            pnlContent.Controls.Add(new Label() { Text = "Vurgulu Altın Etiket", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.FromArgb(255, 215, 0), AutoSize = true, Margin = new Padding(0, 0, 20, 10) });

            // 2. BUTONLAR (BUTTONS)
            AddSectionTitle("2. BUTONLAR (BUTTONS)");

            Button btnPrimary = CreateStyledButton("PRİMER BUTON", UIHelper.AccentColor);
            Button btnSuccess = CreateStyledButton("BAŞARILI İŞLEM", UIHelper.SuccessColor);
            Button btnDanger = CreateStyledButton("KRİTİK HATA", UIHelper.DangerColor);
            
            pnlContent.Controls.Add(btnPrimary);
            pnlContent.Controls.Add(btnSuccess);
            pnlContent.Controls.Add(btnDanger);

            // 3. GİRİŞ ALANLARI (INPUTS)
            AddSectionTitle("3. GİRİŞ ALANLARI (INPUTS)");

            TextBox txtModern = new TextBox() { Width = 300, Height = 40, Margin = new Padding(0, 0, 20, 10) };
            UIHelper.StyleModernInput(txtModern);
            txtModern.Text = "Modern Yazı Alanı";
            
            pnlContent.Controls.Add(txtModern);

            ComboBox cmbModern = new ComboBox() { Width = 300, Margin = new Padding(0, 0, 20, 10) };
            cmbModern.Items.AddRange(new object[] { "Seçenek 1", "Seçenek 2", "Seçenek 3" });
            UIHelper.StyleModernInput(cmbModern);
            cmbModern.SelectedIndex = 0;
            
            pnlContent.Controls.Add(cmbModern);

            // 4. KARTLAR (CARDS)
            AddSectionTitle("4. ÖZEL KARTLAR (CUSTOM CARDS)");

            ModernCarCard card = new ModernCarCard()
            {
                Title = "Tesla Model S",
                Price = "2.500.000 TL",
                Stats = "Menzil: 650km\n0-100: 2.1s",
                Margin = new Padding(0, 0, 20, 10)
            };
            pnlContent.Controls.Add(card);

            this.Controls.Add(pnlContent);
            this.Controls.Add(pnlHeader);
        }

        private void AddSectionTitle(string title)
        {
            Label lbl = new Label()
            {
                Text = title,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = UIHelper.AccentColor,
                AutoSize = true,
                Width = 1000,
                Margin = new Padding(0, 40, 0, 20)
            };
            pnlContent.SetFlowBreak(lbl, true);
            pnlContent.Controls.Add(lbl);
        }

        private Button CreateStyledButton(string text, Color backColor)
        {
            Button btn = new Button()
            {
                Text = text,
                Size = new Size(200, 50),
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 20, 10)
            };
            btn.FlatAppearance.BorderSize = 0;
            
            btn.MouseEnter += (s, e) => btn.BackColor = ControlPaint.Light(backColor, 0.2f);
            btn.MouseLeave += (s, e) => btn.BackColor = backColor;

            return btn;
        }
    }
}
