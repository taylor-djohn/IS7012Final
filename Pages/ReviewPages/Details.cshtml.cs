using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using IS7012Final.Model;
using IS7012Final.Data;

namespace IS7012Final.Pages.ReviewPages;

public class DetailsModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public DetailsModel(ApplicationDbContext context)
    {
        _context = context;
    }
    public Movie? Movie { get; set; }
    public Review? UserReview { get; set; }
    public List<Review> OtherReviews { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int? movieId, int? id)
    {
        //Accept either movieId or id  (from route or query string)
        var targetId = movieId ?? id;

        if (!targetId.HasValue)
        {
            return NotFound();
        }

        // 1. Load Movie along with all existing reviews
        Movie = await _context.Movie
            .Include(m => m.Reviews)
            .FirstOrDefaultAsync(m => m.Id == targetId.Value);

        if(Movie == null)
        {
            return NotFound();
        }

        // 2. Identify the currently logged-in user
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // 3. Separate user's own review from all other reviews
        var allReviews = Movie.Reviews ?? new List<Review>();

        if (!string.IsNullOrEmpty(currentUserId))
        {
            UserReview = allReviews.FirstOrDefault(r => r.UserId == currentUserId);
            OtherReviews = allReviews
                .Where(r => r.UserId != currentUserId)
                .OrderByDescending(r => r.Timestamp).ToList();
        }
        else
        {
            OtherReviews = allReviews
                .OrderByDescending (r => r.Timestamp).ToList();
        }

        return Page();
        
    }
 }