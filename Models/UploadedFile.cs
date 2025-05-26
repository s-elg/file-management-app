namespace FileManagementAPI.Models;

public class UploadedFile
{
    public int Id { get; set; }
    public required string FileName { get; set; }
    public required string OriginalFileName { get; set; }
    public required string ContentType { get; set; }
    public long FileSize { get; set; }
    public required string FilePath { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    
    // Foreign key
    public int UserId { get; set; }
    public User User { get; set; } = null!;
}