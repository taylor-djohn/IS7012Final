using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using IS7012Final.Model;
using IS7012Final.Data;

namespace IS7012Final.Pages.MoviePages;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public IList<IS7012Final.Model.Movie> Movie { get; set; } = default!;

    public async Task OnGetAsync()
    {
        Movie = await _context.Movie.ToListAsync();
    }
}
