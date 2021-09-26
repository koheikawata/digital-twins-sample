using AdtManager.Models;
using Azure;
using Azure.DigitalTwins.Core;
using Azure.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;


namespace AdtManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdtController : ControllerBase
    {
        private readonly IConfiguration configuration;
        
        public AdtController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] List<Motion> motionList)
        {
            string adtInstanceUrl = "https://" + this.configuration.GetValue<string>("AdtHostName");

            //try
            //{
            //    ManagedIdentityCredential cred = new ManagedIdentityCredential(adtAppId);
            //    client = new DigitalTwinsClient(new Uri(adtInstanceUrl), cred);
            //}
            //catch (Exception e)
            //{
            //    Environment.Exit(0);
            //}

            var credential = new DefaultAzureCredential();
            var client = new DigitalTwinsClient(new Uri(adtInstanceUrl), credential);

            // <Print_model>
            // Read a list of models back from the service
            AsyncPageable<DigitalTwinsModelData> modelDataList = client.GetModelsAsync();
            await foreach (DigitalTwinsModelData md in modelDataList)
            {
                Console.WriteLine($"Model: {md.Id}");
                await client.DeleteModelAsync(md.Id);
            }

            AsyncPageable<BasicRelationship> relationships;
            AsyncPageable<IncomingRelationship> incomingRels;

            string query = "SELECT * FROM digitaltwins";
            AsyncPageable<BasicDigitalTwin> queryResult = client.QueryAsync<BasicDigitalTwin>(query);

            await foreach (BasicDigitalTwin twin in queryResult)
            {
                relationships = client.GetRelationshipsAsync<BasicRelationship>(twin.Id);
                await foreach (BasicRelationship relationship in relationships)
                {
                    await client.DeleteRelationshipAsync(twin.Id, relationship.Id);
                }

                incomingRels = client.GetIncomingRelationshipsAsync(twin.Id);
                await foreach (IncomingRelationship incomingRel in incomingRels)
                {
                    await client.DeleteRelationshipAsync(incomingRel.SourceId, incomingRel.RelationshipId);
                }

                await client.DeleteDigitalTwinAsync(twin.Id);
            }

            string dtdl = System.IO.File.ReadAllText("Models/SampleModel.json");
            var models = new List<string> { dtdl };
            try
            {
                await client.CreateModelsAsync(models);
            }
            catch (RequestFailedException e)
            {
                Console.WriteLine($"Upload model error: {e.Status}: {e.Message}");
            }

            // <Print_model>
            // Read a list of models back from the service
            modelDataList = client.GetModelsAsync();
            await foreach (DigitalTwinsModelData md in modelDataList)
            {
                Console.WriteLine($"Model: {md.Id}");
            }


            var twinData = new BasicDigitalTwin();
            twinData.Metadata.ModelId = "dtmi:example:SampleModel;1";
            twinData.Contents.Add("DisplayName", "SampleDevice");
            for (int i = 0; i < 6; i++)
            {
                twinData.Contents.Add($"Data{i+1}", motionList[0].Telemetry[i].TagValue);
            }
            twinData.Id = "sampleTwin";
            try
            {
                await client.CreateOrReplaceDigitalTwinAsync<BasicDigitalTwin>(twinData.Id, twinData);
            }
            catch (RequestFailedException e)
            {
                Console.WriteLine($"Create twin error: {e.Status}: {e.Message}");
            }

            // <Query_twins>
            // Run a query for all twins
            query = "SELECT * FROM digitaltwins";
            queryResult = client.QueryAsync<BasicDigitalTwin>(query);


            await foreach (BasicDigitalTwin twin in queryResult)
            {
                string js = JsonSerializer.Serialize(twin);
                Console.WriteLine(js);
                Console.WriteLine("---------------");
            }

            return Created("twindata", twinData);
        }
    }
}