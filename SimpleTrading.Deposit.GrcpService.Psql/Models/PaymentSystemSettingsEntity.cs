using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleTrading.Deposit.GrcpService.Psql.Models
{
    public class PaymentSystemSettingsEntity
    {
        private const char Separator = ',';
        public string PaymentSystemName { get; set; }
        public string Brand { get; set; }
        public bool IsEnable { get; set; }
        public string SupportedGeo { get; set; }
        public string RestrictedGeo { get; set; }

        public Lazy<IReadOnlyCollection<string>> SupportedGeoCountries => new Lazy<IReadOnlyCollection<string>>(()=>GetSupportedGeo());
        public Lazy<IReadOnlyCollection<string>> RestrictedGeoCountries => new Lazy<IReadOnlyCollection<string>>(() => GetRestrictedGeo());

        private IReadOnlyCollection<string> GetSupportedGeo()
        {
            if (string.IsNullOrEmpty(SupportedGeo)) return Array.Empty<string>();
            return SupportedGeo.Split(Separator).ToList();
        }

        private IReadOnlyCollection<string> GetRestrictedGeo()
        {
            if (string.IsNullOrEmpty(RestrictedGeo)) return Array.Empty<string>();
            return RestrictedGeo.Split(Separator).ToList();
        }

        public bool IsSupportCountry(string country)
        {
            return SupportedGeoCountries.Value.Count == 0 || SupportedGeoCountries.Value.Contains(country, StringComparer.OrdinalIgnoreCase);
        }

        public bool IsNotRestrictedCountry(string country)
        {
            return RestrictedGeoCountries.Value.Count == 0 || !RestrictedGeoCountries.Value.Contains(country, StringComparer.OrdinalIgnoreCase);
        }
    }
}
