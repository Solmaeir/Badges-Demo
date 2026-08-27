namespace ProfilRozetleriModulu.Utils
{
    // Yalnızca BadgeBusinessService içinde kullanılan gün/saniye
    // karşılaştırmaları. DB erişimi yok, saf fonksiyonlar (BadgeDisplayUtils
    // ile aynı kural: tek bir serviste kullanılan yardımcı metotlar Utils'te,
    // birden fazla serviste kullanılanlar Helpers'ta durur).
    //
    // referansGun verilmezse gerçek "bugün" (DateTime.Today, sunucu yerel
    // saati) kullanılır; bu, ModuleEntryCheck gibi neredeyse gerçek zamanlı
    // işleyen çağrılar için yeterli. LoginCheck ise arka plan işi
    // birden fazla güne yayılmış kayıtları tek seferde işleyebildiği için
    // referansGun'u ilgili WebLog kaydının kendi tarihiyle açıkça verir —
    // "bugün" orada işlenen kaydın günü demektir, çalıştırma anının günü değil.
    public static class DateCalculationHelper
    {
        public static bool IsToday(DateTime? tarih, DateTime? referansGun = null)
        {
            var gun = (referansGun ?? DateTime.Today).Date;
            return tarih.HasValue && tarih.Value.Date == gun;
        }

        public static bool IsYesterday(DateTime? tarih, DateTime? referansGun = null)
        {
            var gun = (referansGun ?? DateTime.Today).Date;
            return tarih.HasValue && tarih.Value.Date == gun.AddDays(-1);
        }

        public static double SaniyeFarki(DateTime onceki, DateTime sonraki)
        {
            return (sonraki - onceki).TotalSeconds;
        }
    }
}
