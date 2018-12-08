using WeatherBalloon.Messaging;

namespace WeatherBalloon.BalloonModule
{
    public class BalloonModule
    {
        private double averageAscent = 0.0;
        private double averageDescent = 0.0;

        private bool burstDetected = false;

        private double burstAltitude = 0.0;

        //private int ascentDataPoints = 0;
        //private int descentDataPoints = 0;

        private GPSLocation location;

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
            location = message.Location;

            // update average ascent/descent rate and burst detetection


            // todo
        }

        /// <summary>
        /// Create a Balloon Message with the current balloon state
        /// </summary>
        /// <returns></returns>
        public BalloonMessage CreateBalloonMessage()
        {
            return new BalloonMessage()
            {
                Location = location, 
                AveAscent = averageAscent,
                AveDescent = averageDescent,
                BurstDetected = burstDetected,
                BurstAltitude = burstAltitude 
            };
        }

    }


}