namespace ProfilRozetleriModulu.Models
{
    // Bir rozetin hangi koşula bağlı olduğunu belirler — BadgeBusinessService
    // her tür için farklı bir kazanım kontrolü çalıştırır:
    //
    // - System: sistem geneli, kaç gün üst üste giriş yapıldığına bakar
    //   (UserProfile.ConsecutiveLoginDays >= Badge.RequiredValue).
    // - Module: belirli bir modüle ard arda kaç gün girildiğine bakar
    //   (UserBadgeProgress.RepeatCount >= Badge.RequiredValue).
    // - Discovery: streak gerektirmez, modül bir kez (15+ sn) ziyaret
    //   edilince hemen kazanılır ("sadece o modülü kullandın" tarzı rozet).
    // - ExternalSignal: WebLog/modül ziyaretiyle ilgisi olmayan, host'un
    //   IExternalAchievementProvider seam'i üzerinden cevapladığı dış bir
    //   koşula bağlı (örnek: "Wizard turunu tamamladın").
    //
    // Module/Discovery'de Badge.ModuleId, ExternalSignal'da
    // Badge.ExternalSignalKey dolu olmak zorunda — bu kural hem
    // BadgeAdminService.DogrulaVeOlustur'da hem DB'deki
    // CK_Badges_ModuleId_TypeUyumu constraint'inde uygulanıyor.
    public enum BadgeType
    {
        System = 0,
        Module = 1,
        Discovery = 2,
        ExternalSignal = 3
    }
}
