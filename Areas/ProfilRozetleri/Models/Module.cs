namespace ProfilRozetleriModulu.Models
{
    // Sistemde ayrı bir modül tablosu olmadığı için bu modül tarafından
    // oluşturuluyor. ControllerName, WebLog.Controller alanıyla birebir
    // eşleşecek şekilde doldurulmalı — eşleşme ProfilRozetleri iş kuralının
    // tek dayanağı.
    public class Module
    {
        public int ModuleId { get; set; }
        public string ModuleName { get; set; }
        public string ControllerName { get; set; }
    }
}
