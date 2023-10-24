using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.News;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Polls;
using Nop.Data;
using Nop.Plugin.Misc.Nexport.Extensions;
using Nop.Services.Common;

namespace Nop.Plugin.Misc.Nexport.Services;

public interface ICustomerService : Nop.Services.Customers.ICustomerService
{
    Task<Customer?> GetCustomer(string usernameOrEmail);
}

public class CustomerService : Nop.Services.Customers.CustomerService, ICustomerService
{
    public CustomerService(
        CustomerSettings customerSettings,
        IGenericAttributeService genericAttributeService,
        INopDataProvider dataProvider,
        IRepository<Address> customerAddressRepository,
        IRepository<BlogComment> blogCommentRepository,
        IRepository<Customer> customerRepository,
        IRepository<CustomerAddressMapping> customerAddressMappingRepository,
        IRepository<CustomerCustomerRoleMapping> customerCustomerRoleMappingRepository,
        IRepository<CustomerPassword> customerPasswordRepository,
        IRepository<CustomerRole> customerRoleRepository,
        IRepository<ForumPost> forumPostRepository,
        IRepository<ForumTopic> forumTopicRepository,
        IRepository<GenericAttribute> gaRepository,
        IRepository<NewsComment> newsCommentRepository,
        IRepository<Order> orderRepository,
        IRepository<ProductReview> productReviewRepository,
        IRepository<ProductReviewHelpfulness> productReviewHelpfulnessRepository,
        IRepository<PollVotingRecord> pollVotingRecordRepository,
        IRepository<ShoppingCartItem> shoppingCartRepository,
        IStaticCacheManager staticCacheManager,
        IStoreContext storeContext,
        ShoppingCartSettings shoppingCartSettings)
            : base(
            customerSettings,
            genericAttributeService,
            dataProvider,
            customerAddressRepository,
            blogCommentRepository,
            customerRepository,
            customerAddressMappingRepository,
            customerCustomerRoleMappingRepository,
            customerPasswordRepository,
            customerRoleRepository,
            forumPostRepository,
            forumTopicRepository,
            gaRepository,
            newsCommentRepository,
            orderRepository,
            productReviewRepository,
            productReviewHelpfulnessRepository,
            pollVotingRecordRepository,
            shoppingCartRepository,
            staticCacheManager,
            storeContext,
            shoppingCartSettings)
    { }

    public async Task<Customer?> GetCustomer(string usernameOrEmail)
        => usernameOrEmail.IsValidEmail() ? await GetCustomerByEmailAsync(usernameOrEmail) : await GetCustomerByUsernameAsync(usernameOrEmail);
}
