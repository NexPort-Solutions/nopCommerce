
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Messages;
using Nop.Plugin.Misc.Nexport.Areas.Admin.Models.Redemption;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Controllers;
using static Nop.Web.Areas.Admin.Models.Customers.CustomerModel;

namespace Nop.Plugin.Misc.Nexport.Areas.Admin.Controllers;

public class RedemptionController : BaseAdminController
{
    private readonly IPermissionService _permissionService;
    private readonly NexportService _nexportService;
    private readonly ICustomerService _customerService;
    private readonly IEmailAccountService _emailAccountService;
    private readonly EmailAccountSettings _emailAccountSettings;
    private readonly IQueuedEmailService _queuedEmailService;
    private readonly IMessageTemplateService _messageTemplateService;
    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;
    private readonly IMessageTokenProvider _messageTokenProvider;

    public RedemptionController(
        IPermissionService permissionService,
        NexportService nexportService,
        ICustomerService customerService,
        IEmailAccountService emailAccountService,
        EmailAccountSettings emailAccountSettings,
        IQueuedEmailService queuedEmailService,
        IMessageTemplateService messageTemplateService,
        ILocalizationService localizationService,
        INotificationService notificationService,
        IMessageTokenProvider messageTokenProvider)
    {
        _permissionService = permissionService;
        _nexportService = nexportService;
        _customerService = customerService;
        _emailAccountService = emailAccountService;
        _emailAccountSettings = emailAccountSettings;
        _queuedEmailService = queuedEmailService;
        _messageTemplateService = messageTemplateService;
        _localizationService = localizationService;
        _notificationService = notificationService;
        _messageTokenProvider = messageTokenProvider;
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
            return AccessDeniedView();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Redeem(RedemptionCreateModel model)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
            return AccessDeniedView();
        var invoiceItem = await _nexportService.FindNexportOrderInvoiceItemById(model.OrderInvoiceItemId);
        var userMapping = await _nexportService.FindUserMappingByCustomerId(model.NopCustomerId);
        await _nexportService.RedeemNexportInvoiceItemAsync(invoiceItem, userMapping.NexportUserId);
        var template = (await _messageTemplateService.GetMessageTemplatesByNameAsync(NexportDefaults.NEXPORT_REDEMPTION_STUDENT_NOTIFICATION_MESSAGE_TEMPLATE)).First();
        if (template is null)
            throw new InvalidOperationException($"Missing message template: {NexportDefaults.NEXPORT_REDEMPTION_STUDENT_NOTIFICATION_MESSAGE_TEMPLATE}");
        //! todo await _messageTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
        await SendEmail(model.NopCustomerId, template.Subject, template.Body);
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> SearchStudents(string? term)
    {
        if (term is null)
            return Json(new());
        var students = (await _customerService.GetAllCustomersAsync())?
            .Where(customer => customer is not null)
            .Where(customer => customer.FirstName?.Contains(term) is true || customer.LastName?.Contains(term) is true)
            .Select(customerToJQueryObject);
        return Json(students);

        static JQueryObject customerToJQueryObject(Customer customer) => new($"{customer.FirstName} {customer.LastName}".Trim(), customer.Id.ToString());
    }

    public async Task SendEmail(int nopCustomerId, string subject, string body)
    {
        var customer = await _customerService.GetCustomerByIdAsync(nopCustomerId);
        var emailAccount = await _emailAccountService.GetEmailAccountByIdAsync(_emailAccountSettings.DefaultEmailAccountId);
        var customerFullName = await _customerService.GetCustomerFullNameAsync(customer);
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
        await _queuedEmailService.InsertQueuedEmailAsync(email);
        var message = await _localizationService.GetResourceAsync("Admin.Customers.Customers.SendEmail.Queued");
        _notificationService.SuccessNotification(message);
    }
}
