/* =====================================================================
   ProfilRozetleri modulunun Wizard adimlari (host'ta Wizard modulu varsa).
   WizardSteps tablosunda her modul kendi SortPath grubunu kullanir, gruplar
   birbirine cakismamalidir. Bu script 10.x araligini kullaniyor - calistirmadan
   once WizardSteps tablosunda bu araligin bos oldugu kontrol edilmeli
   (guncel veri: SELECT SortPath FROM dbo.WizardSteps) ve yeni bir modul
   eklenirken en yuksek grup numarasindan devam edilmeli.

   Ilk adimin TargetUrl'i NULL: kenar menu her sayfada gorunur, ayrica
   navigasyona gerek yok. Diger iki adim Profil sayfasina ozel elemanlari
   hedefliyor, bu yuzden TargetUrl doldurulmus.
   ===================================================================== */

INSERT INTO dbo.WizardSteps (ModuleName, Title, Description, TargetSelector, TargetUrl, SortPath, RequiredPermission)
VALUES
    (N'ProfilRozetleri', N'Profilim ve Rozetlerim', N'Profil sekmesinden başarı rozetlerinizi ve seviyenizi görebilirsiniz.', '[data-wizard-id="profile-menu"]', NULL, '10.1', NULL),
    (N'ProfilRozetleri', N'Seviye ve XP', N'Burada güncel seviyeniz ve deneyim puanınız (XP) gösterilir. Düzenli giriş yaptıkça XP kazanırsınız; uzun süre giriş yapmazsanız XP''niz kademeli olarak düşer ve seviyeniz gerileyebilir.', '[data-wizard-id="profile-level-card"]', '/ProfilRozetleri/Badges/MyBadges', '10.2', NULL),
    (N'ProfilRozetleri', N'Rozet Galerisi', N'Kazandığınız rozetler üstte renkli, henüz kazanmadıklarınız altta gri gösterilir. Üzerlerine gelince hangi koşulu sağlamanız gerektiğini görebilirsiniz.', '[data-wizard-id="profile-badges-gallery"]', '/ProfilRozetleri/Badges/MyBadges', '10.3', NULL);
GO
