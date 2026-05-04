using System;
using System.Drawing;
using System.Windows.Forms;

namespace AracKiralamaOtomasyonu
{
    public partial class AracKiralamaForm : Form
    {
        public AracKiralamaForm()
        {
            InitializeComponent();
            UIHelper.ApplyModernBackground(this);
            UIHelper.ApplyShadow(pnlContent);

            // GALERİYİ KEŞFET butonu → mevcut MusteriKiralamaForm açılır
            btnMusteriPaneli.Click += (s, e) =>
            {
                new MusteriKiralamaForm().ShowDialog();
            };

            // YÖNETİCİ GİRİŞİ butonu → mevcut GirisForm açılır
            btnYoneticiPaneli.Click += (s, e) =>
            {
                this.Hide();
                GirisForm gf = new GirisForm();
                gf.FormClosed += (sender, args) => this.Show();
                gf.Show();
            };
        }
    }
}
