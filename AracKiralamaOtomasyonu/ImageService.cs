using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace AracKiralamaOtomasyonu
{
    public static class ImageService
    {
        private static readonly Dictionary<string, Image> _imageCache = new Dictionary<string, Image>(StringComparer.OrdinalIgnoreCase);
        private static readonly object _lock = new object();

        // Marka -> dosya adı eşleştirmesi
        private static readonly Dictionary<string, string> _brandFileMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Fiat", "fiat.png" },
            { "Renault Clio", "renaultclio.png" },
            { "Renault Megane", "renaultmegane.png" },
            { "Toyota", "toyota.png" },
            { "Volkswagen Passat", "vwpassat.png" },
            { "Volkswagen Polo", "vwpolo.png" },
            { "Volkswagen", "vw.png" },
            { "Honda", "honda.png" },
            { "Dacia", "dacia.png" },
            { "Hyundai", "hyundai.png" },
            { "Ford Focus", "fordfocus.png" },
            { "Ford Transit", "fordtransit.png" },
            { "Ford", "fordfocus.png" },
            { "Peugeot", "peugeot.png" },
            { "Nissan", "nissan.png" },
            { "Togg", "togg.png" },
            { "Opel", "opel.png" },
            { "Audi", "audi.png" },
            { "BMW", "bmw.png" },
            { "Mercedes", "mercedes.png" },
            { "Porsche", "porsche.png" },
            { "Tesla", "tesla.png" },
            { "Ferrari", "ferrari.png" },
            { "Lamborghini", "lamborghini.png" },
            { "Jaguar", "jaguar.png" },
            { "Maserati", "maserati.png" },
            { "Bentley", "bentley.png" },
            { "Aston Martin", "astonmartin.png" },
            { "Chevrolet", "corvette.png" },
            { "Land Rover", "landrover.png" },
            { "Subaru", "subaru.png" },
            { "Alfa Romeo", "alfaromeo.png" }
        };

        public static Image GetImage(string brand, string dbPath = null)
        {
            lock (_lock)
            {
                string key = (brand ?? "").Trim().ToLower();
                if (string.IsNullOrEmpty(key)) return GetFallbackImage();

                if (_imageCache.ContainsKey(key))
                    return _imageCache[key];

                Image img = LoadImage(brand, dbPath);
                if (img != null)
                {
                    _imageCache[key] = img;
                    return img;
                }

                return GetFallbackImage();
            }
        }

        private static Image LoadImage(string brand, string dbPath)
        {
            // 1) dbPath'den dene (veritabanında kayıtlı yol)
            if (!string.IsNullOrEmpty(dbPath))
            {
                Image img = TryLoadFromPath(dbPath);
                if (img != null) return img;
            }

            // 2) Brand map'den dene
            string targetFile = ResolveFileName(brand);
            string[] candidates = {
                Path.Combine("Assets", targetFile),
                Path.Combine(Application.StartupPath, "Assets", targetFile),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", targetFile)
            };

            foreach (var p in candidates)
            {
                Image img = TryLoadFromPath(p);
                if (img != null) return img;
            }

            return null;
        }

        private static Image TryLoadFromPath(string path)
        {
            try
            {
                string fullPath = Path.IsPathRooted(path) ? path : Path.Combine(Application.StartupPath, path);
                if (File.Exists(fullPath))
                {
                    using (var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
                    {
                        return Image.FromStream(stream);
                    }
                }
            }
            catch { }
            return null;
        }

        private static string ResolveFileName(string brand)
        {
            if (_brandFileMap.ContainsKey(brand))
                return _brandFileMap[brand];

            // Fallback: marka adını dosya adına çevir
            string b = brand.ToLower().Replace(" ", "").Replace("-", "");
            return b + ".png";
        }

        private static Image GetFallbackImage()
        {
            // sedan.png'yi fallback olarak kullan
            string path = Path.Combine(Application.StartupPath, "Assets", "sedan.png");
            if (!_imageCache.ContainsKey("_fallback_"))
            {
                Image img = TryLoadFromPath(path);
                if (img != null)
                {
                    _imageCache["_fallback_"] = img;
                    return img;
                }

                // Hiç dosya yoksa dinamik placeholder
                Bitmap bmp = new Bitmap(400, 200);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.Clear(Color.FromArgb(241, 245, 249));
                    using (Font f = new Font("Segoe UI", 14, FontStyle.Bold))
                        g.DrawString("Araç Görseli", f, Brushes.Gray, new PointF(120, 85));
                }
                _imageCache["_fallback_"] = bmp;
            }
            return _imageCache["_fallback_"];
        }

        public static void ClearCache()
        {
            lock (_lock)
            {
                foreach (var img in _imageCache.Values) img.Dispose();
                _imageCache.Clear();
            }
        }
    }
}
