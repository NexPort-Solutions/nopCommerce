using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using Nop.Plugin.Sale.CancelPendingOrderRequests.Domains;

namespace Nop.Plugin.Sale.CancelPendingOrderRequests.Data
{
    public class PluginDataMappingNameCompatibility : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new Dictionary<Type, string>
        {
            { typeof(PendingOrderCancellationRequestReason), "PendingOrderCancellationRequestReasons" },
            { typeof(PendingOrderCancellationRequest), "PendingOrderCancellationRequests" }
        };

        public Dictionary<(Type, string), string> ColumnName => new Dictionary<(Type, string), string>();
    }
}
