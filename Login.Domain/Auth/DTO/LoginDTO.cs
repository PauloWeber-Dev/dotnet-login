using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Auth.DTO;

public record LoginDto(string Email, string Password, bool RememberMe = false);
