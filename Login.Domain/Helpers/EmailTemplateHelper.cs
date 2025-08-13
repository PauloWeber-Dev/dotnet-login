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
                    <h1>Password reset request</h1>
                    <p>Dear {user.FirstName} {user.LastName},</p>
                    <p>We received a password reset request for your account. Click on the link below to proceed:</p>
                    <a href='{resetLink}'>Reset password</a>
                    <p>Or copy the URL below and paste it in a browser window:</p>
                    <p>{resetLink}</p>
                    <p>If you did not make this request, please contact us.</p>
                    <p>FOR YOUR SECURITY, THIS LINK WILL EXPIRE IN 1 HOUR.</p>
                </body>
                </html>";
        }

        public static string GetConfirmEmailTemplate(string confirmEmailLink, User user)
        {
            return $@"
                <html>
                <body>
                    <h1>Email confirmation</h1>
                    <p>Dear {user.FirstName} {user.LastName},</p>
                    <p>Thank you for registering with us.To confirm you email, click the link below:</p>
                    <a href='{confirmEmailLink}'>Confirm email</a>
                    <p>Or copy the URL below and paste it in a browser window:</p>
                    <p>{confirmEmailLink}</p>
                    <p>If you did not make this request, please contact us.</p>
                    <p>FOR YOUR SECURITY, THIS LINK WILL EXPIRE IN 2 DAYS.</p>
                    <p>Thank you!</p>
                    <p>FOR TESTING PURPOSES, SWAGGER JSON</p>
                    <pre>
                    {{
                        ""email"": ""{user.Email}"",
                        ""code"": ""{user.EmailConfirmationCode}""
                    }}
                    </pre>
                </body>
                </html>";
        }
    }
}
