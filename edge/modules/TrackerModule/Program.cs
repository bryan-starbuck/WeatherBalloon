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
using Microsoft.Azure.Devices.Shared; // For TwinCollection
using Newtonsoft.Json;

using WeatherBalloon.Common;
using WeatherBalloon.Messaging;

namespace WeatherBalloon.TrackerModule
{
    class Program
    {
        private static TrackerModule trackerModule = new TrackerModule();
        private static WrappedModuleClient wrappedModuleClient;

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
            // Use Mqtt as it is more reliable than ampq 
            MqttTransportSettings mqttSetting = new MqttTransportSettings(TransportType.Mqtt_Tcp_Only);
            ITransportSettings[] settings = { mqttSetting };

            // Open a connection to the Edge runtime
            ModuleClient ioTHubModuleClient = await ModuleClient.CreateFromEnvironmentAsync(settings);
            await ioTHubModuleClient.OpenAsync();
            wrappedModuleClient = new WrappedModuleClient(ioTHubModuleClient);

            Logger.LogInfo("IoT Hub module client initialized.");


            // get module twin settings
            var moduleTwin = await ioTHubModuleClient.GetTwinAsync();
            await OnDesiredPropertiesUpdate(moduleTwin.Properties.Desired, ioTHubModuleClient);

            // Attach a callback for updates to the module twin's desired properties.
            await ioTHubModuleClient.SetDesiredPropertyUpdateCallbackAsync(OnDesiredPropertiesUpdate, null);

            // Register callback to be called when a message is received by the module
            await ioTHubModuleClient.SetInputMessageHandlerAsync(TrackerModule.TelemetryInputName, ProcessTelemetry, ioTHubModuleClient);
            // Register callback to be called when a message is received by the module
            await ioTHubModuleClient.SetInputMessageHandlerAsync(TrackerModule.BalloonInputName, ProcessBalloonData, ioTHubModuleClient);

            
        }

        /// <summary>
        /// Process input message from the Telemetry input message source.
        /// </summary>
        static async Task<MessageResponse> ProcessTelemetry(Message message, object userContext)
        {
            await Task.Run( () => 
            {
                try 
                {
                    Logger.LogInfo("Telemetry Message Received.");

                    var telemetryMessage = MessageHelper.ParseMessage<TelemetryMessage>(message);
                    trackerModule.Receive(telemetryMessage);
                    Logger.LogInfo($"Telemetry Message Processed. {telemetryMessage.@long},{telemetryMessage.lat}");

                    
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex);
                }
            });

            return MessageResponse.Completed;
        }

        /// <summary>
        /// Process input message from the Balloon Data input message source.
        /// </summary>
        static async Task<MessageResponse> ProcessBalloonData(Message message, object userContext)
        {
            //ModuleClient moduleClient = userContext as ModuleClient;
            //if (moduleClient == null)
            //{
            //    throw new ArgumentException("Invalid user context in ProcessBalloonData");
            //}

            //WrappedModuleClient wrappedModuleClient = new WrappedModuleClient(moduleClient);    

            if (wrappedModuleClient == null)
            {
                throw new Exception("module client not initialized in ProcessBalloonData.");
            }

            try 
            {
                var balloonMessage = MessageHelper.ParseMessage<BalloonMessage>(message);
                await trackerModule.Receive(balloonMessage, wrappedModuleClient);
                Logger.LogInfo("Balloon Message Processed.");
            }
            catch (Exception ex)
            {
                byte[] messageBytes = message.GetBytes();
                string messageString = Encoding.UTF8.GetString(messageBytes);

                Logger.LogWarning("Invalid balloon message: "+ messageString);
                Logger.LogException(ex);
            }
            
            return MessageResponse.Completed;
        }

        static Task OnDesiredPropertiesUpdate(TwinCollection desiredProperties, object userContext)
        {
            try
            {
                Console.WriteLine("Desired property change:");
                Console.WriteLine(JsonConvert.SerializeObject(desiredProperties));

                if (desiredProperties["DeviceName"]!=null)
                    trackerModule.DeviceName = desiredProperties["DeviceName"].ToString();

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
                Console.WriteLine();
                Console.WriteLine("Error when receiving desired property: {0}", ex.Message);
            }
            return Task.CompletedTask;
        }
    }
}
