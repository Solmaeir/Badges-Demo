namespace ProfilRozetleriModulu.Models
{
    // Seviye/XP, UserProfile'dan ayrı bir tabloda tutuluyor: giriş serisi
    // (UserProfile) ve seviye/XP (bu tablo) birbirinden bağımsız güncelleniyor,
    // biri değişirken diğerinin şemasına dokunmaya gerek kalmıyor.
    public class UserLevel
    {
        public int UserId { get; set; }
        public int Level { get; set; }
        public int XP { get; set; }
        public DateTime? LastXPUpdateDate { get; set; }
    }
}
