using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SecondHandSharing.Models
{
    public class Message
    {
        [Key]
        public int MessageId { get; set; }

        public int ConversationId { get; set; }

        public int SenderId { get; set; }   // User gửi

        [Required]
        [StringLength(1000)]
        public string Content { get; set; }

        public DateTime SentAt { get; set; } = DateTime.Now;

        public bool IsRead { get; set; } = false;
        public string? ImagePath { get; set; }
        
        [ForeignKey("ConversationId")]
        public Conversation? Conversation { get; set; }

        [ForeignKey("SenderId")]
        public User? Sender { get; set; }
    }
}
