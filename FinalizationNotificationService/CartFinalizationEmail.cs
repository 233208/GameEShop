using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NotificationService;

public class CartFinalizationEmail
{
    private readonly ILogger<CartFinalizationEmail> _logger;

    public CartFinalizationEmail(ILogger<CartFinalizationEmail> logger)
    {
        _logger = logger;
    }

    private class CartItem
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
    }

    private class CartFinalizationMessage
    {
        public string Email { get; set; }
        public CartItem[] Items { get; set; }
    }

    [Function("CartFinalizationTrigger")]
    public async Task Run(
        [KafkaTrigger(
            "kafka:9092",
            "cart-finalization-topic",
            ConsumerGroup = "functions-cart-finalization-group")]
        string kafkaMessageWrapper) 
    {
        _logger.LogInformation($"Received raw Kafka message: {kafkaMessageWrapper}");

        try
        {
            using var doc = JsonDocument.Parse(kafkaMessageWrapper);
            var root = doc.RootElement;

            if (!root.TryGetProperty("Value", out var valueElement) || valueElement.ValueKind == JsonValueKind.Null)
            {
                _logger.LogError("Kafka message is missing 'Value' property or it is null.");
                return;
            }

            string cartJson = valueElement.GetString();
            _logger.LogInformation($"Extracted cart data from 'Value': {cartJson}");

            var message = JsonSerializer.Deserialize<CartFinalizationMessage>(cartJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (message == null || string.IsNullOrEmpty(message.Email))
            {
                _logger.LogError("Invalid cart data format or missing email inside 'Value'.");
                return;
            }

            var emailBody = new StringBuilder();
            emailBody.AppendLine("Thank you for your order in GameEShop!");
            emailBody.AppendLine("You have ordered:");
            emailBody.AppendLine("-------------------");
            foreach (var item in message.Items)
            {
                emailBody.AppendLine($"- {item.Name} (Quantity: {item.Quantity})");
            }
            emailBody.AppendLine("-------------------");
            emailBody.AppendLine("We will process your order shortly.");

            await SendEmailAsync("Your GameEShop Order Confirmation", emailBody.ToString(), message.Email);
        }
        catch (JsonException jsonEx)
        {
            _logger.LogError($"JSON parsing error: {jsonEx.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"An unexpected error occurred: {ex}");
        }
    }

    static async Task SendEmailAsync(string subject, string body, string toEmail)
    {
        try
        {
            string smtpHost = Environment.GetEnvironmentVariable("smtpHost");
            string smtpPortStr = Environment.GetEnvironmentVariable("smtpPort");
            int smtpPort = int.Parse(smtpPortStr);
            string smtpUsername = Environment.GetEnvironmentVariable("smtpUsername");
            string smtpPassword = Environment.GetEnvironmentVariable("smtpPassword");

            toEmail = toEmail.Trim().Replace("\"", "");
            smtpUsername = smtpUsername.Trim().Replace("\"", "");

            using (var client = new SmtpClient(smtpHost, smtpPort))
            {
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(smtpUsername),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = false
                };
                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
                Console.WriteLine($"Email sent to {toEmail} with subject: {subject}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending email: {ex}");
        }
    }
}