# Ayşe Nur İnan Altınkaya — Blog Sitesi (ASP.NET Core MVC)

Dil ve Konuşma Terapisti için hazırlanmış, admin panelinden blog yazısı
eklenip düzenlenebilen kurumsal blog sitesi.

**Teknolojiler:** ASP.NET Core MVC (.NET 8) · Entity Framework Core ·
Pomelo MySQL sağlayıcısı · Cookie tabanlı admin girişi

---

## 1. Gereksinimler (bilgisayarınızda kurulu olmalı)

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [VS Code](https://code.visualstudio.com/) + **C# Dev Kit** eklentisi
- Yerelde test için MySQL/MariaDB (XAMPP, Laragon veya Docker ile) — hosting'e
  yüklerken buna gerek yok, oradaki MySQL'i kullanacaksınız.

## 2. Yerel geliştirme ortamını kurma

```bash
cd site-dotnet
dotnet restore
```

Bağlantı dizesini yerel MySQL bilgilerinizle güncelleyin ve tabloları oluşturun için EF Core Migrations kullanın:

```json
"DefaultConnection": "Server=localhost;Port=3306;Database=blogdb;User=root;Password=;"
```

Veritabanı tablolarını oluşturmak için (EF Core Migrations):

```bash
dotnet tool install --global dotnet-ef   # ilk seferde bir kere
dotnet ef migrations add IlkKurulum
dotnet ef database update
```

Bu komut `posts` ve `admin_users` tablolarını modeldeki alanlara göre otomatik oluşturur.

## 3. Projeyi çalıştırma

```bash
dotnet run
```

Tarayıcıda `https://localhost:5001` (veya terminalde yazan adres) açılır.

## 4. Önemli: Veritabanını sıfırlama (şema değişti)

Kullanıcı/soru-cevap sistemi eklendiğinde veritabanı yapısı değişti (yeni
`kullanicilar` ve `sorular` tabloları, `Rol` alanı). Daha önce eski şemayla
migration oluşturduysanız, **temiz bir başlangıç** yapmanız gerekir:

1. Projenizdeki `Migrations` klasörünü tamamen silin (varsa).
2. MySQL Workbench'te veritabanınızı sıfırlayın:
   ```sql
   DROP DATABASE blogdb;
   CREATE DATABASE blogdb;
   ```
3. Yeni migration'ı oluşturun:
   ```bash
   dotnet ef migrations add IlkKurulum
   dotnet ef database update
   ```

## 5. Kullanıcı sistemi nasıl çalışır?

- **Herkes** `/hesap/kayit` adresinden ücretsiz hesap oluşturabilir.
- Kayıt formunda **"Sahip Kodu"** alanı vardır. `appsettings.json` içindeki
  `SiteAyarlari:SahipKodu` değeriyle birebir aynısını girip kayıt olan kişi
  **"sahip"** rolünü alır (blog yazabilir, gelen soruları cevaplayabilir).
  Bu alanı boş bırakan herkes **"kullanici"** rolüyle kaydolur (sadece soru
  sorabilir).
- **ÖNEMLİ:** `appsettings.json`'daki `SahipKodu` değerini gerçek, tahmin
  edilmesi zor bir kodla değiştirin ve bu kodu sadece Ayşe Nur'a verin.
  Kayıt sonrası isterseniz kodu tekrar değiştirebilirsiniz (yeni kayıt olan
  kimse "sahip" rolü alamaz).
- Ayşe Nur bu kodla kayıt olduktan sonra `/admin` panelinden hem yazı
  yönetimini hem de **Sorular** sekmesinden gelen soruları cevaplayabilir.
- Cevaplanan sorular otomatik olarak `/sorular` (S.S.S.) sayfasında herkese
  açık şekilde görünür.

## 6. Instagram linkini ve site bilgilerini değiştirme

`appsettings.json` içinde:

```json
"SiteAyarlari": {
  "SiteAdi": "Ayşe Nur İnan Altınkaya",
  "SiteAciklama": "Dil ve Konuşma Terapisti",
  "InstagramUrl": "https://instagram.com/kullanici_adiniz"
}
```

## 6. Hakkında sayfası profil fotoğrafı

`wwwroot/images/profil.jpg` konumuna kendi fotoğrafınızı ekleyin (bu klasörü
projeye siz oluşturmalısınız).

Blog kartlarında kapak resmi olmayan (default) durum için isterseniz
`wwwroot/uploads/varsayilan.jpg` adında bir görsel ekleyebilirsiniz.

---

## 7. HOSTİNGE YAYINLAMA (Paylaşımlı Windows Hosting)

Hosting sağlayıcınız **.NET Core 10.X**'i destekliyor — proje zaten bu sürümü
hedefliyor (`net10.0`), ekstra bir ayar gerekmiyor.

### A) Sunucuda ASP.NET Core Hosting Bundle kurulu mu kontrol edin
IIS'in .NET Core uygulamalarını çalıştırabilmesi için sunucuda **.NET 10
ASP.NET Core Hosting Bundle**'ın kurulu olması gerekir. Çoğu Plesk tabanlı
Türk hosting sağlayıcısı panelden ".NET Core Site Ekle" seçeneğiyle bunu
otomatik ayarlar; emin değilseniz destek ekibine "10.X için Hosting Bundle
kurulu mu?" diye sorun.

### B) Visual Studio ile yayına hazırlama (VS Code'dan daha kolay)
1. Visual Studio'da projeyi açın (`BlogSite.csproj`'a çift tıklayın).
2. Solution Explorer'da projeye sağ tık → **Publish**.
3. **Folder** (Klasöre Yayınla) seçin, bir klasör belirleyin, **Publish** deyin.
4. Oluşan klasördeki tüm dosyaları hosting panelinizdeki dosya yöneticisi
   veya FTP ile `.NET Core` sitesi için ayrılan dizine yükleyin.

VS Code'dan komut satırıyla da yapılabilir:

```bash
dotnet publish -c Release -o ./yayin
```

`yayin` klasöründeki her şeyi FTP ile hosting'e yükleyin.

### C) Hosting'deki appsettings.json'u güncelleyin
Sunucuya yüklenen `appsettings.json` içindeki `DefaultConnection` değerini,
hosting panelinden oluşturduğunuz gerçek MySQL veritabanı bilgileriyle
değiştirin (host adı genelde `localhost` kalır, kullanıcı adı/şifre/DB adı
hosting panelinden alınır).

### D) Veritabanı tablolarını sunucuda oluşturma
İki seçenek:
1. **Kolay yol:** Yerelde `dotnet ef migrations script` ile bir SQL dosyası
   üretip phpMyAdmin'den çalıştırın:
   ```bash
   dotnet ef migrations script -o migration.sql
   ```
   Oluşan `migration.sql` dosyasını phpMyAdmin → **İçe Aktar (Import)** ile
   hosting veritabanınıza yükleyin.
2. İlk admin hesabını sunucuda da `/admin/ilk-kurulum` adresinden oluşturun.

### E) IIS için web.config
`dotnet publish` komutu yayın klasörüne otomatik bir `web.config` dosyası
ekler (IIS'in uygulamayı ASP.NET Core Module üzerinden çalıştırması için
gereklidir) — bu dosyayı silmeyin, olduğu gibi yükleyin.

---

## Proje Yapısı

```
Controllers/     → HomeController (ziyaretçi), AdminController (yönetim)
Models/          → Post, AdminUser, ViewModels, SiteAyarlariOptions
Data/            → AppDbContext (EF Core)
Services/        → Slug oluşturma, resim yükleme, tarih formatlama
Views/           → Razor sayfaları (Home/, Admin/, Shared/_Layout.cshtml)
wwwroot/         → CSS, yüklenen görseller (uploads/)
```
