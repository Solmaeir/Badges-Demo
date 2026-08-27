using ProfilRozetleriModulu.Models;

namespace ProfilRozetleriModulu.Data
{
    public interface IBadgeDataService
    {
        List<Badge> GetAll();
        Badge GetById(int badgeId);
        List<Badge> GetByType(BadgeType badgeType);
        // Module VE Discovery tipi rozetler aynı moduleId'ye bağlı olabilir;
        // ModuleEntryCheck ikisini birden değerlendirdiği için tek sorguda döner.
        List<Badge> GetByModuleId(int moduleId);
        bool Add(Badge badge);
        bool Update(Badge badge);
        bool Delete(int badgeId);
    }
}
