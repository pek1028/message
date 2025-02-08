namespace Code1.Models
{
    public class Emoji
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; }
        public string AnimationUrl { get; set; }
        public string EmojiName { get; set; }
        public bool BValid { get; set; }
        public string Error { get; set; }
    }
}

