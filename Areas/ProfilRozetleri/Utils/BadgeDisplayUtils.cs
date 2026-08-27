namespace ProfilRozetleriModulu.Utils
{
    // Yalnızca BadgeBusinessService içinde kullanılan gösterim yardımcıları.
    // DB erişimi yok, saf fonksiyonlar (Helpers ile aynı mantık, farkı tek
    // bir serviste kullanılması — bkz. katman kuralları).
    public static class BadgeDisplayUtils
    {
        // Rozet üzerine gelindiğinde gösterilecek "x/y" ilerleme metni.
        // System rozetlerde y=RequiredValue, x=ConsecutiveLoginDays; Module
        // rozetlerde y=RequiredValue, x=RepeatCount. Çağıran taraf hangi
        // sayacı vereceğine karar verir, bu yalnızca metni biçimlendirir.
        public static string IlerlemeMetni(int mevcut, int gereken)
        {
            var sinirli = Math.Min(mevcut, gereken);
            return $"{sinirli}/{gereken}";
        }
    }
}
