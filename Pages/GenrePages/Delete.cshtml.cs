using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using IS7012Final.Model;
using IS7012Final.Data;

namespace IS7012Final.Pages.GenrePages;

public class DeleteModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public DeleteModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Genre Genre { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var genre = await _context.Genre.FirstOrDefaultAsync(m => m.Id == id);
        if (genre is null)
        {
            return NotFound();
        }
        else
        {
            Genre = genre;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var genre = await _context.Genre.FindAsync(id);
        if (genre != null)
        {
            Genre = genre;
            _context.Genre.Remove(Genre);
            await _context.SaveChangesAsync();
        }

        return RedirectToPage("./Index");
    }
}
