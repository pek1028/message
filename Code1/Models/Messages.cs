using System;
using System.Text.Json.Serialization;

namespace Code1.Models
{
    public class Messages
    {
        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("emoji")]
        public string Emoji { get; set; } 

        [JsonPropertyName("from_user")]
        public string FromUser { get; set; }

        [JsonPropertyName("to_user")]
        public string ToUser { get; set; }

        [JsonPropertyName("isRead")]
        public bool IsRead { get; set; }

        [JsonPropertyName("createdate")]
        public string CreateDate { get; set; }

        [JsonPropertyName("unReadMsgCount")]
        public int UnReadMsgCount { get; set; }

        [JsonPropertyName("bValid")]
        public bool BValid { get; set; }

        [JsonPropertyName("Error")]
        public string? Error { get; set; }
    }
}
