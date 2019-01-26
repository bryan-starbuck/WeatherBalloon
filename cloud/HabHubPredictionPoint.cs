using System;

namespace WeatherBalloon.Cloud
{
    public class HabHubPredictionPoint
    {
        public Int64 Timestamp { get;set;}
        public float Latitude {get;set;}
        public float Longitude {get;set;}
        public float Altitude {get;set;}

    }
}