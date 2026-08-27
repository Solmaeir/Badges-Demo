using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ProfilRozetleriModulu.Business;

namespace ProfilRozetleriModulu.Filters
{
    // Giriş şartını host'un kendi (somut) oturum/Controller sınıflarına
    // bağımlı olmadan uygular — ICurrentUserProvider seam'i üzerinden sorar.
    // Modül Controller'ları host'un taban Controller sınıfından türemediği
    // için bu filtre elle uygulanır.
    public class ProfilRozetleriGirisGerekliAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var currentUserProvider = context.HttpContext.RequestServices.GetRequiredService<ICurrentUserProvider>();

            if (currentUserProvider.GetCurrentUserId() == null)
            {
                var options = context.HttpContext.RequestServices.GetRequiredService<IOptions<ProfilRozetleriOptions>>().Value;
                context.Result = new RedirectResult(options.GirisSayfasiUrl);
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
