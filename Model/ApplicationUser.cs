using Microsoft.AspNetCore.Identity;

namespace IS7012Final.Model
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
}
