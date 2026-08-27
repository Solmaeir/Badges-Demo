using Microsoft.EntityFrameworkCore;
using ProfilRozetleriModulu.Models;

namespace ProfilRozetleriModulu.Data
{
    public class UserBadgeDataService : IUserBadgeDataService
    {
        private readonly ProfilRozetleriDbContext _context;

        public UserBadgeDataService(ProfilRozetleriDbContext context)
        {
            _context = context;
        }

        public List<UserBadge> GetByUserId(int userId)
        {
            return _context.UserBadges.Where(x => x.UserId == userId).ToList();
        }

        public UserBadge GetByUserAndBadge(int userId, int badgeId)
        {
            return _context.UserBadges.FirstOrDefault(x => x.UserId == userId && x.BadgeId == badgeId);
        }

        public UserBadge GetById(int userBadgeId)
        {
            return _context.UserBadges.FirstOrDefault(x => x.UserBadgeId == userBadgeId);
        }

        public bool Add(UserBadge userBadge)
        {
            _context.UserBadges.Add(userBadge);
            return _context.SaveChanges() > 0;
        }

        public bool Update(UserBadge userBadge)
        {
            _context.UserBadges.Update(userBadge);
            return _context.SaveChanges() > 0;
        }
    }
}
