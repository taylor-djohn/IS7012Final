using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using IS7012Final.Model;

namespace IS7012Final.Areas.Identity.Pages.Account.Manage
{
    [Authorize]
    public class EmailModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public EmailModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public string CurrentEmail { get; set; }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            CurrentEmail = user?.Email;
            Input = new InputModel { Email = CurrentEmail };
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login", new { area = "Identity" });

            if (Input.Email != user.Email)
            {
                var setEmail = await _userManager.SetEmailAsync(user, Input.Email);
                if (!setEmail.Succeeded)
                {
                    foreach (var e in setEmail.Errors) ModelState.AddModelError(string.Empty, e.Description);
                    return Page();
                }
            }

            await _signInManager.RefreshSignInAsync(user);
            TempData["StatusMessage"] = "Your email has been updated.";
            return RedirectToPage();
        }
    }
}
