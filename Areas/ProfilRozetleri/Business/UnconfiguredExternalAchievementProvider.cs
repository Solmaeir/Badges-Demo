using Microsoft.Extensions.Logging;

namespace ProfilRozetleriModulu.Business
{
    // Host, kendi IExternalAchievementProvider uygulamasını kaydetmediyse
    // devreye girer. ExternalSignal tipi rozetler hiçbir zaman kazanılmaz,
    // ama eksik log'a yazılır.
    public class UnconfiguredExternalAchievementProvider : IExternalAchievementProvider
    {
        private readonly ILogger<UnconfiguredExternalAchievementProvider> _logger;

        public UnconfiguredExternalAchievementProvider(ILogger<UnconfiguredExternalAchievementProvider> logger)
        {
            _logger = logger;
        }

        public bool IsAchieved(int userId, string signalKey)
        {
            _logger.LogWarning(
                "IExternalAchievementProvider host tarafından kaydedilmemiş. '{SignalKey}' sinyali " +
                "hiçbir zaman sağlanmış sayılmayacak.", signalKey);
            return false;
        }

        public List<ExternalSignalDescriptor> GetSupportedSignals()
        {
            return new List<ExternalSignalDescriptor>();
        }
    }
}
