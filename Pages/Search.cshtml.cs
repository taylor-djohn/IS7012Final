using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using IS7012Final.Pages.GenrePages;
using System.Text.Json;

namespace IS7012Final.Pages
{
    public class SearchModel : PageModel
    {
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

                var allMovies =
                    JsonSerializer.Deserialize<List<JsonMovie>>(json, options)
                    ?? new List<JsonMovie>();

                Movies = allMovies
                    .Where(m => m.Title != null &&
                        m.Title.Contains(
                            SearchTerm,
                            StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
        }
    }
}
