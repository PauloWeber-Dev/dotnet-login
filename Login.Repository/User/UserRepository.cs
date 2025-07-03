using Dapper;
using Domain.Repository.Entities;
using Domain.Repository;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;


namespace Repository.Auth;
public class UserRepository : IUserRepository
{
    private readonly string _connectionString;

    public UserRepository(IConfiguration configuration)
    {
        _connectionString = configuration["Database:ConnectionString"]!;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryFirstOrDefaultAsync<User>(
            "SELECT * FROM Users WHERE Email = @Email", new { Email = email });
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryFirstOrDefaultAsync<User>(
            "SELECT * FROM Users WHERE Id = @Id", new { Id = id });
    }

    public async Task<User?> GetByGoogleIdAsync(string googleId)
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryFirstOrDefaultAsync<User>(
            "SELECT * FROM Users WHERE GoogleId = @GoogleId", new { GoogleId = googleId });
    }

    public async Task<User?> GetByResetTokenAsync(string token)
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryFirstOrDefaultAsync<User>(
            "SELECT * FROM Users WHERE ResetToken = @Token", new { Token = token });
    }

    public async Task<int> CreateAsync(User user)
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.ExecuteScalarAsync<int>(
            @"INSERT INTO Users (FirstName, LastName, BirthDate, Gender, Email, PasswordHash, GoogleId)
              VALUES (@FirstName, @LastName, @BirthDate, @Gender, @Email, @PasswordHash, @GoogleId);
              SELECT SCOPE_IDENTITY();", user);
    }

    public async Task UpdateAsync(User user)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync(
            @"UPDATE Users SET FirstName = @FirstName, LastName = @LastName, BirthDate = @BirthDate,
              Gender = @Gender, Email = @Email, PasswordHash = @PasswordHash, GoogleId = @GoogleId,
              ResetToken = @ResetToken, ResetTokenExpiry = @ResetTokenExpiry
              WHERE Id = @Id", user);
    }

    public Task<User?> GetByFacebookIdAsync(string googleId)
    {
        throw new NotImplementedException();
    }

    public Task<User?> GetByEmailConfirmationCodeAsync(string code)
    {
        throw new NotImplementedException();
    }
}
