/* =====================================================================
   Badges tablosuna ExternalSignalKey sutununu ve BadgeType=3
   (ExternalSignal) destegini ekler. Bu tur, WebLog/modul ziyaretiyle
   ilgisi olmayan, host'un IExternalAchievementProvider seam'i uzerinden
   cevapladigi bir kosula bagli rozetler icindir (ornek: "Wizard turunu
   tamamladin"). Idempotent.
   ===================================================================== */

IF NOT EXISTS (
    SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Badges') AND name = 'ExternalSignalKey'
)
BEGIN
    ALTER TABLE dbo.Badges ADD ExternalSignalKey NVARCHAR(100) NULL;
END
GO

IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Badges_BadgeType')
BEGIN
    ALTER TABLE dbo.Badges DROP CONSTRAINT CK_Badges_BadgeType;
END
GO

IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Badges_ModuleId_TypeUyumu')
BEGIN
    ALTER TABLE dbo.Badges DROP CONSTRAINT CK_Badges_ModuleId_TypeUyumu;
END
GO

ALTER TABLE dbo.Badges ADD CONSTRAINT CK_Badges_BadgeType CHECK (BadgeType IN (0, 1, 2, 3));
ALTER TABLE dbo.Badges ADD CONSTRAINT CK_Badges_ModuleId_TypeUyumu CHECK (
    (BadgeType = 0 AND ModuleId IS NULL AND ExternalSignalKey IS NULL) OR
    (BadgeType IN (1, 2) AND ModuleId IS NOT NULL AND ExternalSignalKey IS NULL) OR
    (BadgeType = 3 AND ModuleId IS NULL AND ExternalSignalKey IS NOT NULL)
);
GO
