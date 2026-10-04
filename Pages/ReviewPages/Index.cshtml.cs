using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using IS7012Final.Model;
using IS7012Final.Data;

namespace IS7012Final.Pages.ReviewPages;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public IList<Review> Review { get; set; } = default!;

    public async Task OnGetAsync()
    {
            Review = await _context.Review
                .Include(r => r.Movie)
                .OrderByDescending(r => r.Timestamp)
                .ToListAsync();
    }
}
