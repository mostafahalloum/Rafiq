using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace Rafiq.Services
{
    public class DeepSeekService : IDeepSeekService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public DeepSeekService (
            HttpClient httpClient,
            IConfiguration configuration )
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<string> AskAI ( string message )
        {
            try {
                var apiKey = _configuration ["OpenRouter:ApiKey"];

                var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    "https://openrouter.ai/api/v1/chat/completions"
                );

                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", apiKey);

                var body = new
                {
                    model = "deepseek/deepseek-chat",

                    messages = new []
                    {
                        new
                        {
                            role = "user",
                            content = message
                        }
                    }
                };

                request.Content = new StringContent(
                    JsonConvert.SerializeObject(body),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.SendAsync(request);

                var json = await response.Content.ReadAsStringAsync();

                if ( !response.IsSuccessStatusCode ) {
                    return json;
                }

                JObject result = JObject.Parse(json);

                return result ["choices"]? [0]? ["message"]? ["content"]?.ToString()
                       ?? "No response";
            }
            catch ( Exception ex ) {
                return ex.Message;
            }
        }
    }
}