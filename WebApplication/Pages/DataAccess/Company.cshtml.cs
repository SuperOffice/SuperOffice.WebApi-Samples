using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNet.Security.OAuth.SuperOffice;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using SuperOffice.WebApi;
using SuperOffice.WebApi.Agents;
using SuperOffice.WebApi.Authorization;
using SuperOffice.WebApi.Data;
using T = System.Threading.Tasks;

namespace WebApplication.Pages.DataAccess
{
    public class CompanyModel : PageModel
    {
        private readonly IOptionsMonitor<SuperOfficeAuthenticationOptions> _superOfficeOptions;

        public CompanyModel(IOptionsMonitor<SuperOfficeAuthenticationOptions> suoptions)
        {
            _superOfficeOptions = suoptions;
        }

        [BindProperty(SupportsGet = true)]
        public int SearchId { get; set; }

        public ContactEntity ContactEntity { get; set; }

        public async T.Task OnGetAsync()
        {
            if (SearchId > 0)
            {
                var authenticationInfo = HttpContext.AuthenticateAsync()?.Result;
                if (authenticationInfo != null)
                {
                    // could use "User.Claims", but still need AuthInfo to access Tokens...
                    
                    var webApiUrl = authenticationInfo.Principal.Claims.Where(c => c.Type.Contains("webapi", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                    var clientSettings = _superOfficeOptions.Get(SuperOfficeAuthenticationDefaults.AuthenticationScheme);
                    var callbackUri = $"{this.Request.Scheme}://{this.Request.Host}{clientSettings.CallbackPath}";

                    var authorization = new AuthorizationAccessToken(
                    authenticationInfo.Properties.GetTokenValue("access_token"),
                    authenticationInfo.Properties.GetTokenValue("refresh_token"),
                    clientSettings.ClientId,
                    clientSettings.ClientSecret,
                    callbackUri,
                    GetEnvironment(clientSettings.Environment)
                    );

                    var config = new WebApiOptions(webApiUrl.Value, authorization);

                    ContactAgent ca = new ContactAgent(config);
                    ContactEntity = await ca.GetContactEntityAsync(SearchId);
                }
            }
        }

        private string GetEnvironment(SuperOfficeAuthenticationEnvironment environment)
        {
            switch (environment)
            {
                case SuperOfficeAuthenticationEnvironment.Development:
                    return SubDomain.Development;
                case SuperOfficeAuthenticationEnvironment.Stage:
                    return SubDomain.Stage;
                case SuperOfficeAuthenticationEnvironment.Production:
                    return SubDomain.Production;
                default:
                    throw new Exception("Should not get here...");
            };
        }
    }
}
