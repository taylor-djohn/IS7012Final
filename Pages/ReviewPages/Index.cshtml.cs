using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using IS7012Final.Model;
using IS7012Final.Data;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace IS7012Final.Pages.ReviewPages;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public IList<Review> Review { get; set; } = default!;

    public async Task OnGetAsync()
    {
        if (User?.Identity?.IsAuthenticated != true)
        {
            Review = new List<Review>();
            return;
        }

        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        Review = await _context.Review
            .Where(r => r.UserId == userId)
            .Include(r => r.Movie)
            .OrderByDescending(r => r.Timestamp)
            .ToListAsync();
    }
}
