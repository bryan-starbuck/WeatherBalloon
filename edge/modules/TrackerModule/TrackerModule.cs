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
        public bool Receive (TelemetryMessage message)
        {
            lock (lockingUpdateObject)
            {
                Location = new GPSLocation()
                {
                    track = message.track,
                    type = message.type,
                    @long = message.@long,
                    lat = message.lat,
                    time = message.time,
                    alt = message.alt, 
                    speed = message.speed,
                    climb = message.climb  
                };

                Logger.LogInfo($"Recieved Telemetry Location.");
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
            Logger.LogInfo($"Recieved Balloon Message.");

            var trackerMessage = new TrackerMessage()
            {
                Location = Location,
                DeviceName = this.DeviceName,
                FlightId = balloonMessage.FlightId,
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

    }
}