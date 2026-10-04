using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using IS7012Final.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace IS7012Final.Areas.Identity.Pages.Account.Manage
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ILogger<IndexModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        public string Email { get; private set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            public string Email { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }

        public async Task OnGetAsync()
        {
            _logger.LogInformation("Manage/Index requested. IsAuthenticated={IsAuthenticated}, Name={Name}", User.Identity?.IsAuthenticated, User.Identity?.Name);
            var user = await _userManager.GetUserAsync(User);
            Email = user?.Email ?? "";
            Input = new InputModel
            {
                Email = user?.Email,
                FirstName = user?.FirstName,
                LastName = user?.LastName
            };
        }

        public async Task<IActionResult> OnPostAsync()
        {
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

            user.FirstName = Input.FirstName;
            user.LastName = Input.LastName;
            
            var update = await _userManager.UpdateAsync(user);
            if (!update.Succeeded)
            {
                foreach (var e in update.Errors) ModelState.AddModelError(string.Empty, e.Description);
                return Page();
            }

            await _signInManager.RefreshSignInAsync(user);
            return RedirectToPage();
        }
    }
}
