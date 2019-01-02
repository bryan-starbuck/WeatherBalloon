using System;
using Xunit;
using Shouldly;
using FakeItEasy;
using System.Text;
using Newtonsoft.Json;

using WeatherBalloon.Common;
using WeatherBalloon.Messaging;
using WeatherBalloon.BalloonModule;

namespace BalloonModule.Test
{
    public class BalloonUnitTests
    {
        [Fact]
        public void ReceiveGPSMessageTest()
        {
            // arrange
            var gpsMessage = CreateGPSMessage();

            var balloonModule = new WeatherBalloon.BalloonModule.BalloonModule();
            
            // act 
            balloonModule.Receive(gpsMessage);

            // verify
            balloonModule.Location.track.ShouldBe(gpsMessage.Location.track);
            balloonModule.Location.@long.ShouldBe(gpsMessage.Location.@long);
            balloonModule.Location.lat.ShouldBe(gpsMessage.Location.lat);
            balloonModule.Location.mode.ShouldBe(gpsMessage.Location.mode);
            balloonModule.Location.time.ShouldBe(gpsMessage.Location.time);
            balloonModule.Location.speed.ShouldBe(gpsMessage.Location.speed);
            balloonModule.Location.climb.ShouldBe(gpsMessage.Location.climb);
        }

        [Theory]
        [InlineData(WeatherBalloon.BalloonModule.BalloonModule.RiseThreshold, WeatherBalloon.BalloonModule.BalloonModule.GroundAltitudeThreshold -1, BalloonState.PreLaunch)]
        [InlineData(0, WeatherBalloon.BalloonModule.BalloonModule.GroundAltitudeThreshold, BalloonState.PreLaunch)]
        [InlineData(WeatherBalloon.BalloonModule.BalloonModule.RiseThreshold, WeatherBalloon.BalloonModule.BalloonModule.GroundAltitudeThreshold , BalloonState.Rising)]
         public void BalloonRisingStateDetection(double climb, double altitude, BalloonState expectedState)
        {
            // arrange
            var fakeModuleClient = new FakeModuleClient();

            var gpsMessage = CreateGPSMessage();

            // set gps to indicate rising balloon state
            gpsMessage.Location.climb = climb;
            gpsMessage.Location.alt = altitude;
            
            var balloonModule = new WeatherBalloon.BalloonModule.BalloonModule();


            // act
            balloonModule.Receive(gpsMessage);
            balloonModule.Transmit(fakeModuleClient);

            // verify

            // Sent a Balloon message?
            fakeModuleClient.SentMessages.Count.ShouldBe(1);

             // Sent on correct output?
            fakeModuleClient.SentMessages[0].Item1.ShouldBe(WeatherBalloon.BalloonModule.BalloonModule.BalloonOutputName);

            // Correct message?
            var rawMessage = fakeModuleClient.SentMessages[0].Item2;
            var balloonMessage = MessageHelper.ParseMessage<BalloonMessage>(rawMessage);

            // balloon location
            balloonMessage.Location.track.ShouldBe(gpsMessage.Location.track);
            balloonMessage.Location.@long.ShouldBe(gpsMessage.Location.@long);
            balloonMessage.Location.lat.ShouldBe(gpsMessage.Location.lat);
            balloonMessage.Location.mode.ShouldBe(gpsMessage.Location.mode);
            balloonMessage.Location.time.ShouldBe(gpsMessage.Location.time);
            balloonMessage.Location.speed.ShouldBe(gpsMessage.Location.speed);
            balloonMessage.Location.climb.ShouldBe(gpsMessage.Location.climb);

            if (expectedState == BalloonState.Rising)
            {
                balloonMessage.AveAscent.ShouldBe(gpsMessage.Location.climb);
                balloonMessage.AveDescent.ShouldBe(0.0);
            }
            else if (expectedState == BalloonState.Falling)
            {
                balloonMessage.AveAscent.ShouldBe(0.0);
                balloonMessage.AveDescent.ShouldBe(gpsMessage.Location.climb);
            }

            balloonMessage.State.ShouldBe(expectedState);
        }


        [Theory]
        [InlineData(WeatherBalloon.BalloonModule.BalloonModule.RiseThreshold, 
                    WeatherBalloon.BalloonModule.BalloonModule.GroundAltitudeThreshold -1, 
                    BalloonState.Rising)]
        [InlineData(0, 
                    WeatherBalloon.BalloonModule.BalloonModule.GroundAltitudeThreshold, 
                    BalloonState.Rising)]
        [InlineData(WeatherBalloon.BalloonModule.BalloonModule.FallingThreshold, 
                    WeatherBalloon.BalloonModule.BalloonModule.GroundAltitudeThreshold, 
                    BalloonState.Falling)]
        public void BalloonFallingStateDetection(double climb, double altitude, BalloonState expectedState)
        {
            // arrange
            var fakeModuleClient = new FakeModuleClient();

            var gpsMessage = CreateGPSMessage();

            // set gps to indicate rising balloon state
            gpsMessage.Location.climb = climb;
            gpsMessage.Location.alt = altitude;
            
            var balloonModule = new WeatherBalloon.BalloonModule.BalloonModule();

            // set the balloon state to Rising, that's the only transition to Falling.
            balloonModule.BalloonState = BalloonState.Rising;


            // act
            balloonModule.Receive(gpsMessage);
            balloonModule.Transmit(fakeModuleClient);

            // verify

            // Sent a Balloon message?
            fakeModuleClient.SentMessages.Count.ShouldBe(1);

             // Sent on correct output?
            fakeModuleClient.SentMessages[0].Item1.ShouldBe(WeatherBalloon.BalloonModule.BalloonModule.BalloonOutputName);

            // Correct message?
            var rawMessage = fakeModuleClient.SentMessages[0].Item2;
            var balloonMessage = MessageHelper.ParseMessage<BalloonMessage>(rawMessage);

            // balloon location
            balloonMessage.Location.track.ShouldBe(gpsMessage.Location.track);
            balloonMessage.Location.@long.ShouldBe(gpsMessage.Location.@long);
            balloonMessage.Location.lat.ShouldBe(gpsMessage.Location.lat);
            balloonMessage.Location.mode.ShouldBe(gpsMessage.Location.mode);
            balloonMessage.Location.time.ShouldBe(gpsMessage.Location.time);
            balloonMessage.Location.speed.ShouldBe(gpsMessage.Location.speed);
            balloonMessage.Location.climb.ShouldBe(gpsMessage.Location.climb);

            if (expectedState == BalloonState.Rising)
            {
                balloonMessage.AveAscent.ShouldBe(gpsMessage.Location.climb);
                balloonMessage.AveDescent.ShouldBe(0.0);
            }
            else if (expectedState == BalloonState.Falling)
            {
                balloonMessage.AveAscent.ShouldBe(0.0);
                balloonMessage.AveDescent.ShouldBe(gpsMessage.Location.climb);
            }

            balloonMessage.State.ShouldBe(expectedState);
        }

        [Theory]
        // rising, not burst
        [InlineData(BalloonState.Rising, 
                    WeatherBalloon.BalloonModule.BalloonModule.RiseThreshold,
                    10000,
                    WeatherBalloon.BalloonModule.BalloonModule.PredictedBurstAltitude)]
        // rising, then falling
        [InlineData(BalloonState.Rising, 
                    WeatherBalloon.BalloonModule.BalloonModule.FallingThreshold,
                    10000, 
                    10000)]
        // falling, continue falling
        [InlineData(BalloonState.Falling, 
                    WeatherBalloon.BalloonModule.BalloonModule.FallingThreshold,
                    10000, 
                    WeatherBalloon.BalloonModule.BalloonModule.PredictedBurstAltitude)]
        public void BalloonBurstDetection(BalloonState initialState, double climb, double altitude, double expectedBurstAltitude)
        {
            // arrange
            var fakeModuleClient = new FakeModuleClient();

            var gpsMessage = CreateGPSMessage();

            // set gps to indicate rising balloon state
            gpsMessage.Location.climb = climb;
            gpsMessage.Location.alt = altitude;
            
            var balloonModule = new WeatherBalloon.BalloonModule.BalloonModule();

            balloonModule.BalloonState = initialState;

            // act
            balloonModule.Receive(gpsMessage);
            balloonModule.Transmit(fakeModuleClient);

            // verify

            // Sent a Balloon message?
            fakeModuleClient.SentMessages.Count.ShouldBe(1);

            // Correct message?
            var rawMessage = fakeModuleClient.SentMessages[0].Item2;
            var balloonMessage = MessageHelper.ParseMessage<BalloonMessage>(rawMessage);

            balloonMessage.BurstAltitude.ShouldBe(expectedBurstAltitude);
        }
    

        private GPSMessage CreateGPSMessage()
        {
            Random random = new Random();

            return new GPSMessage()
            {
                Location = new GPSLocation() { 
                    track = random.NextDouble(),
                    @long = random.NextDouble(),
                    lat = random.NextDouble(),
                    mode = 0, 
                    time = DateTime.UtcNow.ToString(),
                    speed = random.NextDouble(), 
                    climb = random.NextDouble()
                }
            };
        }
    }
}
