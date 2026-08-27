/* UserBadge.EarnedDate sutununu DATE'ten DATETIME'a cevirir.

   "Yeni rozet" etiketi, bir rozetin kullanicinin Rozetlerim sayfasina son
   ziyaretinden SONRA kazanilip kazanilmadigini saat bazinda karsilastirir
   (bkz. BadgeBusinessService.GetSonBadgesZiyaretTarihi). EarnedDate yalnizca
   tarih tutarsa, ayni gun icinde kazanilan bir rozetin saat sirasi
   belirlenemez. Donusum sirasinda var olan satirlar 00:00:00 saatini alir;
   BadgeBusinessService.EarnIfQualifies, EarnedDate'i DateTime.Now ile yazar. */
ALTER TABLE dbo.UserBadge ALTER COLUMN EarnedDate DATETIME NULL;
GO
