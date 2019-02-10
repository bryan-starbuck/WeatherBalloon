using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web;
using System.Linq;
using Newtonsoft.Json;
using System.Text;

using WeatherBalloon.Messaging;
using System.IO;

namespace WeatherBalloon.Cloud
{
    public class PredictionEngine
    {
        private static HttpClient client = new HttpClient();
        private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static DateTime UnixTimeToDateTime(long unixTime)
        {
            return epoch.AddSeconds(unixTime);
        }

        public PredictionEngine()
        {


        }

        public PredictionMessage Generate(TrackerMessage trackerMessage, ILogger log)
        {
            HabHubMessage habHubMessage = new HabHubMessage();
            habHubMessage.alt = trackerMessage.BalloonLocation.alt;
            habHubMessage.burst = trackerMessage.BurstAltitude;
            habHubMessage.lat = trackerMessage.BalloonLocation.lat;
            habHubMessage.lon = trackerMessage.BalloonLocation.@long;

            habHubMessage.ascent = trackerMessage.AveAscent;

            return CreateHabHubPrediction(habHubMessage).Result;
        }

        private static async Task<PredictionMessage> CreateHabHubPrediction(HabHubMessage habHubMessage)
        {
            // First step, tell HabHub.org to run a prediction.  This will give us a Uuid to identify the prediction
            var content = new FormUrlEncodedContent(habHubMessage.ToParameterDictionary());
            
            var response = await client.PostAsync("http://predict.habhub.org/ajax.php?action=submitForm", content);

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();

                var predictionResponse = JsonConvert.DeserializeObject<HabHubPredictionResponse>(responseString);
                // get uuid
                var uuid = predictionResponse.uuid;

                // now check the status of generating the prediction
                var progressUrl = BuildProgressUrl(uuid);
                
                var isComplete = false;
                var counter = 0;

                do 
                {
                    var progressResponse = await client.GetAsync(progressUrl);
                    var result = progressResponse.Content.ReadAsStringAsync().Result;
                    if (result.ToLower().Contains("\"pred_complete\": true"))
                    {
                        isComplete = true;
                    }
                    else 
                    {
                        counter++;
                        System.Threading.Thread.Sleep(50);
                    }
                }
                while (isComplete == false && counter < 5); // wait a max of 250 ms

                // now get the result records
                var url = BuildResultsUrl(uuid);
                response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var csv = getSanitizedCsv(response).Result;
                    var landingRecord = getLastRecord(csv);

                    var predictionMessage = generatePredictionMessage(landingRecord);

                    if (predictionMessage !=null)
                    {
                        return predictionMessage;
                    }
                }
            }
            
            
            return null;
       }

        

        private static string BuildResultsUrl(string uuid)
        {
            // example request: http://predict.habhub.org/ajax.php?action=getCSV&uuid=202f57505788f22612beb5093b66ff53c60a43b5

            var builder = new UriBuilder("http://predict.habhub.org/ajax.php");
            builder.Port = -1;
            var query = HttpUtility.ParseQueryString(builder.Query);
            query["action"] = "getCSV";
            query["uuid"] = uuid;
            builder.Query = query.ToString();
            var url = builder.ToString();

            return url;
        }
        private static string BuildProgressUrl(string uuid)
        {
            // example request: http://predict.habhub.org/preds/127603d910c4b0fbf99dd95d104b489721de6d5a/progress.json

            return $"http://predict.habhub.org/preds/{uuid}/progress.json";
        }

        private static async Task<string> getSanitizedCsv(HttpResponseMessage response)
        {
            var responseString = await response.Content.ReadAsStringAsync();
            var csvString = Regex.Replace(responseString, "\\\",\\\"", ",\r\n").Replace("[", "").Replace("]", "");
            var csvString2 = Regex.Replace(csvString, "\\\"", "");

            return csvString2;

        }

        private static HabHubPredictionPoint getLastRecord(string csv)
        {
            // last record is garbage, chop it off
            var lastIndex = csv.LastIndexOf("\r\n");
            csv = csv.Substring(0, lastIndex - 1);
            
            var csvOfRecords = new CsvHelper.CsvReader(new StringReader(csv));

            csvOfRecords.Configuration.IgnoreQuotes = true;
            csvOfRecords.Configuration.HasHeaderRecord = false;
            csvOfRecords.Configuration.MissingFieldFound = null;
            csvOfRecords.Read();

            return csvOfRecords.GetRecords<HabHubPredictionPoint>().LastOrDefault();

        }

        private static PredictionMessage generatePredictionMessage(HabHubPredictionPoint landingRecord)
        {
            
            var predictionMessage = new PredictionMessage();
            predictionMessage.PredictionDate = DateTime.UtcNow;
            predictionMessage.LandingDateTime = UnixTimeToDateTime(landingRecord.Timestamp);
            predictionMessage.LandingLat = landingRecord.Latitude;
            predictionMessage.LandingLong = landingRecord.Longitude;
            predictionMessage.LandingAltitude = landingRecord.Altitude;

            return predictionMessage;
        }





    }
}