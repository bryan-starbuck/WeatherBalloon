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
using WeatherBalloon.Cloud.Documents;

namespace WeatherBalloon.Cloud
{
    public static class HubToCosmos
    {

        private static string cosmosUrl = Environment.GetEnvironmentVariable("CosmosUrl");
        private static string cosmosKey = Environment.GetEnvironmentVariable("CosmosKey");
        private static string cosmosDB = Environment.GetEnvironmentVariable("CosmosDB");
        private static string cosmosDoc = Environment.GetEnvironmentVariable("CosmosDoc");

        [FunctionName("HubToCosmos")]
        public static async void Run([IoTHubTrigger("messages/events", Connection = "IoTConn", ConsumerGroup ="azurefunction")]EventData message, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var messageString = Encoding.UTF8.GetString(message.Body.Array);
            string messageType = null;

            JObject jsonObject = JObject.Parse(messageString);

            if (jsonObject.ContainsKey("Type"))
            {
                messageType = (string)jsonObject["Type"];
            }

            if (messageType == "tracker")
            {
                var trackerMessage = JsonConvert.DeserializeObject<TrackerMessage>(messageString);
                var trackerDocument =  TrackerDocument.Create(trackerMessage);

                if (trackerDocument != null)
                {
                    await createDocument<TrackerDocument>(trackerDocument);
                }
            }
            else if (messageType == "balloon")
            {
                var balloonMessage = JsonConvert.DeserializeObject<BalloonMessage>(messageString);
                var balloonDocument = BalloonDocument.Create(balloonMessage);

                if (balloonDocument != null)
                {
                    await createDocument<BalloonDocument>(balloonDocument);
                }
            }
        }
        private static async Task createDocument<T>(T content) where T: DocumentBase
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