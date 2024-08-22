using CosmosDBAzureAppService.Model;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace CosmosDBAzureAppService.Controllers
{
    [Route("api/[controller]/[action]")]
    public class TransactionBatchOperationController : ApiController
    {
        //Batch operations only works with same partition key.
        Product saddle = new Product("0120", "Worn Saddle", "accessories-used");
        Product handlebar = new Product("012A", "Rusty Handlebar", "accessories-used");
        PartitionKey partitionKey = new PartitionKey("accessories-used");

        private Container GetContainer()
        {
            return CosmosHelper.CreateDBAndContainer("TransactionBatchOperationDB", "Product", "categoryId").Result;
        }

        private List<Product> GetItemCollection(TransactionalBatchResponse batchRes)
        {
            List<Product> lstProduct = new List<Product>();

            if (batchRes.IsSuccessStatusCode)
            {
                TransactionalBatchOperationResult<Product> result;

                for (int i = 0; i < batchRes.Count; i++)
                {
                    result = batchRes.GetOperationResultAtIndex<Product>(i);
                    lstProduct.Add(result.Resource);
                }
            }

            return lstProduct;
        }

        [HttpPost]
        public async Task<IHttpActionResult> CreateMultipleItem()
        {
            TransactionalBatch batch = GetContainer().CreateTransactionalBatch(partitionKey)
                .CreateItem<Product>(saddle)
                .CreateItem<Product>(handlebar);

            /*
             OR 
                batch.CreateItem<Product>(handlebar);
                batch.DeleteItem(handlebar.id);
             */
            TransactionalBatchResponse batchRes = await batch.ExecuteAsync();
            return Ok(GetItemCollection(batchRes));
        }

        [HttpGet]
        public async Task<IHttpActionResult> ReadMultipleItem()
        {
            TransactionalBatch batch = GetContainer().CreateTransactionalBatch(partitionKey)
                .ReadItem(saddle.id)
                .ReadItem(handlebar.id);
            TransactionalBatchResponse batchRes = await batch.ExecuteAsync();
            return Ok(GetItemCollection(batchRes));
        }

        [HttpPatch]
        public async Task<IHttpActionResult> ReplaceMultipleItem()
        {
            saddle = new Product("0120", "Worn Saddle 1", "accessories-used");
            handlebar = new Product("012A", "Rusty Handlebar 1", "accessories-used");

            TransactionalBatch batch = GetContainer().CreateTransactionalBatch(partitionKey)
                .ReplaceItem<Product>(saddle.id, saddle)
                .ReplaceItem<Product>(handlebar.id, handlebar);
            TransactionalBatchResponse batchRes = await batch.ExecuteAsync();
            return Ok(GetItemCollection(batchRes));
        }

        [HttpPut]
        public async Task<IHttpActionResult> UpsertMultipleItem()
        {
            saddle = new Product("0120", "Worn Saddle 2", "accessories-used");
            handlebar = new Product("012A", "Rusty Handlebar 2", "accessories-used");

            TransactionalBatch batch = GetContainer().CreateTransactionalBatch(partitionKey)
                .UpsertItem<Product>(saddle)
                .UpsertItem<Product>(handlebar);
            TransactionalBatchResponse batchRes = await batch.ExecuteAsync();
            return Ok(GetItemCollection(batchRes));
        }

        [HttpDelete]
        public async Task<IHttpActionResult> DeleteMultipleItem()
        {
            TransactionalBatch batch = GetContainer().CreateTransactionalBatch(partitionKey)
                .DeleteItem(saddle.id)
                .DeleteItem(handlebar.id);
            TransactionalBatchResponse batchRes = await batch.ExecuteAsync();
            return Ok(GetItemCollection(batchRes));
        }
    }
}
