using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using FileManagementAPI.Data;
using FileManagementAPI.Models;
using FileManagementAPI.Services;
using FileManagementAPI.DTOs;

var builder = WebApplication.CreateBuilder(args);

// Configuration
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"] ?? "YourVeryLongSecretKeyThatIsAtLeast32CharactersLong123456";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "FileManagementAPI";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "FileManagementClient";

// Services
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("FileManagementDB")); // Geliştirme için InMemory DB

builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IFileService, FileService>();

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "File Management API", Version = "v1" });
    
    // JWT Authentication için Swagger konfigürasyonu
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll"); 
app.UseAuthentication();
app.UseAuthorization();

// Auth Endpoints
app.MapPost("/api/auth/register", async (RegisterRequest request, AppDbContext db) =>
{
    try
    {
        // Validation
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Results.Json(new ApiResponse<object>(false, "Tüm alanlar doldurulmalıdır."), statusCode: 400);
        }

        // Kullanıcı var mı kontrol et
        if (await db.Users.AnyAsync(u => u.Username == request.Username || u.Email == request.Email))
        {
            return Results.Json(new ApiResponse<object>(false, "Kullanıcı adı veya email zaten kullanımda."), statusCode: 400);
        }

        // Şifre hash'le
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        // Yeni kullanıcı oluştur
        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = passwordHash
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        return Results.Json(new ApiResponse<object>(true, "Kullanıcı başarıyla oluşturuldu."), statusCode: 201);
    }
    catch (Exception ex)
    {
        return Results.Json(new ApiResponse<object>(false, $"Bir hata oluştu: {ex.Message}"), statusCode: 500);
    }
}).WithTags("Auth").WithOpenApi();

app.MapPost("/api/auth/login", async (LoginRequest request, AppDbContext db, IJwtService jwtService) =>
{
    try
    {
        // Validation
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Results.Json(new ApiResponse<object>(false, "Kullanıcı adı ve şifre gereklidir."), statusCode: 400);
        }

        // Kullanıcıyı bul
        var user = await db.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
        
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Results.Json(new ApiResponse<object>(false, "Geçersiz kullanıcı adı veya şifre."), statusCode: 401);
        }

        // JWT token oluştur
        var token = jwtService.GenerateToken(user);

        var response = new LoginResponse(token, user.Username, user.Email);
        return Results.Json(new ApiResponse<LoginResponse>(true, "Giriş başarılı.", response), statusCode: 200);
    }
    catch (Exception ex)
    {
        return Results.Json(new ApiResponse<object>(false, $"Bir hata oluştu: {ex.Message}"), statusCode: 500);
    }
}).WithTags("Auth").WithOpenApi();

// File Endpoints
app.MapPost("/api/files/upload", async (HttpContext context, AppDbContext db, IFileService fileService) =>
{
    try
    {
        // Kullanıcı ID'sini al
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
        {
            return Results.Json(new ApiResponse<object>(false, "Yetkisiz erişim."), statusCode: 401);
        }

        var form = await context.Request.ReadFormAsync();
        var file = form.Files.FirstOrDefault();

        if (file == null || file.Length == 0)
        {
            return Results.Json(new ApiResponse<object>(false, "Dosya seçilmedi."), statusCode: 400);
        }

        // Dosya tipini kontrol et
        if (!fileService.IsValidFileType(file.ContentType))
        {
            return Results.Json(new ApiResponse<object>(false, "Geçersiz dosya tipi. Sadece PDF, PNG ve JPG dosyaları kabul edilir."), statusCode: 400);
        }

        // Dosya boyutunu kontrol et (5MB max)
        if (file.Length > 5 * 1024 * 1024)
        {
            return Results.Json(new ApiResponse<object>(false, "Dosya boyutu 5MB'dan büyük olamaz."), statusCode: 400);
        }

        // Dosyayı kaydet
        var filePath = await fileService.SaveFileAsync(file, userId);
        var uniqueFileName = Path.GetFileName(filePath);

        // Veritabanına kaydet
        var uploadedFile = new UploadedFile
        {
            FileName = uniqueFileName,
            OriginalFileName = file.FileName,
            ContentType = file.ContentType,
            FileSize = file.Length,
            FilePath = filePath,
            UserId = userId
        };

        db.UploadedFiles.Add(uploadedFile);
        await db.SaveChangesAsync();

        var response = new FileUploadResponse(
            uploadedFile.Id,
            uploadedFile.FileName,
            uploadedFile.OriginalFileName,
            uploadedFile.ContentType,
            uploadedFile.FileSize,
            uploadedFile.UploadedAt
        );

        return Results.Json(new ApiResponse<FileUploadResponse>(true, "Dosya başarıyla yüklendi.", response), statusCode: 200);
    }
    catch (Exception ex)
    {
        return Results.Json(new ApiResponse<object>(false, $"Bir hata oluştu: {ex.Message}"), statusCode: 500);
    }
}).RequireAuthorization().WithTags("Files").WithOpenApi();

app.MapGet("/api/files", async (HttpContext context, AppDbContext db) =>
{
    try
    {
        // Kullanıcı ID'sini al
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
        {
            return Results.Json(new ApiResponse<object>(false, "Yetkisiz erişim."), statusCode: 401);
        }

        var files = await db.UploadedFiles
            .Where(f => f.UserId == userId)
            .Select(f => new FileListResponse(
                f.Id,
                f.FileName,
                f.OriginalFileName,
                f.ContentType,
                f.FileSize,
                f.UploadedAt
            ))
            .OrderByDescending(f => f.UploadedAt)
            .ToListAsync();

        return Results.Json(new ApiResponse<List<FileListResponse>>(true, "Dosyalar başarıyla listelendi.", files), statusCode: 200);
    }
    catch (Exception ex)
    {
        return Results.Json(new ApiResponse<object>(false, $"Bir hata oluştu: {ex.Message}"), statusCode: 500);
    }
}).RequireAuthorization().WithTags("Files").WithOpenApi();

app.MapDelete("/api/files/{id:int}", async (int id, HttpContext context, AppDbContext db, IFileService fileService) =>
{
    try
    {
        // Kullanıcı ID'sini al
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
        {
            return Results.Json(new ApiResponse<object>(false, "Yetkisiz erişim."), statusCode: 401);
        }

        // Dosyayı bul
        var file = await db.UploadedFiles.FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId);
        if (file == null)
        {
            return Results.Json(new ApiResponse<object>(false, "Dosya bulunamadı."), statusCode: 404);
        }

        // Fiziksel dosyayı sil
        await fileService.DeleteFileAsync(file.FilePath);

        // Veritabanından sil
        db.UploadedFiles.Remove(file);
        await db.SaveChangesAsync();

        return Results.Json(new ApiResponse<object>(true, "Dosya başarıyla silindi."), statusCode: 200);
    }
    catch (Exception ex)
    {
        return Results.Json(new ApiResponse<object>(false, $"Bir hata oluştu: {ex.Message}"), statusCode: 500);
    }
}).RequireAuthorization().WithTags("Files").WithOpenApi();

app.Run();