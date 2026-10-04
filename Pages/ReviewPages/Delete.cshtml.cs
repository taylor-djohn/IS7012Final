using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using IS7012Final.Model;
using IS7012Final.Data;

namespace IS7012Final.Pages.ReviewPages;

public class DeleteModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public DeleteModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Review Review { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var review = await _context.Review.FirstOrDefaultAsync(m => m.Id == id);
        if (review is null)
        {
            return NotFound();
        }
        else
        {
            Review = review;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var review = await _context.Review.FindAsync(id);
        if (review != null)
        {
            Review = review;
            _context.Review.Remove(Review);
            await _context.SaveChangesAsync();
        }

        return RedirectToPage("./Index");
    }
}
