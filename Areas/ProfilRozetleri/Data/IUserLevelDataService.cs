using ProfilRozetleriModulu.Models;

namespace ProfilRozetleriModulu.Data
{
    public interface IUserLevelDataService
    {
        UserLevel GetByUserId(int userId);
        bool Add(UserLevel userLevel);
        bool Update(UserLevel userLevel);
    }
}
