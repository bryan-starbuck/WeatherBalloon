using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RFMLib;

namespace ChatApp
{
    class Program
    {
        static void Main(string[] args)
        {
            TrancieverConnectionFactory trancieverConnectionFactory = new TrancieverConnectionFactory();
            ITransceiverSpiConnection spiConnection = trancieverConnectionFactory.CreateForDragino();

            RFM9XLoraTransceiver rfm9XLoraTransceiver = new RFM9XLoraTransceiver(spiConnection);
            rfm9XLoraTransceiver.Initialize();

            Console.WriteLine("=============================================");
            Console.WriteLine("FrequencyConfig.Frequency=" + rfm9XLoraTransceiver.FrequencyConfig.Frequency);
            Console.WriteLine("FrequencyConfig.PaSelect=" + rfm9XLoraTransceiver.FrequencyConfig.PaSelect);
            Console.WriteLine("FrequencyConfig.Power=" + rfm9XLoraTransceiver.FrequencyConfig.Power);
            Console.WriteLine("OperationConfig.Mode=" + rfm9XLoraTransceiver.OperationConfig.Mode);
            Console.WriteLine("OperationConfig.IsLongRange = " + rfm9XLoraTransceiver.OperationConfig.IsLongRange);
            Console.WriteLine("OperationConfig.IsLowFrequencyModeOn=" + rfm9XLoraTransceiver.OperationConfig.IsLowFrequencyModeOn);
            Console.WriteLine("IRQs.CadDetected=" + rfm9XLoraTransceiver.IRQs.CadDetected);
            Console.WriteLine("IRQs.FhssChangeChannel=" + rfm9XLoraTransceiver.IRQs.FhssChangeChannel);
            Console.WriteLine("IRQs.CadDone=" + rfm9XLoraTransceiver.IRQs.CadDone);
            Console.WriteLine("IRQs.TxDone=" + rfm9XLoraTransceiver.IRQs.TxDone);
            Console.WriteLine("IRQs.ValidHeader=" + rfm9XLoraTransceiver.IRQs.ValidHeader);
            Console.WriteLine("IRQs.PayloadCrcError=" + rfm9XLoraTransceiver.IRQs.PayloadCrcError);
            Console.WriteLine("IRQs.RxDone=" + rfm9XLoraTransceiver.IRQs.RxDone);
            Console.WriteLine("IRQs.RxTimeout=" + rfm9XLoraTransceiver.IRQs.RxTimeout);
            Console.WriteLine("=============================================");
            Console.WriteLine("");
            Console.WriteLine("");

            CancellationTokenSource recieveCancellationTokenSource = null;
            object recieveTask = null;

            Console.WriteLine("Enter your name:");
            string name = Console.ReadLine();

            Console.WriteLine("Press enter to transmit, Q to quit");

            while (true)
            {
                if (recieveCancellationTokenSource == null || recieveCancellationTokenSource.IsCancellationRequested)
                {
                    recieveCancellationTokenSource = new CancellationTokenSource();

                    rfm9XLoraTransceiver.Recieve(recieveCancellationTokenSource.Token).ContinueWith(task =>
                    {
                        if (task.IsCompleted && task.Result != null)
                        {
                            var rawData = task.Result;

                            if (rawData != null)
                            {
                                Console.WriteLine(Encoding.UTF8.GetString(rawData.Buffer) + " (RSSI:" + rawData.RSSI + ")");
                            }
                        }

                        recieveCancellationTokenSource = null;
                    });
                }

                if (!Console.KeyAvailable)
                {
                    Thread.Sleep(100);
                    continue;
                }

                ConsoleKeyInfo consoleKeyInfo = Console.ReadKey();

                if (consoleKeyInfo.Key == ConsoleKey.Enter)
                {
                    recieveCancellationTokenSource?.Cancel();

                    IList<string> randomText = new List<string>()
                    {
                        "Hi",
                        "Hello",
                        "Hola",
                        "Привет",
                        "You rock!"
                    };

                    Random random = new Random();

                    string text = name + ": " + randomText[random.Next(0, randomText.Count)];
                    Console.WriteLine("Sending [" + text + "]");
                    CancellationTokenSource transmitCancellationTokenSource = new CancellationTokenSource();

                    Task<bool> transmit = rfm9XLoraTransceiver.Transmit(Encoding.UTF8.GetBytes(text), transmitCancellationTokenSource.Token);

                    if (!transmit.Wait(1000))
                    {
                        transmitCancellationTokenSource.Cancel();
                        Console.WriteLine("Send timeout");
                    }
                }
                else if (consoleKeyInfo.Key == ConsoleKey.Q)
                {
                    recieveCancellationTokenSource.Cancel();
                    Console.WriteLine("END");
                    return;
                }

                Thread.Sleep(100);
            }
        }
    }
}
