// Controllers/AdminRevenueController.cs
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecondHandSharing.Data;
using SecondHandSharing.Models.ViewModels;
using SecondHandSharing.Models;

namespace SecondHandSharing.Controllers
{
    [Authorize(Roles = "ADMIN")]
    public class AdminRevenueController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminRevenueController(ApplicationDbContext context)
        {
            _context = context;
        }
        [Authorize(Roles = "ADMIN")]
public async Task<IActionResult> PendingTopups()
{
    var pending = await _context.Transactions
        .Include(t => t.User)
        .Where(t => t.Type == "TopUp" && t.Status == "Pending")
        .OrderByDescending(t => t.CreatedAt)
        .ToListAsync();

    return View(pending);
}
[Authorize(Roles = "ADMIN")]
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> ApproveTopup(int id)
{
    var tran = await _context.Transactions
        .Include(t => t.User)
        .FirstOrDefaultAsync(t => t.TransactionId == id
                                  && t.Type == "TopUp"
                                  && t.Status == "Pending");

    if (tran == null)
    {
        TempData["ErrorMessage"] = "Không tìm thấy giao dịch nạp đang chờ duyệt.";
        return RedirectToAction(nameof(PendingTopups));
    }

    var user = tran.User ?? await _context.Users.FindAsync(tran.UserId);
    if (user == null)
    {
        TempData["ErrorMessage"] = "Không tìm thấy người dùng của giao dịch này.";
        return RedirectToAction(nameof(PendingTopups));
    }

    // ✅ Cộng tiền vào ví
    user.WalletBalance += tran.Amount;

    // ✅ Cập nhật trạng thái giao dịch
    tran.Status = "Success";
    tran.PaidAt = DateTime.Now;
    tran.Note = (tran.Note ?? "") + $" | Admin duyệt lúc {DateTime.Now:dd/MM/yyyy HH:mm}";

    await _context.SaveChangesAsync();

    TempData["SuccessMessage"] = $"Đã duyệt nạp {tran.Amount:N0}đ cho user {user.Email}.";
    return RedirectToAction(nameof(PendingTopups));
}

[Authorize(Roles = "ADMIN")]
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> RejectTopup(int id)
{
    var tran = await _context.Transactions
        .FirstOrDefaultAsync(t => t.TransactionId == id
                                  && t.Type == "TopUp"
                                  && t.Status == "Pending");

    if (tran == null)
    {
        TempData["ErrorMessage"] = "Không tìm thấy giao dịch nạp đang chờ duyệt.";
        return RedirectToAction(nameof(PendingTopups));
    }

    tran.Status = "Cancel";
    tran.Note = (tran.Note ?? "") + $" | Admin từ chối lúc {DateTime.Now:dd/MM/yyyy HH:mm}";

    await _context.SaveChangesAsync();

    TempData["SuccessMessage"] = $"Đã từ chối giao dịch #{tran.TransactionId}.";
    return RedirectToAction(nameof(PendingTopups));
}



        public async Task<IActionResult> Index(int? year, int? month, int? userId)
        {
            // ================================
            // 1. Base query: chỉ lấy giao dịch thành công
            // ================================
            var query = _context.Transactions
                .Include(t => t.User)
                .Include(t => t.Package)
                // Hỗ trợ cả "SUCCESS" và "Success" cho chắc
                .Where(t => t.Status == "SUCCESS" || t.Status == "Success");

            // ================================
            // 2. Áp dụng bộ lọc (năm / tháng / user)
            // ================================
            if (year.HasValue)
                query = query.Where(t => t.CreatedAt.Year == year.Value);

            if (month.HasValue)
                query = query.Where(t => t.CreatedAt.Month == month.Value);

            if (userId.HasValue)
                query = query.Where(t => t.UserId == userId.Value);

            // ================================
            // 3. Lấy danh sách giao dịch sau khi filter
            // ================================
            var transactions = await query
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            decimal total = transactions.Sum(t => t.Amount);

            // ================================
            // 4. Tính doanh thu theo từng user
            //    (dùng chính danh sách đã filter)
            // ================================
            var perUser = transactions
                .GroupBy(t => t.UserId)
                .Select(g => new UserRevenueDto
                {
                    UserId = g.Key,
                    Email = g.First().User?.Email,
                    FullName = g.First().User?.FullName,
                    TotalAmount = g.Sum(x => x.Amount),
                    Count = g.Count()
                })
                .OrderByDescending(x => x.TotalAmount)
                .ToList();

            // ================================
            // 5. Đổ vào ViewModel
            // ================================
            var vm = new AdminRevenueViewModel
            {
                Transactions = transactions,
                TotalRevenue = total,
                PerUserRevenue = perUser,
                SelectedYear = year,
                SelectedMonth = month,
                SelectedUserId = userId
            };

            return View(vm);
        }
        
    }
}
