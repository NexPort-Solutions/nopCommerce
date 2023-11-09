using System.Globalization;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Messages;
using Nop.Plugin.Misc.Nexport.Areas.Admin.Models.Redemption;
using Nop.Plugin.Misc.Nexport.Extensions;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Controllers;
using ICustomerService = Nop.Plugin.Misc.Nexport.Services.ICustomerService;
using static Nop.Plugin.Misc.Nexport.Defaults;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Core;
using DocumentFormat.OpenXml.EMMA;

namespace Nop.Plugin.Misc.Nexport.Areas.Admin.Controllers;

[ResponseCache(Duration = ZERO_SECONDS, NoStore = true)]
public class RedemptionController : BaseAdminController
{
    private readonly IPermissionService _permission;
    private readonly IInvoiceService _invoiceService;
    private readonly IUserMappingService _userMapping;
    private readonly ICustomerService _customer;
    private readonly IEmailAccountService _emailAccount;
    private readonly EmailAccountSettings _emailAccountSettings;
    private readonly IQueuedEmailService _queuedEmail;
    private readonly IMessageTemplateService _messageTemplate;
    private readonly ILocalizationService _localization;
    private readonly INotificationService _notification;
    private readonly IStoreContext _storeContext;

    private const string EMAIL_TEMPLATE = REDEMPTION_STUDENT_NOTIFICATION_MESSAGE_TEMPLATE;

    public RedemptionController(
        IPermissionService permissionService,
        ICustomerService customerService,
        IEmailAccountService emailAccountService,
        EmailAccountSettings emailAccountSettings,
        IQueuedEmailService queuedEmailService,
        IMessageTemplateService messageTemplateService,
        ILocalizationService localizationService,
        INotificationService notificationService,
        IInvoiceService invoiceService,
        IUserMappingService userMapping,
        IStoreContext storeContext)
    {
        _permission = permissionService;
        _customer = customerService;
        _emailAccount = emailAccountService;
        _emailAccountSettings = emailAccountSettings;
        _queuedEmail = queuedEmailService;
        _messageTemplate = messageTemplateService;
        _localization = localizationService;
        _notification = notificationService;
        _invoiceService = invoiceService;
        _userMapping = userMapping;
        _storeContext = storeContext;
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
        {
            return AccessDeniedView();
        }
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Redeem(RedemptionCreateModel model)
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
        {
            return AccessDeniedView();
        }
        if (await _invoiceService.FindOrderInvoiceItemById(model.OrderInvoiceItemId) is not { } invoiceItem)
        {
            return BadRequest("Invoid item not found.");
        }
        if (await _userMapping.FindByCustomerId(model.NopCustomerId) is not { } userMapping)
        {
            return BadRequest("User mapping not found.");
        }
        if (await _storeContext.GetCurrentStoreAsync() is not { Id: var storeId })
        {
            return BadRequest("No store found.");
        }
        return await _messageTemplate.GetMessageTemplatesByNameAsync(EMAIL_TEMPLATE, storeId) switch
        {
            [var template] => await ProcessRedemption(invoiceItem, userMapping, template),
            [] => BadRequest($"Missing message template on current store: {EMAIL_TEMPLATE}"),
            _ => BadRequest($"Duplicate message templates on current store: {EMAIL_TEMPLATE}"),
        };
    }

    [HttpGet]
    public async Task<IActionResult> SearchStudents(string? term)
    {
        if (string.IsNullOrEmpty(term))
        {
            return Json(new());
        }
        var students = (await _customer.GetAllCustomersAsync())?
            .WhereNotNull()
            .Where(customer => customer.FirstName?.Contains(term, StringComparison.OrdinalIgnoreCase) is true || customer.LastName?.Contains(term, StringComparison.OrdinalIgnoreCase) is true)
            .Select(customerToJQueryObject);
        return Json(students);

        static JQueryObject<string> customerToJQueryObject(Customer customer) => new($"{customer.FirstName} {customer.LastName}".Trim(), customer.Id.ToString(CultureInfo.InvariantCulture));
    }

    [HttpGet]
    public async Task SendEmail(int nopCustomerId, string subject, string body)
    {
        var customer = await _customer.GetCustomerByIdAsync(nopCustomerId);
        var emailAccount = await _emailAccount.GetEmailAccountByIdAsync(_emailAccountSettings.DefaultEmailAccountId);
        var customerFullName = await _customer.GetCustomerFullNameAsync(customer);
        var email = new QueuedEmail
        {
            Priority = QueuedEmailPriority.High,
            EmailAccountId = emailAccount.Id,
            FromName = emailAccount.DisplayName,
            From = emailAccount.Email,
            ToName = customerFullName,
            To = customer.Email,
            Subject = subject,
            Body = body,
            CreatedOnUtc = DateTime.UtcNow,
        };
        await _queuedEmail.InsertQueuedEmailAsync(email);
        var message = await _localization.GetResourceAsync("Admin.Customers.Customers.SendEmail.Queued");
        _notification.SuccessNotification(message);
    }

    private async Task<IActionResult> ProcessRedemption(OrderInvoiceItem invoiceItem, UserMapping userMapping, MessageTemplate template)
    {
        //! todo await _messageTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
        await _invoiceService.RedeemInvoiceItemAsync(invoiceItem, userMapping.UserId);
        await SendEmail(userMapping.Id, template.Subject, template.Body);
        return Ok();
    }
}
