# 💈 Kuaför Randevu Sistemi — ASP.NET Core MVC

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/)
[![ASP.NET Core MVC](https://img.shields.io/badge/ASP.NET-Core%20MVC-1f6feb)](https://learn.microsoft.com/aspnet/core)
[![Bootstrap](https://img.shields.io/badge/Bootstrap-5-7952B3)](https://getbootstrap.com/)

Kuaför müşterilerinin kolayca **randevu almasını**; işletmenin de **randevu, hizmet ve yorum** süreçlerini yönetmesini sağlayan modern bir web uygulaması.

> **Durum:** Aktif geliştirme  
> **Ana teknoloji:** ASP.NET Core MVC (.NET 8)  
> **Hedef:** Basit, güvenli, genişletilebilir randevu altyapısı

---

## İçindekiler
- [Özellikler](#özellikler)
- [Ekranlar](#ekranlar)
- [Mimari ve Klasör Yapısı](#mimari-ve-klasör-yapısı)
- [Teknolojiler](#teknolojiler)
- [Kurulum](#kurulum)
- [Geliştirme Komutları](#geliştirme-komutları)
- [Yapılandırma](#yapılandırma)
- [Rotalar / Uç Noktalar](#rotalar--uç-noktalar)
- [Güvenlik Notları](#güvenlik-notları)
- [Sık Karşılaşılan Sorunlar](#sık-karşılaşılan-sorunlar)
- [Yol Haritası](#yol-haritası)
- [Katkıda Bulunma](#katkıda-bulunma)
- [Lisans](#lisans)

---

## Özellikler
- **Kullanıcı Girişi & Kayıt (Modal)**
  - Giriş: logo, karşılama başlığı, kullanıcı adı/e-posta, şifre, *beni hatırla*, bağlantılar
  - Kayıt: ad, soyad, e-posta, telefon, şifre/şifre tekrarı, KVKK/sözleşme onayı, bülten tercihleri
- **Randevu Yönetimi**
  - Ana sayfadaki “**Randevu Al**” butonu; oturum yoksa giriş/kayıt modallarını tetikler
  - Oturum açmış kullanıcı: tarih/saat & **dinamik hizmet** seçimi ile randevu oluşturma
  - Yönetici: onay/red, geçmiş randevular, iptal nedenleri (planlanan)
- **Hizmetler**
  - Dinamik hizmet listesi (başlangıç: **Saç Kesimi**, genişletilebilir: Sakal, Boya, Bakım…)
  - Süre & fiyat bilgisi gösterimi
- **Yorumlar**
  - Müşteri deneyimi girme, admin onayı sonrası yayına alma
- **Arayüz**
  - `_Layout.cshtml` tabanlı, responsive, modern & sade tema
  - Kısımları **partial view** olarak bölüp tekrar kullanılabilir hale getirme
  - (Opsiyonel) Radio-button filtre/AJAX ile bölüm güncelleme

---

## Ekranlar
- **Ana Sayfa (`Home/Index`)**  
  Hero + “Randevu Al” çağrısı, dinamik hizmetler ve kullanıcı yorumları.
- **Hesap (`Account/Login`, `Account/Register`)**  
  Giriş & kayıt modalları; oturum & şifre sıfırlama akışları (plan).
- **Randevular (`Appointments/Index`, `Appointments/Create`)**  
  Kullanıcı: randevu oluşturma ve geçmişini görme.  
  Yönetici: onay/red ve takvim görünümü (plan).

---

## Mimari ve Klasör Yapısı
```
src/
└─ Kuafor.Web
   ├─ Controllers
   │  ├─ HomeController.cs
   │  ├─ AccountController.cs
   │  └─ (plan) AppointmentsController.cs, ServicesController.cs, AdminController.cs
   ├─ Models
   │  ├─ ErrorViewModel.cs
   │  └─ (plan) ApplicationUser.cs, Appointment.cs, Service.cs, Review.cs
   ├─ Views
   │  ├─ Shared
   │  │  ├─ _Layout.cshtml
   │  │  └─ _ValidationScriptsPartial.cshtml
   │  ├─ Home/Index.cshtml
   │  └─ Account/Login.cshtml, Account/Register.cshtml
   ├─ wwwroot
   │  ├─ css/site.css
   │  ├─ js/
   │  └─ lib/
   ├─ Program.cs
   └─ appsettings.json
```
> İleride `Areas/Admin`, `ViewComponents` ve ek `Partial` bölümleri eklenecektir.

---

## Teknolojiler
- **Backend:** ASP.NET Core MVC 8, C#
- **Kimlik Doğrulama:** ASP.NET Identity
- **Veritabanı:** SQL Server (LocalDB veya tam sürüm) *(alternatif: SQLite)*
- **Ön Yüz:** HTML5, CSS3, JavaScript, Bootstrap 5
- **Araçlar:** .NET 8 SDK, Visual Studio / VS Code, Git
- **Opsiyonel:** EF Core Migrations, SMTP ile e-posta bildirimi

---

## Kurulum

### 1) Önkoşullar
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- SQL Server (LocalDB yeterlidir) — *veya* SQLite
- (Opsiyonel) EF Core CLI:
  ```bash
  dotnet tool install --global dotnet-ef
  ```

### 2) Depoyu klonlayın
```bash
git clone https://github.com/Feastskn07/xxx-hairdresser.git
cd xxx-hairdresser/src/Kuafor.Web
```

### 3) Bağımlılıkları yükleyin
```bash
dotnet restore
```

### 4) Veritabanı bağlantısını ayarlayın
`appsettings.json` içindeki `ConnectionStrings:DefaultConnection` değerini kendi ortamınıza göre düzenleyin.  
**SQLite** kullanacaksanız örnek:
```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=kuafor.db"
}
```

### 5) (Opsiyonel) İlk migration & veritabanı
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 6) Çalıştırın
```bash
dotnet run
# veya geliştirme için:
dotnet watch run
```
Tarayıcı: `http://localhost:5000` (veya konsolda belirtilen URL)

---

## Geliştirme Komutları
```bash
# Derleme
dotnet build

# Çalıştırma
dotnet run

# İzleyerek çalıştırma (hot reload)
dotnet watch run

# EF Core
dotnet ef migrations add <MigrationName>
dotnet ef database update
```

---

## Yapılandırma
`appsettings.json` örneği (SQL Server/LocalDB):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\MSSQLLocalDB;Database=KuaforDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Smtp": {
    "Host": "smtp.example.com",
    "Port": 587,
    "EnableSsl": true,
    "User": "no-reply@example.com",
    "Pass": "your-strong-password"
  },
  "AllowedHosts": "*"
}
```

> **Gizli Bilgiler:** Üretimde **User Secrets** veya **ortam değişkenleri** kullanın:
```bash
dotnet user-secrets init
dotnet user-secrets set "Smtp:Pass" "super-secret"
```

---

## Rotalar / Uç Noktalar
| Yöntem | Yol                   | Açıklama                          |
|-------:|-----------------------|-----------------------------------|
| GET    | `/`                   | Ana sayfa (hero, hizmetler, yorumlar) |
| GET    | `/Account/Login`      | Giriş                             |
| POST   | `/Account/Login`      | Giriş işlemi                      |
| GET    | `/Account/Register`   | Kayıt                             |
| POST   | `/Account/Register`   | Kayıt işlemi                      |
| GET    | `/Appointments`       | Kullanıcı randevu listesi         |
| GET    | `/Appointments/Create`| Randevu oluşturma formu           |
| POST   | `/Appointments/Create`| Randevu oluştur                    |
| GET    | `/Services`           | Hizmet listesi                    |
| GET    | `/Admin`              | Admin paneli (yetki gerekli)      |

> “**Randevu Al**” butonu: oturum yoksa giriş/kayıt modallarını açar; varsa `/Appointments/Create`’e yönlendirir.

---

## Güvenlik Notları
- ASP.NET Identity ile parola **hashing** ve **cookie** tabanlı oturum
- `[ValidateAntiForgeryToken]` ile **CSRF** koruması
- Giriş denemelerinde **lockout** politikaları
- Üretimde **HTTPS** zorunluluğu ve **HSTS**
- Sırlar için **User Secrets** / **Environment Variables**

---

## Sık Karşılaşılan Sorunlar
- **“Partial view bulunamadı” hatası**  
  ```
  The partial view '~/Views/Shared/_SomePartial.cshtml' was not found. The following locations were searched: ...
  ```
  **Çözüm:** Dosya yolu & adı doğru mu? `Views/Shared/` altına koyup **Build Action = Content**, **Copy to Output = Do not copy** olarak bırakın. `_ViewImports.cshtml` içinde namespace/TagHelper ayarlarını kontrol edin.

- **LibMan ile FontAwesome indirme sorunu (cdnjs)**  
  `cdnjs` sağlayıcısı zaman zaman paket bulamayabilir. **unpkg** ile deneyin:  
  ```bash
  libman install @fortawesome/fontawesome-free -p unpkg -d wwwroot/lib/fontawesome
  ```
  veya doğrudan CDN `<link>` kullanın.

- **EF Core sürüm/bağlantı sorunları**  
  `dotnet --info` ile SDK sürümünü doğrulayın; `dotnet ef` global aracının güncel olduğundan emin olun.

---

## Yol Haritası
- [x] Giriş & kayıt modalları (_Layout uyumlu)
- [ ] Ana sayfa hero & dinamik hizmetler
- [ ] Yorumlar (admin onay akışı)
- [ ] Randevu (takvim seçimi, çakışma kontrolü)
- [ ] Admin paneli (onay/red, istatistikler)
- [ ] E-posta ile randevu onayı
- [ ] Çoklu dil (tr-TR başlangıç)
- [ ] Radio-button + AJAX filtreleri
- [ ] Testler (unit/integration)

---

## Katkıda Bulunma
1. Repo’yu fork’layın
2. Branch açın: `git checkout -b feature/YeniOzellik`
3. Commit: `git commit -m "Yeni özellik: açıklama"`
4. Push: `git push origin feature/YeniOzellik`
5. Pull Request açın  
> Kod stili için `dotnet format` kullanın; PR açıklamalarına ekran görüntüsü eklemeniz tercih edilir.

---

## Lisans
Bu projeye henüz lisans eklenmedi.
