using System;

namespace DevNet.Online
{
    public class Tenant
    {
        public int ID { get; set; }

        public ApplicationAuthorization ApplicationAuthorization { get; set; }

        public string ContextIdentifier { get; set; }

        public SuperOfficeEnvironment Environment { get; set; }

        public string WebApiUrl => $"{Environment.ClaimsIssuer}/{ContextIdentifier}/api/";

        public Tenant(string contextIdentifer, SuperOfficeEnvironment environment)
        {
            ContextIdentifier = contextIdentifer;
            Environment = environment;
        }
    }
}
