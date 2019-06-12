using System;
using Microsoft.Azure.Devices.Client;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

using WeatherBalloon.Messaging;
using WeatherBalloon.Common;
using System.Collections.Generic;

namespace WeatherBalloon.TrackerModule
{
    /// <summary>
    /// A GPS device that is tracking the weather balloon
    /// </summary>
    public class TrackerModule 
    {
        public string DeviceName { get;set; }

        /// <summary>
        /// IoT Edge Input name for telemetry  
        /// </summary>
        public const string TelemetryInputName = "telemetryInput";
        /// <summary>
        /// IoT Edge Input name for balloon data 
        /// </summary>
        public const string BalloonInputName = "balloonInput";

        /// <summary>
        /// IoT Edge Output name
        /// </summary>
        public const string TrackerOutputName = "trackerOutput";
 

        /// <summary>
        /// Current Location of the tracker
        /// </summary>
        public GPSLocation Location = new GPSLocation();

        private object lockingUpdateObject = new object();

        // history of the last five minutes of balloon message receipts
        private Queue<bool> packetReceiveHistory = new Queue<bool>();
        
        private Timer packetReceiptTimer;
        private const int packetReceiptInterval = 70000;

        private string FlightId;
        
        public TrackerModule ()
        {
            DeviceName = "Unknown Tracker";

            packetReceiptTimer = new Timer(ReceiptTimerCallback, null,  packetReceiptInterval, packetReceiptInterval);
        }

        /// <summary>
        /// Receive a GPS Message, store the current location of the tracker
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool Receive (TelemetryMessage message)
        {
            lock (lockingUpdateObject)
            {
                Location.track = message.track;
                Location.type = message.type;
                Location.@long = message.@long;
                Location.lat = message.lat;
                Location.time = message.time;
                Location.alt = message.alt;
                Location.speed = message.speed;
                Location.climb = message.climb;

                Logger.LogInfo($"Received Telemetry Location. {Location.@long},{Location.lat}");
            }

            return true;
        } 

        /// <summary>
        /// Receive a Balloon Message, need to transmit the data in a new TrackerMessage
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<bool> Receive (BalloonMessage balloonMessage, IModuleClient moduleClient)
        {
            Logger.LogInfo($"Received Balloon Message.");
            UpdatePacketReceiveHistory(true);
            packetReceiptTimer.Change(packetReceiptInterval,packetReceiptInterval);

            this.FlightId = balloonMessage.FlightId; 

            var trackerMessage = new TrackerMessage()
            {
                Location = this.Location,
                DeviceName = this.DeviceName,
                FlightId = balloonMessage.FlightId,
                PacketReceivedPercentage = CalculatePacketReceivedPercentage()
            };

            try 
            {
                Message balloonMessageRaw = new Message(balloonMessage.ToRawBytes());
                await moduleClient.SendEventAsync(TrackerOutputName, balloonMessageRaw);

                Message trackerMessageRaw = new Message(trackerMessage.ToRawBytes());
                await moduleClient.SendEventAsync(TrackerOutputName, trackerMessageRaw);

                Logger.LogInfo($"transmitted message: {JsonConvert.SerializeObject(trackerMessage)}.");
            }
            catch (Exception ex)
            {
                // Todo - wire in with application insights
                Logger.LogWarning($"Error Transmitter tracker message: {ex.Message}");
                return false;
            }

            return true;
        }
        
        private void UpdatePacketReceiveHistory(bool newValue)
        {
            packetReceiveHistory.Enqueue(newValue);
            if (packetReceiveHistory.Count > 5)
            {
                packetReceiveHistory.Dequeue();
            }
        }

        private double CalculatePacketReceivedPercentage()
        {
            if (packetReceiveHistory.Count == 0)
            {
                Logger.LogInfo("packetReceiveHistory is empty.");
                return 0;
            }

            var receivedCount = 0;

            foreach( var flag in packetReceiveHistory)
            {
                if (flag)
                {
                    receivedCount++;
                }
            }

            return (double)receivedCount/(double)packetReceiveHistory.Count;
        }

        private void ReceiptTimerCallback(object state)
        {
            Logger.LogWarning("Timer fired - did not receive expected balloon message");
            UpdatePacketReceiveHistory(false);

            var trackerMessage = new TrackerMessage()
            {
                Location = this.Location,
                DeviceName = this.DeviceName,
                FlightId = this.FlightId,
                PacketReceivedPercentage = CalculatePacketReceivedPercentage()
            };

            try 
            {
                var moduleClient = WrappedModuleClient.Create().Result;
                Message trackerMessageRaw = new Message(trackerMessage.ToRawBytes());
                moduleClient.SendEventAsync(TrackerOutputName, trackerMessageRaw).Wait();
            }
            catch (Exception ex)
            {
                // Todo - wire in with application insights
                Logger.LogWarning($"Error Transmitter tracker message: {ex.Message}");
            }
        }
    }
}