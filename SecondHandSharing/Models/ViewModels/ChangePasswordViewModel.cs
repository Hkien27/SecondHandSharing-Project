// using System.ComponentModel.DataAnnotations;

// namespace SecondHandSharing.Models.ViewModels
// {
//     public class ChangePasswordViewModel
//     {
//         [Required]
//         [DataType(DataType.Password)]
//         [Display(Name = "Mật khẩu hiện tại")]
//         public string CurrentPassword { get; set; }

//         [Required]
//         [DataType(DataType.Password)]
//         [MinLength(5, ErrorMessage = "Mật khẩu mới phải >= 5 ký tự")]
//         [Display(Name = "Mật khẩu mới")]
//         public string NewPassword { get; set; }

//         [Required]
//         [DataType(DataType.Password)]
//         [Compare("NewPassword", ErrorMessage = "Mật khẩu nhập lại không khớp")]
//         [Display(Name = "Nhập lại mật khẩu mới")]
//         public string ConfirmPassword { get; set; }
//     }
// }
