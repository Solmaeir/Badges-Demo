using Microsoft.EntityFrameworkCore;
using ProfilRozetleriModulu.Models;

namespace ProfilRozetleriModulu.Data
{
    public class UserLevelDataService : IUserLevelDataService
    {
        private readonly ProfilRozetleriDbContext _context;

        public UserLevelDataService(ProfilRozetleriDbContext context)
        {
            _context = context;
        }

        public UserLevel GetByUserId(int userId)
        {
            return _context.UserLevels.FirstOrDefault(x => x.UserId == userId);
        }

        public bool Add(UserLevel userLevel)
        {
            _context.UserLevels.Add(userLevel);
            return _context.SaveChanges() > 0;
        }

        public bool Update(UserLevel userLevel)
        {
            _context.UserLevels.Update(userLevel);
            return _context.SaveChanges() > 0;
        }
    }
}
