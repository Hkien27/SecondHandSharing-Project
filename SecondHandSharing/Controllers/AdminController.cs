using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecondHandSharing.Data;
using SecondHandSharing.Models;

namespace SecondHandSharing.Controllers
{
    [Authorize(Roles = "ADMIN")]   // 🔒 chỉ admin truy cập
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            var vm = new AdminDashboardViewModel
            {
                TotalUsers = await _context.Users.CountAsync(),
                TotalAdmins = await _context.Users.CountAsync(u => u.Role.ToUpper() == "ADMIN"),
                TotalItems = await _context.Items.CountAsync(),
                TotalActiveItems = await _context.Items.CountAsync(i => i.Status == "Đang bán"),
                TotalFavorites = _context.Favorites != null
                    ? await _context.Favorites.CountAsync()
                    : 0,
                TotalViewHistories = _context.ViewHistories != null
                    ? await _context.ViewHistories.CountAsync()
                    : 0,
                LatestUsers = await _context.Users
                    .OrderByDescending(u => u.CreatedAt)
                    .Take(5)
                    .ToListAsync(),
                LatestItems = await _context.Items
                    .Include(i => i.User)
                    .OrderByDescending(i => i.CreatedAt)
                    .Take(5)
                    .ToListAsync()
            };

            return View(vm);
        }
         public async Task<IActionResult> PendingItems()
    {
        var items = await _context.Items
            .Include(i => i.User)
            .Where(i => i.Status == "Chờ duyệt")
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

        return View(items);
    }

    // Duyệt tin
    [HttpPost]
    public async Task<IActionResult> ApproveItem(int id)
    {
        var item = await _context.Items.FindAsync(id);
        if (item == null) return NotFound();

        item.Status = "Đã duyệt";
        await _context.SaveChangesAsync();

        TempData["Success"] = "Đã duyệt tin.";
        return RedirectToAction("PendingItems");
    }
[Authorize(Roles = "ADMIN")] // Đảm bảo chỉ Admin mới vào được
public async Task<IActionResult> ManageUsers()
{
    var users = await _context.Users
        .Select(u => new UserManagementViewModel
        {
            UserId = u.UserId,
            UserName = u.FullName,
            Email = u.Email,
            IsLocked = u.IsLocked,
            // Tính doanh thu: Tổng giá các sản phẩm đã bán
            TotalRevenue = _context.Items
                .Where(i => i.UserId == u.UserId && i.IsSold)
                .Sum(i => (decimal?)i.Price) ?? 0,
            // Lấy danh sách sản phẩm đã bán
            SoldItems = _context.Items
                .Where(i => i.UserId == u.UserId && i.IsSold)
                .ToList()
        }).ToListAsync();

    return View(users);
}

// Hàm xử lý Khóa/Mở khóa
[HttpPost]
public async Task<IActionResult> ToggleLock(int id)
{
    var user = await _context.Users.FindAsync(id);
    if (user == null) return NotFound();

    user.IsLocked = !user.IsLocked; // Đảo ngược trạng thái
    await _context.SaveChangesAsync();

    TempData["SuccessMessage"] = user.IsLocked ? "Đã khóa tài khoản!" : "Đã mở khóa tài khoản!";
    return RedirectToAction(nameof(ManageUsers));
}
    // Từ chối tin
    [HttpPost]
    public async Task<IActionResult> RejectItem(int id)
    {
        var item = await _context.Items.FindAsync(id);
        if (item == null) return NotFound();

        item.Status = "Bị từ chối";
        await _context.SaveChangesAsync();

        TempData["Success"] = "Đã từ chối tin.";
        return RedirectToAction("PendingItems");
    }
    
    }
}
