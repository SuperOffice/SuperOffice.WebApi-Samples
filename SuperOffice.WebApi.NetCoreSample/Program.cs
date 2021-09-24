// ************************************************************************************ //
// ************************************************************************************ //
//     ____                  ____  _______           ___           _  __    __          //
//    / __/_ _____  ___ ____/ __ \/ _/ _(_)______   / _ \___ _  __/ |/ /__ / /_         //
//   _\ \/ // / _ \/ -_) __/ /_/ / _/ _/ / __/ -_) / // / -_) |/ /    / -_) __/         //
//  /___/\_,_/ .__/\__/_/  \____/_//_//_/\__/\__/ /____/\__/|___/_/|_/\__/\__/          //
//          /_/                                                                         //
// ************************************************************************************ //
// ************************************************************************************ //
// Samples for your learning enjoyment. Please submit feedback to sdk@superoffice.com   //
// ************************************************************************************ //
// Open Package Manager Console, enter Update-Package -reinstall                        //
//                                                                                      //
// Otherwise...                                                                         //
//                                                                                      //
// Open Package Manager Console, enter UnInstall-Package SuperOffice.WebApi, then...    //
// Enter Install-Package SuperOffice.WebApi -Version 1.0.0-preview                      //
// ************************************************************************************ //
using DevNet.Online;
using SuperOffice.WebApi;
using SuperOffice.WebApi.Agents;
using SuperOffice.WebApi.Data;
using SuperOffice.WebApi.IdentityModel;
using System;
using System.Threading.Tasks;

namespace SuperOffice.WebApi.FullFrameworkSample
{
    class Program
    {

        private ApplicationContext _appContext;
        public ApplicationContext ApplicationContext => _appContext;

        public Program()
        {
            // make sure to update the application context before starting!!!
            // only three pieces of information are require to run this sample:

            // 1. Tenant ID, i.e. Cust12345, 
            // 2. System User Token, get this by approving the app...
            // 3. OAuth access_token

            // Use https://devnet-tokens.azurewebsites.net/account/signin
            // to get an access_token for this call...(dev environment)
            // the redirectUri is already wired up for this client_id.

            _appContext = GetAppContext(
                // tenant context identifier
                "Cust12345",
                // system user token
                "SuperOffice DevNet WebApi Sample-Update_Me",
                // access token
                "8A:Cust12345.AY...bT9"
                );
        }

        static async System.Threading.Tasks.Task Main(string[] args)
        {
            Program p = new Program();

            var tenantStatus = p.GetTenantStatus(p.ApplicationContext.Tenant);
            var apiSysemInfo = await p.GetSystemInfo(p.ApplicationContext.Tenant);

            foreach (var item in apiSysemInfo)
            {
                Console.WriteLine($"Key: {item.Key}, Value: {item.Value}");
            }

            ContactEntity company = await p.GetContactEntityAsync(12, p.ApplicationContext.Tenant);
            Console.WriteLine($"Name: {company.Name}, Dept: {company.Department}");


            PersonEntity contact = await p.GetPersonEntityAsync(5, p.ApplicationContext.Tenant);
            Console.WriteLine($"Name: {contact.Firstname} {contact.Lastname}");

            // wait for everything to return...
            Console.ReadLine();
        }

        private async Task<ContactEntity> GetContactEntityAsync(int contactId, Tenant tenant)
        {
            // maybe the tenant canceled their subscription...
            // make sure the tenant is running (not off-line or in backup or maintenance mode)

            var tenantStatus = GetTenantStatus(tenant);
            if (tenantStatus.IsRunning)
            {
                var sysUserInfo = GetSystemUserInfo();
                var sysUserTicket = await GetSystemUserTicket(sysUserInfo);

                var config = new WebApiOptions(tenant.WebApiUrl);
                config.Authorization = new AuthorizationSystemUserTicket(sysUserInfo, sysUserTicket);

                //config.LanguageCode = "en";
                //config.CultureCode = "en";
                //config.TimeZone = "UTC";

                var contactAgent = new ContactAgent(config);
                return await contactAgent.GetContactEntityAsync(contactId);
            }

            return null;
        }

        private async Task<PersonEntity> GetPersonEntityAsync(int personId, Tenant tenant)
        {
            // maybe the tenant canceled their subscription...
            // make sure the tenant is running (not off-line or in backup or maintenance mode)

            var tenantStatus = GetTenantStatus(tenant);
            if (tenantStatus.IsRunning)
            {
                var config = new WebApiOptions(tenant.WebApiUrl);
                config.Authorization = GetAccessTokenAuthorization();


                var personAgent = new PersonAgent(config);
                return await personAgent.GetPersonEntityAsync(personId);
            }

            return null;
        }

        private async Task<StringDictionary> GetSystemInfo(Tenant tenant)
        {
            WebApiOptions session = new WebApiOptions(tenant.WebApiUrl);

            // no authorization necessary for getting system info...

            var agent = new ApiAgent(session);
            return await agent.GetApiVersionAsync();
        }

        private TenantStatus GetTenantStatus(Tenant tenant)
        {
            WebApiOptions session = new WebApiOptions(tenant.WebApiUrl);

            // no authorization necessary for getting tenant status...

            var agent = new ApiAgent(session);
            return agent.GetTenantStatusAsync(tenant.ContextIdentifier, tenant.Environment.Current).Result;
        }

        private async Task<string> GetSystemUserTicket(SystemUserInfo systemUserInfo)
        {
            var sysUserClient = new SystemUserClient(systemUserInfo);

            var ticket = await sysUserClient.GetSystemUserTicketAsync();
            
            foreach (var claim in sysUserClient.ClaimsIdentity?.Claims)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"type: {claim.Type}, value: {claim.Value}"
                    );
            }

            return ticket;
        }

        private SystemUserInfo GetSystemUserInfo()
        {
            return new SystemUserInfo()
            {
                Environment = ApplicationContext.Tenant.Environment.Current,
                ContextIdentifier = ApplicationContext.Tenant.ContextIdentifier,
                ClientSecret = ApplicationContext.Application.ClientSecret,
                PrivateKey = ApplicationContext.Application.PrivateKey,
                SystemUserToken = ApplicationContext.Tenant.ApplicationAuthorization.SystemUserToken
            };
        }

        private AuthorizationAccessToken GetAccessTokenAuthorization()
        {
            return new AuthorizationAccessToken(
                ApplicationContext.ApplicationUser.AuthTokens.AccessToken,
                ApplicationContext.Tenant.Environment.Current
                );
        }

        private ApplicationContext GetAppContext(string contextIdentifer, string systemUserToken, string accessToken)
        {
            return new ApplicationContext()
            {
                Application = GetApplication(),

                ApplicationUser = new ApplicationUser()
                {
                    // Use https://devnet-tokens.azurewebsites.net/account/signin
                    // to get an access_token for this call...
                    // the redirectUri is already wired up for this client_id.

                    AuthTokens = new OAuthTokens()
                    {
                        AccessToken = accessToken,
                        IdToken = "",
                        RefreshToken = ""
                    }
                },

                Tenant = new Tenant(contextIdentifer, new SuperOfficeEnvironment(OnlineEnvironment.SOD))
                {
                    ApplicationAuthorization = new ApplicationAuthorization()
                    {
                        SystemUserToken = systemUserToken
                    }
                }
            };
        }

        private Application GetApplication()
        {
            return new Application()
            {
                ClientId = "857fd8fa9c83db5fa030b94d1bcc7b60",
                ClientSecret = "ca452f9c29870bc278017796cd16bd11",
                ID = 1,
                Name = "SuperOffice DevNet WebApi Sample",
                PrivateKey = @"<RSAKeyValue>
  <Modulus>zEQhXPTH7SjXutGIaO1JvebvkxmFd3xnPKZexgsx+SiYLUyOUNQG5u4rgF3ZuH4iGZdSfn9f9A3512/J+K2QRKPVA+0GZb25y2o0QEcFt7HP9GOkmwVLJfP2yUitCi1+U8qKOfpPbPPST1v5PTiwbrGQd+CLtl7ScvrKyOsA4S0=</Modulus>
  <Exponent>AQAB</Exponent>
  <P>3bzCzoZ289lErJ9S2fr3LA+OlvoDk3uPzVQ/STB8zUUvKqcA2wUzNDq8dTxWXOf7Y+QZu8srUfaft+FkbPUt5w==</P>
  <Q>69RDNwIN2lyVE1GFOeqYfXKvodUqGQYxSNdPcIdqL1dEDfxt34eaMz0uRKXTuvT47cC3CJrmXf0fQJqh7SFNyw==</Q>
  <DP>HFDLi7YOIKuhIm4iFWYABGdkLRF2PXIs9eqJPl5rwYbRNCApcs6iMExD3rC60phpOONbCeky+f+Fe+TTfzp8Bw==</DP>
  <DQ>pvzok1zq/kIsdT92POp3C+1XnBpa8tlFsLR1VdMtR1RdpiGmk29rqviZeJaLdIjec0vQz1EP6mG/7XkRS94XPQ==</DQ>
  <InverseQ>ZsQXkH8AF7vM42LS1vObpSIMScPU/u2J209QD7PviQmH02H3UWVqHO9HRS+MRKRWk264B6J0Zioub1r2oxjGjg==</InverseQ>
  <D>AWV11OzXcQeWcfB8vjrhBitN9/N0thxjmEaK30+0R7+/So/7aRIJ+gomwfniQyCZmxMtvS+huElgK9jXyJtnIQ3c6R9r/N2mSUCgtVWkIqHr5hLk4pp4xfe+K+hORK9Q2/gjW8BT1ENhWs7A2aCf0XZWZg4LM9T+MLzTTQEwmQE=</D>
</RSAKeyValue>",
                RedirectUri = new Uri("https://devnet-tokens.azurewebsites.net/openid/callback")
            };
        }

        /// <summary>
        /// Use OAuth Code flow to get access_token, id_token and refresh_token,
        /// then set those values in an AuthorizatinAccessToken for auto refresh
        /// in the WebApi client.
        /// </summary>
        /// <returns></returns>
        private AuthorizationAccessToken GetRefreshableAccessTokenAuthorization()
        {
            return new AuthorizationAccessToken(
               ApplicationContext.ApplicationUser.AuthTokens.AccessToken, // access_token
               ApplicationContext.ApplicationUser.AuthTokens.IdToken,     // id_token
               ApplicationContext.ApplicationUser.AuthTokens.RefreshToken,// refresh_token
               ApplicationContext.Application.ClientId,                   // client_id
               ApplicationContext.Application.ClientSecret,               // client_secret
               ApplicationContext.Application.RedirectUri.OriginalString, // redirect_uri
               ApplicationContext.Tenant.Environment.Current              // on-line environment
                );
        }
    }
}
