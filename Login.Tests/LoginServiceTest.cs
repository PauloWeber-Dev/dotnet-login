using System;
using System.Threading.Tasks;
using AutoMapper;
using Domain.Auth;
using Domain.Auth.Services;
using Domain.Repository;
using Domain.Email;
using DTO.Auth;
using Moq;
using Xunit;
using Domain.Repository.Entities;
using Microsoft.Extensions.Configuration;

namespace Login.Tests;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly IAuthService _authService;

    public AuthServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _emailServiceMock = new Mock<IEmailService>();
        _mapperMock = new Mock<IMapper>();
        _configurationMock = new Mock<IConfiguration>();

        // Mock configuration values (e.g., for JWT, Google, or Facebook settings)
        _configurationMock.Setup(c => c["Jwt:Secret"]).Returns("super-secret-jwt-key");
        _configurationMock.Setup(c => c["Google:ClientId"]).Returns("google-client-id");
        _configurationMock.Setup(c => c["Facebook:AppId"]).Returns("facebook-app-id");

        _authService = new AuthService(_userRepositoryMock.Object, _emailServiceMock.Object, _configurationMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task CreatesUserAndSendsEmail()
    {
        // Arrange
        var dto = new RegisterUserDto("First", "Last", DateTime.UtcNow.AddYears(-30), "any", "firstlast@example.com", "Password123", "Password123");
        var user = new User();
        _mapperMock.Setup(m => m.Map<User>(dto)).Returns(user);
        _userRepositoryMock.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync((User)null);
        _userRepositoryMock.Setup(r => r.CreateAsync(user)).ReturnsAsync(1);
        _emailServiceMock.Setup(e => e.SendEmail(user, "Subject", "Content"));

        // Act
        var result = await _authService.RegisterAsync(dto);

        // Assert
        _userRepositoryMock.Verify(r => r.CreateAsync(user), Times.Once());
        _emailServiceMock.Verify(e => e.SendEmail(user, "Subject", "Content"), Times.Once());
        Assert.Equal("User registered successfully. Please confirm your email.", result);
    }

    [Fact]
    public async Task UserExists_ThrowsException()
    {
        // Arrange
        var dto = new RegisterUserDto("First", "Last", DateTime.UtcNow.AddYears(-30), "any", "firstlast@example.com", "Password123", "Password123");
        var existingUser = new User { Email = dto.Email };
        _userRepositoryMock.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync(existingUser);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _authService.RegisterAsync(dto));
        _userRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Never());
        _emailServiceMock.Verify(e => e.SendEmail(existingUser, "Subject", "Content"), Times.Never());
    }

}