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

            Application.Run(new HosgeldinizForm());
        }
    }
}
