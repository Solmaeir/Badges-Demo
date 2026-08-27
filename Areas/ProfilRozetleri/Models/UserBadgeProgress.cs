namespace ProfilRozetleriModulu.Models
{
    // Yalnızca BadgeType.Module olan rozetler için doldurulur; hangi
    // UserBadge kaydına ait olduğu UserBadgeId üzerinden bilinir (BadgeId
    // veya UserId burada ayrıca tutulmuyor, tekilleşmiş kaynak UserBadge'de).
    public class UserBadgeProgress
    {
        public int UserBadgeProgressId { get; set; }
        public int UserBadgeId { get; set; }
        public DateTime? LastSeenDateThisModule { get; set; }
        public int RepeatCount { get; set; }
    }
}
