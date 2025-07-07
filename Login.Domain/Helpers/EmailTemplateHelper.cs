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
                    <h1>Solicitação de redefinição de senha</h1>
                    <p>Caro {user.FirstName} {user.LastName},</p>
                    <p>Recebemos uma solicitação para alterar seu email. Para prosseguirmos com sua solicitação, clique no link abaixo:</p>
                    <a href='{resetLink}'>Redefinir senha</a>
                    <p>Ou copie e cole o texto abaixo no seu navegador:</p>
                    <p>{resetLink}</p>
                    <p>Se você não fez essa solicitação, entre em contato conosco.</p>
                </body>
                </html>";
        }

        public static string GetConfirmEmailTemplate(string confirmEmailLink, User user)
        {
            return $@"
                <html>
                <body>
                    <h1>Confirmação de Email</h1>
                    <p>Caro {user.FirstName} {user.LastName},</p>
                    <p>Obrigado por efetuar seu registro. Para confirmar seu email clique no link abaixo:</p>
                    <a href='{confirmEmailLink}'>Confirmar email</a>
                    <p>Ou copie e cole o texto abaixo no seu navegador:</p>
                    <p>{confirmEmailLink}</p>
                </body>
                </html>";
        }
    }
}
