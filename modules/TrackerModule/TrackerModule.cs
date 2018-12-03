using System;
using WeatherBalloon.Messaging;

namespace WeatherBalloon.TrackerModule
{
    /// <summary>
    /// A GPS device that is tracking the weather balloon
    /// </summary>
    public class TrackerModule 
    {
        public TrackerModule ()
        {


        }

        public bool Receive (GPSMessage message)
        {
            return true;

        } 

    }



}