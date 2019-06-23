using System;
using WeatherBalloon.Cloud.Documents;

namespace WeatherBalloon.Cloud.Twilio
{
    /// <summary>
    /// Notification object to pass properties of Prediction to Twilio SMS
    /// </summary>
    public class PredictionNotification
    {
        public double CurrentLat {get;set;}
        public double CurrentLong {get;set;}

        public DateTime LandingDataTime {get;set;}
        public double LandingLat {get;set;}
        public double LandingLong {get;set;}

        public string FlightId {get;set;}

        public string TrackerDevice {get;set;}

        public string ToPhoneNumber {get;set;}


        public PredictionNotification()
        {

            
        }

        public PredictionNotification(PredictionDocument predictionDocument, double currentLat, double currentLong)
        {
            LandingDataTime = predictionDocument.LandingDateTime.AddHours(-7); // to arizona time
            LandingLat = predictionDocument.Latitude;
            LandingLong = predictionDocument.Longitude;
            FlightId = predictionDocument.FlightId;
            TrackerDevice = predictionDocument.TrackerSource;
            
            this.CurrentLat = currentLat;
            this.CurrentLong = currentLong;
        }

        
    }

}