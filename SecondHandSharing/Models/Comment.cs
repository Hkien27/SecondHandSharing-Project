using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SecondHandSharing.Models
{
    public class Comment
    {
        [Key]
        public int CommentId { get; set; }

        // Sản phẩm được bình luận
        [Required]
        public int ItemId { get; set; }

        // Người bình luận
        [Required]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung bình luận")]
        [StringLength(500)]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Điều hướng
        [ForeignKey("ItemId")]
        public virtual Item? Item { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}
