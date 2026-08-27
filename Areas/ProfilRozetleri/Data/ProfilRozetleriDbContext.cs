using Microsoft.EntityFrameworkCore;
using ProfilRozetleriModulu.Models;

namespace ProfilRozetleriModulu.Data
{
    // Modülün kendi veritabanı bağlamı — host'un AppDbContext'ine eklenmesi
    // gerekmez. Aynı bağlantı dizesine ayrı bir DbContext ile bağlanmak
    // taşınabilirlik için şart: aksi halde her host bu 7 DbSet'i kendi ana
    // context'ine elle eklemek zorunda kalırdı. WebLog ve Users bu context'te
    // YOK — onlar host'un tabloları, modül onlara yalnızca IWebLogProvider
    // seam'i üzerinden erişir.
    public class ProfilRozetleriDbContext : DbContext
    {
        public ProfilRozetleriDbContext(DbContextOptions<ProfilRozetleriDbContext> options) : base(options)
        {
        }

        public DbSet<Module> Modules { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<UserLevel> UserLevels { get; set; }
        public DbSet<Badge> Badges { get; set; }
        public DbSet<UserBadge> UserBadges { get; set; }
        public DbSet<UserBadgeProgress> UserBadgeProgresses { get; set; }
        public DbSet<BadgeProcessState> BadgeProcessStates { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // EF Core'un PK kuralı "Id" ya da "{SınıfAdı}Id" bekler. UserProfile
            // ve UserLevel'da PK kasıtlı olarak "UserId" (Users'a 1:1 FK olduğu
            // için) — bu iki sınıf için PK'yı açıkça belirtmek gerekiyor.
            modelBuilder.Entity<UserProfile>().HasKey(x => x.UserId);
            modelBuilder.Entity<UserLevel>().HasKey(x => x.UserId);

            // EF Core, DbSet property adını (çoğul) tablo adı sayar. Bu beş
            // tablo SQL script'te (01-profilrozetleri-tables.sql) TEKİL adla
            // kuruldu — DbSet adı ile gerçek tablo adı burada eşleştirilmezse
            // EF Core "tablo bulunamadı" hatası verir. Modules/Badges zaten
            // çoğul isimlendirildiği için ayrıca eşlemeye gerek yok.
            modelBuilder.Entity<UserProfile>().ToTable("UserProfile");
            modelBuilder.Entity<UserLevel>().ToTable("UserLevel");
            modelBuilder.Entity<UserBadge>().ToTable("UserBadge");
            modelBuilder.Entity<UserBadgeProgress>().ToTable("UserBadgeProgress");
            modelBuilder.Entity<BadgeProcessState>().ToTable("BadgeProcessState");
        }
    }
}
