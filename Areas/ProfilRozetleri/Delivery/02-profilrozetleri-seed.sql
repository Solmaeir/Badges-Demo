/* =====================================================================
   Profil Rozetleri - ornek Modul ve Rozet verisi
   Idempotent: script tekrar calistirilirsa var olan kayitlari tekrar eklemez.
   ControllerName degerleri bu test ortamindaki gercek controller adlariyla
   (WebLog.Controller alaninin dolacagi degerlerle) birebir eslesir. Baska
   bir sisteme tasinirken bu tablo o sistemin kendi controller adlariyla
   yeniden doldurulmali - rozet isimleri/esikleri ornek amaclidir.
   ===================================================================== */

INSERT INTO dbo.Modules (ModuleName, ControllerName)
SELECT v.ModuleName, v.ControllerName
FROM (VALUES
    (N'Ürünler', N'Urunler'),
    (N'Kategoriler', N'Kategoriler'),
    (N'İşletmeler', N'Isletmeler'),
    (N'Hammaddeler', N'Hammaddeler'),
    (N'Üretim Kayıtları', N'UretimKayitlari'),
    (N'Ürün Reçeteleri', N'UrunHammaddeIhtiyaci'),
    (N'Stok Tükenme Tahmini', N'StokTahmin')
) AS v(ModuleName, ControllerName)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.Modules m WHERE m.ControllerName = v.ControllerName
);
GO

-- Sistem geneli (giris serisi) rozetleri.
INSERT INTO dbo.Badges (BadgeName, BadgeDescription, IconPath, BadgeType, RequiredValue, ModuleId)
SELECT v.BadgeName, v.BadgeDescription, v.IconPath, 0, v.RequiredValue, NULL
FROM (VALUES
    (N'Sadık Ziyaretçi', N'3 gün üst üste giriş yaptın.', N'/Content/Badges/01-steel-navy-r1.png', 3),
    (N'Haftalık Şampiyon', N'7 gün üst üste giriş yaptın.', N'/Content/Badges/03-steel-navy-r3.png', 7),
    (N'Ayın Yıldızı', N'30 gün üst üste giriş yaptın.', N'/Content/Badges/05-steel-navy-r5.png', 30)
) AS v(BadgeName, BadgeDescription, IconPath, RequiredValue)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.Badges b WHERE b.BadgeName = v.BadgeName
);
GO

-- Modül bazlı (o modüle ard arda giriş) rozetleri.
INSERT INTO dbo.Badges (BadgeName, BadgeDescription, IconPath, BadgeType, RequiredValue, ModuleId)
SELECT N'Ürün Kurdu', N'Ürünler sayfasına 3 gün üst üste girdin.', N'/Content/Badges/08-brass-emerald-r3.png', 1, 3, m.ModuleId
FROM dbo.Modules m
WHERE m.ControllerName = N'Urunler'
AND NOT EXISTS (SELECT 1 FROM dbo.Badges b WHERE b.BadgeName = N'Ürün Kurdu');
GO

INSERT INTO dbo.Badges (BadgeName, BadgeDescription, IconPath, BadgeType, RequiredValue, ModuleId)
SELECT N'Stok Kâhini', N'Stok Tükenme Tahmini sayfasına 3 gün üst üste girdin.', N'/Content/Badges/13-copper-amethyst-r3.png', 1, 3, m.ModuleId
FROM dbo.Modules m
WHERE m.ControllerName = N'StokTahmin'
AND NOT EXISTS (SELECT 1 FROM dbo.Badges b WHERE b.BadgeName = N'Stok Kâhini');
GO
