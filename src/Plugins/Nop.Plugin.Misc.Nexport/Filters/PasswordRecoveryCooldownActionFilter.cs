using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Nop.Core.Caching;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Services.PasswordRecovery;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Web.Controllers;
using Nop.Web.Framework.UI;
using Nop.Web.Models.Customer;

namespace Nop.Plugin.Misc.Nexport.Filters;

public sealed class PasswordRecoveryCooldownActionFilter : ActionFilterAttribute
{
    private const string AjaxRequestHeader = "X-Nexport-Password-Recovery-Ajax";

    private static readonly TimeSpan SubmissionLockDuration = TimeSpan.FromSeconds(30);

    private readonly ICustomerService _customerService;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly ILocalizationService _localizationService;
    private readonly ILocker _locker;
    private readonly INopHtmlHelper _nopHtmlHelper;
    private readonly ITempDataDictionaryFactory _tempDataDictionaryFactory;
    private readonly NexportStoreSettings _storeSettings;
    private readonly IPasswordRecoveryCooldownService _cooldownService;

    public PasswordRecoveryCooldownActionFilter(
        ICustomerService customerService,
        IGenericAttributeService genericAttributeService,
        ILocalizationService localizationService,
        ILocker locker,
        INopHtmlHelper nopHtmlHelper,
        ITempDataDictionaryFactory tempDataDictionaryFactory,
        NexportStoreSettings storeSettings,
        IPasswordRecoveryCooldownService cooldownService)
    {
        _customerService = customerService;
        _genericAttributeService = genericAttributeService;
        _localizationService = localizationService;
        _locker = locker;
        _nopHtmlHelper = nopHtmlHelper;
        _tempDataDictionaryFactory = tempDataDictionaryFactory;
        _storeSettings = storeSettings;
        _cooldownService = cooldownService;
    }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var cooldownMinutes = _storeSettings.PasswordRecoveryCooldownMinutes;
        if (!IsPasswordRecoveryAction(context) || cooldownMinutes is <= 0 or > 60)
        {
            await next();
            return;
        }

        AppendPageAssets();

        var cooldownState = PasswordRecoveryCooldownState.Inactive;
        if (!HttpMethods.IsPost(context.HttpContext.Request.Method))
        {
            var getActionExecution = await next();
            await AppendResponseAsync(context, getActionExecution, cooldownState, isAjaxRequest: false,
                sendSucceeded: false, notificationMessage: string.Empty, notificationType: string.Empty);
            return;
        }

        var isAjaxRequest = IsAjaxRequest(context.HttpContext.Request);
        var recoveryModel = context.ActionArguments.Values.OfType<PasswordRecoveryModel>().FirstOrDefault();
        var customer = string.IsNullOrWhiteSpace(recoveryModel?.Email)
            ? null
            : await _customerService.GetCustomerByEmailAsync(recoveryModel.Email);

        if (customer == null || !customer.Active || customer.Deleted)
        {
            var unknownCustomerActionExecution = await next();
            var notFoundMessage = isAjaxRequest && context.ModelState.IsValid
                ? await _localizationService.GetResourceAsync("Account.PasswordRecovery.EmailNotFound")
                : string.Empty;
            await AppendResponseAsync(context, unknownCustomerActionExecution, cooldownState, isAjaxRequest,
                sendSucceeded: false, notificationMessage: notFoundMessage,
                notificationType: notFoundMessage.Length > 0 ? "error" : string.Empty);
            return;
        }

        var lockResource = $"Nexport.PasswordRecoveryCooldown.{customer.Id}";
        ActionExecutedContext actionExecution = null;
        var sendSucceeded = false;
        var notificationMessage = string.Empty;
        var notificationType = string.Empty;
        var lockAcquired = await _locker.PerformActionWithLockAsync(lockResource, SubmissionLockDuration, async () =>
        {
            var utcNow = DateTime.UtcNow;
            cooldownState = await _cooldownService.GetStateAsync(
                customer, cooldownMinutes, utcNow);

            if (cooldownState.IsActive)
            {
                await AddCooldownErrorAsync(context);
                actionExecution = await next();
                return;
            }

            var generatedDateBefore = await _genericAttributeService.GetAttributeAsync<DateTime?>(
                customer, NopCustomerDefaults.PasswordRecoveryTokenDateGeneratedAttribute);

            actionExecution = await next();

            var generatedDateAfter = await _genericAttributeService.GetAttributeAsync<DateTime?>(
                customer, NopCustomerDefaults.PasswordRecoveryTokenDateGeneratedAttribute);
            if (WasRecoveryTokenGenerated(generatedDateBefore, generatedDateAfter))
            {
                await _cooldownService.RecordSuccessfulSendAsync(customer, generatedDateAfter.Value);
                cooldownState = await _cooldownService.GetStateAsync(
                    customer, cooldownMinutes, DateTime.UtcNow);
                sendSucceeded = true;
                if (isAjaxRequest)
                {
                    notificationMessage = await _localizationService.GetResourceAsync(
                        "Account.PasswordRecovery.EmailHasBeenSent");
                    notificationType = "success";
                }
            }
        });

        if (!lockAcquired)
        {
            cooldownState = await _cooldownService.GetStateAsync(
                customer, cooldownMinutes, DateTime.UtcNow);
            if (!cooldownState.IsActive)
            {
                var retryUntilUtc = DateTime.UtcNow.Add(SubmissionLockDuration);
                cooldownState = new PasswordRecoveryCooldownState(
                    true, retryUntilUtc, (int)SubmissionLockDuration.TotalSeconds);
            }

            await AddCooldownErrorAsync(context);
            actionExecution = await next();
        }

        await AppendResponseAsync(context, actionExecution, cooldownState, isAjaxRequest, sendSucceeded,
            notificationMessage, notificationType);
    }

    private static bool IsPasswordRecoveryAction(ActionExecutingContext context)
    {
        var controllerName = context.RouteData.Values["controller"]?.ToString();
        var actionName = context.RouteData.Values["action"]?.ToString();
        if (string.Equals(controllerName, "Customer", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(actionName, nameof(CustomerController.PasswordRecovery), StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return context.HttpContext.Request.Path.Equals("/passwordrecovery", StringComparison.OrdinalIgnoreCase);
    }

    private static bool WasRecoveryTokenGenerated(DateTime? generatedDateBefore, DateTime? generatedDateAfter)
    {
        return generatedDateAfter.HasValue &&
            (!generatedDateBefore.HasValue || generatedDateAfter.Value != generatedDateBefore.Value);
    }

    private static bool IsAjaxRequest(HttpRequest request)
    {
        return request.Headers.TryGetValue(AjaxRequestHeader, out var marker) &&
            string.Equals(marker.ToString(), "true", StringComparison.OrdinalIgnoreCase);
    }

    private void AppendPageAssets()
    {
        _nopHtmlHelper.AppendCssFileParts("~/Plugins/Misc.Nexport/Content/nexport.css");
        _nopHtmlHelper.AppendScriptParts(
            ResourceLocation.Footer,
            "~/Plugins/Misc.Nexport/Scripts/password_recovery.js");
    }

    private async Task AddCooldownErrorAsync(ActionExecutingContext context)
    {
        context.ModelState.AddModelError(
            string.Empty,
            await _localizationService.GetResourceAsync("Plugins.Misc.Nexport.PasswordRecovery.Cooldown"));
    }

    private async Task AppendResponseAsync(
        ActionExecutingContext context,
        ActionExecutedContext actionExecution,
        PasswordRecoveryCooldownState cooldownState,
        bool isAjaxRequest,
        bool sendSucceeded,
        string notificationMessage,
        string notificationType)
    {
        if (isAjaxRequest && actionExecution?.Exception == null)
        {
            ClearActionNotifications(context.HttpContext);
            var jsonResult = new JsonResult(new
            {
                success = sendSucceeded,
                message = notificationMessage,
                messageType = notificationType,
                cooldown = new
                {
                    remainingSeconds = cooldownState.RemainingSeconds
                },
                submitSafetyTimeoutSeconds = (int)SubmissionLockDuration.TotalSeconds,
                errors = GetModelStateErrors(context)
            });
            context.Result = jsonResult;
            if (actionExecution != null)
                actionExecution.Result = jsonResult;

            return;
        }

        var tryAgainIn = await _localizationService.GetResourceAsync(
            "Plugins.Misc.Nexport.PasswordRecovery.TryAgainIn");
        var sending = await _localizationService.GetResourceAsync("Plugins.Misc.Nexport.PasswordRecovery.Sending");
        var requestFailed = await _localizationService.GetResourceAsync(
            "Plugins.Misc.Nexport.PasswordRecovery.RequestFailed");
        var configuration = JsonSerializer.Serialize(new
        {
            remainingSeconds = cooldownState.RemainingSeconds,
            submitSafetyTimeoutSeconds = (int)SubmissionLockDuration.TotalSeconds,
            tryAgainIn,
            sending,
            requestFailed
        });

        _nopHtmlHelper.AppendInlineScriptParts(
            ResourceLocation.Footer,
            $"<script type=\"text/javascript\">window.nexportPasswordRecoveryCooldown = {configuration};</script>");
    }

    private static Dictionary<string, string[]> GetModelStateErrors(ActionExecutingContext context)
    {
        return context.ModelState
            .Where(entry => entry.Value.Errors.Count > 0)
            .ToDictionary(
                entry => entry.Key,
                entry => entry.Value.Errors
                    .Select(error => error.ErrorMessage)
                    .Where(message => !string.IsNullOrWhiteSpace(message))
                    .ToArray());
    }

    private void ClearActionNotifications(HttpContext httpContext)
    {
        var tempData = _tempDataDictionaryFactory.GetTempData(httpContext);
        tempData.Remove(NopMessageDefaults.NotificationListKey);
    }
}
