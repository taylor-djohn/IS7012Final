using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using IS7012Final.Model;
using IS7012Final.Data;

namespace IS7012Final.Pages.ReviewPages;

public class EditModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public EditModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Review Review { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var review = await _context.Review
            .Include(r => r.Movie)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (review is null)
        {
            return NotFound();
        }
        // ensure MovieTitle is populated for the edit form
        if (string.IsNullOrEmpty(review.MovieTitle) && review.Movie != null)
        {
            review.MovieTitle = review.Movie.Title;
        }

        Review = review;
        return Page();
    }

    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see https://aka.ms/RazorPagesCRUD.
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var dbReview = await _context.Review.FindAsync(Review.Id);
        if (dbReview == null) return NotFound();

        dbReview.MovieTitle = Review.MovieTitle;
        dbReview.Rating = Review.Rating;
        dbReview.ReviewText = Review.ReviewText;
        dbReview.Timestamp = Review.Timestamp;

        await _context.SaveChangesAsync();
        return RedirectToPage("./Index");
    }
    private bool ReviewExists(int id)
    {
        return _context.Review.Any(e => e.Id == id);
    }
}
