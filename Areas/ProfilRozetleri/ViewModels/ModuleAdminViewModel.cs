using ProfilRozetleriModulu.Models;

namespace ProfilRozetleriModulu.ViewModels
{
    // Modül Yönetimi ekranının veri şekli: mevcut modüller + henüz Modules
    // tablosunda karşılığı olmayan, sitede otomatik keşfedilmiş controller'lar.
    public class ModuleAdminViewModel
    {
        public List<Module> MevcutModuller { get; set; } = new();
        public List<string> KesfedilenEksikler { get; set; } = new();
    }
}
