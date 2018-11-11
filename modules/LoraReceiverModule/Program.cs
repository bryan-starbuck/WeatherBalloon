using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;

using RFMLib;
    
namespace LoraReceiverModule
{
    
    class Program
    {

        private static ITransceiverSpiConnection _spiConnection;
        private static RFM9XLoraTransceiver _rfm9XLoraTransceiver;

        private static ModuleClient _ioTHubModuleClient;

        private static System.Threading.Timer _heartbeatTimer;

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
            _ioTHubModuleClient = await ModuleClient.CreateFromEnvironmentAsync(settings);
            await _ioTHubModuleClient.OpenAsync();
            Console.WriteLine("IoT Hub module client initialized.");

            // Register callback to be called when a message is received by the module
            await _ioTHubModuleClient.SetInputMessageHandlerAsync("transmitterInput", TransmitIoTMessageWitLora, _ioTHubModuleClient);

            InitializeReceiver();

            Console.WriteLine("Starting Heartbeat timer");
            _heartbeatTimer = new Timer(new TimerCallback(HeartbeatCallback), null, new TimeSpan(0, 0, 0), new TimeSpan(0,1,0));
        }

        public static void HeartbeatCallback(object state)
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes("Lora Heartbeat message: " + DateTime.UtcNow.ToString());

            Console.WriteLine("Sending Lora Heartbeat");
            Task.Run( () => TransmitLoraMessage(messageBytes) );
        }

        static async Task<MessageResponse> TransmitIoTMessageWitLora(Message message, object userContext)
        {
            var moduleClient = userContext as ModuleClient;
            if (moduleClient == null)
            {
                throw new InvalidOperationException("UserContext doesn't contain expected values");
            }

            byte[] messageBytes = message.GetBytes();
            string messageString = Encoding.UTF8.GetString(messageBytes);
            Console.WriteLine($"Received message: Body: [{messageString}]");

            await TransmitLoraMessage(messageBytes);

            return MessageResponse.Completed;
        }

        private static void InitializeReceiver()
        {
            try 
            {
                TrancieverConnectionFactory trancieverConnectionFactory = new TrancieverConnectionFactory();

                _spiConnection = trancieverConnectionFactory.CreateForDragino();
                _rfm9XLoraTransceiver = new RFM9XLoraTransceiver(_spiConnection);

                _rfm9XLoraTransceiver.Initialize();

                Task.Run(() => ReceiveLoraMessages());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to initialize Lora Receiver: " + ex.Message);
            }
        }

        private static void ReceiveLoraMessages()
        {
            while (true)
            {
                Task<RawData> receiveTask = _rfm9XLoraTransceiver.Recieve();

                receiveTask.Wait();

                if (receiveTask.Result != null)
                {
                    Console.WriteLine("Received Lora Message: " + Encoding.ASCII.GetString(receiveTask.Result.Buffer));

                    Task.Run(() => SendIoTMessage(receiveTask.Result.Buffer));
                }
            }
        }

        private static async Task<bool> TransmitLoraMessage(Byte [] messageBytes)
        {
            try 
            {

                var success = await _rfm9XLoraTransceiver.Transmit(messageBytes);
                
                if (!success)
                {
                    Console.WriteLine("Transmit Failed");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to Transmit Lora Message: " + ex.Message);
                return false;
            }

            Console.WriteLine("Transmit Success");
            return true;
        }

        private static async Task<bool> SendIoTMessage(Byte[] messageBytes)
        {
            try 
            {
                if (_ioTHubModuleClient != null)
                {
                    var iotMessage = new Message(messageBytes);

                    await _ioTHubModuleClient.SendEventAsync("receiverOutput", iotMessage);

                    Console.WriteLine("IoT message sent");
                }
            }
            catch ( Exception ex )
            {
                Console.WriteLine("Failed to send iot message: "+ ex.Message);
                return false;
            }

            return true;
        }
    }
}
