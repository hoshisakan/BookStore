// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text;
// using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using MimeKit;
// using SendGrid;
// using SendGrid.Helpers.Mail;
using static HoshiBook.Utility.Config;

namespace HoshiBook.Utility
{
    public class EmailSender : IEmailSender
    {
        // public string SendGridSecret { get; set; }

        public EmailSender(IConfiguration _config)
        {
            // SendGridSecret = _config["SendGrid:SecretKey"];
            // SendGridSecret = _config.GetValue<string>("SendGrid:SecretKey");
            // Console.WriteLine($"SendGridSecret: {SendGridSecret}");
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            //TODO Send email with MimeKit and MailKit (works)
            var emailToSend = new MimeMessage();
            emailToSend.From.Add(MailboxAddress.Parse("hello@dotnetmastery.com"));
            emailToSend.To.Add(MailboxAddress.Parse(email));
            emailToSend.Subject = subject;
            emailToSend.Body = new TextPart(MimeKit.Text.TextFormat.Html){
                Text = htmlMessage
            };
            //send email
            using (var emailClient = new SmtpClient())
            {
                emailClient.Connect(
                    "smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls
                );
                emailClient.Authenticate(GoolgeSenderMailName, GoolgeSenderMailPassword);
                emailClient.Send(emailToSend);
                emailClient.Disconnect(true);
            }
            return Task.CompletedTask;
            //TODO Not working cannot send email to recipient from SendGrid specified sender email
            // var client = new SendGridClient(SendGridSecret);
            // var from = new EmailAddress("todosender000@gmail.com", "Hoshi Book");
            // var to = new EmailAddress("qunjend897436@gmail.com ");
            // var msg = MailHelper.CreateSingleEmail(from, to , subject, "", htmlMessage);
            // return client.SendEmailAsync(msg);
        }
    }
}