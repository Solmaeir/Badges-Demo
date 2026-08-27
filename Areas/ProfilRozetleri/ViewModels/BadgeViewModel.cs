namespace ProfilRozetleriModulu.ViewModels
{
    // Tek bir rozet kartının verisi. Controller bunu JSON'a çevirir,
    // MyBadges.cshtml ve navbar popup'ındaki JS bu JSON'dan kartları
    // (rozetKartiOlustur) dinamik olarak oluşturur — sunucu tarafında HTML
    // üretilmez.
    public class BadgeViewModel
    {
        public int BadgeId { get; set; }
        public string BadgeName { get; set; }
        public string BadgeDescription { get; set; }
        public string IconPath { get; set; }
        public bool IsEarned { get; set; }
        public DateTime? EarnedDate { get; set; }

        // Yalnızca Module/Discovery tipi rozetlerde dolu — hover popup'ta
        // "hangi modülde" bilgisini göstermek için.
        public string ModuleName { get; set; }

        // BadgeDisplayUtils.IlerlemeMetni ile hazırlanmış "x/y" metni.
        public string ProgressText { get; set; }

        // Rozetlerim sayfasına en son ziyaretten SONRA kazanıldıysa true —
        // "Yeni" etiketi bunun için. Yalnızca kazanılmış rozetlerde anlamlı,
        // kilitli rozetlerde hep false.
        public bool IsNew { get; set; }
    }
}
