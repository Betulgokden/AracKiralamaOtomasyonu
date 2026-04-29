using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AracKiralamaOtomasyonu
{
    public class GpsRadarForm : Form
    {
        private Timer timer;
        private float angle = 0;
        private Random rnd = new Random();
        private PointF[] vehicles;
        private string[] vehiclePlates;

        public GpsRadarForm()
        {
            this.Text = "SİSTEM - Canlı GPS Radar Takibi (Artvin Bölgesi)";
            this.Size = new Size(900, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(10, 15, 20);
            this.DoubleBuffered = true;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // Rastgele 15 araç lokasyonu ve sahte plakalar oluştur
            vehicles = new PointF[15];
            vehiclePlates = new string[15];
            for (int i = 0; i < 15; i++)
            {
                // Radar dairesi içine düşecek şekilde ayarla
                double a = rnd.NextDouble() * 2 * Math.PI;
                double r = rnd.NextDouble() * 250;
                vehicles[i] = new PointF((float)(450 + r * Math.Cos(a)), (float)(350 + r * Math.Sin(a)));
                vehiclePlates[i] = "08 " + (char)rnd.Next(65, 90) + (char)rnd.Next(65, 90) + " " + rnd.Next(100, 999);
            }

            timer = new Timer() { Interval = 30 };
            timer.Tick += (s, e) => {
                angle += 2.5f;
                if (angle >= 360) angle = 0;
                
                // Araçları hafifçe hareket ettir (Canlılık hissi)
                for (int i = 0; i < 15; i++) {
                    vehicles[i].X += (float)(rnd.NextDouble() * 1.5 - 0.75);
                    vehicles[i].Y += (float)(rnd.NextDouble() * 1.5 - 0.75);
                }
                this.Invalidate();
            };
            timer.Start();

            // Kapatma Butonu
            Button btnKapat = new Button() { Text = "Sistemi Kapat", Location = new Point(740, 600), Size = new Size(120, 40), BackColor = Color.IndianRed, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand };
            btnKapat.FlatAppearance.BorderSize = 0;
            btnKapat.Click += (s, e) => this.Close();
            this.Controls.Add(btnKapat);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int cx = this.Width / 2;
            int cy = this.Height / 2;
            int radius = 280;

            // Arka Plan Grid
            using (Pen gridPen = new Pen(Color.FromArgb(30, 0, 255, 0), 1))
            {
                for (int i = 0; i < this.Width; i += 40) g.DrawLine(gridPen, i, 0, i, this.Height);
                for (int i = 0; i < this.Height; i += 40) g.DrawLine(gridPen, 0, i, this.Width, i);
            }

            // Radar Halkaları
            using (Pen radarPen = new Pen(Color.FromArgb(80, 0, 255, 0), 2))
            {
                for (int i = 1; i <= 4; i++)
                {
                    int r = (radius / 4) * i;
                    g.DrawEllipse(radarPen, cx - r, cy - r, r * 2, r * 2);
                }
                g.DrawLine(radarPen, cx, cy - radius, cx, cy + radius);
                g.DrawLine(radarPen, cx - radius, cy, cx + radius, cy);
            }

            // Tarama Çizgisi (Sweeping Line)
            double rad = angle * Math.PI / 180.0;
            int endX = cx + (int)(radius * Math.Cos(rad));
            int endY = cy + (int)(radius * Math.Sin(rad));
            g.DrawLine(new Pen(Color.Lime, 3), cx, cy, endX, endY);

            // Radar Tarama Efekti (Gradient Pie)
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddPie(cx - radius, cy - radius, radius * 2, radius * 2, angle - 45, 45);
                using (PathGradientBrush pgb = new PathGradientBrush(path))
                {
                    pgb.CenterPoint = new PointF(cx, cy);
                    pgb.CenterColor = Color.FromArgb(100, 0, 255, 0);
                    pgb.SurroundColors = new Color[] { Color.Transparent };
                    g.FillPath(pgb, path);
                }
            }

            // Araçları Çiz
            Brush inactiveBrush = new SolidBrush(Color.FromArgb(150, 0, 200, 0));
            Brush activeBrush = new SolidBrush(Color.Lime);
            Brush alertBrush = new SolidBrush(Color.Red);

            for (int i = 0; i < vehicles.Length; i++)
            {
                double vAngle = Math.Atan2(vehicles[i].Y - cy, vehicles[i].X - cx) * 180 / Math.PI;
                if (vAngle < 0) vAngle += 360;

                // Tarayıcı çizgi aracın üzerindeyse parlat
                bool isActive = Math.Abs(vAngle - angle) < 15 || Math.Abs(vAngle - angle) > 345;
                Brush b = isActive ? activeBrush : inactiveBrush;
                
                // Rastgele 1 araç hız sınırını aşmış gibi kırmızı yansın
                if (i == 3) b = alertBrush;

                g.FillEllipse(b, vehicles[i].X - 5, vehicles[i].Y - 5, 10, 10);
                
                if (isActive || i == 3) {
                    string info = i == 3 ? $"{vehiclePlates[i]} (HIZ İHLALİ)" : $"{vehiclePlates[i]} (72km/h)";
                    g.DrawString(info, new Font("Consolas", 9, FontStyle.Bold), Brushes.White, vehicles[i].X + 10, vehicles[i].Y - 5);
                }
            }

            // Metinler
            g.DrawString("ARTVİN BÖLGESİ - CANLI UYDU TAKİBİ", new Font("Segoe UI", 18, FontStyle.Bold), Brushes.Lime, 20, 20);
            g.DrawString("GÖREV: BÖLGESEL ARAÇ TARAMASI\nDURUM: AKTİF BAĞLANTI", new Font("Consolas", 10), Brushes.White, 20, 60);
            
            g.DrawString($"SİNYAL AÇISI: {angle:0.0}°", new Font("Consolas", 12), Brushes.Lime, 20, 620);
        }
    }
}
