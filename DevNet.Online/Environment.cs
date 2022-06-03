using SuperOffice.WebApi;
using SuperOffice.WebApi.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace DevNet.Online
{
    public class SuperOfficeEnvironment
    {
        private string _environment;
        private string _tenantStatusUrl;
        private string _metadataEndpoint;
        private string _claimsIssuer;
        private string _authority;

        public const string _tenantStatus = "https://{0}.superoffice.com/api/state/";

        public string Authority => _authority;
        public string ClaimsIssuer => _claimsIssuer;
        public string MetaDataEndpoint => _metadataEndpoint;
        public string TenantStatusUrl => _tenantStatusUrl;

        public string Current 
        {
            get
            {
                return _environment;
            }

            set
            {
                _environment = value;
                SetProperties();
            }
        }

        public SuperOfficeEnvironment(string subdomain)
        {
            _environment = subdomain;
            SetProperties();
        }


        private void SetProperties()
        {
            var environment = GetEnvironment();
            _authority = string.Format(SuperOffice.SystemUser.Constants.OAuth.Authority, environment);
            _claimsIssuer = string.Format(SuperOffice.SystemUser.Constants.OAuth.ClaimsIssuer, environment);
            _metadataEndpoint = string.Format(SuperOffice.SystemUser.Constants.OAuth.MetadataEndpoint, environment);
            _tenantStatusUrl = string.Format(_tenantStatus, _metadataEndpoint);
        }
        
        /// <summary>
        /// Used to determine current target environment.
        /// </summary>
        /// <returns>Returns the online environment, i.e. development, stage, production.</returns>
        private string GetEnvironment()
        {
            return _environment switch
            {
                SubDomain.Development => "sod",
                SubDomain.Stage => "qaonline",
                SubDomain.Production => "online",
                _ => throw new NotSupportedException("Environment property must be set to either Development, Stage or Production."),
            };
        }

    }
}
