using ProfilRozetleriModulu.Models;

namespace ProfilRozetleriModulu.Data
{
    public interface IModuleDataService
    {
        List<Module> GetAll();
        Module GetById(int moduleId);
        Module GetByControllerName(string controllerName);
        bool Add(Module module);
        bool Update(Module module);
        bool Delete(int moduleId);
    }
}
