/* =====================================================================
   Badges tablosundaki CHECK constraint'lerini BadgeType=2 (Discovery)
   degerini de kapsayacak sekilde genisletir. Discovery, bir modulun
   streak gerektirmeden yalnizca bir kez kullanilmasiyla kazanilan rozet
   turudur. Idempotent degil ama guvenli: constraint zaten yoksa DROP hata
   verir, bu yuzden once varligi kontrol edilir.
   ===================================================================== */

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

ALTER TABLE dbo.Badges ADD CONSTRAINT CK_Badges_BadgeType CHECK (BadgeType IN (0, 1, 2));
ALTER TABLE dbo.Badges ADD CONSTRAINT CK_Badges_ModuleId_TypeUyumu CHECK (
    (BadgeType = 0 AND ModuleId IS NULL) OR
    (BadgeType IN (1, 2) AND ModuleId IS NOT NULL)
);
GO
