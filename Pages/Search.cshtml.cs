using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using IS7012Final.Data;
using IS7012Final.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using IS7012Final.Pages.GenrePages;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;

namespace IS7012Final.Pages
{
    public class SearchModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        public SearchModel(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        public List<JsonMovie> Movies { get; set; } = new();

        public async Task OnGetAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchTerm))
            {
                return;
            }

            var jsonPath = Path.Combine("wwwroot", "data", "movies.json");

            if (System.IO.File.Exists(jsonPath))
            {
                var json = await System.IO.File.ReadAllTextAsync(jsonPath);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                List<JsonMovie> allMovies = JsonSerializer.Deserialize<List<JsonMovie>>(json, options) ?? new List<JsonMovie>();
                var filteredMovies = new List<JsonMovie>();
                var index = 0;

                while (index < allMovies.Count)
                {
                    var item = allMovies[index];
                    if (item.Title != null && item.Title.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                    {
                        filteredMovies.Add(item);
                    }
                    index++;
                }
                Movies = filteredMovies;
            }
        }

        public async Task<IActionResult> OnPostAddToWatchlistAsync(int movieId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }


            var jsonPath = Path.Combine(_env.WebRootPath, "data", "movies.json");

            if (!System.IO.File.Exists(jsonPath))
            {
                return NotFound("Movie data file not found.");
            }

            var json = await System.IO.File.ReadAllTextAsync(jsonPath);

            var allMovies = JsonSerializer.Deserialize<List<JsonMovie>>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            ) ?? new List<JsonMovie>();
            var selectedMovie = allMovies.FirstOrDefault(m => m.MovieId == movieId);
            if (selectedMovie != null)
            {
                Console.WriteLine("SELECTED MOVIE: " + selectedMovie.Title);
                Console.WriteLine("JSON MOVIE ID: " + selectedMovie.MovieId);
            }

            if (selectedMovie == null || string.IsNullOrWhiteSpace(selectedMovie.Title))
            {
                return NotFound("Movie not found.");
            }
            var movie = _context.Movie
                .FirstOrDefault(m => m.Title == selectedMovie.Title);
            if (movie == null)
            {
                movie = new Movie
                {
                    Title = selectedMovie.Title,
                    Director = "Unknown",
                    ReleaseYear = 0,
                    Genre = "Unknown"
                };
                _context.Movie.Add(movie);
                await _context.SaveChangesAsync();
            }
            bool alreadyExists = _context.Watchlist
                .Any(w => w.UserId == userId && w.MovieId == movie.Id);

            if (!alreadyExists)
            {
                var watchlistItem = new Watchlist
                {
                    UserId = userId,
                    MovieId = movie.Id,
                    DateAdded = DateTime.UtcNow
                };
                _context.Watchlist.Add(watchlistItem);
                await _context.SaveChangesAsync();
            }
            Console.WriteLine("DATABASE MOVIE: " + movie.Title);
            Console.WriteLine("DATABASE MOVIE ID: " + movie.Id);
            Console.WriteLine("ALREADY IN WATCHLIST: " + alreadyExists);
            return RedirectToPage("/WatchlistPages/Index");
        }
    }
}
