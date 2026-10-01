using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using IS7012Final.Model;

namespace IS7012Final.Pages.MoviePages;

public class PopularModel : PageModel
{
    public List<Movie>? Movie { get; set; }

    public void OnGet()
    {
        var json = System.IO.File.ReadAllText("wwwroot/data/movies.json");
        Movie = JsonSerializer.Deserialize<List<Movie>>(json);
    }
}

public class Movie
{
    public string? MovieId { get; set; }
    public string? Title { get; set; }
    public string? Poster { get; set; }
    public List<string>? Genres { get; set; }
    public string? Release_Date { get; set; }
    public double Rating { get; set; }
    public string? Overview { get; set; }
}

