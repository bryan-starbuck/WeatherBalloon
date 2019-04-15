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

using WeatherBalloon.Cloud.HabHub;
using WeatherBalloon.Cloud.Documents;
using WeatherBalloon.Cloud.Twilio;
using Microsoft.Azure.Documents.Spatial;

namespace WeatherBalloon.Cloud
{

    public static class TrackerToPrediction
    {
        private static string cosmosUrl = Environment.GetEnvironmentVariable("CosmosUrl");
        private static string cosmosKey = Environment.GetEnvironmentVariable("CosmosKey");
        private static string cosmosDB = Environment.GetEnvironmentVariable("CosmosDB");
        private static string cosmosDoc = Environment.GetEnvironmentVariable("CosmosDoc");

        private static string SMSNotificationFunctionKey = Environment.GetEnvironmentVariable("SMSNotificationFunctionKey");
        
        static HttpClient client = new HttpClient();


       
        

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
            queryBuilder.Append(cosmosDoc).Append(" WHERE ").Append(cosmosDoc).Append(whereVariable).Append(" = \'balloon\'");
            queryBuilder.Append(" ORDER BY ").Append(cosmosDoc).Append(orderByVariable).Append(" DESC");

            log.LogInformation($"Query text: {queryBuilder.ToString()}");

            var client = new DocumentClient(new Uri(cosmosUrl), cosmosKey);

            FeedOptions feedOptions = new FeedOptions() { EnableCrossPartitionQuery = true };
            //Get latest balloon message from Cosmos
            var balloonDocument =
                client.CreateDocumentQuery<BalloonDocument>(UriFactory.CreateDocumentCollectionUri(cosmosDB, cosmosDoc), 
                    queryBuilder.ToString(), feedOptions).AsEnumerable().FirstOrDefault();

            log.LogInformation($"Balloon state: {balloonDocument.State}");

            if (balloonDocument.State == BalloonState.PreLaunch || balloonDocument.State == BalloonState.Landed)
            {
                log.LogInformation($"Balloon not in the air, no prediction calculated");
                return;
            }

            if (balloonDocument.AveAscent <= 5)
            {
                // climb rate of less than 1 is a invalid in habhub
                log.LogInformation($"Balloon Ave Ascent rate too low ({balloonDocument.AveAscent}), adjusted to 5 m/s");
                balloonDocument.AveAscent = 5;
            }

            if (balloonDocument.BurstAltitude > 30000)
            {
                log.LogInformation("Balloon burst set too high, forcing to 30,000 m.");
                balloonDocument.BurstAltitude = 30000;
            }

            if (balloonDocument.State == BalloonState.Rising)
            {
                try 
                {
                    PredictionEngine predictionEngine = new PredictionEngine();
                    var predictionDocument = predictionEngine.Generate(balloonDocument, log);

                    // enrich prediction document
                    if (predictionDocument != null)
                    {
                        predictionDocument.FlightId = balloonDocument.FlightId;
                        predictionDocument.TrackerSource = balloonDocument.TrackerSource;
                        predictionDocument.Geopoint = new Point(predictionDocument.Longitude, predictionDocument.Latitude);
                        predictionDocument.partitionid = balloonDocument.partitionid;

                        WriteDocument(predictionDocument).Wait();

                        try
                        {
                            var predictionNotification = new PredictionNotification(predictionDocument);

                            SendPredictionNotification(predictionNotification);
                        }
                        catch (Exception ex)
                        {
                            log.LogError(ex, "Failed to send prediction to notification function.");
                        }

                    }
                }
                catch (Exception ex)
                {
                    log.LogError(ex, $"Failed to generate prediction: {ex.Message}");
                }
            }
            else if (balloonDocument.State == BalloonState.Falling)
            {
                // can't create a new prediction will falling
                var lastPredictionDocument = GetLastPrediction(balloonDocument.FlightId);
                if(lastPredictionDocument != null)
                {
                    lastPredictionDocument.DistanceToLanding = 10; // todo

                    var notification = new PredictionNotification(lastPredictionDocument);

                    // notify of balloon location
                    SendPredictionNotification(notification);
                }
            }
        }

        private static PredictionDocument GetLastPrediction(string flightId)
        {
            using (var client = new DocumentClient(new Uri(cosmosUrl), cosmosKey))
            {
                var query = $"SELECT Top 1* FROM c where c.FlightId = {flightId} and c.Type='prediction' order by c._ts desc";

                FeedOptions feedOptions = new FeedOptions() { EnableCrossPartitionQuery = true };
                //Get Tracker message from Cosmos
                var predictionDocument = client.CreateDocumentQuery<PredictionDocument>(UriFactory.CreateDocumentCollectionUri(cosmosDB, cosmosDoc), 
                    query, feedOptions).AsEnumerable().FirstOrDefault();

                return predictionDocument;
            }
        }

        /// <summary>
        /// Send the new prediction to the SMS Notification function
        /// </summary>
        /// <param name="prediction"></param>
        /// <returns></returns>
        private static void SendPredictionNotification(PredictionNotification predictionNotification)
        {
            // todo - put this in the app properties
            var url = $"https://habservices.azurewebsites.net/api/HttpToSMSNotification?code={SMSNotificationFunctionKey}";
            var body = JsonConvert.SerializeObject(predictionNotification);
            
            client.PostAsJsonAsync(url, body);
        }

        /// <summary>
        /// Write document to cosmos - TODO, refactor Cosmos operations into class shared with other azure functions
        /// </summary>
        /// <param name="prediction"></param>
        /// <returns></returns>
        private static async Task WriteDocument<T>(T prediction) where T : DocumentBase
        {
            // TODO - refactor this code is in two places

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

