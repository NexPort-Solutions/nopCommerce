using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NexportApi.Model;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.Enrollment;

public record NexportEnrollmentResponseItemModel : BaseNopModel
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public DateTime EnrollmentDate { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? ExpirationDate { get; set; }

    public bool HasExpired { get; set; }

    public bool IsExpiringSoon { get; set; }

    public DateTime? LastActivityDate { get; set; }

    public Enums.PhaseEnum Status { get; set; }

    public bool HasCertificate { get; set; }

    public string CertificateUrl { get; set; }

    public Guid? SyllabusId { get; set; }

    public bool IsMarketplacePurchase { get; set; }

    public string ProductUrl { get; set; }
}