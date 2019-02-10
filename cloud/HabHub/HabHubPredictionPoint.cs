using System;

namespace WeatherBalloon.Cloud.HabHub
{
    public class HabHubPredictionPoint
    {
        public Int64 Timestamp { get;set;}
        public Single Latitude {get;set;}
        public Single Longitude {get;set;}
        public Single Altitude {get;set;}

    }
}