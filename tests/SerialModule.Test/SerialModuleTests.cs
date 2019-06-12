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

        [Fact(DisplayName = "DisplayName should be displayed")]
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

            var diffLat = Math.Abs(serializedBalloonMessage.Location.lat - Math.Round(balloonMessage.Location.lat, 5));
            diffLat.ShouldBeLessThan(0.0001);

            var diffLong = Math.Abs(serializedBalloonMessage.Location.@long - Math.Round(balloonMessage.Location.@long, 5));
            diffLong.ShouldBeLessThan(0.0001);

            serializedBalloonMessage.Location.alt.ShouldBe(Math.Round(balloonMessage.Location.alt, 2));
            serializedBalloonMessage.Temperature.ShouldBe(balloonMessage.Temperature);
            serializedBalloonMessage.Pressure.ShouldBe(balloonMessage.Pressure);
            serializedBalloonMessage.Humidity.ShouldBe(balloonMessage.Humidity);
            serializedBalloonMessage.Location.climb.ShouldBe(Math.Round(balloonMessage.Location.climb, 2));
            serializedBalloonMessage.Location.speed.ShouldBe(Math.Round(balloonMessage.Location.speed, 1));
        }

        [Fact]
        public void SignalStrengthTest()
        {
            // arrange
            var balloonMessage = CreateBalloonMessage();
            

            // act
            var compactMessageString = balloonMessage.ToCompactMessage();
            compactMessageString += ":-97.1";
            output.WriteLine("Compact: "+compactMessageString);
            var jsonMessage = JsonConvert.SerializeObject(balloonMessage);
            output.WriteLine(jsonMessage);
            

            var serializedBalloonMessage = BalloonMessage.FromCompactMessage(compactMessageString);
            var jsonResultMessage = JsonConvert.SerializeObject(serializedBalloonMessage);
            output.WriteLine(jsonResultMessage);
            

            // verify
            serializedBalloonMessage.SignalStrength.ShouldBe(-97.1);
        }

        // [Fact]
        // public void ParseTestMessage()
        // {
        //     arrange
        //     var compactMessageString = "155605809220.020.00030000213.37008068.07469000352.50126.825.6969.6Attractive dart:-74";

        //     When
        //     var serializedBalloonMessage = BalloonMessage.FromCompactMessage(compactMessageString);
        //     var jsonResultMessage = JsonConvert.SerializeObject(serializedBalloonMessage);
        //     output.WriteLine(jsonResultMessage);
        
        //     Then
        //     10.ShouldBe(1);
        // }


        private BalloonMessage CreateBalloonMessage()
        {
            Random random = new Random();

            return new BalloonMessage()
            {
                Location = new GPSLocation() { 
                    track = random.NextDouble(),
                    @long = Math.Round(random.NextDouble(), 5),
                    lat = Math.Round(random.NextDouble(), 5),
                    mode = 0, 
                    time = DateTime.UtcNow.ToString(),
                    speed = random.NextDouble(), 
                    climb = random.NextDouble(),
                    alt = 101010.55
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
