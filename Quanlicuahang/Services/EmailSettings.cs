using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Quanlicuahang.Services
{
    // ================================
    //  CẤU HÌNH EMAIL
    // ================================
    public class EmailSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public bool EnableSsl { get; set; }
        public string User { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string From { get; set; } = string.Empty;
    }

    // ================================
    //  INTERFACE EMAIL SERVICE
    // ================================
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string to, string subject, string body);
    }

    // ================================
    //  EMAIL SERVICE (IMPLEMENT)
    // ================================
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;

        public EmailService(IOptions<EmailSettings> options)
        {
            _settings = options.Value;
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                using var client = new SmtpClient(_settings.Host, _settings.Port)
                {
                    EnableSsl = _settings.EnableSsl,
                    Credentials = new NetworkCredential(_settings.User, _settings.Password)
                };

                var mail = new MailMessage
                {
                    From = new MailAddress(_settings.From),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = false
                };

                mail.To.Add(new MailAddress(to));

                await client.SendMailAsync(mail);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
