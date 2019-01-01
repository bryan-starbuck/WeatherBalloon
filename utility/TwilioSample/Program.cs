using System;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace TwilioSample
{
    class Program
    {
        static void Main(string[] args)
        {

            // Find your Account Sid and Token at twilio.com/console
            const string accountSid = "AC3cc1e3e385dc5c0ca8564aa6682a2c---";
            const string authToken = "0fd4dd6188679aefee52501b7f664c------";

            TwilioClient.Init(accountSid, authToken);

            var body = $"Balloon Landing: {DateTime.Now} \n https://www.google.com/maps/search/?api=1&query=33.36992,-111.925381667&query_place_id=PredictedLanding";

            var message = MessageResource.Create(
                body: body,
                from: new Twilio.Types.PhoneNumber("+14802970163"),
                to: new Twilio.Types.PhoneNumber  ("+14806779336")
            );



            Console.WriteLine(message.Sid);
        }

    }
}
