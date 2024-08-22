using CosmosDBAzureAppService.Model;
using Microsoft.Azure.Cosmos;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace CosmosDBAzureAppService.Controllers
{
    [Route("api/[controller]/[action]")]
    public class BulkOperationController : ApiController
    {
        Product saddle = new Product("0120", "Worn Saddle", "accessories-used");
        Product handlebar = new Product("012A", "Rusty Handlebar", "accessories-used");
        private Container GetContainer()
        {
            return CosmosHelper.CreateDBAndContainer("BulkOperationDB", "Bulk", "categoryId").Result;
        }

        [HttpPost]
        public async Task<IHttpActionResult> InsertBulk()
        {
            CosmosClientOptions options = new CosmosClientOptions()
            {
                AllowBulkExecution = true// this flag need to turn ON
            };

            Container container = GetContainer();

            List<Product> productsToInsert = new List<Product>
            {
                saddle,
                handlebar
            };// = GetOurProductsFromSomeWhere(); may be from request.

            List<Task> concurrentTasks = new List<Task>();

            foreach (Product product in productsToInsert)
            {
                concurrentTasks.Add(
                    container.CreateItemAsync(
                        product,
                        new PartitionKey(product.categoryId))
                    .ContinueWith((Task<ItemResponse<Product>> task) =>
                    {
                        ItemResponse<Product> response = task.Result;

                        if (response.StatusCode != System.Net.HttpStatusCode.Created)
                        {
                            //log errors.
                        }
                    })
                );
            }

            await Task.WhenAll(concurrentTasks);

            return Ok();
        }
    }
}
