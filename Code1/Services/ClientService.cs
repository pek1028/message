using Code1.Models;
using System.Text.Json;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Code1.Services
{
    public class WebServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _secretKey;
        private readonly ILogger<WebServiceClient> _logger;
        private const string BaseUrl = "https://wstesting.choco.asia";

        public WebServiceClient(HttpClient httpClient, ILogger<WebServiceClient> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(BaseUrl);
            _secretKey = configuration["WebService:SecretKey"];
            _logger = logger;
        }

        // add message
        public async Task<bool> AddMessage(string text, string userId, string sendUserId)
        {
            return await PostRequest("/WebService1.asmx/AddMessage", new Dictionary<string, string>
            {
                { "text", text },
                { "userid", userId },
                { "senduserid", sendUserId },
                { "secret_key", _secretKey }
            });
        }

        // delete message
        public async Task<bool> DeleteMessage(string messageId, string userId)
        {
            return await PostRequest("/WebService1.asmx/DeleteMessage", new Dictionary<string, string>
            {
                { "messageid", messageId },
                { "userid", userId },
                { "secret_key", _secretKey }
            });
        }

        // get active messages
        public async Task<List<Messages>> GetActiveMessage(string userId)
        {
            return await GetRequest<List<Messages>>($"/WebService1.asmx/GetActiveMessage?userid={userId}&secret_key={_secretKey}");
        }

        // get messages
        public async Task<List<Messages>> GetUserMessage(string userId)
        {
            return await GetRequest<List<Messages>>($"/WebService1.asmx/GetUserMessage?userid={userId}&secret_key={_secretKey}");
        }

        // emoji
        public async Task<List<Emoji>> GetEmoji(string userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/WebService1.asmx/GetEmoji?userid={userId}&secret_key={_secretKey}");
                response.EnsureSuccessStatusCode();
                var xmlContent = await response.Content.ReadAsStringAsync();

                var xmlDoc = XDocument.Parse(xmlContent);
                var emojiString = xmlDoc.Root?.Value;

                return JsonSerializer.Deserialize<List<Emoji>>(emojiString);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching emojis: {ex.Message}");
                return new List<Emoji>();
            }
        }

        // message status
        public async Task<bool> SetMessageRead(string messageId, string userId)
        {
            return await PostRequest("/WebService1.asmx/SetMessageRead", new Dictionary<string, string>
            {
                { "messageid", messageId },
                { "userid", userId },
                { "secret_key", _secretKey }
            });
        }

        // all user
        public async Task<List<User>> GetAllUsers()
        {
            return await GetRequest<List<User>>($"/WebService1.asmx/GetAllUsers?secret_key={_secretKey}");
        }

        // Get helper
        private async Task<T> GetRequest<T>(string url)
        {
            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var xmlContent = await response.Content.ReadAsStringAsync();

                var xmlDoc = XDocument.Parse(xmlContent);
                var jsonString = xmlDoc.Root?.Value;

                return JsonSerializer.Deserialize<T>(jsonString);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetRequest: {ex.Message}");
                return default;
            }
        }

        // POST helper
        private async Task<bool> PostRequest(string url, Dictionary<string, string> parameters)
        {
            try
            {
                var content = new FormUrlEncodedContent(parameters);
                var response = await _httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    return bool.Parse(GetValueFromXml(result));
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in PostRequest: {ex.Message}");
                return false;
            }
        }

        // extract XML value
        private string GetValueFromXml(string xml)
        {
            try
            {
                var doc = new System.Xml.XmlDocument();
                doc.LoadXml(xml);
                return doc.DocumentElement?.InnerText ?? "false";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error parsing XML: {ex.Message}");
                return "false";
            }
        }
    }
}