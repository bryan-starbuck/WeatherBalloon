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
using Microsoft.Azure.Devices.Client.Transport.Mqtt;

using WeatherBalloon.Common;
using WeatherBalloon.Messaging;

namespace WeatherBalloon.BalloonModule
{
    public class Program
    {
        private static BalloonModule balloonModule = new BalloonModule();
        private static Timer transmitTimer;
        private const int transmitInterval = 60000;

        private static WrappedModuleClient moduleClient;

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
            try 
            {
                moduleClient = await WrappedModuleClient.Create();
            }
            catch (Exception ex)
            {
                Logger.LogFatalError("Failed to create module client.");
                Logger.LogException(ex);
            
                // Throw to make module shutdown and get restarted by the iot edge agent.
                throw ex;
            }

            Logger.LogInfo("Created and connected Module Client. ");

            // Register callback to be called when a message is received by the module
            await moduleClient.moduleClient.SetInputMessageHandlerAsync(BalloonModule.TelemetryInputName, ProcessTelemetry, moduleClient.moduleClient);

            transmitTimer = new Timer(TransmitTimerCallback, null,  transmitInterval, transmitInterval);
        }

        /// <summary>
        /// Process input message from the Telemetry input message source.
        /// </summary>
        private static async Task<MessageResponse> ProcessTelemetry(Message message, object userContext)
        {
            await Task.Run( () => 
            {
                try 
                {
                    var telemetryMessage = MessageHelper.ParseMessage<TelemetryMessage>(message);

                    balloonModule.Receive(telemetryMessage);
                    Logger.LogInfo("Telemetry Message Processed.");
                }
                catch (Exception ex)
                {
                    byte[] messageBytes = message.GetBytes();
                    string messageString = Encoding.UTF8.GetString(messageBytes);

                    Logger.LogWarning("Invalid Telemetry message: "+messageString);
                    Logger.LogException(ex);
                }
            });

            return MessageResponse.Completed;
        }

        private static void TransmitTimerCallback(object state)
        {
            if ( moduleClient == null)
            {
                Logger.LogFatalError("Invalid Module client in Transmit Timer.");
                return;
            }

            balloonModule.TransmitBalloonMessage(moduleClient).Wait();
        }
    }
}
