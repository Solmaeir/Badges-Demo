namespace ProfilRozetleriModulu.Business
{
    // Modülün host sisteme sorduğu tek soru: "BadgeProcessState imlecinden
    // sonraki yeni istek kayıtları neler?" Gerçek EDI'de bu, mevcut WebLog
    // tablosunu sorgulayan bir uygulamayla cevaplanır. Bu proje kendi test
    // WebLog'unu okuyan LocalWebLogProvider ile cevaplıyor (host tarafında,
    // modülün parçası değil — bkz. README-ProfilRozetleri.md).
    public interface IWebLogProvider
    {
        List<WebLogEntry> GetNewEntries(int lastProcessedLogId);

        // "Yeni rozet" etiketi için: kullanıcının belirli bir sayfaya en son
        // ne zaman girdiğini bulmak amacıyla en taze kayıtları ister (en
        // yeniden en eskiye sıralı). GetNewEntries'ten farklı bir soru şekli
        // olduğu için ayrı bir metot — cursor'a değil, belirli bir
        // kullanıcı+controller+action'a bağlı.
        List<WebLogEntry> GetRecentEntries(int userId, string controller, string action, int maxCount);
    }
}
