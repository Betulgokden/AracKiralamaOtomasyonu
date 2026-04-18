namespace AracKiralamaOtomasyonu
{
    public static class SessionManager
    {
        public static string CurrentUser { get; set; } = null;
        public static string UserRole { get; set; } = null; // 'Yonetici' veya 'Musteri'

        public static bool IsAdmin => UserRole == "Yonetici";

        public static void Logout()
        {
            CurrentUser = null;
            UserRole = null;
        }
    }
}
