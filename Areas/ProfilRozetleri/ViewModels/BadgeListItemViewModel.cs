namespace ProfilRozetleriModulu.ViewModels
{
    // BadgeAdminController.GetBadgesJson()'ın döndürdüğü, Rozet Yönetimi
    // tablosundaki tek bir satırın veri şekli. Türe göre etiket ("Sistem
    // (Giriş Serisi)" gibi) ve Modül/Sinyal metni burada, Business katmanında
    // hazırlanır — View yalnızca bu hazır alanları JS ile satıra döker,
    // kendi başına yorum/karar üretmez.
    public class BadgeListItemViewModel
    {
        public int BadgeId { get; set; }
        public string BadgeName { get; set; } = "";
        public string? IconPath { get; set; }
        public string BadgeTypeLabel { get; set; } = "";
        public int RequiredValue { get; set; }

        // Modül tipi rozetlerde "Modül #3" gibi, ExternalSignal'da anahtarın
        // kendisi, System'de null (View'de "—" gösterilir).
        public string? ModulSinyalMetni { get; set; }

        // true ise ModulSinyalMetni <code> içinde gösterilir (ExternalSignal),
        // false ise düz metin (Modül).
        public bool ModulSinyalKodMu { get; set; }
    }
}
