using System;

namespace WeatherBalloon.Messaging
{
    public enum BalloonState { PreLaunch = 0, Rising = 1, Falling = 2, Landed = 3};

    public class BalloonMessage : MessageBase
    { 
        public override string Type { get { return "balloon";}}

        public GPSLocation Location {get;set;}
        public double AveAscent {get;set;}
        public double AveDescent {get;set;}
        public double BurstAltitude {get;set;}
        public BalloonState State { get;set;}
    }
}
