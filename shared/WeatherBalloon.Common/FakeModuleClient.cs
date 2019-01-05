using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;

namespace WeatherBalloon.Common
{

    /// <summary>
    /// Fake module client to faciliate automated testing
    /// </summary>
    public class FakeModuleClient : IModuleClient
    {
        /// <summary>
        /// History of messages sent with this faked object
        /// </summary>
        /// <returns></returns>
        public List<Tuple<string, Message>> SentMessages = new List<Tuple<string, Message>>();

        public async Task SendEventAsync(string outputName, Message message)
        {
            SentMessages.Add(new Tuple<string, Message>(outputName, message));
        }
    }
}