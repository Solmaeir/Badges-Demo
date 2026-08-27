/* =====================================================================
   FK_UserBadge_Badges ve FK_UserBadgeProgress_UserBadge yabanci anahtarlarina
   ON DELETE CASCADE ekler. Bir rozet silindiginde, o rozete ait kullanici
   kazanim kayitlari (UserBadge) ve ilerleme kayitlari (UserBadgeProgress) da
   otomatik silinir; boylece kazanilmis bir rozet de silinebilir, aksi halde
   yabanci anahtar ihlaliyle hata olusur. Idempotent.
   ===================================================================== */

IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_UserBadge_Badges' AND delete_referential_action = 0)
BEGIN
    ALTER TABLE dbo.UserBadge DROP CONSTRAINT FK_UserBadge_Badges;
    ALTER TABLE dbo.UserBadge ADD CONSTRAINT FK_UserBadge_Badges FOREIGN KEY (BadgeId)
        REFERENCES dbo.Badges (BadgeId) ON DELETE CASCADE;
END
GO

IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_UserBadgeProgress_UserBadge' AND delete_referential_action = 0)
BEGIN
    ALTER TABLE dbo.UserBadgeProgress DROP CONSTRAINT FK_UserBadgeProgress_UserBadge;
    ALTER TABLE dbo.UserBadgeProgress ADD CONSTRAINT FK_UserBadgeProgress_UserBadge FOREIGN KEY (UserBadgeId)
        REFERENCES dbo.UserBadge (UserBadgeId) ON DELETE CASCADE;
END
GO
