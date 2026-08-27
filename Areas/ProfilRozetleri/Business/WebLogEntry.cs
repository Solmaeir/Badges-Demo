namespace ProfilRozetleriModulu.Business
{
    // IWebLogProvider'ın döndürdüğü tek bir istek kaydı. Host'un gerçek log
    // tablosunda çok daha fazla alan olabilir (Url, ServerIP, UserAgent,
    // Status...) ama rozet/seviye kuralları bunların hiçbirini kullanmıyor —
    // yalnızca "kim, hangi controller/action'a, ne zaman girdi" yeterli.
    public class WebLogEntry
    {
        public int Id { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public DateTime Tarih { get; set; }
        public int? UserId { get; set; }
    }
}
