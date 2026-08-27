using Microsoft.EntityFrameworkCore;
using ProfilRozetleriModulu.Models;

namespace ProfilRozetleriModulu.Data
{
    public class UserBadgeProgressDataService : IUserBadgeProgressDataService
    {
        private readonly ProfilRozetleriDbContext _context;

        public UserBadgeProgressDataService(ProfilRozetleriDbContext context)
        {
            _context = context;
        }

        public UserBadgeProgress GetByUserBadgeId(int userBadgeId)
        {
            return _context.UserBadgeProgresses.FirstOrDefault(x => x.UserBadgeId == userBadgeId);
        }

        public bool Add(UserBadgeProgress userBadgeProgress)
        {
            _context.UserBadgeProgresses.Add(userBadgeProgress);
            return _context.SaveChanges() > 0;
        }

        public bool Update(UserBadgeProgress userBadgeProgress)
        {
            _context.UserBadgeProgresses.Update(userBadgeProgress);
            return _context.SaveChanges() > 0;
        }
    }
}
