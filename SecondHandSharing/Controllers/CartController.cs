// using Microsoft.AspNetCore.Mvc;
// using SecondHandSharing.Data;
// using SecondHandSharing.Models;

// namespace SecondHandSharing.Controllers
// {
//     public class CartController : Controller
//     {
//         private readonly ApplicationDbContext _context;

//         public CartController(ApplicationDbContext context)
//         {
//             _context = context;
//         }

//         // 🛒 Thêm vào giỏ hàng
//         [HttpPost]
//         public IActionResult AddToCart(int itemId, int userId)
//         {
//             var item = _context.Items.Find(itemId);
//             if (item == null) return NotFound();

//             var existingItem = _context.CartItems
//                 .FirstOrDefault(c => c.ItemId == itemId && c.UserId == userId);

//             if (existingItem != null)
//             {
//                 existingItem.Quantity++;
//             }
//             else
//             {
//                 var cartItem = new CartItem
//                 {
//                     ItemId = itemId,
//                     UserId = userId,
//                     Quantity = 1
//                 };
//                 _context.CartItems.Add(cartItem);
//             }

//             _context.SaveChanges();
//             TempData["Success"] = "Đã thêm vào giỏ hàng!";
//             return RedirectToAction("Index");
//         }

//         // 🧾 Hiển thị giỏ hàng
//         public IActionResult Index(int userId)
//         {
//             var cartItems = _context.CartItems
//                 .Where(c => c.UserId == userId)
//                 .Select(c => new
//                 {
//                     c.CartItemId,
//                     c.Quantity,
//                     Item = c.Item
//                 })
//                 .ToList();

//             return View(cartItems);
//         }

//         // ❌ Xóa sản phẩm khỏi giỏ hàng
//         [HttpPost]
//         public IActionResult Remove(int cartItemId)
//         {
//             var item = _context.CartItems.Find(cartItemId);
//             if (item == null) return NotFound();

//             _context.CartItems.Remove(item);
//             _context.SaveChanges();

//             TempData["Success"] = "Đã xóa sản phẩm khỏi giỏ hàng.";
//             return RedirectToAction("Index");
//         }
//     }
// }
