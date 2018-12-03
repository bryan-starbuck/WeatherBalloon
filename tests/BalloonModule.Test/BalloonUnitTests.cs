using System;
using Xunit;

using WeatherBalloon.Messaging;
using WeatherBalloon.BalloonModule;

namespace BalloonModule.Test
{
    public class BalloonUnitTests
    {
        [Fact]
        public void ReceiveGPSMessage()
        {


        }


        private GPSMessage CreateGPSMessage()
        {
            Random random = new Random();

            return new GPSMessage()
            {

            };
        }
    }
}
