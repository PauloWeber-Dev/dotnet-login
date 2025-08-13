using AutoMapper;
using Domain.Auth;
using Domain.Auth.Services;
using Domain.Email;
using Domain.Repository;
using Domain.Repository.Entities;
using DTO.Auth;
using Login.Repository;
using Repository.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;

using System;
using System.Threading.Tasks;
using Xunit;

namespace Login.Tests;
[Collection("LoginTests")]
public class AuthServiceTests
{
    private readonly LoginDbContext _dbContext;
    private readonly IUserRepository _userRepositoryMock;
    private readonly IEmailService _emailServiceMock;
    private readonly IMapper _mapper;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly IAuthService _authService;
    private readonly RegisterUserDto _registerUserDtoMock = new RegisterUserDto("First", "Last", DateTime.UtcNow.AddYears(-30), "any", "firstlast@example.com", "Password123", "Password123");
    private LoginDbContext GetInMemoryDbContext()
    {
        // Create a unique database name for each test to avoid conflicts
        var options = new DbContextOptionsBuilder<LoginDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;
        return new LoginDbContext(options);
    }
    public AuthServiceTests()
    {
        _dbContext = GetInMemoryDbContext();
        _userRepositoryMock = new UserRepositoryEF(_dbContext);
        _configurationMock = new Mock<IConfiguration>();
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<User, RegisterUserDto>().ReverseMap();
        });
        _mapper = config.CreateMapper();


        _configurationMock.Setup(c => c["EmailSettings:SenderName"]).Returns("Valid Sender name");
        _configurationMock.Setup(c => c["EmailSettings:SenderEmail"]).Returns("validemail@gmail.com");
        _configurationMock.Setup(c => c["EmailSettings:SenderPassword"]).Returns("emailpassword");
        _configurationMock.Setup(c => c["App:BaseUrl"]).Returns("https://localhost:5000");
        _configurationMock.Setup(c => c["JWT:Key"]).Returns("justaverylongstringusuallyaguidthatcanbeusedhere");
        _configurationMock.Setup(c => c["JWT:Issuer"]).Returns("https://localhost:5000");
        _configurationMock.Setup(c => c["JWT:Audience"]).Returns("https://localhost:5000");

        _emailServiceMock = new EmailService(_configurationMock.Object);
        _authService = new AuthService(_userRepositoryMock, _emailServiceMock, _configurationMock.Object, _mapper);
        _authService.RegisterAsync(_registerUserDtoMock);

    }

    [Fact]
    public async Task CreatesUser()
    {
        var newUser = new RegisterUserDto("Paulo", "Weber", DateTime.UtcNow.AddYears(-30), "any", "paulo.weber@gmail.com", "123456", "123456");
        var result = await _authService.RegisterAsync(newUser);

        Assert.True(result);
    }

    [Fact]
    public async Task CreateUser_UserExists_ThrowsException()
    {
        await Assert.ThrowsAsync<Exception>(() => _authService.RegisterAsync(_registerUserDtoMock));
    }

    [Fact]
    public async Task ConfirmUserEmail()
    {
        var user = await _userRepositoryMock.GetByEmailAsync(_registerUserDtoMock.Email);
        var result = await _authService.ConfirmEmailAsync(new ConfirmEmailDto(user.Email, user.EmailConfirmationCode));
        Assert.True(result);
    }

    [Fact]
    public async Task Login()
    {
        LoginDto login = new LoginDto(_registerUserDtoMock.Email, _registerUserDtoMock.Password, false);
        var result = await _authService.LoginAsync(login);
        Assert.True(!string.IsNullOrEmpty(result)); 
    }

    [Fact]
    public async Task Login_Fail_ThrowsException()
    {
        LoginDto login = new LoginDto(_registerUserDtoMock.Email, new Guid().ToString(), false);
        await Assert.ThrowsAsync<Exception>(() => _authService.LoginAsync(login));
    }


}