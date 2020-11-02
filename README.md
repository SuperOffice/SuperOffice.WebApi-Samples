# SuperOffice WebApi Samples

The SuperOffice.WebApi nuget package is ready for preview. 

https://www.nuget.org/packages/SuperOffice.WebApi

`Install-Package SuperOffice.WebApi -Version 1.0.0-preview`

Please understand that this preview is still very young and untested in the wild. That's why we are sharing it with you! :-) Please take the time to give it a good test drive and provide your feedback.

Feedback can either go to sdk@superoffice.com, subject: SuperOffice.WebApi Preview, or even better, [create an issue](https://github.com/SuperOffice/SuperOffice.WebApi-Samples/issues/new) on this repo!

## About the samples

The samples use a pre-registered application called **SuperOffice DevNet WebApi Sample**. It is already registered and has the following details:

|Property            |Description                                              |
|--------------------|---------------------------------------------------------|
|Environment         |This application is  registed for __SOD__                |
|ContextIdentifier   |The tenant identifier will be one of your tenants        |
|SystemUserToken     |The SystemUser is issued at app approval on your tenant  |
|ClientId            |857fd8fa9c83db5fa030b94d1bcc7b60                         |
|ClientSecret        |ca452f9c29870bc278017796cd16bd11                         |
|PrivateKey          |See the code sample Program.cs, line 206.                |

To get a **system user token** and **access token**, sign-in using the [DevNet-Tokens](https://devnet-tokens.azurewebsites.net/account/signin) application using the provided client ID and secret in the table above. When redirected back the web site, your System User Token is displayed on the page.

Copy and paste both the System User token and Access Token from the page into each of the sample program.cs files to successfully run the code.

## About SuperOffice.WebApi

First things first! **This is not an OAuth 2.0 client**. When authenticating with OpenID Connect or OAuth, you still have to use another means to obtain OAuth tokens. 

For OAuth/OpenID Connect authentication in web applications, we recommend you take a look at [AspNet.Security.OAuth.SuperOffice](https://www.nuget.org/packages/AspNet.Security.OAuth.SuperOffice/) provider. It's an open source library created by DevNet, and tailored for SuperOffice online.

Now lets begin looking how to use the SuperOffice.WebApi library.

## What is SuperOffice.WebApi

This library is a .NET Standard 2.0 library that works with both .NET Framework (4.6.1 and higher) and .NET Core (2.0 and higher) applications.

Its purpose is to provide an alternative to the existing SuperOffice NetServer WCF proxies. **SuperOffice.WebApi** provides the exact same Agent-style services as [SuperOffice.NetServer.Services](https://www.nuget.org/packages/SuperOffice.NetServer.Services), while adopting modern practices, such as asynchronous methods. 

This library makes it easier to work in a multi-tenant environment. It isolates a tenants' context in a **WebApiConfiguration** instance, where each instance is configured to target one specific tenant. Each instance can be configured with its own language, culture and timezone settings.

This library also has built-in **system user token** support. More on that below.

OK. So that's what the SuperOffice.WebApi library is, now lets see how to use it.

## How to use SuperOffice.WebApi

1) Instantiate a WebApiConfiguration instance.
  * The primary constructor accepts the target web api URL, i.e. https://sod.superoffice.com/cust12345/api.
	
	`WebApiConfiguration(string baseUrl);`
			
	WebApiConfiguration inherits from RequestOptions, which contain the internationalization settings. 	These settings can also be passes into the overloaded contructor.
	
    ```C#
	WebApiConfiguration(
		string baseUrl, 
		IAuthorization authorization, 
		string languageCode = null, 
		string timeZone = null, 
		bool verifyUrl = true
	);
    ```

2) Define the IAuthorization credential type.
	
	The IAuthorization parameter is used to define the credential type, and used to set the Authorization attribute in each HTTP request. 

    There are 4 built-in IAuthorization implementations.

	|Authorization Type           | Used in Online | Used in Onsite |
	|-----------------------------|:--------------:|:--------------:|
	|AuthorizationAccessToken     |X               |                |
	|AuthorizationSystemUserTicket|X               |                |
	|AuthorizationTicket          |                | X              |
	|AuthorizationUsernamePassword|                | X              |

	Assign an instance to the `WebApiConfiguration.Authorization` property.
    
	```C#
    var config = new WebApiConfiguration(tenant.WebApiUrl);
	config.Authorization = 
		new AuthorizationUsernamePassword("jack@black.com", "TenaciousD!");
    ```

3) Create the desired Agent class, passing in the WebApiConfiguration as a constructor parameter. This is the only Agent difference when compared to using the existing WCF proxies.

	```C#
	var contactAgent = new ContactAgent(config);
	```

	Now your code is able to make the same calls as before.	The biggest change is that they are now asynchronous and can use the async await pattern!
	
	```C#
	await contactAgent.GetContactEntityAsync(contactId);
	```

## About IAuthorization

There are cases where a users credentials will expire. It's probably easiest to say that the only case where credentials will not expire at runtime is using AuthorizationUsernamePassword. All other IAuthorization implementations contain time-sensitive credentials.

The SuperOffice WebApi library has limited built-in support to automatically refresh credentials.
	
Both `AuthorizationTicket` and `AuthorizationSystemUserTicket` credentials expire after 6 hours but only the `AuthorizationSystemUserTicket` implementation has automatic refresh support.

To auto-refresh `AuthorizationSystemUserTicket` requires the following information:

* Environment
* ContextIdentifier
* ClientSecret
* PrivateKey
* SystemUserToken

To auto-refresh `AuthorizationAccessToken` requires the following information:

* Access Token
* Refresh Token
* Redirect URI

The system user flow is discussed more in the System User section below.
	
### When is IAuthorization refreshed

This library takes a reactive approach and waits to receive an access denied response prior to attempting to refresh the Authorization.

When an access denied response is received, the client looks to make sure the WebApiConfiguration has an `IAuthorization.RefreshAuthorization` implementation. When present, and the RefreshAuthorization implementation succeeds, `RefreshAuthorization` returns an updated `IAuthorization`.

With an updated Authorization, the client then invokes the `IAuthorization.GetAuthorization` method to get a two-value tuple, the scheme and the parameter, and uses those to set the Authorization header.

`("Bearer", "8A:Cust12345:ABCdefg....XyZ")`

Finally, the client retries that original request with an updated scheme and parameter.

### Certificate Validation

Both `AuthorizationAccessToken` and `AuthorizationSystemUserTicket` validate the response from SuperOffice using the SuperOffice public key. The client does this by requesting the OAuth 2.0 metadata document from the online environment. This process takes two requests:

1) The first request is sent to obtain the OAuth 2.0 metadata document, and then extracts the jwks_uri.
2) The second request is sent to the jwks_uri to obtain the certificate details.

The benefit of this process is that the integration does not need to include any physical public certificates.

### Custom IAuthorization implementation

If for some unknown reason you want to populate the Authorization header with a differnt scheme/parameter values, you can implement your own IAuthorization. The interface is simple. 

```C#
public interface IAuthorization
{
    Func<ReAuthorizationArgs, IAuthorization> RefreshAuthorization { get; set; }
    (string scheme, string parameter) GetAuthorization();
}
```

RefreshAuthorization is a function that accepts a `ReAuthorizationArgs` and returns an `IAuthorization` instance with updated credential values.

The client updates the `WebApiConfiguration` and then calls the **GetAuthorization** method. `GetAuthorization` returns the scheme and parameter, which the client then uses to populate the request Authorization header.

## System User

This library supports the System User flow. The client makes it very easy to call the online PartnerSystemUserService endpoint, validate the JWT and return the claims it contains.

The JWT contains a lot of information, however, it's usually just the Ticket credential that is interesting. Therefore, **SuperOffice.WebApi** simplifies calling the service, validating the response, and then returning the ticket in a single method call.

> :warning: Do **not** ask for a System User Ticket every single time you invoke an Agent method! This creates a new credentials record in the database each and every time. **Take advantage of the 6 hour window** and only ask for a new Ticket when absolutely necessary!

### How to use System User flow

Use the SystemUserClient class, located in the `SuperOffice.WebApi.IdentityModel` namespace.

The constructor accepts a `SystemUserInfo` instance, and contains all of the information required to submit a request to the _partnersystemuserservice.svc_ endpoint.

#### SystemUserInfo Properties

|Property            |Description                                        |
|--------------------|---------------------------------------------------|
|Environment         |The online environment (SOD, Stage, Production.    |
|ContextIdentifier   |The tenant, or customer, identity.                 |
|ClientSecret        |The application secret, a.k.a. client_secret.      |
|PrivateKey          |The applications RSAXML private certificate value. |
|SystemUserToken     |The SystemUser token, issued during app approval.  |

Given the required information, the `SystemUserClient` is able to generate and send a request to the service, then receive and validate the response.

```C#
var sysUserClient = new SystemUserClient(systemUserInfo);
var sysUserJwt = sysUserClient.GetSystemUserJwt();
var sysUserTkt = sysUserClient.GetSystemUserTicket();
```

The **GetSystemUserJWT**, only returns the JWT, wrapped in a `SystemUserResult`. It does not validate or extract any claims. To perform validation and extract claims, the `SystemUserClient` uses the `JwtTokenHandler`, located in the `SuperOffice.WebApi.IdentityModel` namespace.

```C#
var handler = new JwtTokenHandler(
    "YOUR_CLIENT_ID",                 // Application ID, A.K.A. client_id
    new System.Net.Http.HttpClient(), // HttpClient instance.
    OnlineEnvironment.SOD             // target online environment (SOD, Stage or Production)
    );

var tokenValidationResult = handler.ValidateAsync(sysUserJwt.Token);
```

The method `JwtTokenHandler.ValidateAsync` returns a TokenValidationResult, a Microsoft datatype located in the [Microsoft.IdentityModel.JsonWebTokens](https://docs.microsoft.com/en-us/dotnet/api/microsoft.identitymodel.tokens.tokenvalidationresult) namespace, in the `Microsoft.IdentityModel.JsonWebTokens` assembly. 

## Package Dependencies

These will automatically be included when you add the SuperOffice.WebApi package to a project.

.NETStandard 2.0

* Microsoft.IdentityModel.JsonWebTokens (>= 5.6.0)
* Microsoft.IdentityModel.Logging (>= 5.6.0)
* Microsoft.IdentityModel.Tokens (>= 5.6.0)
* Newtonsoft.Json (>= 12.0.2)
* System.Security.Permissions (>= 4.7.0)

## Known Issues

The current SuperOffice.WebApi package dependencies are not the latest Microsoft packages available. Therefore, if your project uses newer versions of the Microsoft packages, there will be conflicts with TokenValidationResult.

The current library does not have any logging, and very little exception handling.

