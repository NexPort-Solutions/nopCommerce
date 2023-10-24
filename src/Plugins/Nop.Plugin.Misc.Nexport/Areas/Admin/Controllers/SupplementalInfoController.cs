using Nop.Core;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Domain.SupplementalInfo;
using Nop.Plugin.Misc.Nexport.Extensions;
using Nop.Plugin.Misc.Nexport.Models.SupplementalInfo;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Plugin.Misc.Nexport.Services.Security;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using static Nop.Plugin.Misc.Nexport.Domain.SupplementalInfo.Question;
using static Nop.Plugin.Misc.Nexport.Domain.SupplementalInfo.Answer;
using Nop.Web.Areas.Admin.Controllers;
using IStoreService = Nop.Plugin.Misc.Nexport.Services.IStoreService;
using ICustomerService = Nop.Plugin.Misc.Nexport.Services.ICustomerService;
using static Nop.Plugin.Misc.Nexport.Defaults;

namespace Nop.Plugin.Misc.Nexport.Areas.Admin.Controllers;

[ResponseCache(Duration = ZERO_SECONDS, NoStore = true)]
internal class SupplementalInfoController : BaseAdminController
{
    private static readonly string s_this = ViewUtilities.GetControllerName<IntegrationController>();

    private readonly Settings _settings;
    private readonly Factories.IPluginModelFactory _model;
    private readonly IWorkContext _workContext;
    private readonly IStoreContext _storeContext;
    private readonly IStoreService _store;
    private readonly ICustomerService _customer;
    private readonly IPermissionService _permission;
    private readonly ILocalizationService _localization;
    private readonly INotificationService _notification;
    private readonly IPluginManager<IRegistrationFieldCustomRender> _registrationFieldCustomRenderPluginManager;
    private readonly ILogger _logger;
    private readonly ISupplementalInfoService _supplementalInfo;
    private readonly IGroupService _group;

    public SupplementalInfoController(
        Settings settings,
        Factories.IPluginModelFactory pluginModelFactory,
        IWorkContext workContext,
        IStoreContext storeContext,
        IStoreService storeService,
        ICustomerService customerService,
        IPermissionService permissionService,
        ILocalizationService localizationService,
        INotificationService notificationService,
        IPluginManager<IRegistrationFieldCustomRender> registrationFieldCustomRenderPluginManager,
        ILogger logger,
        ISupplementalInfoService supplementalInfo,
        IGroupService group)
    {
        _model = pluginModelFactory;
        _workContext = workContext;
        _storeContext = storeContext;
        _store = storeService;
        _customer = customerService;
        _settings = settings;
        _permission = permissionService;
        _localization = localizationService;
        _notification = notificationService;
        _registrationFieldCustomRenderPluginManager = registrationFieldCustomRenderPluginManager;
        _logger = logger;
        _supplementalInfo = supplementalInfo;
        _group = group;
    }

    [HttpGet]
    public async Task<IActionResult> ListSupplementalInfoQuestion()
    {
        if (!await _permission.AuthorizeAsync(PermissionProvider.ManageSupplementalInfo))
        {
            return AccessDeniedView();
        }
        return View("~/Plugins/Misc.Nexport/Views/SupplementalInfo/Question/List.cshtml", new SupplementalInfoQuestionSearchModel());
    }

    [HttpPost]
    public async Task<IActionResult> ListSupplementalInfoQuestion(SupplementalInfoQuestionSearchModel model)
    {
        if (!await _permission.AuthorizeAsync(PermissionProvider.ManageSupplementalInfo))
        {
            return await AccessDeniedDataTablesJson();
        }

        return Json(await _model.SupplementalInfoQuestionListModel(model));
    }

    [HttpGet]
    public async Task<IActionResult> AddSupplementalInfoQuestion()
    {
        if (!await _permission.AuthorizeAsync(PermissionProvider.ManageSupplementalInfo))
        {
            return AccessDeniedView();
        }

        var model = await _model.SupplementalInfoQuestionModel(new SupplementalInfoQuestionModel(), null);
        return View("~/Plugins/Misc.Nexport/Views/SupplementalInfo/Question/Add.cshtml", model);
    }

    [HttpPost]
    [ParameterBasedOnFormName("save-continue", nameof(continueEditing))]
    public async Task<IActionResult> AddSupplementalInfoQuestion(SupplementalInfoQuestionModel model, bool continueEditing)
    {
        if (!await _permission.AuthorizeAsync(PermissionProvider.ManageSupplementalInfo))
        {
            return AccessDeniedView();
        }

        if (ModelState.IsValid)
        {
            var question = model.ToEntity<Question>();
            question.UtcDateCreated = DateTime.UtcNow;
            await _supplementalInfo.InsertSupplementalInfoQuestion(question);
            _notification.SuccessNotification("A new supplemental info question has been added.");
            if (!continueEditing)
            {
                return base.RedirectToAction(nameof(ListSupplementalInfoQuestion), s_this);
            }
            return RedirectToAction(nameof(EditSupplementalInfoQuestion), s_this, new { id = question.Id });
        }
        return View("~/Plugins/Misc.Nexport/Views/SupplementalInfo/Question/Add.cshtml", model);
    }

    [HttpGet]
    public async Task<IActionResult> EditSupplementalInfoQuestion(int id)
    {
        if (!await _permission.AuthorizeAsync(PermissionProvider.ManageSupplementalInfo))
        {
            return AccessDeniedView();
        }

        var supplementalInfoQuestion = await _supplementalInfo.GetSupplementalInfoQuestionById(id);
        if (supplementalInfoQuestion is null)
        {
            return RedirectToAction("ListSupplementalInfoQuestion", s_this);
        }

        var model = await _model.SupplementalInfoQuestionModel(null, supplementalInfoQuestion);
        return View("~/Plugins/Misc.Nexport/Views/SupplementalInfo/Question/Edit.cshtml", model);
    }

    [HttpPost]
    [ParameterBasedOnFormName("save-continue", nameof(continueEditing))]
    public virtual async Task<IActionResult> EditSupplementalInfoQuestion(SupplementalInfoQuestionModel model, bool continueEditing)
    {
        if (!await _permission.AuthorizeAsync(PermissionProvider.ManageSupplementalInfo))
        {
            return AccessDeniedView();
        }
        if (await _supplementalInfo.GetSupplementalInfoQuestionById(model.Id) is not { } supplementalInfoQuestion)
        {
            return RedirectToAction(nameof(ListSupplementalInfoQuestion), s_this);
        }
        if (!ModelState.IsValid)
        {
            model = await _model.SupplementalInfoQuestionModel(model, supplementalInfoQuestion);
            return View("~/Plugins/Misc.Nexport/Views/SupplementalInfo/Question/Edit.cshtml", model);
        }
        supplementalInfoQuestion = model.ToEntity(supplementalInfoQuestion);
        await _supplementalInfo.UpdateSupplementalInfoQuestion(supplementalInfoQuestion);
        _notification.SuccessNotification("Successfully update supplemental info question");
        if (!continueEditing)
        {
            return RedirectToAction(nameof(ListSupplementalInfoQuestion), s_this);
        }
        return RedirectToAction(nameof(EditSupplementalInfoQuestion), s_this, new { id = supplementalInfoQuestion.Id });
    }

    [HttpPost]
    public virtual async Task<IActionResult> DeleteSupplementalInfoQuestion(int id)
    {
        if (!await _permission.AuthorizeAsync(PermissionProvider.ManageSupplementalInfo))
        {
            return AccessDeniedView();
        }

        var supplementalInfoQuestion = await _supplementalInfo.GetSupplementalInfoQuestionById(id);
        if (supplementalInfoQuestion is null)
        {
            return RedirectToAction(nameof(ListSupplementalInfoQuestion), s_this);
        }

        await _supplementalInfo.DeleteSupplementalInfoQuestion(supplementalInfoQuestion);
        _notification.SuccessNotification(await _localization.GetResourceAsync("Admin.Catalog.Attributes.ProductAttributes.Deleted"));
        return RedirectToAction(nameof(ListSupplementalInfoQuestion), s_this);
    }

    [HttpPost]
    public virtual async Task<IActionResult> DeleteSelectedSupplementalInfoQuestion(ICollection<int> selectedIds)
    {
        if (!await _permission.AuthorizeAsync(PermissionProvider.ManageSupplementalInfo))
        {
            return AccessDeniedView();
        }

        if (selectedIds is not null)
        {
            await _supplementalInfo.DeleteSupplementalInfoQuestions(await _supplementalInfo.GetSupplementalInfoQuestionsByIds(selectedIds.ToArray()));
        }

        return Json(new { Result = true });
    }

    [HttpPost]
    public virtual async Task<IActionResult> SupplementalInfoOptionList(SupplementalInfoOptionSearchModel model)
    {
        if (!await _permission.AuthorizeAsync(PermissionProvider.ManageSupplementalInfo))
        {
            return await AccessDeniedDataTablesJson();
        }
        if (await _supplementalInfo.GetSupplementalInfoQuestionById(model.QuestionId) is not { } question)
        {
            return BadRequest("No NexPort supplemental info question found with the specified newMapping");
        }
        return Json(await _model.SupplementalInfoOptionListModel(model, question));
    }

    [HttpGet]
    public virtual async Task<IActionResult> SupplementalInfoOptionCreatePopup(int questionId)
    {
        if (!await _permission.AuthorizeAsync(PermissionProvider.ManageSupplementalInfo))
        {
            return AccessDeniedView();
        }
        if (await _supplementalInfo.GetSupplementalInfoQuestionById(questionId) is null)
        {
            return BadRequest("No NexPort supplemental info question found with the specified newMapping");
        }
        var model = new SupplementalInfoOptionModel { OptionText = string.Empty, QuestionId = questionId };
        return View("~/Plugins/Misc.Nexport/Views/SupplementalInfo/Option/Create.cshtml", model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> SupplementalInfoOptionCreatePopup(SupplementalInfoOptionModel model)
    {
        if (!await _permission.AuthorizeAsync(PermissionProvider.ManageSupplementalInfo))
        {
            return AccessDeniedView();
        }
        if (await _supplementalInfo.GetSupplementalInfoQuestionById(model.QuestionId) is not { } question)
        {
            return BadRequest("No NexPort supplemental question found with the specified newMapping");
        }
        if (ModelState.IsValid)
        {
            var option = model.ToEntity<Option>();
            option.UtcDateCreated = DateTime.UtcNow;
            await _supplementalInfo.InsertSupplementalInfoOption(option);
            ViewBag.RefreshPage = true;
            return View("~/Plugins/Misc.Nexport/Views/SupplementalInfo/Option/Create.cshtml", model);
        }
        model.QuestionId = question.Id;
        return View("~/Plugins/Misc.Nexport/Views/SupplementalInfo/Option/Create.cshtml", model);
    }

    [HttpGet]
    public virtual async Task<IActionResult> SupplementalInfoOptionEditPopup(int optionId)
    {
        if (!await _permission.AuthorizeAsync(PermissionProvider.ManageSupplementalInfo))
        {
            return AccessDeniedView();
        }
        if (await _supplementalInfo.GetSupplementalInfoOptionById(optionId) is not { } option)
        {
            return BadRequest($"No NexPort supplemental info option found with the specified id: {optionId}");
        }
        if (await _supplementalInfo.GetSupplementalInfoQuestionById(option.QuestionId) is not { } question)
        {
            return BadRequest($"No NexPort supplemental info question found with the specified id: {option.QuestionId}");
        }
        var model = option.ToModel<SupplementalInfoOptionModel>();
        model.QuestionId = question.Id;
        return View("~/Plugins/Misc.Nexport/Views/SupplementalInfo/Option/Edit.cshtml", model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> SupplementalInfoOptionEditPopup(SupplementalInfoOptionModel model)
    {
        if (!await _permission.AuthorizeAsync(PermissionProvider.ManageSupplementalInfo))
        {
            return AccessDeniedView();
        }

        var option = await _supplementalInfo.GetSupplementalInfoOptionById(model.Id)
            ?? throw new ArgumentException("No NexPort supplemental info option found with the specified newMapping");
        var question = await _supplementalInfo.GetSupplementalInfoQuestionById(option.QuestionId)
            ?? throw new ArgumentException("No NexPort supplemental question found with the specified newMapping");
        if (!ModelState.IsValid)
        {
            var newModel = option.ToModel<SupplementalInfoOptionModel>();
            newModel.QuestionId = question.Id;
            return View("~/Plugins/Misc.Nexport/Views/SupplementalInfo/Option/Edit.cshtml", newModel);
        }
        option = model.ToEntity(option);
        await _supplementalInfo.UpdateSupplementalInfoOption(option);
        ViewBag.RefreshPage = true;
        return View("~/Plugins/Misc.Nexport/Views/SupplementalInfo/Option/Edit.cshtml", model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> DeleteSupplementalInfoOption(int id)
    {
        if (!await _permission.AuthorizeAsync(PermissionProvider.ManageSupplementalInfo))
        {
            return AccessDeniedView();
        }

        var option = await _supplementalInfo.GetSupplementalInfoOptionById(id)
            ?? throw new ArgumentException("No NexPort supplemental info option found with the specified newMapping", nameof(id));
        await _supplementalInfo.DeleteSupplementalInfoOption(option);
        return new NullJsonResult();
    }

    [Area("Admin")]
    [HttpPost]
    public async Task<IActionResult> AddSupplementalInfoOptionGroupAssociation(int optionId, Guid groupId, string groupName, string groupShortName)
    {
        if (!await _permission.AuthorizeAsync(PermissionProvider.ManageSupplementalInfo))
        {
            return AccessDeniedView();
        }
        var supplementalInfoOption = await _supplementalInfo.GetSupplementalInfoOptionById(optionId)
            ?? throw new ArgumentException("No NexPort supplemental info option found with the specified newMapping", nameof(optionId));
        var groupAssociation = new OptionGroupAssociation
        {
            GroupId = groupId,
            GroupName = groupName,
            GroupShortName = groupShortName,
            OptionId = supplementalInfoOption.Id,
            IsActive = true,
            UtcDateCreated = DateTime.UtcNow,
        };
        await _supplementalInfo.InsertSupplementalInfoOptionGroupAssociation(groupAssociation);
        return Json(new { Result = true });
    }

    [Area("Admin")]
    [HttpPost]
    public async Task<IActionResult> DeleteSupplementalInfoOptionGroupAssociation(int id)
    {
        if (!await _permission.AuthorizeAsync(PermissionProvider.ManageSupplementalInfo))
        {
            return AccessDeniedView();
        }
        if (await _supplementalInfo.GetSupplementalInfoOptionGroupAssociationById(id) is not { } groupAssociation)
        {
            return BadRequest($"No NexPort supplemental info option group association found with the specified newMapping {id}");
        }
        await _supplementalInfo.DeleteSupplementalInfoOptionGroupAssociation(groupAssociation);
        return new NullJsonResult();
    }

    [HttpPost]
    public async Task<IActionResult> ChangeSupplementalInfoOptionGroupAssociationStatus(int id)
    {
        if (!await _permission.AuthorizeAsync(PermissionProvider.ManageSupplementalInfo))
        {
            return AccessDeniedView();
        }
        if (await _supplementalInfo.GetSupplementalInfoOptionGroupAssociationById(id) is not { } groupAssociation)
        {
            return BadRequest($"No NexPort supplemental info option group association found with the specified newMapping {id}");
        }
        groupAssociation.IsActive = !groupAssociation.IsActive;
        groupAssociation.UtcDateModified = DateTime.UtcNow;
        await _supplementalInfo.UpdateSupplementalInfoOptionGroupAssociation(groupAssociation);
        return new NullJsonResult();
    }

    [Area("Admin")]
    [HttpPost]
    public async Task<IActionResult> GetSupplementalInfoOptionGroupAssociations(SupplementalInfoOptionGroupAssociationListSearchModel model)
    {
        if (!await _permission.AuthorizeAsync(PermissionProvider.ManageSupplementalInfo))
        {
            return await AccessDeniedDataTablesJson();
        }

        if (string.IsNullOrWhiteSpace(_settings.AuthenticationToken))
        {
            return await AccessDeniedDataTablesJson();
        }

        return Json(await _model.SupplementalInfoOptionGroupAssociationListModel(model));
    }

    [HttpGet]
    public async Task<IActionResult> AnswerSupplementalInfoQuestion(string returnResource)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (!await _customer.IsRegisteredAsync(customer))
        {
            return Challenge();
        }
        var store = await _storeContext.GetCurrentStoreAsync();
        var requirements = await _supplementalInfo.GetRequiredSupplementalInfos(customer.Id, store.Id);
        var model = await _model.SupplementalInfoAnswerQuestionModel(requirements.ConvertAll(info => info.QuestionId), customer, store, returnResource);
        if (model.QuestionWithoutAnswerIds.Count is 0)
        {
            return Redirect(store.Url + returnResource.TrimStart('/'));
        }
        return View("~/Plugins/Misc.Nexport/Views/SupplementalInfo/AnswerQuestions.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> SaveSupplementalInfoAnswer(SaveSupplementalInfoAnswers request)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (!await _customer.IsRegisteredAsync(customer))
        {
            return Challenge();
        }
        var store = await _storeContext.GetCurrentStoreAsync();
        foreach (var answer in request.Answers)
        {
            foreach (var optionId in answer.Options)
            {
                var newAnswer = new Answer
                {
                    CustomerId = customer.Id,
                    StoreId = store.Id,
                    QuestionId = answer.QuestionId,
                    OptionId = optionId,
                    Status = AnswerStatus.NotProcessed,
                    UtcDateCreated = DateTime.UtcNow,
                };
                await _supplementalInfo.InsertSupplementalInfoAnswer(newAnswer);
                await _supplementalInfo.InsertSupplementalInfoAnswerProcessingQueueItem(new AnswerProcessingQueueItem
                    {
                        AnswerId = newAnswer.Id,
                        UtcDateCreated = DateTime.UtcNow,
                    });
            }
            var requiredSupplementalInfos = await _supplementalInfo.GetRequiredSupplementalInfos(customer.Id, store.Id, answer.QuestionId);
            foreach (var requirement in requiredSupplementalInfos)
            {
                await _supplementalInfo.DeleteRequiredSupplementalInfo(requirement);
            }
        }
        return Json(new
            {
                Result = true
            });
    }

    [HttpPost]
    public async Task<IActionResult> GetCustomerSupplementalInfoQuestions(CustomerSupplementalInfoAnsweredQuestionListSearchModel model)
    {
        if (!await _permission.AuthorizeAsync(PermissionProvider.ManageSupplementalInfo))
        {
            return await AccessDeniedDataTablesJson();
        }

        return Json(await _model.SupplementalInfoQuestionListModel(model));
    }

    [HttpPost]
    public async Task<IActionResult> GetCustomerSupplementalInfoAnswers(SupplementalInfoAnswerListSearchModel model)
    {
        if (!await _permission.AuthorizeAsync(PermissionProvider.ManageSupplementalInfo))
        {
            return await AccessDeniedDataTablesJson();
        }

        return Json(await _model.SupplementalInfoAnswerListModel(model));
    }

    [HttpGet]
    public async Task<IActionResult> EditCustomerSupplementalInfoAnsweredQuestion(int customerId, int storeId, int questionId)
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageProducts)
            || !await _permission.AuthorizeAsync(PermissionProvider.ManageSupplementalInfo))
        {
            return AccessDeniedView();
        }
        if (await _customer.GetCustomerByIdAsync(customerId) is not { } customer)
        {
            return BadRequest($"No customer found with the specified id {customerId}");
        }
        if (await _store.GetStoreByIdAsync(storeId) is not { } store)
        {
            return BadRequest($"No store found with the specified id {storeId}");
        }
        if (await _supplementalInfo.GetSupplementalInfoQuestionById(questionId) is not { } question)
        {
            return BadRequest($"No NexPort supplemental info question found with the specified id {questionId}");
        }
        var model = await _model.CustomerSupplementalInfoAnswersEditModel(customer, store, question);
        return View("~/Plugins/Misc.Nexport/Views/SupplementalInfo/EditCustomerSupplementalInfoAnsweredQuestion.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> EditCustomerSupplementalInfoAnsweredQuestion(int? customerId, int? storeId, EditSupplementInfoAnswerRequestModel editModel)
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageProducts)
            || !await _permission.AuthorizeAsync(PermissionProvider.ManageSupplementalInfo))
        {
            return AccessDeniedView();
        }

        var customer = await GetRecord.OrThrow(_customer.GetCustomerByIdAsync, customerId);
        var store = await GetRecord.OrThrow(_store.GetStoreByIdAsync, storeId);
        var question = await GetRecord.OrThrow(_supplementalInfo.GetSupplementalInfoQuestionById, editModel.QuestionId);
        var answers = await _supplementalInfo.GetSupplementalInfoAnswers(customerId.Value, storeId.Value, editModel.QuestionId);
        if (ModelState.IsValid)
        {
            if (question.Type is QuestionType.SingleOption && answers.FirstOrDefault() is { } updatingAnswer && editModel.OptionIds is [var newOption, ..] && newOption != updatingAnswer.OptionId)
            {
                var memberships = await _supplementalInfo.GetSupplementalInfoAnswerMembershipsByAnswerId(updatingAnswer.Id);
                updatingAnswer.OptionId = newOption;
                updatingAnswer.Status = AnswerStatus.Modified;
                updatingAnswer.UtcDateModified = DateTime.UtcNow;
                await _supplementalInfo.UpdateSupplementalInfoAnswer(updatingAnswer);
                await _supplementalInfo.InsertSupplementalInfoAnswerProcessingQueueItem(new AnswerProcessingQueueItem
                    {
                        AnswerId = updatingAnswer.Id,
                        UtcDateCreated = DateTime.UtcNow,
                    });
                foreach (var membership in memberships)
                {
                    await _group.InsertGroupMembershipRemovalQueueItem(new GroupMembershipRemovalQueueItem
                        {
                            CustomerId = customerId.Value,
                            MembershipId = membership.MembershipId,
                            UtcDateCreated = DateTime.UtcNow,
                        });
                }
            }
            else if (question.Type is QuestionType.MultipleOptions)
            {
                foreach (var newOption1 in editModel.OptionIds)
                {
                    var newAnswer = answers.Find(supplementalInfoAnswer => supplementalInfoAnswer.OptionId == newOption1);
                    if (newAnswer is null)
                    {
                        newAnswer = new()
                        {
                            CustomerId = customerId.Value,
                            StoreId = storeId.Value,
                            OptionId = newOption1,
                            QuestionId = editModel.QuestionId.Value,
                            Status = AnswerStatus.NotProcessed,
                            UtcDateCreated = DateTime.UtcNow,
                        };
                        await _supplementalInfo.InsertSupplementalInfoAnswer(newAnswer);
                        await _supplementalInfo.InsertSupplementalInfoAnswerProcessingQueueItem(new AnswerProcessingQueueItem
                            {
                                AnswerId = newAnswer.Id,
                                UtcDateCreated = DateTime.UtcNow,
                            });
                    }
                }
                var removingAnswers = answers.Where(supplementalInfoAnswer => !editModel.OptionIds.Contains(supplementalInfoAnswer.OptionId));
                var removingQuestionIds = removingAnswers
                    .DistinctBy(removingAnswer => removingAnswer.QuestionId)
                    .Select(removingAnswer => removingAnswer.QuestionId);
                foreach (var removingAnswer in removingAnswers)
                {
                    await _supplementalInfo.DeleteSupplementalInfoAnswer(removingAnswer);
                    var queueItems = (await _supplementalInfo.GetSupplementalInfoAnswerMembershipsByAnswerId(removingAnswer.Id))
                        .Select(membership => new GroupMembershipRemovalQueueItem
                            {
                                CustomerId = customer.Id,
                                MembershipId = membership.MembershipId,
                                UtcDateCreated = DateTime.UtcNow,
                            });
                    foreach (var queueItem in queueItems)
                    {
                        await _group.InsertGroupMembershipRemovalQueueItem(queueItem);
                    }
                }
                var requirements = (await _supplementalInfo.GetUnansweredQuestions(customerId.Value, storeId.Value, removingQuestionIds))
                    .Select(questionId => new RequiredSupplementalInfo
                        {
                            CustomerId = customerId.Value,
                            StoreId = storeId.Value,
                            QuestionId = questionId,
                            UtcDateCreated = DateTime.UtcNow,
                        });
                foreach (var requirement in requirements)
                {
                    await _supplementalInfo.InsertRequiredSupplementalInfo(requirement);
                }
            }
        }
        ViewBag.RefreshPage = true;
        ViewBag.ClosePage = true;
        var model = await _model.CustomerSupplementalInfoAnswersEditModel(customer, store, question);
        return View("~/Plugins/Misc.Nexport/Views/SupplementalInfo/EditCustomerSupplementalInfoAnsweredQuestion.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteCustomerSupplementalInfoAnswer(int answerId)
    {
        if (!await _permission.AuthorizeAsync(StandardPermissionProvider.ManageProducts)
            || !await _permission.AuthorizeAsync(PermissionProvider.ManageSupplementalInfo))
        {
            return await AccessDeniedDataTablesJson();
        }
        if (await _supplementalInfo.GetSupplementalInfoAnswerById(answerId) is not { } answer)
        {
            return BadRequest($"No NexPort supplemental info answer found with the specified newMapping {answerId}");
        }
        var customerId = answer.CustomerId;
        var storeId = answer.StoreId;
        var removingQuestionIds = new List<int> { answer.QuestionId };
        await _supplementalInfo.DeleteSupplementalInfoAnswer(answer);
        foreach (var membership in await _supplementalInfo.GetSupplementalInfoAnswerMembershipsByAnswerId(answer.Id))
        {
            var queueItem = new GroupMembershipRemovalQueueItem
            {
                CustomerId = customerId,
                MembershipId = membership.MembershipId,
                UtcDateCreated = DateTime.UtcNow,
            };
            await _group.InsertGroupMembershipRemovalQueueItem(queueItem);
        }
        foreach (var questionId in await _supplementalInfo.GetUnansweredQuestions(customerId, storeId, removingQuestionIds))
        {
            var requirement = new RequiredSupplementalInfo
            {
                CustomerId = customerId,
                StoreId = storeId,
                QuestionId = questionId,
                UtcDateCreated = DateTime.UtcNow,
            };
            await _supplementalInfo.InsertRequiredSupplementalInfo(requirement);
        }
        return new NullJsonResult();
    }

    
    [HttpsRequirement]
    [HttpGet]
    public async Task<IActionResult> ViewSupplementalInfoAnswers()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (!await _customer.IsRegisteredAsync(customer))
        {
            return Challenge();
        }
        try
        {
            var model = await _model.CustomerSupplementalInfoAnswersModel(customer, await _storeContext.GetCurrentStoreAsync());
            return View("~/Plugins/Misc.Nexport/Views/Customer/SupplementalInfoAnswers.cshtml", model);
        }
        catch (Exception exception)
        {
            await _logger.ErrorAsync(exception.Message, exception, customer);
            _notification.ErrorNotification(exception.Message);
        }
        return new EmptyResult();
    }

    [HttpsRequirement]
    [HttpGet]
    public async Task<IActionResult> EditSupplementalInfoAnswers(int questionId)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (!await _customer.IsRegisteredAsync(customer))
        {
            return Challenge();
        }
        if (await _supplementalInfo.GetSupplementalInfoQuestionById(questionId) is not { } question)
        {
            return RedirectToRoute(nameof(ViewSupplementalInfoAnswers));
        }
        var model = await _model.CustomerSupplementalInfoAnswersEditModel(customer, await _storeContext.GetCurrentStoreAsync(), question);
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> EditSupplementalInfoAnswers(EditSupplementInfoAnswerRequestModel editModel)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (!await _customer.IsRegisteredAsync(customer))
        {
            return Challenge();
        }
        if (editModel.OptionIds is null || editModel.OptionIds.Count is 0)
        {
            return BadRequest("List of submission options cannot be null or empty.");
        }
        var store = await _storeContext.GetCurrentStoreAsync();
        if (await GetRecord.OrDefault(_supplementalInfo.GetSupplementalInfoQuestionById, editModel.QuestionId) is not { } question)
        {
            return RedirectToRoute(nameof(ViewSupplementalInfoAnswers));
        }
        if (await _supplementalInfo.GetSupplementalInfoAnswers(customer.Id, store.Id, editModel.QuestionId) is not [var first, ..] answers)
        {
            return RedirectToRoute(nameof(ViewSupplementalInfoAnswers));
        }
        if (!ModelState.IsValid)
        {
            var model = await _model.CustomerSupplementalInfoAnswersEditModel(customer, store, question);
            return View("~/Plugins/Misc.Nexport/Views/Customer/EditSupplementalInfoAnswer.cshtml", model);
        }
        if (question.Type is QuestionType.SingleOption)
        {
            if (editModel.OptionIds.Count > 1)
            {
                return RedirectToRoute(nameof(SupplementalInfoController.ViewSupplementalInfoAnswers));
            }
            var updatingAnswer = first;
            var newOption = editModel.OptionIds[0];
            if (updatingAnswer.OptionId != newOption)
            {
                var memberships = await _supplementalInfo.GetSupplementalInfoAnswerMembershipsByAnswerId(updatingAnswer.Id);
                updatingAnswer.OptionId = newOption;
                updatingAnswer.Status = AnswerStatus.Modified;
                updatingAnswer.UtcDateModified = DateTime.UtcNow;
                await _supplementalInfo.UpdateSupplementalInfoAnswer(updatingAnswer);
                await _supplementalInfo.InsertSupplementalInfoAnswerProcessingQueueItem(new AnswerProcessingQueueItem
                    {
                        AnswerId = updatingAnswer.Id,
                        UtcDateCreated = DateTime.UtcNow,
                    });
                foreach (var membership in memberships)
                {
                    await _group.InsertGroupMembershipRemovalQueueItem(new GroupMembershipRemovalQueueItem
                        {
                            CustomerId = customer.Id,
                            MembershipId = membership.MembershipId,
                            UtcDateCreated = DateTime.UtcNow,
                        });
                }
            }
        }
        else if (question.Type is QuestionType.MultipleOptions)
        {
            if (editModel.OptionIds.Count < 1)
            {
                return RedirectToRoute(nameof(SupplementalInfoController.ViewSupplementalInfoAnswers));
            }
            foreach (var newOption in editModel.OptionIds)
            {
                if (answers.Find(supplementalInfoAnswer => supplementalInfoAnswer.OptionId == newOption) is { } newAnswer)
                {
                    continue;
                }
                newAnswer = new()
                {
                    CustomerId = customer.Id,
                    StoreId = store.Id,
                    OptionId = newOption,
                    QuestionId = editModel.QuestionId.Value,
                    Status = AnswerStatus.NotProcessed,
                    UtcDateCreated = DateTime.UtcNow,
                };
                await _supplementalInfo.InsertSupplementalInfoAnswer(newAnswer);
                await _supplementalInfo.InsertSupplementalInfoAnswerProcessingQueueItem(new AnswerProcessingQueueItem
                    {
                        AnswerId = newAnswer.Id,
                        UtcDateCreated = DateTime.UtcNow,
                    });
            }
            var removingAnswers = answers.Where(supplementalFieldAnswer => !editModel.OptionIds.Contains(supplementalFieldAnswer.OptionId));
            foreach (var removingAnswer in removingAnswers)
            {
                var queueItems = (await _supplementalInfo.GetSupplementalInfoAnswerMembershipsByAnswerId(removingAnswer.Id))
                    .Select(membership => new GroupMembershipRemovalQueueItem
                        {
                            CustomerId = customer.Id,
                            MembershipId = membership.MembershipId,
                            UtcDateCreated = DateTime.UtcNow,
                        });
                await _supplementalInfo.DeleteSupplementalInfoAnswer(removingAnswer);
                foreach (var queueItem in queueItems)
                {
                    await _group.InsertGroupMembershipRemovalQueueItem(queueItem);
                }
            }
        }
        return RedirectToRoute(nameof(SupplementalInfoController.ViewSupplementalInfoAnswers));
    }
}
