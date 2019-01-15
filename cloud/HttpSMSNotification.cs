/* using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

using WeatherBalloon.Messaging;

namespace WeatherBalloon.Cloud
{
    public static class HttpSMSNotification
    {
        private static string accountSid = Environment.GetEnvironmentVariable("TwilioAccountSID");
        private static string authToken = Environment.GetEnvironmentVariable("TwilioAuthToken");

        private static string fromPhoneNumber = Environment.GetEnvironmentVariable("FromPhoneNumber");

         private static string toPhoneNumber = Environment.GetEnvironmentVariable("ToPhoneNumber");

        /// <summary>
        /// Azure Function that receives a Prediction Message via HTTP and transmits
        /// a google maps link via SMS message
        /// </summary>
        /// <param name="[HttpTrigger(AuthorizationLevel.Anonymous"></param>  TODO - change authentication
        /// <param name="null"></param>
        /// <returns></returns>
        [FunctionName("HttpSMSNotification")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,  
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            TwilioClient.Init(accountSid, authToken);


            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            try
            {
                var predictionMessage = JsonConvert.DeserializeObject<PredictionMessage>(requestBody);

                if (predictionMessage == null)
                {
                    log.LogInformation("Empty Prediction message received.");
                    return new BadRequestObjectResult("Empty prediction, no action to take.");
                }

                var smsMessage = FormatSMS(predictionMessage);
                if (string.IsNullOrEmpty(smsMessage))
                {
                    try 
                    {
                        TransmitSMS(smsMessage, fromPhoneNumber, toPhoneNumber);
                        log.LogInformation("Completed sending SMS.");
                    }
                    catch (Exception ex)
                    {
                        log.LogInformation($"Error sending SMS: {ex.Message}");
                    }
                }

            }
            catch (Exception ex)
            {
                log.LogInformation("Bad request, could not parse prediction");
                return new BadRequestObjectResult($"Invalid Prediction message.  Could not parse: {ex.Message}" );
            }

            return (ActionResult)new OkObjectResult("Complete");
        }

        [FunctionName("HttpTwilio")]
        [return: TwilioSms(AccountSidSetting = "TwilioAccountSID", AuthTokenSetting = "TwilioAuthToken", From = "+14802970163" )]
        public static SMSMessage Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation($"C# http trigger function processed.");

            string requestBody = new StreamReader(req.Body).ReadToEnd();

            try
            {
                var predictionMessage = JsonConvert.DeserializeObject<PredictionMessage>(requestBody);

                if (predictionMessage == null)
                {
                    log.LogInformation("Empty Prediction message received.");
                    return new BadRequestObjectResult("Empty prediction, no action to take.");
                }

                var smsMessage = FormatSMS(predictionMessage);
                if (string.IsNullOrEmpty(smsMessage))
                {
                   var message = new Twilio.()
            {
                Body = $"Hello {order["name"]}, thanks for your order!",
                To = order["mobileNumber"].ToString()
            };
                }

            }
            catch (Exception ex)
            {
                log.LogInformation("Bad request, could not parse prediction");
                return new BadRequestObjectResult($"Invalid Prediction message.  Could not parse: {ex.Message}" );
            }


            

            return message;
        }

        /// <summary>
        /// Create a text body from a prediction message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string FormatSMS(PredictionMessage message)
        {
            // google url docs: https://developers.google.com/maps/documentation/urls/guide
            return $"Balloon Landing: {message.PredictionDate} \n https://www.google.com/maps/search/?api=1&query={message.LandingLat},{message.LandingLong}&query_place_id=PredictedLanding";
        }

        /// <summary>
        /// Transmit text message to Phone number
        /// </summary>
        /// <param name="contents"></param>
        /// <param name="fromPhoneNumber"></param>
        /// <param name="toPhoneNumber"></param>
        public static void TransmitSMS(string contents, string fromPhoneNumber, string toPhoneNumber)
        {
            var message = MessageResource.Create(
                body: contents,
                from: new Twilio.Types.PhoneNumber(fromPhoneNumber),
                to: new Twilio.Types.PhoneNumber(toPhoneNumber)
            );       
        }

    }
}
 */