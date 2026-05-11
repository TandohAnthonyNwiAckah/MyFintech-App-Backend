using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;

public class EmailHelper
{
    private readonly IConfiguration Config;

    public EmailHelper(IConfiguration configuration)
    {
        Config = configuration;
    }

    public void SendEmail(string receiverEmail, string subject, string message)
    {
        var senderEmail = Config["Email:Email"];
        var senderName = Config["Email:Name"];

        var smtpUsername = Config["Email:Email"];
        var smtpPassword = Config["Email:Password"];

        var smtpHost = Config["Email:Host"];
        var smtpPort = int.Parse(Config["Email:Port"]);

        var mail = new MimeMessage();
        mail.From.Add(new MailboxAddress(senderName, senderEmail));
        mail.To.Add(MailboxAddress.Parse(receiverEmail));
        mail.Subject = subject;
        mail.Body = new TextPart(TextFormat.Html) { Text = message };

        // send email
        using var smtp = new SmtpClient();
        smtp.Connect(smtpHost, smtpPort, SecureSocketOptions.StartTls);
        smtp.Authenticate(smtpUsername, smtpPassword);
        smtp.Send(mail);
        smtp.Disconnect(true);
    }
}
