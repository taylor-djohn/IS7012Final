using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using IS7012Final.Data;
using IS7012Final.Model;

namespace IS7012Final.Pages.WatchlistPages;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public IList<Watchlist> WatchlistItems { get; set; } = new List<Watchlist>();

    public async Task OnGetAsync()
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        WatchlistItems = await _context.Watchlist
            .Include(w => w.Movie)
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.DateAdded)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostRemoveAsync(int id)
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var watchlistItem = await _context.Watchlist
            .FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId);

        if (watchlistItem != null)
        {
            _context.Watchlist.Remove(watchlistItem);
            await _context.SaveChangesAsync();
        }

        return RedirectToPage();
    }
}