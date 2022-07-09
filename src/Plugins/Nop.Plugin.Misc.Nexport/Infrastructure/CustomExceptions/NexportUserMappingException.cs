using System;
using Nop.Plugin.Misc.Nexport.Domain;

namespace Nop.Plugin.Misc.Nexport.Infrastructure.CustomExceptions
{
    public class NexportUserMappingException : NexportException
    {
        public NexportUserMapping ExistingUserMapping { get; set; }

        public NexportUserMappingException(string message, NexportUserMapping existingMapping)
            : base(message)
        {
            ExistingUserMapping = existingMapping;
        }

        public NexportUserMappingException(string message, Exception innerException, NexportUserMapping existingMapping)
            : base(message, innerException)
        {
            ExistingUserMapping = existingMapping;
        }
    }
}