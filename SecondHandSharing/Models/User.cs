using System;
using System.ComponentModel.DataAnnotations;

namespace SecondHandSharing.Models {
    public class User {
        [Key]
        public int UserId { get; set; }

        [Required, StringLength(100)]
        public string FullName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        public string PasswordHash { get; set; }
         public string? PhoneNumber { get; set; }  
         public string? AvatarPath { get; set; } = "/images/avatar_default.png";
        public string Role { get; set; } = "USER";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        // === VIP ===
        public bool IsVip { get; set; } = false;
        public DateTime? VipExpireAt { get; set; }

        public bool IsLocked { get; set; } = false;
        public decimal WalletBalance { get; set; } = 0;

        
    }
}
