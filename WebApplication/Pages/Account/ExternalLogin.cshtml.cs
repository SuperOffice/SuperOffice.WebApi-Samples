using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApplication.Pages.Account
{
    public class ExternalLoginModel : PageModel
    {
        [TempData]
        public string ErrorMessage { get; set; }

        public IEnumerable<Claim> Claims { get; private set; }

        public string LoginProvider { get; set; }

        public IEnumerable<AuthenticationToken> Tokens { get; private set; }

        public void OnGet()
        {
            RedirectToPage("./Login");
        }

        public IActionResult OnPost(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
            var properties = new AuthenticationProperties
            {
                RedirectUri = redirectUrl,
                Items =
                {
                    new KeyValuePair<string, string>("LoginProvider", provider)
                }
            };
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToPage("./Login");
            }

            var authResult = await HttpContext.AuthenticateAsync();
            if (!authResult.Succeeded)
            { 
                return RedirectToPage("./Login");
            }

            LoginProvider = authResult.Properties.GetString("LoginProvider");
            Tokens = authResult.Properties.GetTokens();
            Claims = authResult.Principal.Claims;

            return Page();
        }
    }
}
