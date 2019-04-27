using System;
using System.IO;
using System.Text;
using Newtonsoft.Json.Linq;


using Microsoft.Azure.Devices.Client;


namespace WeatherBalloon.StorageModule
{

    public class StorageModule
    {
        public const string StorageInput = "StorageInput";

        public StorageModule()
        {


        }

        public void Write(Message message)
        {
            string filename = "messages.txt";

            byte[] messageBytes = message.GetBytes();
            string messageString = Encoding.UTF8.GetString(messageBytes);

            JObject jsonObject = JObject.Parse(messageString);

            if (jsonObject.ContainsKey("FlightId"))
            {
                filename = $"{((string)jsonObject["FlightId"]).Replace(' ', '-')}.txt";
            }

            try 
            {
                using (StreamWriter writer = File.AppendText("/balloon_data/"+filename))
                {
                    writer.WriteLine(messageString);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to write output. " + ex.Message );
            }
        }


    }
}