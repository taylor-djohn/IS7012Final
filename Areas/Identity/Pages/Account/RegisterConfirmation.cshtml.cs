using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;

namespace IS7012Final.Areas.Identity.Pages.Account
{
    public class RegisterConfirmationModel : PageModel
    {
        public string Email { get; set; }
        public string ReturnUrl { get; set; }
        public string ConfirmationLink { get; set; }

        public void OnGet(string email = null, string returnUrl = null)
        {
            Email = email;
            ReturnUrl = returnUrl;
            ConfirmationLink = TempData["ConfirmationLink"] as string;
        }
    }
}
