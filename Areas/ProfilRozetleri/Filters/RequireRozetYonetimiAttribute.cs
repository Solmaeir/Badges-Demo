using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ProfilRozetleriModulu.Business;

namespace ProfilRozetleriModulu.Filters
{
    // Rozet Yönetimi ekranını korur. Yetki kararı modülün kendisinde değil,
    // IBadgeAccessService uygulamasındadır; burası yalnızca sorar.
    //
    // Aranan yetkinin adı da koda gömülü değil, yapılandırmadan geliyor
    // (ProfilRozetleriOptions.YonetimYetkisiAdi) — host sistemde bu yetki
    // hangi isimle duruyorsa appsettings.json'da o yazılır.
    //
    // Servisler constructor ile değil RequestServices üzerinden alınıyor:
    // attribute'lar normal DI ile beslenmiyor, [ServiceFilter] gibi ek bir
    // kayıt gerekirdi. Bu yol sayesinde filtre sınıfın üstüne düz yazılabiliyor.
    //
    // Reddedince 403 dönülüyor ve modülün kendi uyarı sayfası gösteriliyor;
    // host sisteme "şu adrese git" demek, o adresin var olduğunu varsaymak olurdu.
    public class RequireRozetYonetimiAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var services = context.HttpContext.RequestServices;

            var accessService = services.GetRequiredService<IBadgeAccessService>();
            var options = services.GetRequiredService<IOptions<ProfilRozetleriOptions>>().Value;

            if (!accessService.HasPermission(options.YonetimYetkisiAdi))
            {
                // Boş bir 403 yerine açıklamalı bir sayfa: kullanıcı bembeyaz
                // ekranı sistemin çökmesi sanmasın.
                //
                // Controller'ın ViewData'sı ödünç alınıyor ki sayfa host'un
                // düzeniyle (menü, üst çubuk) render edilebilsin. Controller
                // değilse — beklenmedik bir kullanım — düz 403'e düşüyoruz.
                if (context.Controller is Controller controller)
                {
                    context.Result = new ViewResult
                    {
                        ViewName = "~/Areas/ProfilRozetleri/Views/BadgeAdmin/AccessDenied.cshtml",
                        ViewData = controller.ViewData,
                        StatusCode = StatusCodes.Status403Forbidden
                    };

                    return;
                }

                context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
