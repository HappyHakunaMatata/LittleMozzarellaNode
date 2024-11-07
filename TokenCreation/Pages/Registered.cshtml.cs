using System.Security.Claims;
using System.Xml.Linq;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TokenCreation.Pages
{
	public class RegisteredModel : PageModel
    {

        public string token = "";
        private readonly SignInManager<IdentityUser> _signInManager;

        public RegisteredModel(SignInManager<IdentityUser> signInManager)
        {
            _signInManager = signInManager;
        }

        public async Task<IActionResult> OnGet()
        {
            var ctx = HttpContext.User.Identity;
            if (ctx == null)
            {
                return RedirectToPage("./Index");
            }
            if (!ctx.IsAuthenticated)
            {
                return RedirectToPage("./Index");
            }
            var  token = HttpContext.User.Claims.Reverse().FirstOrDefault(field => field.Type == "Token");
            var email = HttpContext.User.Claims.FirstOrDefault(field => field.Type == ClaimTypes.Email);
            if (token == null || email == null)
            {
                await _signInManager.SignOutAsync();
                return RedirectToPage("./Index");
            }
            this.token = $"{email.Value}:{token.Value}";
            return Page();
        }
    }
}
