using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Rafiq.DTOs.Request;
using Rafiq.Models;

namespace Rafiq.Services
{
    public class NotificationService : INotificationService
    {
        private readonly HttpClient _httpClient;

        var botTokenKey = _configuration["Telegram:BotToken"];
        private readonly string _botToken = botTokenKey;

        var chatIdKey = _configuration["Telegram:ChatId"];
        private readonly string _chatId = chatIdKey;




        public NotificationService ( HttpClient httpClient )
        {
            _httpClient = httpClient;
        }

        public async Task SendOrderAlertAsync ( Order order, string customerName, string status )
        {
            var messageBuilder = new StringBuilder();
            messageBuilder.AppendLine("🚨 <b>Rafiq System: New Order Confirmed!</b>");
            messageBuilder.AppendLine("=================================");

            messageBuilder.AppendLine($"📦 <b>Order ID:</b> #{order.Id}");
            messageBuilder.AppendLine($"🆔 <b>User ID:</b> <code>{order.UserId}</code>");
            messageBuilder.AppendLine($"👤 <b>Customer:</b> {customerName}");


            DateTime localTime = order.CreateAt.AddHours(3);
            messageBuilder.AppendLine($"📅 <b>Order Time:</b> {localTime:yyyy-MM-dd hh:mm tt}");
            messageBuilder.AppendLine($"⚡ <b>Status:</b> <code>{status}</code>");
            messageBuilder.AppendLine("=================================");

        
            messageBuilder.AppendLine("🛒 <b>Order Items:</b>");
            messageBuilder.AppendLine("---------------------------------");

            if ( order.OrderItems != null && order.OrderItems.Count > 0 ) {
                foreach ( var item in order.OrderItems ) {
                    string productName = item.Product?.Name ?? "Unknown Product";
                    decimal itemTotal = ( decimal ) ( item.Price * item.Quantity );

                    messageBuilder.AppendLine($"🔹 <b>{productName}</b>");
                    messageBuilder.AppendLine($"   🔹 {item.Quantity} x {item.Price:N2} EGP = <b>{itemTotal:N2} EGP</b>");
                    messageBuilder.AppendLine("· · · · · · · · · · · · · · · · · · · · · · · · ·");
                }
            }
            else {
                messageBuilder.AppendLine("⚠️ No item details found (Check Includes).");
            }

            messageBuilder.AppendLine("=================================");
            messageBuilder.AppendLine($"💰 <b>TOTAL PRICE: {order.TotalPrice:N2} EGP</b>");
            messageBuilder.AppendLine("=================================");
            messageBuilder.AppendLine("💻 <i>Please open Rafiq Dashboard to manage shipping.</i>");

            var payload = new
            {
                chat_id = _chatId,
                text = messageBuilder.ToString(),
                parse_mode = "HTML"
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var url = $"https://api.telegram.org/bot{_botToken}/sendMessage";

            try {
                var response = await _httpClient.PostAsync(url, content);
                if ( !response.IsSuccessStatusCode ) {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"Telegram API Error: {errorContent}");
                }
            }
            catch ( Exception ex ) {
                System.Diagnostics.Debug.WriteLine($"Exception sending telegram notification: {ex.Message}");
            }
        }
    }
}