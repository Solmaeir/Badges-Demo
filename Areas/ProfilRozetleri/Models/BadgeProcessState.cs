namespace ProfilRozetleriModulu.Models
{
    // Arka plan işinin WebLog'da nereye kadar işleme yaptığını tutan tek
    // satırlık imleç (cursor). Id her zaman 1 — DB'de CHECK ile tekil satır
    // garanti edilir (bkz. 01-profilrozetleri-tables.sql).
    public class BadgeProcessState
    {
        public int Id { get; set; }
        public int LastProcessedLogId { get; set; }
        public DateTime? LastRunDate { get; set; }
    }
}
