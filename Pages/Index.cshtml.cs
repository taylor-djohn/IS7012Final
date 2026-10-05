using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using IS7012Final.Model;
using IS7012Final.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using System.Linq;
using System.Text.Json;
using System.IO;
using System;
using System.Threading.Tasks;

namespace IS7012Final.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IS7012Final.Data.ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public IndexModel(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public List<MovieCard> TrendingMovies { get; set; } = new();
        public List<MovieCard> TopRatedMovies { get; set; } = new();
        // name matches the Razor page usage (RecentActivity)
        public List<FeedActivityItem> RecentActivity { get; set; } = new();

        public async Task OnGetAsync()
        {
            // Build trending and top-rated movie cards by combining DB and JSON data
            var movies = await _context.Movie
                .Include(m => m.Reviews)
                .ToListAsync();

            // For each movie, build a MovieCard with Poster, Rating (average), GenreName
            var movieCards = new List<MovieCard>();
            foreach (var m in movies)
            {
                var mc = new MovieCard
                {
                    Id = m.Id,
                    Title = m.Title ?? "",
                    GenreName = !string.IsNullOrEmpty(m.Genre) ? m.Genre : (m.Genres?.FirstOrDefault()?.Name ?? ""),
                    Rating = m.Reviews != null && m.Reviews.Count > 0 ? m.Reviews.Average(r => r.Rating) : 0.0,
                    Poster = GetPosterForMovie(m.Id)
                };
                movieCards.Add(mc);
            }

            TrendingMovies = movieCards.Take(3).ToList();
            TopRatedMovies = movieCards.OrderByDescending(c => c.Rating).Take(3).ToList();

            // Recent activity: latest 10 reviews
            var reviews = await _context.Review
                .Include(r => r.Movie)
                .OrderByDescending(r => r.Timestamp)
                .Take(10)
                .ToListAsync();

            foreach (var rev in reviews)
            {
                var user = await _context.Users.FindAsync(rev.UserId);
                var userReviewCount = await _context.Review.CountAsync(r => r.UserId == rev.UserId);

                RecentActivity.Add(new FeedActivityItem
                {
                    UserName = user?.UserName ?? rev.UserId ?? "Unknown",
                    MovieTitle = rev.Movie?.Title ?? "Unknown Movie",
                    MoviePoster = rev.Movie != null ? GetPosterForMovie(rev.MovieId) : null,
                    Comment = rev.ReviewText,
                    Rating = rev.Rating,
                    Timestamp = rev.Timestamp,
                    UserReviewCount = userReviewCount,
                    Likes = 0
                });
            }
        }

        private string? GetPosterForMovie(int movieId)
        {
            try
            {
                var filePath = Path.Combine(_env.WebRootPath ?? string.Empty, "data", "movies.json");
                if (!System.IO.File.Exists(filePath)) return null;
                var json = System.IO.File.ReadAllText(filePath);
                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var list = JsonSerializer.Deserialize<List<JsonMovie>>(json, opts);
                var jm = list?.FirstOrDefault(x => x.Id == movieId);
                return jm?.Poster;
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
            public string? Poster { get; set; }
        }
    }

    public class FeedActivityItem
    {
        public string UserName { get; set; } = string.Empty;
        public string MovieTitle { get; set; } = string.Empty;
        public string? MoviePoster { get; set; }
        public string? Comment { get; set; }
        public int Rating { get; set; }
        public DateTime Timestamp { get; set; }
        public int UserReviewCount { get; set; }
        public int Likes { get; set; }
    }

    public class MovieCard
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Poster { get; set; }
        public double Rating { get; set; }
        public string GenreName { get; set; } = string.Empty;
    }
}
