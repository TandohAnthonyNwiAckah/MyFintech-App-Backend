using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace TANA.Helpers
{
    public class SmsHelper
    {
        private readonly IConfiguration Config;

        public SmsHelper(IConfiguration configuration)
        {
            Config = configuration;
        }

        public async Task SendSms(string phoneNumber, string message)
        {
            try
            {
                // Implementation for sending SMS using Amane3Connect
                var response = await SmsAmane3Connect(phoneNumber, message);
                Console.WriteLine($"SMS Amane3Connect Response: {response}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending SMS to {phoneNumber}: {ex.Message}");
            }
        }

        private async Task<string> SmsAmane3Connect(string recipients, string message)
        {
            // Initialize HttpClient (In production, use IHttpClientFactory for better performance)
            using (var client = new HttpClient())
            {
                var apiUrl = "https://sms.amane3connect.com/api/v1/sms/send";

                // Get configurations or use defaults
                var apiKey = Config["SMS:Amane3Connect:ApiKey"] ?? "";
                var sender = Config["SMS:Amane3Connect:Sender"] ?? "Amane3Connect";

                // Convert recipients string to a list/array
                var recipientsList = recipients.Split(
                    new[] { ',' },
                    StringSplitOptions.RemoveEmptyEntries
                );

                // Set payload
                var payload = new
                {
                    sender = sender,
                    message = message,
                    recipients = recipientsList,
                };

                // Convert payload to JSON
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Set Header
                client.DefaultRequestHeaders.Add("api-key", apiKey);

                // Execute POST request
                var response = await client.PostAsync(apiUrl, content);

                // Return the response body
                return await response.Content.ReadAsStringAsync();
            }
        }
    }
}
