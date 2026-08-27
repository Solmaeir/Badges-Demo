/* =====================================================================
   Ornek ExternalSignal rozeti: Wizard turunu tamamlama. Idempotent.
   ===================================================================== */

INSERT INTO dbo.Badges (BadgeName, BadgeDescription, IconPath, BadgeType, RequiredValue, ModuleId, ExternalSignalKey)
SELECT N'Wizard Ustası', N'Tanıtım turunu baştan sona tamamladın.', N'/Content/Badges/25-gold-jade-r5.png', 3, 1, NULL, N'wizard-tour-completed'
WHERE NOT EXISTS (SELECT 1 FROM dbo.Badges WHERE BadgeName = N'Wizard Ustası');
GO
