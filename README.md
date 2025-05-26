# File Management System

Modern ve güvenli bir dosya yönetim sistemi oluşturdum. Kullanıcılar kayıt olup, giriş yaparak dosyalarını yükleyebilir, görüntüleyebilir ve silebilirler.

## 🚀 Özellikler

### Güvenlik
- **JWT Authentication**: Token tabanlı güvenli kimlik doğrulama
- **BCrypt Şifreleme**: Kullanıcı şifrelerinin güvenli hash'lenmesi
- **Yetkilendirme**: Kullanıcılar sadece kendi dosyalarına erişebilir

### Dosya Yönetimi
- **Desteklenen Formatlar**: PDF, PNG, JPG/JPEG
- **Dosya Boyutu Sınırı**: Maksimum 5MB
- **Güvenli Yükleme**: Dosya tipi ve boyut validasyonu
- **Fiziksel Dosya Yönetimi**: Sunucuda güvenli dosya saklama

### Kullanıcı Deneyimi
- **Modern UI**: Responsive ve kullanıcı dostu arayüz
- **Real-time Feedback**: Anlık başarı/hata mesajları
- **Kolay Navigasyon**: Sezgisel kullanıcı arayüzü

## 🏗️ Sistem Mimarisi

### Backend (ASP.NET Core Minimal API)
- **Framework**: .NET 8.0+ 
- **Database**: Entity Framework Core (In-Memory)
- **Authentication**: JWT Bearer Token
- **API Documentation**: Swagger/OpenAPI

### Frontend (React + Vite)
- **Framework**: React 18+
- **Build Tool**: Vite (Fast development server)
- **State Management**: React Hooks (useState, useEffect)
- **HTTP Client**: Fetch API
- **Styling**: Inline CSS (Modern gradient design)

## 🔧 Kurulum ve Çalıştırma

### Gereksinimler
- .NET 8.0+ SDK
- Node.js 18+ (sadece frontend için)
- npm veya yarn
- Modern web browser

### Backend

1. **Proje klasörüne gidin**
```bash
cd FileManagementAPI
```

2. **Uygulamayı çalıştırın**
```bash
dotnet run
```

Backend varsayılan olarak `http://localhost:5167` adresinde çalışır.

### Frontend Kurulumu

1. **Proje klasörüne gidin**
```bash
cd FileManagementAPI/frontend
```

4. **Uygulamayı çalıştırın**
```bash
npm run dev
```

Frontend varsayılan olarak `http://localhost:5173` adresinde çalışır (Vite default port).

## 🔗 API Endpoints

### Authentication
- `POST /api/auth/register` - Kullanıcı kaydı
- `POST /api/auth/login` - Kullanıcı girişi

### File Management (Yetkilendirme gerekli)
- `POST /api/files/upload` - Dosya yükleme
- `GET /api/files` - Kullanıcının dosyalarını listeleme
- `DELETE /api/files/{id}` - Dosya silme

## 🔐 JWT Konfigürasyonu

`appsettings.json` dosyasında JWT ayarlarını yapılandırabilirsiniz:

```json
{
  "Jwt": {
    "SecretKey": "YourVeryLongSecretKeyThatIsAtLeast32CharactersLong123456",
    "Issuer": "FileManagementAPI",
    "Audience": "FileManagementClient"
  }
}
```

## 📊 Veritabanı Modelleri

### User (Kullanıcı)
```csharp
public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### UploadedFile (Yüklenen Dosya)
```csharp
public class UploadedFile
{
    public int Id { get; set; }
    public string FileName { get; set; }
    public string OriginalFileName { get; set; }
    public string ContentType { get; set; }
    public long FileSize { get; set; }
    public string FilePath { get; set; }
    public int UserId { get; set; }
    public DateTime UploadedAt { get; set; }
}
```

## 🛡️ Güvenlik Özellikleri

- **Password Hashing**: BCrypt ile güvenli şifre saklama
- **JWT Token**: Stateless authentication
- **File Validation**: Dosya tipi ve boyut kontrolü
- **User Isolation**: Kullanıcılar sadece kendi dosyalarını görebilir
- **CORS Policy**: Cross-origin istekleri için yapılandırılmış

## 🎨 UI/UX Özellikleri

- **Modern Gradient Design**: Çekici görsel tasarım
- **Responsive Layout**: Tüm cihazlarda uyumlu
- **Real-time Feedback**: Anlık kullanıcı geri bildirimleri
- **Intuitive Navigation**: Kolay kullanım
- **File Type Icons**: Dosya türlerine göre simgeler

## 📝 Kullanım Adımları

1. **Kayıt Ol**: Kullanıcı adı, e-posta ve şifre ile hesap oluştur
2. **Giriş Yap**: Kullanıcı adı ve şifre ile sisteme gir
3. **Dosya Yükle**: PDF, PNG veya JPG dosyası seç ve yükle
4. **Dosyaları Görüntüle**: Yüklediğin dosyaları listele
5. **Dosya Sil**: İstemediğin dosyaları sil

## 🔄 Geliştirme Notları

- **In-Memory Database**: Geliştirme için kullanılıyor, production'da SQL Server/PostgreSQL/MySQL kullanın
- **File Storage**: Şu anda yerel dosya sistemi, cloud storage entegrasyonu eklenebilir
- **Error Handling**: Comprehensive hata yönetimi implementasyonu
- **Logging**: Production için detaylı logging eklenebilir

## 🚀 Production Hazırlığı

Production ortamına geçiş için:

1. **Veritabanı**: SQL Server/PostgreSQL/MySQL kullanın
2. **File Storage**: AWS S3, Azure Blob Storage gibi cloud çözümler
3. **Environment Variables**: Sensitive bilgileri environment variables'da saklayın
4. **HTTPS**: SSL sertifikası ekleyin
5. **Logging**: Structured logging implementasyonu
6. **Health Checks**: Sistem sağlığı kontrolü

## 📞 Destek

Herhangi bir sorun yaşarsanız veya geliştirme önerileriniz varsa, proje repository'sinde issue açabilirsiniz.

---

**Not**: Bu sistem eğitim ve demo amaçlıdır. Production kullanımı için ek güvenlik ve performans optimizasyonları gerekebilir.
