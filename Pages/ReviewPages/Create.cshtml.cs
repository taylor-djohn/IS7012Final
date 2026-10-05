using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using IS7012Final.Model;
using IS7012Final.Data;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace IS7012Final.Pages.ReviewPages;

[Authorize]
public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;

    public CreateModel(ApplicationDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    private JsonMovie? GetJsonMovieById(int id)
    {
        try
        {
            var filePath = Path.Combine(_env.WebRootPath ?? string.Empty, "data", "movies.json");
            if (!System.IO.File.Exists(filePath))
                return null;

            var json = System.IO.File.ReadAllText(filePath);
            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var list = JsonSerializer.Deserialize<List<JsonMovie>>(json, opts);
            return list?.FirstOrDefault(m => m.Id == id);
        }
        catch
        {
            return null;
        }
    }

    [BindProperty]
    public Review Review { get; set; } = default!;

    public SelectList MovieList { get; set; } = default!;

    public IActionResult OnGet()
    {
        MovieList = LoadMoviesFromJson() ?? new SelectList(
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

        // Ensure the referenced movie exists in the DB. If the movie was chosen from the
        // JSON file (not present in the DB), create a minimal Movie record so the
        // Review's foreign key constraint is satisfied.
        var movieExists = await _context.Movie.FindAsync(Review.MovieId);
        if (movieExists == null)
        {
            var jm = GetJsonMovieById(Review.MovieId);
            if (jm != null)
            {
                var newMovie = new Movie
                {
                    Id = jm.Id,
                    Title = jm.Title ?? "Unknown"
                };
                _context.Movie.Add(newMovie);
                // No SaveChanges here; both Movie and Review will be saved together below.
            }
        }

        _context.Review.Add(Review);

        // Add the movie to the user's watchlist if it's not already there
        var userId = Review.UserId;
        var movieId = Review.MovieId;
        if (!string.IsNullOrEmpty(userId))
        {
            var wlExists = await _context.Watchlist
                .FirstOrDefaultAsync(w => w.UserId == userId && w.MovieId == movieId);

            if (wlExists == null)
            {
                // Link the watchlist entry to the review by setting the Review navigation property.
                // EF will populate ReviewId when SaveChanges runs because the Review instance
                // is tracked in the same DbContext.
                _context.Watchlist.Add(new Watchlist
                {
                    UserId = userId,
                    MovieId = movieId,
                    DateAdded = DateTime.Now,
                    Review = Review
                });
            }
        }

        await _context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }
    private SelectList? LoadMoviesFromJson()
    {
        try
        {
            var filePath = Path.Combine(_env.WebRootPath ?? string.Empty, "data", "movies.json");
            if (!System.IO.File.Exists(filePath))
            {
                return null;
            }

            var json = System.IO.File.ReadAllText(filePath);
            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var list = JsonSerializer.Deserialize<List<JsonMovie>>(json, opts);
            if (list == null || list.Count == 0)
                return null;

            var ordered = list.OrderBy(m => m.Title);
            return new SelectList(ordered, "Id", "Title");
        }
        catch
        {
            // If anything goes wrong reading/parsing json, fall back to DB in the caller
            return null;
        }
    }

    private class JsonMovie
    {
        public int Id { get; set; }
        public string? Title { get; set; }
    }
}
