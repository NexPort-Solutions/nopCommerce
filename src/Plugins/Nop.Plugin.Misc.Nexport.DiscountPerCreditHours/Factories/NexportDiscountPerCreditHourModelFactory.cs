using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Localization;
using Nop.Data;
using Nop.Plugin.Misc.Nexport.DiscountPerCreditHours.Models;
using Nop.Plugin.Misc.Nexport.DiscountPerCreditHours.Services;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Localization;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Plugin.Misc.Nexport.DiscountPerCreditHours.Factories;

public class NexportDiscountPerCreditHourModelFactory : INexportDiscountPerCreditHourModelFactory
{
    private readonly NexportDiscountPerCreditHoursPluginService _pluginService;
    private readonly IRepository<LocaleStringResource> _localeStringResourceRepository;

    public NexportDiscountPerCreditHourModelFactory(
        NexportDiscountPerCreditHoursPluginService pluginService,
        IRepository<LocaleStringResource> localeStringResourceRepository)
    {
        _pluginService = pluginService;
        _localeStringResourceRepository = localeStringResourceRepository;
    }

    public async Task<DiscountPerCreditHoursPluginResourceListModel> PrepareDiscountPerCreditHourPluginResourceListModelAsync(
        DiscountPerCreditHoursPluginResourceListSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        var results = await _pluginService.GetConflictedLocalizedResourcesAsync();

        var resources = new PagedList<LocaleStringResource>(results, searchModel.Page - 1, searchModel.PageSize);

        var model = await new DiscountPerCreditHoursPluginResourceListModel().PrepareToGridAsync(searchModel, resources, () =>
        {
            return resources.SelectAwait(async resource =>
            {
                var localeResourceModel = resource.ToModel<LocaleResourceModel>();
                return localeResourceModel;
            });
        });

        return model;
    }
}