using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;

namespace WeatherBalloon.Common
{

    public class FakeModuleClient : IModuleClient
    {
        public List<Tuple<string, Message>> SentMessages = new List<Tuple<string, Message>>();

        public async Task SendEventAsync(string outputName, Message message)
        {
            SentMessages.Add(new Tuple<string, Message>(outputName, message));
        }
    }

    

}