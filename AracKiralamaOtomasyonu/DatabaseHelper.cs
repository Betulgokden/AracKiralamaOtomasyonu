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

        public static SQLiteConnection GetConnection()
        {
            var con = new SQLiteConnection(ConnectionString);
            con.Open();
            return con;
        }
    }
}
