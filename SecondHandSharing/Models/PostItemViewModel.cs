using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace SecondHandSharing.Models
{
    public class PostItemViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mô tả sản phẩm")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        public string Category { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn tình trạng sản phẩm")]
        public string Condition { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập chi tiết sản phẩm")]
        [Display(Name = "Chi tiết sản phẩm")]
        public string ProductDetail { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá")]
        [Range(1, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
        public string Address { get; set; }

        [Display(Name = "Ảnh sản phẩm")]
        public IFormFile ImageFile { get; set; }
    }
}
