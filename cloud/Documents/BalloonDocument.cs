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

        public static BalloonDocument Create(TrackerMessage message)
        {
            return new BalloonDocument()
            {
                AveAscent = message.AveAscent,
                AveDescent = message.AveDescent,
                BurstAltitude = message.BurstAltitude,
                State = message.State,
                Track = (message.BalloonLocation != null ) ? message.BalloonLocation.track : 0,
                GpsMode = (message.BalloonLocation != null ) ? message.BalloonLocation.mode : 0,
                GpsTime = (message.BalloonLocation != null ) ? message.BalloonLocation.time : "unknown",
                Speed = (message.BalloonLocation != null ) ? message.BalloonLocation.speed : 0,
                Climb = (message.BalloonLocation != null ) ? message.BalloonLocation.climb : 0,
                FlightId = message.FlightId,
                TimestampUTC = message.Timestamp,
                partitionid = message.partitionid,
                Latitude = (message.BalloonLocation != null ) ? message.BalloonLocation.lat : 0,
                Longitude = (message.BalloonLocation != null ) ? message.BalloonLocation.@long : 0,
                Altitude = (message.BalloonLocation != null ) ? message.BalloonLocation.alt : 0,
                Geopoint = (message.BalloonLocation != null ) ? new Point(message.BalloonLocation.@long,message.BalloonLocation.lat) : null,
                TrackerSource = message.DeviceName,
                Humidity = message.Humidity,
                Pressure = message.Pressure,
                Temperature = message.Temperature,
                SignalStrength = message.SignalStrength
            };

        }
        
    }

}