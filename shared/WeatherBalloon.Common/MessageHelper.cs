using System;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System.Text;

namespace WeatherBalloon.Common
{

    public class MessageHelper
    {
        public static T ParseMessage<T>(Message message)
        {
            byte[] messageBytes = message.GetBytes();
            string messageString = Encoding.UTF8.GetString(messageBytes);

            return JsonConvert.DeserializeObject<T>(messageString);
        }
    }

}