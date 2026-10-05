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

        _context.Review.Add(Review);
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
