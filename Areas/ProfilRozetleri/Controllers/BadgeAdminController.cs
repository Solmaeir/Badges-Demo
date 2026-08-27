using Microsoft.AspNetCore.Mvc;
using ProfilRozetleriModulu.Business;
using ProfilRozetleriModulu.Filters;
using ProfilRozetleriModulu.ViewModels;

namespace ProfilRozetleriModulu.Controllers
{
    // Rozetlerin yönetildiği ekran: rozet eklemek/düzenlemek kod
    // değiştirmeden, bu form üzerinden yapılır.
    //
    // İki filtre sırayla çalışır: önce giriş şartı (ProfilRozetleriGirisGerekli
    // — giriş yoksa login sayfasına yönlendirir), sonra yönetim yetkisi
    // (RequireRozetYonetimi — giriş var ama yetki yoksa 403 + uyarı sayfası
    // gösterir). Yetki kararının kendisi modülde değil, host'un
    // IBadgeAccessService uygulamasındadır.
    [Area("ProfilRozetleri")]
    [ProfilRozetleriGirisGerekli]
    [RequireRozetYonetimi]
    public class BadgeAdminController : Controller
    {
        private readonly IBadgeAdminService _badgeAdminService;

        public BadgeAdminController(IBadgeAdminService badgeAdminService)
        {
            _badgeAdminService = badgeAdminService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            ViewData["Title"] = "Rozet Yönetimi";
            return View();
        }

        // Index.cshtml'nin tablosu bunu çağırıp kolonları JS ile kurar —
        // sunucuda elle <tr>/<td> yazılmaz (işlem butonları hariç).
        [HttpGet]
        public IActionResult GetBadgesJson()
        {
            return Json(_badgeAdminService.GetBadgeListItems());
        }

        [HttpGet]
        public IActionResult Add()
        {
            ViewData["Title"] = "Yeni Rozet";
            return View(Doldur(new BadgeAdminViewModel()));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(BadgeAdminViewModel model)
        {
            ViewData["Title"] = "Yeni Rozet";

            if (!ModelState.IsValid)
            {
                return View(Doldur(model));
            }

            if (!_badgeAdminService.Add(model, out var hata))
            {
                ModelState.AddModelError(string.Empty, hata);
                return View(Doldur(model));
            }

            TempData["RozetMesaj"] = "Rozet eklendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            ViewData["Title"] = "Rozeti Düzenle";

            var badge = _badgeAdminService.GetById(id);
            if (badge == null)
            {
                TempData["RozetMesaj"] = "Rozet bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            var model = new BadgeAdminViewModel
            {
                BadgeId = badge.BadgeId,
                BadgeName = badge.BadgeName,
                BadgeDescription = badge.BadgeDescription,
                IconPath = badge.IconPath,
                BadgeType = badge.BadgeType,
                RequiredValue = badge.RequiredValue,
                ModuleId = badge.ModuleId,
                ExternalSignalKey = badge.ExternalSignalKey
            };

            return View(Doldur(model));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(BadgeAdminViewModel model)
        {
            ViewData["Title"] = "Rozeti Düzenle";

            if (!ModelState.IsValid)
            {
                return View(Doldur(model));
            }

            if (!_badgeAdminService.Update(model, out var hata))
            {
                ModelState.AddModelError(string.Empty, hata);
                return View(Doldur(model));
            }

            TempData["RozetMesaj"] = "Rozet güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            TempData["RozetMesaj"] = _badgeAdminService.Delete(id)
                ? "Rozet silindi."
                : "Rozet silinemedi. Kayıt bulunamadı.";

            return RedirectToAction(nameof(Index));
        }

        // --- Modül Yönetimi -----------------------------------------------------------
        // Bu ekranın hem tam sayfa hali (Modules.cshtml) hem Rozet Ekle/Düzenle
        // formundaki popup'ı aynı akışı kullanır: sayfa açılışında GetModulesJson
        // çağrılır, tablolar JS ile kurulur; Ekle/Sil de AJAX ile gönderilir ve
        // JSON durum döner — hiçbir yerde sunucu tarafında elle <tr>/<td>
        // üretilmez (bkz. Views/BadgeAdmin/_ModulYonetimiScript.cshtml).

        [HttpGet]
        public IActionResult Modules()
        {
            ViewData["Title"] = "Modül Yönetimi";
            return View();
        }

        [HttpGet]
        public IActionResult GetModulesJson()
        {
            return Json(_badgeAdminService.GetModuleManagementData());
        }

        // Rozet Ekle/Düzenle formundaki Modül dropdown'ını, popup'ta bir modül
        // eklenip silindikten sonra sayfa yenilenmeden tazelemek için.
        [HttpGet]
        public IActionResult ModuleOptions()
        {
            var secenekler = _badgeAdminService.GetModules()
                .Select(m => new { id = m.ModuleId, text = $"{m.ModuleName} ({m.ControllerName})" });

            return Json(secenekler);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddModule(string moduleName, string controllerName)
        {
            var basarili = _badgeAdminService.AddModule(moduleName, controllerName, out var hata);
            return Json(new { success = basarili, message = basarili ? "Modül eklendi." : hata });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteModule(int id)
        {
            var basarili = _badgeAdminService.DeleteModule(id, out var hata);
            return Json(new { success = basarili, message = basarili ? "Modül silindi." : hata });
        }

        // Add/Edit GET+POST'un hepsinde dropdown/ikon listeleri aynı şekilde
        // tazelenmesi gerektiği için tek yerde toplandı.
        private BadgeAdminViewModel Doldur(BadgeAdminViewModel model)
        {
            model.ModuleSecenekleri = _badgeAdminService.GetModules();
            model.SinyalSecenekleri = _badgeAdminService.GetExternalSignals();
            model.IkonSecenekleri = _badgeAdminService.GetAvailableIcons();
            return model;
        }
    }
}
