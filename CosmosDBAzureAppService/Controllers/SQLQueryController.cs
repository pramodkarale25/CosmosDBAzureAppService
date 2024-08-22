using CosmosDBAzureAppService.Model;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace CosmosDBAzureAppService.Controllers
{
    [Route("api/[controller]/[action]")]
    public class SQLQueryController : ApiController
    {
        SQLProduct product = new SQLProduct()
        {
            id = "86FD9250-4BD5-42D2-B941-1C1865A6A65E",
            categoryId = "F3FBB167-11D8-41E4-84B4-5AAA92B1E737",
            categoryName = "Components, Touring Frames",
            sku = "FR-T67U-58",
            name = "LL Touring Frame - Blue, 58",
            description = "The product called LL Touring Frame - Blue, 58",
            price = 333.42,
            tags = new Tag[]
            {
                new Tag(){id= "764C1CC8-2E5F-4EF5-83F6-8FF7441290B3",name= "Tag-190"},
                new Tag(){id= "765EF7D7-331C-42C0-BF23-A3022A723BF7",name= "Tag-191"}
            },
        };

        private Container GetContainer()
        {
            return CosmosHelper.CreateDBAndContainer("ProductDB", "Product", "categoryId").Result;
        }

        [HttpPost]
        public async Task<IHttpActionResult> CreateItem()
        {
            return Ok(await GetContainer().CreateItemAsync<SQLProduct>(product, new PartitionKey(product.categoryId)));
        }

        [HttpGet]
        public async Task<IHttpActionResult> GetItem(string query)
        {
            /*
                select * from c

                select * 
                from c
                where c.price=333.422

                select c.id,c.name 
                from c
                where c.price=333.42

                select c.id as ID,c.name as NAME 
                from c
                where c.price=333.42

                select distinct c.id as ID,c.name as NAME 
                from c
                where c.price=333.42
             */

            QueryRequestOptions queryOptions = new QueryRequestOptions();
            queryOptions.PartitionKey = new PartitionKey(product.categoryId);
            QueryDefinition definition = new QueryDefinition(query);
            FeedIterator<SQLProduct> feedIterator = GetContainer().GetItemQueryIterator<SQLProduct>(definition, null, queryOptions);
            List<SQLProduct> products = new List<SQLProduct>();

            while (feedIterator.HasMoreResults)
            {
                FeedResponse<SQLProduct> sqlProduct = await feedIterator.ReadNextAsync();

                foreach (SQLProduct item in sqlProduct)
                {
                    products.Add(item);
                }
            }

            return Ok(products);
        }

        [HttpGet]
        public async Task<IHttpActionResult> GetItemValueWithoutModel(string query)
        {
            /*
                SELECT DISTINCT VALUE p.categoryName FROM products p
             */

            QueryRequestOptions queryOptions = new QueryRequestOptions();
            queryOptions.PartitionKey = new PartitionKey(product.categoryId);
            QueryDefinition definition = new QueryDefinition(query);
            FeedIterator<string> feedIterator = GetContainer().GetItemQueryIterator<string>(definition, null, queryOptions);
            List<string> categoryList = new List<string>();

            while (feedIterator.HasMoreResults)
            {
                FeedResponse<string> catList = await feedIterator.ReadNextAsync();

                foreach (string category in catList)
                {
                    categoryList.Add(category);
                }
            }

            return Ok(categoryList);
        }

        [HttpGet]
        public async Task<IHttpActionResult> GetItemValueWithoutModelBuildInFunction(string query)
        {
            /*
                SELECT DISTINCT IS_DEFINED(p.tags) AS tags_exist FROM products p
                SELECT IS_ARRAY(p.tags) AS tags_is_array FROM products p
                SELECT IS_NULL(p.tags) AS tags_is_null FROM products p
                SELECT * FROM products p WHERE IS_NUMBER(p.PRICE)
                SELECT * FROM products p WHERE IS_STRING(p.PRICE)
                SELECT * FROM products p WHERE IS_BOOLEAN(p.PRICE)
                SELECT * FROM products p WHERE IS_OBJECT(p.PRICE)
                SELECT VALUE CONCAT(p.name, ' | ', p.categoryName) FROM products p
                SELECT VALUE LOWER(p.sku) FROM products p
                SELECT GetCurrentDateTime() FROM products p
             */
            QueryRequestOptions queryOptions = new QueryRequestOptions();
            queryOptions.PartitionKey = new PartitionKey(product.categoryId);
            QueryDefinition definition = new QueryDefinition(query);
            FeedIterator<string> feedIterator = GetContainer().GetItemQueryIterator<string>(definition, null, queryOptions);
            List<string> categoryList = new List<string>();

            while (feedIterator.HasMoreResults)
            {
                FeedResponse<string> catList = await feedIterator.ReadNextAsync();

                foreach (string category in catList)
                {
                    categoryList.Add(category);
                }
            }

            return Ok(categoryList);
        }

        [HttpGet]
        public async Task<IHttpActionResult> GetCrossProductResult()
        {
            string query = @"
                SELECT p.id,p.name, t.class
                FROM products p
                JOIN t IN p.tags
                where t.class='group'
                order by p.id";

            return Ok(query);
        }

        [HttpGet]
        public async Task<IHttpActionResult> GetQueryWithParameter()
        {
            string query = @"
                SELECT p.id,p.name, t.class
                FROM products p
                JOIN t IN p.tags
                where t.class=@className
                order by p.id";

            QueryDefinition qd = new QueryDefinition(query);
            qd.WithParameter("@classname", "");
            return Ok(query);
        }

        [HttpGet]
        public async Task<IHttpActionResult> GetPaginationQuery()
        {
            string query = "SELECT * from p";

            QueryDefinition qd = new QueryDefinition(query);
            QueryRequestOptions options = new QueryRequestOptions();
            options.MaxItemCount = 1;

            FeedIterator<Product> iterator = GetContainer().GetItemQueryIterator<Product>(query, requestOptions: options);

            while (iterator.HasMoreResults)// indicates more pages to return
            {
                FeedResponse<Product> res = await iterator.ReadNextAsync();//get next set of items

                foreach (Product item in res)
                {

                }
            }

            return Ok(query);
        }
    }

    public class SQLProduct
    {
        public string id { get; set; }
        public string categoryId { get; set; }
        public string categoryName { get; set; }
        public string sku { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public double price { get; set; }
        public Tag[] tags { get; set; }
    }

    public class Tag
    {
        public string id { get; set; }
        public string name { get; set; }
    }
}
