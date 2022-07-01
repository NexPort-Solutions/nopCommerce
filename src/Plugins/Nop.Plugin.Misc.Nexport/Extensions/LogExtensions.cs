using System;
using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Logging;
using Nop.Services.Logging;

namespace Nop.Plugin.Misc.Nexport.Extensions
{
    public static class LogExtensions
    {
        public static async Task DebugAsync(this ILogger logger, string message, Exception exception = null,
            Customer customer = null)
        {
            if (exception is System.Threading.ThreadAbortException)
                return;

            await logger.InsertLogAsync(LogLevel.Debug, message, exception?.ToString(), customer);
        }
    }
}
