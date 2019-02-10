using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System.Net;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;
using System.Text;
using WeatherBalloon.Messaging;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web;
using System.Collections.Generic;
using System.IO;

namespace WeatherBalloon.Cloud
{

    public static class TrackerToPrediction
    {
        private static string cosmosUrl = Environment.GetEnvironmentVariable("CosmosUrl");
        private static string cosmosKey = Environment.GetEnvironmentVariable("CosmosKey");
        private static string cosmosDB = Environment.GetEnvironmentVariable("CosmosDB");
        private static string cosmosDoc = Environment.GetEnvironmentVariable("CosmosDoc");
        
        


        // for time conversion
        

        [FunctionName("TrackerToPrediction")]
        public static void Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            //Configurable variables for Cosmos Query
            var minPartitionId = DateTime.Now.ToString("yyyy-MM-dd HH");
            var whereVariable = ".Type";
            var orderByVariable = "._ts";

            var queryBuilder = new StringBuilder();
            queryBuilder.Append("SELECT Top 1* FROM ");
            queryBuilder.Append(cosmosDoc).Append(" WHERE ").Append(cosmosDoc).Append(whereVariable).Append(" = \'tracker\'");
            queryBuilder.Append(" ORDER BY ").Append(cosmosDoc).Append(orderByVariable).Append(" DESC");

            log.LogInformation($"Query text: {queryBuilder.ToString()}");

            var client = new DocumentClient(new Uri(cosmosUrl), cosmosKey);

            FeedOptions feedOptions = new FeedOptions() { EnableCrossPartitionQuery = true };
            //Get Tracker message from Cosmos
            var trackerMessage =
                client.CreateDocumentQuery<TrackerMessage>(UriFactory.CreateDocumentCollectionUri(cosmosDB, cosmosDoc), 
                    queryBuilder.ToString(), feedOptions).AsEnumerable().FirstOrDefault();

            log.LogInformation($"Balloon state: {trackerMessage.State}");

            if (trackerMessage.State == BalloonState.PreLaunch || trackerMessage.State == BalloonState.Landed)
            {
                log.LogInformation($"Balloon not in the air, no prediction calculated");
                return;
            }

            if (trackerMessage.BalloonLocation.climb < 1)
            {
                // climb rate of less than 1 is a invalid in habhub
                log.LogInformation($"Balloon Climb rate too low ({trackerMessage.BalloonLocation.climb}), adjusted to 5 m/s");
                trackerMessage.BalloonLocation.climb = 5;
            }

            if (trackerMessage.BurstAltitude > 30000)
            {
                log.LogInformation("Balloon burst set too high, forcing to 30,000 m.");
                trackerMessage.BurstAltitude = 30000;
            }

            try 
            {
                PredictionEngine predictionEngine = new PredictionEngine();
                var prediction = predictionEngine.Generate(trackerMessage, log);
                if (prediction != null)
                {
                    prediction.BalloonLocation = trackerMessage.BalloonLocation;
                    WriteDocument(prediction).Wait();
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, $"Failed to generate prediction: {ex.Message}");
            }
        }

        /// <summary>
        /// Write document to cosmos - TODO, refactor Cosmos operations into class shared with other azure functions
        /// </summary>
        /// <param name="prediction"></param>
        /// <returns></returns>
        private static async Task WriteDocument(PredictionMessage prediction)
        {
            try
            {
                using (var client = new DocumentClient(new Uri(cosmosUrl), cosmosKey))
                {
                    await client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(cosmosDB, cosmosDoc), 
                        prediction, new RequestOptions { PartitionKey = new PartitionKey(prediction.partitionid)});
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}

