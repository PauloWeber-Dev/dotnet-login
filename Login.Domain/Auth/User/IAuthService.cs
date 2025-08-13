using Domain.Auth.Services;
using DTO.Auth;


namespace Domain.Auth;

public interface IAuthService
{
    Task<bool> RegisterAsync(RegisterUserDto dto);
    Task<string> LoginAsync(LoginDto dto);
    Task<string> GoogleLoginAsync(string googleToken);
    Task<string> FacebookLoginAsync(string googleToken);
    Task RequestPasswordResetAsync(string email);
    Task ResetPasswordAsync(string token, string newPassword);
    Task <bool> ConfirmEmailAsync(ConfirmEmailDto dto);
    Task<string> EnableMfaAsync(string email);
    Task<string> VerifyMfaAsync(VerifyMfaDto dto);
    Task<bool> SetAdmin(int userId);
    Task<bool> SetStatus(int userId, UserStatus status);
    Task<string> GetLatestTOU();
    Task<string> GetLatestPP();
}
