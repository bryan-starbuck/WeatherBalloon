using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RFMLib;

namespace SimpleReciever
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
                Task<RawData> recieveTask = rfm9XLoraTransceiver.Recieve();

                recieveTask.Wait();

                if (recieveTask.Result != null)
                {
                    Console.WriteLine(Encoding.ASCII.GetString(recieveTask.Result.Buffer));
                }
            }
        }
    }
}
