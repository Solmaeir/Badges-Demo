/* =====================================================================
   Badges.BadgeType sutununu TINYINT'ten INT'e cevirir.

   C# tarafindaki BadgeType enum'inin varsayilan alt tipi int'tir; sutun
   tinyint oldugunda EF Core'un SqlDataReader uzerinden GetInt32 cagrisi
   "Unable to cast object of type 'System.Byte' to type 'System.Int32'"
   hatasi firlatir. Idempotent: sutun zaten INT ise hicbir sey yapmaz.
   ===================================================================== */

IF EXISTS (
    SELECT 1 FROM sys.columns c
    JOIN sys.types t ON c.user_type_id = t.user_type_id
    WHERE c.object_id = OBJECT_ID('dbo.Badges') AND c.name = 'BadgeType' AND t.name = 'tinyint'
)
BEGIN
    ALTER TABLE dbo.Badges DROP CONSTRAINT CK_Badges_BadgeType;
    ALTER TABLE dbo.Badges DROP CONSTRAINT CK_Badges_ModuleId_TypeUyumu;

    ALTER TABLE dbo.Badges ALTER COLUMN BadgeType INT NOT NULL;

    ALTER TABLE dbo.Badges ADD CONSTRAINT CK_Badges_BadgeType CHECK (BadgeType IN (0, 1));
    ALTER TABLE dbo.Badges ADD CONSTRAINT CK_Badges_ModuleId_TypeUyumu CHECK (
        (BadgeType = 0 AND ModuleId IS NULL) OR
        (BadgeType = 1 AND ModuleId IS NOT NULL)
    );
END
GO
