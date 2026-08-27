namespace ProfilRozetleriModulu.Business
{
    // Modülün appsettings.json > ProfilRozetleriModulu bölümünden okunan
    // ayarları. Sayı olduğu için config'e alındı: kod değiştirmeden,
    // appsettings.json üzerinden ayarlanabilsin.
    public class ProfilRozetleriOptions
    {
        // BadgeProcessingJob'un (BackgroundService) WebLog'u ne sıklıkla
        // tarayacağı, saniye cinsinden.
        public int JobAralikSaniye { get; set; } = 20;

        // BadgeBusinessService.ModuleEntryCheck'te bir modülde "yeterince
        // kalındı" sayılması için gereken minimum süre (saniye) — bu eşiği
        // geçmeyen ziyaretler Module/Discovery rozetleri için sayılmaz.
        public int ModulKalmaEsigiSaniye { get; set; } = 15;

        // Birbirine bu kadar saniyeden az aralıklı WebLog kayıtları TEK bir
        // "an" (aynı sayfa yüklemesinin parçası — sayfanın kendisi + navbar/
        // arka plan isteklerinin ürettiği gürültü) sayılır; bkz.
        // BadgeBusinessService.ModuleEntryCheck ve GetSonBadgesZiyaretTarihi.
        // Bu değer, o arka plan isteklerinin gerçekte ne kadar hızlı
        // tamamlandığına (sunucu/ağ performansına) bağlı — farklı bir
        // ortamda bu süre gözlemlenip gerekirse ayarlanmalı.
        public double BurstEsigiSaniye { get; set; } = 2.0;

        // Günlük giriş ve rozet kazanımı XP miktarları; seviye atlamak için
        // gereken XP (Level = XP / SeviyeBasinaXP + 1). Modülde geçirilen
        // sürenin XP'ye ek bir etkisi yok — yalnızca bu iki olay XP verir.
        public int GunlukGirisXP { get; set; } = 10;
        public int RozetKazanimXP { get; set; } = 50;
        public int SeviyeBasinaXP { get; set; } = 100;

        // AwardXP çağrılmadan (yani XP kazanılmadan) kaç gün geçerse XP
        // düşmeye başlar (XPDususBaslamaGunu) ve her gün ne kadar düşer
        // (GunlukXPDususu) — bkz. BadgeBusinessService.ApplyXPDecayIfNeeded.
        public int XPDususBaslamaGunu { get; set; } = 3;
        public int GunlukXPDususu { get; set; } = 5;

        // Giriş yapılmamış bir istek ProfilRozetleriGirisGerekliAttribute
        // tarafından buraya yönlendirilir.
        public string GirisSayfasiUrl { get; set; } = "/Account/Login";

        // Rozet Yönetimi ekranına (BadgeAdminController) girebilmek için
        // gereken yetkinin, host sistemdeki adı. IBadgeAccessService.
        // HasPermission'a olduğu gibi geçilir — bu ad host sisteme ait, kod
        // içinde sabit tutulmaz. Host'un yetki tablosunda bu yetki hangi
        // isimle duruyorsa appsettings.json'da o yazılmalı.
        public string YonetimYetkisiAdi { get; set; } = "RozetYonetimi";

        // Rozet Yönetimi > Modül Yönetimi ekranındaki "Keşfedilen Controller'lar"
        // listesi, sitedeki TÜM controller'ları (IActionDescriptorCollectionProvider'dan)
        // gösterir — bu liste host'a ait, "modül" sayılması anlamlı olmayan
        // controller'ları (giriş sayfası, panel, başka bir modülün yönetim ekranı
        // gibi) da içerir. Modülün kendi controller'ları (Badges, BadgeAdmin)
        // BadgeAdminService içinde zaten her zaman otomatik hariç tutuluyor;
        // host'a ait olanlar (Account, Wizard...) için host bu listeyi doldurur.
        public List<string> HaricTutulanControllerler { get; set; } = new();
    }
}
