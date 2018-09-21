using System;

using System.Collections.Generic;

namespace PredictionModule.Models
{
    public class PredictionMessage
    {
        public string Type {get;set;}
        public DateTime PredictionDate {get;set;}
        
        public float LandingLat {get;set;}
        public float LandingLong {get;set;}
        public DateTime LandingDateTime {get;set;}

    }
}