using ProfilRozetleriModulu.Models;
using ProfilRozetleriModulu.ViewModels;

namespace ProfilRozetleriModulu.Business
{
    // Rozet Yönetimi ekranının iş kuralları: BadgeType'a göre hangi alanların
    // zorunlu olduğunu burada doğruluyoruz — DB'deki CHECK constraint'lere
    // (bkz. 01-profilrozetleri-tables.sql) çarpıp genel bir SQL hatası almak
    // yerine, yöneticiye hangi alanın eksik olduğunu burada söylüyoruz.
    public interface IBadgeAdminService
    {
        List<Badge> GetAll();

        // Index.cshtml'nin tablosu için hazır (View-şekilli) veri — tür
        // etiketi ve Modül/Sinyal metni burada hesaplanır, View'de değil.
        List<BadgeListItemViewModel> GetBadgeListItems();

        Badge? GetById(int badgeId);
        List<Module> GetModules();
        List<ExternalSignalDescriptor> GetExternalSignals();

        // Modül Yönetimi ekranının (tam sayfa + popup, ikisi de) tükettiği
        // veri — Mevcut Modüller ve Keşfedilen Controller'lar tek çağrıda.
        ModuleAdminViewModel GetModuleManagementData();

        // Sitedeki TÜM controller adları (ASP.NET Core'un kendi routing
        // kaydından — WebLog.Controller'a yazılan değerle aynı kaynak).
        // Elle yazım hatası riskini sıfırlamak için: admin bir controller
        // adını KENDİSİ yazmaz, buradan seçer.
        List<string> GetDiscoveredControllerNames();

        // Keşfedilen ama henüz Modules tablosunda karşılığı olmayanlar.
        List<string> GetUnregisteredControllerNames();

        bool AddModule(string moduleName, string controllerName, out string hata);
        bool DeleteModule(int moduleId, out string hata);

        // wwwroot/Content/Badges altındaki PNG'lerin URL listesi (rozet seç
        // ekranı için). Modülün kendi asset klasörü — host'a sorulmuyor.
        List<string> GetAvailableIcons();

        bool Add(BadgeAdminViewModel model, out string hata);
        bool Update(BadgeAdminViewModel model, out string hata);
        bool Delete(int badgeId);
    }
}
