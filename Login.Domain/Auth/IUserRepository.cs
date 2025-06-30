using Domain.Auth.Entities;


namespace Domain.Auth;
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByGoogleIdAsync(string googleId);
        Task<User?> GetByResetTokenAsync(string token);
        Task<int> CreateAsync(User user);
        Task UpdateAsync(User user);
    }
