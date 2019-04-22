using System;
using System.Diagnostics;
using Xunit;
using Xunit.Abstractions;
using Shouldly;
using Newtonsoft.Json;

using WeatherBalloon.Messaging;

namespace SerialModule.Test
{
    public class SerialModuleTests
    {
        private readonly ITestOutputHelper output;

        public SerialModuleTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void CompactSerializationTest()
        {
            // arrange
            var balloonMessage = CreateBalloonMessage();
            

            // act
            var compactMessageString = balloonMessage.ToCompactMessage();
            output.WriteLine("Compact: "+compactMessageString);
            var jsonMessage = JsonConvert.SerializeObject(balloonMessage);
            output.WriteLine(jsonMessage);
            

            var serializedBalloonMessage = BalloonMessage.FromCompactMessage(compactMessageString);
            var jsonResultMessage = JsonConvert.SerializeObject(serializedBalloonMessage);
            output.WriteLine(jsonResultMessage);
            

            // verify
            serializedBalloonMessage.State.ShouldBe(balloonMessage.State);
            serializedBalloonMessage.Timestamp.ShouldBe(balloonMessage.Timestamp.AddTicks(-(balloonMessage.Timestamp.Ticks % TimeSpan.TicksPerSecond)));
            serializedBalloonMessage.AveAscent.ShouldBe(balloonMessage.AveAscent);
            serializedBalloonMessage.AveDescent.ShouldBe(balloonMessage.AveDescent);
            serializedBalloonMessage.FlightId.ShouldBe(balloonMessage.FlightId);
            serializedBalloonMessage.BurstAltitude.ShouldBe(balloonMessage.BurstAltitude);
            serializedBalloonMessage.Location.lat.ShouldBe(Math.Round(balloonMessage.Location.lat, 5));
            serializedBalloonMessage.Location.@long.ShouldBe(Math.Round(balloonMessage.Location.@long, 5));
            serializedBalloonMessage.Location.alt.ShouldBe(balloonMessage.Location.alt);
            serializedBalloonMessage.Temperature.ShouldBe(balloonMessage.Temperature);
            serializedBalloonMessage.Pressure.ShouldBe(balloonMessage.Pressure);
            serializedBalloonMessage.Humidity.ShouldBe(balloonMessage.Humidity);
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
                FlightId = "Awesome test",
                Temperature = -10,
                Humidity = 95.1,
                Pressure = 971.1
            };
        }
    }
}
