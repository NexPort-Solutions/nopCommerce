using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Misc.Nexport.Areas.Admin.Models.FundingPool;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Factories;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Plugin.Misc.Nexport.Services.Security;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Messages;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Misc.Nexport.Areas.Admin.Controllers;

public class FundingPoolController : BaseAdminController
{
    private readonly INexportPluginModelFactory _pluginModelFactory;
    private readonly IPermissionService _permission;
    private readonly IStoreService _store;
    private readonly IWholesaleService _wholesale;
    private readonly IWorkContext _workContext;
    private readonly NexportService _nexport;
    private readonly IPaymentPluginManager _paymentPluginManager;
    private readonly IProductService _product;
    private readonly ICustomerService _customer;
    private readonly IFundingPoolService _fundingPoolService;
    private readonly INotificationService _notification;
    private readonly NexportSettings _settings;
    private readonly IProductService _productService;
    private readonly IPermissionService _permissionService;

    public FundingPoolController(
        INexportPluginModelFactory nexportModelFactory,
        IPermissionService permission,
        IStoreService store,
        IWholesaleService wholesale,
        IWorkContext workContext,
        NexportService nexportService,
        IPaymentPluginManager paymentPluginManager,
        IProductService product,
        IFundingPoolService fundingPool,
        ICustomerService customer,
        INotificationService notification,
        NexportSettings settings,
        IProductService productService,
        IPermissionService permissionService)
    {
        _pluginModelFactory = nexportModelFactory;
        _permission = permission;
        _store = store;
        _wholesale = wholesale;
        _workContext = workContext;
        _nexport = nexportService;
        _paymentPluginManager = paymentPluginManager;
        _product = product;
        _fundingPoolService = fundingPool;
        _customer = customer;
        _notification = notification;
        _settings = settings;
        _productService = productService;
        _permissionService = permissionService;
    }

    public IActionResult Index()
    {
        return RedirectToAction(nameof(List));
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        if (!await _permission.AuthorizeAsync(NexportPermissionProvider.ManageNexportFundingPools))
        {
            return AccessDeniedView();
        }
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (!customer.Active || await _nexport.FindUserMappingByCustomerId(customer.Id) is null)
        {
            return AccessDeniedView();
        }
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Search(string term)
    {
        var available = await _fundingPoolService.GetAll();
        var list = available.Select(pool => new { label = pool.Name, value = pool.Id }).ToList();
        return Json(list);
    }

    [HttpPost]
    [ParameterBasedOnFormName("save-continue", "continueEditing")]
    [FormValueRequired("save", "save-continue")]
    public async Task<IActionResult> Save(FundingPoolModel model, bool continueEditing)
    {
        if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportFundingPools))
        {
            return AccessDeniedView();
        }
        try
        {
            FundingPool entity;
            if (model.Id is not null)
            {
                entity = new()
                {
                    Id = model.Id.Value,
                    Name = model.Name,
                    Code = model.Code,
                    Description = model.Description,
                };
            }
            else
            {
                entity = new()
                {
                    Name = model.Name,
                    Code = model.Code,
                    Description = model.Description,
                };
            }
            await _fundingPoolService.InsertOrUpdate(entity);
            _notification.SuccessNotification($"Funding pool {model.Name} was saved successfully.");
            if (!continueEditing)
            {
                return RedirectToAction(nameof(List));
            }
            return RedirectToAction(nameof(Edit), new { id = model.Id });
        }
        catch(Exception e)
        {
            _notification.ErrorNotification($"Error saving funding pool: {e.Message}");
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> List()
    {
        if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportFundingPools))
        {
            return AccessDeniedView();
        }
        var searchModel = new FundingPoolSearchModel();
        searchModel.SetGridPageSize();
        return View(searchModel);
    }

    [HttpPost]
    public async Task<IActionResult> List(FundingPoolSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportFundingPools))
        {
            return await AccessDeniedDataTablesJson();
        }
        var model = await _fundingPoolService.GetAll();
        var list = new List
        {
            Data = model.Select(FundingPoolModel.FromEntity).ToList(),
            RecordsFiltered = model.Count,
            RecordsTotal = model.Count,
            Draw = searchModel.Draw,
        };
        return Json(list);
    }

    [HttpGet]
    public virtual async Task<IActionResult> Edit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportFundingPools))
        {
            return AccessDeniedView();
        }
        if (await _fundingPoolService.GetById(id) is not { } fundingPool)
        {
            return RedirectToAction(nameof(List));
        }
        var model = FundingPoolModel.FromEntity(fundingPool);
        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Delete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportFundingPools))
        {
            return AccessDeniedView();
        }
        if (await _fundingPoolService.GetById(id) is not { } fundingPool)
        {
            return RedirectToAction(nameof(List));
        }
        await _fundingPoolService.Delete(fundingPool);
        return RedirectToAction(nameof(List));
    }
}
