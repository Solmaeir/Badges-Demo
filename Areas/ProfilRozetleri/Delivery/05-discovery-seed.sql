/* =====================================================================
   Ornek Discovery (tek seferlik kesif) rozeti. Idempotent.
   ===================================================================== */

INSERT INTO dbo.Badges (BadgeName, BadgeDescription, IconPath, BadgeType, RequiredValue, ModuleId)
SELECT N'Kaşif', N'Ürünler sayfasını kullandın.', N'/Content/Badges/06-brass-emerald-r1.png', 2, 1, m.ModuleId
FROM dbo.Modules m
WHERE m.ControllerName = N'Urunler'
AND NOT EXISTS (SELECT 1 FROM dbo.Badges b WHERE b.BadgeName = N'Kaşif');
GO
