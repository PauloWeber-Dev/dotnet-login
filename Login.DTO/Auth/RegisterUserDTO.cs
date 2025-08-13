namespace DTO.Auth;

public record RegisterUserDto(
string FirstName,
string LastName,
DateTime BirthDate,
string Gender,
string Email,
string Password,
string ConfirmPassword,
bool? ExternalLogin = null,
string? GoogleId = null,
string? FacebookId = null
);
