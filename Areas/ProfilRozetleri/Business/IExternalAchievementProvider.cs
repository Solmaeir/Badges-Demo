namespace ProfilRozetleriModulu.Business
{
    // Modülün host sisteme sorduğu üçüncü soru: "WebLog/modül ziyaretiyle
    // ilgisi olmayan şu koşul (signalKey) bu kullanıcı için sağlandı mı?"
    // Modül signalKey'in ne anlama geldiğini BİLMEZ — sadece Badge tablosunda
    // ExternalSignalKey alanına yazılmış serbest metni olduğu gibi host'a
    // iletir. Host bu anahtarı yorumlayıp cevap verir (örnek: bu projede
    // "wizard-tour-completed" → WizardTourAchievementProvider, Wizard
    // modülünün WizardStepViews tablosuna bakar). Böylece ProfilRozetleri
    // Wizard'ın (ya da başka bir modülün) varlığından hiç haberdar olmaz;
    // iki modül birbirine değil, host'a bağlıdır.
    public interface IExternalAchievementProvider
    {
        bool IsAchieved(int userId, string signalKey);

        // Yönetim ekranının "hangi sinyaller seçilebilir" dropdown'ı için.
        // Host, gerçekten uyguladığı signalKey'leri (ve okunabilir bir
        // etiketi) burada listeler — modül kendi başına bir liste icat
        // etmiyor, çünkü hangi sinyallerin var olduğunu yalnızca host bilir.
        List<ExternalSignalDescriptor> GetSupportedSignals();
    }
}
