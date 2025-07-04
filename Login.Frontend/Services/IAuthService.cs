using DTO.Auth;


namespace Login.Frontend.Services
{
    public interface IAuthService
    {
        Task<string> RegisterAsync(RegisterUserDto dto);
        Task<string> LoginAsync(LoginDto dto);
        Task<string> GoogleLoginAsync(string googleToken);
        Task RequestPasswordResetAsync(string email);
        Task ResetPasswordAsync(string token, string newPassword);
        Task ConfirmEmailAsync(ConfirmEmailDto dto);
        Task<string> EnableMfaAsync(int userId);
        Task<string> VerifyMfaAsync(VerifyMfaDto dto);
    }
}