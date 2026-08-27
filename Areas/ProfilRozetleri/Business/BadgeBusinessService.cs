using Microsoft.Extensions.Options;
using ProfilRozetleriModulu.Data;
using ProfilRozetleriModulu.Models;
using ProfilRozetleriModulu.Utils;
using ProfilRozetleriModulu.ViewModels;

namespace ProfilRozetleriModulu.Business
{
    // Modülün tüm iş kuralları burada toplanır: ProcessNewWebLogEntries
    // (arka plan işinin giriş noktası) → LoginCheck (giriş serisi) →
    // ModuleEntryCheck (modül ziyaret serisi) → BadgeEarnCheck/EarnIfQualifies
    // (rozet kazanımı) → AwardXP (XP/seviye). Data katmanının üzerinde tek
    // if/foreach barındıran katman burasıdır; Controller'lar ve Data
    // servisleri kendi başlarına hiçbir karar vermez, hepsi burayı çağırır.
    //
    // Rozet/seviye olaylarında ayrıca bir bildirim gönderilmiyor — kullanıcı
    // yeni kazandığı rozeti navbar popup'ında ve Rozetlerim sayfasında
    // "Yeni" etiketiyle kendi görüyor (bkz. GetSonBadgesZiyaretTarihi,
    // BadgeViewModel.IsNew, ProfileBadgesViewModel.LastEarnedBadge).
    public class BadgeBusinessService : IBadgeBusinessService
    {
        private readonly IBadgeProcessStateDataService _badgeProcessStateDataService;
        private readonly IWebLogProvider _webLogProvider;
        private readonly IModuleDataService _moduleDataService;
        private readonly IUserProfileDataService _userProfileDataService;
        private readonly IUserLevelDataService _userLevelDataService;
        private readonly IBadgeDataService _badgeDataService;
        private readonly IUserBadgeDataService _userBadgeDataService;
        private readonly IUserBadgeProgressDataService _userBadgeProgressDataService;
        private readonly IExternalAchievementProvider _externalAchievementProvider;
        private readonly ProfilRozetleriOptions _options;

        public BadgeBusinessService(
            IBadgeProcessStateDataService badgeProcessStateDataService,
            IWebLogProvider webLogProvider,
            IModuleDataService moduleDataService,
            IUserProfileDataService userProfileDataService,
            IUserLevelDataService userLevelDataService,
            IBadgeDataService badgeDataService,
            IUserBadgeDataService userBadgeDataService,
            IUserBadgeProgressDataService userBadgeProgressDataService,
            IExternalAchievementProvider externalAchievementProvider,
            IOptions<ProfilRozetleriOptions> options)
        {
            _badgeProcessStateDataService = badgeProcessStateDataService;
            _webLogProvider = webLogProvider;
            _moduleDataService = moduleDataService;
            _userProfileDataService = userProfileDataService;
            _userLevelDataService = userLevelDataService;
            _badgeDataService = badgeDataService;
            _userBadgeDataService = userBadgeDataService;
            _userBadgeProgressDataService = userBadgeProgressDataService;
            _externalAchievementProvider = externalAchievementProvider;
            _options = options.Value;
        }

        // --- ProcessNewWebLogEntries -------------------------------------------------

        public void ProcessNewWebLogEntries()
        {
            var state = _badgeProcessStateDataService.Get();
            var yeniKayitlar = _webLogProvider.GetNewEntries(state.LastProcessedLogId);

            if (yeniKayitlar.Count == 0)
            {
                state.LastRunDate = DateTime.Now;
                _badgeProcessStateDataService.Update(state);
                return;
            }

            // Varsayılan: bu turdaki tüm yeni kayıtlar işlendi sayılır, imleç
            // en son kaydın Id'sine ilerler. Ama ModuleEntryCheck bir modül
            // ziyaretinin ne kadar sürdüğünü SONRAKİ kayıtla kıyaslayarak
            // anlıyor — kullanıcının en son ziyareti için henüz "sonraki
            // kayıt" yoksa (kullanıcı hâlâ o sayfada ya da iş tam o anda
            // çalıştı), o ziyaretin süresi bu turda bilinemez. Bu durumda
            // imleç o ziyaretin BAŞLANGICINDA durdurulur ki bir sonraki
            // çalıştırmada aynı kayıt yeniden değerlendirilebilsin.
            var guvenliCursor = yeniKayitlar.Max(x => x.Id);

            foreach (var grup in yeniKayitlar.Where(x => x.UserId.HasValue).GroupBy(x => x.UserId!.Value))
            {
                var userId = grup.Key;
                var kayitlar = grup.OrderBy(x => x.Id).ToList();

                LoginCheck(userId, kayitlar);

                var kullaniciSiniri = ModuleEntryCheck(userId, kayitlar);
                if (kullaniciSiniri.HasValue && kullaniciSiniri.Value < guvenliCursor)
                {
                    guvenliCursor = kullaniciSiniri.Value;
                }

                // WebLog'a bağlı değil (lookahead gerekmiyor); bu turda görülen
                // her kullanıcı için ayrıca kontrol edilir.
                ExternalSignalBadgeEarnCheck(userId);
            }

            state.LastProcessedLogId = guvenliCursor;
            state.LastRunDate = DateTime.Now;
            _badgeProcessStateDataService.Update(state);
        }

        // --- LoginCheck ---------------------------------------------------------------

        private void LoginCheck(int userId, List<WebLogEntry> kayitlar)
        {
            // Account/Login (giriş formunun kendisi) ve Account/LoginError
            // (başarısız giriş denemesi) bir "giriş" sayılmaz — bunlar henüz
            // oturum açılmadan önceki isteklerdir, kullanıcının sisteme
            // gerçekten girdiğini göstermez.
            var gecerliKayitlar = kayitlar
                .Where(x => !(x.Controller == "Account" && (x.Action == "Login" || x.Action == "LoginError")))
                .ToList();

            if (gecerliKayitlar.Count == 0) return;

            // Bu toplu işlemede birden fazla YENİ gün olabilir (iş uzun süre
            // çalışmadıysa); her günün ilk kaydı, o günün "giriş anı"dır.
            var gunlukIlkKayitlar = gecerliKayitlar
                .GroupBy(x => x.Tarih.Date)
                .Select(g => g.OrderBy(x => x.Id).First())
                .OrderBy(x => x.Tarih)
                .ToList();

            var profil = _userProfileDataService.GetByUserId(userId);
            var yeniProfil = false;
            if (profil == null)
            {
                profil = new UserProfile { UserId = userId, ConsecutiveLoginDays = 0 };
                yeniProfil = true;
            }

            foreach (var ilkKayit in gunlukIlkKayitlar)
            {
                var gun = ilkKayit.Tarih.Date;

                if (DateCalculationHelper.IsToday(profil.LastLoginDate, gun))
                {
                    continue;
                }

                if (DateCalculationHelper.IsYesterday(profil.LastLoginDate, gun))
                {
                    profil.ConsecutiveLoginDays += 1;
                }
                else
                {
                    // Seri kırıldıysa ya da hiç girilmemişse, bugün serinin
                    // 1. günüdür — 0 değil, çünkü kullanıcı bugün zaten giriş
                    // yapmış durumdadır.
                    profil.ConsecutiveLoginDays = 1;
                }

                profil.LastLoginDate = gun;
                AwardXP(userId, _options.GunlukGirisXP);
            }

            if (yeniProfil) _userProfileDataService.Add(profil);
            else _userProfileDataService.Update(profil);

            BadgeEarnCheckSystem(userId, profil.ConsecutiveLoginDays);
        }

        // --- ModuleEntryCheck -----------------------------------------------------------

        // Bir sayfa ziyareti tek bir WebLog kaydı üretmez: sayfa yüklenince
        // aynı anda birkaç arka plan isteği daha ateşlenebilir (host'un kendi
        // arka plan kontrolleri, tablo sayfalarının kendi veri çekme
        // istekleri gibi) — hepsi FARKLI controller'lara ait ama aynı sayfa
        // ziyaretinin parçasıdır. Bu yüzden "bir sonraki WebLog kaydı",
        // sayfadan gerçekten ayrılış anını değil, çoğu zaman 0.2-0.3 saniye
        // sonraki bu arka plan gürültüsünü işaret eder.
        //
        // Birbirine çok yakın (ProfilRozetleriOptions.BurstEsigiSaniye
        // içindeki) kayıtlar TEK bir "an" (aynı sayfa yüklemesinin parçası)
        // sayılır; kalma süresi iki AYRI an arasında ölçülür, ard arda gelen
        // tekil kayıtlar arasında değil.
        private int? ModuleEntryCheck(int userId, List<WebLogEntry> kayitlar)
        {
            var anlar = new List<List<WebLogEntry>>();
            foreach (var kayit in kayitlar)
            {
                var sonAn = anlar.Count > 0 ? anlar[^1] : null;
                if (sonAn != null && Math.Abs(DateCalculationHelper.SaniyeFarki(sonAn[^1].Tarih, kayit.Tarih)) < _options.BurstEsigiSaniye)
                {
                    sonAn.Add(kayit);
                }
                else
                {
                    anlar.Add(new List<WebLogEntry> { kayit });
                }
            }

            int? sinir = null;

            for (int i = 0; i < anlar.Count; i++)
            {
                var an = anlar[i];
                // Anın hangi sayfayı temsil ettiği, o anın İLK kaydının
                // controller'ı — genellikle asıl sayfa isteği (Index), arkadan
                // gelen AJAX çağrıları değil.
                var temsilKayit = an[0];
                var modul = _moduleDataService.GetByControllerName(temsilKayit.Controller);
                if (modul == null) continue; // Modules'de eşleşme yok, atlanır.

                if (i + 1 >= anlar.Count)
                {
                    // Bu anın ardından henüz başka bir an yok, dolayısıyla bu
                    // ziyaretin ne kadar sürdüğü henüz ölçülemez (bkz.
                    // ProcessNewWebLogEntries'teki guvenliCursor açıklaması).
                    // İmleç bu anın başlangıcını geçmeyecek şekilde ayarlanır
                    // ki bir sonraki çalıştırmada bu kayıt tekrar değerlendirilsin.
                    sinir = temsilKayit.Id - 1;
                    continue;
                }

                var sonrakiAn = anlar[i + 1];
                var saniyeFarki = DateCalculationHelper.SaniyeFarki(an[^1].Tarih, sonrakiAn[0].Tarih);
                if (saniyeFarki < _options.ModulKalmaEsigiSaniye) continue; // yeterince kalınmadı

                ModuleBadgeEarnCheck(userId, modul.ModuleId, temsilKayit.Tarih.Date);
            }

            return sinir;
        }

        private void ModuleBadgeEarnCheck(int userId, int moduleId, DateTime gun)
        {
            // Module (ard arda giriş serisi) ve Discovery (tek seferlik keşif)
            // rozetleri aynı moduleId'ye bağlanabilir, ikisi de burada işlenir.
            foreach (var rozet in _badgeDataService.GetByModuleId(moduleId))
            {
                var userBadge = GetOrCreateUserBadge(userId, rozet.BadgeId);

                if (rozet.BadgeType == BadgeType.Discovery)
                {
                    // Streak yok: modül en az bir kez (15+ sn kalınarak)
                    // kullanıldıysa hemen kazanılır.
                    EarnIfQualifies(userBadge, rozet, mevcutDeger: 1);
                    continue;
                }

                var ilerleme = _userBadgeProgressDataService.GetByUserBadgeId(userBadge.UserBadgeId);
                var yeniIlerleme = false;
                if (ilerleme == null)
                {
                    ilerleme = new UserBadgeProgress { UserBadgeId = userBadge.UserBadgeId, RepeatCount = 0 };
                    yeniIlerleme = true;
                }

                if (DateCalculationHelper.IsToday(ilerleme.LastSeenDateThisModule, gun))
                {
                    // Aynı gün içinde modüle tekrar tekrar girmek sayacı
                    // artırmaz — RepeatCount "kaç GÜN ard arda girildi" demek,
                    // "kaç kez tıklandı" değil.
                }
                else if (DateCalculationHelper.IsYesterday(ilerleme.LastSeenDateThisModule, gun))
                {
                    ilerleme.RepeatCount += 1;
                }
                else
                {
                    // LoginCheck'teki aynı düzeltme: seri kırıldıysa/hiç
                    // yoksa bugün serinin 1. günüdür, 0 değil.
                    ilerleme.RepeatCount = 1;
                }

                ilerleme.LastSeenDateThisModule = gun;

                if (yeniIlerleme) _userBadgeProgressDataService.Add(ilerleme);
                else _userBadgeProgressDataService.Update(ilerleme);

                EarnIfQualifies(userBadge, rozet, ilerleme.RepeatCount);
            }
        }

        // --- BadgeEarnCheck -------------------------------------------------------------

        private void BadgeEarnCheckSystem(int userId, int consecutiveLoginDays)
        {
            foreach (var rozet in _badgeDataService.GetByType(BadgeType.System))
            {
                var userBadge = GetOrCreateUserBadge(userId, rozet.BadgeId);
                EarnIfQualifies(userBadge, rozet, consecutiveLoginDays);
            }
        }

        // WebLog/modül ziyaretiyle ilgisi yok; IExternalAchievementProvider
        // seam'i üzerinden host'a soruluyor (örn. "Wizard turu tamamlandı mı").
        private void ExternalSignalBadgeEarnCheck(int userId)
        {
            foreach (var rozet in _badgeDataService.GetByType(BadgeType.ExternalSignal))
            {
                var userBadge = GetOrCreateUserBadge(userId, rozet.BadgeId);
                if (userBadge.IsEarned) continue; // zaten kazanılmış, tekrar sormaya gerek yok

                var saglandi = _externalAchievementProvider.IsAchieved(userId, rozet.ExternalSignalKey!);
                EarnIfQualifies(userBadge, rozet, mevcutDeger: saglandi ? 1 : 0);
            }
        }

        private UserBadge GetOrCreateUserBadge(int userId, int badgeId)
        {
            var userBadge = _userBadgeDataService.GetByUserAndBadge(userId, badgeId);
            if (userBadge == null)
            {
                userBadge = new UserBadge { UserId = userId, BadgeId = badgeId, IsEarned = false };
                // EF Core, Add + SaveChanges sonrası bu nesnenin UserBadgeId'sini
                // (IDENTITY) doldurur; çağıran taraf tekrar sorgu atmaz.
                _userBadgeDataService.Add(userBadge);
            }
            return userBadge;
        }

        private void EarnIfQualifies(UserBadge userBadge, Badge rozet, int mevcutDeger)
        {
            if (userBadge.IsEarned) return; // zaten kazanılmış, tekrar bildirim gönderilmez

            if (mevcutDeger < rozet.RequiredValue) return;

            userBadge.IsEarned = true;
            // DateTime.Now (Today değil): "Yeni" etiketi (GetSonBadgesZiyaretTarihi)
            // rozetin son ziyaretten SONRA mı kazanıldığını saat bazında
            // karşılaştırıyor — gün bazlı olsaydı "bugün kazanıp bugün bakma"
            // senaryosunda etiket yanlışlıkla hiç görünmezdi. Kolon buna göre
            // DATETIME'a yükseltildi (10-earneddate-datetime.sql).
            userBadge.EarnedDate = DateTime.Now;
            _userBadgeDataService.Update(userBadge);

            AwardXP(userBadge.UserId, _options.RozetKazanimXP);
        }

        // --- LevelUpdate ----------------------------------------------------------------

        private void AwardXP(int userId, int miktar)
        {
            var seviye = _userLevelDataService.GetByUserId(userId);
            var yeni = false;
            if (seviye == null)
            {
                seviye = new UserLevel { UserId = userId, Level = 1, XP = 0 };
                yeni = true;
            }

            ApplyXPDecayIfNeeded(seviye);

            seviye.XP = Math.Max(0, seviye.XP + miktar);
            seviye.Level = (seviye.XP / _options.SeviyeBasinaXP) + 1;
            seviye.LastXPUpdateDate = DateTime.Today;

            if (yeni) _userLevelDataService.Add(seviye);
            else _userLevelDataService.Update(seviye);
        }

        // Modülde geçirilen süre XP'yi etkilemiyor — yalnızca günlük giriş
        // (LoginCheck) ve rozet kazanımı (EarnIfQualifies) AwardXP çağırır.
        // XP düşüşü "tembel" hesaplanır: burada arka planda periyodik
        // çalışan bir zamanlayıcı yok, düşüş yalnızca kullanıcı bir sonraki
        // kez XP kazandığında (AwardXP çağrıldığında) geriye dönük hesaplanır.
        private void ApplyXPDecayIfNeeded(UserLevel seviye)
        {
            if (!seviye.LastXPUpdateDate.HasValue) return;

            var gecenGun = (DateTime.Today - seviye.LastXPUpdateDate.Value.Date).Days;
            if (gecenGun <= _options.XPDususBaslamaGunu) return;

            var dususGunSayisi = gecenGun - _options.XPDususBaslamaGunu;
            seviye.XP = Math.Max(0, seviye.XP - (dususGunSayisi * _options.GunlukXPDususu));
        }

        // --- YeniRozetKontrolu ----------------------------------------------------------

        // Rozetlerim sayfasına en son GERÇEKTEN ne zaman girildiğini WebLog'dan
        // hesaplar — ayrı bir "son görüldü" alanı tutulmuyor, WebLog burada da
        // (login serisi/modül ziyareti gibi) tek doğruluk kaynağı.
        //
        // GetData, MyBadges sayfasının kendisi tarafından çağrılıyor; o sayfa
        // yüklemesinin WebLog kaydı GetData çalışana kadar ZATEN yazılmış
        // olur (host'un istek günlükleme mekanizması action bitince, sayfa
        // render'ından ÖNCE yazıyor). Yani en taze kayıt her zaman "şu an
        // içinde bulunulan ziyaret"in kendisidir, geçmiş bir ziyaret değil —
        // bunu cutoff olarak kullanmak "bugün kazandığın hiçbir rozet asla
        // yeni görünmez" hatasına yol açar (ProfilRozetleriOptions.
        // BurstEsigiSaniye içindeki tüm kayıtlar zaten aynı "an"ın parçası —
        // ModuleEntryCheck'te kullanılan aynı mantık). Bu yüzden en taze
        // kayıt "şimdi"ye (BurstEsigiSaniye içinde) çok yakınsa atlanır,
        // ondan önceki kayıt gerçek "son ziyaret" sayılır. (GetData ileride
        // MyBadges dışında bir yerden de çağrılırsa — örn. başka bir sayfadan
        // — bu mantık orada da doğru çalışır: en taze kayıt zaten eski
        // olacağından atlama devreye girmez.)
        private DateTime? GetSonBadgesZiyaretTarihi(int userId)
        {
            var kayitlar = _webLogProvider.GetRecentEntries(userId, "Badges", "MyBadges", 5)
                .OrderByDescending(x => x.Tarih)
                .ToList();

            if (kayitlar.Count == 0) return null;

            var enTaze = kayitlar[0];
            if (Math.Abs(DateCalculationHelper.SaniyeFarki(enTaze.Tarih, DateTime.Now)) < _options.BurstEsigiSaniye)
            {
                return kayitlar.Count > 1 ? kayitlar[1].Tarih : (DateTime?)null;
            }

            return enTaze.Tarih;
        }

        // --- Okuma: BadgesController için ------------------------------------------------

        public ProfileBadgesViewModel GetProfileBadges(int userId)
        {
            var seviye = _userLevelDataService.GetByUserId(userId);
            var profil = _userProfileDataService.GetByUserId(userId);

            var xp = seviye?.XP ?? 0;

            var seviyeDegeri = seviye?.Level ?? 1;

            var vm = new ProfileBadgesViewModel
            {
                Level = seviyeDegeri,
                XP = xp,
                XPPercentToNextLevel = (xp % _options.SeviyeBasinaXP) * 100 / _options.SeviyeBasinaXP,
                XPGerekenSeviyeEsigi = seviyeDegeri * _options.SeviyeBasinaXP,
                ConsecutiveLoginDays = profil?.ConsecutiveLoginDays ?? 0
            };

            var kullaniciRozetleri = _userBadgeDataService.GetByUserId(userId)
                .ToDictionary(ub => ub.BadgeId);

            var sonZiyaret = GetSonBadgesZiyaretTarihi(userId);

            foreach (var rozet in _badgeDataService.GetAll())
            {
                kullaniciRozetleri.TryGetValue(rozet.BadgeId, out var userBadge);

                var moduleAdi = rozet.ModuleId.HasValue
                    ? _moduleDataService.GetById(rozet.ModuleId.Value)?.ModuleName
                    : null;

                var mevcutDeger = 0;
                if (userBadge != null)
                {
                    mevcutDeger = rozet.BadgeType switch
                    {
                        BadgeType.System => profil?.ConsecutiveLoginDays ?? 0,
                        // Discovery/ExternalSignal'da streak/ilerleme kaydı yok,
                        // kazanılmışsa 1.
                        BadgeType.Discovery => userBadge.IsEarned ? 1 : 0,
                        BadgeType.ExternalSignal => userBadge.IsEarned ? 1 : 0,
                        _ => _userBadgeProgressDataService.GetByUserBadgeId(userBadge.UserBadgeId)?.RepeatCount ?? 0
                    };
                }

                var badgeVm = new BadgeViewModel
                {
                    BadgeId = rozet.BadgeId,
                    BadgeName = rozet.BadgeName,
                    BadgeDescription = rozet.BadgeDescription,
                    IconPath = rozet.IconPath,
                    IsEarned = userBadge?.IsEarned ?? false,
                    EarnedDate = userBadge?.EarnedDate,
                    ModuleName = moduleAdi,
                    ProgressText = BadgeDisplayUtils.IlerlemeMetni(mevcutDeger, rozet.RequiredValue),
                    IsNew = userBadge?.IsEarned == true && userBadge.EarnedDate.HasValue &&
                            (sonZiyaret == null || userBadge.EarnedDate.Value > sonZiyaret.Value)
                };

                (badgeVm.IsEarned ? vm.EarnedBadges : vm.UnearnedBadges).Add(badgeVm);
            }

            vm.EarnedBadges = vm.EarnedBadges.OrderByDescending(x => x.EarnedDate).ToList();
            vm.LastEarnedBadge = vm.EarnedBadges.FirstOrDefault();

            return vm;
        }
    }
}
