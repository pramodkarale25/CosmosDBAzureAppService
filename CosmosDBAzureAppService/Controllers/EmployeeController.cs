using CosmosDBAzureAppService.Model;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace CosmosDBAzureAppService.Controllers
{
    [Route("api/[controller]/[action]")]
    public class EmployeeController : ApiController
    {
        /// <summary>
        /// Commom Container Client, you can also pass the configuration paramter dynamically.
        /// </summary>
        /// <returns> Container Client </returns>
        private async Task<Container> ContainerClient()
        {
            CosmosClient cosmosDbClient = new CosmosClient(CosmosHelper.CosmosDBAccountUri, CosmosHelper.CosmosDBAccountPrimaryKey);
            CosmosClient cosmosDbClient1 = new CosmosClient(CosmosHelper.conString);
            InteractWithAccountProperties(cosmosDbClient1);
            Database cosmosDB = await cosmosDbClient.CreateDatabaseIfNotExistsAsync(CosmosHelper.CosmosDbName);
            await cosmosDB.CreateContainerIfNotExistsAsync(CosmosHelper.CosmosDbContainerName, "/Department");
            Container containerClient = cosmosDbClient.GetContainer(CosmosHelper.CosmosDbName, CosmosHelper.CosmosDbContainerName);
            return containerClient;
        }

        private async void InteractWithAccountProperties(CosmosClient cosmosDbClient)
        {
            AccountProperties accountProp = await cosmosDbClient.ReadAccountAsync();
            IEnumerable<AccountRegion> region = accountProp.ReadableRegions;
            region = accountProp.WritableRegions;
            string accID = accountProp.Id;
            AccountConsistency consistency = accountProp.Consistency;

            //Retrieve an existing database
            Database database = cosmosDbClient.GetDatabase("EmployeeManagementDB");
            //Create a new database
            database = await cosmosDbClient.CreateDatabaseAsync("EmployeeManagementDB");
            //Create database if it doesn't already exist
            database = await cosmosDbClient.CreateDatabaseIfNotExistsAsync("EmployeeManagementDB");

            //Retrieve an existing container
            Container container = database.GetContainer("Employees"); //OR
            cosmosDbClient.GetContainer("EmployeeManagementDB", "Employees");

            //Create a new container
            container = await database.CreateContainerAsync("Employees", "/Department", 400);

            //Create container if it doesn't already exist
            container = await database.CreateContainerIfNotExistsAsync("Employees", "/Department", 400);
        }

        private void ConfigureCosmosClientOptions()
        {
            CosmosClientOptions options = new CosmosClientOptions()
            {
                //ConnectionMode = ConnectionMode.Gateway,//used for connection through integrated cache.
                ConnectionMode = ConnectionMode.Direct,//used for direct connection to cosmos DB.
                ConsistencyLevel = ConsistencyLevel.Eventual,
                ApplicationRegion = "westus",
                ApplicationPreferredRegions = new List<string> { "westus", "eastus" },
            };

            CosmosClient client = new CosmosClient(CosmosHelper.conString, options);
        }

        [HttpPost]
        public async Task<IHttpActionResult> AddEmployee(EmployeeModel employee)
        {
            try
            {
                Container container = ContainerClient().Result;
                EmployeeModel empResponse = await container.CreateItemAsync(employee, new PartitionKey(employee.Department));
                return Ok(empResponse);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IHttpActionResult> GetEmployeeDetails()
        {
            try
            {
                Container container = ContainerClient().Result;
                string sqlQuery = "SELECT * FROM c";
                QueryDefinition queryDefinition = new QueryDefinition(sqlQuery);
                //queryDefinition.QueryText = string.Empty
                //queryDefinition.WithParameter("name", "value");

                FeedIterator<EmployeeModel> queryResultSetIterator = container.GetItemQueryIterator<EmployeeModel>(queryDefinition);
                List<EmployeeModel> employees = new List<EmployeeModel>();

                while (queryResultSetIterator.HasMoreResults)
                {
                    FeedResponse<EmployeeModel> currentResultSet = await queryResultSetIterator.ReadNextAsync();

                    foreach (EmployeeModel employee in currentResultSet)
                    {
                        employees.Add(employee);
                    }
                }

                return Ok(employees);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IHttpActionResult> GetEmployeeDetailsById(string employeeId, string partitionKey)
        {
            try
            {
                Container container = ContainerClient().Result;
                ItemResponse<EmployeeModel> response = await container.ReadItemAsync<EmployeeModel>(employeeId, new PartitionKey(partitionKey));
                return Ok(response.Resource);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        public async Task<IHttpActionResult> UpdateEmployee(EmployeeModel emp, string partitionKey)
        {
            try
            {
                Container container = ContainerClient().Result;
                ItemResponse<EmployeeModel> res = await container.ReadItemAsync<EmployeeModel>(emp.id, new PartitionKey(partitionKey));
                //Get Existing Item
                var existingItem = res.Resource;
                //Replace existing item values with new values
                existingItem.Name = emp.Name;
                existingItem.Country = emp.Country;
                existingItem.City = emp.City;
                existingItem.Department = emp.Department;
                existingItem.Designation = emp.Designation;
                var updateRes = await container.ReplaceItemAsync(existingItem, emp.id, new PartitionKey(partitionKey));
                return Ok(updateRes.Resource);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        public async Task<IHttpActionResult> DeleteEmployee(string empId, string partitionKey)
        {
            try
            {
                var container = ContainerClient().Result;
                var response = await container.DeleteItemAsync<EmployeeModel>(empId, new PartitionKey(partitionKey));
                return Ok(response.StatusCode);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}