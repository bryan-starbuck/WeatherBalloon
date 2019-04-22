using System;
using System.IO.Ports;

namespace SerialPortDemo
{

    /// <summary>
    ///  Just some test code to prove the serial port can rx and tx.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            // Get a list of serial port names.
            string[] ports = SerialPort.GetPortNames();
            Console.WriteLine("The following serial ports were found:");
            
            // Display each port name to the console.
            foreach(string port in ports)
            {
                Console.WriteLine(port);
            }

            var serialPortTransmit = new SerialPort();
            serialPortTransmit.PortName = "COM6";
            serialPortTransmit.BaudRate = 115200;
            serialPortTransmit.ReadTimeout = 1500;
            serialPortTransmit.WriteTimeout = 1500;

            //serialPortTransmit.Open();


            var serialPortRecieve = new SerialPort();
            serialPortRecieve.PortName = "COM4";
            serialPortRecieve.BaudRate = 115200;
            serialPortRecieve.RtsEnable = true;
            //serialPortRecieve.ReadTimeout = 1500;
            //serialPortRecieve.WriteTimeout = 1500;

            serialPortRecieve.DataReceived += new SerialDataReceivedEventHandler(DataReceived);
            
            serialPortRecieve.Open();

            // while (true)
            // {
            //     try
            //     {
            //         var c = serialPortRecieve.ReadChar();
            //         Console.WriteLine(c);

            //         //string message = serialPortRecieve.ReadLine();
            //         //Console.WriteLine(message);
            //     }
            //     catch (TimeoutException) { }
            // }
                

            //serialPortTransmit.WriteLine("Test");

            //var message = serialPortRecieve.ReadLine();

            //Console.WriteLine("Read: " + message);

            //serialPort.Close();        

            Console.ReadLine();
            //serialPortRecieve.Close();
        }

        private static void DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            string indata = sp.ReadExisting();
            Console.WriteLine("Data Received:");
            Console.Write(indata);
        }
    }
}
