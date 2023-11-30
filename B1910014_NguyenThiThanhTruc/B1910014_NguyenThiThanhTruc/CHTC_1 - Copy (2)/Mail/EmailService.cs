using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Net;

namespace CHTC_1.Mail
{
    public class EmailService
    {
        private readonly EmailSettings _emailSettings;

        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            using (var client = new SmtpClient())
            {
                var credentials = new NetworkCredential
                {
                    UserName = _emailSettings.SmtpUsername,
                    Password = _emailSettings.SmtpPassword
                };

                client.Credentials = credentials;
                client.Host = _emailSettings.SmtpServer;
                client.Port = _emailSettings.SmtpPort;
                client.EnableSsl = _emailSettings.SmtpUseSSL;

                using (var emailMessage = new MailMessage())
                {
                    emailMessage.From = new MailAddress(_emailSettings.SmtpUsername);
                    emailMessage.To.Add(toEmail);
                    emailMessage.Subject = subject;
                    emailMessage.Body = message;
                    emailMessage.IsBodyHtml = true;

                    await client.SendMailAsync(emailMessage);
                }
            }
        }
    }
}
