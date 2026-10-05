using System.Security.Claims;
using IS7012Final.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
// removed unused imports

namespace IS7012Final.Pages
{
    public class StatistiscsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public StatistiscsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public int TotalReviews { get; set; }
        public double AverageRating { get; set; }
        public List<RatingDistributionItem> RatingDistribution { get; set; } = new();
        public async Task OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            
            if (!string.IsNullOrEmpty(userId))
            {
                var userReviews = await _context.Review
                    .Where(r => r.UserId == userId)
                    .ToListAsync();

                if (userReviews.Any())
                {
                    TotalReviews = userReviews.Count;
                    AverageRating = userReviews.Average(r => r.Rating);
                    

                    var stars = 5;
                    while (stars >= 1)
                    {
                        var count = userReviews.Count(r => r.Rating == stars);
                        var pct = TotalReviews > 0 ? (double)count / (double)TotalReviews * 100.0 : 0.0;

                        RatingDistribution.Add(new RatingDistributionItem
                        {
                            Stars = $"{stars} Star{(stars > 1 ? "s" : "")}",
                            Count = count,
                            Percentage = Math.Round(pct, 1)
                        });
                        stars--;
                    }
                    return;
                }
            }

            TotalReviews = 0;
            AverageRating = 0;
            RatingDistribution = new List<RatingDistributionItem>
            {
                new RatingDistributionItem { Stars = "5 Stars", Count = 0, Percentage = 0 },
                new RatingDistributionItem { Stars = "4 Stars", Count = 0, Percentage = 0 },
                new RatingDistributionItem { Stars = "3 Stars", Count = 0, Percentage = 0 },
                new RatingDistributionItem { Stars = "2 Stars", Count = 0, Percentage = 0 },
                new RatingDistributionItem { Stars = "1 Star", Count = 0, Percentage = 0 }
            };
        }
    }
    public class RatingDistributionItem
    {
        public string Stars { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
    }
}
