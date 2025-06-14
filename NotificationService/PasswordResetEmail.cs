using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Mail;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net;

namespace NotificationService;

public class PasswordResetEmail
{
    private readonly ILogger<PasswordResetEmail> _logger;
    public PasswordResetEmail(ILogger<PasswordResetEmail> logger) { _logger = logger; }

    private class PasswordResetMessage
    {
        public string Email { get; set; }
        public string NewPassword { get; set; }
    }

    [Function("PasswordResetTrigger")]
    public async Task Run(
        [KafkaTrigger(
            "kafka:9092",
            "password-reset-topic",
            ConsumerGroup = "functions-password-reset-group")]
        string kafkaMessageWrapper)
    {
        _logger.LogInformation($"Received password reset message: {kafkaMessageWrapper}");

        try
        {
            using var doc = JsonDocument.Parse(kafkaMessageWrapper);
            if (!doc.RootElement.TryGetProperty("Value", out var valueElement) || valueElement.ValueKind == JsonValueKind.Null)
            {
                _logger.LogError("Kafka message is missing 'Value' property.");
                return;
            }

            var message = JsonSerializer.Deserialize<PasswordResetMessage>(valueElement.GetString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (message == null || string.IsNullOrEmpty(message.Email))
            {
                _logger.LogError("Invalid password reset data format.");
                return;
            }

            string subject = "Your new password for GameEShop";
            string body = $"Hello,\n\nYour new temporary password is: {message.NewPassword}\n\nPlease log in and change it as soon as possible.\n\nBest regards,\nThe GameEShop Team";

            // To wywo³anie jest teraz poprawne
            await SendEmailAsync(subject, body, message.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error processing password reset message: {ex.Message}");
        }
    }

    // --- POPRAWIONA FUNKCJA POMOCNICZA ---
    static async Task SendEmailAsync(string subject, string body, string toEmail)
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
                    Subject = subject, // Poprawione: u¿ywa przekazanego tematu
                    Body = body,       // Poprawione: u¿ywa przekazanej treœci
                    IsBodyHtml = false
                };
                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
                Console.WriteLine($"Email successfully sent to {toEmail}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while sending email: {ex}");
        }
    }
}