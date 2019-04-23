using System;

namespace WeatherBalloon.Messaging
{
    public class TelemetryMessage
    {
        public double track { get; set; }
        public string type { get; set; }
        public double @long { get; set; }
        public int mode { get; set; }
        public string time { get; set; }
        public double lat { get; set; }
        public double alt { get; set; }
        public double speed { get; set; }
        public double climb { get; set; }
        public double temp {get;set;}
        public double humidity {get;set;}
        public double pressure {get;set;}
    }

}