using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using IS7012Final.Data;
using IS7012Final.Model;
using Microsoft.AspNetCore.Hosting;
using System.Text.Json;
using System.IO;
using System;
using System.Linq;
using System.Collections.Generic;

namespace IS7012Final.Pages.WatchlistPages;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;

    public IndexModel(ApplicationDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    public IList<Watchlist> WatchlistItems { get; set; } = new List<Watchlist>();
    // map movieId -> poster url (if available from movies.json)
    public Dictionary<int, string?> PosterUrls { get; set; } = new Dictionary<int, string?>();

    public async Task OnGetAsync()
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        WatchlistItems = await _context.Watchlist
            .Include(w => w.Movie)
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.DateAdded)
            .ToListAsync();

        // For any watchlist item whose Movie navigation is null or missing key fields,
        // try to populate displayable properties from the JSON data in wwwroot/data/movies.json.
        foreach (var item in WatchlistItems)
        {
            var jm = GetJsonMovieByTitle(item.Movie?.Title);

            if (jm != null)
            {
                PosterUrls[item.MovieId] = jm.Poster;
            }
            else
            {
                PosterUrls[item.MovieId] = null;
            }
        }
    }

    private int ParseYear(string? releaseDate)
    {
        if (string.IsNullOrEmpty(releaseDate)) return 0;
        if (DateTime.TryParse(releaseDate, out var dt)) return dt.Year;
        // try substring first 4 chars
        if (releaseDate.Length >= 4 && int.TryParse(releaseDate.Substring(0,4), out var y)) return y;
        return 0;
    }

    private JsonMovie? GetJsonMovieByTitle(string? title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return null;
        }

        var jsonPath = Path.Combine(_env.WebRootPath, "data", "movies.json");

        if (!System.IO.File.Exists(jsonPath))
        {
            return null;
        }

        var json = System.IO.File.ReadAllText(jsonPath);

        var movies = JsonSerializer.Deserialize<List<JsonMovie>>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }
        );

        return movies?.FirstOrDefault(m =>
            string.Equals(m.Title, title, StringComparison.OrdinalIgnoreCase));
    }

    private class JsonMovie
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Overview { get; set; }
        public string? Poster { get; set; }
        public string? ReleaseDate { get; set; }
        public string[]? Genres { get; set; }
    }

    public async Task<IActionResult> OnPostRemoveAsync(int id)
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var watchlistItem = await _context.Watchlist
            .FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId);

        if (watchlistItem != null)
        {
            _context.Watchlist.Remove(watchlistItem);
            await _context.SaveChangesAsync();
        }

        return RedirectToPage();
    }
}