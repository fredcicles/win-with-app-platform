using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Microsoft.Training
{
    public static class PatientsFunction
    {
        [FunctionName("PatientsFunction")]
        public static IActionResult GetSubmissionsByPatient(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "patients/{patientId}/submissions")] HttpRequest req,
            [CosmosDB(
                databaseName: "ToDoList",
                collectionName: "Items",
                ConnectionStringSetting = "CosmosDBConnection",
                PartitionKey = "{patientId}")]
                IEnumerable<Items> items,
            ILogger log)
        {
            log.LogInformation("Getting todo list items");
            return new OkObjectResult(items);
        }

        [FunctionName("SubmissionsFunction")]
        public static IActionResult GetSubmissionsById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "patients/{patientId}/submissions/{submissionId}")] HttpRequest req,
            [CosmosDB(
                databaseName: "ToDoList",
                collectionName: "Items",
                ConnectionStringSetting = "CosmosDBConnection",
                Id = "{submissionId}",
                PartitionKey = "{patientId}")]
                Items item,
            ILogger log, string submissionId)
        {
            log.LogInformation("Getting todo list item");
            return new OkObjectResult(item);
        }

        [FunctionName("CreatePatientsFunction")]
        public static async Task<IActionResult> CreateSubmission(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "patients/{patientId}/submissions")] HttpRequest req,
            [CosmosDB(
                databaseName: "ToDoList",
                collectionName: "Items",
                ConnectionStringSetting = "CosmosDBConnection")]
                IAsyncCollector<dynamic> itemOut,
            string patientId,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            if (!string.IsNullOrEmpty(patientId))
            {
                // Add a JSON document to the output container.
                await itemOut.AddAsync(new
                {
                    // create a random ID
                    id = System.Guid.NewGuid().ToString(),
                    patientId = patientId,
                    submittedOn = DateTime.Now,
                    healthStatus = "well",
                    symptoms = new List<string>{ "none" }
                });
            }

            string responseMessage = string.IsNullOrEmpty(patientId)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {patientId}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }
    }

    public class Items
    {
        [JsonProperty("id")]
        public string id {get;set;}
        [JsonProperty("partitionKey")]
        public string patientId {get;set;}
        public DateTime submittedOn {get;set;}
        public string healthStatus {get;set;}
        public List<string> symptoms {get; set;}
    }
}
