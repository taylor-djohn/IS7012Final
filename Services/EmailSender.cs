using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;

namespace IS7012Final.Services
{
    // Development email sender that logs emails (including confirmation links) so you can copy from logs during testing.
    public class EmailSender : IEmailSender
    {
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(ILogger<EmailSender> logger)
        {
            _logger = logger;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            _logger.LogInformation("SendEmailAsync to {Email}. Subject: {Subject}", email, subject);
            _logger.LogInformation("Email body (truncated): {Body}", htmlMessage?.Length > 1000 ? htmlMessage.Substring(0, 1000) + "..." : htmlMessage);
            return Task.CompletedTask;
        }
    }
}
