using Nop.Plugin.Misc.Nexport.Extensions;
using Nop.Services.Payments;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Payments;

namespace Nop.Plugin.Misc.Nexport.Areas.Admin.Controllers;

public class PaymentController : BaseAdminController
{
    private readonly IPaymentPluginManager _paymentPluginManager;

    public PaymentController(IPaymentPluginManager paymentPluginManager) => _paymentPluginManager = paymentPluginManager;

    [HttpGet]
    public virtual async Task<IActionResult> SearchPurchasingAgents(string term)
    {
        var paymentMethods = (await _paymentPluginManager.LoadActivePluginsAsync())
            .Select(paymentMethod => paymentMethod.ToPluginModel<PaymentMethodModel>())
            .Select(paymentMethodToJQueryObject);
        return Json(paymentMethods);

        static JQueryObject<string> paymentMethodToJQueryObject(PaymentMethodModel paymentMethod) => new(paymentMethod.FriendlyName, paymentMethod.SystemName);
    }
}
