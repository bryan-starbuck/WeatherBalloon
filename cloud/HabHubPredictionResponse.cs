using System;

namespace WeatherBalloon.Cloud
{
    public class HabHubPredictionResponse
    {
        public string valid { get; set; }
        public string uuid { get; set; }
        public int timestamp { get; set; }
    }
}