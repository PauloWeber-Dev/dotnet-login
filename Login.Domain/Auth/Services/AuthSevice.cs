using AutoMapper;

using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;


using DTO.Auth;
using Domain.Repository.Entities;
using Domain.Repository;
using OtpNet;
using Login.Domain.Helpers;
using Domain.Email;


namespace Domain.Auth.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;
    private readonly IEmailService _emailService;

    public AuthService(IUserRepository userRepository, IEmailService emailService, IConfiguration configuration, IMapper mapper)
    {
        _userRepository = userRepository;
        _configuration = configuration;
        _mapper = mapper;
        _emailService = emailService;
    }

    public async Task<bool> RegisterAsync(RegisterUserDto dto)
    {
        try
        {
            var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
            if (existingUser != null)
                throw new Exception("Email exists");


            var user = _mapper.Map<User>(dto);

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            user.EmailConfirmationCode = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

            user.EmailConfirmationCodeExpiry = DateTime.UtcNow.AddDays(2);

            user.UserRoles = new List<string> { "user" };

            user.Status = UserStatus.Registered;

            if (dto.ExternalLogin == true)
            {
                if (!string.IsNullOrEmpty(dto.GoogleId))
                {
                    user.GoogleId = dto.GoogleId;
                }
                else if (!string.IsNullOrEmpty(dto.FacebookId))
                {
                    user.FacebookId = dto.FacebookId;
                }
            }
            else
            {
                user.GoogleId = null;
                user.FacebookId = null;
            }

            await _userRepository.CreateAsync(user);

            string confirmationLink = $"{_configuration["App:BaseUrl"]}/confirm-email?email={Uri.EscapeDataString(user.Email)}&code={Uri.EscapeDataString(user.EmailConfirmationCode)}";

            string emailContent = EmailTemplateHelper.GetConfirmEmailTemplate(confirmationLink, user);

            return await _emailService.SendEmail(user, "Confirm your email", emailContent);

        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<string> LoginAsync(LoginDto dto)
    {
        var user = await _userRepository.GetByEmailAsync(dto.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new Exception("Invalid credentials");

        ValidateUserStatus(user);

        if(user.MfaSecretKey != null)
           return "MFA";

        return GenerateJwtToken(user, dto.RememberMe);
    }

    public async Task RequestPasswordResetAsync(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null)
            return;

        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        user.ResetToken = token;
        user.ResetTokenExpiry = DateTime.UtcNow.AddHours(1);
        user.Status = UserStatus.PasswordResetRequested;
        await _userRepository.UpdateAsync(user);
        await SendResetEmail(user);
    }

    public async Task ResetPasswordAsync(string token, string newPassword)
    {
        var user = await _userRepository.GetByResetTokenAsync(token);
        if (user == null || user.ResetTokenExpiry < DateTime.UtcNow)
            throw new Exception("Invalid or expired token");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.ResetToken = null;
        user.ResetTokenExpiry = null;
        user.Status = UserStatus.Active; // Reset status to active after password reset
        await _userRepository.UpdateAsync(user);

    }

    private string GenerateJwtToken(User user, bool rememberMe = false)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user), "User cannot be null");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (user.UserRoles != null && user.UserRoles.Count > 0)
        {
            foreach (var role in user.UserRoles)
            {
                claims = claims.Append(new Claim(ClaimTypes.Role, role)).ToArray();
            }
        }
        else
        {
            claims = claims.Append(new Claim(ClaimTypes.Role, "user")).ToArray();
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = rememberMe ? DateTime.UtcNow.AddYears(1) : DateTime.UtcNow.AddHours(1),
            Issuer = _configuration["JWT:Issuer"],
            Audience = _configuration["JWT:Audience"],
            SigningCredentials = creds
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    #region Email Confirmation
    private async Task SendResetEmail(User user)
    {
        string resetLink = $"{_configuration["App:BaseUrl"]}/reset-password?token={user.ResetToken}";
        string emailBody = EmailTemplateHelper.GetResetEmailTemplate(resetLink, user);
        await _emailService.SendEmail(user, "Password Reset Request", emailBody);

    }

    public async Task<bool> ConfirmEmailAsync(ConfirmEmailDto dto)
    {
        var user = await _userRepository.GetByEmailAsync(dto.Email);
        if (user == null || user.EmailConfirmationCode != dto.Code || user.EmailConfirmationCodeExpiry < DateTime.UtcNow)
            throw new Exception("Confirmation token invalid or expired");

        user.Status = UserStatus.Active;
        user.EmailConfirmationCode = null; // Clear the token after confirmation
        user.EmailConfirmationCodeExpiry = null; // Clear the expiry date
        await _userRepository.UpdateAsync(user);
        return true;
    }
    #endregion

    #region Facebook Login
    public async Task<string> FacebookLoginAsync(string facebookToken)
    {
        throw new NotImplementedException();
    }
    #endregion

    #region Google Login

    public async Task<string> GoogleLoginAsync(string googleToken)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region MFA
    public async Task<string> EnableMfaAsync(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null)
            return string.Empty;

        ValidateUserStatus(user);

        var secretKey = KeyGeneration.GenerateRandomKey(20);
        user.MfaSecretKey = Base32Encoding.ToString(secretKey);
        await _userRepository.UpdateAsync(user);

        // Gerar QR Code URL para o Google Authenticator
        var issuer = _configuration["Jwt:Issuer"];
        var qrCodeUrl = $"otpauth://totp/{issuer}:{user.Email}?secret={user.MfaSecretKey}&issuer={issuer}";
        return qrCodeUrl; // Retorna URL para o cliente gerar o QR Code
    }

    public async Task<string> VerifyMfaAsync(VerifyMfaDto dto)
    {
        var user = await _userRepository.GetByEmailAsync(dto.Email);
        if (user == null || user.MfaSecretKey == null)
            throw new Exception("MFA not enabled or user not found");

        var totp = new Totp(Base32Encoding.ToBytes(user.MfaSecretKey));
        if (!totp.VerifyTotp(dto.Code, out _, new VerificationWindow(2)))
            throw new Exception("Invalid MFA code");

        return GenerateJwtToken(user);
    }






    #endregion

    #region TOU and PP
    public async Task<string> GetLatestTOU() => await Task.Run(() => { return "Terms of Use content goes here. This should be fetched from a database or a file."; });


    public async Task<string> GetLatestPP() => await Task.Run(() => { return "Privacy Policy content goes here. This should be fetched from a database or a file."; });
    #endregion

    #region Admin Management

    // Sets the user as an admin
    //TODO: Implement proper role management and permissions
    //TODO: This should be set in a separate admin management service, but for simplicity, it's included here
    public async Task<bool> SetAdmin(int userId)
    {
        var user = _userRepository.GetByIdAsync(userId).Result;
        if (user == null)
            throw new Exception("User not found");
        ValidateUserStatus(user);
        if (user.UserRoles == null) user.UserRoles = new List<string>();
        user.UserRoles.Add("admin");
        user.Status = UserStatus.Active; // Ensure user is active
        return await _userRepository.UpdateAsync(user).ContinueWith(t => t.IsCompletedSuccessfully);
    }


    public async Task<bool> SetStatus(int userId, UserStatus status)
    {
        var user = _userRepository.GetByIdAsync(userId).Result;
        if (user == null)
            throw new Exception("User not found");
        user.Status = status;
        return await _userRepository.UpdateAsync(user).ContinueWith(t => t.IsCompletedSuccessfully);
    }
    #endregion

    private void ValidateUserStatus(User user)
    {
        switch (user.Status)
        {
            case UserStatus.Registered:
                throw new Exception("Email not confirmed");
            case UserStatus.PasswordResetRequested:
                throw new Exception("Password reset requested, please reset your password");
            case UserStatus.Inactive:
                throw new Exception("User is inactive, please contact support");
            case UserStatus.Deleted:
                throw new Exception("User is deleted, please contact support");
            default: break;
        }
    }

}

