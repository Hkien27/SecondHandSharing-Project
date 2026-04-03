using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace SecondHandSharing.Models
{
    public class ServicePackage
    {
        [Key]                    // ✅ KHÓA CHÍNH
        public int PackageId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;   // Ví dụ: "Boost 1 ngày"

        [Required]
        [StringLength(20)]
        public string Type { get; set; } = string.Empty;   // "BOOST" hoặc "VIP"

        [Precision(18, 2)]
        public decimal Price { get; set; }                 // Giá gói

        public int DurationDays { get; set; }              // Số ngày hiệu lực

        public bool IsActive { get; set; } = true;
        
        [StringLength(200)]
        public string? Description { get; set; }

        // Navigation
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
