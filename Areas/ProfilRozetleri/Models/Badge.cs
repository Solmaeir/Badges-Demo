namespace ProfilRozetleriModulu.Models
{
    // Rozet tanımı. ModuleId yalnızca Module/Discovery'de,
    // ExternalSignalKey yalnızca ExternalSignal'da dolu olmalı — bu kural
    // DB'de CHECK constraint ile de uygulanıyor, bkz.
    // 01-profilrozetleri-tables.sql ve 06-externalsignal-ekleme.sql.
    public class Badge
    {
        public int BadgeId { get; set; }
        public string BadgeName { get; set; }
        public string BadgeDescription { get; set; }
        public string IconPath { get; set; }
        public BadgeType BadgeType { get; set; }
        public int RequiredValue { get; set; }
        public int? ModuleId { get; set; }

        // Yalnızca BadgeType.ExternalSignal'da dolu. IExternalAchievementProvider
        // seam'ine "hangi koşulu soruyorum" diye geçilen serbest metin anahtar
        // (örnek: "wizard-tour-completed") — host bu anahtarı yorumlar.
        public string? ExternalSignalKey { get; set; }
    }
}
