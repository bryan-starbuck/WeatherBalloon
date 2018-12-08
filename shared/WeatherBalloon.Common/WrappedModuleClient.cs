using System;
using Microsoft.Azure.Devices.Client;

namespace WeatherBalloon.Common
{

    public class WrappedModuleClient : IModuleClient
    {
        private ModuleClient moduleClient;


        public WrappedModuleClient(ModuleClient client)
        {
            moduleClient = client;
        }
    }
}