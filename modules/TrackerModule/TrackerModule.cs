using System;
using Microsoft.Azure.Devices.Client;
using System.Threading;
using System.Threading.Tasks;

using WeatherBalloon.Messaging;
using WeatherBalloon.Common;

namespace WeatherBalloon.TrackerModule
{
    /// <summary>
    /// A GPS device that is tracking the weather balloon
    /// </summary>
    public class TrackerModule 
    {
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
            // Map data to a new Tracker message
            var trackerMessage = new TrackerMessage()
            {
                TrackerLocation = Location,
                BalloonLocation = message.Location, 
                State = message.State, 
                AveAscent = message.AveAscent, 
                AveDescent = message.AveDescent,
                BurstAltitude = message.BurstAltitude
            };

            try 
            {
                Message iotMessage = new Message(trackerMessage.ToRawBytes());

                await moduleClient.SendEventAsync(TrackerOutputName, iotMessage);
            }
            catch (Exception ex)
            {
                Logger.LogWarning($"Error Transmitter tracker message: {ex.Message}");
                return false;
            }

            return true;
        }

    }
}