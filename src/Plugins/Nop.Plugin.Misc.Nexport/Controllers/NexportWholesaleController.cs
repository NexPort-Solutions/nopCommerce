using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Http.Extensions;
using Nop.Plugin.Misc.Nexport.Extensions;
using Nop.Plugin.Misc.Nexport.Factories;
using Nop.Plugin.Misc.Nexport.Services;
using Nop.Plugin.Misc.Nexport.Services.Security;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Security;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Models.Checkout;
using Nop.Services.Logging;
using DocumentFormat.OpenXml.Spreadsheet;
using NexportApi.Model;
using Nop.Core.Domain.Customers;
using DocumentFormat.OpenXml.EMMA;
using Nop.Plugin.Misc.Nexport.Models.NexportWholesale;
using Nop.Plugin.Misc.Nexport.Models.NexportWholesale.WholesalePurchases;
using Nop.Services.Orders;
using Nop.Plugin.Misc.Nexport.Models.NexportWholesale.WholesalePurchases.RedeemProduct;
using Nop.Services.Localization;

namespace Nop.Plugin.Misc.Nexport.Controllers
{
    public class NexportWholesaleController : BasePluginController
    {
        #region Fields

        private readonly INexportPluginModelFactory _nexportPluginModelFactory;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ICustomerService _customerService;
        private readonly NexportService _nexportService;
        private readonly IPermissionService _permissionService;
        private readonly NexportSettings _nexportSettings;
        private readonly IProductService _productService;
        private readonly ILogger _logger;
        private readonly ILocalizationService _localizationService;

        #endregion

        #region Constructor

        public NexportWholesaleController(
            INexportPluginModelFactory nexportPluginModelFactory,
            IGenericAttributeService genericAttributeService,
            IWorkContext workContext,
            IStoreContext storeContext,
            ICustomerService customerService,
            NexportService nexportService,
            IPermissionService permissionService,
            NexportSettings nexportSettings,
            IProductService productService,
            ILocalizationService localizationService,
            ILogger logger
            )
        {
            _nexportPluginModelFactory = nexportPluginModelFactory;
            _genericAttributeService = genericAttributeService;
            _workContext = workContext;
            _storeContext = storeContext;
            _customerService = customerService;
            _nexportService = nexportService;
            _permissionService = permissionService;
            _nexportSettings = nexportSettings;
            _productService = productService;
            _localizationService = localizationService;
            _logger = logger;
        }

        #endregion

        #region Actions


        [AutoValidateAntiforgeryToken]
        [HttpPost]
        public async Task<IActionResult> SetPurchaseGroupForCustomer(string groupSelected)
        {
            if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesale))
                return Challenge();

            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();

            //save group for customer in generic attribute so it can be saved for the order later
            await _genericAttributeService.SaveAttributeAsync(customer, $"GroupForCustomer",
                groupSelected, store.Id);

            return Json(new
            {
                Result = true
            });
        }

        [HttpsRequirement]
        public async Task<IActionResult> CustomerNexportGroups(int? pageNumber)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();

            if (await _nexportService.HasGroupPermissionAsync(customer))
            {
                var searchModel = new NexportGroupListSearchModel();
                searchModel.AdminView = false;
                ViewData["PathForPartialView"] = "~/Plugins/Misc.Nexport/Views/NexportWholesale/WholesalePurchases/NexportGroups.cshtml";
                ViewData["ModelForPartialView"] = searchModel;

                return View("~/Plugins/Misc.Nexport/Views/NexportWholesale/WholesalePurchases/MyNexportGroups.cshtml");
            }
            else
            {
                var nexportGroupProductListSearchModel = await _nexportPluginModelFactory.PrepareNexportGroupProductListSearchModelAsync(null);

                // hide groups link in breadcrumbs
                nexportGroupProductListSearchModel.HasGroupPermission = false;
                ViewData["PathForPartialView"] = "~/Plugins/Misc.Nexport/Views/NexportWholesale/WholesalePurchases/NexportGroupProducts.cshtml";
                ViewData["ModelForPartialView"] = nexportGroupProductListSearchModel;

                return View("~/Plugins/Misc.Nexport/Views/NexportWholesale/WholesalePurchases/MyNexportGroups.cshtml");   
            }
        }

        [HttpsRequirement]
        public async Task<IActionResult> CustomerNexportGroupProducts(Guid? groupId)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();

            //customer must either have wholesale orders or be a purchasing agent to view this page
            if(!await _nexportService.HasWholesaleOrders(customer) && !await _nexportService.HasGroupPermissionAsync(customer))
                return Content("You are not authorized to view this page");

            var nexportGroupProductListSearchModel = await _nexportPluginModelFactory.PrepareNexportGroupProductListSearchModelAsync(groupId);

            //show groups link in breadcrumbs
            if(await _nexportService.HasGroupPermissionAsync(customer))
                nexportGroupProductListSearchModel.HasGroupPermission = true;

            ViewData["PathForPartialView"] = "~/Plugins/Misc.Nexport/Views/NexportWholesale/WholesalePurchases/NexportGroupProducts.cshtml";
            ViewData["ModelForPartialView"] = nexportGroupProductListSearchModel;

            return View("~/Plugins/Misc.Nexport/Views/NexportWholesale/WholesalePurchases/MyNexportGroups.cshtml");
        }

        [HttpsRequirement]
        public async Task<IActionResult> CustomerNexportGroupProductRedemptions(Guid? groupId, int productId, int? orderId = null)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();

            //customer must either have wholesale orders or be a purchasing agent to view this page
            if(!await _nexportService.HasWholesaleOrders(customer) && !await _nexportService.HasGroupPermissionAsync(customer))
                return Content("You are not authorized to view this page");

            //show groups link in breadcrumbs
            
            var nexportGroupProductRedemptionListSearchModel = await _nexportPluginModelFactory.PrepareNexportGroupProductRedemptionListSearchModelAsync(groupId, productId, orderId);

            if(await _nexportService.HasGroupPermissionAsync(customer))
                nexportGroupProductRedemptionListSearchModel.HasGroupPermission = true;

            ViewData["PathForPartialView"] = "~/Plugins/Misc.Nexport/Views/NexportWholesale/WholesalePurchases/NexportGroupProductRedemptions.cshtml";
            ViewData["ModelForPartialView"] = nexportGroupProductRedemptionListSearchModel;

            return View("~/Plugins/Misc.Nexport/Views/NexportWholesale/WholesalePurchases/MyNexportGroups.cshtml");
        }


        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> GetNexportGroups(NexportGroupListSearchModel searchModel)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return await AccessDeniedDataTablesJson();

            var model = await _nexportPluginModelFactory.PrepareNexportGroupListModelAsync(searchModel, customer);

            return Json(model);
        }


        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> GetNexportGroupProducts(NexportGroupProductListSearchModel searchModel, Guid? groupId)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return await AccessDeniedDataTablesJson();


            var model = await _nexportPluginModelFactory.PrepareNexportGroupProductListModelAsync(searchModel, groupId, customer);

            return Json(model);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> GetNexportGroupProductRedemptions(NexportGroupProductRedemptionListSearchModel searchModel, Guid? groupId, int productId, int? orderId)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return await AccessDeniedDataTablesJson();

            var model = await _nexportPluginModelFactory.PrepareNexportGroupProductRedemptionListModelAsync(searchModel, groupId, productId, customer, orderId);

            return Json(model);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> GetAvailableNexportGroupProductRedemptionsCount(
            NexportGroupProductRedemptionListSearchModel searchModel, Guid? groupId, int productId, int? orderId = null)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();

            int? count = null;
            if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesale))
                count = await _nexportService.GetAvailableNexportGroupProductRedemptionsCountAsync(groupId, productId, orderId, customer);
            else
                count = await _nexportService.GetAvailableNexportGroupProductRedemptionsCountAsync(groupId, productId, orderId);

            
            return Json(
                new { result = count }
            );
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> InvoiceItemUnassign(Guid invoiceItemId)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();

            var invoiceItem = await _nexportService.FindNexportOrderInvoiceItemByGuidAsync(invoiceItemId);

            if (invoiceItem != null)
                await _nexportService.UnassignInvoiceItem(invoiceItem);

            return Json(
                new
                {
                    result = invoiceItem != null ? invoiceItem.RedemptionStatus.GetDisplayName() : "",
                }
            );
        }

        public virtual async Task<IActionResult> GetMatchingUsers(CustomerStepModel searchModel)
        {
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(currentCustomer))
                return Challenge();

            var paged = new List<NexportUserModel>().ToPagedList(searchModel);

            if (searchModel.TableFirstDraw)
            {
                return Json(paged);
            }

            var customers = await _nexportService.SearchCustomersAsync(searchModel.SearchEmail ?? "");
            var nexportUsers = new List<NexportUserModel>();
            foreach (var customer in customers)
            {
                var userMapping = await _nexportService.FindUserMappingByCustomerId(customer.Id);
                if (userMapping != null)
                {
                    try
                    {
                        var nexUser = await _nexportService.GetNexportUserAsync(userMapping.NexportUserId);

                        if (nexUser != null)
                        {
                            // populate name and email from nop customer so we don't get confused if the
                            // linked nexport account has a different name and email
                            nexportUsers.Add(new NexportUserModel
                            {
                                UserId = nexUser.UserId,
                                FirstName = customer.FirstName,
                                LastName = customer.LastName,
                                Email = customer.Email,
                                OwnerOrgId = nexUser.OwnerOrgId,
                                OwnerOrg = nexUser.OwnerOrgId != null
                                    ? (await _nexportService.GetOrganizationDetailsAsync(nexUser.OwnerOrgId.Value))
                                    ?.Name
                                    : "",
                                OwnerOrgShortName = nexUser.OwnerOrgShortName
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        await _logger.ErrorAsync(ex.Message, ex);
                    }
                }
            }

            paged = await nexportUsers.SelectAwait(async x => x).ToPagedListAsync(searchModel);


            var dtlist = new NexportUserListModel();

            dtlist = await dtlist.PrepareToGridAsync(searchModel, paged, () =>
            {
                return paged.SelectAwait(async x => x);
            });

            return Json(dtlist);
        }

        [HttpsRequirement]
        public virtual async Task<IActionResult> RedeemByEmail(int invoiceItemId, string email, int productMappingId)
        {
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(currentCustomer))
                return Challenge();
            //if (currentCustomer.Email!=email)
            //    return Challenge();
            
            var model = await _nexportPluginModelFactory.PrepareRedeemByEmailModel(invoiceItemId,
                email, productMappingId);
            return View("~/Plugins/Misc.Nexport/Views/NexportWholesale/WholesalePurchases/RedeemByEmail.cshtml", model);
        }

        [HttpsRequirement]
        public virtual async Task<IActionResult> RedeemAwaitingInvoiceItem(int invoiceItemId, Guid nexportUserId, int productMappingId)
        {
            var invoiceItem = await _nexportService.FindNexportOrderInvoiceItemById(invoiceItemId);
            if (invoiceItem != null)
                await _nexportService.RedeemAwaitingInvoiceItem(invoiceItem, nexportUserId, productMappingId);

            return Json(new
            {
                Result = true
            });
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> InvoiceItemCancelAwaiting(Guid invoiceItemId)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();

            var invoiceItem = await _nexportService.FindNexportOrderInvoiceItemByGuidAsync(invoiceItemId);

            if (invoiceItem != null)
                await _nexportService.CancelAwaitingInvoiceItem(invoiceItem);

            return Json(
                new
                {
                    result = invoiceItem != null ? invoiceItem.RedemptionStatus.GetDisplayName() : "",
                }
            );
        }

        [HttpsRequirement]
        public async Task<IActionResult> RedeemProduct(Guid? groupId, Guid? invoiceItemId, int? productId)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();

            var model = await _nexportPluginModelFactory.PrepareRedeemProductModel(groupId, invoiceItemId, productId);
            ViewData["PathForPartialView"] = "~/Plugins/Misc.Nexport/Views/NexportWholesale/WholesalePurchases/RedeemProduct.cshtml";
            ViewData["ModelForPartialView"] = model;

            //reset generic attributes for the redemption form
            var emailAddressAttribute = await _genericAttributeService.GetAttributeAsync<string?>(customer, "RedeemProductModel_EmailAddress");
            if (emailAddressAttribute != null)
                await _genericAttributeService.SaveAttributeAsync<string?>(customer, "RedeemProductModel_EmailAddress", null);

            var sendViaEmailAttribute = await _genericAttributeService.GetAttributeAsync<bool?>(customer, "RedeemProductModel_SendViaEmail");
            if (sendViaEmailAttribute != null)
                await _genericAttributeService.SaveAttributeAsync<bool?>(customer, "RedeemProductModel_SendViaEmail", null);

            var selectedUserAttribute = await _genericAttributeService.GetAttributeAsync<Guid?>(customer, "RedeemProductModel_SelectedUserId");
            if (selectedUserAttribute != null)
                await _genericAttributeService.SaveAttributeAsync<Guid?>(customer, "RedeemProductModel_SelectedUserId", null);

            var firstName = await _genericAttributeService.GetAttributeAsync<string?>(customer, "RedeemProductModel_FirstName");
            if (firstName != null)
                await _genericAttributeService.SaveAttributeAsync<string?>(customer, "RedeemProductModel_FirstName", null);

            var lastName = await _genericAttributeService.GetAttributeAsync<string?>(customer, "RedeemProductModel_lastName");
            if (lastName != null)
                await _genericAttributeService.SaveAttributeAsync<string?>(customer, "RedeemProductModel_LastName", null);

            await _genericAttributeService.SaveAttributeAsync<string?>(customer, "RedeemProductModel_ReturnUrl",
                Url.RouteUrl("Plugin.Misc.Nexport.Customer.Group.Product.Redemptions") + "?groupId=" + groupId +
                "&productId=" + productId);

            await _genericAttributeService.SaveAttributeAsync<Guid?>(customer, "RedeemProductModel_InvoiceItemId",
                invoiceItemId);

            await _genericAttributeService.SaveAttributeAsync<int?>(customer, "RedeemProductModel_ProductId",
                productId);

            await _genericAttributeService.SaveAttributeAsync<int?>(customer, "RedeemProductModel_OpenEndedProductMappingId",
                model.ProductMappingIdForOpenEndedProduct);

            await _genericAttributeService.SaveAttributeAsync<int?>(customer, "RedeemProductModel_SelectedProductMappingId", model.SelectedProductMappingId);



            return View("~/Plugins/Misc.Nexport/Views/NexportWholesale/WholesalePurchases/MyNexportGroups.cshtml");
        }

        [HttpPost]
        public virtual async Task<IActionResult> AsnSaveCustomer(CustomerStepModel model, IFormCollection form)
        {
            try
            {
                var customer = await _workContext.GetCurrentCustomerAsync();

                if (model.SelectedUserId != null)
                {
                    await _genericAttributeService.SaveAttributeAsync<bool?>(customer, "RedeemProductModel_SendViaEmail", false);
                    await _genericAttributeService.SaveAttributeAsync<Guid?>(customer, "RedeemProductModel_SelectedUserId", model.SelectedUserId.Value);
                    await _genericAttributeService.SaveAttributeAsync<string?>(customer, "RedeemProductModel_EmailAddress", model.SelectedUserEmailAddress);
                    await _genericAttributeService.SaveAttributeAsync<string?>(customer, "RedeemProductModel_FirstName", model.SelectedUserFirstName);
                    await _genericAttributeService.SaveAttributeAsync<string?>(customer, "RedeemProductModel_LastName", model.SelectedUserLastName);
                }
                else
                {
                    if (!ModelState.IsValid)
                    {
                        //model is not valid. redisplay the form with validation message
                        return Json(new
                        {
                            update_section = new UpdateSectionJsonModel
                            {
                                name = "email",
                                html = await RenderPartialViewToStringAsync("~/Plugins/Misc.Nexport/Views/NexportWholesale/WholesalePurchases/RedeemProduct/_CustomerStep.cshtml", model)
                            }
                        });
                    }

                    await _genericAttributeService.SaveAttributeAsync<bool?>(customer, "RedeemProductModel_SendViaEmail", true);
                    await _genericAttributeService.SaveAttributeAsync<Guid?>(customer, "RedeemProductModel_SelectedUserId", null);
                    await _genericAttributeService.SaveAttributeAsync<string?>(customer, "RedeemProductModel_EmailAddress", model.Email);
                    await _genericAttributeService.SaveAttributeAsync<string?>(customer, "RedeemProductModel_FirstName", model.FirstName);
                    await _genericAttributeService.SaveAttributeAsync<string?>(customer, "RedeemProductModel_LastName", model.LastName);
                }

                return await GoToTrainingStep(customer);
            }
            catch (Exception exc)
            {
                await _logger.WarningAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
                return Json(new { error = 1, message = exc.Message });
            }
        }

        public async Task<IActionResult> GoToTrainingStep(Customer customer)
        {
            var productId =
                await _genericAttributeService.GetAttributeAsync<int?>(customer, "RedeemProductModel_productId");

            var invoiceItemId =
                await _genericAttributeService.GetAttributeAsync<Guid?>(customer, "RedeemProductModel_InvoiceItemId");

            var productStepModel = await _nexportPluginModelFactory.PrepareProductStepModel(productId, invoiceItemId);

            if (productStepModel.AvailableMappings.Count < 1)
                throw new Exception("Error preparing step. Could not load mappings for product");

            if (productStepModel.AvailableMappings.Count == 1)
            {
                return await GoToConfirmStep(productStepModel, customer);
            }

            return Json(new
            {
                update_section = new UpdateSectionJsonModel
                {
                    name = "training",
                    html = await RenderPartialViewToStringAsync("~/Plugins/Misc.Nexport/Views/NexportWholesale/WholesalePurchases/RedeemProduct/_ProductStep.cshtml", productStepModel)
                },
                goto_section = "training"
            });
        }

        public async Task<ActionResult> GoToConfirmStep(ProductStepModel model, Customer customer)
        {
            if (model.SelectedProductMappingId == null)
                throw new Exception("There was an error retreiving the product information from the previous steps");
            //await _genericAttributeService.SaveAttributeAsync<int?>(customer, "RedeemProductModel_SelectedProductMappingId", model.SelectedProductMappingId);

            var productMapping = await _nexportService.GetProductMappingById(model.SelectedProductMappingId.Value);

            if (productMapping == null)
                throw new Exception("There was an error retrieving the product mapping information");

            var product = await _productService.GetProductByIdAsync(productMapping.NopProductId);

            if (product == null)
                throw new Exception("There was an error retrieving the product");

            var sendViaEmail = await _genericAttributeService.GetAttributeAsync<bool?>(customer, "RedeemProductModel_SendViaEmail");
            var email = await _genericAttributeService.GetAttributeAsync<string?>(customer, "RedeemProductModel_EmailAddress");
            var firstName = await _genericAttributeService.GetAttributeAsync<string?>(customer, "RedeemProductModel_FirstName");
            var lastName = await _genericAttributeService.GetAttributeAsync<string?>(customer, "RedeemProductModel_LastName");

            return Json(new
            {
                update_section = new UpdateSectionJsonModel
                {
                    name = "confirm",
                    html = await RenderPartialViewToStringAsync("~/Plugins/Misc.Nexport/Views/NexportWholesale/WholesalePurchases/RedeemProduct/_ConfirmStep.cshtml", new ConfirmStepModel
                    {
                        Email = email,
                        FirstName = firstName,
                        LastName = lastName,
                        Product = product.Name,
                        SendViaEmail = (sendViaEmail != null && sendViaEmail.Value) ? "Email" : "Instant"
                    })
                },
                goto_section = "confirm"
            });
        }


        [HttpPost]
        public virtual async Task<IActionResult> AsnSaveProduct(ProductStepModel model, IFormCollection form)
        {
            try
            {
                var customer = await _workContext.GetCurrentCustomerAsync();

                if (model.SelectedProductMappingId == null)
                    throw new Exception("There was an error retreiving the product information from the previous steps");

                await _genericAttributeService.SaveAttributeAsync<int?>(customer, "RedeemProductModel_SelectedProductMappingId", model.SelectedProductMappingId);

                var productMapping = await _nexportService.GetProductMappingById(model.SelectedProductMappingId.Value);

                if (productMapping == null)
                    throw new Exception("There was an error retrieving the product mapping information");

                var product = await _productService.GetProductByIdAsync(productMapping.NopProductId);

                if (product == null)
                    throw new Exception("There was an error retrieving the product");

                var sendViaEmail = await _genericAttributeService.GetAttributeAsync<bool?>(customer, "RedeemProductModel_SendViaEmail");

                var email = await _genericAttributeService.GetAttributeAsync<string?>(customer, "RedeemProductModel_EmailAddress");
                var firstName = await _genericAttributeService.GetAttributeAsync<string?>(customer, "RedeemProductModel_FirstName");
                var lastName = await _genericAttributeService.GetAttributeAsync<string?>(customer, "RedeemProductModel_LastName");

                return Json(new
                {
                    update_section = new UpdateSectionJsonModel
                    {
                        name = "confirm",
                        html = await RenderPartialViewToStringAsync("~/Plugins/Misc.Nexport/Views/NexportWholesale/WholesalePurchases/RedeemProduct/_ConfirmStep.cshtml", new ConfirmStepModel
                        {
                            Email = email,
                            Product = product.Name,
                            FirstName = firstName,
                            LastName = lastName,
                            SendViaEmail = (sendViaEmail != null && sendViaEmail.Value) ? "Email" : "Instant"
                        })
                    },
                    goto_section = "confirm"
                });
            }
            catch (Exception exc)
            {
                await _logger.WarningAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
                return Json(new { error = 1, message = exc.Message });
            }
        }

        [HttpPost]
        //[AutoValidateAntiforgeryToken]
        public virtual async Task<IActionResult> AsnSaveConfirm(ConfirmStepModel model, IFormCollection form)
        {
            try
            {
                var customer = await _workContext.GetCurrentCustomerAsync();
                var returnUrl = await _genericAttributeService.GetAttributeAsync<string?>(customer, "RedeemProductModel_ReturnUrl");
                var selectedProductMappingId = await _genericAttributeService.GetAttributeAsync<int?>(customer, "RedeemProductModel_SelectedProductMappingId");
                var sendViaEmail = await _genericAttributeService.GetAttributeAsync<bool?>(customer, "RedeemProductModel_SendViaEmail");
                var firstName = await _genericAttributeService.GetAttributeAsync<string?>(customer, "RedeemProductModel_FirstName");
                var lastName = await _genericAttributeService.GetAttributeAsync<string?>(customer, "RedeemProductModel_LastName");
                var openEndedMappingId = await _genericAttributeService.GetAttributeAsync<int?>(customer, "RedeemProductModel_OpenEndedProductMappingId");
                var emailAddress = await _genericAttributeService.GetAttributeAsync<string?>(customer, "RedeemProductModel_EmailAddress");
                var invoiceItemId = await _genericAttributeService.GetAttributeAsync<Guid?>(customer, "RedeemProductModel_InvoiceItemId");
                var selectedUserId = await _genericAttributeService.GetAttributeAsync<Guid?>(customer, "RedeemProductModel_SelectedUserId");

                if (invoiceItemId == null)
                    throw new Exception("error retreiving invoice item id for transaction.");


                bool success = await _nexportService.RedeemProductForCustomer(new RedeemProductModel
                {
                    ReturnUrl = returnUrl ?? "",
                    SelectedProductMappingId = selectedProductMappingId,
                    AssignmentType = sendViaEmail != null && sendViaEmail.Value ? "Email" : "Instant",
                    Email = emailAddress,
                    FirstName = firstName,
                    LastName = lastName,
                    ProductMappingIdForOpenEndedProduct = openEndedMappingId,
                    InvoiceItemId = invoiceItemId.Value,
                    UserId = selectedUserId
                });

                return Json(new { success = 1 });
            }
            catch (Exception exc)
            {
                await _logger.WarningAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
                return Json(new { error = 1, message = exc.Message });
            }
        }

        #endregion
    }

}