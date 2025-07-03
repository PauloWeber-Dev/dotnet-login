using System;
namespace Domain.Repository.Entities;

public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? GoogleId { get; set; }
    public string? ResetToken { get; set; }
    public DateTime? ResetTokenExpiry { get; set; }
    public string? MfaSecretKey { get; set; } // Chave secreta para MFA
    public bool IsEmailConfirmed { get; set; } // Status de confirmação do email
    public string? EmailConfirmationCode { get; set; } // Código de confirmação
    public DateTime? EmailConfirmationCodeExpiry { get; set; } // Expiração do código
}




