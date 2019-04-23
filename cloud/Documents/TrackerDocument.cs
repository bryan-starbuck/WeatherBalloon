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
                Altitude = message.TrackerLocation.alt,
                DeviceName = message.DeviceName,
                FlightId = message.FlightId,
                TimestampUTC = message.Timestamp,
                partitionid = message.partitionid,
                Latitude = message.TrackerLocation.lat,
                Longitude = message.TrackerLocation.@long,
                Geopoint = new Point(message.TrackerLocation.@long,message.TrackerLocation.lat)

                // DistanceToBalloon  - todo
            };
        }

    }
    
}