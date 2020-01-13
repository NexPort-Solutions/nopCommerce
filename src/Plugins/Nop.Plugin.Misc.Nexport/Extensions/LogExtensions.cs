using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Logging;
using Nop.Services.Logging;

namespace Nop.Plugin.Misc.Nexport.Extensions
{
    public static class LogExtensions
    {
        public static void Debug(this ILogger logger, string message, Exception exception = null, Customer customer = null)
        {
            if (exception is System.Threading.ThreadAbortException)
                return;

            logger.InsertLog(LogLevel.Debug, message, exception?.ToString(), customer);
        }
    }
}
