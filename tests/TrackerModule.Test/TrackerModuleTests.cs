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
        public void ReceiveGPSMessageTest()
        {
            // arrange
            var gpsMessage = CreateGPSMessage();

            var trackerModule = new WeatherBalloon.TrackerModule.TrackerModule();
            
            // act 
            trackerModule.Receive(gpsMessage);

            // verify
            trackerModule.Location.track.ShouldBe(gpsMessage.Location.track);
            trackerModule.Location.@long.ShouldBe(gpsMessage.Location.@long);
            trackerModule.Location.lat.ShouldBe(gpsMessage.Location.lat);
            trackerModule.Location.mode.ShouldBe(gpsMessage.Location.mode);
            trackerModule.Location.time.ShouldBe(gpsMessage.Location.time);
            trackerModule.Location.speed.ShouldBe(gpsMessage.Location.speed);
            trackerModule.Location.climb.ShouldBe(gpsMessage.Location.climb);
        }

        [Fact]
        public void ReceiveBalloonMessage()
        {
            // arrange
            var fakeModuleClient = new FakeModuleClient();

            var gpsMessage = CreateGPSMessage();
            var balloonMessage = CreateBalloonMessage();

            var trackerModule = new WeatherBalloon.TrackerModule.TrackerModule();

            // act 
            trackerModule.Receive(gpsMessage);
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
            receivedTrackerMessage.Location.track.ShouldBe(gpsMessage.Location.track);
            receivedTrackerMessage.Location.@long.ShouldBe(gpsMessage.Location.@long);
            receivedTrackerMessage.Location.lat.ShouldBe(gpsMessage.Location.lat);
            receivedTrackerMessage.Location.mode.ShouldBe(gpsMessage.Location.mode);
            receivedTrackerMessage.Location.time.ShouldBe(gpsMessage.Location.time);
            receivedTrackerMessage.Location.speed.ShouldBe(gpsMessage.Location.speed);
            receivedTrackerMessage.Location.climb.ShouldBe(gpsMessage.Location.climb);
            receivedTrackerMessage.FlightId.ShouldBe(balloonMessage.FlightId);
        }

        [Fact]
        public void TransmitError()
        {
            // arrange
            var fakeModuleClient = A.Fake<IModuleClient>();
            A.CallTo(fakeModuleClient).Throws(new Exception("Fake exception generated for testing"));

            var gpsMessage = CreateGPSMessage();
            var balloonMessage = CreateBalloonMessage();

            var trackerModule = new WeatherBalloon.TrackerModule.TrackerModule();

            // act 
            trackerModule.Receive(gpsMessage);
            var task = trackerModule.Receive(balloonMessage, fakeModuleClient);

            // verify
            task.Result.ShouldBe(false);
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
