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
            // always try to populate poster URL from the JSON data (if present)
            var jm = GetJsonMovieById(item.MovieId);
            if (jm != null)
            {
                if (!PosterUrls.ContainsKey(item.MovieId)) PosterUrls[item.MovieId] = jm.Poster;

                if (item.Movie == null || string.IsNullOrEmpty(item.Movie.Title))
                {
                    item.Movie = new Movie
                    {
                        Id = jm.Id,
                        Title = jm.Title,
                        // parse release year from release_date like "2009-12-10"
                        ReleaseYear = ParseYear(jm.ReleaseDate),
                        Genre = jm.Genres != null && jm.Genres.Length > 0 ? jm.Genres[0] : null
                    };
                }
            }
            else
            {
                // ensure mapping exists even if null so view can check TryGetValue
                if (!PosterUrls.ContainsKey(item.MovieId)) PosterUrls[item.MovieId] = null;
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

    private JsonMovie? GetJsonMovieById(int id)
    {
        try
        {
            // Try common locations for the JSON file
            var candidatePaths = new[] {
                Path.Combine(_env.WebRootPath ?? string.Empty, "data", "movies.json"),
                Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data", "movies.json"),
                Path.Combine(Directory.GetCurrentDirectory(), "data", "movies.json")
            };

            string? filePath = candidatePaths.FirstOrDefault(p => !string.IsNullOrEmpty(p) && System.IO.File.Exists(p));
            if (string.IsNullOrEmpty(filePath)) return null;

            using var doc = JsonDocument.Parse(System.IO.File.ReadAllText(filePath));
            if (doc.RootElement.ValueKind != JsonValueKind.Array) return null;

            foreach (var el in doc.RootElement.EnumerateArray())
            {
                if (!el.TryGetProperty("id", out var idProp)) continue;
                if (idProp.ValueKind != JsonValueKind.Number) continue;
                if (idProp.GetInt32() != id) continue;

                var jm = new JsonMovie { Id = id };

                if (el.TryGetProperty("title", out var titleProp) && titleProp.ValueKind == JsonValueKind.String)
                    jm.Title = titleProp.GetString();

                if (el.TryGetProperty("release_date", out var rdProp) && rdProp.ValueKind == JsonValueKind.String)
                    jm.ReleaseDate = rdProp.GetString();

                if (el.TryGetProperty("genres", out var genresProp) && genresProp.ValueKind == JsonValueKind.Array)
                {
                    var list = new List<string>();
                    foreach (var g in genresProp.EnumerateArray()) if (g.ValueKind == JsonValueKind.String) list.Add(g.GetString()!);
                    jm.Genres = list.ToArray();
                }

                if (el.TryGetProperty("overview", out var ovProp) && ovProp.ValueKind == JsonValueKind.String)
                    jm.Overview = ovProp.GetString();

                if (el.TryGetProperty("poster", out var pProp) && pProp.ValueKind == JsonValueKind.String)
                    jm.Poster = pProp.GetString();

                return jm;
            }

            return null;
        }
        catch
        {
            return null;
        }
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