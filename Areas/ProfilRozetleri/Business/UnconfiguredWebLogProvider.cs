using Microsoft.Extensions.Logging;

namespace ProfilRozetleriModulu.Business
{
    // Host, kendi IWebLogProvider uygulamasını kaydetmediyse devreye girer.
    // Arka plan işi çökmesin diye boş liste döner ama neyin eksik olduğunu
    // log'a yazar.
    public class UnconfiguredWebLogProvider : IWebLogProvider
    {
        private readonly ILogger<UnconfiguredWebLogProvider> _logger;

        public UnconfiguredWebLogProvider(ILogger<UnconfiguredWebLogProvider> logger)
        {
            _logger = logger;
        }

        public List<WebLogEntry> GetNewEntries(int lastProcessedLogId)
        {
            _logger.LogWarning(
                "IWebLogProvider host tarafından kaydedilmemiş. ProfilRozetleri modülü " +
                "yeni istek kaydı bulamıyor, hiçbir rozet/seviye güncellemesi yapılmayacak.");
            return new List<WebLogEntry>();
        }

        public List<WebLogEntry> GetRecentEntries(int userId, string controller, string action, int maxCount)
        {
            _logger.LogWarning(
                "IWebLogProvider host tarafından kaydedilmemiş. 'Yeni rozet' etiketi için " +
                "son ziyaret tarihi hiç bulunamayacak.");
            return new List<WebLogEntry>();
        }
    }
}
