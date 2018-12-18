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

        [Fact]
        public void TransmitGPSMessageTest()
        {
            // arrange
            var fakeModuleClient = A.Fake<IModuleClient>();

            var gpsMessage = CreateGPSMessage();
            
            var balloonModule = new WeatherBalloon.BalloonModule.BalloonModule();


            // act
            balloonModule.Receive(gpsMessage);
            balloonModule.Transmit(fakeModuleClient);

            //Console.WriteLine($"GPS: {JsonConvert.SerializeObject(gpsMessage)}");

            // verify
            A.CallTo(() => fakeModuleClient.SendEventAsync(WeatherBalloon.BalloonModule.BalloonModule.BalloonOutputName, 
                A<Microsoft.Azure.Devices.Client.Message>.Ignored))
                .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => fakeModuleClient.SendEventAsync(WeatherBalloon.BalloonModule.BalloonModule.BalloonOutputName, 
                A<Microsoft.Azure.Devices.Client.Message>.That.Matches(msg => 
                DeserializeBytes<BalloonMessage>(msg.GetBytes()).Location.track.Equals(gpsMessage.Location.track))))
                .MustHaveHappened();
            A.CallTo(() => fakeModuleClient.SendEventAsync(WeatherBalloon.BalloonModule.BalloonModule.BalloonOutputName, 
                A<Microsoft.Azure.Devices.Client.Message>.That.Matches(msg => 
                DeserializeBytes<BalloonMessage>(msg.GetBytes()).Location.lat.Equals(gpsMessage.Location.lat))))
                .MustHaveHappened();     
                
        }

        private T DeserializeBytes<T>(byte[] bytes)
        {
            var encodedString = Encoding.UTF8.GetString(bytes);

            Console.WriteLine(encodedString);
            BalloonMessage deserialized = JsonConvert.DeserializeObject<BalloonMessage>(encodedString);
            Console.WriteLine(deserialized.Location.track);

            return JsonConvert.DeserializeObject<T>(encodedString);
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
