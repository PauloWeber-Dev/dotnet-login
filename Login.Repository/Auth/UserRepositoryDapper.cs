using Dapper;
using Domain.Repository;
using Domain.Repository.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;


namespace Repository.Auth;
public class UserRepositoryDapper : IUserRepository
{
    private readonly string _connectionString;

    public UserRepositoryDapper(IConfiguration configuration)
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
            @"INSERT INTO Users (FirstName, LastName, BirthDate, Gender, Email, PasswordHash, GoogleId, EmailConfirmationCode, EmailConfirmationCodeExpiry, Roles)
              VALUES (@FirstName, @LastName, @BirthDate, @Gender, @Email, @PasswordHash, @GoogleId, @EmailConfirmationCode, @EmailConfirmationCodeExpiry, @Roles);
              SELECT SCOPE_IDENTITY();", user);
    }

    public async Task UpdateAsync(User user)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync(
            @"UPDATE Users SET FirstName = @FirstName, LastName = @LastName, BirthDate = @BirthDate,
              Gender = @Gender, Email = @Email, PasswordHash = @PasswordHash, GoogleId = @GoogleId,
              ResetToken = @ResetToken, ResetTokenExpiry = @ResetTokenExpiry,  EmailConfirmationCode = @EmailConfirmationCode,
              EmailConfirmationCodeExpiry = @EmailConfirmationCodeExpiry, Status = @Status, Roles = @Roles, MfaSecretKey = @MfaSecretKey
              WHERE Id = @Id", user);
    }

    public async Task<User?> GetByFacebookIdAsync(string facebookId)
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryFirstOrDefaultAsync<User>(
            "SELECT * FROM Users WHERE FacebookId = @FacebookId", new { FacebookId = facebookId });
    }

    public async Task<User?> GetByEmailConfirmationCodeAsync(string code)
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryFirstOrDefaultAsync<User>(
            "SELECT * FROM Users WHERE EmailConfirmationCode = @Code", new { Code = code });
    }

    public async Task<bool> IsValidLogin(string email, string password)
    {
        using var connection = new SqlConnection(_connectionString);
        var user = await connection.QueryFirstOrDefaultAsync<User>(
            "SELECT * FROM Users WHERE Email = @Email and PasswordHash=@Password", new { Email = email, Password=password });
        return user != null;
    }
}
