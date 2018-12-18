using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;

using WeatherBalloon.Common;
using WeatherBalloon.Messaging;

namespace WeatherBalloon.BalloonModule
{
    public class Program
    {
        private static BalloonModule _balloonModule = new BalloonModule();
        private static Timer _transmitTimer;
        private const int _transmitInterval = 60000;

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
            AmqpTransportSettings amqpSetting = new AmqpTransportSettings(TransportType.Amqp_Tcp_Only);
            ITransportSettings[] settings = { amqpSetting };

            // Open a connection to the Edge runtime
            ModuleClient ioTHubModuleClient = await ModuleClient.CreateFromEnvironmentAsync(settings);
            await ioTHubModuleClient.OpenAsync();
            Console.WriteLine("IoT Hub module client initialized.");

            // Register callback to be called when a message is received by the module
            await ioTHubModuleClient.SetInputMessageHandlerAsync("telemetry", ProcessTelemetry, ioTHubModuleClient);

            _transmitTimer = new Timer(TransmitTimerCallback, new WrappedModuleClient(ioTHubModuleClient),  _transmitInterval, _transmitInterval);
        }

        /// <summary>
        /// This method is called whenever the module is sent a message from the EdgeHub. 
        /// It just pipe the messages without any change.
        /// It prints all the incoming messages.
        /// </summary>
        static async Task<MessageResponse> ProcessTelemetry(Message message, object userContext)
        {
            await Task.Run( () => 
            {
                byte[] messageBytes = message.GetBytes();
                string messageString = Encoding.UTF8.GetString(messageBytes);

                try 
                {
                    var gpsMessage = JsonConvert.DeserializeObject<GPSMessage>(messageString);
                    _balloonModule.Receive(gpsMessage);
                    Logger.LogInfo("GPS Message Processed.");
                }
                catch (Exception ex)
                {
                    Logger.LogWarning("Invalid balloon message: "+ ex.Message);
                }
            });

            return MessageResponse.Completed;
        }

        private static void TransmitTimerCallback(object state)
        {
            var wrappedModuleClient = state as WrappedModuleClient;
            if ( wrappedModuleClient == null)
            {
                Logger.LogFatalError("Invalid Module client in Transmit Timer.");
                return;
            }

            _balloonModule.Transmit(wrappedModuleClient);
        }
    }
}
