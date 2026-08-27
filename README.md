# Rozet Sistemi

Kullanıcıların bir yazılım sistemine ve o sistemdeki tanımlı modüllere düzenli
kullanımını rozet ve seviye/deneyim puanı (XP) mekanizmasıyla ödüllendiren,
bağımsız bir modül.

## İçindekiler

- [Klasör Yapısı](#klasör-yapısı)
- [Veritabanı Şeması](#veritabanı-şeması)
- [Sistemin İşleyişi](#sistemin-i̇şleyişi)
- [Entegrasyon Noktaları](#entegrasyon-noktaları)
- [Kurulum](#kurulum)
- [Yönetim Ekranları](#yönetim-ekranları)
- [Dış Sinyal Rozeti Eklemek](#dış-sinyal-rozeti-eklemek)
- [Sık Karşılaşılan Durumlar](#sık-karşılaşılan-durumlar)
- [Sorgu Kodları](#sorgu-kodları)

## Klasör Yapısı

```
Areas/ProfilRozetleri/
├── Models/          Veritabanı tablolarının karşılığı olan sınıflar.
├── ViewModels/       Ekranlara taşınan veri şekilleri, doğrulama kuralları.
├── Data/             Veritabanı erişiminin yapıldığı tek katman.
├── Business/         İş kuralları, arka plan işi, entegrasyon noktası (seam) tanımları.
├── Controllers/      HTTP uç noktaları; iş mantığı içermez, Business katmanını çağırır.
├── Views/            Ekranlar. Liste ekranları sunucu tarafında satır üretmez;
│                     tarayıcı ilgili veriyi JSON olarak çekip kendisi kurar.
├── Filters/          Giriş ve yetki kontrolü.
├── Utils/            Tek bir Business sınıfı içinde kullanılan yardımcı fonksiyonlar.
└── Delivery/         Veritabanı şema betikleri.
```

Ayrıca, modülün **dışında** ama kurulum için gerekli bir klasör daha vardır:

```
wwwroot/Content/Badges/
```

Rozet ikonu olarak kullanılabilecek görsel dosyaları içerir. Modül kodu
`Areas/ProfilRozetleri/` altındadır ama bu görseller host projenin kendi
`wwwroot` klasöründe durur; modül yalnızca bu klasörün yolunu bilir, içeriğini
üretmez. **Bu modülü başka bir projeye taşırken `Areas/ProfilRozetleri/` ile
birlikte bu klasörün de kopyalanması gerekir**, aksi hâlde rozet ekleme
ekranındaki ikon seçici boş görünür.

## Veritabanı Şeması

Sistem yedi tablodan oluşur.

### Modules

Sistemde hangi controller'ların rozet sistemine "modül" olarak tanıtıldığını
tutar. Bir alanın modül olarak tanımlanması, o alana yapılan ziyaretlerin
rozet kazanım kurallarında değerlendirilebilmesi için gereklidir.

| Alan | Tip | Açıklama |
|---|---|---|
| ModuleId | INT (Identity) | Birincil anahtar. |
| ModuleName | NVARCHAR(100) | Ekranlarda gösterilen modül adı. |
| ControllerName | NVARCHAR(100) | İstek günlüğündeki Controller alanıyla birebir eşleşmesi gereken teknik ad; üzerinde benzersizlik kısıtı vardır. |

### UserProfile

Kullanıcının sisteme giriş sıklığını izler. "Sistem" türü rozetlerin kazanım
kontrolü bu tablo üzerinden yapılır.

| Alan | Tip | Açıklama |
|---|---|---|
| UserId | INT | Birincil anahtar; kullanıcı tablosuna yabancı anahtar (bire bir ilişki). |
| LastLoginDate | DATE (NULL) | En son giriş yapılan gün. |
| ConsecutiveLoginDays | INT (varsayılan 0) | Üst üste giriş yapılan gün sayısı. |

### UserLevel

Kullanıcının seviye ve deneyim puanı (XP) bilgisini tutar.

| Alan | Tip | Açıklama |
|---|---|---|
| UserId | INT | Birincil anahtar; kullanıcı tablosuna yabancı anahtar (bire bir ilişki). |
| Level | INT (varsayılan 1) | Güncel seviye. |
| XP | INT (varsayılan 0) | Güncel deneyim puanı. |
| LastXPUpdateDate | DATE (NULL) | XP'nin en son güncellendiği gün. |

### Badges

Sistemde tanımlı her rozetin bilgisini tutar.

| Alan | Tip | Açıklama |
|---|---|---|
| BadgeId | INT (Identity) | Birincil anahtar. |
| BadgeName | NVARCHAR(100) | Rozet adı. |
| BadgeDescription | NVARCHAR(500) (NULL) | Rozet açıklaması. |
| IconPath | NVARCHAR(255) (NULL) | Rozet ikonunun dosya yolu. |
| BadgeType | INT | Rozet türü: 0=Sistem, 1=Modül, 2=Keşif, 3=Dış Sinyal. |
| RequiredValue | INT | Kazanım için gereken sayı. |
| ModuleId | INT (NULL) | Yalnızca Modül/Keşif türünde dolu; Modules tablosuna yabancı anahtar. |
| ExternalSignalKey | NVARCHAR(100) (NULL) | Yalnızca Dış Sinyal türünde dolu; dış koşulu tanımlayan anahtar. |

Veritabanı kısıtları (CHECK), rozet türüne göre `ModuleId`/`ExternalSignalKey`
alanlarının doğru şekilde dolu ya da boş olmasını zorunlu kılar.

### UserBadge

Bir kullanıcının bir rozeti kazanıp kazanmadığını ve kazanma tarihini tutar.

| Alan | Tip | Açıklama |
|---|---|---|
| UserBadgeId | INT (Identity) | Birincil anahtar. |
| UserId | INT | Kullanıcı tablosuna yabancı anahtar. |
| BadgeId | INT | Badges tablosuna yabancı anahtar (silme durumunda ilişkili kayıtlar da silinir). |
| IsEarned | BIT (varsayılan 0) | Rozetin kazanılıp kazanılmadığı. |
| EarnedDate | DATETIME (NULL) | Kazanılma tarihi ve saati. |

`(UserId, BadgeId)` ikilisi üzerinde benzersizlik kısıtı vardır. Bir rozet
tanımı silindiğinde, o rozete ait tüm kullanıcı kazanım kayıtları da
otomatik silinir.

### UserBadgeProgress

Yalnızca "Modül" türü rozetler için, bir kullanıcının ilgili modüle en son
hangi gün girdiğini ve kaç gündür üst üste girdiğini tutar.

| Alan | Tip | Açıklama |
|---|---|---|
| UserBadgeProgressId | INT (Identity) | Birincil anahtar. |
| UserBadgeId | INT | UserBadge tablosuna yabancı anahtar (silme durumunda ilişkili kayıt da silinir). |
| LastSeenDateThisModule | DATE (NULL) | İlgili modüle en son girilen gün. |
| RepeatCount | INT (varsayılan 0) | Üst üste giriş yapılan gün sayısı. |

Bir `UserBadge` kaydının yalnızca bir ilerleme kaydı olabilir.

### BadgeProcessState

Arka planda periyodik olarak çalışan işlemin, istek günlüğünde nereye kadar
işlem yaptığını tutan tek satırlık bir konum bilgisidir (imleç). Tabloda tam
olarak bir satır bulunması gerekir (Id=1); bu satır olmadan arka plan işlemi
çalışamaz.

| Alan | Tip | Açıklama |
|---|---|---|
| Id | INT | Birincil anahtar; yalnızca 1 değerini alabilir. |
| LastProcessedLogId | INT (varsayılan 0) | İşlenen son istek günlüğü kaydının kimliği. |
| LastRunDate | DATETIME2 (NULL) | İşlemin en son çalıştığı zaman. |

## Sistemin İşleyişi

### Arka Plan İşleminin Çalışma Döngüsü

Rozet ve seviye güncellemeleri, düzenli aralıklarla (yapılandırılabilir bir
süreyle) otomatik çalışan bir arka plan işlemi tarafından yapılır. Her
çalıştırmada:

1. En son işlenen istek günlüğü kaydının kimliği (`BadgeProcessState`'te
   tutulan imleç) okunur.
2. Bu kimlikten sonraki tüm yeni istek günlüğü kayıtları çekilir.
3. Kayıtlar kullanıcıya göre gruplanır.
4. Her kullanıcı için sırasıyla giriş kontrolü, modül ziyaret kontrolü ve
   dış sinyal kontrolü çalıştırılır.
5. İşlem bitince imleç, işlenen en son kaydın kimliğine ilerletilir.

İmleç güvenliği: Bir kullanıcının en son ziyaretinin ne kadar sürdüğü, ancak
ondan sonraki bir kayıt geldiğinde anlaşılabilir (bkz. "An Gruplama
Mekanizması" aşağıda). Böyle bir sonraki kayıt henüz yoksa, o ziyaretin
süresi bu çalıştırmada hesaplanamaz; bu durumda imleç, o ziyaretin
başlangıcını geçmeyecek şekilde durdurulur, böylece bir sonraki çalıştırmada
aynı kayıt yeniden değerlendirilir.

### Rozet Türleri ve Kazanım Kontrolü

**Sistem türü** — Kullanıcının genel giriş sıklığına bakar. Bir toplu
işlemede kullanıcının ilk kaydının günü, o günün "giriş anı" sayılır (oturum
açma sayfasının kendisine yapılan istekler bu sayıma dahil edilmez). Bu gün,
kullanıcının önceki en son giriş günüyle karşılaştırılır: aynı günse hiçbir
şey değişmez; bir önceki günse ardışık giriş sayacı bir artırılır; daha eski
bir günse ya da hiç kayıt yoksa sayaç 1'e ayarlanır (0'a değil, çünkü bugün
zaten yeni serinin ilk günüdür). Bu sayaç, rozetin gereken sayısına
ulaştığında rozet kazanılır.

**Modül türü** — Kullanıcının belirli bir modüle yaptığı ziyaretlere bakar,
ardışık gün serisi gerektirir. Bir ziyaretin "yeterli" sayılması için, o
modülde belirli bir süre (varsayılan 15 saniye) kalınmış olması gerekir; bu
süre, aynı modüle ait iki ayrı "an" arasındaki zaman farkı olarak hesaplanır.
Yeterli bir ziyaret tespit edildiğinde ilerleme kaydı güncellenir: aynı gün
içindeki tekrar ziyaretler sayacı artırmaz; bir önceki günse sayaç artar;
daha eskiyse ya da hiç kayıt yoksa sayaç 1'e ayarlanır. Sayaç, rozetin
gereken sayısına ulaştığında rozet kazanılır.

**Keşif türü** — Modül türüyle aynı "yeterli ziyaret" tanımını kullanır ama
ilerleme kaydı tutmaz; yeterli bir ziyaret tespit edildiği anda rozet hemen
kazanılır.

**Dış Sinyal türü** — Kazanım koşulu istek günlüğünde yoktur. Arka plan
işleminin her çalıştırmasında, o turda görülen her kullanıcı için (bir istek
günlüğü kaydına bağlı olmadan) şu soru host sisteme yöneltilir: "Bu kullanıcı
için, şu anahtar kelimeyle tanımlı koşul sağlandı mı?" Sistem bu anahtar
kelimenin ne anlama geldiğini bilmez, olduğu gibi host'a iletir. Cevap
olumluysa rozet hemen kazanılır.

### "An" (Aynı Ziyaret) Gruplama Mekanizması

Bir sayfa ziyareti tek bir istek günlüğü kaydı üretmeyebilir: sayfa
yüklenirken tarayıcı, aynı anda başka arka plan istekleri de gönderebilir.
Bu istekler farklı hedeflere gitse de aynı sayfa ziyaretinin parçasıdır.

Bunu ayırt etmek için, birbirine belirli bir süre (varsayılan 2 saniye,
yapılandırılabilir) içinde gelen kayıtlar tek bir "an" olarak gruplanır. Bir
modülde kalınan süre, iki ayrı an arasındaki fark olarak ölçülür — art arda
gelen tekil kayıtlar arasında değil.

### Seviye ve Deneyim Puanı (XP) Hesabı

Günlük ilk giriş ve her rozet kazanımı, belirlenmiş miktarlarda XP kazandırır
(miktarlar yapılandırılabilir). Seviye, toplam XP'nin "seviye başına gereken
XP" değerine bölünmesiyle hesaplanır.

XP azalması "tembel" biçimde hesaplanır: ayrı bir zamanlayıcı yoktur. Bir
kullanıcı yeniden XP kazandığında, en son XP güncellemesinden bu yana geçen
gün sayısına bakılır; belirli bir gün sayısından sonra, geçen her ek gün için
belirli bir miktar XP düşürülür. Bu düşüş, yeni XP eklenmeden hemen önce
uygulanır.

### "Yeni" Rozet Etiketi

Kazanılan bir rozetin arayüzde "Yeni" olarak işaretlenip işaretlenmeyeceği,
kullanıcının rozet görüntüleme sayfasına en son ne zaman girdiğine bakılarak
belirlenir. Bu son ziyaret tarihi de istek günlüğünden hesaplanır; "an"
gruplama mantığı burada da kullanılır, çünkü sayfayı görüntülemenin kendisi
de bir istek günlüğü kaydı üretir ve bu kayıt sorgu çalıştığı sırada zaten
yazılmış olabilir — bu yüzden en taze kayıt "şimdi"ye çok yakınsa atlanır,
ondan önceki kayıt gerçek "son ziyaret" sayılır. Bir rozet, bu son ziyaretten
sonra kazanılmışsa "Yeni" gösterilir; sayfa bir kez görüntülendikten sonra
etiket kendiliğinden kalkar.

### Yönetim Ekranlarının Çalışma Prensibi

Rozet ve modül tanımlarının listelendiği ekranlarda tablo satırları sunucu
tarafında üretilmez. Sayfa açıldığında tarayıcı, ilgili veriyi bir uçtan JSON
biçiminde çeker ve tablo satırlarını bu veriden kendisi kurar. Ekleme ve
silme işlemleri de sayfa yenilenmeden, aynı şekilde arka planda gerçekleşir.

## Entegrasyon Noktaları

Sistem kendi başına çalışmaz; dört noktada içinde bulunduğu yazılıma (host)
bağımlıdır. Sistem bu noktalarda bir soru sorar, cevabı host verir; sistemin
kendisi cevabın nasıl üretildiğini bilmez.

| Entegrasyon Noktası | Sorduğu Soru | Cevaplanmazsa Ne Olur |
|---|---|---|
| İstek günlüğü erişimi | Hangi kullanıcı hangi sayfaya ne zaman girdi? | Hiçbir rozet/seviye güncellemesi yapılmaz. |
| Kimlik bilgisi | Geçerli oturumda kim var? | Herkes "giriş yapılmamış" sayılır. |
| Dış başarım sinyali | İstek günlüğüyle ilgisi olmayan şu koşul sağlandı mı? | Dış Sinyal türü rozetler hiçbir zaman kazanılmaz. |
| Yönetim yetkisi | Bu kullanıcının yönetim ekranına erişim yetkisi var mı? | Yönetim ekranına kimse erişemez (güvenli varsayılan). |

Bu dört noktadan biri host sistemde karşılığı olmadan bırakılırsa sistem hata
vermez; yalnızca ilgili işlev güvenli bir şekilde devre dışı kalır. Bu,
kurulumun aşamalı yapılabilmesini sağlar (örneğin önce temel işlevler, sonra
Dış Sinyal desteği).

Yetkilendirme akışı iki aşamalıdır: önce kullanıcının oturum açmış olup
olmadığına bakılır (açmamışsa oturum açma sayfasına yönlendirilir), ardından
yönetim yetkisi host sisteme sorulur (yetkisi yoksa erişim reddedilir ve
açıklamalı bir sayfa gösterilir). Aranan yetkinin adı sistemde sabit değildir,
yapılandırma üzerinden belirlenir.

## Kurulum

1. **Veritabanı şeması**: Bu belgenin sonundaki [Sorgu Kodları](#sorgu-kodları)
   bölümündeki betiği hedef veritabanında çalıştırın. Kullanıcı tablosuna
   yapılan yabancı anahtar referanslarının, hedef sistemin kendi kullanıcı
   tablosu birincil anahtarına göre uyarlanması gerekir. Betik tekrar
   çalıştırıldığında var olan tabloları yeniden oluşturmaz; tablo adlarının
   hedef sistemde başka bir amaçla kullanılıp kullanılmadığının önceden
   kontrol edilmesi önerilir.
2. **Kod ve varlıkların kopyalanması**: `Areas/ProfilRozetleri/` klasörünün
   tamamı ve `wwwroot/Content/Badges/` klasörü hedef projeye kopyalanır.
3. **Entegrasyon noktalarının uygulanması**: Yukarıdaki dört noktanın her biri
   için hedef sistemde bir karşılık yazılıp bağımlılık kaydına (dependency
   injection) eklenir.
4. **Yapılandırma ayarları**: Aşağıdaki ayarlar uygulama yapılandırma
   dosyasına eklenir.

| Anahtar | Ayar | Anlamı |
|---|---|---|
| JobAralikSaniye | Arka plan işi çalışma aralığı | İstek günlüğünün ne sıklıkla tarandığı (saniye). |
| ModulKalmaEsigiSaniye | Modülde kalma eşiği | Bir modül ziyaretinin "yeterli" sayılması için gereken minimum süre (saniye). |
| BurstEsigiSaniye | Aynı an sayılma eşiği | Birbirine bu kadar yakın istek günlüğü kayıtlarının tek bir ziyaret anı sayılacağı süre (saniye). |
| GunlukGirisXP | Günlük giriş XP'si | Günün ilk girişinde kazanılan deneyim puanı. |
| RozetKazanimXP | Rozet kazanım XP'si | Bir rozet kazanıldığında verilen deneyim puanı. |
| SeviyeBasinaXP | Seviye başına gereken XP | Bir sonraki seviyeye geçmek için gereken toplam XP. |
| XPDususBaslamaGunu | XP düşüş başlama günü | Kaç gün XP kazanılmazsa düşüşün başlayacağı. |
| GunlukXPDususu | Günlük XP düşüşü | Düşüş başladıktan sonra her gün ne kadar XP azalacağı. |
| GirisSayfasiUrl | Giriş sayfası adresi | Oturum açmamış bir kullanıcının yönlendirileceği adres. |
| HaricTutulanControllerler | Hariç tutulan controller listesi | Modül olarak tanımlanması anlamlı olmayan alanların (örn. oturum açma sayfası, ana panel) listesi. |
| YonetimYetkisiAdi | Yönetim yetkisi adı | Yönetim ekranına erişim için host sistemden sorulacak yetkinin adı. |

Bu anahtarlar, uygulamanın yapılandırma dosyasında aşağıdaki gibi bir bölüm
altında tanımlanır. Bölümün adı ("ProfilRozetleriModulu"), modülün sağladığı
bağımlılık kaydı uzantısının okuduğu sabit addır; değiştirilecekse o
uzantıdaki karşılığının da güncellenmesi gerekir.

```json
"ProfilRozetleriModulu": {
  "JobAralikSaniye": 20,
  "ModulKalmaEsigiSaniye": 15,
  "BurstEsigiSaniye": 2.0,
  "GunlukGirisXP": 10,
  "RozetKazanimXP": 50,
  "SeviyeBasinaXP": 100,
  "XPDususBaslamaGunu": 3,
  "GunlukXPDususu": 5,
  "GirisSayfasiUrl": "/Account/Login",
  "HaricTutulanControllerler": [ "Account", "Home" ],
  "YonetimYetkisiAdi": "RozetYonetimi"
}
```

## Yönetim Ekranları

### Rozet Listesi Ekranı

Tanımlı tüm rozetleri; ikon, ad, tür, gereken sayı ve bağlı olduğu
modül/sinyal bilgisiyle listeler. Her satırda düzenleme ve silme işlemleri
bulunur. Bir rozetin silinmesi, o rozete ait tüm kullanıcı kazanım
kayıtlarını da birlikte siler; bu, kazanılmış bir rozetin de silinebilmesi
için kasıtlı bir davranıştır.

### Yeni Rozet Ekleme / Düzenleme

| Alan | Açıklama |
|---|---|
| Rozet Adı | Kullanıcıya gösterilecek isim. |
| Açıklama | Rozetin üzerine gelindiğinde gösterilen açıklama metni. |
| Rozet Türü | Dört seçenekten biri; aşağıdaki tabloya bakınız. |
| Gereken Sayı | Sistem/Modül türünde gereken ardışık gün sayısı. Keşif/Dış Sinyal türünde anlamsızdır, otomatik olarak 1'e sabitlenir ve değiştirilemez. |
| Modül | Yalnızca Modül/Keşif türünde görünür ve zorunludur. |
| Dış Sinyal | Yalnızca Dış Sinyal türünde görünür ve zorunludur; yalnızca hedef sistemin gerçekten desteklediği (koda eklenmiş) sinyaller listede görünür. |
| Rozet İkonu | Sağlanan görsel seçeneklerinden biri seçilir. |

### Rozet Türleri — Ne Zaman Hangisi Kullanılır

| Tür | Ne Zaman Kullanılır | Ek Kod Gerekir mi? |
|---|---|---|
| Sistem | Genel giriş sıklığına dayalı ödüller (örn. "7 gün üst üste giriş"). | Hayır |
| Modül | Belirli bir alana ardışık günlerde girişi ödüllendirmek için. | Hayır |
| Keşif | Bir alanın yalnızca bir kez, yeterli süreyle kullanılmasını ödüllendirmek için. | Hayır |
| Dış Sinyal | İstek günlüğüyle ilgisi olmayan bir koşulu ödüllendirmek için. | Evet |

### Modül Yönetimi Ekranı

Modül/Keşif türü rozetlerin bağlanacağı alanların tanımlandığı ekrandır. İki
yol sunar:

- **Otomatik keşif**: Hedef yazılımın kendi yönlendirme (routing) kaydından
  bulunabilen, henüz modül olarak tanımlanmamış alanlar otomatik listelenir;
  bir görünecek ad verilip eklenir. Teknik ad elle yazılmaz, listeden seçilir.
- **Elle ekleme**: Otomatik keşif yalnızca hedef yazılımın kendi
  altyapısından bulunabilenleri gösterir. Bu şekilde bulunamayan bir alan
  için, istek günlüğündeki Controller değeri elle girilerek de modül
  tanımlanabilir. Bu değerin istek günlüğündeki gerçek değerle birebir
  eşleşmesi gerekir; eşleşmezse o modüle bağlı rozetler hiçbir zaman
  kazanılmaz.

Bir modülün silinmesi, o modüle bağlı en az bir rozet varsa engellenir
(rozet silmedeki davranışın aksine) — önce bağlı rozetlerin silinmesi ya da
başka bir modüle taşınması gerekir.

## Dış Sinyal Rozeti Eklemek

Diğer üç rozet türünün aksine, Dış Sinyal türü bir kazanım koşulunu kod
yazarak tanımlamayı gerektirir; yönetim ekranından yalnızca veri girişi
yeterli değildir.

**Adım 1 — Sinyal Anahtarını Belirleyin.** Kazanım koşulunu temsil eden,
benzersiz ve açıklayıcı bir metin anahtarı seçilir (örnek:
`rapor-disa-aktarildi`). Küçük harf ve tire ile yazılmalı (kebab-case),
boşluk veya Türkçe karakter içermemelidir.

**Adım 2 — Koşulu Sorgulayan Kodu Yazın.** Hedef sistemde, dış başarım
sağlayıcı arayüzünü uygulayan bir sınıf bulunur ya da oluşturulur:

- `IsAchieved(kullanıcı kimliği, sinyal anahtarı)`: Gelen sinyal anahtarı
  Adım 1'deki anahtarla karşılaştırılır; eşleşirse koşulun gerçekten
  sağlanıp sağlanmadığı kontrol edilip doğru/yanlış döndürülür.
- `GetSupportedSignals()`: Desteklenen tüm sinyal anahtarlarının listesini
  (anahtar + okunabilir etiket) döndürür. Adım 1'deki anahtar bu listeye
  eklenmelidir — bu adım atlanırsa sinyal yönetim ekranında görünmez.

```csharp
public bool IsAchieved(int kullaniciId, string sinyalAnahtari)
{
    if (sinyalAnahtari == "rapor-disa-aktarildi")
    {
        return kullaniciRaporDisaAktarmisMi(kullaniciId);
    }
    return false;
}

public List<SinyalTanimi> GetSupportedSignals()
{
    return new List<SinyalTanimi>
    {
        new SinyalTanimi { Key = "rapor-disa-aktarildi", Label = "Rapor Dışa Aktarıldı" }
    };
}
```

**Adım 3 — Kodu Dağıtın.** Yazılan kod hedef sisteme dağıtılıp çalışır hale
getirilmelidir; aksi halde sinyal yönetim ekranında görünmez.

**Adım 4 — Yönetim Ekranından Rozeti Tanımlayın.** Rozet Türü olarak "Dış
Sinyal" seçilir, Adım 2'de eklenen sinyal seçilir, Gereken Sayı otomatik
1'e sabitlenir, ikon seçilip kaydedilir.

**Adım 5 — Doğrulama.** İlgili koşulu sağlayan bir kullanıcı hesabıyla test
edilip rozetin beklenen şekilde kazanıldığı doğrulanmalıdır. Arka plan
işleminin çalışma aralığı kadar gecikme olabileceği unutulmamalıdır.

## Sık Karşılaşılan Durumlar

| Durum | Olası Neden |
|---|---|
| Bir alan "Keşfedilen Controller'lar" listesinde görünmüyor. | Zaten bir modüle bağlı olabilir ya da hariç tutulan controller listesinde olabilir. |
| Bir rozet hiç kazanılmıyor (Modül/Keşif türü). | Modülün teknik adı, istek günlüğündeki değerle birebir eşleşmiyor olabilir. |
| Dış Sinyal listesi boş görünüyor. | İlgili entegrasyon noktası hedef sistemde hiç uygulanmamış olabilir. |
| Yeni eklenen bir Dış Sinyal seçim listesinde yok. | Yukarıdaki Adım 2 ya da Adım 3 tamamlanmamış olabilir. |
| Yönetim ekranına erişilemiyor. | Kullanıcının yönetim yetkisi olmayabilir. |

## Sorgu Kodları

Aşağıdaki betik, yedi tabloyu ve aralarındaki ilişkileri kurar. Betik
idempotenttir: tablo zaten varsa yeniden oluşturmaz, tekrar çalıştırmak
güvenlidir. `Users` tablosuna yapılan yabancı anahtar referanslarının, hedef
sistemin kendi kullanıcı tablosu birincil anahtarına göre uyarlanması
gerekir.

```sql
/* Modul/controller tanimlari. ControllerName, istek gunlugundeki Controller
   alaniyla birebir eslesecek sekilde girilmelidir. */
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

/* Kullanicinin genel giris siklik bilgisini tutar. UserId hem PK hem
   kullanici tablosuna FK (bire bir iliski). */
IF OBJECT_ID('dbo.UserProfile', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.UserProfile
    (
        UserId               INT  NOT NULL,
        LastLoginDate        DATE NULL,
        ConsecutiveLoginDays INT  NOT NULL CONSTRAINT DF_UserProfile_ConsecutiveLoginDays DEFAULT (0),

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

/* Rozet tanimlari. BadgeType: 0=Sistem, 1=Modul, 2=Kesif, 3=Dis Sinyal.
   CHECK kisitlari, ModuleId/ExternalSignalKey alanlarinin turle uyumunu
   veritabani seviyesinde garanti eder. BadgeType INT'tir (TINYINT degil):
   uygulama tarafinda enum'in varsayilan alt tipi int'tir. */
IF OBJECT_ID('dbo.Badges', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Badges
    (
        BadgeId           INT           IDENTITY(1,1) NOT NULL,
        BadgeName         NVARCHAR(100) NOT NULL,
        BadgeDescription  NVARCHAR(500) NULL,
        IconPath          NVARCHAR(255) NULL,
        BadgeType         INT           NOT NULL,
        RequiredValue     INT           NOT NULL,
        ModuleId          INT           NULL,
        ExternalSignalKey NVARCHAR(100) NULL,

        CONSTRAINT PK_Badges PRIMARY KEY CLUSTERED (BadgeId),
        CONSTRAINT FK_Badges_Modules FOREIGN KEY (ModuleId)
            REFERENCES dbo.Modules (ModuleId),
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
        UserBadgeId INT      IDENTITY(1,1) NOT NULL,
        UserId      INT      NOT NULL,
        BadgeId     INT      NOT NULL,
        IsEarned    BIT      NOT NULL CONSTRAINT DF_UserBadge_IsEarned DEFAULT (0),
        EarnedDate  DATETIME NULL,

        CONSTRAINT PK_UserBadge PRIMARY KEY CLUSTERED (UserBadgeId),
        CONSTRAINT FK_UserBadge_Users FOREIGN KEY (UserId)
            REFERENCES dbo.Users (Id),
        -- ON DELETE CASCADE: kazanilmis bir rozetin de silinebilmesi
        -- gerekir; aksi halde FK ihlaliyle hata verir.
        CONSTRAINT FK_UserBadge_Badges FOREIGN KEY (BadgeId)
            REFERENCES dbo.Badges (BadgeId) ON DELETE CASCADE
    );

    CREATE UNIQUE INDEX UX_UserBadge_User_Badge
        ON dbo.UserBadge (UserId, BadgeId);
END
GO

/* Yalnizca Modul turu rozetler icin doldurulur. Hangi UserBadge kaydina ait
   oldugu UserBadgeId'den bilinir. */
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

/* Arka plan isinin istek gunlugunde nereye kadar isleme yaptigini tutan tek
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
```
