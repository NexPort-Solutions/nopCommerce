using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Microsoft.AspNetCore.Http;
using Nop.Web.Framework;
using System.Net;

namespace Nop.Plugin.Misc.Nexport.Services;

public interface IWorkflowMessagingService
{
    Task<List<int>> SendNewOrderApprovalStoreOwnerNotificationAsync(Order order, int languageId);
}

public class WorkflowMessagingService : IWorkflowMessagingService
{
    private readonly IWorkflowMessageService _workflowMessage;
    private readonly IMessageTokenProvider _messageTokenProvider;
    private readonly ILanguageService _language;
    private readonly IMessageTemplateService _messageTemplate;
    private readonly IEmailAccountService _emailAccount;
    private readonly EmailAccountSettings _emailAccountSettings;
    private readonly ILocalizationService _localization;
    private readonly IUrlHelperFactory _urlHelperFactory;
    private readonly IActionContextAccessor _actionContextAccessor;
    private readonly IStoreService _store;

    public WorkflowMessagingService(
        IWorkflowMessageService workflowMessage,
        IMessageTokenProvider messageTokenProvider,
        ILanguageService language,
        IMessageTemplateService messageTemplate,
        IEmailAccountService emailAccount,
        EmailAccountSettings emailAccountSettings,
        ILocalizationService localization,
        IActionContextAccessor actionContextAccessor,
        IUrlHelperFactory urlHelperFactory,
        IStoreService store)
    {
        _workflowMessage = workflowMessage;
        _messageTokenProvider = messageTokenProvider;
        _language = language;
        _messageTemplate = messageTemplate;
        _emailAccount = emailAccount;
        _emailAccountSettings = emailAccountSettings;
        _localization = localization;
        _actionContextAccessor = actionContextAccessor;
        _urlHelperFactory = urlHelperFactory;
        _store = store;
    }

    public async Task<List<int>> SendNewOrderApprovalStoreOwnerNotificationAsync(Order order, int languageId)
    {
        var store = await _store.GetStoreByIdAsync(order.StoreId);
        languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);
        var messageTemplates = await GetActiveMessageTemplatesAsync(
            Defaults.ORDER_MANUAL_APPROVAL_STORE_OWNER_NOTIFICATION_MESSAGE_TEMPLATE, store.Id);
        if (messageTemplates.Count is 0)
        {
            return new List<int>();
        }
        var commonTokens = new List<Token>();
        await _messageTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
        await AddOrderApprovalTokensAsync(commonTokens, order);
        return await messageTemplates.SelectAwait(async messageTemplate =>
        {
            var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);
            var tokens = new List<Token>(commonTokens);
            await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);
            var toEmail = emailAccount?.Email;
            var toName = emailAccount?.DisplayName;
            return await _workflowMessage.SendNotificationAsync(
                messageTemplate,
                emailAccount,
                languageId,
                tokens,
                toEmail,
                toName);
        })
            .ToListAsync();
    }

    protected async Task<int> EnsureLanguageIsActiveAsync(int languageId, int storeId)
    {
        // load language by specified ID
        var language = await _language.GetLanguageByIdAsync(languageId);
        if (language?.Published is not true)
        {
            // load any language from the specified store
            language = (await _language.GetAllLanguagesAsync(storeId: storeId)).FirstOrDefault();
        }
        if (language?.Published is not true)
        {
            // load any language
            language = (await _language.GetAllLanguagesAsync()).FirstOrDefault();
        }
        if (language is null)
        {
            throw new("No active language could be loaded");
        }
        return language.Id;
    }

    private async Task<List<MessageTemplate>> GetActiveMessageTemplatesAsync(string messageTemplateName, int storeId)
        => await (await _messageTemplate.GetMessageTemplatesByNameAsync(messageTemplateName, storeId))
            .Where(messageTemplate => messageTemplate.IsActive)
            .ToListAsync();

    private async Task<EmailAccount?> GetEmailAccountOfMessageTemplateAsync(MessageTemplate messageTemplate, int languageId)
    {
        var emailAccountId = await _localization.GetLocalizedAsync(messageTemplate, template => template.EmailAccountId, languageId);
        // some 0 validation (for localizable "Email account" dropdownlist which saves 0 if "Standard" value is chosen)
        if (emailAccountId is 0)
        {
            emailAccountId = messageTemplate.EmailAccountId;
        }
        return (await _emailAccount.GetEmailAccountByIdAsync(emailAccountId)
            ?? await _emailAccount.GetEmailAccountByIdAsync(_emailAccountSettings.DefaultEmailAccountId))
            ?? (await _emailAccount.GetAllEmailAccountsAsync()).FirstOrDefault();
    }

    private async Task AddOrderApprovalTokensAsync(List<Token> tokens, Order order)
    {
        tokens.Add(new Token("OrderApproval.Id", order.Id));
        var store = await _store.GetStoreByIdAsync(order.StoreId) ?? throw new("No store could be loaded");

        // ensure that the store URL is specified
        if (string.IsNullOrEmpty(store.Url))
        {
            throw new("URL cannot be null");
        }

        if (_actionContextAccessor.ActionContext is null)
        {
            throw new($"{nameof(_actionContextAccessor.ActionContext)} missing.");
        }

        // generate a URL with an absolute path
        var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
        var url = new PathString(urlHelper.Action("Edit", "Order", new { id = order.Id }));

        // remove the application path from the generated URL if exists
        var pathBase = _actionContextAccessor.ActionContext?.HttpContext?.Request?.PathBase ?? PathString.Empty;
        url.StartsWithSegments(pathBase, out url);

        // compose the result
        var orderUrl = Uri.EscapeDataString(WebUtility.UrlDecode($"{store.Url}{AreaNames.Admin}/{url}"));
        tokens.Add(new Token("OrderApproval.AdminViewOrderUrl", orderUrl, true));
    }
}
