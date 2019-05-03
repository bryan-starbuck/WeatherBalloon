using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Client.Transport.Mqtt;
using System.Collections.Generic;     
using Microsoft.Azure.Devices.Shared; // For TwinCollection
using Newtonsoft.Json;

using WeatherBalloon.Common;
using WeatherBalloon.Messaging;

namespace WeatherBalloon.SerialModule
{
    class Program
    {
        private static SerialModule serialModule;
        private static string comPort;

        static void Main(string[] args)
        {
            Init().Wait();

            // Wait until the app unloads or is cancelled
            var cts = new CancellationTokenSource();
            AssemblyLoadContext.Default.Unloading += (ctx) => cts.Cancel();
            Console.CancelKeyPress += (sender, cpe) => cts.Cancel();
            WhenCancelled(cts.Token).Wait();
        }

        /// <summary>
        /// Handles cleanup operations when app is cancelled or unloads
        /// </summary>
        public static Task WhenCancelled(CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).SetResult(true), tcs);
            return tcs.Task;
        }

        /// <summary>
        /// Initializes the ModuleClient and sets up the callback to receive
        /// messages containing temperature information
        /// </summary>
        static async Task Init()
        {
            MqttTransportSettings mqttSetting = new MqttTransportSettings(TransportType.Mqtt_Tcp_Only);
            ITransportSettings[] settings = { mqttSetting };

            // Open a connection to the Edge runtime
            ModuleClient ioTHubModuleClient = await ModuleClient.CreateFromEnvironmentAsync(settings);
            await ioTHubModuleClient.OpenAsync();
            Console.WriteLine("IoT Hub module client initialized.");

            // Read the COM Port value from the module twin's desired properties
            // var moduleTwin = await ioTHubModuleClient.GetTwinAsync();
            // await OnDesiredPropertiesUpdate(moduleTwin.Properties.Desired, ioTHubModuleClient);

            // Attach a callback for updates to the module twin's desired properties.
            //await ioTHubModuleClient.SetDesiredPropertyUpdateCallbackAsync(OnDesiredPropertiesUpdate, null);

            serialModule = new SerialModule();
            await serialModule.Initialize();

            // Register callback to be called when a message is received by the module
            await ioTHubModuleClient.SetInputMessageHandlerAsync(SerialModule.BalloonInputName, ProcessBalloonMessage, ioTHubModuleClient);
        }

        private static Task OnDesiredPropertiesUpdate(TwinCollection desiredProperties, object userContext)
        {
            try
            {
                Console.WriteLine("Desired property change:");
                Console.WriteLine(JsonConvert.SerializeObject(desiredProperties));

                if (desiredProperties["ComPort"] != null)
                {
                    comPort = desiredProperties["ComPort"].ToString();

                    Logger.LogInfo("Comport set as ComPort: " + comPort);
                }
            }
            catch (AggregateException ex)
            {
                foreach (Exception exception in ex.InnerExceptions)
                {
                    Console.WriteLine();
                    Console.WriteLine("Error when receiving desired property: {0}", exception);
                }
            }
            catch (Exception ex)
            {
                Logger.LogInfo("Error when receiving desired property");
                Logger.LogException(ex);
            }
            return Task.CompletedTask;
        }

         /// <summary>
        /// Process input message from the Telemetry input message source.
        /// </summary>
        static async Task<MessageResponse> ProcessBalloonMessage(Message message, object userContext)
        {
            await Task.Run( () => 
            {
                try 
                {
                    var balloonMessage = MessageHelper.ParseMessage<BalloonMessage>(message);
                    serialModule.OnReceive(balloonMessage);
                    Logger.LogInfo("Balloon Message Processed.");
                }
                catch (Exception ex)
                {
                    byte[] messageBytes = message.GetBytes();
                    string messageString = Encoding.UTF8.GetString(messageBytes);

                    Logger.LogWarning("Invalid balloon message: "+messageString);
                    Logger.LogException(ex);
                }
            });

            return MessageResponse.Completed;
        }
    }
}
