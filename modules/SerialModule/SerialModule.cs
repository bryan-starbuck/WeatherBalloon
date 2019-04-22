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

        public SerialModule(WrappedModuleClient moduleClient)
        {
            this.moduleClient = moduleClient;
        }

        public void Initialize(string port)
        {
            if (serialPort != null && serialPort.IsOpen())
            {
                serialPort.RtsEnable = false;
                serialPort.Close();
            }

            serialPort = new SerialPort(port, 115200);
            serialPort.RtsEnable = true;

            serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceived);
            
            serialPort.Open();
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

        private void DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            string receivedData = sp.ReadExisting();

            Logger.LogInfo("SerialData : "+receivedData);

            if (!String.IsNullOrEmpty(receivedData))
            {
                try 
                {
                    var balloonMessage = BalloonMessage.FromCompactMessage(receivedData);
                    SendBalloonMessage(balloonMessage);
                }
                catch (Exception ex)
                {
                    Logger.LogError("Failed to process serial message: "+ ex.Message);
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
                Logger.LogError($"Failed to transmit iot message. Exception: {ex.Message}");
                return false;
            }

            return true;
        }
    }
}

