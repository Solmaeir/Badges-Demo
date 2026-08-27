using Microsoft.Extensions.Logging;

namespace ProfilRozetleriModulu.Business
{
    // Host, kendi ICurrentUserProvider uygulamasını kaydetmediyse devreye
    // girer. Herkesi "giriş yapılmamış" sayar ve log'a yazar.
    public class UnconfiguredCurrentUserProvider : ICurrentUserProvider
    {
        private readonly ILogger<UnconfiguredCurrentUserProvider> _logger;

        public UnconfiguredCurrentUserProvider(ILogger<UnconfiguredCurrentUserProvider> logger)
        {
            _logger = logger;
        }

        public int? GetCurrentUserId()
        {
            _logger.LogWarning(
                "ICurrentUserProvider host tarafından kaydedilmemiş. ProfilRozetleri modülü " +
                "kimseyi giriş yapmış saymıyor.");
            return null;
        }
    }
}
