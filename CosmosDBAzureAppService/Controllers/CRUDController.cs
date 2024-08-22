using CosmosDBAzureAppService.Model;
using Microsoft.Azure.Cosmos;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Container = Microsoft.Azure.Cosmos.Container;

namespace CosmosDBAzureAppService.Controllers
{
    [Route("api/[controller]/[action]")]
    public class CRUDController : ApiController
    {
        static string id = "027D0B9A-F9D9-4C96-8213-C8546C4AAE71";
        static string categoryId = "26C74104-40BC-4541-8EF5-9892F7F03D72";

        private Container GetContainer()
        {
            return CosmosHelper.CreateDBAndContainer("CRUDDB", "Product", "categoryId").Result;
        }

        [HttpPost]
        public async Task<IHttpActionResult> CreateItem()
        {
            Product product = new Product()
            {
                id = id,
                categoryId = categoryId,
                name = "LL Road Seat/Saddle",
                price = 27.12d,
                tags = new string[] { "brown", "weathered" }
            };

            ItemResponse<Product> pr;

            try
            {
                pr = await GetContainer().CreateItemAsync(product, new PartitionKey(product.categoryId));
                return Ok(pr);
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
        public async Task<IHttpActionResult> ReadItem(string id, string partitionKey)
        {
            ItemResponse<Product> pr;

            try
            {
                pr = await GetContainer().ReadItemAsync<Product>(id, new PartitionKey(partitionKey));
                return Ok(pr.Resource);
            }
            catch (CosmosException ex)
            {
                return BadRequest();
            }
        }

        [HttpPost]
        public async Task<IHttpActionResult> CreateORUpdateItem()
        {
            Product product = new Product()
            {
                id = id,
                categoryId = categoryId,
                name = "LL Road Seat/Saddle",
                price = 35.00d,
                tags = new string[] { "brown", "new", "crisp" },
            };
            ItemResponse<Product> pr;

            try
            {
                pr = await GetContainer().UpsertItemAsync(product, new PartitionKey(product.categoryId));
                //if(pr.StatusCode == HttpStatusCode.Created)200
                //if(pr.StatusCode == HttpStatusCode.OK)//updated/replaced

                return Ok(pr);
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

        [HttpPost]
        public async Task<IHttpActionResult> CreateORUpdateItemWithTTL(int ttl)
        {
            Product product = new Product()
            {
                id = Guid.NewGuid().ToString(),
                categoryId = Guid.NewGuid().ToString(),
                name = "LL Road Seat/Saddle",
                price = 35.00d,
                tags = new string[] { "brown", "new", "crisp" },
                ttl = ttl
            };
            ItemResponse<Product> pr;

            try
            {
                pr = await GetContainer().UpsertItemAsync(product, new PartitionKey(product.categoryId));
                //if(pr.StatusCode == HttpStatusCode.Created)200
                //if(pr.StatusCode == HttpStatusCode.OK)//updated/replaced

                return Ok(pr);
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

        [HttpDelete]
        public async Task<IHttpActionResult> DeleteItem(string id, string partitionKey)
        {
            ItemResponse<Product> pr;

            try
            {
                pr = await GetContainer().DeleteItemAsync<Product>(id, new PartitionKey(partitionKey));
                return Ok(pr);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict) //Example of exception filter.
            {
                return BadRequest();
            }
        }
    }
}
