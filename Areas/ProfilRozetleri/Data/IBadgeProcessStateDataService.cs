using ProfilRozetleriModulu.Models;

namespace ProfilRozetleriModulu.Data
{
    // Tek satırlık imleç. Get her zaman o tek satırı döner (SQL script bunu
    // Id=1 ile seed eder); Update sadece UPDATE'tir, Add yok.
    public interface IBadgeProcessStateDataService
    {
        BadgeProcessState Get();
        bool Update(BadgeProcessState state);
    }
}
