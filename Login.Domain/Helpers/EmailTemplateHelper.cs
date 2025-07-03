using Domain.Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Login.Domain.Helpers
{
    internal static class EmailTemplateHelper
    {
        public static string GetResetEmailTemplate(string resetLink, User user)
        {
            return $@"
                <html>
                <body>
                    <h1>Password Reset Request</h1>
                    <p>Dear {user.FirstName} {user.LastName},</p>
                    <p>We received a request to reset your password. Click the link below to reset it:</p>
                    <a href='{resetLink}'>Reset Password</a>
                    <p>If you did not request this, please ignore this email.</p>
                </body>
                </html>";
        }
    }
}
