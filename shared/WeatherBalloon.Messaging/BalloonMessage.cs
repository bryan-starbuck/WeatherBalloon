using System;

namespace WeatherBalloon.Messaging
{
    public enum BalloonState { PreLaunch = 0, Rising = 1, Falling = 2, Landed = 3};

    public class BalloonMessage : MessageBase
    { 
        private static DateTime EpochTime = new DateTime(1970, 1, 1);
        
        public override string Type { get { return "balloon";}}

        public string FlightId {get;set;}

        public GPSLocation Location {get;set;}
        public double AveAscent {get;set;}
        public double AveDescent {get;set;}
        public double BurstAltitude {get;set;}
        public BalloonState State { get;set;}

        public double Temperature {get;set;}
        public double Humidity {get;set;}
        public double Pressure {get;set;}

         // Lora Signal strength indicator
        public double SignalStrength { get;set; }

        // to convert values to unsigned use the following offsets
        public const double latitude_offset = 180;
        public const double longitude_offset = 180;

        public const double ascent_offset = 20;
        public const double temperature_offset = 100;

       

        public string ToCompactMessage()
        {
            Int32 unixTimestamp = (Int32)(Timestamp.Subtract(EpochTime)).TotalSeconds;

            // unix time 10 digits
            // 

            return string.Format("{0,0000000000}{1:00.0}{2:00.0}{3}{4:000000}{5:000.00000}{6:000.00000}{7:000000.00}{8:000.0}{9:00.0}{10:000.0}{11}",
                unixTimestamp,
                AveAscent+BalloonMessage.ascent_offset,
                AveDescent+BalloonMessage.ascent_offset,
                (int)State, 
                BurstAltitude,
                Location.lat+BalloonMessage.latitude_offset, 
                Location.@long+BalloonMessage.longitude_offset, 
                Location.alt,
                Temperature + BalloonMessage.temperature_offset,
                Humidity,
                Pressure,
                FlightId);

        }

        public static BalloonMessage FromCompactMessage(string message) 
        {
            var balloonMessage = new BalloonMessage();

            try 
            {
                // sample: 
                // 155594903120.519.9900000000180.17180.92000000.00100.000.0000.0Awesome test

                Int32 epocSeconds = Int32.Parse(message.Substring(0, 10));
                
                balloonMessage.Timestamp = EpochTime.AddSeconds(epocSeconds);

                balloonMessage.AveAscent = Math.Round(float.Parse(message.Substring(10, 4)) - BalloonMessage.ascent_offset, 1);
                balloonMessage.AveDescent = Math.Round(float.Parse(message.Substring(14, 4)) - BalloonMessage.ascent_offset, 1);
                balloonMessage.State = (BalloonState)Int16.Parse(message.Substring(18,1));

                balloonMessage.BurstAltitude = float.Parse(message.Substring(19,6));
                balloonMessage.Location =  new GPSLocation();
                balloonMessage.Location.lat = Math.Round(float.Parse(message.Substring(25, 9)) - BalloonMessage.latitude_offset, 5);
                balloonMessage.Location.@long = Math.Round(float.Parse(message.Substring(34, 9)) - BalloonMessage.longitude_offset, 5);
                balloonMessage.Location.alt = float.Parse(message.Substring(43, 9));
                balloonMessage.Temperature = Math.Round(float.Parse(message.Substring(52,5)) - BalloonMessage.temperature_offset, 1);
                balloonMessage.Humidity = Math.Round(float.Parse(message.Substring(57,4)), 1);
                balloonMessage.Pressure = Math.Round(float.Parse(message.Substring(61,5)), 1);
                
                // The Signal Strength appears at the end of the message
                int position = message.IndexOf(":");
                if ( position > 0)
                {
                    balloonMessage.FlightId = message.Substring(66, position - 66);
                    balloonMessage.SignalStrength = Math.Round(float.Parse(message.Substring(position+1)), 1);
                }
                else 
                {   
                    balloonMessage.FlightId = message.Substring(66);
                }
                
            }
            catch (Exception ex)
            {
                // failure parsing - todo
                throw ;
            }

            return balloonMessage;
        }
    }
}
