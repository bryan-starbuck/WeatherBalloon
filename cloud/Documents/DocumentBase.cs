using System;

using Microsoft.Azure.Documents.Spatial;

namespace WeatherBalloon.Cloud.Documents
{

    public class DocumentBase
    {
        public virtual string Type { get;set;}
        public string partitionid {get;set;}
        public string FlightId { get;set; }
        public DateTime TimestampUTC {get;set;}

        public double Latitude {get;set;}

        public double Longitude {get;set;}

        public double Altitude {get; set;}

        public Point Geopoint {get;set;}


        // todo - initialize Geopoint
    }

}