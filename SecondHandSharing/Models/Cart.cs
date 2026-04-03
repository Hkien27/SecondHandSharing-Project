// using System.Collections.Generic;
// using System.ComponentModel.DataAnnotations;
// using System.ComponentModel.DataAnnotations.Schema;

// namespace SecondHandSharing.Models
// {
//     public class Cart
//     {
//         [Key]
//         public int CartId { get; set; }

//         [ForeignKey("User")]
//         public int UserId { get; set; }
//         public User User { get; set; }

//         public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
//     }
// }
