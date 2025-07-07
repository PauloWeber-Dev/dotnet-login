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

    public async Task<string> RegisterAsync(RegisterUserDto dto)
    {
        try
        {
            var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
            if (existingUser != null)
                throw new Exception("Email existe");


            var user = _mapper.Map<User>(dto);

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            user.EmailConfirmationCode = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

            await _userRepository.CreateAsync(user);

            string confirmationLink = $"{_configuration["App:BaseUrl"]}/confirm-email?email={Uri.EscapeDataString(user.Email)}&code={Uri.EscapeDataString(user.EmailConfirmationCode)}";

            string emailContent = EmailTemplateHelper.GetConfirmEmailTemplate(confirmationLink, user);

            _emailService.SendEmail(user, "Confirme seu email", emailContent);

            return "Operação realizada com sucesso. Um email de confirmação foi enviado para o seu endereço de email.";

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

        return await GenerateJwtToken(user);
    }

    public async Task RequestPasswordResetAsync(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null)
            return; 

        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        user.ResetToken = token;
        user.ResetTokenExpiry = DateTime.UtcNow.AddHours(1);
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

    #region Email Confirmation
    private async Task SendResetEmail(User user)
    {
        string resetLink = $"{_configuration["App:BaseUrl"]}/reset-password?token={user.ResetToken}";
        string emailBody = EmailTemplateHelper.GetResetEmailTemplate(resetLink, user);
        _emailService.SendEmail(user, "Password Reset Request", emailBody);

    }

    public async Task <bool> ConfirmEmailAsync(ConfirmEmailDto dto)
    {
        var user = await _userRepository.GetByEmailAsync(dto.Email);
        if (user == null || user.EmailConfirmationCode != dto.Code || user.EmailConfirmationCodeExpiry < DateTime.UtcNow)
            throw new Exception("Confirmation token invalid or expired");

        user.IsEmailConfirmed = true;
        user.EmailConfirmationCode = null; // Clear the token after confirmation
        await _userRepository.UpdateAsync(user);
        return user.IsEmailConfirmed;
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
        // Validate the Google token
        var payload = await GoogleJsonWebSignature.ValidateAsync(googleToken);
        if (payload == null)
            throw new Exception("Invalid Google token");
        // Check if user already exists
        var user = await _userRepository.GetByGoogleIdAsync(payload.Subject);
        if (user == null)
        {
            // Create a new user if not found
            user = new User
            {
                FirstName = payload.GivenName,
                LastName = payload.FamilyName,
                Email = payload.Email,
                GoogleId = payload.Subject
            };
            await _userRepository.CreateAsync(user);
        }
        return await GenerateJwtToken(user);
    }

    #endregion

    #region MFA
    public async Task<string> EnableMfaAsync(EnableMfaDto dto)
    {
        var user = await _userRepository.GetByIdAsync(dto.UserId);
        if (user == null)
            throw new Exception("User not found");
        if (!user.IsEmailConfirmed)
            throw new Exception("Email not confirmed");

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

        return await GenerateJwtToken(user);
    }

    public async Task<string> GetLatestTOU() => await Task.Run(() => { return "Terms of Use content goes here. This should be fetched from a database or a file."; });


    public async Task<string> GetLatestPP() => await Task.Run(() => {return "Privacy Policy content goes here. This should be fetched from a database or a file."; });
      


    #endregion
}

