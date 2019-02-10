using System;

namespace WeatherBalloon.Cloud.Twilio
{

    public static class SMSHelper 
    {
        /// <summary>
        /// Create a text body from a prediction notification
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string FormatSMS(PredictionNotification notification)
        {
            //google url docs: https://developers.google.com/maps/documentation/urls/guide
            return $"Balloon Landing: {notification.PredictionDate} \n https://www.google.com/maps/search/?api=1&query={notification.LandingLat},{notification.LandingLong}&query_place_id=PredictedLanding";
        }

    }
}