namespace ProfilRozetleriModulu.ViewModels
{
    // BadgesController.GetData()'nın JSON olarak döndürdüğü, MyBadges.cshtml
    // ve navbar popup'ının (_Layout.cshtml) fetch ile tükettiği veri şekli.
    // EarnedBadges/UnearnedBadges ayrımı, sayfadaki "Kazanılan"/"Kazanılmamış"
    // iki ayrı bölümün doğrudan karşılığı — View bu iki listeyi ayrı ayrı
    // render eder.
    public class ProfileBadgesViewModel
    {
        public int Level { get; set; }
        public int XP { get; set; }

        // Mevcut kademe içindeki XP yüzdesi (0-100) — profil sayfasındaki
        // ilerleme çubuğu için. Yüzde hesabı Business katmanında
        // (ProfilRozetleriOptions.SeviyeBasinaXP) yapılıyor; View bir sabit
        // taşımasın diye.
        public int XPPercentToNextLevel { get; set; }

        // Bir sonraki seviyeye geçmek için toplamda gereken XP eşiği
        // (Level * SeviyeBasinaXP) — "110 / 200" gibi ham sayı gösterimi için.
        public int XPGerekenSeviyeEsigi { get; set; }

        public int ConsecutiveLoginDays { get; set; }

        public List<BadgeViewModel> EarnedBadges { get; set; } = new();
        public List<BadgeViewModel> UnearnedBadges { get; set; } = new();

        // Navbar popup'ındaki "son kazanılan rozet" önizlemesi için — en son
        // EarnedDate'e sahip rozet (null ise kullanıcı hiç rozet kazanmamış,
        // navbar bu durumda önizlemeyi gizler).
        public BadgeViewModel LastEarnedBadge { get; set; }
    }
}
