using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Auth.DTO;

public record RegisterUserDto(
string FirstName,
string LastName,
DateTime BirthDate,
string Gender,
string Email,
string Password,
bool? ExternalLogin = null,
string? GoogleId = null
);
