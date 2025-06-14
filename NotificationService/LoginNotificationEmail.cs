using System;
using System.Net.Mail;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Text.Json;

namespace NotificationService;

public class LoginNotificationEmail
{
    private readonly ILogger<LoginNotificationEmail> _logger;

    public LoginNotificationEmail(ILogger<LoginNotificationEmail> logger)
    {
        _logger = logger;
    }

    [Function("KafkaTriggerFunction")]
    public async Task Run(
    [KafkaTrigger(
        "kafka:9092",
        "after-login-email-topic",
        ConsumerGroup = "function-consumer-group")]
    string message)
    {
        _logger.LogInformation($"Odebrano wiadomo z Kafki: {message}");

        try
        {
            using var doc = JsonDocument.Parse(message);
            var root = doc.RootElement;

            if (root.TryGetProperty("Value", out var valueElement))
            {
                var value = valueElement.GetString();
                if (!string.IsNullOrEmpty(value))
                {
                    var parts = value.Split(':', 2);
                    if (parts.Length == 2)
                    {
                        var toEmail = parts[0].Trim().Replace("\"", "");
                        var body = parts[1];
                        _logger.LogInformation($"Próba wys³ania e-maila do: {toEmail}");
                        await SendEmailAsync(body, toEmail);
                    }
                    else
                    {
                        _logger.LogError("Nieprawid³owy format Value: " + value);
                    }
                }
                else
                {
                    _logger.LogError("Value property is empty in Kafka message.");
                }
            }
            else
            {
                _logger.LogError("Brak w³aœciwoœci 'Value' w wiadomoœci Kafka: " + message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"B³¹d podczas przetwarzania wiadomoœci Kafka: {ex}");
        }
    }

    static async Task SendEmailAsync(string message, string toEmail)
    {
        try
        {
            string smtpHost = Environment.GetEnvironmentVariable("smtpHost");
            string smtpPortStr = Environment.GetEnvironmentVariable("smtpPort");
            string smtpUsername = Environment.GetEnvironmentVariable("smtpUsername");
            string smtpPassword = Environment.GetEnvironmentVariable("smtpPassword");

            if (string.IsNullOrWhiteSpace(smtpHost) ||
                string.IsNullOrWhiteSpace(smtpPortStr) ||
                string.IsNullOrWhiteSpace(smtpUsername) ||
                string.IsNullOrWhiteSpace(smtpPassword))
            {
                Console.WriteLine("SMTP configuration is missing or invalid.");
                return;
            }

            int smtpPort = int.Parse(smtpPortStr);

            toEmail = toEmail.Trim().Replace("\"", "");
            smtpUsername = smtpUsername.Trim().Replace("\"", "");

            using (var client = new SmtpClient(smtpHost, smtpPort))
            {
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(smtpUsername),
                    Subject = "Test",
                    Body = message,
                    IsBodyHtml = false
                };
                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
                Console.WriteLine($"E-mail wyslany do {toEmail} z wiadomosci: {message}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Bd podczas wysyania e-maila: {ex}");
        }
    }
}
