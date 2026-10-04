using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using IS7012Final.Model;
using IS7012Final.Data;
using System.Security.Claims;

namespace IS7012Final.Pages.ReviewPages;

[Authorize]
public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public CreateModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Review Review { get; set; } = default!;

    public SelectList MovieList { get; set; } = default!;

    public IActionResult OnGet()
    {
        MovieList = new SelectList(
            _context.Movie.OrderBy(m => m.Title),
            "Id",
            "Title"
        );

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Review.UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        Review.Timestamp = DateTime.Now;
        ModelState.Remove("Review.UserId");
        ModelState.Remove("Review.Timestamp");
        ModelState.Remove("Review.Movie");

        if (!ModelState.IsValid)
        {
            MovieList = new SelectList(
                _context.Movie.OrderBy(m => m.Title),
                "Id",
                "Title"
            );

            return Page();
        }

        _context.Review.Add(Review);
        await _context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }
}
