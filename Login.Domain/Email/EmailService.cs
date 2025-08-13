using Domain.Repository.Entities;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;


namespace Domain.Email
{
    public class EmailService : IEmailService
    {
        public readonly IConfiguration _configuration;
        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<bool> SendEmail(User user, string subject, string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_configuration["EmailSettings:SenderName"]!,_configuration["EmailSettings:SenderEmail"]!));
            message.To.Add(new MailboxAddress(@"{user.FirstName} {user.LastName}", user.Email!));
            
            message.Subject =subject;
            message.Body = new TextPart("html")
            {
                Text = body
            };
            try
            {
                using (var client = new SmtpClient())
                {

                    client.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                    client.Authenticate(_configuration["EmailSettings:SenderEmail"]!, _configuration["EmailSettings:SenderPassword"]!);
                    await client.SendAsync(message);
                    client.Disconnect(true);
                }
                return true;
            }
            catch (Exception)
            {

                throw;
            }

        }
    }
}
