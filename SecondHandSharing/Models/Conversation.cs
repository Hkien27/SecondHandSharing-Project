using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SecondHandSharing.Models
{
    public class Conversation
    {
        [Key]
        public int ConversationId { get; set; }

        // Người tham gia 1
        public int User1Id { get; set; }

        // Người tham gia 2
        public int User2Id { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("User1Id")]
        public User? User1 { get; set; }

        [ForeignKey("User2Id")]
        public User? User2 { get; set; }

        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
