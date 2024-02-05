using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Http.Extensions;
using Nop.Plugin.Misc.Nexport.Extensions;
using Nop.Plugin.Misc.Nexport.Factories;
using Nop.Plugin.Misc.Nexport.Models.Wholesale;
using Nop.Plugin.Misc.Nexport.Models.Wholesale.RedeemProduct;
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

            if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesale))
                return Challenge();

            var searchModel = new NexportGroupListSearchModel();
            searchModel.AdminView = false;
            ViewData["PathForPartialView"] = "~/Plugins/Misc.Nexport/Views/NexportGroups.cshtml";
            ViewData["ModelForPartialView"] = searchModel;

            return View("~/Plugins/Misc.Nexport/Views/MyNexportGroups.cshtml");

        }

        [HttpsRequirement]
        public async Task<IActionResult> CustomerNexportGroupProducts(Guid? groupId)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();

            if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesale))
                return Challenge();

            var nexportGroupProductListSearchModel = await _nexportPluginModelFactory.PrepareNexportGroupProductListSearchModelAsync(groupId);

            ViewData["PathForPartialView"] = "~/Plugins/Misc.Nexport/Views/NexportGroupProducts.cshtml";
            ViewData["ModelForPartialView"] = nexportGroupProductListSearchModel;

            return View("~/Plugins/Misc.Nexport/Views/MyNexportGroups.cshtml");
        }

        [HttpsRequirement]
        public async Task<IActionResult> CustomerNexportGroupProductRedemptions(Guid? groupId, int productId)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();

            if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesale))
                return Challenge();

            var nexportGroupProductRedemptionListSearchModel = await _nexportPluginModelFactory.PrepareNexportGroupProductRedemptionListSearchModelAsync(groupId, productId);

            ViewData["PathForPartialView"] = "~/Plugins/Misc.Nexport/Views/NexportGroupProductRedemptions.cshtml";
            ViewData["ModelForPartialView"] = nexportGroupProductRedemptionListSearchModel;

            return View("~/Plugins/Misc.Nexport/Views/MyNexportGroups.cshtml");
        }


        //[HttpPost]
        //public async Task<IActionResult> RedeemProductForCustomer(RedeemProductModel model)
        //{
        //    if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesale))
        //        return Challenge();

        //    return Redirect(await _nexportService.RedeemProductForCustomer(model));
        //}

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> GetNexportGroups(NexportGroupListSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesale))
                return await AccessDeniedDataTablesJson();

            var currentCustomer = await _workContext.GetCurrentCustomerAsync();

            var model = await _nexportPluginModelFactory.PrepareNexportGroupListModelAsync(searchModel, currentCustomer);

            return Json(model);
        }


        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> GetNexportGroupProducts(NexportGroupProductListSearchModel searchModel, Guid? groupId)
        {
            if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesale))
                return await AccessDeniedDataTablesJson();

            var currentCustomer = await _workContext.GetCurrentCustomerAsync();

            var model = await _nexportPluginModelFactory.PrepareNexportGroupProductListModelAsync(searchModel, groupId, currentCustomer);

            return Json(model);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> GetNexportGroupProductRedemptions(NexportGroupProductRedemptionListSearchModel searchModel, Guid? groupId, int productId)
        {
            if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesale))
                return await AccessDeniedDataTablesJson();

            var currentCustomer = await _workContext.GetCurrentCustomerAsync();

            var model = await _nexportPluginModelFactory.PrepareNexportGroupProductRedemptionListModelAsync(searchModel, groupId, productId, currentCustomer);

            return Json(model);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> GetAvailableNexportGroupProductRedemptionsCount(
            NexportGroupProductRedemptionListSearchModel searchModel, Guid? groupId, int productId)
        {
            if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesale))
                return Challenge();

            var count = await _nexportService.GetAvailableNexportGroupProductRedemptionsCountAsync(groupId, productId);
            return Json(
                new { result = count }
            );
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> InvoiceItemUnassign(Guid invoiceItemId)
        {
            if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesale))
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

        public virtual async Task<IActionResult> SearchNexportUsers(string term)
        {
            if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesale))
                return Challenge();

            const int searchTermMinimumLength = 3;
            if (string.IsNullOrWhiteSpace(term) || term.Length < searchTermMinimumLength)
                return Content(string.Empty);

            var nexportUsers = await _nexportService.GetNexportUsersAsync(term);

            var result = nexportUsers.Select(c => new
            {
                label = $"{c.FirstName} {c.LastName} ({c.Email})",
                nexportUserId = c.UserId
            }).ToList();

            return Json(result);
        }

        public virtual async Task<IActionResult> GetMatchingUsers(NexportUserListSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesale))
                return Challenge();

            var customers = await _nexportService.SearchCustomersAsync(searchModel.SearchEmail ?? "");
            var nexportUsers = new List<GetUserResponse>();
            foreach (var customer in customers)
            {
                var userMapping = await _nexportService.FindUserMappingByCustomerId(customer.Id);
                if (userMapping != null)
                {
                    var nexUser = await _nexportService.GetNexportUserAsync(userMapping.NexportUserId);
                    nexportUsers.Add(nexUser);
                }
            }

            var paged = await nexportUsers.SelectAwait(async x => new NexportUserModel
            {
                UserId = x.UserId,
                FirstName = x.FirstName,
                MiddleName = x.MiddleName,
                LastName = x.LastName,
                Email = x.Email,
                OwnerOrgId = x.OwnerOrgId,
                OwnerOrg = x.OwnerOrgId != null ? (await _nexportService.GetOrganizationDetailsAsync(x.OwnerOrgId.Value))?.Name : ""
            }).ToPagedListAsync(searchModel);


            var dtlist = new NexportUserListModel();

            dtlist = await dtlist.PrepareToGridAsync(searchModel, paged, () =>
            {
                return paged.SelectAwait(async x => x);
            });

            return Json(dtlist);
        }

        [HttpsRequirement]
        public virtual async Task<IActionResult> RedeemByEmail(int invoiceItemId, int productMappingId)
        {
            var model = _nexportPluginModelFactory.PrepareRedeemByEmailModel(invoiceItemId,
                productMappingId);
            return View("~/Plugins/Misc.Nexport/Views/RedeemByEmail.cshtml", model);
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
            if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesale))
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

            if (!await _permissionService.AuthorizeAsync(NexportPermissionProvider.ManageNexportWholesale))
                return Challenge();

            var model = await _nexportPluginModelFactory.PrepareRedeemProductModel(groupId, invoiceItemId, productId);
            ViewData["PathForPartialView"] = "~/Plugins/Misc.Nexport/Views/RedeemProduct.cshtml";
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
            if(firstName != null)
                await _genericAttributeService.SaveAttributeAsync<string?>(customer, "RedeemProductModel_FirstName", null);

            var lastName = await _genericAttributeService.GetAttributeAsync<string?>(customer, "RedeemProductModel_lastName");
            if(lastName != null)
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



            return View("~/Plugins/Misc.Nexport/Views/MyNexportGroups.cshtml");
        }

        [HttpPost]
        public virtual async Task<IActionResult> AsnSaveEmail(EmailStepModel model, IFormCollection form)
        {
            try
            {
                if (!ModelState.IsValid || model.Email == null)
                {
                    //model is not valid. redisplay the form with validation message
                    return Json(new
                    {
                        update_section = new UpdateSectionJsonModel
                        {
                            name = "email",
                            html = await RenderPartialViewToStringAsync("~/Plugins/Misc.Nexport/Views/_EmailStep.cshtml", model)
                        }
                    });
                }
                var customer = await _workContext.GetCurrentCustomerAsync();

                //store email address in attribute for later steps
                await _genericAttributeService.SaveAttributeAsync<string?>(customer, "RedeemProductModel_EmailAddress", model.Email);

                if (model.EmailStepSendViaEmail)
                {
                    //store sendviaemail boolean in attribute for later steps//store email in attribute for later steps
                    await _genericAttributeService.SaveAttributeAsync<bool?>(customer, "RedeemProductModel_SendViaEmail", model.EmailStepSendViaEmail);


                    // go to the email info step
                    return Json(new
                    {
                        update_section = new UpdateSectionJsonModel
                        {
                            name = "email-info",
                            html = await RenderPartialViewToStringAsync(
                                "~/Plugins/Misc.Nexport/Views/_EmailInfoStep.cshtml", new EmailInfoStepModel()
                                {

                                })
                        },
                        goto_section = "email-info"
                    });

                }
                else
                {

                    var nexportUsers = await _nexportService.GetNexportUsersAsync(model.Email);

                    var customerStepModel = new CustomerStepListSearchModel();

                    if (nexportUsers != null && nexportUsers.Any())
                    {
                        Uri.TryCreate(new Uri(_nexportSettings.Url.TrimEnd('/')), $"Account/Info.nex?user=",
                            out var userInfoUrl);
                        if (userInfoUrl != null)
                            customerStepModel.UserLinkUri = userInfoUrl.AbsoluteUri;

                        customerStepModel.SearchEmail = model.Email;
                    }

                    return Json(new
                    {
                        update_section = new UpdateSectionJsonModel
                        {
                            name = "customer",
                            html = await RenderPartialViewToStringAsync(
                                "~/Plugins/Misc.Nexport/Views/_CustomerStep.cshtml", customerStepModel)
                        },
                        goto_section = "customer"
                    });
                }
            }
            catch (Exception exc)
            {
                await _logger.WarningAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
                return Json(new { error = 1, message = exc.Message });
            }
        }

        [HttpPost]
        public virtual async Task<IActionResult> AsnSaveCustomer(CustomerStepListSearchModel model, IFormCollection form)
        {
            try
            {
                if (model.SelectedUserId == Guid.Empty)
                    throw new Exception("Invalid. selected user id cannot be empty.");

                if (model.CustomerStepSendViaEmail && model.SelectedUserId != null)
                    throw new Exception("Invalid. Send via email and selected user id cannot both be checked.");

                if (!ModelState.IsValid)
                {
                    //model is not valid. redisplay the form with validation message
                    return Json(new
                    {
                        update_section = new UpdateSectionJsonModel
                        {
                            name = "customer",
                            html = await RenderPartialViewToStringAsync(
                                "~/Plugins/Misc.Nexport/Views/_CustomerStep.cshtml", model)
                        },
                        goto_section = "customer"
                    });
                }

                var customer = await _workContext.GetCurrentCustomerAsync();


                if (model.CustomerStepSendViaEmail)
                {
                    await _genericAttributeService.SaveAttributeAsync<bool?>(customer, "RedeemProductModel_SendViaEmail", model.CustomerStepSendViaEmail);

                    // go to the email info step
                    return Json(new
                    {
                        update_section = new UpdateSectionJsonModel
                        {
                            name = "email-info",
                            html = await RenderPartialViewToStringAsync(
                                "~/Plugins/Misc.Nexport/Views/_EmailInfoStep.cshtml", new EmailInfoStepModel()
                                {

                                })
                        },
                        goto_section = "email-info"
                    });
                }

                if (model.SelectedUserId != null)
                    await _genericAttributeService.SaveAttributeAsync<Guid?>(customer, "RedeemProductModel_SelectedUserId", model.SelectedUserId.Value);

                await _genericAttributeService.SaveAttributeAsync<string?>(customer, "RedeemProductModel_FirstName", model.SelectedUserFirstName);
                await _genericAttributeService.SaveAttributeAsync<string?>(customer, "RedeemProductModel_LastName", model.SelectedUserLastName);


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

            var trainingStepModel = await _nexportPluginModelFactory.PrepareTrainingStepModel(productId, invoiceItemId);

            if (trainingStepModel.AvailableMappings.Count < 1)
                throw new Exception("Error preparing step. Could not load mappings for product");

            if (trainingStepModel.AvailableMappings.Count == 1)
            {
               return await GoToConfirmStep(trainingStepModel, customer);
            }

            return Json(new
            {
                update_section = new UpdateSectionJsonModel
                {
                    name = "training",
                    html = await RenderPartialViewToStringAsync("~/Plugins/Misc.Nexport/Views/_TrainingStep.cshtml", trainingStepModel)
                },
                goto_section = "training"
            });
        }

        public async Task<ActionResult> GoToConfirmStep(TrainingStepModel trainingStepModel, Customer customer)
        {
            if (trainingStepModel.SelectedProductMappingId == null)
                throw new Exception("There was an error retreiving the product information from the previous steps");
            //await _genericAttributeService.SaveAttributeAsync<int?>(customer, "RedeemProductModel_SelectedProductMappingId", model.SelectedProductMappingId);

            var productMapping = await _nexportService.GetProductMappingById(trainingStepModel.SelectedProductMappingId.Value);

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
                    html = await RenderPartialViewToStringAsync("~/Plugins/Misc.Nexport/Views/_ConfirmStep.cshtml", new ConfirmStepModel
                    {
                        Email = email,
                        FirstName = firstName,
                        LastName = lastName,
                        Product = product.Name,
                        SendViaEmail = (sendViaEmail!=null && sendViaEmail.Value)?"Yes":"No"
                    })
                },
                goto_section = "confirm"
            });
        }

        [HttpPost]
        public virtual async Task<IActionResult> AsnSaveEmailInfo(EmailInfoStepModel model, IFormCollection form)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    //model is not valid. redisplay the form with validation message
                    return Json(new
                    {
                        update_section = new UpdateSectionJsonModel
                        {
                            name = "email-info",
                            html = await RenderPartialViewToStringAsync("~/Plugins/Misc.Nexport/Views/_EmailInfoStep.cshtml", model)
                        }
                    });
                }

                var customer = await _workContext.GetCurrentCustomerAsync();

                await _genericAttributeService.SaveAttributeAsync<string?>(customer, "RedeemProductModel_FirstName", model.FirstName);
                await _genericAttributeService.SaveAttributeAsync<string?>(customer, "RedeemProductModel_LastName", model.LastName);

                return await GoToTrainingStep(customer);
            }
            catch (Exception exc)
            {
                await _logger.WarningAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
                return Json(new { error = 1, message = exc.Message });
            }
        }

        [HttpPost]
        public virtual async Task<IActionResult> AsnSaveTraining(TrainingStepModel model, IFormCollection form)
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

                if (sendViaEmail != null && sendViaEmail.Value)
                {
                    var email = await _genericAttributeService.GetAttributeAsync<string?>(customer,
                        "RedeemProductModel_EmailAddress");

                    if (email == null)
                        throw new Exception("There was an error retrieving the email address from the previous steps");


                    return Json(new
                    {
                        update_section = new UpdateSectionJsonModel
                        {
                            name = "confirm",
                            html = await RenderPartialViewToStringAsync("~/Plugins/Misc.Nexport/Views/_ConfirmStep.cshtml", new ConfirmStepModel
                            {
                                Email = email,
                                Product = product.Name,
                                SendViaEmail = "Yes"
                            })
                        },
                        goto_section = "confirm"
                    });
                }
                else
                {
                    return Json(new
                    {
                        update_section = new UpdateSectionJsonModel
                        {
                            name = "confirm",
                            html = await RenderPartialViewToStringAsync("~/Plugins/Misc.Nexport/Views/_ConfirmStep.cshtml", new ConfirmStepModel
                            {
                                Email = "",
                                Product = product.Name,
                                SendViaEmail = "Yes"
                            })
                        },
                        goto_section = "confirm"
                    });
                }
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
                
                
                bool success =  await _nexportService.RedeemProductForCustomer(new RedeemProductModel
                {
                    ReturnUrl = returnUrl??"",
                    SelectedProductMappingId = selectedProductMappingId,
                    AssignmentType = sendViaEmail!=null && sendViaEmail.Value?"Email":"Instant",
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