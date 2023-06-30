using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure;
using NexportApi.Model;
using Nop.Plugin.Misc.Nexport.Models.Organization;
using Nop.Web.Areas.Admin.Models.Catalog;

namespace Nop.Plugin.Misc.Nexport.Services
{
    public class MockNexportApiService
    {
        public List<OrganizationResponseItem> orgs = new List<OrganizationResponseItem>
        {
            new OrganizationResponseItem(Guid.NewGuid(), $"organization 1", $"org_1",new ApiErrorEntity()),
            new OrganizationResponseItem(Guid.NewGuid(), $"organization 2", $"org_2",new ApiErrorEntity()),
            new OrganizationResponseItem(Guid.NewGuid(), $"organization 3", $"org_3",new ApiErrorEntity()),
            new OrganizationResponseItem(Guid.NewGuid(), $"organization 4", $"org_4",new ApiErrorEntity()),
            new OrganizationResponseItem(Guid.NewGuid(), $"organization 5", $"org_5",new ApiErrorEntity()),
            new OrganizationResponseItem(Guid.NewGuid(), $"organization 6", $"org_6",new ApiErrorEntity()),
            new OrganizationResponseItem(Guid.NewGuid(), $"organization 7", $"org_7",new ApiErrorEntity()),
            new OrganizationResponseItem(Guid.NewGuid(), $"organization 8", $"org_8",new ApiErrorEntity()),
            new OrganizationResponseItem(Guid.NewGuid(), $"organization 9", $"org_9",new ApiErrorEntity()),
            new OrganizationResponseItem(Guid.NewGuid(), $"organization 10", $"org_10",new ApiErrorEntity()),
            new OrganizationResponseItem(Guid.NewGuid(), $"organization 11", $"org_11",new ApiErrorEntity()),
            new OrganizationResponseItem(Guid.NewGuid(), $"organization 12", $"org_12",new ApiErrorEntity()),
        };

        public List<ProductModel> products = new List<ProductModel>
        {
            new ProductModel{Name="Product 1"},
            new ProductModel{Name="Product 2"},
            new ProductModel{Name="Product 3"},
            new ProductModel{Name="Product 4"},
            new ProductModel{Name="Product 5"},
            new ProductModel{Name="Product 6"},
            new ProductModel{Name="Product 7"}
            
        };

        public static bool IsPurchasingAgent = true;

        public bool CheckPurchasingAgentPermissionForCustomer(int customerId)
        {
            return true;
        }

        public NexportOrganizationResponse GetNexportGroupsForCustomer(int customerId)
        {
            var response = new NexportOrganizationResponse();
            response.OrganizationList = orgs;
            return response;
        }

        public IList<ProductModel> GetProductsForGroup(Guid groupGuid)
        {
            var list = products;
            return list;
        }


        
    }
}
