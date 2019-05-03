using System;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;

using WeatherBalloon.Messaging;
using WeatherBalloon.Common;

namespace WeatherBalloon.SerialModule
{
    /// <summary>
    /// IoT Module that reads and writes to the serial port
    /// </summary>
    public class SerialModule
    {
        /// <summary>
        /// IoT Edge Input name for balloon data 
        /// </summary>
        public const string BalloonInputName = "balloonInput";

        /// <summary>
        /// IoT Edge Output name
        /// </summary>
        public const string SerialOutputName = "serialOutput";

        private object transmitLock = new object();

        private static SerialPort serialPort;

        private const int MaxMessageSize = 136;

        private WrappedModuleClient moduleClient;

        public SerialModule()
        {
            
        }

        public async Task<bool> Initialize()
        {
            moduleClient = await WrappedModuleClient.Create();

            var acm0 = OpenSerialPort("/dev/ttyACM0");
            if (acm0 != null)
            {
                Logger.LogInfo("Successfully opened /dev/ACM0");
                serialPort = acm0;
            }
            else
            {
                Logger.LogInfo("ACM0 not available, opening serial port ACM1");
                var acm1 = OpenSerialPort("/dev/ttyACM1");
                if (acm1 != null)
                {
                    Logger.LogInfo("Successfully opened /dev/ACM1");
                    serialPort = acm1;
                }
            }

            return true;
        }

        public SerialPort OpenSerialPort(string port)
        {
            try 
            {
                var newSerialPort = new SerialPort(port, 115200);
                newSerialPort.RtsEnable = true;

                newSerialPort.DataReceived += new SerialDataReceivedEventHandler(SerialDataReceived);
                
                newSerialPort.Open();
                return newSerialPort;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public void OnReceive(BalloonMessage message)
        {
            var compactMessage = message.ToCompactMessage();

            if (compactMessage.Length > MaxMessageSize)
            {
                Logger.LogWarning("Max message size exceeded: "+compactMessage);
            }

            lock (transmitLock)
            {
                serialPort.WriteLine(compactMessage);
            }
        }

        private void SerialDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            //string receivedData = sp.ReadExisting();
            string receivedData = sp.ReadLine();

            Logger.LogInfo("SerialData : "+receivedData);

            if (receivedData.StartsWith('-'))
            {
                // ignore, debug messages.
                return;
            }

            if (!String.IsNullOrEmpty(receivedData))
            {
                try 
                {
                    var balloonMessage = BalloonMessage.FromCompactMessage(receivedData);
                    SendBalloonMessage(balloonMessage).Wait();
                }
                catch (Exception ex)
                {
                    Logger.LogInfo("Bad serial message: "+receivedData);
                    Logger.LogException(ex);
                }
            }
        }

    
        private async Task<bool> SendBalloonMessage(BalloonMessage balloonMessage)
        {
            Message message = new Message(balloonMessage.ToRawBytes());

            try 
            {
                await moduleClient.SendEventAsync(SerialOutputName, message);
        
                Logger.LogInfo($"transmitted message: {JsonConvert.SerializeObject(balloonMessage)}.");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to transmit iot balloon message.");
                Logger.LogException(ex);
                return false;
            }

            return true;
        }
    }
}

