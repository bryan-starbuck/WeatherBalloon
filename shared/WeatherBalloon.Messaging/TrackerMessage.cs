using System;

namespace WeatherBalloon.Messaging 
{
    public class TrackerMessage : MessageBase
    {
        public override string Type { get { return "tracker";}}

        public GPSLocation BalloonLocation {get;set;}
        public GPSLocation TrackerLocation {get;set;}

        public double AveAscent {get;set;}
        public double AveDescent {get;set;}
        public double BurstAltitude {get;set;}
        public BalloonState State { get;set;}

    }


}