using System;
using Microsoft.Azure.Devices.Client;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

using WeatherBalloon.Messaging;
using WeatherBalloon.Common;

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
        public GPSLocation Location;

        private object lockingUpdateObject = new object();
        
        
        public TrackerModule ()
        {
            DeviceName = "Unknown Tracker";
        }

        /// <summary>
        /// Receive a GPS Message, store the current location of the tracker
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool Receive (GPSMessage message)
        {
            lock (lockingUpdateObject)
            {
                Location = message.Location;

                Logger.LogInfo($"Recieved GPS Location.");
            }

            return true;
        } 

        /// <summary>
        /// Receive a Balloon Message, need to transmit the data in a new TrackerMessage
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<bool> Receive (BalloonMessage message, IModuleClient moduleClient)
        {
            Logger.LogInfo($"Recieved Balloon Message.");

            // Map data to a new Tracker message
            var trackerMessage = new TrackerMessage()
            {
                TrackerLocation = Location,
                BalloonLocation = message.Location, 
                State = message.State, 
                AveAscent = message.AveAscent, 
                AveDescent = message.AveDescent,
                BurstAltitude = message.BurstAltitude,
                DeviceName = this.DeviceName,
                FlightId = message.FlightId,
                Humidity = message.Humidity,
                Temperature = message.Temperature,
                Pressure = message.Pressure,
                SignalStrength = message.SignalStrength
            };

            try 
            {
                Message iotMessage = new Message(trackerMessage.ToRawBytes());

                await moduleClient.SendEventAsync(TrackerOutputName, iotMessage);

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

    }
}