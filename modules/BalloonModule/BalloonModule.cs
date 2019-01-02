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
        public  double BurstAltitude = PredictedBurstAltitude;

        public const double RiseThreshold = 0.5;  // todo validate number
        public const double FallingThreshold = -0.5; // todo validate number

        public const double GroundAltitudeThreshold = 1500; // assume on ground when lower than this value in feet

        public const double PredictedBurstAltitude = 90000.0;

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

        private long ascentDataPoints = 0;
        private long descentDataPoints = 0;

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

                var newState = DetermineNewState(BalloonState, message.Location);

                if (newState != BalloonState)
                {
                    // Did the balloon just pop?
                    if (BalloonState == BalloonState.Rising && newState == BalloonState.Falling)
                    {
                        Logger.LogInfo("Burst DETECTED!!!");

                        BurstAltitude = message.Location.alt;
                    }

                    Logger.LogInfo($"Transitioned Balloon State. From: {BalloonState} To: {newState}");
                    BalloonState = newState;
                }

                // Update averaged telemetry data
                if (BalloonState == BalloonState.Rising)
                {
                    ascentDataPoints++;
                    AverageAscent = ((ascentDataPoints - 1) / ascentDataPoints) * AverageAscent + (message.Location.climb/ascentDataPoints);
                
                } 
                else if (BalloonState == BalloonState.Falling)
                {
                    descentDataPoints++;
                    AverageDescent = ((descentDataPoints - 1) / descentDataPoints) * AverageDescent + (message.Location.climb/descentDataPoints);
                }  
            }
        }

        public BalloonState DetermineNewState(BalloonState currentState, GPSLocation newLocation)
        {
            BalloonState newState = currentState;

            switch (currentState)
            {
                case BalloonState.PreLaunch:
                    if ((newLocation.alt >= GroundAltitudeThreshold) && (newLocation.climb >= RiseThreshold))
                    {
                        newState = BalloonState.Rising;
                    }
                    break;
                case BalloonState.Rising:
                    if ((newLocation.alt >= GroundAltitudeThreshold) && (newLocation.climb <= FallingThreshold))
                    {
                        newState = BalloonState.Falling;
                    }
                    break;
                case BalloonState.Falling:
                    if (newLocation.alt <= GroundAltitudeThreshold)
                    {
                        newState = BalloonState.Landed;
                    }
                    break;

            }

            return newState;
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