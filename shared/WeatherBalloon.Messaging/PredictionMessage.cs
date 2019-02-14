using System;

namespace WeatherBalloon.Messaging
{

    public class PredictionMessage : MessageBase
    {
        public override string Type { get { return "prediction";}}

        public DateTime PredictionDate {get;set;}
        
        public float LandingLat {get;set;}
        public float LandingLong {get;set;}
        public float LandingAltitude {get;set;}
        public DateTime LandingDateTime {get;set;}
        public Int64 UnixTimestamp {get;set;}

        public GPSLocation BalloonLocation {get;set;}

        public string FlightId { get;set; }

        public string TrackerSource { get;set; }
    }
}