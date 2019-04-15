using System;
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
using Twilio;
using WeatherBalloon.Cloud.Twilio;
using System.Text.RegularExpressions;
using WeatherBalloon.Cloud.Documents;

namespace WeatherBalloon.Cloud
{
    public static class HttpToSMSNotification
    {
        private static string accountSid = Environment.GetEnvironmentVariable("TwilioAccountSID");
        private static string authToken = Environment.GetEnvironmentVariable("TwilioAuthToken");

        private static string fromPhoneNumber = Environment.GetEnvironmentVariable("FromPhoneNumber");

         private static string toPhoneNumber = Environment.GetEnvironmentVariable("ToPhoneNumber");

        [FunctionName("HttpToSMSNotification")]
        [return: TwilioSms(AccountSidSetting = "TwilioAccountSid", AuthTokenSetting = "TwilioAuthToken")]
        public static CreateMessageOptions Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] 
        HttpRequest req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            try 
            {
                string requestBody = new StreamReader(req.Body).ReadToEndAsync().Result;

                // the string is coming through with extra escape characters in the string.
                string fixedBody = Regex.Unescape(requestBody);
                fixedBody = fixedBody.Substring(1);
                fixedBody = fixedBody.Substring(0, fixedBody.Length -1);

                log.LogInformation($"Message Body: {fixedBody}");

                var predictionNotification = JsonConvert.DeserializeObject<PredictionNotification>(fixedBody);

                var message = new CreateMessageOptions(toPhoneNumber)
                {
                    Body = SMSHelper.FormatSMS(predictionNotification),
                    From = fromPhoneNumber,
                };

                return message;
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error sending SMS Message.");
            }

            return null;
        }
    }
}