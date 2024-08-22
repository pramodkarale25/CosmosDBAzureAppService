using CosmosDBAzureAppService.Model;
using Microsoft.Azure.Cosmos;
using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace CosmosDBAzureAppService.Controllers
{
    [Route("api/[controller]/[action]")]
    public class IndexMetricsController : ApiController
    {
        //static string id = "027D0B9A-F9D9-4C96-8213-C8546C4AAE71";
        //static string partitionKey = "26C74104-40BC-4541-8EF5-9892F7F03D72";

        private Container GetContainer()
        {
            return CosmosHelper.CreateDBAndContainer("IndexMetricsDB", "Product", "categoryId").Result;
        }

        [HttpPost]
        public async Task<IHttpActionResult> CreateItem()
        {
            Product product = new Product()
            {
                id = Guid.NewGuid().ToString(),
                categoryId = Guid.NewGuid().ToString(),
                name = "LL Road Seat/Saddle",
                price = 27.12d,
                tags = new string[] { "brown", "weathered" }
            };
            ItemResponse<Product> pr;

            try
            {
                pr = await GetContainer().CreateItemAsync(product, new PartitionKey(product.categoryId));
                //pr.RequestCharge; Request charge for point operation.
                return Ok("Request charge for point operation(Fetched from RequestCharge property of ItemResponse class) - " + pr.RequestCharge);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                //Add logic to handle conflicting ids
                //400 Bad Request           - Something was wrong with the item in the body of the request
                //403 Forbidden             - Container was likely full
                //409 Conflict              - Item in container likely already had a matching id
                //413 RequestEntityTooLarge - Item exceeds max entity size
                //429 TooManyRequests       - Current request exceeds the maximum RU / s provisioned for the container
            }
            catch (CosmosException ex)
            {
                // Add general exception handling logic
            }

            return BadRequest();
        }

        [HttpGet]
        public async Task<IHttpActionResult> GetIndexMetrics()
        {
            try
            {
                QueryRequestOptions options = new QueryRequestOptions()
                {
                    PopulateIndexMetrics = true,
                    //PartitionKey = new PartitionKey(partitionKey)
                };

                string query = "select * from c where c.price>=32.4";
                QueryDefinition def = new QueryDefinition(query);

                FeedIterator<Product> iterator = GetContainer().GetItemQueryIterator<Product>(def, requestOptions: options);
                StringBuilder sb= new StringBuilder();

                while(iterator.HasMoreResults)
                {
                    FeedResponse<Product> res = await iterator.ReadNextAsync();

                    sb.Append(res.IndexMetrics.ToString()); // This will give the index recommendation if any.
                    sb.Append("Request charge for point operation(Fetched from RequestCharge property of ItemResponse class) - " + res.RequestCharge);
                    foreach (var item in res)
                    {

                    }

                    //res.RequestCharge; Provides RU"s consumed to fetch this particular page.
                    //totalRUs += res.RequestCharge;  Total RU's for all pages.
                }

                return Ok(sb.ToString());
            }
            catch (CosmosException ex)
            {
                return BadRequest();
            }
        }
    }
}
