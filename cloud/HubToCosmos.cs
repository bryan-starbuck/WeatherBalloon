using IoTHubTrigger = Microsoft.Azure.WebJobs.EventHubTriggerAttribute;

using System.Text;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Documents.Client;
using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.EventHubs;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Balloon
{

    public class IoTDocument
    {
        public string etlTime { get; set; }
        public double lat { get; set; }
        public double lon { get; set; }
        public double initial_alt { get; set; }
        public double hour { get; set; }
        public double min { get; set; }
        public double second { get; set; }
        public double day { get; set; }
        public double month { get; set; }
        public double year { get; set; }
        public double ascent { get; set; }
        public double drag { get; set; }
        public string burst { get; set; }
    }
    public static class HubToCosmos
    {

        private static string cosmosUrl = Environment.GetEnvironmentVariable("CosmosUrl");
        private static string cosmosKey = Environment.GetEnvironmentVariable("CosmosKey");
        private static string cosmosDB = Environment.GetEnvironmentVariable("CosmosDB");
        private static string cosmosDoc = Environment.GetEnvironmentVariable("CosmosDoc");
        private static string burst = Environment.GetEnvironmentVariable("Burst");

        [FunctionName("HubToCosmos")]
        public static void Run([IoTHubTrigger("messages/events", Connection = "IoTHub")]EventData message, ILogger log)
        {
            var receivedIoT = JsonConvert.DeserializeObject<IoTDocument>(Encoding.UTF8.GetString(message.Body.Array));
            receivedIoT.etlTime = DateTime.Now.ToString();
            receivedIoT.burst = burst;
            createDocument(receivedIoT);
        }
        private static async Task createDocument(IoTDocument content)
        {
            using (var client = new DocumentClient(new Uri(cosmosUrl), cosmosKey))
            {
                await client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(cosmosDB, cosmosDoc), content);
            }
        }
    }
}