using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;

using WeatherBalloon.Common;
using WeatherBalloon.Messaging;

namespace WeatherBalloon.BalloonModule
{
    /// <summary>
    /// Module logic for the Balloon
    /// </summary>
    public class BalloonModule
    {
        /// <summary>
        /// IoT Edge Input name  
        /// </summary>
        public const string TelemetryInputName = "telemetryInput";
        /// <summary>
        /// IoT Edge Output name
        /// </summary>
        public const string BalloonOutputName = "balloonOutput";
 
        /// <summary>
        /// Rate the balloon is rising since Launch detected
        /// </summary>
        public double AverageAscent = 0.0;
        /// <summary>
        /// Rate the balloon is falling since Burst detected
        /// </summary>
        public double AverageDescent = 0.0;

        /// <summary>
        /// Altitude Burst was detected, default to 90,000 feet until balloon pops.
        /// </summary>
        public  double BurstAltitude = 90000.0;


        /// <summary>
        /// Balloon State 
        /// - PreLaunch = On the Ground
        /// - Rising = Balloon going up, burst not detected
        /// - Falling = Balloon Burst detected, device falling
        /// - Landed = Flight Complete
        /// </summary>
        public BalloonState BalloonState = BalloonState.PreLaunch;

        /// <summary>
        /// Current location according to the most recent gps message
        /// </summary>
        public GPSLocation Location;

        //private int ascentDataPoints = 0;
        //private int descentDataPoints = 0;

        private object lockingUpdateObject = new object();

        
        /// <summary>
        /// Constructor
        /// </summary>
        public BalloonModule ()
        {
            
        }

        /// <summary>
        /// Receive new GPS information
        /// </summary>
        /// <param name="message"></param>
        public void Receive(GPSMessage message)
        {
            lock (lockingUpdateObject)
            {
                Location = message.Location;

                if (BalloonState == BalloonState.PreLaunch)
                {

                }

                // update average ascent/descent rate and burst detetection


                // todo
            }
            
        }

        public async void Transmit(IModuleClient moduleClient)
        {
            var balloonMessage = CreateBalloonMessage();

            Message message = new Message(balloonMessage.ToRawBytes());

            await moduleClient.SendEventAsync(BalloonOutputName, message);
        
            Logger.LogInfo($"transmitted message: {JsonConvert.SerializeObject(balloonMessage)}.");
        }



        /// <summary>
        /// Create a Balloon Message with the current balloon state
        /// </summary>
        /// <returns></returns>
        private BalloonMessage CreateBalloonMessage()
        {
            return new BalloonMessage()
            {
                Location = Location, 
                AveAscent = AverageAscent,
                AveDescent = AverageDescent,
                BurstAltitude = BurstAltitude,
                State = BalloonState 
            };
        }
    }
}