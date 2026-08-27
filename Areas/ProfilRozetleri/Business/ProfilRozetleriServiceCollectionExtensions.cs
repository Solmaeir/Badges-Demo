using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ProfilRozetleriModulu.Data;

namespace ProfilRozetleriModulu.Business
{
    // Modülün DI kaydı. Host'un tek yapması gereken bunu çağırıp kendi
    // IWebLogProvider / ICurrentUserProvider / IExternalAchievementProvider /
    // IBadgeAccessService uygulamalarını kaydetmek.
    public static class ProfilRozetleriServiceCollectionExtensions
    {
        // baglantiDizesi parametre olarak alınır — modül kendi DbContext'ini
        // kurar, host'un ana DbContext'ine yedi DbSet eklemesi gerekmez.
        public static IServiceCollection AddProfilRozetleriModulu(this IServiceCollection services, string baglantiDizesi, IConfiguration configuration)
        {
            services.AddDbContext<ProfilRozetleriDbContext>(options => options.UseSqlServer(baglantiDizesi));

            services.AddScoped<IModuleDataService, ModuleDataService>();
            services.AddScoped<IUserProfileDataService, UserProfileDataService>();
            services.AddScoped<IUserLevelDataService, UserLevelDataService>();
            services.AddScoped<IBadgeDataService, BadgeDataService>();
            services.AddScoped<IUserBadgeDataService, UserBadgeDataService>();
            services.AddScoped<IUserBadgeProgressDataService, UserBadgeProgressDataService>();
            services.AddScoped<IBadgeProcessStateDataService, BadgeProcessStateDataService>();

            services.AddScoped<IBadgeBusinessService, BadgeBusinessService>();
            // Rozet Yönetimi ekranının iş kuralları (BadgeAdminController).
            services.AddScoped<IBadgeAdminService, BadgeAdminService>();

            services.Configure<ProfilRozetleriOptions>(configuration.GetSection("ProfilRozetleriModulu"));

            // Arka plan işi: WebLog'u periyodik tarar, rozet/seviye
            // güncellemelerini burada yapar (bkz. BadgeProcessingJob).
            services.AddHostedService<BadgeProcessingJob>();

            // Dört seam için güvenli varsayılanlar. TryAdd olduğu için host kendi
            // uygulamasını kaydettiyse hiç devreye girmez.
            services.TryAddScoped<IWebLogProvider, UnconfiguredWebLogProvider>();
            services.TryAddScoped<ICurrentUserProvider, UnconfiguredCurrentUserProvider>();
            services.TryAddScoped<IExternalAchievementProvider, UnconfiguredExternalAchievementProvider>();
            services.TryAddScoped<IBadgeAccessService, UnconfiguredBadgeAccessService>();

            return services;
        }
    }
}
