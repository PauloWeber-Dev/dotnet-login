using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Auth.Services;


public enum UserStatus : byte
{
    Registered, // Initial status when user registers
    Active, // User is active and can log in, email confirmed
    PasswordResetRequested, // User requested a password reset
    Inactive, // User is inactive, admin action
    Suspended, // User is suspended, cannot log in, admin action
    Deleted // User is deleted, cannot log in, admin action
}


