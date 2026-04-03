using System.ComponentModel.DataAnnotations;

namespace SecondHandSharing.Models.ViewModels
{
    public class UserAccountViewModel
    {
        // ===== THÔNG TIN TÀI KHOẢN =====
        public int UserId { get; set; }

        [Required, StringLength(100)]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; }

        [Required, EmailAddress]
        [Display(Name = "Email (không thể đổi)")]
        public string Email { get; set; }

        [Display(Name = "Số điện thoại")]
        public string? PhoneNumber { get; set; }
        public string? AvatarPath { get; set; }
        // ===== ĐỔI MẬT KHẨU =====
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu hiện tại")]
        public string? CurrentPassword { get; set; }

        [DataType(DataType.Password)]
        [MinLength(5)]
        [Display(Name = "Mật khẩu mới")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu nhập lại không khớp")]
        [Display(Name = "Nhập lại mật khẩu mới")]
        public string? ConfirmPassword { get; set; }
        public IFormFile? AvatarFile { get; set; }

    }
}
