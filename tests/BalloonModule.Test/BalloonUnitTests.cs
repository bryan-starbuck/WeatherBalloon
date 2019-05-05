using System;
using Xunit;
using Shouldly;
using FakeItEasy;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;

using WeatherBalloon.Common;
using WeatherBalloon.Messaging;
using WeatherBalloon.BalloonModule;

namespace BalloonModule.Test
{
    public class BalloonUnitTests
    {
        [Fact]
        public void ReceiveTelemetryMessageTest()
        {
            // arrange
            var telemetryMessage = CreateTelemetryMessage();

            var balloonModule = new WeatherBalloon.BalloonModule.BalloonModule();
            
            // act 
            balloonModule.Receive(telemetryMessage);

            // verify
            balloonModule.Location.track.ShouldBe(telemetryMessage.track);
            balloonModule.Location.@long.ShouldBe(telemetryMessage.@long);
            balloonModule.Location.lat.ShouldBe(telemetryMessage.lat);
            balloonModule.Location.mode.ShouldBe(telemetryMessage.mode);
            balloonModule.Location.time.ShouldBe(telemetryMessage.time);
            balloonModule.Location.speed.ShouldBe(telemetryMessage.speed);
            balloonModule.Location.climb.ShouldBe(telemetryMessage.climb);
        }

        [Theory]
        [InlineData(WeatherBalloon.BalloonModule.BalloonModule.RiseThreshold, WeatherBalloon.BalloonModule.BalloonModule.GroundAltitudeThreshold -1, BalloonState.PreLaunch)]
        [InlineData(0, WeatherBalloon.BalloonModule.BalloonModule.GroundAltitudeThreshold, BalloonState.PreLaunch)]
        [InlineData(WeatherBalloon.BalloonModule.BalloonModule.RiseThreshold, WeatherBalloon.BalloonModule.BalloonModule.GroundAltitudeThreshold , BalloonState.Rising)]
         public void BalloonRisingStateDetection(double climb, double altitude, BalloonState expectedState)
        {
            // arrange
            var fakeModuleClient = new FakeModuleClient();

            var telemetryMessage = CreateTelemetryMessage();

            // set gps to indicate rising balloon state
            telemetryMessage.climb = climb;
            telemetryMessage.alt = altitude;
            
            var balloonModule = new WeatherBalloon.BalloonModule.BalloonModule();


            // act
            balloonModule.Receive(telemetryMessage);
            var task = balloonModule.TransmitBalloonMessage(fakeModuleClient);

            // verify
            task.Result.ShouldBe(true);


            // Sent a Balloon message?
            fakeModuleClient.SentMessages.Count.ShouldBe(1);

             // Sent on correct output?
            fakeModuleClient.SentMessages[0].Item1.ShouldBe(WeatherBalloon.BalloonModule.BalloonModule.BalloonOutputName);

            // Correct message?
            var rawMessage = fakeModuleClient.SentMessages[0].Item2;
            var balloonMessage = MessageHelper.ParseMessage<BalloonMessage>(rawMessage);

            // balloon location
            balloonMessage.Location.track.ShouldBe(telemetryMessage.track);
            balloonMessage.Location.@long.ShouldBe(telemetryMessage.@long);
            balloonMessage.Location.lat.ShouldBe(telemetryMessage.lat);
            balloonMessage.Location.mode.ShouldBe(telemetryMessage.mode);
            balloonMessage.Location.time.ShouldBe(telemetryMessage.time);
            balloonMessage.Location.speed.ShouldBe(telemetryMessage.speed);
            balloonMessage.Location.climb.ShouldBe(telemetryMessage.climb);

            if (expectedState == BalloonState.Rising)
            {
                balloonMessage.AveAscent.ShouldBe(telemetryMessage.climb);
                balloonMessage.AveDescent.ShouldBe(0.0);
            }
            else if (expectedState == BalloonState.Falling)
            {
                balloonMessage.AveAscent.ShouldBe(0.0);
                balloonMessage.AveDescent.ShouldBe(telemetryMessage.climb);
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

            var telemetryMessage = CreateTelemetryMessage();

            // set gps to indicate rising balloon state
            telemetryMessage.climb = climb;
            telemetryMessage.alt = altitude;
            
            var balloonModule = new WeatherBalloon.BalloonModule.BalloonModule();

            // set the balloon state to Rising, that's the only transition to Falling.
            balloonModule.BalloonState = BalloonState.Rising;


            // act
            balloonModule.Receive(telemetryMessage);
            var task = balloonModule.TransmitBalloonMessage(fakeModuleClient);

            // verify
            task.Result.ShouldBe(true);

            // Sent a Balloon message?
            fakeModuleClient.SentMessages.Count.ShouldBe(1);

             // Sent on correct output?
            fakeModuleClient.SentMessages[0].Item1.ShouldBe(WeatherBalloon.BalloonModule.BalloonModule.BalloonOutputName);

            // Correct message?
            var rawMessage = fakeModuleClient.SentMessages[0].Item2;
            var balloonMessage = MessageHelper.ParseMessage<BalloonMessage>(rawMessage);

            // balloon location
            balloonMessage.Location.track.ShouldBe(telemetryMessage.track);
            balloonMessage.Location.@long.ShouldBe(telemetryMessage.@long);
            balloonMessage.Location.lat.ShouldBe(telemetryMessage.lat);
            balloonMessage.Location.mode.ShouldBe(telemetryMessage.mode);
            balloonMessage.Location.time.ShouldBe(telemetryMessage.time);
            balloonMessage.Location.speed.ShouldBe(telemetryMessage.speed);
            balloonMessage.Location.climb.ShouldBe(telemetryMessage.climb);
            balloonMessage.Temperature.ShouldBe(telemetryMessage.temp);
            balloonMessage.Pressure.ShouldBe(telemetryMessage.pressure);
            balloonMessage.Humidity.ShouldBe(telemetryMessage.humidity);


            if (expectedState == BalloonState.Rising)
            {
                balloonMessage.AveAscent.ShouldBe(telemetryMessage.climb);
                balloonMessage.AveDescent.ShouldBe(0.0);
            }
            else if (expectedState == BalloonState.Falling)
            {
                balloonMessage.AveAscent.ShouldBe(0.0);
                balloonMessage.AveDescent.ShouldBe(telemetryMessage.climb);
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

            var telemetryMessage = CreateTelemetryMessage();

            // set gps to indicate rising balloon state
            telemetryMessage.climb = climb;
            telemetryMessage.alt = altitude;
            
            var balloonModule = new WeatherBalloon.BalloonModule.BalloonModule();

            balloonModule.BalloonState = initialState;

            // act
            balloonModule.Receive(telemetryMessage);
            var task = balloonModule.TransmitBalloonMessage(fakeModuleClient);

            // verify
            task.Result.ShouldBe(true);

            // Sent a Balloon message?
            fakeModuleClient.SentMessages.Count.ShouldBe(1);

            // Correct message?
            var rawMessage = fakeModuleClient.SentMessages[0].Item2;
            var balloonMessage = MessageHelper.ParseMessage<BalloonMessage>(rawMessage);

            balloonMessage.BurstAltitude.ShouldBe(expectedBurstAltitude);
        }

        [Theory]
        // Falling, at ground threshold
        [InlineData(BalloonState.Falling, 
                    WeatherBalloon.BalloonModule.BalloonModule.GroundAltitudeThreshold,
                    BalloonState.Landed)]
        // Falling, above ground threshold
        [InlineData(BalloonState.Falling, 
                    WeatherBalloon.BalloonModule.BalloonModule.GroundAltitudeThreshold+1,
                    BalloonState.Falling)]
        public void BalloonLandedDetection(BalloonState initialState, double altitude, BalloonState expectedState)
        {
            // arrange
            var fakeModuleClient = new FakeModuleClient();

            var telemetryMessage = CreateTelemetryMessage();

            // set gps to indicate rising balloon state
            telemetryMessage.alt = altitude;
            
            var balloonModule = new WeatherBalloon.BalloonModule.BalloonModule();

            balloonModule.BalloonState = initialState;

            // act
            balloonModule.Receive(telemetryMessage);
            var task = balloonModule.TransmitBalloonMessage(fakeModuleClient);

            // verify
            task.Result.ShouldBe(true);

            // Sent a Balloon message?
            fakeModuleClient.SentMessages.Count.ShouldBe(1);

            // Correct message?
            var rawMessage = fakeModuleClient.SentMessages[0].Item2;
            var balloonMessage = MessageHelper.ParseMessage<BalloonMessage>(rawMessage);

            balloonMessage.State.ShouldBe(expectedState);
        }

        [Fact]
        public void TransmitError()
        {
            // arrange
            var fakeModuleClient = A.Fake<IModuleClient>();
            A.CallTo(fakeModuleClient).Throws(new Exception("Fake exception generated for testing"));

            var telemetryMessage = CreateTelemetryMessage();
            
            var balloonModule = new WeatherBalloon.BalloonModule.BalloonModule();

            // act 
            balloonModule.Receive(telemetryMessage);
            var task = balloonModule.TransmitBalloonMessage(fakeModuleClient);

            // verify
            task.Result.ShouldBe(false);
        }
    

        private TelemetryMessage CreateTelemetryMessage()
        {
            Random random = new Random();

            return new TelemetryMessage()
            {
                track = random.NextDouble(),
                @long = random.NextDouble(),
                lat = random.NextDouble(),
                mode = 0, 
                time = DateTime.UtcNow.ToString(),
                speed = random.NextDouble(), 
                climb = random.NextDouble(),
                temp = random.NextDouble(),
                humidity = random.NextDouble(),
                pressure = random.NextDouble()
            };
        }
    }
}
