using System;

using WeatherBalloon.Messaging;
using Microsoft.Azure.Documents.Spatial;

namespace WeatherBalloon.Cloud.Documents
{
    /// <summary>
    /// This is a data object representing the Tracker Message that will 
    /// be written to cosmos.
    /// 
    /// </summary>
    public class TrackerDocument
    {
        public string Type { get { return "tracker";}}

        public string FlightId {get;set;}
        public string DeviceName { get;set; }

        public GPSLocation BalloonLocation {get;set;}
        public GPSLocation TrackerLocation {get;set;}

        public double AveAscent {get;set;}
        public double AveDescent {get;set;}
        public double BurstAltitude {get;set;}
        public BalloonState State { get;set;}

        public Point BalloonPointLocation {get;set;}

        public Point TrackerPointLocation {get;set;}

        public double DistanceToBalloon {get;set;}

        public string partitionid {get;set;}


        public TrackerDocument(TrackerMessage message)
        {
            FlightId = message.FlightId;
            DeviceName = message.DeviceName;
            AveAscent = message.AveAscent;
            AveDescent = message.AveDescent;
            BurstAltitude = message.BurstAltitude;
            State = message.State;
            BalloonLocation = message.BalloonLocation;
            TrackerLocation = message.TrackerLocation;
            partitionid = message.partitionid;

            EnrichWithGeolocationData();
        }

        private void EnrichWithGeolocationData()
        {
            if (BalloonLocation != null)
            {
                BalloonPointLocation = new Point(this.BalloonLocation.@long, this.BalloonLocation.lat);
            }

            if (TrackerLocation != null)
            {
                TrackerPointLocation = new Point(this.TrackerLocation.@long, this.TrackerLocation.lat);
            }

            if (( TrackerPointLocation != null) && (BalloonPointLocation != null))
            {
                //DistanceToBalloon = TrackerPointLocation.Distance(BalloonPointLocation);
            }
        }

    }
    
}