using Domain.Auth;
using DTO.Auth;
using Domain.Auth.Services;
using Domain.Repository;
using Domain.Email;
using Repository.Auth;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddAutoMapper(Assembly.Load("Login.Domain"));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Login API", Version = "v1" });
    });
builder.Services
    .AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Google:ClientId"];
        options.ClientSecret = builder.Configuration["Google:ClientSecret"];
    }).AddFacebook(options =>
    {
        options.AppId = builder.Configuration["Facebook:AppId"];
        options.AppSecret = builder.Configuration["Facebook:AppSecret"];
    });



var app = builder.Build();

// Configure the HTTP request pipeline
app.UseAuthentication();

if(app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Login API v1"));
}
else
{
    // For production, you might want to use a more robust error handling middleware
    app.UseExceptionHandler("/error");
}
// Endpoints
app.MapPost("/register", async (RegisterUserDto dto, IAuthService authService) =>
{
    try
    {
        var message = await authService.RegisterAsync(dto);
        return Results.Ok(new { message });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

app.MapPost("/confirm-email", async (ConfirmEmailDto dto, IAuthService authService) =>
{
    try
    {
        var message = await authService.ConfirmEmailAsync(dto);
        return Results.Ok(new { message });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

app.MapPost("/login", async (LoginDto dto, IAuthService authService) =>
{
    try
    {
        var token = await authService.LoginAsync(dto);
        return Results.Ok(new { Token = token });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

app.MapPost("/google-login", async (string googleToken, IAuthService authService) =>
{
    try
    {
        var token = await authService.GoogleLoginAsync(googleToken);
        return Results.Ok(new { Token = token });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

app.MapPost("/facebook-login", async (string facebookToken, IAuthService authService) =>
{
    try
    {
        var token = await authService.FacebookLoginAsync(facebookToken);
        return Results.Ok(new { Token = token });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

app.MapPost("/forgot-password", async (string email, IAuthService authService) =>
{
    try
    {
        await authService.RequestPasswordResetAsync(email);
        return Results.Ok("Password reset email sent");
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

app.MapPost("/reset-password", async (string token, string newPassword, IAuthService authService) =>
{
    try
    {
        await authService.ResetPasswordAsync(token, newPassword);
        return Results.Ok("Password reset successfully");
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

app.MapPost("/enable-mfa", async (EnableMfaDto dto, IAuthService authService) =>
{
    try
    {
        var qrCodeUrl = await authService.EnableMfaAsync(dto);
        return Results.Ok(new { QrCodeUrl = qrCodeUrl });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

app.MapPost("/verify-mfa", async (VerifyMfaDto dto, IAuthService authService) =>
{
    try
    {
        var token = await authService.VerifyMfaAsync(dto);
        return Results.Ok(new { Token = token });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

app.Run();