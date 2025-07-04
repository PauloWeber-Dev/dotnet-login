using DTO.Auth;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text.Json;

namespace Login.Frontend.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public AuthService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _baseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:8080";
            // Ignorar validação de certificado SSL para desenvolvimento (NÃO USAR EM PRODUÇÃO)
#if DEBUG
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            _httpClient = new HttpClient(handler);
#endif
        }

        public async Task<string> RegisterAsync(RegisterUserDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/register", dto);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            return result.GetProperty("message").GetString()!;
        }

        public async Task<string> LoginAsync(LoginDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/login", dto);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            return result.GetProperty("token").GetString()!;
        }

        public async Task<string> GoogleLoginAsync(string googleToken)
        {
            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/google-login", new { googleToken });
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            return result.GetProperty("token").GetString()!;
        }

        public async Task RequestPasswordResetAsync(string email)
        {
            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/forgot-password", new { email });
            response.EnsureSuccessStatusCode();
        }

        public async Task ResetPasswordAsync(string token, string newPassword)
        {
            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/reset-password", new { token, newPassword });
            response.EnsureSuccessStatusCode();
        }

        public async Task ConfirmEmailAsync(ConfirmEmailDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/confirm-email", dto);
            response.EnsureSuccessStatusCode();
        }

        public async Task<string> EnableMfaAsync(int userId)
        {
            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/enable-mfa", new { userId });
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            return result.GetProperty("qrCodeUrl").GetString()!;
        }

        public async Task<string> VerifyMfaAsync(VerifyMfaDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/verify-mfa", dto);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            return result.GetProperty("token").GetString()!;
        }
    }
}
