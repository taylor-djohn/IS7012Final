using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using IS7012Final.Model;
using IS7012Final.Data;

namespace IS7012Final.Pages.ReviewPages;

public class DetailsModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public Movie Movie { get; private set; }
    public DetailsModel(ApplicationDbContext context) => _context = context;
  
    public Review Review { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? movieId, int? id)
    {
        // If caller passed movieId, show first review for that movie
        if (movieId.HasValue)
        {
            Movie = await _context.Movie
                .Include(m => m.Reviews)
                .FirstOrDefaultAsync(m => m.Id == movieId.Value);
            if (Movie == null) return NotFound();

            Review = Movie.Reviews.FirstOrDefault();
            if (Review != null && string.IsNullOrEmpty(Review.MovieTitle))
            {
                Review.MovieTitle = Movie.Title;
            }

            return Page();
        }

        // If caller passed id, treat it as a Review id (links from Review index use asp-route-id)
        if (id.HasValue)
        {
            var rev = await _context.Review
                .Include(r => r.Movie)
                .FirstOrDefaultAsync(r => r.Id == id.Value);
            if (rev == null) return NotFound();

            Review = rev;
            Movie = rev.Movie!;
            if (string.IsNullOrEmpty(Review.MovieTitle) && Movie != null)
            {
                Review.MovieTitle = Movie.Title;
            }

            return Page();
        }

        return NotFound();
    }
}
