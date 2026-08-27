namespace ProfilRozetleriModulu.Models
{
    // Her kullanıcı-rozet ikilisinin kazanım durumunu tutan ilişki tablosu.
    // (UserId, BadgeId) DB'de unique — bir kullanıcı bir rozete yalnızca bir
    // kez sahip olabilir.
    public class UserBadge
    {
        public int UserBadgeId { get; set; }
        public int UserId { get; set; }
        public int BadgeId { get; set; }
        public bool IsEarned { get; set; }
        public DateTime? EarnedDate { get; set; }
    }
}
