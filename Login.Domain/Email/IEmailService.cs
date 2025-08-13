using Domain.Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Email
{
    public interface IEmailService
    {
        Task<bool> SendEmail(User user, string subject, string body);
    }
}
