using Microsoft.Extensions.Logging;

namespace ProfilRozetleriModulu.Business
{
    // Host, kendi IBadgeAccessService uygulamasını kaydetmediyse devreye
    // girer. Varsayılan "kimseye izin verme" — unutulan bir DI kaydı, Rozet
    // Yönetimi'ni sessizce herkese açık bırakmasın diye. Reddi log'a yazar ki
    // entegrasyonu yapan kişi eksiği hemen fark etsin.
    public class UnconfiguredBadgeAccessService : IBadgeAccessService
    {
        private readonly ILogger<UnconfiguredBadgeAccessService> _logger;

        public UnconfiguredBadgeAccessService(ILogger<UnconfiguredBadgeAccessService> logger)
        {
            _logger = logger;
        }

        public bool HasPermission(string permissionName)
        {
            _logger.LogWarning(
                "IBadgeAccessService host tarafından kaydedilmemiş. Güvenlik gereği '{Yetki}' " +
                "yetkisi için erişim reddedildi. Kendi yetki kontrolünüzü içeren bir " +
                "IBadgeAccessService uygulaması kaydedin.", permissionName);
            return false;
        }
    }
}
