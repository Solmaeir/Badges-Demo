namespace ProfilRozetleriModulu.Business
{
    // Modülün host sisteme sorduğu ikinci soru: "şu an oturumda kim var?"
    // (Wizard'daki IWizardUserProvider ile aynı rol.) null dönerse giriş
    // yapılmamış demektir — ProfilRozetleriGirisGerekliAttribute buna göre
    // yönlendirir.
    public interface ICurrentUserProvider
    {
        int? GetCurrentUserId();
    }
}
