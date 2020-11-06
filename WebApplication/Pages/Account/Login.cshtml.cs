using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication;

namespace WebApplication.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly IAuthenticationSchemeProvider _schemeProvider;

        public LoginModel(IAuthenticationSchemeProvider schemeProvider)
        {
            _schemeProvider = schemeProvider;
        }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        public async void OnGet()
        {
            ExternalLogins = (from scheme in await _schemeProvider.GetAllSchemesAsync()
                                    where !string.IsNullOrEmpty(scheme.DisplayName)
                                    select scheme).ToArray();
        }

        
    }
}
