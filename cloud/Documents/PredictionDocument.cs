using System;

using WeatherBalloon.Messaging;
using Microsoft.Azure.Documents.Spatial;

namespace WeatherBalloon.Cloud.Documents
{
    /// <summary>
    /// Data object to store the prediction in cosmos 
    /// 
    /// </summary>
    public class PredictionDocument
    {
        public string Type { get { return "prediction";}}

        public DateTime PredictionDate {get;set;}
        
        public float LandingLat {get;set;}
        public float LandingLong {get;set;}
        public float LandingAltitude {get;set;}
        public DateTime LandingDateTime {get;set;}
        public Int64 UnixTimestamp {get;set;}
        public string FlightId { get;set; }
        public string TrackerSource { get;set; }

        public GPSLocation BalloonLocation {get;set;}

        public string partitionid {get;set;}

        public PredictionDocument(PredictionMessage message)
        {
            PredictionDate = message.PredictionDate;
            LandingLat = message.LandingLat;
            LandingLong = message.LandingLong;
            LandingAltitude = message.LandingAltitude;
            LandingDateTime = message.LandingDateTime;
            UnixTimestamp = message.UnixTimestamp;
            FlightId = message.FlightId;
            TrackerSource = message.TrackerSource;
            BalloonLocation = message.BalloonLocation;
            partitionid = message.partitionid;

            EnrichWithGeolocationData();
        }

        public Point BalloonPointLocation {get;set;}

        public Point PredictionPointLocation {get;set;}

        public double DistanceToLanding {get;set;}

        public void EnrichWithGeolocationData()
        {
            if (BalloonLocation != null)
            {
                BalloonPointLocation = new Point(this.BalloonLocation.@long, this.BalloonLocation.lat);
            }

            PredictionPointLocation = new Point(this.LandingLong, this.LandingLat);

            if (( PredictionPointLocation != null) && (BalloonPointLocation != null))
            {
                //DistanceToLanding = BalloonPointLocation.Distance(PredictionPointLocation);
            }
        }


    }

}