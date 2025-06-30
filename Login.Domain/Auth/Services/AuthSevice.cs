using Domain.Auth.DTO;
using Domain.Auth.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using Google.Apis.Auth;
using System.Threading.Tasks;

namespace Domain.Auth.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;

    public AuthService(IUserRepository userRepository, IConfiguration configuration, IMapper mapper)
    {
        _userRepository = userRepository;
        _configuration = configuration;
        _mapper = mapper;
    }

    public async Task<string> RegisterAsync(RegisterUserDto dto)
    {
        var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
        if (existingUser != null)
            throw new Exception("Email already exists");


        var user = _mapper.Map<User>(dto);

        await _userRepository.CreateAsync(user);
        return await GenerateJwtToken(user);
    }

    public async Task<string> LoginAsync(LoginDto dto)
    {
        var user = await _userRepository.GetByEmailAsync(dto.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new Exception("Invalid credentials");

        return await GenerateJwtToken(user);
    }

    public async Task<string> GoogleLoginAsync(string googleToken)
    {
        // Validate Google token (simplified, use Google.Apis.Auth in production)
        var userInfo = await ValidateGoogleToken(googleToken);
        var user = await _userRepository.GetByGoogleIdAsync(userInfo.Sub);

        if (user == null)
        {
            user = new User
            {
                Email = userInfo.Email,
                FirstName = userInfo.GivenName,
                LastName = userInfo.FamilyName,
                GoogleId = userInfo.Sub
            };
            await _userRepository.CreateAsync(user);
        }

        return await GenerateJwtToken(user);
    }

    public async Task RequestPasswordResetAsync(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null)
            return; // Silently fail to avoid email enumeration

        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        user.ResetToken = token;
        user.ResetTokenExpiry = DateTime.UtcNow.AddHours(1);
        await _userRepository.UpdateAsync(user);

        await SendResetEmail(user.Email, token);
    }

    public async Task ResetPasswordAsync(string token, string newPassword)
    {
        var user = await _userRepository.GetByResetTokenAsync(token);
        if (user == null || user.ResetTokenExpiry < DateTime.UtcNow)
            throw new Exception("Invalid or expired token");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.ResetToken = null;
        user.ResetTokenExpiry = null;
        await _userRepository.UpdateAsync(user);
    }

    private async Task<string> GenerateJwtToken(User user) => await Task.Run(() =>
                                                                   {
                                                                       if (user == null)
                                                                           throw new ArgumentNullException(nameof(user), "User cannot be null");

                                                                       var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                                                                       var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                                                                       var claims = new[]
                                                                       {
                                                                            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                                                                            new Claim(JwtRegisteredClaimNames.Email, user.Email),
                                                                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                                                                        };

                                                                       var token = new JwtSecurityToken(
                                                                           issuer: _configuration["Jwt:Issuer"],
                                                                           audience: _configuration["Jwt:Audience"],
                                                                           claims: claims,
                                                                           expires: DateTime.Now.AddHours(1),
                                                                           signingCredentials: creds);

                                                                       return new JwtSecurityTokenHandler().WriteToken(token);
                                                                   });

    private async Task SendResetEmail(string email, string token)
    {
        throw new NotImplementedException();
    }

    private async Task<GoogleUserInfo> ValidateGoogleToken(string token)
    {
        throw new NotImplementedException();
    }

    public async Task<string> FacebookLoginAsync(string token)
    {
        throw new NotImplementedException();
    }

    private record GoogleUserInfo
    {
        public string Sub { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string GivenName { get; set; } = string.Empty;
        public string FamilyName { get; set; } = string.Empty;
    }
}
