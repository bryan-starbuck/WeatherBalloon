using System;
using Microsoft.Azure.Devices.Client;
using System.Threading.Tasks;

namespace WeatherBalloon.Common
{

    public class WrappedModuleClient : IModuleClient
    {
        private ModuleClient moduleClient;


        public WrappedModuleClient(ModuleClient client)
        {
            moduleClient = client;
        }

        public async Task SendEventAsync(string outputName, Message message)
        {
            if (moduleClient != null)
            {
                await moduleClient.SendEventAsync(outputName, message);
            }
            else 
            {
                throw new InvalidOperationException("Empty module client.");
            }               
        }
    }
}