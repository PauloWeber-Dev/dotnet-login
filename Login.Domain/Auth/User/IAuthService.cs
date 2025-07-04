using DTO.Auth;


namespace Domain.Auth;

public interface IAuthService
{
    Task<string> RegisterAsync(RegisterUserDto dto);
    Task<string> LoginAsync(LoginDto dto);
    Task<string> GoogleLoginAsync(string googleToken);
    Task<string> FacebookLoginAsync(string googleToken);
    Task RequestPasswordResetAsync(string email);
    Task ResetPasswordAsync(string token, string newPassword);
    Task ConfirmEmailAsync(ConfirmEmailDto dto);
    Task<string> EnableMfaAsync(EnableMfaDto dto);
    Task<string> VerifyMfaAsync(VerifyMfaDto dto);
}
