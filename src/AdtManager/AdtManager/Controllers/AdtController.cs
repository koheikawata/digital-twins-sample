using AdtManager.Models;
using Azure;
using Azure.DigitalTwins.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AdtManager.Controllers
{
    [ApiController]
    public class AdtController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly DigitalTwinsClient digitalTwinsClient;
        private readonly BasicDigitalTwin basicDigitalTwin;

        private const string twinDisplayName = "SampleDevice";
        private const string twinId = "SampleTwin";

        public AdtController(IConfiguration configuration, DigitalTwinsClient digitalTwinsClient, BasicDigitalTwin basicDigitalTwin)
        {
            this.configuration = configuration;
            this.digitalTwinsClient = digitalTwinsClient;
            this.basicDigitalTwin = basicDigitalTwin;
        }

        [HttpPost("api/[controller]/init")]
        public async Task<IActionResult> AdtInit([FromBody] object content)
        {
            await DeleteModelRelationshipTwins(this.digitalTwinsClient);
            await UploadModel(content);

            Dtdl dtdl = JsonConvert.DeserializeObject<Dtdl>(content.ToString());
            this.basicDigitalTwin.Id = twinId;
            this.basicDigitalTwin.Metadata.ModelId = dtdl.Id;
            this.basicDigitalTwin.Contents.Add("DisplayName", twinDisplayName);
            for (int i = 0; i < 6; i++)
            {
                this.basicDigitalTwin.Contents.Add($"Data{i + 1}", 0);
            }
            await WriteDigitalTwins(this.basicDigitalTwin);

            return Ok(this.basicDigitalTwin);
        }

        [HttpPost("api/[controller]/write")]
        public async Task<IActionResult> AdtWrite([FromBody] List<Motion> motionList)
        {
            for (int i = 0; i < 6; i++)
            {
                basicDigitalTwin.Contents[$"Data{i + 1}"] = motionList[0].Telemetry[i].TagValue;
            }
            await WriteDigitalTwins(this.basicDigitalTwin);

            return Ok(this.basicDigitalTwin);
        }

        [HttpGet("api/[controller]")]
        public async Task<IActionResult> AdtGet()
        {
            string query = "SELECT * FROM digitaltwins";
            AsyncPageable<BasicDigitalTwin> queryResult = this.digitalTwinsClient.QueryAsync<BasicDigitalTwin>(query);
            var twinList = new List<BasicDigitalTwin>();

            await foreach (BasicDigitalTwin twin in queryResult)
            {
                twinList.Add(twin);
            }
            return Created("twinlist", twinList);
        }

        private async Task DeleteModelRelationshipTwins(DigitalTwinsClient digitalTwinsClient)
        {
            AsyncPageable<DigitalTwinsModelData> modelDataList = this.digitalTwinsClient.GetModelsAsync();
            await foreach (DigitalTwinsModelData md in modelDataList)
            {
                await this.digitalTwinsClient.DeleteModelAsync(md.Id);
            }

            AsyncPageable<BasicRelationship> relationships;
            AsyncPageable<IncomingRelationship> incomingRels;

            string query = "SELECT * FROM digitaltwins";
            AsyncPageable<BasicDigitalTwin> queryResult = this.digitalTwinsClient.QueryAsync<BasicDigitalTwin>(query);

            await foreach (BasicDigitalTwin twin in queryResult)
            {
                relationships = this.digitalTwinsClient.GetRelationshipsAsync<BasicRelationship>(twin.Id);
                await foreach (BasicRelationship relationship in relationships)
                {
                    await this.digitalTwinsClient.DeleteRelationshipAsync(twin.Id, relationship.Id);
                }

                incomingRels = this.digitalTwinsClient.GetIncomingRelationshipsAsync(twin.Id);
                await foreach (IncomingRelationship incomingRel in incomingRels)
                {
                    await this.digitalTwinsClient.DeleteRelationshipAsync(incomingRel.SourceId, incomingRel.RelationshipId);
                }

                await this.digitalTwinsClient.DeleteDigitalTwinAsync(twin.Id);
            }
        }

        private async Task UploadModel(object content)
        {
            string dtdl = content.ToString();
            var models = new List<string> { dtdl };
            try
            {
                await this.digitalTwinsClient.CreateModelsAsync(models);
            }
            catch (RequestFailedException e)
            {
                Console.WriteLine($"Upload model error: {e.Status}: {e.Message}");
            }
        }

        private async Task WriteDigitalTwins(BasicDigitalTwin basicDigitalTwin)
        {
            try
            {
                await digitalTwinsClient.CreateOrReplaceDigitalTwinAsync<BasicDigitalTwin>(this.basicDigitalTwin.Id, this.basicDigitalTwin);
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
}