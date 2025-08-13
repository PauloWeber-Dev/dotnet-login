using Domain.Repository;
using Domain.Repository.Entities;
using Login.Repository;
using Domain.Auth.Services;


namespace Repository.Auth;
public class UserRepositoryEF : IUserRepository
{
    private readonly LoginDbContext _context;

    public UserRepositoryEF(LoginDbContext context)
    {
        _context = context;
    }
    public async Task<int> CreateAsync(User user)
    {
        _context.Users.Add(user);
        _context.SaveChanges();
        return await Task.FromResult(user.Id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await Task.FromResult(_context.Users.FirstOrDefault(u => u.Email == email));
    }

    public async Task<User?> GetByEmailConfirmationCodeAsync(string code)
    {
        return await Task.FromResult(_context.Users.FirstOrDefault(u => u.EmailConfirmationCode == code));
    }

    public async Task<User?> GetByFacebookIdAsync(string facebookId)
    {
        return await Task.FromResult(_context.Users.FirstOrDefault(u => u.FacebookId == facebookId));
    }

    public async Task<User?> GetByGoogleIdAsync(string googleId)
    {
        return await Task.FromResult(_context.Users.FirstOrDefault(u => u.GoogleId == googleId));
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await Task.FromResult(_context.Users.FirstOrDefault(u => u.Id == id));
    }

    public async Task<User?> GetByResetTokenAsync(string token)
    {
        return await Task.FromResult(_context.Users.FirstOrDefault(u => u.ResetToken == token));
    }

    public Task<bool> IsValidLogin(string email, string password)
    {
        return Task.FromResult(_context.Users.Any(u => u.Email == email && u.PasswordHash==password));
    }

    public async Task UpdateAsync(User user)
    {
        var existingUser = _context.Users.Find(user.Id);
        if (existingUser != null)
        {
            existingUser.FirstName = user.FirstName;
            existingUser.LastName = user.LastName;
            existingUser.BirthDate = user.BirthDate;
        }
        await Task.FromResult(_context.SaveChanges());
    }

    public async Task UpdateEmailConfirmationCodeAsync(User user)
    {
        var existingUser = _context.Users.Find(user.Id);
        if (existingUser != null)
        {
            existingUser.EmailConfirmationCode = user.EmailConfirmationCode;
            existingUser.EmailConfirmationCodeExpiry = user.EmailConfirmationCodeExpiry;
            existingUser.Status = UserStatus.Active;
        }
        await Task.FromResult(_context.SaveChanges());

    }
}
