using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Services;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Framework.Factories;
using Nop.Plugin.Sale.CancelPendingOrderRequests.Domains;
using Nop.Plugin.Sale.CancelPendingOrderRequests.Domains.Enums;
using Nop.Plugin.Sale.CancelPendingOrderRequests.Models;
using Nop.Plugin.Sale.CancelPendingOrderRequests.Services;

namespace Nop.Plugin.Sale.CancelPendingOrderRequests.Factories
{
    public class PendingOrderCancellationRequestModelFactory : IPendingOrderCancellationRequestModelFactory
    {
        private readonly ILocalizedModelFactory _localizedModelFactory;
        private readonly ICacheManager _cacheManager;
        private readonly ICustomerService _customerService;
        private readonly ILocalizationService _localizationService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IPendingOrderCancellationRequestService _pendingOrderCancellationRequestService;
        private readonly IWorkContext _workContext;

        public PendingOrderCancellationRequestModelFactory(
            ILocalizedModelFactory localizedModelFactory,
            ICacheManager cacheManager,
            ICustomerService customerService,
            ILocalizationService localizationService,
            IDateTimeHelper dateTimeHelper,
            IPendingOrderCancellationRequestService pendingOrderCancellationRequestService,
            IWorkContext workContext)
        {
            _localizedModelFactory = localizedModelFactory;
            _cacheManager = cacheManager;
            _customerService = customerService;
            _localizationService = localizationService;
            _dateTimeHelper = dateTimeHelper;
            _pendingOrderCancellationRequestService = pendingOrderCancellationRequestService;
            _workContext = workContext;
        }

        public PendingOrderCancellationRequestSearchModel PreparePendingOrderCancellationRequestSearchModel(
            PendingOrderCancellationRequestSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var availableStatusItems = PendingOrderCancellationRequestStatus.Accepted.ToSelectList(false);
            foreach (var statusItem in availableStatusItems)
            {
                searchModel.RequestStatusList.Add(statusItem);
            }

            searchModel.RequestStatusId = -1;
            searchModel.RequestStatusList.Insert(0, new SelectListItem
            {
                Value = "-1",
                Text = _localizationService.GetResource("Admin.CancellationRequests.SearchCancellationRequestStatus.All")
            });

            searchModel.SetGridPageSize();

            return searchModel;
        }

        public PendingOrderCancellationRequestListModel PreparePendingOrderCancellationRequestListModel(
            PendingOrderCancellationRequestSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var startDateValue = !searchModel.StartDate.HasValue ?
                null :
                (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, _dateTimeHelper.CurrentTimeZone);
            var endDateValue = !searchModel.EndDate.HasValue ?
                null :
                (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);
            var cancelRequestStatus = searchModel.RequestStatusId == -1 ?
                null :
                (PendingOrderCancellationRequestStatus?)searchModel.RequestStatusId;

            // Get cancellation requests
            var cancellationRequests =
                _pendingOrderCancellationRequestService.SearchCancellationRequests(
                    requestStatus: cancelRequestStatus,
                    createdFromUtc: startDateValue, createdToUtc: endDateValue,
                    pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            var model = new PendingOrderCancellationRequestListModel().PrepareToGrid(searchModel,
                cancellationRequests, () =>
            {
                return cancellationRequests.Select(cancellationRequest =>
                {
                    var requestModel = cancellationRequest.ToModel<PendingOrderCancellationRequestModel>();

                    requestModel.UtcCreatedDate =
                        _dateTimeHelper.ConvertToUserTime(cancellationRequest.UtcCreatedDate, DateTimeKind.Utc);

                    var customer = _customerService.GetCustomerById(cancellationRequest.CustomerId);

                    requestModel.CustomerInfo = customer != null && customer.IsRegistered()
                        ? customer.Email : _localizationService.GetResource("Admin.Customers.Guest");

                    return requestModel;
                });
            });

            return model;
        }

        public SubmitCancellationRequestModel PrepareSubmitCancellationRequestModel(SubmitCancellationRequestModel model, Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.OrderId = order.Id;

            model.AvailableCancelReasons = _cacheManager.Get(string.Format(PluginDefaults.CancellationRequestReasonsModelKey, _workContext.WorkingLanguage.Id),
                () =>
                {
                    return _pendingOrderCancellationRequestService
                        .GetAllCancellationRequestReasons()
                        .Select(reason =>
                            new PendingOrderCancellationRequestReasonModel
                            {
                                Id = reason.Id,
                                Name = _localizationService.GetLocalized(reason, x => x.Name)
                            }).ToList();
                });

            return model;
        }

        public PendingOrderCancellationRequestModel PreparePendingOrderCancellationRequestModel(
            PendingOrderCancellationRequestModel model, PendingOrderCancellationRequest cancellationRequest, bool excludeProperties = false)
        {
            if (cancellationRequest == null)
                return model;

            //fill in model values from the entity
            model = model ?? new PendingOrderCancellationRequestModel
            {
                Id = cancellationRequest.Id,
                CustomerId = cancellationRequest.CustomerId,
            };

            model.UtcCreatedDate = _dateTimeHelper.ConvertToUserTime(cancellationRequest.UtcCreatedDate, DateTimeKind.Utc);

            var customer = _customerService.GetCustomerById(cancellationRequest.CustomerId);

            model.CustomerInfo = customer.IsRegistered()
                ? customer.Email
                : _localizationService.GetResource("Admin.Customers.Guest");
            model.OrderId = cancellationRequest.OrderId;

            if (excludeProperties)
                return model;

            model.ReasonForCancellation = cancellationRequest.ReasonForCancellation;
            model.CustomerComments = cancellationRequest.CustomerComments;
            model.StaffNotes = cancellationRequest.StaffNotes;
            model.RequestStatus = cancellationRequest.RequestStatus;

            return model;
        }

        public PendingOrderCancellationRequestReasonModel PreparePendingOrderCancellationRequestReasonModel(
            PendingOrderCancellationRequestReasonModel model, PendingOrderCancellationRequestReason cancellationRequestReason, bool excludeProperties = false)
        {
            Action<PendingOrderCancellationRequestReasonLocalizedModel, int> localizedModelConfiguration = null;

            if (cancellationRequestReason != null)
            {
                model = model ?? cancellationRequestReason.ToModel<PendingOrderCancellationRequestReasonModel>();

                localizedModelConfiguration = (locale, languageId) =>
                {
                    locale.Name = _localizationService.GetLocalized(
                        cancellationRequestReason,
                        entity => entity.Name,
                        languageId, false, false);
                };
            }

            if (!excludeProperties)
                model.Locales = _localizedModelFactory.PrepareLocalizedModels(localizedModelConfiguration);

            return model;
        }

        public PendingOrderCancellationRequestReasonListModel PreparePendingOrderCancellationRequestReasonListModel(PendingOrderCancellationRequestReasonSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var reasons = _pendingOrderCancellationRequestService.GetAllCancellationRequestReasons().ToPagedList(searchModel);

            var model = new PendingOrderCancellationRequestReasonListModel().PrepareToGrid(searchModel, reasons, () =>
            {
                return reasons.Select(reason => reason.ToModel<PendingOrderCancellationRequestReasonModel>());
            });

            return model;
        }
    }
}