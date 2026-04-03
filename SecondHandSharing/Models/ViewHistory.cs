using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SecondHandSharing.Models
{
    public class ViewHistory
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }
        public int ItemId { get; set; }
        public DateTime ViewedAt { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [ForeignKey("ItemId")]
        public virtual Item? Item { get; set; }
        
    }
}
