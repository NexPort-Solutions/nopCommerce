using System;
using Microsoft.AspNetCore.Hosting;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Logging;
using Nop.Core.Infrastructure;
using Nop.Data;

namespace Nop.Plugin.Misc.Nexport.Infrastructure.Logging
{
    public class DefaultLogger : Nop.Services.Logging.DefaultLogger
    {
        private readonly CommonSettings _commonSettings;
        private readonly IDbContext _dbContext;
        private readonly IRepository<Log> _logRepository;
        private readonly IWebHelper _webHelper;

        public DefaultLogger(CommonSettings commonSettings, IDbContext dbContext, IRepository<Log> logRepository, IWebHelper webHelper) : base(commonSettings, dbContext, logRepository, webHelper)
        {
            _commonSettings = commonSettings;
            _dbContext = dbContext;
            _logRepository = logRepository;
            _webHelper = webHelper;
        }

        public override Log InsertLog(LogLevel logLevel, string shortMessage, string fullMessage = "", Customer customer = null)
        {
            if (!IsEnabled(logLevel))
                return null;

            //check ignore word/phrase list?
            if (IgnoreLog(shortMessage) || IgnoreLog(fullMessage))
                return null;

            var log = new Log
            {
                LogLevel = logLevel,
                ShortMessage = shortMessage,
                FullMessage = fullMessage,
                IpAddress = _webHelper.GetCurrentIpAddress(),
                Customer = customer,
                PageUrl = _webHelper.GetThisPageUrl(true),
                ReferrerUrl = _webHelper.GetUrlReferrer(),
                CreatedOnUtc = DateTime.UtcNow
            };

            _logRepository.Insert(log);

            return log;
        }

        public override bool IsEnabled(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Debug:
                    var hostingEnvironment = EngineContext.Current.Resolve<IHostingEnvironment>();
                    return hostingEnvironment.IsDevelopment();

                case LogLevel.Information:
                    return true;

                default:
                    return base.IsEnabled(level);
            }
        }

        /// <summary>
        /// Information
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="exception">Exception</param>
        /// <param name="customer">Customer</param>
        public override void Information(string message, Exception exception = null, Customer customer = null)
        {
            //don't log thread abort exception
            if (exception is System.Threading.ThreadAbortException)
                return;

            InsertLog(LogLevel.Information, message, exception?.ToString() ?? string.Empty, customer);
        }

        /// <summary>
        /// Warning
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="exception">Exception</param>
        /// <param name="customer">Customer</param>
        public override void Warning(string message, Exception exception = null, Customer customer = null)
        {
            //don't log thread abort exception
            if (exception is System.Threading.ThreadAbortException)
                return;

            InsertLog(LogLevel.Warning, message, exception?.ToString() ?? string.Empty, customer);
        }

        /// <summary>
        /// Error
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="exception">Exception</param>
        /// <param name="customer">Customer</param>
        public override void Error(string message, Exception exception = null, Customer customer = null)
        {
            //don't log thread abort exception
            if (exception is System.Threading.ThreadAbortException)
                return;

            InsertLog(LogLevel.Error, message, exception?.ToString() ?? string.Empty, customer);
        }
    }
}
