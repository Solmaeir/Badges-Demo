using Microsoft.EntityFrameworkCore;
using ProfilRozetleriModulu.Models;

namespace ProfilRozetleriModulu.Data
{
    public class UserProfileDataService : IUserProfileDataService
    {
        private readonly ProfilRozetleriDbContext _context;

        public UserProfileDataService(ProfilRozetleriDbContext context)
        {
            _context = context;
        }

        public UserProfile GetByUserId(int userId)
        {
            return _context.UserProfiles.FirstOrDefault(x => x.UserId == userId);
        }

        public bool Add(UserProfile userProfile)
        {
            _context.UserProfiles.Add(userProfile);
            return _context.SaveChanges() > 0;
        }

        public bool Update(UserProfile userProfile)
        {
            _context.UserProfiles.Update(userProfile);
            return _context.SaveChanges() > 0;
        }
    }
}
