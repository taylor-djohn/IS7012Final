using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using IS7012Final.Model;
using IS7012Final.Data;

namespace IS7012Final.Pages.MoviePages;

public class DeleteModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public DeleteModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public IS7012Final.Model.Movie Movie { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var movie = await _context.Movie.FirstOrDefaultAsync(m => m.Id == id);
        if (movie is null)
        {
            return NotFound();
        }
        else
        {
            Movie = movie;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var movie = await _context.Movie.FindAsync(id);
        if (movie != null)
        {
            Movie = movie;
            _context.Movie.Remove(Movie);
            await _context.SaveChangesAsync();
        }

        return RedirectToPage("./Index");
    }
}
