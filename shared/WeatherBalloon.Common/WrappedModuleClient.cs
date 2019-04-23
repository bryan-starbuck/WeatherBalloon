using System;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Client.Transport.Mqtt;

namespace WeatherBalloon.Common
{

    public class WrappedModuleClient : IModuleClient
    {
        public ModuleClient moduleClient;


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

        public static async Task<WrappedModuleClient> Create()
        {
            MqttTransportSettings mqttSetting = new MqttTransportSettings(TransportType.Mqtt_Tcp_Only);
            ITransportSettings[] settings = { mqttSetting };

            // Open a connection to the Edge runtime
            ModuleClient ioTHubModuleClient = await ModuleClient.CreateFromEnvironmentAsync(settings);
            await ioTHubModuleClient.OpenAsync();

            return new WrappedModuleClient(ioTHubModuleClient);
        }
    }
}