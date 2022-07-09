using System;
using Nop.Core;

namespace Nop.Plugin.Misc.Nexport.Infrastructure.CustomExceptions
{
    public class NexportException : NopException
    {
        public NexportException(string message) : base(message)
        {
        }

        public NexportException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}