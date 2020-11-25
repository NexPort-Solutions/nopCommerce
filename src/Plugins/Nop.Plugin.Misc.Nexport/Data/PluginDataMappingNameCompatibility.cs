using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Domain.RegistrationField;

namespace Nop.Plugin.Misc.Nexport.Data
{
    public class PluginDataMappingNameCompatibility : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new Dictionary<Type, string>
        {
            { typeof(NexportSupplementalInfoAnswerProcessingQueueItem), "NexportSupplementalInfoAnswerProcessingQueue" },
            { typeof(NexportOrderProcessingQueueItem), "NexportOrderProcessingQueue" },
            { typeof(NexportOrderInvoiceRedemptionQueueItem), "NexportOrderInvoiceRedemptionQueue" },
            { typeof(NexportRegistrationFieldSynchronizationQueueItem), "NexportRegistrationFieldSynchronizationQueue" }
        };

        public Dictionary<(Type, string), string> ColumnName => new Dictionary<(Type, string), string>();
    }
}
