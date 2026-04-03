// using System.ComponentModel.DataAnnotations;
// using System.ComponentModel.DataAnnotations.Schema;

// namespace SecondHandSharing.Models
// {
//     public class CartItem
//     {
//         [Key]
//         public int Id { get; set; }

//         [ForeignKey("Item")]
//         public int ItemId { get; set; }
//         public Item Item { get; set; }

//         [ForeignKey("User")]
//         public string UserId { get; set; } = string.Empty; // 👈 Thêm dòng này
//         public ApplicationUser User { get; set; } // 👈 Và dòng này

//         public int Quantity { get; set; } = 1;
//     }
// }
