using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;
using System;

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
        /// Threshold to determine balloon is rising 
        /// </summary>
        public const double RiseThreshold = 0.5;  // todo validate number and units

        /// <summary>
        /// Threshold to determine balloon is falling
        /// </summary>
        public const double FallingThreshold = -0.5; // todo validate number and units

        /// <summary>
        /// Threshold to determine balloon is off the ground
        /// </summary>
        public const double GroundAltitudeThreshold = 1500; // assume on ground when lower than this value in feet

        /// <summary>
        /// Expected burst altitude
        /// </summary>
        public const double PredictedBurstAltitude = 30000.0; // todo validate number

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

        /// <summary>
        /// number of data points seen with a rising climb
        /// </summary>
        private long ascentDataPoints = 0;

        /// <summary>
        /// number of data points seen with a falling climb
        /// </summary>
        private long descentDataPoints = 0;

        private object lockingUpdateObject = new object();

        private string FlightId;

        /// <summary>
        /// Constructor
        /// </summary>
        public BalloonModule ()
        {
            FlightId = FlightIdGenerator.Generate();
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

        /// <summary>
        /// Determine balloon state based on latest gps location
        /// </summary>
        /// <param name="currentState"></param>
        /// <param name="newLocation"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Transmit current balloon data
        /// </summary>
        /// <param name="moduleClient"></param>
        /// <returns></returns>
        public async Task<bool> TransmitBalloonMessage(IModuleClient moduleClient)
        {
            var balloonMessage = CreateBalloonMessage();

            Message message = new Message(balloonMessage.ToRawBytes());

            try 
            {
                await moduleClient.SendEventAsync(BalloonOutputName, message);
        
                Logger.LogInfo($"transmitted message: {JsonConvert.SerializeObject(balloonMessage)}.");
            }
            catch (Exception ex)
            {
                // Todo - wire in with application insights
                Logger.LogError($"Failed to transmit balloon message. Exception: {ex.Message}");
                return false;
            }

            return true;
        }



        /// <summary>
        /// Create a Balloon Message with the current balloon state
        /// </summary>
        /// <returns></returns>
        private BalloonMessage CreateBalloonMessage()
        {
            return new BalloonMessage()
            {
                FlightId = this.FlightId,
                Location = Location, 
                AveAscent = AverageAscent,
                AveDescent = AverageDescent,
                BurstAltitude = BurstAltitude,
                State = BalloonState 
            };
        }
    }
}