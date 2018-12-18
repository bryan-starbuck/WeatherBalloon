using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;

using WeatherBalloon.Common;
using WeatherBalloon.Messaging;



namespace WeatherBalloon.BalloonModule
{
    public class BalloonModule
    {
        public const string TelemetryInputName = "telemetryInput";
        public const string BalloonOutputName = "balloonOutput";
 
        public double AverageAscent = 0.0;
        public double AverageDescent = 0.0;

        public bool BurstDetected = false;

        public  double BurstAltitude = 0.0;

        //private int ascentDataPoints = 0;
        //private int descentDataPoints = 0;

        private object lockingUpdateObject = new object();

        public GPSLocation Location;

        /// <summary>
        /// Constructor - TODO - default values?
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
                BurstDetected = BurstDetected,
                BurstAltitude = BurstAltitude 
            };
        }
    }
}