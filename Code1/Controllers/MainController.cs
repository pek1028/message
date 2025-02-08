using System.Configuration;
using Code1.Models;
using Code1.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;

namespace Code1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebServiceController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly WebServiceClient _webServiceClient;
        private readonly ILogger<WebServiceController> _logger;

        public WebServiceController(WebServiceClient webServiceClient, ILogger<WebServiceController> logger, IConfiguration configuration)
        {
            _configuration = configuration;
            _webServiceClient = webServiceClient;
            _logger = logger;
        }

        [HttpPost("message")]
        public async Task<IActionResult> AddMessage([FromBody] AddMessageRequest request)
        {
            try
            {
                var result = await _webServiceClient.AddMessage(request.Text, request.UserId, request.SendUserId);
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error adding message: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while processing your request." });
            }
        }

        [HttpDelete("message/{messageId}/{userId}")]
        public async Task<IActionResult> DeleteMessage(string messageId, string userId)
        {
            try
            {
                var result = await _webServiceClient.DeleteMessage(messageId, userId);
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting message: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while processing your request." });
            }
        }

        [HttpGet("messages/active/{userId}")]
        public async Task<IActionResult> GetActiveMessages(string userId)
        {
            try
            {
                var messages = await _webServiceClient.GetActiveMessage(userId);
                return Ok(new { success = true, data = messages });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting active messages: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while processing your request." });
            }
        }

        [HttpGet("emojis/{userId}")]
        public async Task<IActionResult> GetEmojis(string userId)
        {
            try
            {
                var secretKey = _configuration["WebService:SecretKey"];

                var emojis = await _webServiceClient.GetEmoji(userId);
                if (emojis == null || !emojis.Any())
                {
                    _logger.LogWarning("No emojis found for user {UserId}", userId);
                }
                else
                {
                    _logger.LogInformation("Fetched {Count} emojis for user {UserId}", emojis.Count, userId);
                }
                return Ok(new { success = true, data = emojis });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting emojis: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while processing your request." });
            }
        }

        [HttpGet("messages/{userId}")]
        public async Task<IActionResult> GetUserMessages(string userId)
        {
            try
            {
                var messages = await _webServiceClient.GetUserMessage(userId);
                return Ok(new { success = true, data = messages });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting user messages: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while processing your request." });
            }
        }

        [HttpPost("message/read")]
        public async Task<IActionResult> SetMessageRead([FromBody] SetMessageReadRequest request)
        {
            try
            {
                var result = await _webServiceClient.SetMessageRead(request.MessageId, request.UserId);
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error setting message as read: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while processing your request." });
            }
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _webServiceClient.GetAllUsers();
                return Ok(new { success = true, data = users });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting all users: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while processing your request." });
            }
        }

        [HttpGet("checkUser")]
        public IActionResult CheckUser(string username)
        {
            bool isRegistered = true;
            var secretKey = _configuration["WebService:SecretKey"];
            return Ok(new { isRegistered, secretKey });
        }

        [HttpGet("test/{userId}")]
        public async Task<IActionResult> TestGetMessages(string userId)
        {
            try
            {
                var activeMessages = await _webServiceClient.GetActiveMessage(userId);
                var userMessages = await _webServiceClient.GetUserMessage(userId);

                return Ok(new
                {
                    success = true,
                    activeMessages = activeMessages,
                    userMessages = userMessages
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error testing messages: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while processing your request." });
            }
        }
    }

    public class AddMessageRequest
    {
        public string Text { get; set; }
        public string UserId { get; set; }
        public string SendUserId { get; set; }
    }

    public class SetMessageReadRequest
    {
        public string MessageId { get; set; }
        public string UserId { get; set; }
    }
}

