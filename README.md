# SuperOffice WebApi Samples

The SuperOffice.WebApi nuget package contains the SuperOffice REST API client proxy agents. 

https://www.nuget.org/packages/SuperOffice.WebApi

`Install-Package SuperOffice.WebApi`

Feedback goes to sdk@superoffice.com, subject: SuperOffice.WebApi, or [create an issue](https://github.com/SuperOffice/SuperOffice.WebApi-Samples/issues/new) on this repo!

## About the samples

The samples use a pre-registered application called **SuperOffice DevNet WebApi Sample**. It is already registered and has the following details:

|Property            |Description                                                   |
|--------------------|--------------------------------------------------------------|
|Environment         |This application is  registed for __SOD__                     |
|ContextIdentifier   |The tenant identifier will be your SOD tenants ID (Cust12345) |
|ClientId            |857fd8fa9c83db5fa030b94d1bcc7b60 (for this sample)            |
|ClientSecret        |ca452f9c29870bc278017796cd16bd11 (for this sample)            |
|PrivateKey          |See the code sample Program.cs, line ca. ~200.                |

## About SuperOffice.WebApi

First things first! **This is not an OAuth 2.0 client**. When authenticating with OpenID Connect or OAuth, you still have to use another means to obtain OAuth tokens. 

For OAuth/OpenID Connect authentication in web applications, we recommend you take a look at 
[AspNet.Security.OAuth.SuperOffice](https://www.nuget.org/packages/AspNet.Security.OAuth.SuperOffice/) provider. It's an open source library created by DevNet, 
and tailored for SuperOffice online.

Now lets begin looking how to use the SuperOffice.WebApi library.

## What is SuperOffice.WebApi

This library is a .NET Standard 2.0 library that works with both .NET Framework (4.6.1 and higher) and .NET Core (2.0 and higher) applications.

Its purpose is to provide an alternative to the existing SuperOffice NetServer WCF proxies. **SuperOffice.WebApi** provides the exact same Agent-style services 
as [SuperOffice.NetServer.Services](https://www.nuget.org/packages/SuperOffice.NetServer.Services), while adopting modern practices, such as asynchronous methods.

This library makes it easier to work in a multi-tenant environment. It isolates a tenants' context in a **WebApiOptions** instance, where each instance is configured 
to target one specific tenant. Each instance can be configured with its own language, culture and timezone settings. Each request is configurable to adjust the 
culture and timezone settings, as well as pass in a CancellationToken.

This library does not contain built-in **system user token** support. More on that below.

OK. So that's what the SuperOffice.WebApi library is, now lets see how to use it.

## How to use SuperOffice.WebApi

1) Instantiate a WebApiOptions instance.

  * The primary constructor accepts the target web api URL, i.e. https://sod.superoffice.com/cust12345/api.
	
	`WebApiOptions(string baseUrl);`
			
	WebApiOptions inherits from RequestOptions, which contain the internationalization settings. These settings can also be passes into the overloaded contructor.
	
    ```C#
	WebApiOptions(
		string baseUrl, 
		IAuthorization authorization, 
		string languageCode = null, 
		string timeZone = null, 
		bool verifyUrl = true
	);
    ```

2) Define the IAuthorization credential type.
	
	The IAuthorization parameter is used to define the credential type that sets the Authorization header in each HTTP request. 

	SuperOffice.WebApi depends on the SuperOffice.WebApi.Authorization package, which contains three default Authorization types. 
	See `About IAuthorization` for more information about Authorization extensibility in the next section.

	|Authorization Type           | Used in Online | Used in Onsite |
	|-----------------------------|:--------------:|:--------------:|
	|AuthorizationTicket          |                | X              |
	|AuthorizationUsernamePassword|                | X              |
    |AuthorizationImplicit        |                | X              |

	Assign an instance to the `WebApiOptions.Authorization` property.
    
	```C#
    var auth = new AuthorizationUsernamePassword("jack@black.com", "TenaciousD!");
    var config = new WebApiOptions(tenant.WebApiUrl, auth);
    ```

    or

	```C#
    var config = new WebApiOptions(tenant.WebApiUrl);
	config.Authorization = 
		new AuthorizationUsernamePassword("jack@black.com", "TenaciousD!");
    ```

3) Create the desired Agent class, passing in the WebApiOptions as a constructor parameter. This is the only Agent difference when compared to using the existing WCF proxies.

	```C#
	var contactAgent = new ContactAgent(config);
	```

	Now your code is able to make the same calls as before.	The biggest change is that they are now asynchronous and can use the async await pattern!
	
	```C#
	await contactAgent.GetContactEntityAsync(contactId);
	```

## About IAuthorization

The IAuthorization interface is used to define the credential type that sets the Authorization header in each HTTP request. It is an interface anyone can implement and assign to 
the WebApiOptions.Authorization property to populate and optionally refresh the Authorization header value for all http requests.

There are cases where a users credentials will expire. It's probably easiest to say that the only case where credentials will not expire at runtime is using AuthorizationUsernamePassword. 
All other IAuthorization implementations contain time-sensitive credentials.

The SuperOffice WebApi Authorization library is extended by other packages with built-in support to automatically refresh credentials.

| Package name                                    |Authorization Type           | Used in Online | Used in Onsite |
|:------------------------------------------------|-----------------------------|:--------------:|:--------------:|
|SuperOffice.WebApi.Authorization.AccessToken     |AuthorizationAccessToken     |X               |                |
|SuperOffice.WebApi.Authorization.SystemUserTicket|AuthorizationSystemUserTicket|X               |                |
|SuperOffice.WebApi (only built-in Authorization) |AuthorizationUserToken       |                | X              |
	
Both `AuthorizationTicket` and `AuthorizationSystemUserTicket` credentials expire after 6 hours but only the `AuthorizationSystemUserTicket` implementation has automatic refresh support.

To auto-refresh `AuthorizationSystemUserTicket` requires the following information:

* Environment
* ContextIdentifier
* ClientSecret
* PrivateKey
* SystemUserToken

To auto-refresh `AuthorizationAccessToken` requires the following information:

* Access Token
* Refresh Token
* Redirect URI

To auto-refresh `AuthorizationUserToken` requires the following information:

* Username
* Password

The system user flow is discussed more in the [System User Client repository](https://github.com/SuperOffice/SuperOffice.SystemUser.Client).
	
### When is IAuthorization refreshed

This library takes a reactive approach and waits to receive an access denied response prior to attempting to refresh the Authorization.

When an access denied response is received, the client looks to make sure the WebApiOptions has an `IAuthorization.RefreshAuthorization` implementation. 
When present, and the RefreshAuthorization implementation succeeds, `RefreshAuthorization` returns an updated `IAuthorization`.

With an updated Authorization, the client then invokes the `IAuthorization.GetAuthorization` method to get a two-value tuple, the scheme and the parameter, 
and uses those to set the Authorization header.

`("Bearer", "8A:Cust12345:ABCdefg....XyZ")`

Finally, the client retries that original request with an updated scheme and parameter.

### Certificate Validation

Both `AuthorizationAccessToken` and `AuthorizationSystemUserTicket` validate the response from SuperOffice using the SuperOffice public key. 
The client does this by requesting the OAuth 2.0 metadata document from the online environment. This process takes two requests:

1) The first request is sent to obtain the OAuth 2.0 metadata document, and then extracts the jwks_uri.
2) The second request is sent to the jwks_uri to obtain the certificate details.

The benefit of this process is that the integration does not need to include any physical public certificates.

### Custom IAuthorization implementation

When you want to populate the Authorization header with a differnt scheme/parameter values, you can implement your own IAuthorization. The interface is simple. 

```C#
public interface IAuthorization
{
    Func<ReAuthorizationArgs, IAuthorization> RefreshAuthorization { get; set; }
    AuthenticationHeaderValue GetAuthorization();
}
```

RefreshAuthorization is a function that accepts a `ReAuthorizationArgs` and returns an `IAuthorization` instance with updated credential values.

The client updates the `WebApiOptions` and then calls the **GetAuthorization** method. `GetAuthorization` returns the scheme and parameter, which the client then uses to populate the request Authorization header.

## System User

This functionality has moved into the [SuperOffice.WebApi.Authorization.SystemUserTicket](https://www.nuget.org/packages/SuperOffice.WebApi.Authorization.SystemUserTicket) package, which depends on the 
[SuperOffice.SystemUser.Client](https://www.nuget.org/packages/SuperOffice.SystemUser.Client) package.

## Package Dependencies

See the [nuget package page](https://www.nuget.org/packages/SuperOffice.WebApi).

## Known Issues

None.

