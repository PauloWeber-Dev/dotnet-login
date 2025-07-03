using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Login.Domain.Auth.DTO;

public record ConfirmEmailDto(string Email, string Code);