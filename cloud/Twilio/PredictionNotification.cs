using System;
using WeatherBalloon.Cloud.Documents;

namespace WeatherBalloon.Cloud.Twilio
{
    /// <summary>
    /// Notification object to pass properties of Prediction to Twilio SMS
    /// </summary>
    public class PredictionNotification
    {
        public DateTime LandingDataTime {get;set;}
        public double LandingLat {get;set;}
        public double LandingLong {get;set;}

        public string FlightId {get;set;}

        public string TrackerDevice {get;set;}

        public double DistanceToLanding {get;set;}


        public PredictionNotification()
        {

            
        }

        public PredictionNotification(PredictionDocument predictionDocument)
        {
            LandingDataTime = predictionDocument.LandingDateTime.AddHours(-7); // to arizona time
            LandingLat = predictionDocument.Latitude;
            LandingLong = predictionDocument.Longitude;
            FlightId = predictionDocument.FlightId;
            TrackerDevice = predictionDocument.TrackerSource;
            DistanceToLanding = predictionDocument.DistanceToLanding;
        }

        
    }

}