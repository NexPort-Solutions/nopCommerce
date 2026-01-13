using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Services.Cms;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.Sale.CancelPendingOrderRequests.Services;

public class EventConsumer : IConsumer<AdminMenuCreatedEvent>
{
    #region Fields

    private readonly IWidgetPluginManager _pluginManager;

    #endregion

    #region Ctor

    public EventConsumer(IWidgetPluginManager pluginManager)
    {
        _pluginManager = pluginManager;
    }

    #endregion

    public async Task HandleEventAsync(AdminMenuCreatedEvent eventMessage)
    {
        var plugin = await _pluginManager.LoadPluginBySystemNameAsync("Sale.CancelPendingOrderRequests");

        if (plugin == null || !_pluginManager.IsPluginActive(plugin))
            return;

        var rootNode = eventMessage.RootMenuItem;
        var salesNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "Sales");
        if (salesNode == null)
            return;

        var cancelOrderRequestNode = new AdminMenuItem()
        {
            SystemName = "Cancellation requests",
            Title = "Cancellation requests",
            Url = eventMessage.GetMenuItemUrl("CancelPendingOrderRequests", "List"),
            IconClass = "far fa-dot-circle",
            Visible = true
        };
    }
}