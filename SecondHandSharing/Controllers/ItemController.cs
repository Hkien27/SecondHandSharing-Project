using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecondHandSharing.Data;
using SecondHandSharing.Models;
using SecondHandSharing.Models.ViewModels;
using System.Security.Claims;

namespace SecondHandSharing.Controllers
{
    [Authorize] // ✅ yêu cầu đăng nhập cho toàn bộ controller (trừ chỗ có [AllowAnonymous])
    public class ItemController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ItemController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // =========================================
        // ✅ TRANG CHỦ: HIỂN THỊ SẢN PHẨM ĐÃ DUYỆT & CHƯA BÁN
        // =========================================
        [AllowAnonymous]
        public IActionResult Index()
        {
            var now = DateTime.Now;
            var items = _context.Items
        .Include(i => i.User)
        .Where(i => i.Status == "Đã duyệt" && !i.IsSold)
        .OrderByDescending(i => i.IsBoosted && i.BoostExpireAt > now)          // 1. Tin boost
        .ThenByDescending(i => i.User.IsVip && i.User.VipExpireAt > now)       // 2. User VIP
        .ThenByDescending(i => i.CreatedAt)                                    // 3. Tin mới
        .ToList();

            return View(items);
        }

        // =========================================
        // ✅ CHI TIẾT SẢN PHẨM + LỊCH SỬ XEM + SẢN PHẨM LIÊN QUAN
        // =========================================
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            // 🔍 Tìm sản phẩm theo ID
            var item = await _context.Items
                .Include(i => i.User)
                .Include(i => i.Comments)
                    .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(i => i.ItemId == id);

            if (item == null)
            {
                TempData["ErrorMessage"] = "❌ Không tìm thấy sản phẩm.";
                return RedirectToAction(nameof(Index));
            }

            // Sản phẩm liên quan (cùng category, đã duyệt, chưa bán, khác id)
            var related = await _context.Items
                .Where(i => i.Category == item.Category
                            && i.ItemId != item.ItemId
                            && i.Status == "Đã duyệt"
                            && !i.IsSold)
                .OrderByDescending(i => i.CreatedAt)
                .Take(6)
                .ToListAsync();

            ViewBag.RelatedItems = related;

            // ✅ Nếu người dùng đã đăng nhập → ghi lịch sử xem
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null)
            {
                int userId = int.Parse(userIdClaim.Value);

                var existing = await _context.ViewHistories
                    .FirstOrDefaultAsync(v => v.UserId == userId && v.ItemId == item.ItemId);

                if (existing != null)
                {
                    // Cập nhật thời gian xem gần nhất
                    existing.ViewedAt = DateTime.Now;
                    _context.ViewHistories.Update(existing);
                }
                else
                {
                    // Thêm bản ghi mới
                    var history = new ViewHistory
                    {
                        UserId = userId,
                        ItemId = item.ItemId,
                        ViewedAt = DateTime.Now
                    };
                    _context.ViewHistories.Add(history);
                }

                await _context.SaveChangesAsync();
            }

            return View(item);
        }

        // ĐỒ GIA DỤNG
[AllowAnonymous]
public IActionResult GiaDung(string loai, string hang, string condition, decimal? minPrice, decimal? maxPrice, string searchTerm)
{
    var now = DateTime.Now;
    // Lấy các tin thuộc category GiaDung
    var query = _context.Items
        .Where(i => i.Category == "GiaDung" && i.Status == "Đã duyệt" && !i.IsSold)
        .AsQueryable();

    if (!string.IsNullOrEmpty(searchTerm))
        query = query.Where(i => i.Title.Contains(searchTerm) || i.Description.Contains(searchTerm));

    if (!string.IsNullOrEmpty(loai))
        query = query.Where(i => i.Loai == loai);

    if (!string.IsNullOrEmpty(hang))
        query = query.Where(i => i.Hang == hang);

    if (!string.IsNullOrEmpty(condition))
        query = query.Where(i => i.Condition == condition);

    if (minPrice.HasValue) query = query.Where(i => i.Price >= minPrice.Value);
    if (maxPrice.HasValue) query = query.Where(i => i.Price <= maxPrice.Value);

    var items = query
        .OrderByDescending(i => i.IsBoosted && i.BoostExpireAt > now)
        .ThenByDescending(i => i.User.IsVip && i.User.VipExpireAt > now)
        .ThenByDescending(i => i.CreatedAt)
        .ToList();

    ViewBag.CategoryName = "Đồ Gia Dụng & Thời Trang";
    return View("Category/GiaDung", items);
}

// NỘI THẤT
[AllowAnonymous]
public IActionResult NoiThat()
{
    var now = DateTime.Now;
    var items = _context.Items
        .Where(i => i.Category == "NoiThat"
                    && i.Status == "Đã duyệt"
                    && !i.IsSold)
        .OrderByDescending(i => i.IsBoosted && i.BoostExpireAt > now)
        .ThenByDescending(i => i.User.IsVip && i.User.VipExpireAt > now)
        .ThenByDescending(i => i.CreatedAt)
        .ToList();

    ViewBag.CategoryName = "Nội thất";
    ViewBag.Banner = "/images/banner_noithat.jpg";
    return View("Category", items);
}

// THỜI TRANG
[AllowAnonymous]
public IActionResult ThoiTrang(string loai, string hang, string condition, decimal? minPrice, decimal? maxPrice, string searchTerm)
{
    var now = DateTime.Now;
    // Lọc theo Category "ThoiTrang"
    var query = _context.Items
        .Where(i => i.Category == "ThoiTrang" && i.Status == "Đã duyệt" && !i.IsSold)
        .AsQueryable();

    if (!string.IsNullOrEmpty(searchTerm))
        query = query.Where(i => i.Title.Contains(searchTerm));

    if (!string.IsNullOrEmpty(loai))
        query = query.Where(i => i.Loai == loai);

    if (!string.IsNullOrEmpty(hang))
        query = query.Where(i => i.Hang == hang);

    if (minPrice.HasValue) query = query.Where(i => i.Price >= minPrice.Value);
    if (maxPrice.HasValue) query = query.Where(i => i.Price <= maxPrice.Value);

    var items = query
        .OrderByDescending(i => i.IsBoosted && i.BoostExpireAt > now)
        .ThenByDescending(i => i.CreatedAt)
        .ToList();

    ViewBag.CategoryName = "Thời Trang & Phụ Kiện";
    return View("Category/ThoiTrang", items);
}

// TẶNG MIỄN PHÍ – dùng IsFree, không dùng Category
[AllowAnonymous]
public IActionResult TangMienPhi()
{
    var now = DateTime.Now;
    var items = _context.Items
        .Where(i => i.IsFree
                    && i.Status == "Đã duyệt"
                    && !i.IsSold)
        .OrderByDescending(i => i.IsBoosted && i.BoostExpireAt > now)
        .ThenByDescending(i => i.User.IsVip && i.User.VipExpireAt > now)
        .ThenByDescending(i => i.CreatedAt)
        .ToList();

    ViewBag.CategoryName = "Tặng miễn phí";
    ViewBag.Banner = "/images/banner_tangmienphi.jpg";
    return View("Category", items);
}

// XE CỘ
[AllowAnonymous]
// Trong ItemController.cs

[HttpGet]
public async Task<IActionResult> XeCo(string loai, string hang, decimal? minPrice, decimal? maxPrice)
{
    // 1. Lấy dữ liệu gốc: Danh mục XeCo và Trạng thái Đã duyệt
    var query = _context.Items.Where(i => i.Category == "XeCo" && i.Status == "Đã duyệt");

    // 2. Lọc theo Loại (Xe máy, Ô tô...) nếu có
    if (!string.IsNullOrEmpty(loai))
    {
        query = query.Where(i => i.Loai == loai);
    }

    // 3. Lọc theo Hãng nếu có
    if (!string.IsNullOrEmpty(hang))
    {
        query = query.Where(i => i.Hang == hang);
    }

    // 4. Lọc theo giá
    if (minPrice.HasValue) query = query.Where(i => i.Price >= minPrice);
    if (maxPrice.HasValue) query = query.Where(i => i.Price <= maxPrice);

    // 5. Sắp xếp: Tin ưu tiên (IsBoosted) lên đầu, sau đó đến tin mới nhất
    var items = await query
        .OrderByDescending(i => i.IsBoosted)
        .ThenByDescending(i => i.CreatedAt)
        .ToListAsync();

    // 6. Trả về đúng đường dẫn View bạn đã tạo
    return View("Category/XeCo", items);
}


// THÚ CƯNG
[AllowAnonymous]
public IActionResult ThuCung(string loai, string giong, string condition, decimal? minPrice, decimal? maxPrice, string searchTerm)
{
    var now = DateTime.Now;

    // 1. Khởi tạo Query lấy danh mục Thú cưng
    var query = _context.Items
        .Where(i => i.Category == "ThuCung" && i.Status == "Đã duyệt" && !i.IsSold)
        .AsQueryable();

    // 2. Lọc theo từ khóa tìm kiếm (Search Term)
    if (!string.IsNullOrEmpty(searchTerm))
    {
        query = query.Where(i => i.Title.Contains(searchTerm) || i.Description.Contains(searchTerm));
    }

    // 3. Lọc theo Loại (Chó, Mèo, Chim...)
    if (!string.IsNullOrEmpty(loai))
    {
        query = query.Where(i => i.Loai == loai);
    }

    // 4. Lọc theo Giống (Poodle, Phốc, Mèo Anh...)
    // Lưu ý: Nếu trong DB bạn đặt tên cột là 'Hang' thì sửa i.Hang == giong
    if (!string.IsNullOrEmpty(giong))
    {
        query = query.Where(i => i.Hang == giong); 
    }

    // 5. Lọc theo Tình trạng (Mới/Cũ)
    if (!string.IsNullOrEmpty(condition))
    {
        query = query.Where(i => i.Condition == condition);
    }

    // 6. Lọc theo Khoảng giá
    if (minPrice.HasValue)
    {
        query = query.Where(i => i.Price >= minPrice.Value);
    }
    if (maxPrice.HasValue)
    {
        query = query.Where(i => i.Price <= maxPrice.Value);
    }

    // 7. Sắp xếp ưu tiên: Tin đẩy -> VIP -> Tin mới nhất
    var items = query
        .OrderByDescending(i => i.IsBoosted && i.BoostExpireAt > now)
        .ThenByDescending(i => i.User.IsVip && i.User.VipExpireAt > now)
        .ThenByDescending(i => i.CreatedAt)
        .ToList();

    // 8. Truyền dữ liệu bổ sung ra View
    ViewBag.CategoryName = "Thú cưng";
    ViewBag.Banner = "/images/banner_thucung.jpg";
    
    // Nếu bạn dùng chung View "Category" thì đảm bảo View đó có code hiển thị hãng/giống
    return View("Category/ThuCung", items);
}
[AllowAnonymous]
public IActionResult DoDien(string loai, string hang, string condition, decimal? minPrice, decimal? maxPrice, string searchTerm)
{
    var now = DateTime.Now;
    // Lọc theo Category "DoDien"
    var query = _context.Items
        .Where(i => i.Category == "DoDien" && i.Status == "Đã duyệt" && !i.IsSold)
        .AsQueryable();

    if (!string.IsNullOrEmpty(searchTerm))
        query = query.Where(i => i.Title.Contains(searchTerm) || i.Description.Contains(searchTerm));

    if (!string.IsNullOrEmpty(loai))
        query = query.Where(i => i.Loai == loai);

    if (!string.IsNullOrEmpty(hang))
        query = query.Where(i => i.Hang == hang);

    if (minPrice.HasValue) query = query.Where(i => i.Price >= minPrice.Value);
    if (maxPrice.HasValue) query = query.Where(i => i.Price <= maxPrice.Value);

    var items = query
        .OrderByDescending(i => i.IsBoosted && i.BoostExpireAt > now)
        .ThenByDescending(i => i.CreatedAt)
        .ToList();

    ViewBag.CategoryName = "Đồ Điện Tử & Công Nghệ";
    return View("Category/DoDien", items);
}


        // =========================================
        // ✅ TÌM KIẾM SẢN PHẨM
        // =========================================
        [HttpGet, AllowAnonymous]
        public IActionResult Search(string keyword)
        {
            var now = DateTime.Now;
            if (string.IsNullOrEmpty(keyword))
                return RedirectToAction(nameof(Index));

            var items = _context.Items
                .Where(i =>
                    (i.Title.Contains(keyword) || i.Description.Contains(keyword))
                    && i.Status == "Đã duyệt"
                    && !i.IsSold)
                .OrderByDescending(i => i.IsBoosted && i.BoostExpireAt > now)
        .ThenByDescending(i => i.User.IsVip && i.User.VipExpireAt > now)
        .ThenByDescending(i => i.CreatedAt)
        .ToList();

            ViewBag.Keyword = keyword;
            return View("SearchResult", items);
        }

        // Gợi ý tìm kiếm (AJAX)
        [HttpGet, AllowAnonymous]
        public JsonResult SearchSuggestions(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return Json(new List<string>());

            var suggestions = _context.Items
                .Where(i => i.Title.Contains(keyword)
                            && i.Status == "Đã duyệt"
                            && !i.IsSold)
                .Select(i => i.Title)
                .Take(5)
                .ToList();

            return Json(suggestions);
        }

        // =========================================
        // ✅ ĐĂNG TIN
        // =========================================
        [HttpGet]
        public IActionResult PostItem()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
public async Task<IActionResult> PostItem(Item model, IFormFile? ImageFile)
{
    if (!ModelState.IsValid)
    {
        TempData["ErrorMessage"] = "⚠️ Vui lòng nhập đầy đủ thông tin hợp lệ.";
        return View(model);
    }

    try
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            TempData["ErrorMessage"] = "⚠️ Bạn cần đăng nhập để đăng sản phẩm.";
            return RedirectToAction("Login", "Account");
        }

        // 🖼️ Lưu ảnh (giữ code cũ)
        string? imagePath = "/images/no-image.png";
        if (ImageFile != null && ImageFile.Length > 0)
        {
            string uploadFolder = Path.Combine(_env.WebRootPath, "images");
            if (!Directory.Exists(uploadFolder))
                Directory.CreateDirectory(uploadFolder);

            string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
            string filePath = Path.Combine(uploadFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await ImageFile.CopyToAsync(fileStream);
            }

            imagePath = "/images/" + uniqueFileName;
        }

        // 🧾 Gán thông tin
        model.Image = imagePath;
        model.CreatedAt = DateTime.Now;
        model.Status = "Chờ duyệt";
        model.UserId = int.Parse(userIdClaim.Value);

        // 👉 Nếu là đồ miễn phí → giá = 0
        if (model.IsFree)
        {
            model.Price = 0;
        }

        _context.Items.Add(model);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "✅ Đăng tin thành công, vui lòng chờ admin duyệt.";
        return RedirectToAction(nameof(MyItems));
    }
    catch (Exception ex)
    {
        TempData["ErrorMessage"] = "❌ Lỗi khi lưu sản phẩm: " + ex.Message;
        return View(model);
    }
}


        // =========================================
        // ✅ TRANG QUẢN LÝ TIN ĐĂNG CỦA TÔI (APPROVED / PENDING / REJECTED)
        // =========================================
        public IActionResult MyItems()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                TempData["ErrorMessage"] = "⚠️ Bạn cần đăng nhập để xem sản phẩm của mình.";
                return RedirectToAction("Login", "Account");
            }

            int currentUserId = int.Parse(userIdClaim.Value);

            var allItems = _context.Items
                .Where(i => i.UserId == currentUserId)
                .OrderByDescending(i => i.CreatedAt)
                .ToList();

            var vm = new MyItemsViewModel
            {
                // Tin đã duyệt & chưa bán
                ApprovedItems = allItems
                    .Where(i => (i.Status ?? "")
                                .Contains("đã duyệt", StringComparison.OrdinalIgnoreCase)
                             && !i.IsSold)
                    .ToList(),

                // Tin chờ duyệt
                PendingItems = allItems
                    .Where(i => (i.Status ?? "")
                                .Contains("chờ", StringComparison.OrdinalIgnoreCase))
                    .ToList(),

                // Tin bị từ chối / hủy
                RejectedItems = allItems
                    .Where(i => (i.Status ?? "").Contains("từ chối", StringComparison.OrdinalIgnoreCase)
                             || (i.Status ?? "").Contains("hủy", StringComparison.OrdinalIgnoreCase)
                             || (i.Status ?? "").Contains("huỷ", StringComparison.OrdinalIgnoreCase))
                    .ToList()
            };

            return View(vm);
        }

        // =========================================
        // ✅ XÓA SẢN PHẨM
        // =========================================
        [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Delete(int id)
{
    var item = await _context.Items.FindAsync(id);
    if (item == null) return NotFound();

    try {
        // 1. Xóa tất cả các dữ liệu ở các bảng con trỏ về ItemId này
        // Phải xóa bảng Transactions vì đây là nơi gây ra lỗi bạn vừa gặp
        await _context.Database.ExecuteSqlRawAsync("DELETE FROM Transactions WHERE ItemId = {0}", id);
        
        // Xóa các bảng khác như bạn đã làm
        await _context.Database.ExecuteSqlRawAsync("DELETE FROM Comments WHERE ItemId = {0}", id);
        await _context.Database.ExecuteSqlRawAsync("DELETE FROM Favorites WHERE ItemId = {0}", id);
        await _context.Database.ExecuteSqlRawAsync("DELETE FROM ViewHistories WHERE ItemId = {0}", id);
        
        // 2. Xóa ảnh vật lý trong thư mục wwwroot (nếu có) để tránh rác server
        if (!string.IsNullOrEmpty(item.Image) && item.Image != "/images/no-image.png")
        {
            var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", item.Image.TrimStart('/'));
            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }
        }

        // 3. Cuối cùng mới xóa sản phẩm trong bảng Items
        _context.Items.Remove(item);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "🗑️ Đã xóa sản phẩm và các dữ liệu liên quan thành công!";
    }
    catch (Exception ex) {
        // Log lỗi chi tiết nếu cần
        TempData["ErrorMessage"] = "❌ Lỗi khi xóa: " + ex.Message;
    }

    return RedirectToAction(nameof(MyItems));
}
        // =========================================
        // ✅ SỬA SẢN PHẨM
        // =========================================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _context.Items.FindAsync(id);
            if (item == null)
            {
                TempData["ErrorMessage"] = "❌ Không tìm thấy sản phẩm.";
                return RedirectToAction(nameof(MyItems));
            }

            // 🧩 Kiểm tra quyền sở hữu
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || item.UserId != int.Parse(userIdClaim.Value))
            {
                TempData["ErrorMessage"] = "⚠️ Bạn không có quyền chỉnh sửa sản phẩm này.";
                return RedirectToAction(nameof(MyItems));
            }

            return View(item);
        }

        [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Edit(int id, Item model, IFormFile? ImageFile)
{
    if (id != model.ItemId) return NotFound();

    // 1. Lấy dữ liệu gốc từ Database
    var itemInDb = await _context.Items.FindAsync(id);
    if (itemInDb == null) return NotFound();

    // 2. Chỉ cập nhật các trường được phép sửa
    itemInDb.Title = model.Title;
    itemInDb.Description = model.Description;
    itemInDb.Category = model.Category;
    itemInDb.Condition = model.Condition;
    itemInDb.Address = model.Address;
    itemInDb.IsFree = model.IsFree;
    itemInDb.Price = model.IsFree ? 0 : model.Price;

    // LƯU Ý: KHÔNG gán itemInDb.Status = model.Status; 
    // Trạng thái "Đã duyệt" sẽ được giữ nguyên 100%.

    // 3. Xử lý ảnh nếu có file mới
    if (ImageFile != null && ImageFile.Length > 0)
    {
        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/items", fileName);
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await ImageFile.CopyToAsync(stream);
        }
        itemInDb.Image = "/images/items/" + fileName;
    }

    try {
        _context.Update(itemInDb);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "✅ Cập nhật sản phẩm thành công!";
        return RedirectToAction(nameof(MyItems));
    }
    catch (Exception ex) {
        ModelState.AddModelError("", "Lỗi: " + ex.Message);
        return View(model);
    }
}

        // =========================================
        // ✅ LỊCH SỬ XEM TIN
        // =========================================
        public async Task<IActionResult> ViewHistory()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                TempData["ErrorMessage"] = "⚠️ Bạn cần đăng nhập để xem lịch sử.";
                return RedirectToAction("Login", "Account");
            }

            int userId = int.Parse(userIdClaim.Value);

            var histories = await _context.ViewHistories
                .Include(v => v.Item)
                .Where(v => v.UserId == userId)
                .OrderByDescending(v => v.ViewedAt)
                .ToListAsync();

            return View(histories);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteHistory(int itemId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return RedirectToAction("Login", "Account");

            int userId = int.Parse(userIdClaim.Value);

            var history = await _context.ViewHistories
                .FirstOrDefaultAsync(v => v.UserId == userId && v.ItemId == itemId);

            if (history == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy bản ghi lịch sử.";
                return RedirectToAction(nameof(ViewHistory));
            }

            _context.ViewHistories.Remove(history);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã xóa một mục khỏi lịch sử xem tin.";
            return RedirectToAction(nameof(ViewHistory));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearHistory()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return RedirectToAction("Login", "Account");

            int userId = int.Parse(userIdClaim.Value);

            var histories = await _context.ViewHistories
                .Where(v => v.UserId == userId)
                .ToListAsync();

            if (histories.Any())
            {
                _context.ViewHistories.RemoveRange(histories);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa toàn bộ lịch sử xem tin.";
            }
            else
            {
                TempData["ErrorMessage"] = "Không có lịch sử để xóa.";
            }

            return RedirectToAction(nameof(ViewHistory));
        }

        // =========================================
        // ✅ YÊU THÍCH
        // =========================================
        [HttpPost]
        public async Task<IActionResult> ToggleFavorite(int itemId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                TempData["ErrorMessage"] = "Bạn cần đăng nhập.";
                return RedirectToAction("Login", "Account");
            }

            int userId = int.Parse(userIdClaim.Value);

            var existing = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.ItemId == itemId);

            if (existing != null)
            {
                _context.Favorites.Remove(existing);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã bỏ yêu thích sản phẩm.";
            }
            else
            {
                Favorite f = new Favorite
                {
                    UserId = userId,
                    ItemId = itemId,
                    AddedAt = DateTime.Now
                };

                _context.Favorites.Add(f);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Đã thêm vào yêu thích!";
            }

            return RedirectToAction("Details", new { id = itemId });
        }

        public async Task<IActionResult> MyFavorites()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return RedirectToAction("Login", "Account");

            int userId = int.Parse(userIdClaim.Value);

            var items = await _context.Favorites
                .Where(f => f.UserId == userId)
                .Include(f => f.Item)
                .OrderByDescending(f => f.AddedAt)
                .ToListAsync();

            return View(items);
        }

        // =========================================
        // ✅ BÌNH LUẬN
        // =========================================
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostComment(int itemId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập nội dung bình luận.";
                return RedirectToAction(nameof(Details), new { id = itemId });
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để bình luận.";
                return RedirectToAction("Login", "Account");
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var comment = new Comment
            {
                ItemId = itemId,
                UserId = userId,
                Content = content.Trim(),
                CreatedAt = DateTime.Now
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = itemId });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]     
public async Task<IActionResult> DeleteComment(int commentId)
{
    var comment = await _context.Comments.FindAsync(commentId);
    if (comment == null) return NotFound();

    var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
    if (comment.UserId != userId) return Forbid();

    _context.Comments.Remove(comment);
    await _context.SaveChangesAsync();

    return Redirect(Request.Headers["Referer"].ToString());
}


        // =========================================
        // ✅ ĐÁNH DẤU SẢN PHẨM ĐÃ BÁN
        // =========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsSold(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return RedirectToAction("Login", "Account");

            int userId = int.Parse(userIdClaim.Value);

            var item = await _context.Items
                .FirstOrDefaultAsync(i => i.ItemId == id && i.UserId == userId);

            if (item == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy sản phẩm hoặc bạn không có quyền.";
                return RedirectToAction(nameof(MyItems));
            }

            item.IsSold = true;
            item.SoldAt = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã đánh dấu sản phẩm là 'Đã bán'.";
            return RedirectToAction(nameof(MyItems));
        }

        // =========================================
        // ✅ TRANG QUẢN LÝ DOANH THU CỦA NGƯỜI BÁN
        // =========================================
        [Authorize]
        [Authorize]
[HttpGet]
public async Task<IActionResult> MyRevenue(int? year, int? month)
{
    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
    if (userIdClaim == null)
    {
        TempData["ErrorMessage"] = "⚠️ Bạn cần đăng nhập để xem doanh thu.";
        return RedirectToAction("Login", "Account");
    }

    int currentUserId = int.Parse(userIdClaim.Value);

    // ====== 1. Lấy list sản phẩm đã bán (áp dụng filter nếu có) ======
    var soldQuery = _context.Items
    .Where(i => i.UserId == currentUserId
             && i.IsSold
             && i.SoldAt != null
             && !i.IsFree);     // ❌ loại đồ miễn phí ra


    if (year.HasValue)
        soldQuery = soldQuery.Where(i => i.SoldAt!.Value.Year == year.Value);

    if (month.HasValue)
        soldQuery = soldQuery.Where(i => i.SoldAt!.Value.Month == month.Value);

    var soldItems = await soldQuery
        .OrderByDescending(i => i.SoldAt)
        .ToListAsync();

    decimal totalRevenue = soldItems.Sum(i => i.Price);

    // ====== 2. Lấy list năm có doanh thu để đổ vào dropdown ======
    var years = await _context.Items
        .Where(i => i.UserId == currentUserId && i.IsSold && i.SoldAt != null)
        .Select(i => i.SoldAt!.Value.Year)
        .Distinct()
        .OrderBy(y => y)
        .ToListAsync();

    int statsYear = year ?? (years.Any() ? years.Max() : DateTime.Now.Year);

    // ====== 3. Thống kê doanh thu theo tháng trong năm statsYear ======
    var monthlyStats = await _context.Items
    .Where(i => i.UserId == currentUserId
                && i.IsSold
                && i.SoldAt != null
                && !i.IsFree
                && i.SoldAt.Value.Year == statsYear)
        .GroupBy(i => i.SoldAt!.Value.Month)
        .Select(g => new MonthlyRevenueDto
        {
            Year = statsYear,
            Month = g.Key,
            Total = g.Sum(x => x.Price),
            Count = g.Count()
        })
        .OrderBy(x => x.Month)
        .ToListAsync();

    var vm = new MyRevenueViewModel
    {
        SoldItems = soldItems,
        TotalRevenue = totalRevenue,
        MonthlyStats = monthlyStats,
        SelectedYear = year,
        SelectedMonth = month,
        Years = years
    };

    return View(vm);
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> RelistItem(int id)
{
    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
    if (userIdClaim == null)
        return RedirectToAction("Login", "Account");

    int userId = int.Parse(userIdClaim.Value);

    // Chỉ cho phép đăng lại sản phẩm của chính user và đang ở trạng thái đã bán
    var item = await _context.Items
        .FirstOrDefaultAsync(i => i.ItemId == id && i.UserId == userId && i.IsSold);

    if (item == null)
    {
        TempData["ErrorMessage"] = "Không tìm thấy sản phẩm hoặc sản phẩm không ở trạng thái đã bán.";
        return RedirectToAction(nameof(MyRevenue));
    }

    // Đăng lại: bỏ trạng thái đã bán, cho admin duyệt lại
    item.IsSold = false;
    item.SoldAt = null;
    item.Status = "Chờ duyệt";

    await _context.SaveChangesAsync();

    TempData["SuccessMessage"] = "Đã đăng lại sản phẩm. Vui lòng chờ admin duyệt.";
    return RedirectToAction(nameof(MyItems));
}
[Authorize(Roles = "ADMIN")]
public async Task<IActionResult> AdminIndex()
{
    var items = await _context.Items.Include(i => i.User).ToListAsync();
    
    // Phải trỏ trực tiếp đến đường dẫn file vì folder khác tên Controller
    return View("~/Views/Admin/AdminIndex.cshtml", items); 
}

// Action Duyệt tin nhanh cho Admin
[HttpPost]
public async Task<IActionResult> ApproveItem(int id)
{
    var item = await _context.Items.FindAsync(id);
    if (item != null)
    {
        item.Status = "Approved"; // Hoặc giá trị tương ứng trong logic của bạn
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Đã duyệt sản phẩm thành công!";
    }
    return RedirectToAction(nameof(AdminIndex));
}

    }
}
