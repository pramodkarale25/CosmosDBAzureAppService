using CosmosDBAzureAppService.Model;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Scripts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace CosmosDBAzureAppService.Controllers
{
    [Route("api/[controller]/[action]")]
    public class SP_UDF_Triggers_Controller : ApiController
    {
        Product productItem = new Product("0120", "Worn Saddle", "accessories-used");
        PartitionKey partitionKey = new PartitionKey("accessories-used");
        private Container GetContainer()
        {
            return CosmosHelper.CreateDBAndContainer("SP_UDF_Trigger", "Product", "categoryId").Result;
        }

        [HttpPost]
        public async Task<StoredProcedureResponse> CreateStoredProc()
        {
            string storedProc = @"
                                    function createProduct(item)
                                    {
                                        var context = getContext();
                                        var container = context.getCollection(); 
                                        var accepted = container.createDocument(
                                            container.getSelfLink(),
                                            item,
                                            (error, newItem) => {
                                                if (error) throw error;
                                                context.getResponse().setBody(newItem)
                                            }
                                        );
                                        if (!accepted) return;
                                    }
                                ";

            StoredProcedureProperties storedProcedureProperties = new StoredProcedureProperties()
            {
                Id = "CreateProductSP",
                Body = storedProc
            };

            StoredProcedureResponse storedProcedureResponse = await GetContainer().Scripts.CreateStoredProcedureAsync(storedProcedureProperties);
            return storedProcedureResponse;
        }

        [HttpPost]
        public async Task<UserDefinedFunctionResponse> CreateUDF()
        {
            string UDF = @"
                            function getTaxedPrice(price)
                            {
                                return (price * 1.5);
                            }
                        ";

            UserDefinedFunctionProperties userDefinedFunctionProperties = new UserDefinedFunctionProperties()
            {
                Id = "getTaxedPriceUDF",
                Body = UDF
            };

            UserDefinedFunctionResponse udfResponse = await GetContainer().Scripts.CreateUserDefinedFunctionAsync(userDefinedFunctionProperties);
            return udfResponse;
        }

        [HttpPost]
        public async Task<TriggerResponse> CreatePreTrigger()
        {
            string preTrigger = @"
                            function AddPrice()
                            {
                               var context = getContext();
                               var request = context.getRequest();
                               var item = request.getBody();
                                
                               if (!('price' in item))
                                    item['price'] = 100;

                                request.setBody(item);
                            }
                        ";

            TriggerProperties triggerProperties = new TriggerProperties()
            {
                Id = "AddPricePreTriggerUDF",
                Body = preTrigger,
                TriggerOperation = TriggerOperation.Create,
                TriggerType = TriggerType.Pre
            };

            TriggerResponse triggerResponse = await GetContainer().Scripts.CreateTriggerAsync(triggerProperties);
            return triggerResponse;
        }

        [HttpPost]
        public async Task<TriggerResponse> CreatePostTrigger()
        {
            string postTrigger = @"
                            function CreateNewProduct()
                            {
                               var context = getContext();
                               var container = context.getCollection();
                               var request = context.getResponse();
                               var item = request.getBody();
                                
                               var newProduct = 
                                            {
                                                id : '1254',
                                                name: 'new item',
                                                categoryId : item.categoryId
                                            }

                                   var accepted = container.createDocument(
                                    container.getSelfLink(),
                                    newProduct,
                                    (error, newItem) => {
                                        if (error) throw error;
                                    }
                                );
                                if (!accepted) return;
                            }
                        ";

            TriggerProperties triggerProperties = new TriggerProperties()
            {
                Id = "CreateNewProductPostTriggerUDF",
                Body = postTrigger,
                TriggerOperation = TriggerOperation.Create,
                TriggerType = TriggerType.Post
            };

            TriggerResponse triggerResponse = await GetContainer().Scripts.CreateTriggerAsync(triggerProperties);
            return triggerResponse;
        }

        [HttpPost]
        public async Task<IHttpActionResult> ExecuteTrigger()
        {
            ItemRequestOptions itemRequestOptions = new ItemRequestOptions()
            {
                PreTriggers = new List<string>() { "AddPricePreTriggerUDF" },
                PostTriggers = new List<string>() { "CreateNewProductPostTriggerUDF" },
            };

            Product p = await GetContainer().CreateItemAsync<Product>(productItem, requestOptions: itemRequestOptions);
            return Ok(p);
        }

        [HttpGet]
        public async Task<IHttpActionResult> ExecuteUDF()
        {
            string query = "select c.name,c.price,udf.getTaxedPriceUDF(c.price) as TaxedPrice from c";

            QueryDefinition queryDefinition = new QueryDefinition(query);

            FeedIterator<Product> iterator = GetContainer().GetItemQueryIterator<Product>(queryDefinition);
            FeedResponse<Product> response = null;

            if (iterator.HasMoreResults)
            {
                response = await iterator.ReadNextAsync();
            }

            return Ok(response);
        }

        [HttpPost]
        public async Task<IHttpActionResult> ExecuteStoredProc()
        {
            return Ok();
        }
    }
}
