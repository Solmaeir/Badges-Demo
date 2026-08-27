using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ProfilRozetleriModulu.Business
{
    // Arka planda sürekli çalışan zamanlanmış iş: BadgeBusinessService.
    // ProcessNewWebLogEntries()'i periyodik olarak tetikler (aralık:
    // ProfilRozetleriOptions.JobAralikSaniye). Hiçbir controller'ı
    // tetiklemez, kullanıcı isteğinden tamamen bağımsız çalışır — rozet/
    // seviye güncellemeleri bu döngü sayesinde kullanıcı hiçbir şeye
    // tıklamasa bile arka planda gerçekleşir.
    //
    // BackgroundService (Singleton) ömrü boyunca yaşar ama IBadgeBusinessService
    // (ve altındaki DbContext) Scoped'dır — bir Singleton, Scoped bir servisi
    // doğrudan constructor'a alamaz. Bu yüzden her turda IServiceScopeFactory
    // ile kendi geçici scope'unu açıp işini bitirince kapatır; ASP.NET Core'da
    // background job'ların Scoped servis kullanmasının standart yoludur.
    public class BadgeProcessingJob : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ProfilRozetleriOptions _options;
        private readonly ILogger<BadgeProcessingJob> _logger;
         
        public BadgeProcessingJob(
            IServiceScopeFactory scopeFactory,
            IOptions<ProfilRozetleriOptions> options,
            ILogger<BadgeProcessingJob> logger)
        {
            _scopeFactory = scopeFactory;
            _options = options.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var badgeBusinessService = scope.ServiceProvider.GetRequiredService<IBadgeBusinessService>();
                    badgeBusinessService.ProcessNewWebLogEntries();
                }
                catch (Exception ex)
                {
                    // İş çöküp arka plan servisini tamamen durdurmasın diye
                    // yakalanıyor; bir sonraki turda tekrar denenecek.
                    _logger.LogError(ex, "ProfilRozetleri BadgeProcessingJob turu başarısız oldu.");
                }

                await Task.Delay(TimeSpan.FromSeconds(_options.JobAralikSaniye), stoppingToken);
            }
        }
    }
}
