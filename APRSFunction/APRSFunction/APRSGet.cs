using System;
using System.Net.Http;
using System.Text;
using APRSFunction.Entities;
using Microsoft.Azure.Devices;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace APRSFunction
{
    public static class Function1
    {
        private const string iotHubConnectionString = "HostName=starbuckhub.azure-devices.net;SharedAccessKeyName=SmartMeter;SharedAccessKey=a302IYQjaTnAyUdf8IzrG1U0//LnRzAXG76t+CjyPG4=";
        private const string deviceId = "aprs";

        [FunctionName("APRSGet")]
        public static void Run([TimerTrigger("0 */5 * * * *", RunOnStartup = true)]TimerInfo myTimer, TraceWriter log)
        {
            log.Info($"C# Timer trigger function executed at: {DateTime.Now}");

            APRSApi aprs = new APRSApi() { BaseAddress = "https://api.aprs.fi", ApiKey = "113013.5H8d4YSdqCUWi05", CallSign = "NU0Z" };

            var task = aprs.GetLatest();

            task.Wait();

            var aprsResponse = task.Result;

            log.Info("Got record: " + JsonConvert.SerializeObject(aprsResponse));

            var iotMessages = IoTMessage.CreateMessages(aprsResponse);


            try
            {
                using (var serviceClient = ServiceClient.CreateFromConnectionString(iotHubConnectionString))
                {
                    
                    foreach (var iotMessage in iotMessages)
                    {
                        var messageText = JsonConvert.SerializeObject(iotMessage);

                        var encodedMessage = new Message(Encoding.ASCII.GetBytes(messageText));

                        serviceClient.SendAsync(deviceId, encodedMessage).Wait();
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error calling iotHub: " + ex.Message);

            }

        }



    }
}
