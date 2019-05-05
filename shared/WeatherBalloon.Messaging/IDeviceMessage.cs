using System;


namespace WeatherBalloon.Messaging
{
    /// <summary>
    /// Common properties for Device Messages, these properties are shared between Trackers and the Balloon to 
    /// show devices in a common view.
    /// </summary>
    public interface IDeviceMessage
    {
        string DeviceName { get;}

        string FlightId { get;set;}

        GPSLocation Location { get;set;}
    }


}