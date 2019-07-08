﻿using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Services.Configuration;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.Nexport.Components
{
    [ViewComponent(Name = "WidgetsNexportStoreDetails")]
    public class WidgetsNexportStoreDetails : NopViewComponent
    {
        private readonly IStoreContext _storeContext;
        private readonly IStaticCacheManager _cacheManager;
        private readonly ISettingService _settingService;
        private readonly IStoreModelFactory _storeModelFactory;

        public WidgetsNexportStoreDetails(
            IStoreModelFactory storeModelFactory,
            IStoreContext storeContext,
            IStaticCacheManager cacheManager,
            ISettingService settingService)
        {
            _storeModelFactory = storeModelFactory;
            _storeContext = storeContext;
            _cacheManager = cacheManager;
            _settingService = settingService;
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            if(_storeContext.CurrentStore == null)
                return Content("");

            var model = _storeModelFactory.PrepareStoreModel(null, _storeContext.CurrentStore);

            return View("~/Plugins/Misc.Nexport/Views/Widget/Store/NexportStoreDetails.cshtml", model);
        }
    }
}
