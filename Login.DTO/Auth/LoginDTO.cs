namespace DTO.Auth;

public record LoginDto(string Email, string Password, bool RememberMe = false);
