using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages.Authentication
{
    [Authorize(AuthenticationSchemes = "Discord")]
    public class LoginModel : PageModel
    {
        public async Task<IActionResult> OnGet()
        {
            return Redirect("~/Home/Guilds");
        }
    }
}
