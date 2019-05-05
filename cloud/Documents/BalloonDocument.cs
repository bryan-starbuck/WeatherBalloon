using System;
using Microsoft.Azure.Documents.Spatial;
using WeatherBalloon.Messaging;

namespace WeatherBalloon.Cloud.Documents
{
    public class BalloonDocument : DocumentBase
    {
        public override string Type { get {return "balloon";}}

        public double AveAscent {get;set;}
        public double AveDescent {get;set;}
        public double BurstAltitude {get;set;}
        public BalloonState State { get;set;}

        public double Track { get; set; }

        public int GpsMode { get; set; }

        public string GpsTime { get; set; }

        public double Speed { get; set; }

        public double Climb { get; set; }

        public string TrackerSource {get;set;}

        public double Temperature {get;set;}

        public double Humidity {get;set;}

        public double Pressure {get;set;}

        public double SignalStrength {get;set;}

        public static BalloonDocument Create(BalloonMessage message)
        {
            return new BalloonDocument()
            {
                AveAscent = message.AveAscent,
                AveDescent = message.AveDescent,
                BurstAltitude = message.BurstAltitude,
                State = message.State,
                Track = (message.Location != null ) ? message.Location.track : 0,
                GpsMode = (message.Location != null ) ? message.Location.mode : 0,
                GpsTime = (message.Location != null ) ? message.Location.time : "unknown",
                Speed = (message.Location != null ) ? message.Location.speed : 0,
                Climb = (message.Location != null ) ? message.Location.climb : 0,
                FlightId = message.FlightId,
                TimestampUTC = message.Timestamp,
                partitionid = message.partitionid,
                Latitude = (message.Location != null ) ? message.Location.lat : 0,
                Longitude = (message.Location != null ) ? message.Location.@long : 0,
                Altitude = (message.Location != null ) ? message.Location.alt : 0,
                Geopoint = (message.Location != null ) ? new Point(message.Location.@long,message.Location.lat) : null,
                TrackerSource = message.DeviceName,
                Humidity = message.Humidity,
                Pressure = message.Pressure,
                Temperature = message.Temperature,
                SignalStrength = message.SignalStrength
            };

        }
        
    }

}