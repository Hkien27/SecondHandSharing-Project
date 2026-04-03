using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;


namespace SecondHandSharing.Models
{
    public class Item
    {
        [Key]
        public int ItemId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tiêu đề")]
        [StringLength(100)]
        public string Title { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mô tả")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá không hợp lệ")]
        [Precision(18, 2)]  
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Chọn danh mục")]
        public string Category { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tình trạng")]
        public string Condition { get; set; }
        public string? ProductDetail { get; set; }

        public string? Address { get; set; }

        public string? Status { get; set; }

        public string? Image { get; set; }
        public bool IsSold { get; set; } = false;
        public DateTime? SoldAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsFree { get; set; } = false;

        // Liên kết user đăng tin
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
        // === BOOST TIN ===
        public bool IsBoosted { get; set; } = false;
        public DateTime? BoostExpireAt { get; set; }
        [NotMapped]
        public IFormFile? ImageFile { get; set; }

        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

        public int? NamSanXuat { get; set; }   // VD: 2020
        public string? Loai { get; set; }       // VD: Xe máy
        public string? Hang { get; set; }       // VD: Honda
        // public virtual ICollection<ItemImage> Images { get; set; }

    }
}
