using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.VariantTypes;
using Nop.Core;
using Nop.Core.Domain.Localization;
using Nop.Data;
using Nop.Plugin.Misc.Nexport.DiscountPerCreditHours.Models.Plugins;
using Nop.Plugin.Misc.Nexport.DiscountPerCreditHours.Services;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Localization;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Plugin.Misc.Nexport.DiscountPerCreditHours.Factories
{
    public class NexportDiscountPerCreditHourModelFactory : INexportDiscountPerCreditHourModelFactory
    {
        private readonly IRepository<LocaleStringResource> _localeStringResourceRepository;

        public NexportDiscountPerCreditHourModelFactory(
            IRepository<LocaleStringResource> localeStringResourceRepository)
        {
            _localeStringResourceRepository = localeStringResourceRepository;
        }

        public Task<DiscountPerCreditHoursPluginResourceListModel> PrepareDiscountPerCreditHourPluginResourceListModelAsync(
            DiscountPerCreditHoursPluginResourceListSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var results = new List<LocaleStringResource>();

            foreach (var localeResource in NexportDiscountPerCreditHoursPluginService.GetLocaleResources())
            {
                var localeStringResources = _localeStringResourceRepository.Table
                    .Where(l => l.ResourceName == localeResource.Key && l.ResourceValue != localeResource.Value)
                    .ToList();

                results.AddRange(localeStringResources);
            }

            var resources = new PagedList<LocaleStringResource>(results, searchModel.Page - 1, searchModel.PageSize);

            // prepare to grid
            var model = new DiscountPerCreditHoursPluginResourceListModel().PrepareToGrid(searchModel, resources, () =>
            {
                return resources.Select(resource =>
                {
                    var localeResourceModel = resource.ToModel<LocaleResourceModel>();
                    return localeResourceModel;
                });
            });

            // the interface allows asynchronous callers, but this method is synchronous
            return Task.FromResult(model);
        }
    }
}
