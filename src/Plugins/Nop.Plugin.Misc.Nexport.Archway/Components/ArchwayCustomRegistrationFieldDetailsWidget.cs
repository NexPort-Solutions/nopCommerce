using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.Nexport.Archway.Components
{
    [ViewComponent(Name = "ArchwayCustomRegistrationFieldDetailsWidget")]
    public class ArchwayCustomRegistrationFieldDetailsWidget : NopViewComponent
    {
        public ArchwayCustomRegistrationFieldDetailsWidget()
        {
        }

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            return View("~/Plugins/Misc.Nexport.Archway/Areas/Admin/Views/Customer/CustomRegistrationFieldDetails.cshtml");
        }
    }
}
