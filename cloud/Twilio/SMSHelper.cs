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
            //return $"Lands @ {notification.LandingDataTime} \n https://www.google.com/maps/search/?api=1&query={notification.LandingLat},{notification.LandingLong}&query_place_id={notification.FlightId.Replace(' ', '-')} ";

            if ((notification.LandingLat == notification.CurrentLat) && 
                (notification.LandingLong == notification.CurrentLong))
            {
                return $"Balloon Position @ {notification.LandingDataTime} \nhttps://www.google.com/maps/search/?api=1&query={notification.CurrentLat},{notification.CurrentLong}";
            }
            else 
            {
               return $"Balloon Landing @ {notification.LandingDataTime} \nhttps://www.google.com/maps/dir/?api=1&origin={notification.CurrentLat},{notification.CurrentLong}&destination={notification.LandingLat},{notification.LandingLong}&travelmode=walking";
            }
         }

    }
}