using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;

namespace WeatherBalloon.LoraModule
{    
    
    public class Program
    {
        public static bool isTracker = false; 
        public static Timer receiveTimer;

        private const int LoraRecieverTime = 1000;

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
            await ioTHubModuleClient.SetInputMessageHandlerAsync("input", PipeMessage, ioTHubModuleClient);

            var moduleTwin = await ioTHubModuleClient.GetTwinAsync();
            var moduleTwinCollection = moduleTwin.Properties.Desired;
            try {
                string mode = moduleTwinCollection["Mode"];

                if ( mode == "tracker")
                {
                    isTracker = true;
                    Console.WriteLine("Mode is Tracker");
                }
                else
                {
                    Console.WriteLine("Mode is Balloon");
                }
            } 
            catch(ArgumentOutOfRangeException e) 
            {
                Console.WriteLine($"Property Mode not exist: {e.Message}"); 
            }

            if (!isTracker)
            {
                try 
                {
                    RF95_Client.Transmit("0 Lora Transmitter started.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed RF95 Init: "+ ex.Message);
                }
            }
            else 
            {
                Console.WriteLine("Starting Receiver timer");
                receiveTimer = new Timer( new TimerCallback(ReceiveTimerCallback), ioTHubModuleClient, LoraRecieverTime, 0 );

            }
        }

        private static void ReceiveTimerCallback ( object parameter)
        {
            try 
            {
                var ioTHubModuleClient = parameter as ModuleClient;

                receiveTimer.Change(Timeout.Infinite, Timeout.Infinite);

                var messageString = RF95_Client.Receive();

                if (!string.IsNullOrEmpty(messageString))
                {
                    Console.WriteLine("Lora message received: "+messageString);

                    var iotMessage = new Message(Encoding.UTF8.GetBytes(messageString));

                    ioTHubModuleClient.SendEventAsync("loraOutput", iotMessage);

                    Console.WriteLine("IoT message sent");
                }

                // todo - this probably doesn't need be a timer
                receiveTimer.Change(LoraRecieverTime, 0);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in receive timer: " + ex.Message);
            }
        }

        /// <summary>
        /// This method is called whenever the module is sent a message from the EdgeHub. 
        /// It just pipe the messages without any change.
        /// It prints all the incoming messages.
        /// </summary>
        static async Task<MessageResponse> PipeMessage(Message message, object userContext)
        {
            var moduleClient = userContext as ModuleClient;
            if (moduleClient == null)
            {
                throw new InvalidOperationException("UserContext doesn't contain " + "expected values");
            }

            byte[] messageBytes = message.GetBytes();
            string messageString = Encoding.UTF8.GetString(messageBytes);
            Console.WriteLine($"Received message. Body: [{messageString}]");

            if (!string.IsNullOrEmpty(messageString))
            {
                try 
                {
                    await Task.Run( () =>  {
                        RF95_Client.Transmit(messageString);
                        Console.WriteLine("Transmit complete.");
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed Transmit: " + ex.Message);
                }
            }
            return MessageResponse.Completed;
        }
    }
}
