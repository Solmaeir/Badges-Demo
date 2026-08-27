using Microsoft.EntityFrameworkCore;
using ProfilRozetleriModulu.Models;

namespace ProfilRozetleriModulu.Data
{
    public class BadgeProcessStateDataService : IBadgeProcessStateDataService
    {
        private readonly ProfilRozetleriDbContext _context;

        public BadgeProcessStateDataService(ProfilRozetleriDbContext context)
        {
            _context = context;
        }

        public BadgeProcessState Get()
        {
            return _context.BadgeProcessStates.FirstOrDefault(x => x.Id == 1);
        }

        public bool Update(BadgeProcessState state)
        {
            _context.BadgeProcessStates.Update(state);
            return _context.SaveChanges() > 0;
        }
    }
}
