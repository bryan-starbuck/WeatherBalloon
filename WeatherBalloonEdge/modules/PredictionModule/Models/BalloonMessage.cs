using System;

namespace PredictionModule.Models 
{
    public class BalloonMessage
    {

        public float LaunchLat { get; set;}
        public float LaunchLong {get;set;}

        public float CurrentLat {get;set;}
        public float CurrentLong {get;set;}

        public float AscentRate {get;set;}
        public float DescentRate {get;set;}

        public float Weight {get;set;}

        public float LaunchAltitude {get;set;}

        public DateTime LaunchTime {get;set;}

        public float BurstAltitude {get;set;}

    }
}
