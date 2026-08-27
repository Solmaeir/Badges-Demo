using ProfilRozetleriModulu.ViewModels;

namespace ProfilRozetleriModulu.Business
{
    public interface IBadgeBusinessService
    {
        // BadgeProcessingJob tarafından periyodik çağrılır.
        void ProcessNewWebLogEntries();

        // BadgesController.MyBadges() tarafından çağrılır.
        ProfileBadgesViewModel GetProfileBadges(int userId);
    }
}
