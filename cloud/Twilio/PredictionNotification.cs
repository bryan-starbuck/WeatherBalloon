using System;

using WeatherBalloon.Messaging;

namespace WeatherBalloon.Cloud.Twilio
{
    /// <summary>
    /// Notification object to pass properties of Prediction to Twilio SMS
    /// </summary>
    public class PredictionNotification
    {
        public DateTime PredictionDate {get;set;}
        public float LandingLat {get;set;}
        public float LandingLong {get;set;}

        public PredictionNotification(PredictionMessage predictionMessage)
        {
            PredictionDate = predictionMessage.PredictionDate;
            LandingLat = predictionMessage.LandingLat;
            LandingLong = predictionMessage.LandingLong;

        }

        
    }

}