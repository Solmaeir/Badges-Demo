using ProfilRozetleriModulu.Models;

namespace ProfilRozetleriModulu.Data
{
    public interface IUserProfileDataService
    {
        UserProfile GetByUserId(int userId);
        bool Add(UserProfile userProfile);
        bool Update(UserProfile userProfile);
    }
}
