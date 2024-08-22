using CosmosDBAzureAppService.Model;
using Microsoft.Azure.Cosmos;
using System.Threading.Tasks;
using System.Web.Http;

namespace CosmosDBAzureAppService.Controllers
{
    [Route("api/[controller]")]
    public class ConcurrencyController : ApiController
    {
        Product saddle = new Product("0120", "Worn Saddle", "accessories-used");
        PartitionKey partitionKey = new PartitionKey("accessories-used");

        private Container GetContainer()
        {
            return CosmosHelper.CreateDBAndContainer("ConcurrencyDB", "Product", "categoryId").Result;
        }

        [HttpPut]
        public async Task<IHttpActionResult> UpsertMultipleItem()
        {
            /*
             *Fetch Etag from item and pass it to next update statement.
             *Etag value changes everytime we update the item.
             */
            ItemRequestOptions options = new ItemRequestOptions();

            try
            {
                ItemResponse<Product> itemResponse = await GetContainer().ReadItemAsync<Product>(saddle.id, new PartitionKey(saddle.categoryId));

                if (!string.IsNullOrWhiteSpace(itemResponse?.ETag))
                {
                    options.IfMatchEtag = itemResponse.ETag;
                }
            }
            catch(CosmosException ex)
            {
                //if item not exists then it is throwing exception.
            }
            
            saddle = new Product("0120", "Worn Saddle 55", "accessories-used");
            Product res = await GetContainer().UpsertItemAsync<Product>(saddle, new PartitionKey(saddle.categoryId), options);
            return Ok(res);
        }
    }
}
