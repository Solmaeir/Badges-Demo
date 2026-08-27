using ProfilRozetleriModulu.Models;

namespace ProfilRozetleriModulu.Data
{
    public interface IUserBadgeProgressDataService
    {
        UserBadgeProgress GetByUserBadgeId(int userBadgeId);
        bool Add(UserBadgeProgress userBadgeProgress);
        bool Update(UserBadgeProgress userBadgeProgress);
    }
}
