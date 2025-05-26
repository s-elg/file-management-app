namespace FileManagementAPI.DTOs;

public record FileUploadResponse(
    int Id,
    string FileName,
    string OriginalFileName,
    string ContentType,
    long FileSize,
    DateTime UploadedAt
);

public record FileListResponse(
    int Id,
    string FileName,
    string OriginalFileName,
    string ContentType,
    long FileSize,
    DateTime UploadedAt
);