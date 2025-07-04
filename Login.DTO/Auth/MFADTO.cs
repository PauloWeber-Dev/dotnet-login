using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO.Auth;

public record EnableMfaDto(int UserId);
public record VerifyMfaDto(string Email, string Code);