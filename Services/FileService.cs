namespace FileManagementAPI.Services;

public interface IFileService
{
    Task<string> SaveFileAsync(IFormFile file, int userId);
    Task<bool> DeleteFileAsync(string filePath);
    bool IsValidFileType(string contentType);
    string GenerateUniqueFileName(string originalFileName);
}

public class FileService : IFileService
{
    private readonly IWebHostEnvironment _environment;
    private readonly string _uploadsPath;
    private readonly string[] _allowedContentTypes = {
        "application/pdf",
        "image/png",
        "image/jpeg",
        "image/jpg"
    };

    public FileService(IWebHostEnvironment environment)
    {
        _environment = environment;
        _uploadsPath = Path.Combine(_environment.ContentRootPath, "Uploads");
        
        // Uploads klasörü yoksa oluştur
        if (!Directory.Exists(_uploadsPath))
        {
            Directory.CreateDirectory(_uploadsPath);
        }
    }

    public async Task<string> SaveFileAsync(IFormFile file, int userId)
    {
        // Kullanıcı klasörü oluştur
        var userFolderPath = Path.Combine(_uploadsPath, userId.ToString());
        if (!Directory.Exists(userFolderPath))
        {
            Directory.CreateDirectory(userFolderPath);
        }

        // Benzersiz dosya adı oluştur
        var uniqueFileName = GenerateUniqueFileName(file.FileName);
        var filePath = Path.Combine(userFolderPath, uniqueFileName);

        // Dosyayı kaydet
        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return filePath;
    }

    public async Task<bool> DeleteFileAsync(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                await Task.Run(() => File.Delete(filePath));
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    public bool IsValidFileType(string contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            return false;
            
        return _allowedContentTypes.Contains(contentType.ToLowerInvariant());
    }

    public string GenerateUniqueFileName(string originalFileName)
    {
        var extension = Path.GetExtension(originalFileName);
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var guid = Guid.NewGuid().ToString("N")[..8];
        
        return $"{fileNameWithoutExtension}_{timestamp}_{guid}{extension}";
    }
}