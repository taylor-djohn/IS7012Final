using System.Threading.Tasks;
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

    [BindProperty(SupportsGet = true)]
    public int? MovieId { get; set; }

    public async Task OnGetAsync(int? id, int? movieId)
    {
        if (User?.Identity?.IsAuthenticated != true)
        {
            Review = new List<Review>();
            return;
        }

        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (MovieId.HasValue)
        {
            // Show all reviews for the specified movie
            Review = await _context.Review
                .Where(r => r.MovieId == MovieId.Value)
                .Include(r => r.Movie)
                .OrderByDescending(r => r.Timestamp)
                .ToListAsync();
        }
        else
        {
            // Default: show reviews by the current user
            Review = await _context.Review
                .Where(r => r.UserId == userId)
                .Include(r => r.Movie)
                .OrderByDescending(r => r.Timestamp)
                .ToListAsync();
        }
    }
}
