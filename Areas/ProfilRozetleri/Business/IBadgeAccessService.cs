namespace ProfilRozetleriModulu.Business
{
    // Rozet Yönetimi ekranının yetki konusunda host sisteme sorduğu tek nokta.
    // Modülün kendi rol/yetki kavramı yoktur; yalnızca "geçerli kullanıcıda şu
    // yetki var mı" diye sorar (Wizard modülünün IWizardAccessService'iyle
    // aynı felsefe — hangi host sisteme taşınırsa taşınsın, yetki kuralının
    // kendisi modülün içinde tutulmaz, host'a sorulur).
    //
    // Soru geçerli kullanıcı üzerinedir, kimlik parametresi almaz — host'un
    // kendi yetki mekanizması (örn. User.IsInRole) zaten o anki istek sahibi
    // üzerinden çalışıyorsa, ayrıca bir id'den kullanıcı aramaya gerek kalmaz.
    public interface IBadgeAccessService
    {
        // Yetki adı boşsa çağrılmaz; boş ad "herkese açık" anlamına gelir.
        bool HasPermission(string permissionName);
    }
}
