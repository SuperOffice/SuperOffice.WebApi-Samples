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
                    authenticationInfo.Properties.GetTokenValue(Constants.OAuth.AccessToken),
                    authenticationInfo.Properties.GetTokenValue(Constants.OAuth.IdToken),
                    authenticationInfo.Properties.GetTokenValue(Constants.OAuth.RefreshToken),
                    clientSettings.ClientId,
                    clientSettings.ClientSecret,
                    callbackUri,
                    GetEnvironment(clientSettings.Environment)
                    );

                    var config = new WebApiConfiguration(webApiUrl.Value, authorization);

                    ContactAgent ca = new ContactAgent(config);
                    ContactEntity = await ca.GetContactEntityAsync(SearchId);
                }
            }
        }

        private OnlineEnvironment GetEnvironment(SuperOfficeAuthenticationEnvironment environment)
        {
            switch (environment)
            {
                case SuperOfficeAuthenticationEnvironment.Development:
                    return OnlineEnvironment.SOD;
                case SuperOfficeAuthenticationEnvironment.Stage:
                    return OnlineEnvironment.Stage;
                case SuperOfficeAuthenticationEnvironment.Production:
                    return OnlineEnvironment.Production;
                default:
                    throw new Exception("Should not get here...");
            };
        }
    }
}
