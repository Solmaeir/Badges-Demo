using ProfilRozetleriModulu.Models;

namespace ProfilRozetleriModulu.Data
{
    public interface IUserBadgeDataService
    {
        List<UserBadge> GetByUserId(int userId);
        UserBadge GetByUserAndBadge(int userId, int badgeId);
        UserBadge GetById(int userBadgeId);
        bool Add(UserBadge userBadge);
        bool Update(UserBadge userBadge);
    }
}
