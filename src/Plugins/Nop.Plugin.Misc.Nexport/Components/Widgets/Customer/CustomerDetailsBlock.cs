using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Customers;
using Nop.Web.Framework.Components;
using Nop.Plugin.Misc.Nexport.Factories;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Plugin.Misc.Nexport.Models.Customer;
using IStoreService = Nop.Plugin.Misc.Nexport.Services.IStoreService;
using NexportApi.Model;

namespace Nop.Plugin.Misc.Nexport.Components.Widgets.Customer;

[ViewComponent]
public class CustomerDetailsBlock : NopViewComponent
{
    private readonly IUserMappingService _userMapping;
    private readonly IPluginModelFactory _model;
    private readonly INotificationService _notification;
    private readonly IStoreService _store;
    private readonly ILogger _logger;
    private readonly IUserService _user;

    public CustomerDetailsBlock(
        IPluginModelFactory pluginModelFactory,
        INotificationService notification,
        ILogger logger,
        IStoreService storeService,
        IUserService user,
        IUserMappingService userMapping)
    {
        _model = pluginModelFactory;
        _notification = notification;
        _logger = logger;
        _store = storeService;
        _user = user;
        _userMapping = userMapping;
    }

    public async Task<IViewComponentResult> InvokeAsync(string _, object additionalData)
    {
        if (additionalData is not CustomerModel customerModel || customerModel.Id is default(int))
        {
            await _logger.ErrorAsync($"{nameof(additionalData)} is not a valid {nameof(CustomerModel)}");
            return View("~/Plugins/Misc.Nexport/Views/Widget/Customer/AddCustomerAdditionalInfo.cshtml");
        }
        var model = await _model.AdditionalInfoModel(customerModel.ToEntity<Core.Domain.Customers.Customer>());
        if (await _userMapping.FindByCustomerId(customerModel.Id) is not { } mapping
            || await _user.GetUser(mapping.UserId) is not { } user)
        {
            return View("~/Plugins/Misc.Nexport/Views/Widget/Customer/CustomerDetails.cshtml", model);
        }
        InitializeModel(model, user);
        return View("~/Plugins/Misc.Nexport/Views/Widget/Customer/CustomerDetails.cshtml", model);
    }

    private static void InitializeModel(CustomerAdditionalInfoModel model, GetUserResponse user)
    {
        model.UserFullName = $"{user.FirstName} {user.LastName}";
        model.Email = user.InternalEmail;
        if (user.OwnerOrgId is not null)
        {
            model.OwnerOrgId = user.OwnerOrgId;
        }
        if (!string.IsNullOrWhiteSpace(user.OwnerOrgShortName))
        {
            model.OwnerOrgShortName = user.OwnerOrgShortName;
        }
        model.Editable = true;
    }
}
