namespace FileManagementAPI.DTOs;

public record LoginRequest(string Username, string Password);

public record RegisterRequest(string Username, string Email, string Password);

public record LoginResponse(string Token, string Username, string Email);

public record ApiResponse<T>(bool Success, string Message, T? Data = default);