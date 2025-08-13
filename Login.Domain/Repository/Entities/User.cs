using Domain.Auth.Services;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Domain.Repository.Entities;
public class User
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    public DateTime BirthDate { get; set; }

    [StringLength(20)]
    public string Gender { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(256)]
    public string PasswordHash { get; set; } = string.Empty;

    public string? GoogleId { get; set; }

    public string? FacebookId { get; set; }

    public string? ResetToken { get; set; }

    public DateTime? ResetTokenExpiry { get; set; }

    public string? MfaSecretKey { get; set; }

    public UserStatus Status { get; set; }

    public string? EmailConfirmationCode { get; set; }

    public DateTime? EmailConfirmationCodeExpiry { get; set; }

    public string? Roles { get; set; }
    [NotMapped]
    public List<string> UserRoles {         
        get => string.IsNullOrEmpty(Roles) ? new List<string>() : Roles.ToLowerInvariant().Split(',').ToList();
        set => Roles = string.Join(',', value).ToLowerInvariant();
    }
}




