namespace ProfilRozetleriModulu.Business
{
    // IExternalAchievementProvider.GetSupportedSignals()'ın döndürdüğü öğe.
    // Yönetim ekranındaki dropdown bunu kullanır — admin elle bir signalKey
    // yazıp hiçbir karşılığı olmayan (hiçbir zaman kazanılamayacak) bir rozet
    // oluşturamasın diye.
    public class ExternalSignalDescriptor
    {
        public string Key { get; set; } = "";
        public string Label { get; set; } = "";
    }
}
