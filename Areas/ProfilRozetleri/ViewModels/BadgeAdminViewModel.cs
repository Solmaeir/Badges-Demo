using System.ComponentModel.DataAnnotations;
using ProfilRozetleriModulu.Business;
using ProfilRozetleriModulu.Models;

namespace ProfilRozetleriModulu.ViewModels
{
    // Rozet Yönetimi ekranının Add/Edit formu. BadgeType'a göre hangi
    // alanların anlamlı olduğu değişir (Module/Discovery -> ModuleId,
    // ExternalSignal -> ExternalSignalKey) - bu View'de JS ile gösterilip
    // gizleniyor, doğrulama ise BadgeAdminService'te (Business katmanı).
    public class BadgeAdminViewModel
    {
        public int BadgeId { get; set; }

        [Required(ErrorMessage = "Rozet adı gerekli.")]
        [StringLength(100)]
        [Display(Name = "Rozet Adı")]
        public string BadgeName { get; set; } = "";

        [StringLength(500)]
        [Display(Name = "Açıklama")]
        public string? BadgeDescription { get; set; }

        [Display(Name = "İkon")]
        public string? IconPath { get; set; }

        [Required]
        [Display(Name = "Rozet Türü")]
        public BadgeType BadgeType { get; set; } = BadgeType.System;

        [Range(1, 3650, ErrorMessage = "1 ile 3650 arasında bir sayı olmalı.")]
        [Display(Name = "Gereken Sayı")]
        public int RequiredValue { get; set; } = 1;

        [Display(Name = "Modül")]
        public int? ModuleId { get; set; }

        [StringLength(100)]
        [Display(Name = "Dış Sinyal")]
        public string? ExternalSignalKey { get; set; }

        // Formu doldurmak için — POST'ta gelen değerler kullanılmaz, her
        // seferinde controller tarafından tazeleniyor.
        public List<Module> ModuleSecenekleri { get; set; } = new();
        public List<ExternalSignalDescriptor> SinyalSecenekleri { get; set; } = new();
        public List<string> IkonSecenekleri { get; set; } = new();
    }
}
