using System;

namespace Clients.Bff.InMemoryTests
{
    public class AuthSettings : IValidatable
    {
        public const string SettingsKey = "auth";


        public string TenantedAuthorityFormat { get; set; }
        public string TenantedClientId { get; set; }
        public string TenantedClientSecret { get; set; }

        public string TenantlessAuthority { get; set; }
        public string TenantlessClientId { get; set; }
        public string TenantlessClientSecret { get; set; }


        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(TenantedAuthorityFormat))
            {
                throw new FormatException($"{nameof(TenantedAuthorityFormat)} must be a valid URI");
            }

            if (string.IsNullOrWhiteSpace(TenantedClientId))
            {
                throw new FormatException($"{nameof(TenantedClientId)} cannot be null or empty");
            }

            if (!Uri.IsWellFormedUriString(TenantlessAuthority, UriKind.Absolute))
            {
                throw new FormatException($"{nameof(TenantlessAuthority)} must be a valid URI");
            }

            if (string.IsNullOrWhiteSpace(TenantlessClientId))
            {
                throw new FormatException($"{nameof(TenantlessClientId)} cannot be null or empty");
            }
        }
    }
}