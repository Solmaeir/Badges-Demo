/* =====================================================================
   Profil Rozetleri modulu - veritabani semasi (SQL Server)
   Temiz kurulum, migration'a bagli degil. Tablo/alan adlari Ingilizce
   (Badges, UserBadge, ...) - projenin geri kalani (Urunler, Kategoriler)
   Turkce isimlendiriyor ama bu modulde bilerek bu isimlendirme korundu.

   Users tablosuna FK veren sutunlar bu test ortaminda dbo.Users(Id)'ye
   baglanir. Gercek EDI'de PK adi UserId'dir - script o sisteme tasinirken
   FK hedefi buna gore uyarlanmali.

   WebLog burada YOK: o host'un (gercek EDI'de zaten var olan, bu test
   ortaminda ayrica kurulan) tablosu, modulun degil - modul ona yalnizca
   IWebLogProvider seam'i uzerinden erisir. Bkz. WebLogDelivery/01-weblog-
   tablosu.sql ve README-ProfilRozetleri.md.
   ===================================================================== */

/* Sistemde ayri bir modul tablosu olmadigi icin bu modul tarafindan
   kuruluyor. ControllerName, WebLog.Controller alaniyla birebir eslesecek. */
IF OBJECT_ID('dbo.Modules', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Modules
    (
        ModuleId       INT           IDENTITY(1,1) NOT NULL,
        ModuleName     NVARCHAR(100) NOT NULL,
        ControllerName NVARCHAR(100) NOT NULL,

        CONSTRAINT PK_Modules PRIMARY KEY CLUSTERED (ModuleId)
    );

    -- Ayni controller iki module birden eslenemez.
    CREATE UNIQUE INDEX UX_Modules_ControllerName
        ON dbo.Modules (ControllerName);
END
GO

/* Kullanicinin genel girisi bilgilerini tutar. UserId hem PK hem Users'a
   FK (1:1 iliski) - C# tarafinda navigation property yok, sade int. */
IF OBJECT_ID('dbo.UserProfile', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.UserProfile
    (
        UserId                INT  NOT NULL,
        LastLoginDate         DATE NULL,
        ConsecutiveLoginDays  INT  NOT NULL CONSTRAINT DF_UserProfile_ConsecutiveLoginDays DEFAULT (0),

        CONSTRAINT PK_UserProfile PRIMARY KEY CLUSTERED (UserId),
        CONSTRAINT FK_UserProfile_Users FOREIGN KEY (UserId)
            REFERENCES dbo.Users (Id)
    );
END
GO

/* Seviye/XP, UserProfile'dan ayri bir tabloda tutulur: giris serisi
   (UserProfile) ve seviye/XP (bu tablo) birbirinden bagimsiz guncellenir. */
IF OBJECT_ID('dbo.UserLevel', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.UserLevel
    (
        UserId           INT  NOT NULL,
        Level            INT  NOT NULL CONSTRAINT DF_UserLevel_Level DEFAULT (1),
        XP               INT  NOT NULL CONSTRAINT DF_UserLevel_XP DEFAULT (0),
        LastXPUpdateDate DATE NULL,

        CONSTRAINT PK_UserLevel PRIMARY KEY CLUSTERED (UserId),
        CONSTRAINT FK_UserLevel_Users FOREIGN KEY (UserId)
            REFERENCES dbo.Users (Id)
    );
END
GO

/* Rozet tanimlari. BadgeType: 0=System, 1=Module (C# tarafinda enum,
   BadgeType.cs). CHECK, ModuleId'nin yalnizca Module tipinde dolu
   olmasini DB seviyesinde garanti eder. */
IF OBJECT_ID('dbo.Badges', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Badges
    (
        BadgeId          INT           IDENTITY(1,1) NOT NULL,
        BadgeName        NVARCHAR(100) NOT NULL,
        BadgeDescription NVARCHAR(500) NULL,
        IconPath         NVARCHAR(255) NULL,
        -- INT (TINYINT değil): C# tarafında BadgeType enum'ının varsayılan alt
        -- tipi int'tir; sütun tinyint olursa EF Core'un okuma sırasında
        -- reader.GetInt32() çağrısı "Unable to cast Byte to Int32" ile patlar.
        BadgeType        INT           NOT NULL,
        RequiredValue    INT           NOT NULL,
        ModuleId         INT           NULL,
        -- Yalnızca BadgeType=3 (ExternalSignal) için dolu; host'un
        -- IExternalAchievementProvider'ına iletilen serbest metin anahtar.
        ExternalSignalKey NVARCHAR(100) NULL,

        CONSTRAINT PK_Badges PRIMARY KEY CLUSTERED (BadgeId),
        CONSTRAINT FK_Badges_Modules FOREIGN KEY (ModuleId)
            REFERENCES dbo.Modules (ModuleId),
        -- 0=System, 1=Module (ard arda giriş serisi), 2=Discovery (o modülü
        -- bir kez keşfetme), 3=ExternalSignal (WebLog dışı dış koşul).
        CONSTRAINT CK_Badges_BadgeType CHECK (BadgeType IN (0, 1, 2, 3)),
        CONSTRAINT CK_Badges_ModuleId_TypeUyumu CHECK (
            (BadgeType = 0 AND ModuleId IS NULL AND ExternalSignalKey IS NULL) OR
            (BadgeType IN (1, 2) AND ModuleId IS NOT NULL AND ExternalSignalKey IS NULL) OR
            (BadgeType = 3 AND ModuleId IS NULL AND ExternalSignalKey IS NOT NULL)
        )
    );
END
GO

/* Her kullanici-rozet ikilisinin kazanim durumu. (UserId, BadgeId) unique -
   bir kullanici bir rozete yalnizca bir kez sahip olabilir. */
IF OBJECT_ID('dbo.UserBadge', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.UserBadge
    (
        UserBadgeId INT  IDENTITY(1,1) NOT NULL,
        UserId      INT  NOT NULL,
        BadgeId     INT  NOT NULL,
        IsEarned    BIT  NOT NULL CONSTRAINT DF_UserBadge_IsEarned DEFAULT (0),
        EarnedDate  DATETIME NULL,

        CONSTRAINT PK_UserBadge PRIMARY KEY CLUSTERED (UserBadgeId),
        CONSTRAINT FK_UserBadge_Users FOREIGN KEY (UserId)
            REFERENCES dbo.Users (Id),
        -- ON DELETE CASCADE: Rozet Yonetimi ekranindan kazanilmis bir rozet
        -- de silinebilmeli (WizardStepViews'daki "gorulmus adim da silinebilir"
        -- kuraliyla ayni gerekce) - aksi halde FK ihlaliyle 500 hatasi verir.
        CONSTRAINT FK_UserBadge_Badges FOREIGN KEY (BadgeId)
            REFERENCES dbo.Badges (BadgeId) ON DELETE CASCADE
    );

    CREATE UNIQUE INDEX UX_UserBadge_User_Badge
        ON dbo.UserBadge (UserId, BadgeId);
END
GO

/* Yalnizca BadgeType.Module olan rozetler icin doldurulur. Hangi UserBadge
   kaydina ait oldugu UserBadgeId'den bilinir - BadgeId/UserId burada
   ayrica tutulmaz, tekillesmis kaynak UserBadge'dir. */
IF OBJECT_ID('dbo.UserBadgeProgress', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.UserBadgeProgress
    (
        UserBadgeProgressId    INT  IDENTITY(1,1) NOT NULL,
        UserBadgeId            INT  NOT NULL,
        LastSeenDateThisModule DATE NULL,
        RepeatCount            INT  NOT NULL CONSTRAINT DF_UserBadgeProgress_RepeatCount DEFAULT (0),

        CONSTRAINT PK_UserBadgeProgress PRIMARY KEY CLUSTERED (UserBadgeProgressId),
        CONSTRAINT FK_UserBadgeProgress_UserBadge FOREIGN KEY (UserBadgeId)
            REFERENCES dbo.UserBadge (UserBadgeId) ON DELETE CASCADE
    );

    -- Bir UserBadge kaydinin ilerlemesi tekildir.
    CREATE UNIQUE INDEX UX_UserBadgeProgress_UserBadgeId
        ON dbo.UserBadgeProgress (UserBadgeId);
END
GO

/* Arka plan isinin WebLog'da nereye kadar isleme yaptigini tutan tek
   satirlik imlec (cursor). Id=1 disinda satir eklenemez (CHECK). */
IF OBJECT_ID('dbo.BadgeProcessState', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.BadgeProcessState
    (
        Id                 INT          NOT NULL,
        LastProcessedLogId INT          NOT NULL CONSTRAINT DF_BadgeProcessState_LastProcessedLogId DEFAULT (0),
        LastRunDate        DATETIME2(7) NULL,

        CONSTRAINT PK_BadgeProcessState PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT CK_BadgeProcessState_TekSatir CHECK (Id = 1)
    );

    INSERT INTO dbo.BadgeProcessState (Id, LastProcessedLogId, LastRunDate)
    VALUES (1, 0, NULL);
END
GO
