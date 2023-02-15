using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using MimeKit;


namespace HoshiBook.Utility
{
    public class EmailSender : IEmailSender
    {
        public Dictionary<string, object> SMTPGoogle { get; private set; }
        public Dictionary<string, object> SMTPSenderGrid { get; private set; }

        public EmailSender(IConfiguration _config)
        {
            SMTPGoogle = new Dictionary<string, object>(){
                {"Host", _config.GetValue<string>("SMTP:Google:Host")},
                {"Port", _config.GetValue<int>("SMTP:Google:Port")},
                {"Username", _config.GetValue<string>("SMTP:Google:Username")},
                {"Password", _config.GetValue<string>("SMTP:Google:Password")},
                {"Sender", _config.GetValue<string>("SMTP:Google:Sender")},
            };
            SMTPSenderGrid = new Dictionary<string, object>(){
                {"Host", _config.GetValue<string>("SMTP:SenderGrid:Host")},
                {"Port", _config.GetValue<int>("SMTP:SenderGrid:Port")},
                {"Username", _config.GetValue<string>("SMTP:SenderGrid:Username")},
                {"Password", _config.GetValue<string>("SMTP:SenderGrid:Password")}
            };
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            //TODO Send email with MimeKit and MailKit (works)
            var emailToSend = new MimeMessage();
            emailToSend.From.Add(MailboxAddress.Parse(SMTPGoogle["Sender"].ToString()));
            emailToSend.To.Add(MailboxAddress.Parse(email));
            emailToSend.Subject = subject;
            emailToSend.Body = new TextPart(MimeKit.Text.TextFormat.Html){
                Text = htmlMessage
            };
            //TODO Send email
            using (var emailClient = new SmtpClient())
            {
                emailClient.Connect(
                    SMTPGoogle["Host"].ToString(),
                    Convert.ToInt32(SMTPGoogle["Port"]),
                    MailKit.Security.SecureSocketOptions.StartTls
                );
                emailClient.Authenticate(
                    SMTPGoogle["Username"].ToString(),
                    SMTPGoogle["Password"].ToString()
                );
                emailClient.Send(emailToSend);
                emailClient.Disconnect(true);
            }
            return Task.CompletedTask;
        }
    }
}