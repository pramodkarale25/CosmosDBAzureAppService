using Newtonsoft.Json;

namespace CosmosDBAzureAppService.Model
{
    public class Product
    {
        public Product()
        {
        }

        public Product(string id, string name, string categoryId, double price = 123, string[] tags = null)
        {
            this.id = id;
            this.name = name;
            this.categoryId = categoryId;
            this.price = price;
            this.tags = tags;
        }

        public string id { get; set; }

        public string name { get; set; }

        public string categoryId { get; set; }

        public double price { get; set; }

        public string[] tags { get; set; }

        [JsonProperty(PropertyName = "ttl", NullValueHandling = NullValueHandling.Ignore)]
        public int? ttl { get; set; }
    }
}
