using System;

namespace WeatherBalloon.Messaging
{
    public class BalloonMessage : MessageBase
    { 
        public override string Type { get { return "balloon";}}

        public GPSLocation Location {get;set;}
        public double AveAscent {get;set;}
        public double AveDescent {get;set;}
        public double BurstAltitude {get;set;}
        public bool BurstDetected { get;set;}
    }
}
