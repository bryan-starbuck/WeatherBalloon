using System;
using System.Text;
using System.Linq;

namespace WeatherBalloon.Messaging
{
    public enum BalloonState { PreLaunch = 0, Rising = 1, Falling = 2, Landed = 3};

    public class BalloonMessage : MessageBase, IDeviceMessage
    { 
        private static DateTime MyEpochTime;
        
        public override string Type { get { return "balloon";}}

        // Properties common with Tracker Message
        public string DeviceName {get {return "Weather Balloon";}}

        public string FlightId {get;set;}

        public GPSLocation Location {get;set;}


        // Balloon Specific properties
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
        public const double climb_offset = 10;
        public const double speed_offset = 50;

        public BalloonMessage()
        {
            Location = new GPSLocation();

            MyEpochTime = DateTime.UtcNow.AddDays(-1).Date;
        }

        public string ToCompactMessage()
        {
            Int32 timeOffset = (Int32)(Timestamp.Subtract(MyEpochTime)).TotalSeconds;

            // unix time 10 digits
            // 

            var messageString = string.Format("{0,000000}{1:00.0}{2:00.0}{3}{4:000000}{5:000.00000}{6:000.00000}{7:000000.00}{8:00.00}{9:00.0}{10:000.0}{11:00.0}{12:000.0}{13}",
                timeOffset,
                AveAscent+BalloonMessage.ascent_offset,
                AveDescent+BalloonMessage.ascent_offset,
                (int)State, 
                BurstAltitude,
                Location.lat+BalloonMessage.latitude_offset, 
                Location.@long+BalloonMessage.longitude_offset, 
                Location.alt,
                Location.climb + BalloonMessage.climb_offset,
                Location.speed + BalloonMessage.speed_offset,
                Temperature + BalloonMessage.temperature_offset,
                Humidity,
                Pressure,
                FlightId);
            if ( timeOffset < 100000)
            {
                messageString = "0"+ messageString;
            }

            return messageString.Replace(".", "");
        }

        public static BalloonMessage FromCompactMessage(string message) 
        {
            var balloonMessage = new BalloonMessage();

            var decimalPositions = new[] {8, 11, 22, 30, 41, 45, 49, 53, 56, 60 };

            try 
            {
                // sample: 
                // 0903120.519.9900000000180.17180.92000000.00100.000.0000.0Awesome test
                
                // add in decimals
                StringBuilder stringBuilder = new StringBuilder(message.Length*2);
                for(var index = 0; index < message.Length; index++)
                {
                    if (decimalPositions.Contains(index))
                    {
                        stringBuilder.Append(".");
                    }
                    stringBuilder.Append(message[index]);
                } 
                
                message = stringBuilder.ToString();


                Int32 epocSeconds = Int32.Parse(message.Substring(0, 6));
                
                balloonMessage.Timestamp = MyEpochTime.AddTicks(-(MyEpochTime.Ticks % TimeSpan.TicksPerSecond)).AddSeconds(epocSeconds);

                balloonMessage.AveAscent = Math.Round(float.Parse(message.Substring(6, 4)) - BalloonMessage.ascent_offset, 1);
                balloonMessage.AveDescent = Math.Round(float.Parse(message.Substring(10, 4)) - BalloonMessage.ascent_offset, 1);
                balloonMessage.State = (BalloonState)Int16.Parse(message.Substring(14,1));

                balloonMessage.BurstAltitude = float.Parse(message.Substring(15,6));
                balloonMessage.Location.lat = Math.Round(float.Parse(message.Substring(21, 9)) - BalloonMessage.latitude_offset, 5);
                balloonMessage.Location.@long = Math.Round(float.Parse(message.Substring(30, 9)) - BalloonMessage.longitude_offset, 5);
                balloonMessage.Location.alt = Math.Round(float.Parse(message.Substring(39, 9)), 2);
                balloonMessage.Location.climb = Math.Round(float.Parse(message.Substring(48, 5)) - BalloonMessage.climb_offset, 2);
                balloonMessage.Location.speed = Math.Round(float.Parse(message.Substring(53, 4)) - BalloonMessage.speed_offset, 1);

                balloonMessage.Temperature = Math.Round(float.Parse(message.Substring(57,5)) - BalloonMessage.temperature_offset, 1);
                balloonMessage.Humidity = Math.Round(float.Parse(message.Substring(62,4)), 1);
                balloonMessage.Pressure = Math.Round(float.Parse(message.Substring(66,5)), 1);
                
                // The Signal Strength appears at the end of the message
                int position = message.IndexOf(":");
                if ( position > 0)
                {
                    balloonMessage.FlightId = message.Substring(71, position - 71);
                    balloonMessage.SignalStrength = Math.Round(float.Parse(message.Substring(position+1)), 1);
                }
                else 
                {   
                    balloonMessage.FlightId = message.Substring(71);
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