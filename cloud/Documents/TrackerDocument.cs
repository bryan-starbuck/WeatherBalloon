using System;

using WeatherBalloon.Messaging;
using Microsoft.Azure.Documents.Spatial;

namespace WeatherBalloon.Cloud.Documents
{
    /// <summary>
    /// This is a data object representing the Tracker data that will 
    /// be written to cosmos.
    /// 
    /// </summary>
    public class TrackerDocument : DocumentBase
    {
        public override string Type { get { return "tracker";}}

        public string DeviceName { get;set; }

        public double DistanceToBalloon {get;set;}

        public static TrackerDocument Create(TrackerMessage message)
        {
            return new TrackerDocument() 
            {
                Altitude = (message.TrackerLocation != null) ? message.TrackerLocation.alt : 0,
                DeviceName = message.DeviceName,
                FlightId = message.FlightId,
                TimestampUTC = message.Timestamp,
                partitionid = message.partitionid,
                Latitude =  (message.TrackerLocation != null) ? message.TrackerLocation.lat : 0,
                Longitude =  (message.TrackerLocation != null) ? message.TrackerLocation.@long : 0,
                Geopoint =  (message.TrackerLocation != null) ? new Point(message.TrackerLocation.@long,message.TrackerLocation.lat) : null

                // DistanceToBalloon  - todo
            };
        }

    }
    
}