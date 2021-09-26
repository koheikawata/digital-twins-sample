using System;
using System.Threading.Tasks;
using Azure;
using Azure.DigitalTwins.Core;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace DeviceConnector
{
    class Program
    {
        private static async Task Main(string[] args)
        {
            IConfiguration configuration = new ConfigurationBuilder()
              .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
              .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true)
              .Build();

            string adtInstanceUrl = "https://" + configuration.GetValue<string>("AdtHostName");

            var credential = new DefaultAzureCredential();
            var client = new DigitalTwinsClient(new Uri(adtInstanceUrl), credential);

            string query = "SELECT * FROM digitaltwins";
            AsyncPageable<BasicDigitalTwin> queryResult = client.QueryAsync<BasicDigitalTwin>(query);

            await foreach (BasicDigitalTwin twin in queryResult)
            {
                string js = System.Text.Json.JsonSerializer.Serialize(twin);
                var parsedJson = JsonConvert.DeserializeObject(js);
                Console.WriteLine(JsonConvert.SerializeObject(parsedJson, Formatting.Indented));
                Console.WriteLine("---------------");
            }
        }
    }
}
