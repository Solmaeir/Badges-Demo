using Microsoft.EntityFrameworkCore;
using ProfilRozetleriModulu.Models;

namespace ProfilRozetleriModulu.Data
{
    public class BadgeDataService : IBadgeDataService
    {
        private readonly ProfilRozetleriDbContext _context;

        public BadgeDataService(ProfilRozetleriDbContext context)
        {
            _context = context;
        }

        public List<Badge> GetAll()
        {
            return _context.Badges.ToList();
        }

        public Badge GetById(int badgeId)
        {
            return _context.Badges.FirstOrDefault(x => x.BadgeId == badgeId);
        }

        public List<Badge> GetByType(BadgeType badgeType)
        {
            return _context.Badges.Where(x => x.BadgeType == badgeType).ToList();
        }

        public List<Badge> GetByModuleId(int moduleId)
        {
            return _context.Badges.Where(x => x.ModuleId == moduleId).ToList();
        }

        public bool Add(Badge badge)
        {
            _context.Badges.Add(badge);
            return _context.SaveChanges() > 0;
        }

        public bool Update(Badge badge)
        {
            _context.Badges.Update(badge);
            return _context.SaveChanges() > 0;
        }

        public bool Delete(int badgeId)
        {
            var badge = _context.Badges.FirstOrDefault(x => x.BadgeId == badgeId);
            if (badge == null) return false;

            _context.Badges.Remove(badge);
            return _context.SaveChanges() > 0;
        }
    }
}
