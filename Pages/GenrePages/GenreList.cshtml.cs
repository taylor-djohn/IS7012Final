using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using IS7012Final.Data;
using IS7012Final.Model;
using Microsoft.AspNetCore.Mvc;

namespace IS7012Final.Pages.GenrePages;

public class GenreListModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public GenreListModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public IList<Genre> Genre { get; set; } = default!;
    public List<JsonMovie>? JsonMovies { get; set; }
    public Dictionary<int, List<JsonMovie>> MoviesByGenre { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public int? GenreId { get; set; }

    public async Task OnGetAsync()
    {
        // Eager-load related movies and their genres to avoid multiple queries or lazy-loading issues
        Genre = await _context.Genre
            .Include(g => g.Movie)
                .ThenInclude(m => m.Genres)
            .ToListAsync();

        // Also load the static movies.json (used by the Popular page) and map movies to genres by name
        var jsonPath = Path.Combine("wwwroot", "data", "movies.json");
        if (System.IO.File.Exists(jsonPath))
        {
            var json = await System.IO.File.ReadAllTextAsync(jsonPath);
            var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            JsonMovies = System.Text.Json.JsonSerializer.Deserialize<List<JsonMovie>>(json, options);

            if (JsonMovies != null)
            {
                foreach (var g in Genre)
                {
                    var list = JsonMovies.Where(m => m.Genres != null && m.Genres.Contains(g.Name)).ToList();
                    MoviesByGenre[g.Id] = list;
                }
            }
        }
    }
}

public class JsonMovie
{
    public string? MovieId { get; set; }
    public string? Title { get; set; }
    public string? Poster { get; set; }
    public List<string>? Genres { get; set; }
    public string? Release_Date { get; set; }
    public double Rating { get; set; }
    public string? Overview { get; set; }
}
