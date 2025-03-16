using System;

namespace InstantMessenger.Models
{
    public class Message
    {
        public string Sender { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsFromMe { get; set; }

        public Message(string sender, string content, bool isFromMe = false)
        {
            Sender = sender;
            Content = content;
            Timestamp = DateTime.Now;
            IsFromMe = isFromMe;
        }

        public override string ToString()
        {
            return IsFromMe ? $"Tú: {Content}" : $"{Sender}: {Content}";
        }
    }
}