using System;
using System.Collections.Generic;
using System.Linq;
using NexportApi.Model;
using Nop.Core;
using Nop.Core.Data;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Plugins;
using Nop.Services.Stores;
using Nop.Services.Tasks;

namespace Nop.Plugin.Misc.Nexport.Services.Tasks
{
    public class NexportOrderProcessingTask : IScheduleTask
    {
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly IWidgetPluginManager _widgetPluginManager;
        private readonly IPluginService _pluginService;
        private readonly ISettingService _settingService;
        private readonly IRepository<NexportProductMapping> _nexportProductRepository;
        private readonly IRepository<NexportOrderProcessingQueueItem> _nexportOrderProcessingQueueRepository;
        private readonly IRepository<NexportOrderInvoiceItem> _nexportOrderInvoiceItemRepository;
        private readonly NexportService _nexportService;
        private readonly IOrderService _orderService;
        private readonly IStoreService _storeService;
        private readonly NexportSettings _nexportSettings;
        private readonly IGenericAttributeService _genericAttributeService;

        private int _batchSize = 5;

        public NexportOrderProcessingTask(
            IPluginService pluginService,
            IWidgetPluginManager widgetPluginManager,
            ILocalizationService localizationService,
            ILogger logger,
            IOrderService orderService,
            IStoreService storeService,
            IGenericAttributeService genericAttributeService,
            ISettingService settingService,
            IRepository<NexportProductMapping> nexportProduct,
            IRepository<NexportOrderProcessingQueueItem> nexportOrderProcessingQueueRepository,
            IRepository<NexportOrderInvoiceItem> nexportOrderInvoiceItemRepository,
            NexportService nexportService,
            NexportSettings nexportSettings)
        {
            _widgetPluginManager = widgetPluginManager;
            _pluginService = pluginService;
            _localizationService = localizationService;
            _logger = logger;
            _orderService = orderService;
            _storeService = storeService;
            _genericAttributeService = genericAttributeService;
            _settingService = settingService;
            _nexportProductRepository = nexportProduct;
            _nexportOrderProcessingQueueRepository = nexportOrderProcessingQueueRepository;
            _nexportOrderInvoiceItemRepository = nexportOrderInvoiceItemRepository;
            _nexportService = nexportService;
            _nexportSettings = nexportSettings;
        }

        public void Execute()
        {
            //var pluginDescriptor = _pluginService.GetPluginDescriptorBySystemName<IPlugin>("Misc.Nexport", LoadPluginsMode.All);

            if (!_widgetPluginManager.IsPluginActive("Misc.Nexport"))
                return;

            try
            {
                var orders = (from q in _nexportOrderProcessingQueueRepository.Table
                    orderby q.UtcDateCreated
                    select q.Id).Take(_batchSize).ToList();

                ProcessNexportOrders(orders);
            }
            catch (Exception ex)
            {
                _logger.Error("Cannot process Nexport redemption", ex);
            }
        }

        public void ProcessNexportOrders(IList<int> queueItemIds)
        {
            try
            {
                foreach (var orderId in queueItemIds)
                {
                    try
                    {
                        _logger.Information($"Begin processing order {orderId}");

                        var queueItem = _nexportOrderProcessingQueueRepository.GetById(orderId);
                        var order = _orderService.GetOrderById(queueItem.OrderId);
                        var userMapping = _nexportService.FindUserMappingByCustomerId(order.CustomerId);

                        var store = _storeService.GetStoreById(order.StoreId);
                        var orgId = _genericAttributeService.GetAttribute<Guid>(store, "NexportSubscriptionOrganizationId", store.Id);

                        if (orgId == Guid.Empty)
                        {
                            orgId = _nexportSettings.RootOrganizationId.Value;
                        }

                        var invoiceId = _nexportService.BeginNexportOrderInvoiceTransaction(orgId, userMapping.NexportUserId);

                        decimal invoiceTotalCost = 0;

                        foreach (var orderItem in order.OrderItems)
                        {
                            var mapping = _nexportService.GetProductMappingByNopProductId(orderItem.ProductId);
                            if (mapping != null)
                            {
                                var productCost = orderItem.Product.ProductCost;
                                var subscriptionOrgId = mapping.NexportSubscriptionOrgId ?? orgId;

                                string invoiceItemId;

                                if (mapping.Type == NexportProductTypeEnum.Catalog)
                                {
                                    invoiceItemId = _nexportService.AddItemToNexportOrderInvoice(invoiceId, mapping.NexportCatalogId,
                                        CreateInvoiceItemRequest.ProductTypeEnum.Catalog, productCost, subscriptionOrgId);
                                } else
                                {
                                    invoiceItemId =_nexportService.AddItemToNexportOrderInvoice(invoiceId, mapping.NexportCatalogSyllabusLinkId.Value,
                                        CreateInvoiceItemRequest.ProductTypeEnum.Syllabus, productCost, subscriptionOrgId);
                                }

                                _nexportService.InsertNexportOrderInvoiceItem(new NexportOrderInvoiceItem()
                                {
                                    OrderId = queueItem.OrderId,
                                    OrderItemId = orderItem.Id,
                                    InvoiceItemId = Guid.Parse(invoiceItemId),
                                    UtcDateProcessed = DateTime.UtcNow
                                });

                                invoiceTotalCost += productCost;
                            }
                        }

                        //Add Payment
                        _nexportService.AddPaymentToNexportOrderInvoice(invoiceId, invoiceTotalCost, userMapping.NexportUserId, orderId, DateTime.UtcNow);

                        // Commit
                        _nexportService.CommitNexportOrderInvoiceTransaction(invoiceId);

                        // Delete queue item
                        _nexportService.DeleteNexportOrderProcessingQueueItem(queueItem);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Cannot process the NexportRedemptionProcessingQueue item with Id {orderId}", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Cannot process the NexportRedemptionProcessingQueue item", ex);
            }
        }
    }
}
