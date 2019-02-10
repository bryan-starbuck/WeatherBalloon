using System;
using Newtonsoft.Json;
using System.Text;
using System.Collections.Generic;

namespace WeatherBalloon.Cloud.HabHub
{
    public class HabHubMessage
    {
        public double lat { get; set; }
        public double lon { get; set; }
        public double alt { get; set; }
        public double ascent { get; set; }
        public double burst { get; set; }

        // HabHub Messages must be for the future, Time will be built 5 min in the future
        private static DateTime targetTime = DateTime.UtcNow.AddMinutes(10);        
        public string hour = targetTime.ToString("HH");
        public string min = targetTime.ToString("mm");
        public string second = targetTime.ToString("ss");
        public string year = targetTime.ToString("yyyy");
        public string month = targetTime.ToString("MM");
        public string day = targetTime.ToString("dd");

        public HabHubMessage()
        {
            
        }

        public Dictionary<string,string> ToParameterDictionary()
        {
            return new Dictionary<string, string>
            {
                { "lat",  lat.ToString()},
                { "lon",  lon.ToString()},
                { "initial_alt", alt.ToString()},
                { "hour", hour.ToString()},
                { "min", min.ToString()},
                { "second", second.ToString()},
                { "day", day.ToString()},
                { "month", month.ToString()},
                { "year", year.ToString()},
                { "ascent", ascent.ToString()},
                { "drag", "5"},
                { "burst", burst.ToString()},
                { "submit", "Run Prediction"}
            };
        }
    }
}