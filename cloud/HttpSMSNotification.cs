using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

using WeatherBalloon.Messaging;

namespace WeatherBalloon.Cloud
{
    public static class HttpSMSNotification
    {
        [FunctionName("HttpSMSNotification")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["prediction"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            try
            {
                var predictionMessage = JsonConvert.DeserializeObject<PredictionMessage>(requestBody);

                if (predictionMessage == null)
                {
                    log.LogInformation("Empty Prediction message received.");
                    return BadRequestObjectResult("Empty prediction, no action to take.");
                }

                var smsMessage = FormatSMS(predictionMessage);
                if (string.IsNullOrEmpty(smsMessage))
                {
                    var result = TransmitSMS(smsMessage);

                    if (!result)
                    {
                        log.LogInformation("Error Transmitting SMS.");
                    }
                }

            }
            catch (Exception ex)
            {
                return BadRequestObjectResult($"Invalid Prediction message.  Could not parse: {ex.Message}" );
            }

            return (ActionResult)new OkObjectResult("Complete");
        }

        public static string FormatSMS(PredictionMessage message)
        {

            return "";
        }

        public static bool TransmitSMS(string contents)
        {


            return false;                
        }

    }
}
