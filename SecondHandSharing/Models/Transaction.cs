using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using Microsoft.EntityFrameworkCore;
namespace SecondHandSharing.Models
{
    public class Transaction
    {
        // [Key]
        // public int TransactionId { get; set; }

        // public int UserId { get; set; }
        
        // public int? ItemId { get; set; }
        // public int? PackageId { get; set; }

        // [Column(TypeName = "decimal(18,2)")]   // ← Không lỗi, mọi .NET đều hỗ trợ
        // public decimal Amount { get; set; }
        // [StringLength(20)]
        // public string PaymentMethod { get; set; } = "WALLET";   // WALLET / MOMO / ...

        // [StringLength(20)]
        // public string Status { get; set; } = "Success";        // Success / Pending / Failed

        // public DateTime? PaidAt { get; set; }
                             
        // [MaxLength(20)]
        // public string Type { get; set; } = default!;           // "TopUp" | "VIP" | "Boost"
        // public DateTime CreatedAt { get; set; }
        
        
        // public virtual User User { get; set; }
        // public virtual Item? Item { get; set; }
        // public ServicePackage? Package { get; set; }
        // [StringLength(200)]
        // public string? Note { get; set; }
        // // ✅ THÊM PROPERTY NÀY
        // [StringLength(200)]
        // public string? Description { get; set; }
         [Key]
        public int TransactionId { get; set; }

        // User thực hiện giao dịch
        public int UserId { get; set; }

        public int? PackageId { get; set; }   // VIP / BOOST (nếu có)
        public int? ItemId { get; set; }      // Tin được boost (nếu có)

        [Precision(18, 2)]
        public decimal Amount { get; set; }

        /// <summary>
        /// TopUp / VIP / Boost
        /// </summary>
        [MaxLength(50)]
        public string Type { get; set; } = "TopUp";

        /// <summary>
        /// MOMO_MANUAL / WALLET ...
        /// </summary>
        [MaxLength(50)]
        public string PaymentMethod { get; set; } = "MOMO_MANUAL";

        /// <summary>
        /// PENDING / SUCCESS / FAILED
        /// </summary>
        [MaxLength(20)]
        public string Status { get; set; } = "PENDING";

        // 🔹 Dùng cho thanh toán MoMo thủ công
        [MaxLength(50)]
        public string? PaymentCode { get; set; }   // nội dung CK để đối chiếu

        [MaxLength(200)]
        public string? QrImageUrl { get; set; }    // link ảnh QR

        [MaxLength(200)]
        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? PaidAt { get; set; }

        // NAVIGATION
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("PackageId")]
        public virtual ServicePackage? Package { get; set; }

        [ForeignKey("ItemId")]
        public virtual Item? Item { get; set; }
    }
}
