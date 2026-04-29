using System;
using System.Data.SQLite;
using System.IO;

namespace AracKiralamaOtomasyonu
{
    public static class DatabaseHelper
    {
        private static string dbFile = "AracKiralamaOtomasyonu.db";
        public static string ConnectionString = $"Data Source={dbFile};Version=3;";

        public static void InitializeDatabase()
        {
            if (!File.Exists(dbFile))
            {
                SQLiteConnection.CreateFile(dbFile);
            }

            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                // Kullanıcılar (Users)
                string createKullanicilar = @"
                    CREATE TABLE IF NOT EXISTS Kullanicilar (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        KullaniciAdi TEXT NOT NULL UNIQUE,
                        Sifre TEXT NOT NULL,
                        Rol TEXT NOT NULL
                    );";

                // Müşteriler (Customers)
                string createMusteriler = @"
                    CREATE TABLE IF NOT EXISTS Musteriler (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        TCNo TEXT NOT NULL UNIQUE,
                        AdSoyad TEXT NOT NULL,
                        Telefon TEXT NOT NULL,
                        EhliyetNo TEXT NOT NULL,
                        Adres TEXT
                    );";

                // Araçlar (Vehicles) - Teknik Özellikler eklendi
                string createAraclar = @"
                    CREATE TABLE IF NOT EXISTS Araclar (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Plaka TEXT NOT NULL UNIQUE,
                        Marka TEXT NOT NULL,
                        Model TEXT NOT NULL,
                        Yil INTEGER NOT NULL,
                        Vites TEXT NOT NULL,
                        Yakit TEXT NOT NULL,
                        Kilometre INTEGER NOT NULL,
                        GunlukFiyat REAL NOT NULL,
                        Durum TEXT NOT NULL, -- 'Boş', 'Dolu'
                        ResimYolu TEXT,
                        Guc INTEGER DEFAULT 150, -- Beygir Gücü
                        Hizlanma REAL DEFAULT 8.5, -- 0-100 km/h
                        KoltukSayisi INTEGER DEFAULT 5
                    );";

                // Kiralamalar (Rentals) - Ek Özellikler eklendi
                string createKiralamalar = @"
                    CREATE TABLE IF NOT EXISTS Kiralamalar (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        MusteriId INTEGER NOT NULL,
                        AracId INTEGER NOT NULL,
                        BaslangicTarihi TEXT NOT NULL,
                        BitisTarihi TEXT NOT NULL,
                        ToplamTutar REAL NOT NULL,
                        Durum TEXT NOT NULL, -- 'Aktif', 'Tamamlandı'
                        SigortaTipi TEXT,
                        EkstraHizmetler TEXT,
                        OdemeYontemi TEXT,
                        FOREIGN KEY(MusteriId) REFERENCES Musteriler(Id),
                        FOREIGN KEY(AracId) REFERENCES Araclar(Id)
                    );";

                ExecuteCommand(createKullanicilar, connection);
                ExecuteCommand(createMusteriler, connection);
                ExecuteCommand(createAraclar, connection);
                ExecuteCommand(createKiralamalar, connection);

                // Kolon Kontrolü (Migration)
                PerformMigration(connection);
                
                // --- ULTIMATE IMAGE FIX: FORCE SYNC ---
                FixImagePaths(connection);

                SeedAdminUser(connection);
                SeedSampleData(connection);
                UpdateArtvinData(connection);
            }
        }

        private static void FixImagePaths(SQLiteConnection connection)
        {
            // We just ensure ResimYolu is set to the default pattern if empty.
            // ImageService will handle the actual logic dynamically.
            using (var cmd = new SQLiteCommand("UPDATE Araclar SET ResimYolu = 'Assets\\' || LOWER(REPLACE(Marka, ' ', '')) || '.png' WHERE ResimYolu IS NULL OR ResimYolu = ''", connection))
            {
                cmd.ExecuteNonQuery();
            }
        }

        private static void PerformMigration(SQLiteConnection connection)
        {
            // Araclar Migration
            string[] araclarCols = { "ResimYolu", "Guc", "Hizlanma", "KoltukSayisi" };
            foreach (var col in araclarCols)
            {
                if (!ColumnExists("Araclar", col, connection))
                {
                    string type = col == "Hizlanma" ? "REAL" : (col == "ResimYolu" ? "TEXT" : "INTEGER");
                    ExecuteCommand($"ALTER TABLE Araclar ADD COLUMN {col} {type}", connection);
                    if (col == "ResimYolu") ExecuteCommand("UPDATE Araclar SET ResimYolu = 'Assets\\sedan.png' WHERE ResimYolu IS NULL", connection);
                }
            }

            // Kiralamalar Migration
            string[] kiralamaCols = { "SigortaTipi", "EkstraHizmetler", "OdemeYontemi" };
            foreach (var col in kiralamaCols)
            {
                if (!ColumnExists("Kiralamalar", col, connection))
                {
                    ExecuteCommand($"ALTER TABLE Kiralamalar ADD COLUMN {col} TEXT", connection);
                }
            }
        }

        private static bool ColumnExists(string table, string column, SQLiteConnection connection)
        {
            using (var cmd = new SQLiteCommand($"PRAGMA table_info({table})", connection))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (reader["name"].ToString().Equals(column, StringComparison.OrdinalIgnoreCase)) return true;
                }
            }
            return false;
        }

        private static void ExecuteCommand(string query, SQLiteConnection connection)
        {
            using (var cmd = new SQLiteCommand(query, connection))
            {
                cmd.ExecuteNonQuery();
            }
        }

        private static void SeedAdminUser(SQLiteConnection connection)
        {
            // Eski 'admin' kullanıcısını temizleyip yeni 'Betül' kullanıcısını ekliyoruz
            string checkQuery = "SELECT COUNT(*) FROM Kullanicilar WHERE KullaniciAdi = 'Betül'";
            using (var checkCmd = new SQLiteCommand(checkQuery, connection))
            {
                long count = (long)checkCmd.ExecuteScalar();
                if (count == 0)
                {
                    ExecuteCommand("DELETE FROM Kullanicilar WHERE KullaniciAdi = 'admin'", connection);
                    ExecuteCommand("INSERT INTO Kullanicilar (KullaniciAdi, Sifre, Rol) VALUES ('Betül', '0808', 'Yonetici')", connection);
                }
            }
        }

        private static void SeedSampleData(SQLiteConnection connection)
        {
            long aracSayisi = (long)new SQLiteCommand("SELECT COUNT(*) FROM Araclar", connection).ExecuteScalar();
            if (aracSayisi == 0)
            {
                string[] araclar = {
                    "INSERT INTO Araclar (Plaka, Marka, Model, Yil, Vites, Yakit, Kilometre, GunlukFiyat, Durum, ResimYolu, Guc, Hizlanma, KoltukSayisi) VALUES ('34 FIA 01', 'Fiat', 'Egea', 2024, 'Manuel', 'Benzin', 15000, 1500, 'Boş', 'Assets\\fiat.png', 95, 11.9, 5)",
                    "INSERT INTO Araclar (Plaka, Marka, Model, Yil, Vites, Yakit, Kilometre, GunlukFiyat, Durum, ResimYolu, Guc, Hizlanma, KoltukSayisi) VALUES ('06 REN 02', 'Renault', 'Clio', 2024, 'Otomatik', 'Benzin', 8000, 1400, 'Boş', 'Assets\\renaultclio.png', 100, 11.2, 5)",
                    "INSERT INTO Araclar (Plaka, Marka, Model, Yil, Vites, Yakit, Kilometre, GunlukFiyat, Durum, ResimYolu, Guc, Hizlanma, KoltukSayisi) VALUES ('35 REN 03', 'Renault', 'Megane', 2023, 'Otomatik', 'Dizel', 25000, 1600, 'Boş', 'Assets\\renaultmegane.png', 115, 10.8, 5)",
                    "INSERT INTO Araclar (Plaka, Marka, Model, Yil, Vites, Yakit, Kilometre, GunlukFiyat, Durum, ResimYolu, Guc, Hizlanma, KoltukSayisi) VALUES ('34 TOY 04', 'Toyota', 'Corolla', 2024, 'Otomatik', 'Hibrit', 5000, 1800, 'Boş', 'Assets\\toyota.png', 122, 10.5, 5)",
                    "INSERT INTO Araclar (Plaka, Marka, Model, Yil, Vites, Yakit, Kilometre, GunlukFiyat, Durum, ResimYolu, Guc, Hizlanma, KoltukSayisi) VALUES ('16 VW 05', 'Volkswagen', 'Passat', 2023, 'Otomatik', 'Dizel', 30000, 2500, 'Boş', 'Assets\\vwpassat.png', 150, 9.1, 5)",
                    "INSERT INTO Araclar (Plaka, Marka, Model, Yil, Vites, Yakit, Kilometre, GunlukFiyat, Durum, ResimYolu, Guc, Hizlanma, KoltukSayisi) VALUES ('07 VW 06', 'Volkswagen', 'Polo', 2024, 'Otomatik', 'Benzin', 10000, 1300, 'Boş', 'Assets\\vwpolo.png', 95, 11.5, 5)",
                    "INSERT INTO Araclar (Plaka, Marka, Model, Yil, Vites, Yakit, Kilometre, GunlukFiyat, Durum, ResimYolu, Guc, Hizlanma, KoltukSayisi) VALUES ('34 HON 07', 'Honda', 'Civic', 2024, 'Otomatik', 'Benzin', 3000, 2000, 'Boş', 'Assets\\honda.png', 158, 8.8, 5)",
                    "INSERT INTO Araclar (Plaka, Marka, Model, Yil, Vites, Yakit, Kilometre, GunlukFiyat, Durum, ResimYolu, Guc, Hizlanma, KoltukSayisi) VALUES ('06 DAC 08', 'Dacia', 'Duster', 2024, 'Manuel', 'Dizel', 12000, 1700, 'Boş', 'Assets\\dacia.png', 115, 12.2, 5)",
                    "INSERT INTO Araclar (Plaka, Marka, Model, Yil, Vites, Yakit, Kilometre, GunlukFiyat, Durum, ResimYolu, Guc, Hizlanma, KoltukSayisi) VALUES ('41 HYU 09', 'Hyundai', 'i20', 2024, 'Otomatik', 'Benzin', 6000, 1200, 'Boş', 'Assets\\hyundai.png', 100, 10.9, 5)",
                    "INSERT INTO Araclar (Plaka, Marka, Model, Yil, Vites, Yakit, Kilometre, GunlukFiyat, Durum, ResimYolu, Guc, Hizlanma, KoltukSayisi) VALUES ('35 FOR 10', 'Ford', 'Focus', 2023, 'Otomatik', 'Benzin', 20000, 1500, 'Boş', 'Assets\\fordfocus.png', 125, 10.2, 5)",
                    "INSERT INTO Araclar (Plaka, Marka, Model, Yil, Vites, Yakit, Kilometre, GunlukFiyat, Durum, ResimYolu, Guc, Hizlanma, KoltukSayisi) VALUES ('34 PEU 11', 'Peugeot', '3008', 2024, 'Otomatik', 'Dizel', 8000, 2200, 'Boş', 'Assets\\peugeot.png', 130, 9.8, 5)",
                    "INSERT INTO Araclar (Plaka, Marka, Model, Yil, Vites, Yakit, Kilometre, GunlukFiyat, Durum, ResimYolu, Guc, Hizlanma, KoltukSayisi) VALUES ('06 NIS 12', 'Nissan', 'Qashqai', 2024, 'Otomatik', 'Benzin', 10000, 2000, 'Boş', 'Assets\\nissan.png', 140, 10.0, 5)",
                    "INSERT INTO Araclar (Plaka, Marka, Model, Yil, Vites, Yakit, Kilometre, GunlukFiyat, Durum, ResimYolu, Guc, Hizlanma, KoltukSayisi) VALUES ('34 FOR 13', 'Ford', 'Transit', 2023, 'Manuel', 'Dizel', 45000, 2500, 'Boş', 'Assets\\fordtransit.png', 170, 13.5, 3)",
                    "INSERT INTO Araclar (Plaka, Marka, Model, Yil, Vites, Yakit, Kilometre, GunlukFiyat, Durum, ResimYolu, Guc, Hizlanma, KoltukSayisi) VALUES ('34 TOG 14', 'Togg', 'T10X', 2024, 'Otomatik', 'Elektrik', 2000, 3000, 'Boş', 'Assets\\togg.png', 200, 7.6, 5)",
                    "INSERT INTO Araclar (Plaka, Marka, Model, Yil, Vites, Yakit, Kilometre, GunlukFiyat, Durum, ResimYolu, Guc, Hizlanma, KoltukSayisi) VALUES ('07 OPE 15', 'Opel', 'Corsa', 2024, 'Otomatik', 'Benzin', 5000, 1300, 'Boş', 'Assets\\opel.png', 100, 11.0, 5)"
                };

                foreach (var s in araclar) ExecuteCommand(s, connection);
            }

            long musteriSayisi = (long)new SQLiteCommand("SELECT COUNT(*) FROM Musteriler", connection).ExecuteScalar();
            if (musteriSayisi == 0)
            {
                string[] musteriler = {
                    "INSERT INTO Musteriler (TCNo, AdSoyad, Telefon, EhliyetNo, Adres) VALUES ('11122233344', 'Ahmet Yılmaz', '0532 111 22 33', 'EHL123', 'İstanbul')",
                    "INSERT INTO Musteriler (TCNo, AdSoyad, Telefon, EhliyetNo, Adres) VALUES ('55566677788', 'Ayşe Demir', '0544 444 55 66', 'EHL456', 'Ankara')"
                };
                foreach (var s in musteriler) ExecuteCommand(s, connection);
            }
        }

        private static void UpdateArtvinData(SQLiteConnection connection)
        {
            // Eski kayıtları güncelle (Artvin ve ilçeleri yap)
            ExecuteCommand("UPDATE Musteriler SET Adres = 'Merkez, Artvin' WHERE Id % 9 = 0", connection);
            ExecuteCommand("UPDATE Musteriler SET Adres = 'Hopa, Artvin' WHERE Id % 9 = 1", connection);
            ExecuteCommand("UPDATE Musteriler SET Adres = 'Borçka, Artvin' WHERE Id % 9 = 2", connection);
            ExecuteCommand("UPDATE Musteriler SET Adres = 'Şavşat, Artvin' WHERE Id % 9 = 3", connection);
            ExecuteCommand("UPDATE Musteriler SET Adres = 'Arhavi, Artvin' WHERE Id % 9 = 4", connection);
            ExecuteCommand("UPDATE Musteriler SET Adres = 'Yusufeli, Artvin' WHERE Id % 9 = 5", connection);
            ExecuteCommand("UPDATE Musteriler SET Adres = 'Ardanuç, Artvin' WHERE Id % 9 = 6", connection);
            ExecuteCommand("UPDATE Musteriler SET Adres = 'Murgul, Artvin' WHERE Id % 9 = 7", connection);
            ExecuteCommand("UPDATE Musteriler SET Adres = 'Kemalpaşa, Artvin' WHERE Id % 9 = 8", connection);

            // Eğer kiralama işlemi azsa ekle (Ana sayfada dolsun diye)
            long kiralamaSayisi = (long)new SQLiteCommand("SELECT COUNT(*) FROM Kiralamalar", connection).ExecuteScalar();
            if (kiralamaSayisi < 10)
            {
                // Müşterileri zorla ekle (Eğer daha önce eklenmemişse TC ile kontrol ederek)
                string[] yeniMusteriler = {
                    "INSERT OR IGNORE INTO Musteriler (TCNo, AdSoyad, Telefon, EhliyetNo, Adres) VALUES ('10000000001', 'Ali Yılmaz', '0530 100 00 01', 'EHL101', 'Merkez, Artvin')",
                    "INSERT OR IGNORE INTO Musteriler (TCNo, AdSoyad, Telefon, EhliyetNo, Adres) VALUES ('10000000002', 'Elif Doğan', '0530 100 00 02', 'EHL102', 'Arhavi, Artvin')",
                    "INSERT OR IGNORE INTO Musteriler (TCNo, AdSoyad, Telefon, EhliyetNo, Adres) VALUES ('10000000003', 'Mehmet Öztürk', '0530 100 00 03', 'EHL103', 'Şavşat, Artvin')",
                    "INSERT OR IGNORE INTO Musteriler (TCNo, AdSoyad, Telefon, EhliyetNo, Adres) VALUES ('10000000004', 'Zeynep Koç', '0530 100 00 04', 'EHL104', 'Hopa, Artvin')",
                    "INSERT OR IGNORE INTO Musteriler (TCNo, AdSoyad, Telefon, EhliyetNo, Adres) VALUES ('10000000005', 'Caner Aydın', '0530 100 00 05', 'EHL105', 'Yusufeli, Artvin')",
                    "INSERT OR IGNORE INTO Musteriler (TCNo, AdSoyad, Telefon, EhliyetNo, Adres) VALUES ('10000000006', 'Merve Erdoğan', '0530 100 00 06', 'EHL106', 'Borçka, Artvin')",
                    "INSERT OR IGNORE INTO Musteriler (TCNo, AdSoyad, Telefon, EhliyetNo, Adres) VALUES ('10000000007', 'Mustafa Çelik', '0530 100 00 07', 'EHL107', 'Ardanuç, Artvin')",
                    "INSERT OR IGNORE INTO Musteriler (TCNo, AdSoyad, Telefon, EhliyetNo, Adres) VALUES ('10000000008', 'Büşra Yavuz', '0530 100 00 08', 'EHL108', 'Murgul, Artvin')",
                    "INSERT OR IGNORE INTO Musteriler (TCNo, AdSoyad, Telefon, EhliyetNo, Adres) VALUES ('10000000009', 'Burak Aslan', '0530 100 00 09', 'EHL109', 'Kemalpaşa, Artvin')",
                    "INSERT OR IGNORE INTO Musteriler (TCNo, AdSoyad, Telefon, EhliyetNo, Adres) VALUES ('10000000010', 'Seda Kılıç', '0530 100 00 10', 'EHL110', 'Merkez, Artvin')",
                    "INSERT OR IGNORE INTO Musteriler (TCNo, AdSoyad, Telefon, EhliyetNo, Adres) VALUES ('10000000011', 'Kemal Şahin', '0530 100 00 11', 'EHL111', 'Arhavi, Artvin')",
                    "INSERT OR IGNORE INTO Musteriler (TCNo, AdSoyad, Telefon, EhliyetNo, Adres) VALUES ('10000000012', 'Gizem Çetin', '0530 100 00 12', 'EHL112', 'Şavşat, Artvin')",
                    "INSERT OR IGNORE INTO Musteriler (TCNo, AdSoyad, Telefon, EhliyetNo, Adres) VALUES ('10000000013', 'Volkan Yıldız', '0530 100 00 13', 'EHL113', 'Hopa, Artvin')",
                    "INSERT OR IGNORE INTO Musteriler (TCNo, AdSoyad, Telefon, EhliyetNo, Adres) VALUES ('10000000014', 'Fatma Aksoy', '0530 100 00 14', 'EHL114', 'Yusufeli, Artvin')",
                    "INSERT OR IGNORE INTO Musteriler (TCNo, AdSoyad, Telefon, EhliyetNo, Adres) VALUES ('10000000015', 'Ebru Tekin', '0530 100 00 15', 'EHL115', 'Borçka, Artvin')"
                };
                foreach (var s in yeniMusteriler) ExecuteCommand(s, connection);

                // Müşterilerin Id'lerini çekelim (İsimlerine göre eşleştirerek sağlam yaparız)
                string[] kiralamalar = {
                    $"INSERT INTO Kiralamalar (MusteriId, AracId, BaslangicTarihi, BitisTarihi, ToplamTutar, Durum, SigortaTipi, EkstraHizmetler, OdemeYontemi) VALUES ((SELECT Id FROM Musteriler WHERE TCNo='10000000001'), 1, '{DateTime.Now.AddDays(-2):yyyy-MM-dd}', '{DateTime.Now.AddDays(2):yyyy-MM-dd}', 6000, 'Aktif', 'Tam Kapsamlı', 'Bebek Koltuğu', 'Kredi Kartı')",
                    $"INSERT INTO Kiralamalar (MusteriId, AracId, BaslangicTarihi, BitisTarihi, ToplamTutar, Durum, SigortaTipi, EkstraHizmetler, OdemeYontemi) VALUES ((SELECT Id FROM Musteriler WHERE TCNo='10000000002'), 2, '{DateTime.Now.AddDays(-1):yyyy-MM-dd}', '{DateTime.Now.AddDays(3):yyyy-MM-dd}', 5600, 'Aktif', 'Standart', 'Yok', 'Nakit')",
                    $"INSERT INTO Kiralamalar (MusteriId, AracId, BaslangicTarihi, BitisTarihi, ToplamTutar, Durum, SigortaTipi, EkstraHizmetler, OdemeYontemi) VALUES ((SELECT Id FROM Musteriler WHERE TCNo='10000000003'), 3, '{DateTime.Now.AddDays(-5):yyyy-MM-dd}', '{DateTime.Now.AddDays(-1):yyyy-MM-dd}', 6400, 'Tamamlandı', 'Standart', 'Navigasyon', 'Kredi Kartı')",
                    $"INSERT INTO Kiralamalar (MusteriId, AracId, BaslangicTarihi, BitisTarihi, ToplamTutar, Durum, SigortaTipi, EkstraHizmetler, OdemeYontemi) VALUES ((SELECT Id FROM Musteriler WHERE TCNo='10000000004'), 4, '{DateTime.Now.AddDays(1):yyyy-MM-dd}', '{DateTime.Now.AddDays(4):yyyy-MM-dd}', 5400, 'Aktif', 'Tam Kapsamlı', 'Yok', 'Havale')",
                    $"INSERT INTO Kiralamalar (MusteriId, AracId, BaslangicTarihi, BitisTarihi, ToplamTutar, Durum, SigortaTipi, EkstraHizmetler, OdemeYontemi) VALUES ((SELECT Id FROM Musteriler WHERE TCNo='10000000005'), 5, '{DateTime.Now.AddDays(-3):yyyy-MM-dd}', '{DateTime.Now.AddDays(5):yyyy-MM-dd}', 20000, 'Aktif', 'Tam Kapsamlı', 'Ek Sürücü', 'Kredi Kartı')",
                    $"INSERT INTO Kiralamalar (MusteriId, AracId, BaslangicTarihi, BitisTarihi, ToplamTutar, Durum, SigortaTipi, EkstraHizmetler, OdemeYontemi) VALUES ((SELECT Id FROM Musteriler WHERE TCNo='10000000006'), 6, '{DateTime.Now.AddDays(-10):yyyy-MM-dd}', '{DateTime.Now.AddDays(-7):yyyy-MM-dd}', 3900, 'Tamamlandı', 'Standart', 'Yok', 'Kredi Kartı')",
                    $"INSERT INTO Kiralamalar (MusteriId, AracId, BaslangicTarihi, BitisTarihi, ToplamTutar, Durum, SigortaTipi, EkstraHizmetler, OdemeYontemi) VALUES ((SELECT Id FROM Musteriler WHERE TCNo='10000000007'), 7, '{DateTime.Now.AddDays(-1):yyyy-MM-dd}', '{DateTime.Now.AddDays(2):yyyy-MM-dd}', 6000, 'Aktif', 'Standart', 'Bebek Koltuğu', 'Nakit')",
                    $"INSERT INTO Kiralamalar (MusteriId, AracId, BaslangicTarihi, BitisTarihi, ToplamTutar, Durum, SigortaTipi, EkstraHizmetler, OdemeYontemi) VALUES ((SELECT Id FROM Musteriler WHERE TCNo='10000000008'), 8, '{DateTime.Now.AddDays(0):yyyy-MM-dd}', '{DateTime.Now.AddDays(3):yyyy-MM-dd}', 5100, 'Aktif', 'Tam Kapsamlı', 'Yok', 'Kredi Kartı')",
                    $"INSERT INTO Kiralamalar (MusteriId, AracId, BaslangicTarihi, BitisTarihi, ToplamTutar, Durum, SigortaTipi, EkstraHizmetler, OdemeYontemi) VALUES ((SELECT Id FROM Musteriler WHERE TCNo='10000000009'), 9, '{DateTime.Now.AddDays(-4):yyyy-MM-dd}', '{DateTime.Now.AddDays(1):yyyy-MM-dd}', 6000, 'Aktif', 'Standart', 'Navigasyon', 'Havale')",
                    $"INSERT INTO Kiralamalar (MusteriId, AracId, BaslangicTarihi, BitisTarihi, ToplamTutar, Durum, SigortaTipi, EkstraHizmetler, OdemeYontemi) VALUES ((SELECT Id FROM Musteriler WHERE TCNo='10000000010'), 10, '{DateTime.Now.AddDays(-2):yyyy-MM-dd}', '{DateTime.Now.AddDays(0):yyyy-MM-dd}', 3000, 'Tamamlandı', 'Standart', 'Yok', 'Kredi Kartı')",
                    $"INSERT INTO Kiralamalar (MusteriId, AracId, BaslangicTarihi, BitisTarihi, ToplamTutar, Durum, SigortaTipi, EkstraHizmetler, OdemeYontemi) VALUES ((SELECT Id FROM Musteriler WHERE TCNo='10000000011'), 11, '{DateTime.Now.AddDays(-6):yyyy-MM-dd}', '{DateTime.Now.AddDays(-2):yyyy-MM-dd}', 8800, 'Tamamlandı', 'Full Kasko', 'Yok', 'Kredi Kartı')",
                    $"INSERT INTO Kiralamalar (MusteriId, AracId, BaslangicTarihi, BitisTarihi, ToplamTutar, Durum, SigortaTipi, EkstraHizmetler, OdemeYontemi) VALUES ((SELECT Id FROM Musteriler WHERE TCNo='10000000012'), 12, '{DateTime.Now.AddDays(-1):yyyy-MM-dd}', '{DateTime.Now.AddDays(6):yyyy-MM-dd}', 14000, 'Aktif', 'Standart', 'Çocuk Koltuğu', 'Nakit')",
                    $"INSERT INTO Kiralamalar (MusteriId, AracId, BaslangicTarihi, BitisTarihi, ToplamTutar, Durum, SigortaTipi, EkstraHizmetler, OdemeYontemi) VALUES ((SELECT Id FROM Musteriler WHERE TCNo='10000000013'), 13, '{DateTime.Now.AddDays(-8):yyyy-MM-dd}', '{DateTime.Now.AddDays(-5):yyyy-MM-dd}', 7500, 'Tamamlandı', 'Standart', 'Yok', 'Kredi Kartı')",
                    $"INSERT INTO Kiralamalar (MusteriId, AracId, BaslangicTarihi, BitisTarihi, ToplamTutar, Durum, SigortaTipi, EkstraHizmetler, OdemeYontemi) VALUES ((SELECT Id FROM Musteriler WHERE TCNo='10000000014'), 14, '{DateTime.Now.AddDays(0):yyyy-MM-dd}', '{DateTime.Now.AddDays(2):yyyy-MM-dd}', 6000, 'Aktif', 'Tam Kapsamlı', 'Yok', 'Kredi Kartı')"
                };

                foreach (var k in kiralamalar)
                {
                    try { ExecuteCommand(k, connection); } catch { }
                }

                ExecuteCommand("UPDATE Araclar SET Durum = 'Dolu' WHERE Id IN (1, 2, 4, 5, 7, 8, 9, 12, 14)", connection);
            }

            // --- LÜKS VE PREMIUM ARAÇLARI ÇOĞALTMA ---
            long aracSayisi = (long)new SQLiteCommand("SELECT COUNT(*) FROM Araclar", connection).ExecuteScalar();
            if (aracSayisi < 20)
            {
                string[] luksAraclar = {
                    "INSERT INTO Araclar (Plaka, Marka, Model, Yil, Vites, Yakit, Kilometre, GunlukFiyat, Durum, ResimYolu, Guc, Hizlanma, KoltukSayisi) VALUES ('34 MER 01', 'Mercedes', 'S-Class', 2024, 'Otomatik', 'Hibrit', 1500, 7500, 'Boş', 'Assets\\mercedes.png', 367, 5.1, 5)",
                    "INSERT INTO Araclar (Plaka, Marka, Model, Yil, Vites, Yakit, Kilometre, GunlukFiyat, Durum, ResimYolu, Guc, Hizlanma, KoltukSayisi) VALUES ('06 BMW 02', 'BMW', '520i', 2024, 'Otomatik', 'Benzin', 4000, 5000, 'Boş', 'Assets\\bmw.png', 170, 7.8, 5)",
                    "INSERT INTO Araclar (Plaka, Marka, Model, Yil, Vites, Yakit, Kilometre, GunlukFiyat, Durum, ResimYolu, Guc, Hizlanma, KoltukSayisi) VALUES ('35 AUD 03', 'Audi', 'A6', 2023, 'Otomatik', 'Dizel', 12000, 4800, 'Boş', 'Assets\\audi.png', 204, 8.1, 5)",
                    "INSERT INTO Araclar (Plaka, Marka, Model, Yil, Vites, Yakit, Kilometre, GunlukFiyat, Durum, ResimYolu, Guc, Hizlanma, KoltukSayisi) VALUES ('34 POR 04', 'Porsche', 'Taycan', 2024, 'Otomatik', 'Elektrik', 500, 12000, 'Boş', 'Assets\\porsche.png', 408, 5.4, 4)",
                    "INSERT INTO Araclar (Plaka, Marka, Model, Yil, Vites, Yakit, Kilometre, GunlukFiyat, Durum, ResimYolu, Guc, Hizlanma, KoltukSayisi) VALUES ('34 TES 05', 'Tesla', 'Model S', 2024, 'Otomatik', 'Elektrik', 1200, 9000, 'Boş', 'Assets\\tesla.png', 670, 3.2, 5)",
                    "INSERT INTO Araclar (Plaka, Marka, Model, Yil, Vites, Yakit, Kilometre, GunlukFiyat, Durum, ResimYolu, Guc, Hizlanma, KoltukSayisi) VALUES ('34 FER 06', 'Ferrari', 'Roma', 2023, 'Otomatik', 'Benzin', 3000, 25000, 'Boş', 'Assets\\ferrari.png', 620, 3.4, 2)",
                    "INSERT INTO Araclar (Plaka, Marka, Model, Yil, Vites, Yakit, Kilometre, GunlukFiyat, Durum, ResimYolu, Guc, Hizlanma, KoltukSayisi) VALUES ('07 LAM 07', 'Lamborghini', 'Urus', 2024, 'Otomatik', 'Benzin', 2500, 30000, 'Boş', 'Assets\\lamborghini.png', 650, 3.6, 5)",
                    "INSERT INTO Araclar (Plaka, Marka, Model, Yil, Vites, Yakit, Kilometre, GunlukFiyat, Durum, ResimYolu, Guc, Hizlanma, KoltukSayisi) VALUES ('34 MAS 08', 'Maserati', 'Ghibli', 2023, 'Otomatik', 'Hibrit', 8000, 8500, 'Boş', 'Assets\\maserati.png', 330, 5.7, 5)",
                    "INSERT INTO Araclar (Plaka, Marka, Model, Yil, Vites, Yakit, Kilometre, GunlukFiyat, Durum, ResimYolu, Guc, Hizlanma, KoltukSayisi) VALUES ('06 BEN 09', 'Bentley', 'Continental', 2024, 'Otomatik', 'Benzin', 1000, 35000, 'Boş', 'Assets\\bentley.png', 550, 4.0, 4)",
                    "INSERT INTO Araclar (Plaka, Marka, Model, Yil, Vites, Yakit, Kilometre, GunlukFiyat, Durum, ResimYolu, Guc, Hizlanma, KoltukSayisi) VALUES ('34 AST 10', 'Aston Martin', 'DB11', 2023, 'Otomatik', 'Benzin', 4500, 28000, 'Boş', 'Assets\\astonmartin.png', 503, 4.0, 4)",
                    "INSERT INTO Araclar (Plaka, Marka, Model, Yil, Vites, Yakit, Kilometre, GunlukFiyat, Durum, ResimYolu, Guc, Hizlanma, KoltukSayisi) VALUES ('34 JAG 11', 'Jaguar', 'F-Type', 2024, 'Otomatik', 'Benzin', 2000, 11000, 'Boş', 'Assets\\jaguar.png', 300, 5.7, 2)",
                    "INSERT INTO Araclar (Plaka, Marka, Model, Yil, Vites, Yakit, Kilometre, GunlukFiyat, Durum, ResimYolu, Guc, Hizlanma, KoltukSayisi) VALUES ('06 RVR 12', 'Land Rover', 'Range Rover', 2024, 'Otomatik', 'Dizel', 6000, 15000, 'Boş', 'Assets\\landrover.png', 300, 7.3, 5)",
                    "INSERT INTO Araclar (Plaka, Marka, Model, Yil, Vites, Yakit, Kilometre, GunlukFiyat, Durum, ResimYolu, Guc, Hizlanma, KoltukSayisi) VALUES ('34 ALF 13', 'Alfa Romeo', 'Giulia', 2023, 'Otomatik', 'Benzin', 9000, 4500, 'Boş', 'Assets\\alfaromeo.png', 280, 5.2, 5)",
                    "INSERT INTO Araclar (Plaka, Marka, Model, Yil, Vites, Yakit, Kilometre, GunlukFiyat, Durum, ResimYolu, Guc, Hizlanma, KoltukSayisi) VALUES ('34 CRV 14', 'Corvette', 'Stingray', 2024, 'Otomatik', 'Benzin', 1000, 18000, 'Boş', 'Assets\\corvette.png', 490, 2.9, 2)",
                    "INSERT INTO Araclar (Plaka, Marka, Model, Yil, Vites, Yakit, Kilometre, GunlukFiyat, Durum, ResimYolu, Guc, Hizlanma, KoltukSayisi) VALUES ('06 SUB 15', 'Subaru', 'Impreza', 2023, 'Otomatik', 'Benzin', 15000, 3000, 'Boş', 'Assets\\subaru.png', 152, 9.8, 5)"
                };

                foreach (var a in luksAraclar)
                {
                    try { ExecuteCommand(a, connection); } catch { }
                }
            }
        }

        public static SQLiteConnection GetConnection()
        {
            var con = new SQLiteConnection(ConnectionString);
            con.Open();
            return con;
        }
    }
}
