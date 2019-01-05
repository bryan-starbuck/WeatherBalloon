using System;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Newtonsoft.Json;

using WeatherBalloon.Messaging;

namespace TwilioSample
{
    class Program
    {
        static void Main(string[] args)
        {
            var predictionMessage = CreatePredictionMessage();
            var text = JsonConvert.SerializeObject(predictionMessage);
            Console.WriteLine(text);


            // Find your Account Sid and Token at twilio.com/console
            const string accountSid = "-------------------";
            const string authToken = "-----------";

            TwilioClient.Init(accountSid, authToken);

            var body = $"Balloon Landing: {DateTime.Now} \n https://www.google.com/maps/search/?api=1&query=33.36992,-111.925381667&query_place_id=PredictedLanding";

            var message = MessageResource.Create(
                body: body,
                from: new Twilio.Types.PhoneNumber("------"),
                to: new Twilio.Types.PhoneNumber  ("------")
            );

            Console.WriteLine(message.Sid);
        }

        private static PredictionMessage CreatePredictionMessage()
        {
            var message = new PredictionMessage()
            {
                PredictionDate = DateTime.Now, 
                LandingLat = 33.36992F,
                LandingLong = -111.925F
            };

            return message;
        }

    }
}
