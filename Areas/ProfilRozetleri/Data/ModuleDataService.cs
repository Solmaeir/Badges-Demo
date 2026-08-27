using Microsoft.EntityFrameworkCore;
using ProfilRozetleriModulu.Models;

namespace ProfilRozetleriModulu.Data
{
    public class ModuleDataService : IModuleDataService
    {
        private readonly ProfilRozetleriDbContext _context;

        public ModuleDataService(ProfilRozetleriDbContext context)
        {
            _context = context;
        }

        public List<Module> GetAll()
        {
            return _context.Modules.ToList();
        }

        public Module GetById(int moduleId)
        {
            return _context.Modules.FirstOrDefault(x => x.ModuleId == moduleId);
        }

        public Module GetByControllerName(string controllerName)
        {
            return _context.Modules.FirstOrDefault(x => x.ControllerName == controllerName);
        }

        public bool Add(Module module)
        {
            _context.Modules.Add(module);
            return _context.SaveChanges() > 0;
        }

        public bool Update(Module module)
        {
            _context.Modules.Update(module);
            return _context.SaveChanges() > 0;
        }

        public bool Delete(int moduleId)
        {
            var module = _context.Modules.FirstOrDefault(x => x.ModuleId == moduleId);
            if (module == null) return false;

            _context.Modules.Remove(module);
            return _context.SaveChanges() > 0;
        }
    }
}
