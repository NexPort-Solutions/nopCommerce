using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Logging;
using Nop.Core.Infrastructure;
using Nop.Data;

namespace Nop.Plugin.Misc.Nexport.Infrastructure.Logging;

public class DefaultLogger : Nop.Services.Logging.DefaultLogger
{
    private readonly IRepository<Log> _logs;
    private readonly IWebHelper _webHelper;

    public DefaultLogger(CommonSettings commonSettings, IRepository<Log> logRepository, IWebHelper webHelper)
        : base(commonSettings, logRepository, webHelper)
    {
        _logs = logRepository;
        _webHelper = webHelper;
    }

    public override async Task<Log?> InsertLogAsync(LogLevel logLevel, string shortMessage, string fullMessage = "", Customer? customer = null)
    {
        if (!IsEnabled(logLevel))
        {
            return null;
        }
        // check ignore word/phrase list?
        if (IgnoreLog(shortMessage) || IgnoreLog(fullMessage))
        {
            return null;
        }
        var log = new Log
        {
            LogLevel = logLevel,
            ShortMessage = shortMessage,
            FullMessage = fullMessage,
            IpAddress = _webHelper.GetCurrentIpAddress(),
            CustomerId = customer?.Id,
            PageUrl = _webHelper.GetThisPageUrl(true),
            ReferrerUrl = _webHelper.GetUrlReferrer(),
            CreatedOnUtc = DateTime.UtcNow,
        };
        await _logs.InsertAsync(log);
        return log;
    }

    public override bool IsEnabled(LogLevel level) => level switch
    {
        LogLevel.Debug => EngineContext.Current.Resolve<IWebHostEnvironment>().IsDevelopment(),
        LogLevel.Information => true,
        _ => base.IsEnabled(level)
    };
}
