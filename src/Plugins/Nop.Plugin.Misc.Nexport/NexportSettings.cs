using System;
using System.ComponentModel.DataAnnotations;
using Nop.Core.Configuration;

namespace Nop.Plugin.Misc.Nexport
{
    public class NexportSettings : ISettings
    {
        [DataType(DataType.Url)]
        public string Url { get; set; }

        public string AuthenticationToken { get; set; }

        public DateTime? UtcExpirationDate { get; set; }

        public Guid? RootOrganizationId { get; set; }

        public Guid? MerchantAccountId { get; set; }

        public bool IgnoreAcl { get; set; }

        public bool IgnoreStoreLimitations { get; set; }
    }
}
