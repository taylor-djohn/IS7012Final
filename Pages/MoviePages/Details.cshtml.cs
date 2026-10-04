using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using IS7012Final.Model;
using IS7012Final.Data;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;


namespace IS7012Final.Pages.MoviePages;

public class DetailsModel : PageModel
{
    public bool IsInWatchlist { get; set; }
    private readonly ApplicationDbContext _context;
    public DetailsModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public IS7012Final.Model.Movie Movie { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var movie = await _context.Movie
            .FirstOrDefaultAsync(m => m.Id == id);

        if (movie is null)
        {
            return NotFound();
        }

        Movie = movie;
        if (User.Identity?.IsAuthenticated == true)
        {
            string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            IsInWatchlist = await _context.Watchlist
                .AnyAsync(w => w.UserId == userId && w.MovieId == Movie.Id);
        }

        return Page();
    }
    public async Task<IActionResult> OnPostAddToWatchlistAsync(int id)
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return Challenge();
        }

        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var movie = await _context.Movie.FindAsync(id);

        if (movie == null)
        {
            return NotFound();
        }

        bool alreadyExists = await _context.Watchlist
            .AnyAsync(w => w.UserId == userId && w.MovieId == id);

        if (!alreadyExists)
        {
            var watchlistItem = new Watchlist
            {
                UserId = userId,
                MovieId = id,
                DateAdded = DateTime.Now
            };

            _context.Watchlist.Add(watchlistItem);
            await _context.SaveChangesAsync();
        }

        return RedirectToPage(new { id });
    }
}
