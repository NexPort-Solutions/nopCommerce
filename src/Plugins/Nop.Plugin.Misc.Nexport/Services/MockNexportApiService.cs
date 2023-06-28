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
        public static List<OrganizationResponseItem> orgs = new List<OrganizationResponseItem>
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

        public NexportOrganizationResponse GetNexportGroupsForCustomer(int customerId)
        {
            var response = new NexportOrganizationResponse();
            response.OrganizationList = orgs;
                //new List<OrganizationResponseItem>();
            //for (int i = 0; i < 20; i++)
            //{
            //    var item = new OrganizationResponseItem(Guid.NewGuid(), $"organization {i}", $"org_{i}",new ApiErrorEntity());
            //    response.OrganizationList.Add(item);
            //}
            return response;
        }

        public IList<ProductModel> GetProductsForGroup(Guid groupGuid)
        {
            var list = new List<ProductModel>();
            for (int i = 0; i < 20; i++)
            {
                list.Add(new ProductModel {Name=$"product {i}"});
            }
            return list;
        }


        
    }
}
