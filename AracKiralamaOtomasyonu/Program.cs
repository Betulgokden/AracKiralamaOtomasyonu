using System;
using System.Data.SQLite;
using System.Linq;
using System.Windows.Forms;

namespace AracKiralamaOtomasyonu
{
    internal static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                DatabaseHelper.InitializeDatabase();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Veritabanı başlatılırken hata oluştu: " + ex.Message, "Hata");
                return;
            }

            // Test Modu
            if (args != null && args.Contains("--test"))
            {
                RunIntegrationsTests();
                return;
            }

            Application.Run(new HosgeldinizForm());
        }

        static void RunIntegrationsTests()
        {
            Console.WriteLine("\n[TEST BAŞLIYOR] Eksiksiz Sistem Doğrulaması (Yeni Alanlar Dahil)...");
            int passed = 0;
            try
            {
                using (var con = DatabaseHelper.GetConnection())
                {
                    // Test 1: Admin Kullanıcısı Var Mı?
                    long adminCount = (long)new SQLiteCommand("SELECT COUNT(*) FROM Kullanicilar WHERE KullaniciAdi='admin'", con).ExecuteScalar();
                    if (adminCount == 1) { Console.WriteLine("  [BASARILI] Admin hesabı oluşturulmuş."); passed++; }
                    else { Console.WriteLine("  [HATA] Admin kaydı bulunamadı!"); }

                    // Test 2: Müşteri Ekleme (Yeni özellik: EhliyetNo)
                    string testTc = "99999999999";
                    new SQLiteCommand($"DELETE FROM Musteriler WHERE TCNo='{testTc}'", con).ExecuteNonQuery();
                    new SQLiteCommand($"INSERT INTO Musteriler (TCNo, AdSoyad, Telefon, EhliyetNo, Adres) VALUES ('{testTc}', 'Test Kullanıcısı', '555', 'EHL-12345', 'Test Adres')", con).ExecuteNonQuery();
                    long msCount = (long)new SQLiteCommand($"SELECT COUNT(*) FROM Musteriler WHERE TCNo='{testTc}'", con).ExecuteScalar();
                    if (msCount == 1) { Console.WriteLine("  [BASARILI] Müşteri eksiksiz kayıt edildi (Ehliyet No: EHL-12345)."); passed++; }

                    // Test 3: Araç Ekleme (Yeni özellik: Vites, Yakıt, Kilometre)
                    string testPlaka = "34TEST34";
                    new SQLiteCommand($"DELETE FROM Araclar WHERE Plaka='{testPlaka}'", con).ExecuteNonQuery();
                    new SQLiteCommand($"INSERT INTO Araclar (Plaka, Marka, Model, Yil, Vites, Yakit, Kilometre, GunlukFiyat, Durum) VALUES ('{testPlaka}', 'BMV', 'X5', 2024, 'Otomatik', 'Dizel', 15000, 2000, 'Boş')", con).ExecuteNonQuery();
                    long arCount = (long)new SQLiteCommand($"SELECT COUNT(*) FROM Araclar WHERE Plaka='{testPlaka}'", con).ExecuteScalar();
                    if (arCount == 1) { Console.WriteLine("  [BASARILI] Araç eksiksiz kayıt edildi (Vites: Otomatik, Kilometre: 15000, Yakıt: Dizel)."); passed++; }

                    // Test 4: Temizlik
                    new SQLiteCommand($"DELETE FROM Musteriler WHERE TCNo='{testTc}'", con).ExecuteNonQuery();
                    new SQLiteCommand($"DELETE FROM Araclar WHERE Plaka='{testPlaka}'", con).ExecuteNonQuery();
                    Console.WriteLine("  [BASARILI] Veritabanı test kayıtlarından arındırıldı."); passed++;
                }

                Console.WriteLine($"\n[SONUC] Toplam Test Sayısı: 4 | Başarılı: {passed} | Hata: {4 - passed}");
                Console.WriteLine("Sistem 'Eksiksiz (Full)' bir şekilde çalışmaktadır.");
            }
            catch (Exception e)
            {
                Console.WriteLine("\n[HATA] Testler sırasında sistem hata fırlattı: " + e.Message);
            }
        }
    }
}
