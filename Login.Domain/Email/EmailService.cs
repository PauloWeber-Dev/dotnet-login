using Domain.Repository.Entities;
using Microsoft.Extensions.Configuration;
using MimeKit;
using Newtonsoft.Json.Linq;
using System.Net.Mail;


namespace Login.Domain.Email
{
    public class EmailService : IEmailService
    {
        public readonly IConfiguration _configuration;
        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public void SendEmail(User user, string subject, string body)
        {
            var message = new MailMessage();
            message.From = new MailAddress(_configuration["Smtp:Sender"]!, "Auth API");
            message.To.Add(new MailAddress(user.Email!, @"{user.FirstName} {user.LastName}"));
            
            message.Subject =subject;
            message.Body = body;
            message.IsBodyHtml = true;

            using var client = new SmtpClient(_configuration["Smtp:Host"]!, int.Parse(_configuration["Smtp:Port"]!));
            client.EnableSsl = bool.Parse(_configuration["Smtp:EnableSsl"]!);
            client.Credentials = new System.Net.NetworkCredential(_configuration["Smtp:Username"]!, _configuration["Smtp:Password"]!);
            client.SendAsync(message, Guid.NewGuid().ToString());
        }
    }
}
