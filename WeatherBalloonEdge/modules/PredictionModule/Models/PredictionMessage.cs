using System;

using System.Collections.Generic;

namespace PredictionModule.Models
{
    public class PredictionMessage
    {
        public float currentLat {get;set;}
        public float currentLong {get;set;}

        public DateTime PredictionDate {get;set;}
        public List<PredictionPoint> PredictionPoints;

        public PredictionMessage()
        {
            PredictionPoints = new List<PredictionPoint>();

        }

    }

    public class PredictionPoint
    {
        public DateTime Time {get;set;}
        public float Lat {get;set;}
        public float Long {get;set;}
        public float Alt {get;set;}

    }

}