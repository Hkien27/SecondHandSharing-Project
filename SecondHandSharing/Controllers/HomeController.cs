using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecondHandSharing.Data;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
{
    DateTime now = DateTime.Now;

    // 🔥 Lấy danh sách tin nổi bật (Boosted)
    var boostedItems = _context.Items
        .Where(i => i.IsBoosted == true
                 && i.BoostExpireAt != null
                 && i.BoostExpireAt > now
                 && i.Status == "Đã duyệt"
                 && !i.IsSold)
        .OrderByDescending(i => i.BoostExpireAt)
        .Take(12)
        .ToList();

    ViewBag.BoostedItems = boostedItems;

    // ⭐ Sản phẩm mới nhất
    var newest = _context.Items
        .Where(i => i.Status == "Đã duyệt" && !i.IsSold)
        .OrderByDescending(i => i.CreatedAt)
        .Take(20)
        .ToList();

    return View("/Views/Item/Index.cshtml", newest);


}

}
