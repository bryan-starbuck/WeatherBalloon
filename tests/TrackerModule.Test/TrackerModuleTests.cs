using System;
using Xunit;
using Shouldly;
using FakeItEasy;
using System.Threading.Tasks;

using WeatherBalloon.TrackerModule;
using WeatherBalloon.Messaging;
using WeatherBalloon.Common;

namespace TrackerModule.Test
{
    public class TrackerModuleTests
    {
        [Fact]
        public void ReceiveTelemetryMessageTest()
        {
            // arrange
            var telemetryMessage = CreateTelemetryMessage();

            var trackerModule = new WeatherBalloon.TrackerModule.TrackerModule();
            
            // act 
            trackerModule.Receive(telemetryMessage);

            // verify
            trackerModule.Location.track.ShouldBe(telemetryMessage.track);
            trackerModule.Location.@long.ShouldBe(telemetryMessage.@long);
            trackerModule.Location.lat.ShouldBe(telemetryMessage.lat);
            trackerModule.Location.mode.ShouldBe(telemetryMessage.mode);
            trackerModule.Location.time.ShouldBe(telemetryMessage.time);
            trackerModule.Location.speed.ShouldBe(telemetryMessage.speed);
            trackerModule.Location.climb.ShouldBe(telemetryMessage.climb);
        }

        [Fact]
        public void ReceiveBalloonMessage()
        {
            // arrange
            var fakeModuleClient = new FakeModuleClient();

            var telemetryMessage = CreateTelemetryMessage();
            var balloonMessage = CreateBalloonMessage();

            var trackerModule = new WeatherBalloon.TrackerModule.TrackerModule();

            // act 
            trackerModule.Receive(telemetryMessage);
            var task = trackerModule.Receive(balloonMessage, fakeModuleClient);

            // verify
            task.Result.ShouldBe(true);

            fakeModuleClient.SentMessages.Count.ShouldBe(2);

            // Sent on correct output?
            fakeModuleClient.SentMessages[0].Item1.ShouldBe(WeatherBalloon.TrackerModule.TrackerModule.TrackerOutputName);
            
            // Correct message?
            var rawMessage0 = fakeModuleClient.SentMessages[0].Item2;
            var receivedBalloonMessage = MessageHelper.ParseMessage<BalloonMessage>(rawMessage0);

            var rawMessage1 = fakeModuleClient.SentMessages[1].Item2;
            var receivedTrackerMessage = MessageHelper.ParseMessage<TrackerMessage>(rawMessage1);

            // balloon message fields
            receivedBalloonMessage.Location.track.ShouldBe(balloonMessage.Location.track);
            receivedBalloonMessage.Location.@long.ShouldBe(balloonMessage.Location.@long);
            receivedBalloonMessage.Location.lat.ShouldBe(balloonMessage.Location.lat);
            receivedBalloonMessage.Location.mode.ShouldBe(balloonMessage.Location.mode);
            receivedBalloonMessage.Location.time.ShouldBe(balloonMessage.Location.time);
            receivedBalloonMessage.Location.speed.ShouldBe(balloonMessage.Location.speed);
            receivedBalloonMessage.Location.climb.ShouldBe(balloonMessage.Location.climb);
            receivedBalloonMessage.State.ShouldBe(balloonMessage.State);
            receivedBalloonMessage.AveAscent.ShouldBe(balloonMessage.AveAscent);
            receivedBalloonMessage.AveDescent.ShouldBe(balloonMessage.AveDescent);
            receivedBalloonMessage.FlightId.ShouldBe(balloonMessage.FlightId);
            receivedBalloonMessage.DeviceName.ShouldBe("Weather Balloon");

            // tracker message fields
            receivedTrackerMessage.Location.track.ShouldBe(telemetryMessage.track);
            receivedTrackerMessage.Location.@long.ShouldBe(telemetryMessage.@long);
            receivedTrackerMessage.Location.lat.ShouldBe(telemetryMessage.lat);
            receivedTrackerMessage.Location.mode.ShouldBe(telemetryMessage.mode);
            receivedTrackerMessage.Location.time.ShouldBe(telemetryMessage.time);
            receivedTrackerMessage.Location.speed.ShouldBe(telemetryMessage.speed);
            receivedTrackerMessage.Location.climb.ShouldBe(telemetryMessage.climb);
            receivedTrackerMessage.FlightId.ShouldBe(balloonMessage.FlightId);
        }

        [Fact]
        public void TransmitError()
        {
            // arrange
            var fakeModuleClient = A.Fake<IModuleClient>();
            A.CallTo(fakeModuleClient).Throws(new Exception("Fake exception generated for testing"));

            var telemetryMessage = CreateTelemetryMessage();
            var balloonMessage = CreateBalloonMessage();

            var trackerModule = new WeatherBalloon.TrackerModule.TrackerModule();

            // act 
            trackerModule.Receive(telemetryMessage);
            var task = trackerModule.Receive(balloonMessage, fakeModuleClient);

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
                climb = random.NextDouble()
            };
        }


        private BalloonMessage CreateBalloonMessage()
        {
            Random random = new Random();

            return new BalloonMessage()
            {
                Location = new GPSLocation() { 
                    track = random.NextDouble(),
                    @long = random.NextDouble(),
                    lat = random.NextDouble(),
                    mode = 0, 
                    time = DateTime.UtcNow.ToString(),
                    speed = random.NextDouble(), 
                    climb = random.NextDouble()
                },
                State = BalloonState.PreLaunch,
                AveAscent = 0.5,
                AveDescent = -0.1,
                BurstAltitude = 90000, 
                FlightId = "Super Test!"
            };
        }
    }
}
