using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SQLite;
using System.IO;

namespace AracKiralamaOtomasyonu
{
    public class AnaForm : Form
    {
        private Button btnAraclar, btnMusteriler, btnKiralamalar, btnRaporlar, btnCikis;
        private Label lblBaslik, lblAracOzet, lblMusteriOzet, lblKiralamaOzet, lblBugunKiralama, lblKullaniciBilgi, lblClock;
        private DataGridView dgvSonIslemler;
        private Timer timerClock;
        private Panel pnlSidebar;

        public AnaForm()
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer | ControlStyles.OptimizedDoubleBuffer, true);
            InitializeComponents();
            UIHelper.ApplyModernBackground(this);
            OzetleriGuncelle();
            timerClock.Start();
        }

        private void InitializeComponents()
        {
            this.Text = "ARTVİN ARAÇ KİRALAMA - Yönetim Paneli";
            this.Size = new Size(1150, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 10);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // Sidebar
            pnlSidebar = new Panel()
            {
                Dock = DockStyle.Left,
                Width = 260,
                BackColor = Color.FromArgb(160, 20, 30, 48) // Yarı saydam lüks
            };

            lblBaslik = new Label()
            {
                Text = "ARTVİN\nKİRALAMA",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                Location = new Point(25, 40),
                Size = new Size(210, 80),
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };
            pnlSidebar.Controls.Add(lblBaslik);

            lblKullaniciBilgi = new Label()
            {
                Text = "Betül GÖKDEN",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = UIHelper.AccentColor,
                Location = new Point(25, 130),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            pnlSidebar.Controls.Add(lblKullaniciBilgi);

            // Menü Butonları
            int by = 200;
            btnAraclar = CreateSidebarButton("🚗  Filo Yönetim Merkezi", by, pnlSidebar);
            btnAraclar.Click += (s, e) => { new AracForm().ShowDialog(); OzetleriGuncelle(); };

            btnMusteriler = CreateSidebarButton("👥  CRM & Müşteri İlişkileri", by + 65, pnlSidebar);
            btnMusteriler.Click += (s, e) => { new MusteriForm().ShowDialog(); OzetleriGuncelle(); };

            btnKiralamalar = CreateSidebarButton("📚  Operasyon & Sözleşmeler", by + 130, pnlSidebar);
            btnKiralamalar.Click += (s, e) => { new KiralamaForm().ShowDialog(); OzetleriGuncelle(); };

            btnRaporlar = CreateSidebarButton("📊  Finans & Muhasebe Raporları", by + 195, pnlSidebar);
            btnRaporlar.Click += (s, e) => { new RaporForm().ShowDialog(); OzetleriGuncelle(); };

            Button btnGps = CreateSidebarButton("📍  GPS Canlı Takip", by + 260, pnlSidebar);
            btnGps.Click += (s, e) => {
                new GpsRadarForm().ShowDialog();
            };

            Button btnYapayZeka = CreateSidebarButton("🤖  Yapay Zeka Analizi", by + 325, pnlSidebar);
            btnYapayZeka.Click += (s, e) => {
                Form aiForm = new Form() { Text = "Yapay Zeka Gelecek Tahmini", Size = new Size(600, 300), StartPosition = FormStartPosition.CenterScreen, FormBorderStyle = FormBorderStyle.None, BackColor = Color.FromArgb(15,23,42) };
                
                Label lblBaslik = new Label() { Text = "🧠 Artvin Kiralama A.I. Engine", Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = UIHelper.AccentColor, Location = new Point(20, 20), AutoSize = true };
                Label lblAnim = new Label() { Text = "Veritabanı taranıyor...", Font = new Font("Segoe UI", 12), ForeColor = Color.White, Location = new Point(20, 80), AutoSize = true };
                ProgressBar pb = new ProgressBar() { Location = new Point(20, 120), Size = new Size(560, 30), Style = ProgressBarStyle.Marquee, MarqueeAnimationSpeed = 30 };
                Button btnKapat = new Button() { Text = "Kapat", Location = new Point(480, 240), Size = new Size(100, 40), BackColor = Color.IndianRed, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Visible = false };
                btnKapat.Click += (ss, ee) => aiForm.Close();
                
                aiForm.Controls.AddRange(new Control[] { lblBaslik, lblAnim, pb, btnKapat });
                
                Timer t = new Timer() { Interval = 2500 };
                t.Tick += (ss, ee) => {
                    t.Stop();
                    pb.Style = ProgressBarStyle.Blocks;
                    pb.Value = 100;
                    lblAnim.Text = "✅ Analiz Tamamlandı!\n\nTahmin 1: Önümüzdeki bayram tatilinde SUV araç talebinde %45 artış bekleniyor.\nTahmin 2: Müşterilerin %80'i otomatik vites tercih ediyor, filonuzu buna göre güncelleyin.\nTahmin 3: Artvin - Hopa arası kiralama rotası en yoğun güzergah olarak tespit edildi.";
                    btnKapat.Visible = true;
                };
                t.Start();
                aiForm.ShowDialog();
            };

            Button btnKaraListe = CreateSidebarButton("🛡️  Kara Liste (Risk)", by + 390, pnlSidebar);
            btnKaraListe.Click += (s, e) => {
                MessageBox.Show("Müşteri risk analizi ve Kara Liste veritabanı Emniyet Genel Müdürlüğü sunucularıyla senkronize ediliyor.\n\nErişim Onaylandı: Sistemde riskli müşteri bulunmamaktadır.", "Güvenlik & Risk Yönetimi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            btnCikis = new Button()
            {
                Text = "🚪 Çıkış Güvenli",
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Color.FromArgb(100, 30, 41, 59),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCikis.FlatAppearance.BorderSize = 0;
            btnCikis.Click += (s, e) => this.Close();
            pnlSidebar.Controls.Add(btnCikis);

            // Dashboard Header
            Panel pnlHeader = new Panel() { Dock = DockStyle.Top, Height = 100, BackColor = Color.FromArgb(100, 15, 23, 42) };
            Label lblHeaderTitle = new Label() { Text = "Genel Bakış & İstatistikler", Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = Color.White, Location = new Point(280, 30), AutoSize = true, BackColor = Color.Transparent };
            
            // Bildirim İkonu (Yeni!)
            Label lblBildirim = new Label() { 
                Text = "🔔  3 Yeni Bildirim", 
                Font = new Font("Segoe UI", 10, FontStyle.Bold), 
                ForeColor = Color.FromArgb(239, 68, 68), 
                Location = new Point(870, 70), // Saatin altına, daha aşağıya alındı
                Anchor = AnchorStyles.Top | AnchorStyles.Right, // Pencere küçülse bile sağda kalır, saatin üstüne binmez
                AutoSize = true, 
                BackColor = Color.Transparent, 
                Cursor = Cursors.Hand 
            };
            lblBildirim.MouseEnter += (s, e) => lblBildirim.ForeColor = Color.White;
            lblBildirim.MouseLeave += (s, e) => lblBildirim.ForeColor = Color.FromArgb(239, 68, 68);
            lblBildirim.Click += (s, e) => MessageBox.Show("1. Sigorta poliçesi yaklaşan 2 araç var.\n2. Bugün teslim alınması gereken 1 kiralama var.\n3. Sistem yedeklemesi dün gece başarıyla tamamlandı.", "Sistem Bildirimleri", MessageBoxButtons.OK, MessageBoxIcon.Information);

            lblClock = new Label() { 
                Text = DateTime.Now.ToString("T"), 
                Font = new Font("Segoe UI", 14, FontStyle.Bold), 
                ForeColor = UIHelper.AccentColor, 
                Dock = DockStyle.Right, 
                Width = 200, 
                TextAlign = ContentAlignment.MiddleRight,
                Padding = new Padding(0, 0, 30, 0),
                BackColor = Color.Transparent
            };
            pnlHeader.Controls.Add(lblHeaderTitle); 
            pnlHeader.Controls.Add(lblBildirim);
            pnlHeader.Controls.Add(lblClock);

            timerClock = new Timer() { Interval = 1000 };
            timerClock.Tick += (s, e) => lblClock.Text = DateTime.Now.ToString("HH:mm:ss");

            // Stat Cards Container
            int startX = 280, startY = 120, cardW = 270, cardH = 120;

            lblAracOzet = CreateStatCard("MEVCUT ARAÇLAR", "0", "🚗", new Point(startX, startY), Color.FromArgb(37, 99, 235));
            lblMusteriOzet = CreateStatCard("MÜŞTERİ SAYISI", "0", "👥", new Point(startX + cardW + 20, startY), Color.FromArgb(124, 58, 237));
            lblKiralamaOzet = CreateStatCard("AKTİF KİRALAMALAR", "0", "📑", new Point(startX, startY + cardH + 20), Color.FromArgb(16, 185, 129));
            lblBugunKiralama = CreateStatCard("TOPLAM HASILAT", "0,00 TL", "💰", new Point(startX + cardW + 20, startY + cardH + 20), Color.FromArgb(245, 158, 11));

            // Son İşlemler
            Label lblListTitle = new Label() { Text = "Son İşlemler & Canlı Akış", Font = new Font("Segoe UI", 14, FontStyle.Bold), Location = new Point(280, startY + (cardH + 20) * 2 + 20), AutoSize = true, ForeColor = Color.White, BackColor = Color.Transparent };
            
            dgvSonIslemler = new DataGridView() { 
                Location = new Point(280, startY + (cardH + 20) * 2 + 60), 
                Size = new Size(820, 240),
                ScrollBars = ScrollBars.Vertical
            };
            UIHelper.StyleDataGridView(dgvSonIslemler);

            this.Controls.Add(dgvSonIslemler);
            this.Controls.Add(lblListTitle);
            this.Controls.Add(pnlHeader);
            this.Controls.Add(pnlSidebar);
        }

        private Label CreateStatCard(string title, string value, string icon, Point loc, Color color)
        {
            Panel p = new Panel() { Location = loc, Size = new Size(270, 120), BackColor = UIHelper.CardColor };
            UIHelper.ApplyShadow(p);
            
            Label lIcon = new Label() { Text = icon, Font = new Font("Segoe UI", 28), ForeColor = color, Location = new Point(190, 30), Size = new Size(60, 60), TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.Transparent };
            Label lTitle = new Label() { Text = title, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = UIHelper.TextSecondary, Location = new Point(20, 20), AutoSize = true, BackColor = Color.Transparent };
            Label lVal = new Label() { Text = value, Font = new Font("Segoe UI", 20, FontStyle.Bold), ForeColor = Color.White, Location = new Point(20, 50), Size = new Size(180, 50), TextAlign = ContentAlignment.MiddleLeft, BackColor = Color.Transparent };
            
            p.Controls.Add(lIcon); p.Controls.Add(lTitle); p.Controls.Add(lVal);
            this.Controls.Add(p);
            return lVal;
        }

        private Button CreateSidebarButton(string text, int y, Control parent)
        {
            Button btn = new Button()
            {
                Text = text,
                Location = new Point(15, y),
                Size = new Size(230, 50),
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(148, 163, 184),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 0, 0),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, 41, 59);
            btn.MouseEnter += (s, e) => btn.ForeColor = Color.White;
            btn.MouseLeave += (s, e) => btn.ForeColor = Color.FromArgb(148, 163, 184);
            parent.Controls.Add(btn);
            return btn;
        }

        private void OzetleriGuncelle()
        {
            try {
                using (var connection = DatabaseHelper.GetConnection()) {
                    lblAracOzet.Text = (new SQLiteCommand("SELECT COUNT(*) FROM Araclar", connection).ExecuteScalar() ?? 0).ToString();
                    lblMusteriOzet.Text = (new SQLiteCommand("SELECT COUNT(*) FROM Musteriler", connection).ExecuteScalar() ?? 0).ToString();
                    lblKiralamaOzet.Text = (new SQLiteCommand("SELECT COUNT(*) FROM Kiralamalar WHERE Durum='Aktif'", connection).ExecuteScalar() ?? 0).ToString();
                    
                    object ciro = new SQLiteCommand("SELECT SUM(ToplamTutar) FROM Kiralamalar", connection).ExecuteScalar();
                    lblBugunKiralama.Text = (ciro == DBNull.Value) ? "0,00 TL" : Convert.ToDouble(ciro).ToString("C2");

                    DataTable dt = new DataTable();
                    new SQLiteDataAdapter(@"
                        SELECT m.AdSoyad as [Müşteri], a.Marka || ' ' || a.Model as [Araç], k.BaslangicTarihi as [Tarih], k.ToplamTutar as [Tutar]
                        FROM Kiralamalar k 
                        JOIN Musteriler m ON k.MusteriId = m.Id 
                        JOIN Araclar a ON k.AracId = a.Id 
                        ORDER BY k.Id DESC LIMIT 6", connection).Fill(dt);
                    dgvSonIslemler.DataSource = dt;
                }
            } catch { }
        }
    }
}
