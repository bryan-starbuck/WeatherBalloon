using WeatherBalloon.Messaging;

namespace WeatherBalloon.BalloonModule
{
    public class BalloonModule
    {
        public double AverageAscent = 0.0;
        public double AverageDescent = 0.0;

        public bool BurstDetected = false;

        public  double BurstAltitude = 0.0;

        //private int ascentDataPoints = 0;
        //private int descentDataPoints = 0;

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
            Location = message.Location;

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
                Location = Location, 
                AveAscent = AverageAscent,
                AveDescent = AverageDescent,
                BurstDetected = BurstDetected,
                BurstAltitude = BurstAltitude 
            };
        }

    }


}