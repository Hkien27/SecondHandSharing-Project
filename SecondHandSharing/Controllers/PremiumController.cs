using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecondHandSharing.Data;
using SecondHandSharing.Models;
using SecondHandSharing.Models.ViewModels;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SecondHandSharing.Controllers
{
    [Authorize]
    public class PremiumController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PremiumController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ===============================
        // HELPER – Lấy UserId
        // ===============================
        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null)
                throw new Exception("User not logged in");

            return int.Parse(claim.Value);
        }

        // ===============================
        // 1. TRANG VÍ
        // ===============================
        public async Task<IActionResult> Wallet()
        {
            int userId = GetCurrentUserId();
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            return View(user);
        }

        // ===============================
        // 1.1 NẠP TIỀN TRỰC TIẾP (GIẢ LẬP)
        // ===============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TopUp(decimal amount)
        {
            if (amount <= 0)
            {
                TempData["ErrorMessage"] = "Số tiền nạp phải lớn hơn 0.";
                return RedirectToAction(nameof(Wallet));
            }

            int userId = GetCurrentUserId();
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            user.WalletBalance += amount;

            var tran = new Transaction
            {
                UserId = userId,
                Amount = amount,
                PaymentMethod = "WALLET_TOPUP",
                Status = "SUCCESS",
                CreatedAt = DateTime.Now,
                PaidAt = DateTime.Now,
                Note = $"Nạp ví {amount:N0}đ (giả lập)",
                Type = "TopUp"
            };

            _context.Transactions.Add(tran);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Nạp tiền thành công!";
            return RedirectToAction(nameof(Wallet));
        }

        // ===============================
        // 1.2 NẠP TIỀN QUA MOMO THỦ CÔNG
        // ===============================

        // GET: form nhập số tiền
        [HttpGet]
        public IActionResult TopUpMomo()
        {
            return View();
        }

        // POST: tạo transaction PENDING + chuyển tới trang QR
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TopUpMomo(decimal amount)
        {
            if (amount < 10000)
            {
                TempData["ErrorMessage"] = "Số tiền tối thiểu là 10.000đ.";
                return RedirectToAction(nameof(TopUpMomo));
            }

            int userId = GetCurrentUserId();

            // Random nội dung chuyển khoản để admin đối chiếu
            string paymentCode = "PAY" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();

            var tran = new Transaction
            {
                UserId = userId,
                Amount = amount,
                Type = "TopUp",
                Status = "PENDING",
                PaymentMethod = "MOMO_MANUAL",
                CreatedAt = DateTime.Now,
                PaymentCode = paymentCode,
                QrImageUrl = "/images/qr-momo-demo.png", // nhớ có file này trong wwwroot/images
                Note = $"Nạp ví qua MoMo thủ công - {paymentCode}"
            };

            _context.Transactions.Add(tran);
            await _context.SaveChangesAsync();

            // chuyển sang trang hiển thị QR + thông tin CK
            return RedirectToAction(nameof(PayMomo), new { id = tran.TransactionId });
        }

        // Trang hiển thị QR + nội dung chuyển khoản cho giao dịch đã tạo
        [HttpGet]
        public async Task<IActionResult> PayMomo(int id)
        {
            var tran = await _context.Transactions
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.TransactionId == id);

            if (tran == null) return NotFound();

            return View(tran);
        }

        // ===============================
        // 2. MUA GÓI VIP (TRỪ VÀO VÍ)
        // ===============================
        [HttpGet]
        public async Task<IActionResult> BuyVip()
        {
            var packages = await _context.ServicePackages
                .Where(p => p.Type == "VIP")
                .OrderBy(p => p.Price)
                .ToListAsync();

            return View(packages);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BuyVip(int packageId)
        {
            int userId = GetCurrentUserId();
            var user = await _context.Users.FindAsync(userId);
            var package = await _context.ServicePackages
                .FirstOrDefaultAsync(p => p.PackageId == packageId && p.Type == "VIP");

            if (user == null || package == null)
            {
                TempData["ErrorMessage"] = "Gói không hợp lệ.";
                return RedirectToAction(nameof(BuyVip));
            }

            if (user.WalletBalance < package.Price)
            {
                TempData["ErrorMessage"] = "Số dư ví không đủ.";
                return RedirectToAction(nameof(Wallet));
            }

            user.WalletBalance -= package.Price;

            var now = DateTime.Now;
            var baseDate = (user.VipExpireAt.HasValue && user.VipExpireAt.Value > now)
                ? user.VipExpireAt.Value
                : now;

            user.IsVip = true;
            user.VipExpireAt = baseDate.AddDays(package.DurationDays);

            var tran = new Transaction
            {
                UserId = userId,
                PackageId = package.PackageId,
                Amount = package.Price,
                Type = "VIP",
                PaymentMethod = "WALLET",
                Status = "SUCCESS",
                CreatedAt = now,
                PaidAt = now,
                Note = $"Mua gói VIP: {package.Name}"
            };

            _context.Transactions.Add(tran);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Mua VIP thành công!";
            return RedirectToAction(nameof(BuyVip));
        }

        // ===============================
        // 3. MUA GÓI BOOST CHO SẢN PHẨM
        // ===============================
        [HttpGet]
        public async Task<IActionResult> BuyBoost()
        {
            int userId = GetCurrentUserId();

            var items = await _context.Items
                .Where(i => i.UserId == userId && !i.IsSold && i.Status == "Đã duyệt")
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();

            var packages = await _context.ServicePackages
                .Where(p => p.Type == "BOOST")
                .OrderBy(p => p.Price)
                .ToListAsync();

            var vm = new BuyBoostViewModel
            {
                MyItems = items,
                BoostPackages = packages
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BuyBoost(int itemId, int packageId)
        {
            int userId = GetCurrentUserId();
            var user = await _context.Users.FindAsync(userId);

            var item = await _context.Items
                .FirstOrDefaultAsync(i => i.ItemId == itemId && i.UserId == userId);

            var package = await _context.ServicePackages
                .FirstOrDefaultAsync(p => p.PackageId == packageId && p.Type == "BOOST");

            if (user == null || item == null || package == null)
            {
                TempData["ErrorMessage"] = "Dữ liệu không hợp lệ.";
                return RedirectToAction(nameof(BuyBoost));
            }

            if (user.WalletBalance < package.Price)
            {
                TempData["ErrorMessage"] = "Số dư ví không đủ.";
                return RedirectToAction(nameof(Wallet));
            }

            user.WalletBalance -= package.Price;

            var now = DateTime.Now;
            var baseDate = (item.BoostExpireAt.HasValue && item.BoostExpireAt.Value > now)
                ? item.BoostExpireAt.Value
                : now;

            item.IsBoosted = true;
            item.BoostExpireAt = baseDate.AddDays(package.DurationDays);

            var tran = new Transaction
            {
                UserId = userId,
                ItemId = item.ItemId,
                PackageId = package.PackageId,
                Amount = package.Price,
                Type = "Boost",
                PaymentMethod = "WALLET",
                Status = "SUCCESS",
                CreatedAt = now,
                PaidAt = now,
                Note = $"Đẩy tin #{item.ItemId} với gói {package.Name}"
            };

            _context.Transactions.Add(tran);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đẩy tin thành công!";
            return RedirectToAction(nameof(BuyBoost));
        }

        // ===============================
        // 4. LỊCH SỬ GIAO DỊCH CỦA USER
        // ===============================
        [HttpGet]
        public async Task<IActionResult> MyTransactions()
        {
            int userId = GetCurrentUserId();

            var list = await _context.Transactions
                .Include(t => t.Package)
                .Include(t => t.Item)
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return View(list);
        }
    }
}
