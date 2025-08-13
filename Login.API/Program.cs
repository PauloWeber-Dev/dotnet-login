using Domain.Auth;
using Domain.Auth.Services;
using Domain.Email;
using Domain.Repository;
using DTO.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Repository.Auth;
using Serilog;
using System.Reflection;
using System.Text;
//Uncomment the following lines to use EntityFramework
using Login.Repository;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IAuthService, AuthService>();
// Uncomment the following line to use Dapper with SQL Server
//builder.Services.AddScoped<IUserRepository, UserRepositoryDapper>();
// Uncomment the following line to use Entity Framework with SQL Server
builder.Services.AddScoped<IUserRepository, UserRepositoryEF>().AddDbContext<LoginDbContext>(options =>
    options.UseSqlServer(builder.Configuration["Database:ConnectionString"]!));
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddAutoMapper(Assembly.Load("Login.Domain"));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
    {
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        });

        // Require JWT for protected endpoints
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
        options.SwaggerDoc("v1", new OpenApiInfo { Title = "Login API", Version = "v1" });
    });
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JWT:Issuer"],
            ValidAudience = builder.Configuration["JWT:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"]))
        };
    })
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Google:ClientId"];
        options.ClientSecret = builder.Configuration["Google:ClientSecret"];
    }).AddFacebook(options =>
    {
        options.AppId = builder.Configuration["Facebook:AppId"];
        options.AppSecret = builder.Configuration["Facebook:AppSecret"];
    });

builder.Services.AddAuthorizationBuilder()
  .AddPolicy("admin_login", policy =>
        policy
            .RequireRole("admin"));

// Configure Serilog for logging
builder.Host.UseSerilog((context, config) =>
{
    config
        .WriteTo.Console()
        .WriteTo.File("logs/app.log", rollingInterval: RollingInterval.Day)
        .MinimumLevel.Information();
});

var app = builder.Build();


app.UseAuthentication();
app.UseAuthorization();

//Setup Swagger
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Login API v1"));
}
else
{
    // TODO: Configure production error handling
    app.UseExceptionHandler("/error");
}
// Endpoints
app.MapPost("/register", async (RegisterUserDto dto, IAuthService authService, ILogger<Program> logger) =>
{
    try
    {
        var message = await authService.RegisterAsync(dto);
        logger.LogInformation("User registered: {Email}", dto.Email);
        return Results.Ok();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error registering user: {Email}", dto.Email);
        return Results.BadRequest(ex.Message);
    }
});

app.MapPost("/confirm-email", async (ConfirmEmailDto dto, IAuthService authService, ILogger<Program> logger) =>
{
    try
    {
        var message = await authService.ConfirmEmailAsync(dto);
        return Results.Ok();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error confirming email: {Email}", dto.Email);
        return Results.BadRequest(ex.Message);
    }
});

app.MapPost("/login", async (LoginDto dto, IAuthService authService, ILogger<Program> logger) =>
{
    try
    {
        var token = await authService.LoginAsync(dto);
        return Results.Ok(new { Token = token });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error logging in user: {Email}", dto.Email);
        return Results.BadRequest(ex.Message);
    }
});

//TODO: Implement service
app.MapPost("/google-login", async (string googleToken, IAuthService authService, ILogger<Program> logger) =>
{
    try
    {
        var token = await authService.GoogleLoginAsync(googleToken);
        return Results.Ok(new { Token = token });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error logging in with Google token");
        return Results.BadRequest(ex.Message);
    }
});

//TODO: Implement service
app.MapPost("/facebook-login", async (string facebookToken, IAuthService authService, ILogger<Program> logger) =>
{
    try
    {
        var token = await authService.FacebookLoginAsync(facebookToken);
        return Results.Ok(new { Token = token });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error logging in with Facebook token");
        return Results.BadRequest(ex.Message);
    }
});

app.MapPost("/forgot-password", async (string email, IAuthService authService, ILogger<Program> logger) =>
{
    try
    {
        logger.LogInformation("Requesting password reset for email: {Email}", email);
        await authService.RequestPasswordResetAsync(email);
        return Results.Ok();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error requesting password reset for email: {Email}", email);
        return Results.BadRequest(ex.Message);
    }
});

//TODO: Password is coming in plain text, hash it before sending
app.MapPost("/reset-password", async (string token, string newPassword, IAuthService authService, ILogger<Program> logger) =>
{
    try
    {
        logger.LogInformation("Resetting password with token: {Token}", token);
        await authService.ResetPasswordAsync(token, newPassword);
        return Results.Ok();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error resetting password with token: {Token}", token);
        return Results.BadRequest(ex.Message);
    }
});

app.MapPost("/enable-mfa", async (string email, IAuthService authService, ILogger<Program> logger) =>
{
    try
    {
        logger.LogInformation("Enabling MFA for email: {Email}", email);
        var qrCodeUrl = await authService.EnableMfaAsync(email);
        return Results.Ok(new { QrCodeUrl = qrCodeUrl });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error enabling MFA for email: {Email}", email);
        return Results.BadRequest(ex.Message);
    }
});

app.MapPost("/verify-mfa", async (VerifyMfaDto dto, IAuthService authService, ILogger<Program> logger) =>
{
    try
    {
        var token = await authService.VerifyMfaAsync(dto);
        return Results.Ok(new { Token = token });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error verifying MFA for email: {Email}", dto.Email);
        return Results.BadRequest(ex.Message);
    }
});


//TODO: Move this to an admin microservice (eventually)
app.MapPost("/change-status", [Authorize(Policy = "admin_login")] async (int userId, int status, IAuthService authService, ILogger<Program> logger) =>
{
    try
    {
        logger.LogInformation("Changing user status for userId: {UserId} to status: {Status}", userId, status);
        var token = await authService.SetStatus(userId, (UserStatus)status);
        return Results.Ok(new { Token = token });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error changing user status for userId: {UserId}", userId);
        return Results.BadRequest(ex.Message);
    }
});

//TODO: Move this to an admin microservice (eventually)
app.MapPost("/set-admin", [Authorize(Policy = "admin_login")] async (int userId, IAuthService authService, ILogger<Program> logger) =>
{
    try
    {
        logger.LogInformation("Setting admin role for userId: {UserId}", userId);
        var token = await authService.SetAdmin(userId);
        return Results.Ok();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error setting admin role for userId: {UserId}", userId);
        return Results.BadRequest(ex.Message);
    }
});

app.Run();