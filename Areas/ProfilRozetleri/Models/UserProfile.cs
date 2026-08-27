namespace ProfilRozetleriModulu.Models
{
    // Users tablosunun birebir karşılığı değil, ona 1:1 eklenen bir uydu
    // tablo. UserId hem PK hem host'un Users tablosuna FK'dır — navigation
    // property yok, taşınabilirlik için sade int (host'un Users şeması ne
    // olursa olsun modül yalnızca bu int'i bilir).
    public class UserProfile
    {
        public int UserId { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public int ConsecutiveLoginDays { get; set; }
    }
}
