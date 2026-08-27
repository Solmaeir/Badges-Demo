using Microsoft.AspNetCore.Mvc;
using ProfilRozetleriModulu.Business;
using ProfilRozetleriModulu.Filters;

namespace ProfilRozetleriModulu.Controllers
{
    // Adı "ProfileController" değil "BadgesController": bu ekran genel bir
    // "profil" sayfası değil, yalnızca rozet/seviye görüntülüyor — host'un
    // kendi profil sayfası varsa isim karışmasın diye. Controller yalnızca
    // BadgeBusinessService'i çağırıp sonucu View'e taşır; kendi içinde
    // foreach ya da iş kuralı yok, hepsi Business katmanında.
    [Area("ProfilRozetleri")]
    [ProfilRozetleriGirisGerekli]
    public class BadgesController : Controller
    {
        private readonly IBadgeBusinessService _badgeBusinessService;
        private readonly ICurrentUserProvider _currentUserProvider;

        public BadgesController(IBadgeBusinessService badgeBusinessService, ICurrentUserProvider currentUserProvider)
        {
            _badgeBusinessService = badgeBusinessService;
            _currentUserProvider = currentUserProvider;
        }

        public IActionResult MyBadges()
        {
            ViewBag.Special = new
            {
                SayfaBasligi = "Rozetlerim"
            };

            return View();
        }

        [HttpGet]
        public JsonResult GetData()
        {
            // ProfilRozetleriGirisGerekliAttribute zaten giriş şartını
            // sağladığı için burada userId her zaman dolu.
            var userId = _currentUserProvider.GetCurrentUserId()!.Value;
            return Json(_badgeBusinessService.GetProfileBadges(userId));
        }
    }
}
