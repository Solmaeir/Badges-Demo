using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Options;
using ProfilRozetleriModulu.Controllers;
using ProfilRozetleriModulu.Data;
using ProfilRozetleriModulu.Models;
using ProfilRozetleriModulu.ViewModels;

namespace ProfilRozetleriModulu.Business
{
    public class BadgeAdminService : IBadgeAdminService
    {
        // Bu modülün KENDİ controller'ları — "modül" olarak eklenip rozetlere
        // bağlanması hiçbir zaman anlamlı değil (rozet sisteminin kendisini
        // ziyaret etmek bir "iş modülü" ziyareti sayılmaz). Host'a ait değil,
        // bu yüzden konfigürasyona değil koda yazıldı: modül nereye taşınırsa
        // taşınsın bu iki controller aynı kalır.
        //
        // nameof() ile türetiliyor (elle yazılmış "Badges"/"BadgeAdmin" metin
        // sabitleri yerine): sınıf adı değişirse (yeniden adlandırma) bu liste
        // otomatik güncellenir, derleyici de kontrol eder. ASP.NET Core'un
        // routing kuralı "Controller" sonekini attığı için burada da aynı
        // şekilde atılıyor — WebLog.Controller'a yazılan değerle AYNI kural.
        private static readonly HashSet<string> ModulunKendiControllerlari = new(StringComparer.OrdinalIgnoreCase)
        {
            ControllerAdi<BadgesController>(),
            ControllerAdi<BadgeAdminController>()
        };

        private static string ControllerAdi<T>() where T : Microsoft.AspNetCore.Mvc.Controller
            => typeof(T).Name.EndsWith("Controller", StringComparison.Ordinal)
                ? typeof(T).Name[..^"Controller".Length]
                : typeof(T).Name;

        private readonly IBadgeDataService _badgeDataService;
        private readonly IModuleDataService _moduleDataService;
        private readonly IExternalAchievementProvider _externalAchievementProvider;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IActionDescriptorCollectionProvider _actionDescriptorCollectionProvider;
        private readonly ProfilRozetleriOptions _options;

        public BadgeAdminService(
            IBadgeDataService badgeDataService,
            IModuleDataService moduleDataService,
            IExternalAchievementProvider externalAchievementProvider,
            IWebHostEnvironment webHostEnvironment,
            IActionDescriptorCollectionProvider actionDescriptorCollectionProvider,
            IOptions<ProfilRozetleriOptions> options)
        {
            _badgeDataService = badgeDataService;
            _moduleDataService = moduleDataService;
            _externalAchievementProvider = externalAchievementProvider;
            _webHostEnvironment = webHostEnvironment;
            _actionDescriptorCollectionProvider = actionDescriptorCollectionProvider;
            _options = options.Value;
        }

        // Bir controller adının modül olarak eklenmesi YASAK mı? Hem otomatik
        // keşfedilen listeyi süzmek için hem de AddModule'daki sunucu tarafı
        // doğrulama için tek yerde kullanılıyor — ikisi de aynı kurala uysun diye.
        private bool ControllerYasakliMi(string controllerName)
        {
            var haricTutulanlar = new HashSet<string>(_options.HaricTutulanControllerler, StringComparer.OrdinalIgnoreCase);
            return ModulunKendiControllerlari.Contains(controllerName) || haricTutulanlar.Contains(controllerName);
        }

        // ASP.NET Core'un kendi routing kaydından okunuyor — host'un istek
        // günlüğüne (WebLog) yazdığı Controller değeriyle AYNI kaynak. Elle
        // bir liste tutmak yerine bunu kullanmak, "yazım hatası yüzünden rozet
        // hiç tetiklenmiyor" riskini tamamen ortadan kaldırıyor. Bu YALNIZCA
        // bu ASP.NET Core uygulamasının kendi routing kaydını okuyabildiği için
        // çalışıyor — farklı bir sisteme taşınırken (örn. gerçek EDI, tamamen
        // farklı bir teknolojiyle yazılmışsa) bu keşif hiç mümkün olmayabilir,
        // bu yüzden AddModule listede olmayan bir controllerName'i de kabul
        // ediyor (bkz. AddModule, ControllerYasakliMi).
        //
        // Sitedeki HER controller döner (login sayfası, panel, başka modüllerin
        // yönetim ekranları dahil) — bunların hepsi "iş modülü" değil, bu yüzden
        // ControllerYasakliMi ile süzülüyor.
        public List<string> GetDiscoveredControllerNames()
        {
            return _actionDescriptorCollectionProvider.ActionDescriptors.Items
                .OfType<ControllerActionDescriptor>()
                .Select(a => a.ControllerName)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Where(ad => !ControllerYasakliMi(ad))
                .OrderBy(ad => ad)
                .ToList();
        }

        public List<string> GetUnregisteredControllerNames()
        {
            var kayitliOlanlar = _moduleDataService.GetAll()
                .Select(m => m.ControllerName)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            return GetDiscoveredControllerNames()
                .Where(ad => !kayitliOlanlar.Contains(ad))
                .ToList();
        }

        public bool AddModule(string moduleName, string controllerName, out string hata)
        {
            if (string.IsNullOrWhiteSpace(moduleName))
            {
                hata = "Modül adı gerekli.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(controllerName))
            {
                hata = "Bir controller seçilmeli.";
                return false;
            }

            if (_moduleDataService.GetByControllerName(controllerName) != null)
            {
                hata = "Bu controller zaten bir modüle bağlı.";
                return false;
            }

            // controllerName burada iki farklı yoldan gelebilir: "Keşfedilen
            // Controller'lar" listesinden seçilmiş (bu sitenin gerçek bir
            // controller'ı) ya da "Elle Ekle" formuna yazılmış serbest metin
            // (bu test ortamının keşfedemediği, örn. gerçek EDI'ye özgü bir
            // controller adı). İkinci yol var olma kontrolünden GEÇMEZ — sadece
            // yasaklı listesine (modülün kendi controller'ları + host'un hariç
            // tuttukları) bakılır, WebLog'un gerçekten o adla kayıt üretip
            // üretmediği admin'in sorumluluğunda.
            if (ControllerYasakliMi(controllerName))
            {
                hata = "Bu controller modül olarak eklenemez.";
                return false;
            }

            var basarili = _moduleDataService.Add(new Module { ModuleName = moduleName, ControllerName = controllerName });
            hata = basarili ? "" : "Modül eklenemedi.";
            return basarili;
        }

        public bool DeleteModule(int moduleId, out string hata)
        {
            // Badges.ModuleId, Module/Discovery tipinde CHECK ile zorunlu
            // kılınmış (NULL olamaz) — bağlı rozet varken modülü silmek CHECK'i
            // ihlal ederdi. Rozet silmedeki gibi cascade yerine burada
            // engellemek doğru: "modülü sil" ile "o modüle bağlı rozetleri de
            // sil" çok farklı, kullanıcıyı şaşırtabilecek bir kısayol olurdu.
            var baglantiliRozetVarMi = _badgeDataService.GetAll().Any(b => b.ModuleId == moduleId);
            if (baglantiliRozetVarMi)
            {
                hata = "Bu modüle bağlı en az bir rozet var. Önce o rozetleri silin ya da başka bir modüle taşıyın.";
                return false;
            }

            hata = "";
            return _moduleDataService.Delete(moduleId);
        }

        public List<Badge> GetAll() => _badgeDataService.GetAll();

        public List<BadgeListItemViewModel> GetBadgeListItems()
        {
            return _badgeDataService.GetAll()
                .Select(BadgeListOgesiOlustur)
                .ToList();
        }

        private static BadgeListItemViewModel BadgeListOgesiOlustur(Badge rozet)
        {
            return new BadgeListItemViewModel
            {
                BadgeId = rozet.BadgeId,
                BadgeName = rozet.BadgeName,
                IconPath = rozet.IconPath,
                BadgeTypeLabel = TurEtiketi(rozet.BadgeType),
                RequiredValue = rozet.RequiredValue,
                ModulSinyalMetni = rozet.ModuleId.HasValue ? $"Modül #{rozet.ModuleId}" : rozet.ExternalSignalKey,
                ModulSinyalKodMu = !string.IsNullOrWhiteSpace(rozet.ExternalSignalKey)
            };
        }

        private static string TurEtiketi(BadgeType tur) => tur switch
        {
            BadgeType.System => "Sistem (Giriş Serisi)",
            BadgeType.Module => "Modül (Ard Arda Giriş)",
            BadgeType.Discovery => "Keşif (Tek Seferlik)",
            BadgeType.ExternalSignal => "Dış Sinyal",
            _ => tur.ToString()
        };

        public Badge? GetById(int badgeId) => _badgeDataService.GetById(badgeId);

        public List<Module> GetModules() => _moduleDataService.GetAll();

        public List<ExternalSignalDescriptor> GetExternalSignals() => _externalAchievementProvider.GetSupportedSignals();

        public ModuleAdminViewModel GetModuleManagementData()
        {
            return new ModuleAdminViewModel
            {
                MevcutModuller = GetModules(),
                KesfedilenEksikler = GetUnregisteredControllerNames()
            };
        }

        public List<string> GetAvailableIcons()
        {
            var klasor = Path.Combine(_webHostEnvironment.WebRootPath, "Content", "Badges");

            if (!Directory.Exists(klasor))
            {
                return new List<string>();
            }

            return Directory.GetFiles(klasor, "*.png")
                .Select(Path.GetFileName)
                .OrderBy(ad => ad)
                .Select(ad => "/Content/Badges/" + ad)
                .ToList()!;
        }

        public bool Add(BadgeAdminViewModel model, out string hata)
        {
            if (!DogrulaVeOlustur(model, out hata, out var badge))
            {
                return false;
            }

            return _badgeDataService.Add(badge!);
        }

        public bool Update(BadgeAdminViewModel model, out string hata)
        {
            if (!DogrulaVeOlustur(model, out hata, out var badge))
            {
                return false;
            }

            badge!.BadgeId = model.BadgeId;
            return _badgeDataService.Update(badge);
        }

        public bool Delete(int badgeId) => _badgeDataService.Delete(badgeId);

        // BadgeType'a göre hangi alanların dolu/boş olması gerektiğini
        // uygular (bkz. CK_Badges_ModuleId_TypeUyumu): System'de ikisi de
        // NULL, Module/Discovery'de yalnızca ModuleId, ExternalSignal'da
        // yalnızca ExternalSignalKey dolu olmalı. Discovery ve
        // ExternalSignal'da RequiredValue her zaman 1'e sabitlenir — ikisi de
        // streak değil, tek koşulun sağlanıp sağlanmadığına bakar; admin
        // yanlışlıkla 5 girerse rozet hiçbir zaman kazanılmazdı.
        private bool DogrulaVeOlustur(BadgeAdminViewModel model, out string hata, out Badge? badge)
        {
            badge = null;

            var yeniBadge = new Badge
            {
                BadgeName = model.BadgeName,
                BadgeDescription = model.BadgeDescription ?? "",
                IconPath = model.IconPath ?? "",
                BadgeType = model.BadgeType,
                RequiredValue = model.RequiredValue,
                ModuleId = null,
                ExternalSignalKey = null
            };

            switch (model.BadgeType)
            {
                case BadgeType.System:
                    break;

                case BadgeType.Module:
                case BadgeType.Discovery:
                    if (model.ModuleId == null)
                    {
                        hata = "Bu rozet türü için bir modül seçilmelidir.";
                        return false;
                    }

                    yeniBadge.ModuleId = model.ModuleId;

                    if (model.BadgeType == BadgeType.Discovery)
                    {
                        yeniBadge.RequiredValue = 1;
                    }

                    break;

                case BadgeType.ExternalSignal:
                    if (string.IsNullOrWhiteSpace(model.ExternalSignalKey))
                    {
                        hata = "Bu rozet türü için bir dış sinyal seçilmelidir.";
                        return false;
                    }

                    // Admin'in elle bir signalKey yazması mümkün değil (View'de
                    // dropdown), ama form dışından POST edilebileceği için
                    // burada da host'un gerçekten desteklediği anahtarlardan
                    // biri mi diye kontrol ediliyor.
                    var destekleniyor = GetExternalSignals().Any(s => s.Key == model.ExternalSignalKey);
                    if (!destekleniyor)
                    {
                        hata = "Seçilen dış sinyal artık desteklenmiyor.";
                        return false;
                    }

                    yeniBadge.ExternalSignalKey = model.ExternalSignalKey;
                    yeniBadge.RequiredValue = 1;
                    break;

                default:
                    hata = "Geçersiz rozet türü.";
                    return false;
            }

            hata = "";
            badge = yeniBadge;
            return true;
        }
    }
}
