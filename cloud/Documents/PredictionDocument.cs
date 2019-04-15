using System;

using WeatherBalloon.Messaging;
using Microsoft.Azure.Documents.Spatial;

namespace WeatherBalloon.Cloud.Documents
{
    /// <summary>
    /// Data object to store the prediction in cosmos 
    /// 
    /// </summary>
    public class PredictionDocument : DocumentBase
    {
        public override string Type { get { return "prediction";}}

        public DateTime LandingDateTime {get;set;}
        public string TrackerSource { get;set; }

        public double DistanceToLanding {get;set;}

        public static PredictionDocument Create(PredictionMessage message)
        {
            return new PredictionDocument()
            {
                FlightId = message.FlightId,
                TimestampUTC = message.Timestamp,
                partitionid = message.partitionid,
                Latitude = message.LandingLat,
                Longitude = message.LandingLong,
                Altitude = message.LandingAltitude,
                Geopoint = new Point(message.LandingLong,message.LandingLat),

                TrackerSource = message.TrackerSource,
                LandingDateTime = message.LandingDateTime
            };
        }

    }

}