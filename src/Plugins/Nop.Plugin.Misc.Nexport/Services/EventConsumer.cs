using System.Threading.Tasks;
using KellermanSoftware.CompareNetObjects;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Plugin.Misc.Nexport.Services.Security;
using Nop.Services.Cms;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.Misc.Nexport.Services;

public class EventConsumer : IConsumer<AdminMenuCreatedEvent>
{
    #region Fields

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILocalizationService _localizationService;
    private readonly IShortTermCacheManager _shortTermCacheManager;
    private readonly IStoreContext _storeContext;
    private readonly IWidgetPluginManager _pluginManager;
    private readonly IWorkContext _workContext;
    private readonly IWebHelper _webHelper;
    private readonly IPermissionService _permissionService;
    private readonly NexportSettings _nexportSettings;

    #endregion

    #region Ctor

    public EventConsumer(IHttpContextAccessor httpContextAccessor,
        ILocalizationService localizationService,
        IPermissionService permissionService,
        IShortTermCacheManager shortTermCacheManager,
        IStoreContext storeContext,
        IWidgetPluginManager pluginManager,
        IWebHelper webHelper,
        IWorkContext workContext,
        NexportSettings nexportSettings)
    {
        _httpContextAccessor = httpContextAccessor;
        _localizationService = localizationService;
        _permissionService = permissionService;
        _pluginManager = pluginManager;
        _shortTermCacheManager = shortTermCacheManager;
        _storeContext = storeContext;
        _webHelper = webHelper;
        _workContext = workContext;
        _nexportSettings = nexportSettings;
    }

    #endregion

    public async Task HandleEventAsync(AdminMenuCreatedEvent eventMessage)
    {
        var plugin = await _pluginManager.LoadPluginBySystemNameAsync(NexportDefaults.SystemName);

        //the LoadPluginBySystemNameAsync method returns only plugins that are already fully installed,
        //while the IConsumer<AdminMenuCreatedEvent> event can be called before the installation is complete
        if (plugin == null || !_pluginManager.IsPluginActive(plugin))
            return;

        if (string.IsNullOrWhiteSpace(_nexportSettings.AuthenticationToken))
            return;

        var rootNode = eventMessage.RootMenuItem;

        var helpNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "Help");
        if (helpNode != null)
        {
            if (!string.IsNullOrWhiteSpace(_nexportSettings.DocumentationUrl))
            {
                foreach (var helpNodeChildNode in helpNode.ChildNodes)
                {
                    helpNodeChildNode.Visible = false;
                }

                helpNode.ChildNodes.Add(new AdminMenuItem
                {
                    Visible = true,
                    Title = "Nexport Marketplace Documentation",
                    SystemName = "Nexport Marketplace Documentation",
                    Url = _nexportSettings.DocumentationUrl,
                    IconClass = "far fa-dot-circle"
                });
            }
        }

        var storeUrl = _webHelper.GetStoreLocation();

        var systemNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "System");
        systemNode?.ChildNodes.Add(new AdminMenuItem
        {
            Visible = true,
            Title = "Hangfire Dashboard",
            SystemName = "Hangfire Dashboard",
            Url = $"{storeUrl}Admin/Hangfire",
            IconClass = "far fa-dot-circle"
        });

        var node = new AdminMenuItem
        {
            SystemName = "Nexport",
            Visible = true,
            Title = "Nexport Integration",
            IconClass = "fas fa-plug",
        };

        var pluginSettingsNode = new AdminMenuItem
        {
            SystemName = "Nexport Integration - Settings",
            Visible = true,
            Title = "Settings",
            IconClass = "far fa-dot-circle",
        };

        pluginSettingsNode.ChildNodes.Add(new AdminMenuItem
        {
            Visible = await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS),
            Title = "General",
            SystemName = "Nexport Integration - Settings - General",
            Url = eventMessage.GetMenuItemUrl("NexportIntegration", "Configure"),
            IconClass = "far fa-circle"
        });

        pluginSettingsNode.ChildNodes.Add(new AdminMenuItem
        {
            Visible = await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS),
            Title = "Store settings",
            SystemName = "Nexport Integration - Settings - Store settings",
            Url = eventMessage.GetMenuItemUrl("NexportSetting", "Store"),
            IconClass = "far fa-circle"
        });

        node.ChildNodes.Add(pluginSettingsNode);

        node.ChildNodes.Add(new AdminMenuItem
        {
            Visible = await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS),
            Title = "Store configuration",
            SystemName = "Nexport Integration - Store Configuration",
            Url = eventMessage.GetMenuItemUrl("Store", "List"),
            IconClass = "far fa-dot-circle"
        });

        node.ChildNodes.Add(new AdminMenuItem
        {
            Visible = await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS),
            Title = "Supplemental Info",
            SystemName = NexportDefaults.SUPPLEMENTAL_INFO_MENU_SYSTEM_NAME,
            Url = eventMessage.GetMenuItemUrl("NexportIntegration", "ListSupplementalInfoQuestion"),
            IconClass = "far fa-dot-circle"
        });

        var wholesaleNode = new AdminMenuItem
        {
            SystemName = "Nexport",
            Visible = await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS),
            Title = "Nexport Wholesale",
            IconClass = "fas fa-shopping-basket",
        };

        var wholesaleListNode = new AdminMenuItem
        {
            Visible = await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS),
            Title = await _localizationService.GetResourceAsync("Plugins.Misc.Nexport.Admin.Navigation.Groups"),
            SystemName = "Wholesale Purchases",
            IconClass = "far fa-dot-circle"
        };

        wholesaleListNode.ChildNodes.Add(new AdminMenuItem
        {
            Visible = await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS),
            Title = "By Group",
            SystemName = "Wholesale Purchases - By Group",
            Url = eventMessage.GetMenuItemUrl("NexportWholesale", "AdminNexportGroupProducts"),
            IconClass = "far fa-circle"
        });

        wholesaleListNode.ChildNodes.Add(new AdminMenuItem
        {
            Visible = await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS),
            Title = "By Funding Pools",
            SystemName = "Wholesale Purchases - By Funding Pools",
            Url = eventMessage.GetMenuItemUrl("NexportWholesale", "AdminNexportWholesalePurchasesByFundingPoolsList"),
            IconClass = "far fa-circle"
        });

        wholesaleNode.ChildNodes.Add(wholesaleListNode);

        wholesaleNode.ChildNodes.Add(new AdminMenuItem
        {
            Visible = await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS),
            Title = "New Wholesale Order",
            SystemName = "New Wholesale Order",
            Url = eventMessage.GetMenuItemUrl("NexportWholesale", "CreateWholesaleOrder"),
            IconClass = "far fa-dot-circle"
        });

        wholesaleNode.ChildNodes.Add(new AdminMenuItem
        {
            Visible = await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS),
            Title = "Funding Pools",
            SystemName = "Nexport Funding Pools",
            Url = eventMessage.GetMenuItemUrl("NexportWholesale", "ListFundingPools"),
            IconClass = "far fa-dot-circle"
        });

        wholesaleNode.ChildNodes.Add(new AdminMenuItem
        {
            Visible = await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS),
            Title = "Assignment Approval Requests",
            SystemName = "Assignment Approval Requests",
            Url = eventMessage.GetMenuItemUrl("NexportWholesale", "AssignmentApprovalRequestsList"),
            IconClass = "far fa-dot-circle"
        });

        var unassigmentNode = new AdminMenuItem
        {
            Visible = await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS),
            Title = "Unassignments",
            SystemName = "Unassignment",
            IconClass = "far fa-dot-circle"
        };

        unassigmentNode.ChildNodes.Add(new AdminMenuItem
        {
            Visible = await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS),
            Title = "Requests",
            SystemName = "Unassignment - Requests",
            Url = eventMessage.GetMenuItemUrl("NexportWholesale", "UnassignmentRequestsList"),
            IconClass = "far fa-circle"
        });

        unassigmentNode.ChildNodes.Add(new AdminMenuItem
        {
            Visible = await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS),
            Title = "Request Reasons",
            SystemName = "Unassignment - Request Reasons",
            Url = eventMessage.GetMenuItemUrl("NexportWholesale", "UnassignmentRequestReasonsList"),
            IconClass = "far fa-circle"
        });

        wholesaleNode.ChildNodes.Add(unassigmentNode);

        rootNode.ChildNodes.Add(node);
        rootNode.ChildNodes.Add(wholesaleNode);
    }
}