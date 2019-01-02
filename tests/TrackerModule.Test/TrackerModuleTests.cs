using System;
using Xunit;
using Shouldly;
using FakeItEasy;

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
            trackerModule.Receive(balloonMessage, fakeModuleClient);

            // verify
            fakeModuleClient.SentMessages.Count.ShouldBe(1);

            // Sent on correct output?
            fakeModuleClient.SentMessages[0].Item1.ShouldBe(WeatherBalloon.TrackerModule.TrackerModule.TrackerOutputName);
            
            // Correct message?
            var rawMessage = fakeModuleClient.SentMessages[0].Item2;
            var trackerMessage = MessageHelper.ParseMessage<TrackerMessage>(rawMessage);

            // tracker location
            trackerMessage.TrackerLocation.track.ShouldBe(gpsMessage.Location.track);
            trackerMessage.TrackerLocation.@long.ShouldBe(gpsMessage.Location.@long);
            trackerMessage.TrackerLocation.lat.ShouldBe(gpsMessage.Location.lat);
            trackerMessage.TrackerLocation.mode.ShouldBe(gpsMessage.Location.mode);
            trackerMessage.TrackerLocation.time.ShouldBe(gpsMessage.Location.time);
            trackerMessage.TrackerLocation.speed.ShouldBe(gpsMessage.Location.speed);
            trackerMessage.TrackerLocation.climb.ShouldBe(gpsMessage.Location.climb);

            // balloon message fields
            trackerMessage.BalloonLocation.track.ShouldBe(balloonMessage.Location.track);
            trackerMessage.BalloonLocation.@long.ShouldBe(balloonMessage.Location.@long);
            trackerMessage.BalloonLocation.lat.ShouldBe(balloonMessage.Location.lat);
            trackerMessage.BalloonLocation.mode.ShouldBe(balloonMessage.Location.mode);
            trackerMessage.BalloonLocation.time.ShouldBe(balloonMessage.Location.time);
            trackerMessage.BalloonLocation.speed.ShouldBe(balloonMessage.Location.speed);
            trackerMessage.BalloonLocation.climb.ShouldBe(balloonMessage.Location.climb);

            trackerMessage.State.ShouldBe(balloonMessage.State);
            trackerMessage.AveAscent.ShouldBe(balloonMessage.AveAscent);
            trackerMessage.AveDescent.ShouldBe(balloonMessage.AveDescent);

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
                BurstAltitude = 90000
            };
        }
    }
}
