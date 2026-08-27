# Rozet Sistemi

Kullanıcıların bir yazılım sistemine ve o sistemdeki tanımlı modüllere düzenli
kullanımını rozet ve seviye/deneyim puanı (XP) mekanizmasıyla ödüllendiren,
bağımsız bir modül. Belirli bir kuruma özgü değildir; kendi kullanıcı ve
istek günlüğü altyapısı bulunan herhangi bir ASP.NET Core (.NET) tabanlı
yazılıma entegre edilebilecek şekilde tasarlanmıştır.

## Belgeler

Bu depoda kod dışında üç ayrı rapor bulunur; ayrıntılı bilgi için bunlara
bakılmalıdır:

| Belge | İçerik |
|---|---|
| **Rozet Sistemi Veritabanı Raporu** | Tabloların tam alan listesi, veri tipleri, ilişkileri ve olası çakışma riskleri. |
| **Rozet Sistemi İşleyiş ve Algoritma Raporu** | Arka plan işleminin çalışma döngüsü, rozet türlerine göre kazanım algoritması, "an" gruplama mekanizması, seviye/XP hesabı. |
| **Rozet Sistemi Kurulum ve Kullanım Yönergesi** | Kurulum adımları, yönetim ekranlarının kullanımı, yeni bir Dış Sinyal rozeti eklemek için adım adım kod talimatı. |

Bu README, yalnızca depo yapısını ve hızlı bir başlangıç noktasını özetler;
kurulum ve kullanım ayrıntıları yukarıdaki belgelerdedir.

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
└── Delivery/         Veritabanı şema betikleri (bkz. aşağıdaki "Kurulum" bölümü).
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

## Mimari İlkeler

- **Host'a bağımlı değildir.** Modül, kendi veritabanı bağlamını (DbContext)
  kurar; host'un ana veritabanı bağlamına dokunmaz.
- **Dört entegrasyon noktası (seam) üzerinden dışarıyla konuşur.** Modül bir
  soru sorar, host cevabı verir; modül cevabın nasıl üretildiğini bilmez.

  | Entegrasyon Noktası | Sorduğu Soru |
  |---|---|
  | İstek günlüğü erişimi | Hangi kullanıcı hangi sayfaya ne zaman girdi? |
  | Kimlik bilgisi | Geçerli oturumda kim var? |
  | Dış başarım sinyali | İstek günlüğüyle ilgisi olmayan şu koşul sağlandı mı? |
  | Yönetim yetkisi | Bu kullanıcının yönetim ekranına erişim yetkisi var mı? |

  Bu noktalardan biri host tarafında karşılığı olmadan bırakılırsa modül hata
  vermez; yalnızca ilgili işlev güvenli bir şekilde devre dışı kalır.
- **Veritabanı şeması betiklerle (script) kurulur, migration'a bağlı
  değildir.** `Delivery/` klasöründeki betikler doğrudan çalıştırılır;
  herhangi bir migration geçmişi tablosuna ihtiyaç duymaz.
- **Görsel liste ekranları tarayıcı tarafında dinamik kurulur.** Sunucu
  tarafında satır/kolon üretilmez; sayfa bir uçtan JSON veri çeker, tabloyu
  kendisi oluşturur.

## Rozet Türleri (Özet)

| Tür | Koşul | Ek Kod Gerekir mi? |
|---|---|---|
| Sistem | Genel giriş sıklığı (ardışık gün sayısı). | Hayır |
| Modül | Belirli bir alana ardışık günlerde giriş. | Hayır |
| Keşif | Bir alanın yalnızca bir kez, yeterli süreyle kullanılması. | Hayır |
| Dış Sinyal | İstek günlüğüyle ilgisi olmayan, ayrı bir mekanizmadan sorulan koşul. | Evet |

Dış Sinyal türü için gereken kod adımları Kurulum ve Kullanım Yönergesi'nde
ayrıntılı olarak anlatılmıştır.

## Kurulum (Özet)

1. `Delivery/01-profilrozetleri-tables.sql` betiğini hedef veritabanında
   çalıştırın. Kullanıcı tablosuna yapılan yabancı anahtar referanslarının,
   hedef sistemin kendi kullanıcı tablosu birincil anahtarına göre
   uyarlanması gerekir. `Delivery/` klasöründeki diğer numaralı betikler
   tarihsel düzeltmelerdir ve güncel `01` betiğine zaten dahildir; yeni bir
   kurulumda yalnızca `01` yeterlidir.
2. `Areas/ProfilRozetleri/` klasörünün tamamını ve `wwwroot/Content/Badges/`
   klasörünü hedef projeye kopyalayın.
3. Dört entegrasyon noktasının her biri için hedef sistemde bir karşılık
   yazıp bağımlılık kaydına (dependency injection) ekleyin.
4. Yapılandırma ayarlarını (arka plan işi aralığı, XP/seviye miktarları,
   yönetim yetkisi adı, hariç tutulan controller listesi vb.) uygulama
   yapılandırma dosyasına ekleyin.

Adımların tam açıklaması ve her yapılandırma ayarının anlamı için Kurulum ve
Kullanım Yönergesi belgesine bakılmalıdır.
