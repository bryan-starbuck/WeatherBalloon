using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;


namespace WeatherBalloon.Common
{
    public interface IModuleClient
    {
        Task SendEventAsync(string outputName, Message message);
    }
}