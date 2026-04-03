using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecondHandSharing.Data;
using SecondHandSharing.Models;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Security.Claims;

namespace SecondHandSharing.Controllers

{
    [Authorize]
    public class ChatController : Controller
    {

        private readonly ApplicationDbContext _context;
        


        public ChatController(ApplicationDbContext context)
    {
        _context = context;
        
    }
        public IActionResult Index()
    {
        return View();
    }
        // public ChatController(ApplicationDbContext context)
        // {
        //     _context = context;
        // _http = new HttpClient();
        // _http.DefaultRequestHeaders.Add("Authorization",
        //     $"Bearer YOUR_OPENAI_API_KEY");
        // }


        // Lấy userId hiện tại
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return int.Parse(userIdClaim!.Value);
        }

        // Danh sách cuộc trò chuyện của tôi
        public async Task<IActionResult> MyChats()
        {
            int currentUserId = GetCurrentUserId();

            var conversations = await _context.Conversations
                .Include(c => c.User1)
                .Include(c => c.User2)
                .Where(c => c.User1Id == currentUserId || c.User2Id == currentUserId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(conversations);
        }

        // Mở phòng chat theo ConversationId
        public async Task<IActionResult> Room(int id)
        {
            int currentUserId = GetCurrentUserId();

            var convo = await _context.Conversations
                .Include(c => c.User1)
                .Include(c => c.User2)
                .Include(c => c.Messages)
                    .ThenInclude(m => m.Sender)
                .FirstOrDefaultAsync(c => c.ConversationId == id);

            if (convo == null)
                return NotFound();

            // Không phải người trong cuộc
            if (convo.User1Id != currentUserId && convo.User2Id != currentUserId)
                return Forbid();

            // Đánh dấu đã đọc
            var unread = convo.Messages
                .Where(m => !m.IsRead && m.SenderId != currentUserId)
                .ToList();

            foreach (var msg in unread)
                msg.IsRead = true;

            await _context.SaveChangesAsync();

            return View(convo);
        }

        // Tạo (hoặc mở) chat với 1 user khác (ví dụ người bán)
        public async Task<IActionResult> StartChat(int targetUserId, int? itemId = null)
{
    int currentUserId = GetCurrentUserId();

    if (currentUserId == targetUserId)
        return RedirectToAction(nameof(MyChats));

    var convo = await _context.Conversations
        .Include(c => c.Messages)
        .FirstOrDefaultAsync(c =>
            (c.User1Id == currentUserId && c.User2Id == targetUserId) ||
            (c.User1Id == targetUserId && c.User2Id == currentUserId));

    if (convo == null)
    {
        convo = new Conversation
        {
            User1Id = currentUserId,
            User2Id = targetUserId
        };
        _context.Conversations.Add(convo);
        await _context.SaveChangesAsync();
    }

    // 🔥 Nếu có itemId (bấm từ trang sản phẩm) -> luôn gửi tin giới thiệu
    if (itemId.HasValue)
    {
        var item = await _context.Items
            .FirstOrDefaultAsync(i => i.ItemId == itemId.Value);

        if (item != null)
        {
            var productUrl = Url.Action(
                "Details",
                "Item",
                new { id = item.ItemId },
                Request.Scheme
            );

            var introMsg = new Message
            {
                ConversationId = convo.ConversationId,
                SenderId = currentUserId,
                Content =
                    $"[Quan tâm sản phẩm]\n" +
                    $"Tiêu đề: {item.Title}\n" +
                    $"Giá: {item.Price:N0} đ\n" +
                    $"Link: {productUrl}"
            };

            _context.Messages.Add(introMsg);
            await _context.SaveChangesAsync();
        }
    }

    return RedirectToAction(nameof(Room), new { id = convo.ConversationId });
}


        // Chat với admin (tự tìm 1 user có Role = ADMIN)
        public async Task<IActionResult> ChatWithAdmin()
        {
            var admin = await _context.Users
                .FirstOrDefaultAsync(u => u.Role.ToUpper() == "ADMIN");

            if (admin == null)
            {
                TempData["ErrorMessage"] = "Hiện chưa có admin.";
                return RedirectToAction("Index", "Item");
            }

            return await StartChat(admin.UserId);
        }

        // Gửi tin nhắn (có thể kèm ảnh)
        [HttpPost]
        public async Task<IActionResult> SendMessage(int conversationId, string content, IFormFile? imageFile)
        {
            // Không có gì để gửi
            if (string.IsNullOrWhiteSpace(content) &&
                (imageFile == null || imageFile.Length == 0))
            {
                return RedirectToAction(nameof(Room), new { id = conversationId });
            }

            int currentUserId = GetCurrentUserId();

            var convo = await _context.Conversations
                .FirstOrDefaultAsync(c => c.ConversationId == conversationId);

            if (convo == null)
                return NotFound();

            if (convo.User1Id != currentUserId && convo.User2Id != currentUserId)
                return Forbid();

            string? imagePath = null;

            // Lưu ảnh nếu có
            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "uploads",
                    "chat"
                );

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                imagePath = "/uploads/chat/" + fileName;
            }

            var msg = new Message
            {
                ConversationId = conversationId,
                SenderId = currentUserId,
                Content = string.IsNullOrWhiteSpace(content) ? "" : content,
                SentAt = DateTime.Now,
                ImagePath = imagePath
            };

            _context.Messages.Add(msg);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Room), new { id = conversationId });
        }
        ///////////////////////////////
        /// 
    
    // [HttpPost]
    // [Route("api/chat/send")]
    // public async Task<IActionResult> Send([FromBody] ChatRequest request)
    // {
    //     if (request == null || string.IsNullOrWhiteSpace(request.Message))
    //     {
    //         return BadRequest("Tin nhắn không hợp lệ");
    //     }

    //     // 1. Lấy sản phẩm từ DB
    //     var items = await _context.Items
    //         .Where(i => i.Status == "Đã duyệt" && !i.IsSold)
    //         .OrderByDescending(i => i.CreatedAt)
    //         .Take(5)
    //         .ToListAsync();

    //     if (!items.Any())
    //     {
    //         return Ok(new { answer = "Hiện chưa có sản phẩm phù hợp." });
    //     }

    //     // 2. Build context
    //     var contextBuilder = new StringBuilder();
    //     contextBuilder.AppendLine("Danh sách sản phẩm hiện có:");

    //     foreach (var item in items)
    //     {
    //         contextBuilder.AppendLine(
    //             $"- {item.Title} | {item.Price:N0}đ | {item.Category} | {item.Address} | ID:{item.ItemId}"
    //         );
    //     }

    //     // 3. Gửi OpenAI
    //     // var answer = await _ai.AskAsync(
    //     //     systemPrompt: "Bạn là trợ lý bán đồ cũ. Chỉ trả lời dựa trên dữ liệu được cung cấp.",
    //     //     context: contextBuilder.ToString(),
    //     //     userMessage: request.Message
    //     // );

    // //     return Ok(new { answer });
    // // }
    
    // }
    }
}

