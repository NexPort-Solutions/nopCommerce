using NexportApi.Model;
using Nop.Plugin.Misc.Nexport.Extensions;
using Nop.Plugin.Misc.Nexport.Models.Wholesale;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Misc.Nexport.Controllers;

public class GroupController : BasePluginController
{
    private readonly IOrganizationService _organization;

    public GroupController(IOrganizationService organization) => _organization = organization;

    [HttpGet]
    public async Task<JsonResult> GetAvailableGroupsUnderRootOrganization() => Json(await GetOrganizations());

    private async Task<IEnumerable<JQueryObject<GroupModel>>> GetOrganizations()
        => (await _organization.FindAllOrganizationsUnderRootOrganization()).Select(OrganizationToJQueryObject);

    private static JQueryObject<GroupModel> OrganizationToJQueryObject(OrganizationResponseItem organization)
        => new(organization.Name, new(organization.OrgId, organization.Name, organization.ShortName));
}
