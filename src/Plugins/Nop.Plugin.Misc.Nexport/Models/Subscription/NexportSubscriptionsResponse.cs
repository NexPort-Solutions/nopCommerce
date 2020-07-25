using System.Collections.Generic;
using NexportApi.Model;
using Nop.Plugin.Misc.Nexport.Models.Api;

namespace Nop.Plugin.Misc.Nexport.Models.Subscription
{
    public class NexportSubscriptionsResponse : NexportApiResponseBase
    {
        public List<SubscriptionResponse> Subscriptions { get; set; }
    }
}
