using IoTHubTrigger = Microsoft.Azure.WebJobs.EventHubTriggerAttribute;

using System.Text;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.EventHubs;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

using System.Net;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;

using WeatherBalloon.Messaging;

namespace WeatherBalloon.Cloud
{
    public static class HubToCosmos
    {

        private static string cosmosUrl = Environment.GetEnvironmentVariable("CosmosUrl");
        private static string cosmosKey = Environment.GetEnvironmentVariable("CosmosKey");
        private static string cosmosDB = Environment.GetEnvironmentVariable("CosmosDB");
        private static string cosmosDoc = Environment.GetEnvironmentVariable("CosmosDoc");

        [FunctionName("HubToCosmos")]
        public static void Run([IoTHubTrigger("messages/events", Connection = "IoTConn")]EventData message, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            var receivedIoT = JsonConvert.DeserializeObject<TrackerMessage>(Encoding.UTF8.GetString(message.Body.Array));
            createDocument(receivedIoT);

        }
        private static async Task createDocument(TrackerMessage content)
        {
            try
            {
                using (var client = new DocumentClient(new Uri(cosmosUrl), cosmosKey))
                {
                    await client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(cosmosDB, cosmosDoc), 
                        content, new RequestOptions { PartitionKey = new PartitionKey(content.partitionid)});
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }
    }
}