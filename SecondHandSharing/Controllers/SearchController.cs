using Microsoft.AspNetCore.Mvc;
using System.Linq;
using SecondHandSharing.Data;

public class SearchController : Controller
{
    private readonly ApplicationDbContext _context;
    public SearchController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public JsonResult Suggest(string query)
    {
        var results = _context.Items
            .Where(p => p.Title.Contains(query))
            .Select(p => new { p.ItemId, p.Title })
            .Take(10)
            .ToList();

        return Json(results);
    }
    [HttpGet]
    public IActionResult Index(string keyword)
    {
        var results = _context.Items
            .Where(p => p.Title.Contains(keyword))
            .ToList();

        ViewBag.Keyword = keyword;
        return View(results);
    }

}
