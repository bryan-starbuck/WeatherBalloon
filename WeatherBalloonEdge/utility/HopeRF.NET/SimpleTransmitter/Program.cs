using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RFMLib;

namespace SimpleTransmitter
{
    class Program
    {
        static void Main(string[] args)
        {
            TrancieverConnectionFactory trancieverConnectionFactory = new TrancieverConnectionFactory();
            ITransceiverSpiConnection spiConnection = trancieverConnectionFactory.CreateForDragino();

            RFM9XLoraTransceiver rfm9XLoraTransceiver = new RFM9XLoraTransceiver(spiConnection);
            rfm9XLoraTransceiver.Initialize();

            while (true)
            {
                Console.Write("Sending..");

                Task<bool> transmitTask = rfm9XLoraTransceiver.Transmit(Encoding.ASCII.GetBytes("All your base are belogn to us!"));
                transmitTask.Wait();

                Console.WriteLine("OK");

                Thread.Sleep(1000);
            }
        }
    }
}
