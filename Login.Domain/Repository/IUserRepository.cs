using Domain.Repository.Entities;

namespace Domain.Repository;
public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByGoogleIdAsync(string googleId);
    Task<User?> GetByFacebookIdAsync(string googleId);
    Task<User?> GetByResetTokenAsync(string token);
    Task<User?> GetByEmailConfirmationCodeAsync(string code);
    Task<int> CreateAsync(User user);
    Task<bool> IsValidLogin(string email, string password);
    Task UpdateAsync(User user);
}
