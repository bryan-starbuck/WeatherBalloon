using System;
using System.IO.Ports;

using WeatherBalloon.Messaging;

namespace WeatherBalloon.SerialModule
{

    public class SerialModule
    {
        private const char StartMessage = '*';
        private const char EndMessage = '~';

        private const char TimeField = 't';
        private const char StateField = 's';
        private const char FlightField = 'f';


        private object TransmitLock = new object();

        private static SerialPort serialPort;

        private BalloonMessage currentMessage;

        public SerialModule()
        {


        }

        public void Initialize()
        {
            string[] ports = SerialPort.GetPortNames();             
            Console.WriteLine("The following serial ports were found:");
            // Display each port name to the console.             
            foreach(string port in ports)
            {
                Console.WriteLine(port); 
            }

            serialPort = new SerialPort(port, 115200);

            //serialPort = new SerialPortInput();

            // Listen to Serial Port events
            // serialPort.ConnectionStatusChanged += delegate(object sender, ConnectionStatusChangedEventArgs args) 
            // {
            //     Console.WriteLine("Connected = {0}", args.Connected);
            // };

            // serialPort.MessageReceived += delegate(object sender, MessageReceivedEventArgs args)
            // {
                
            //     ProcessSerialMessage(BitConverter.ToString(args.Data));
            // };

            // Set port options
            //serialPort.SetPort("/dev/ttyUSB0", 115200);

            // Connect the serial port
            //serialPort.Connect();
        }

        public void OnReceive(BalloonMessage message)
        {
            lock (TransmitLock)
            {
                TransmitSerialMessage(StartMessage.ToString());
                TransmitSerialMessage(String.Format("{0}=|{1}", TimeField, message.Timestamp));
                TransmitSerialMessage(String.Format("{0}=|{1}", FlightField.ToString(), message.FlightId));
                TransmitSerialMessage(String.Format("{0}=|{1}", StateField.ToString(), message.State));
                TransmitSerialMessage(EndMessage.ToString());
            }


        }

        private void ProcessSerialMessage(string message)
        {
            Console.WriteLine("Received message: {0}", message);

            var parameters = message.Split("=");
            string key = parameters[0];
            string value = parameters[1];

            switch (key[0])
            {
                case StartMessage : currentMessage = new BalloonMessage();
                    break;
                
                // case StateField : currentMessage.State = value;
                //     break;
                case FlightField : currentMessage.FlightId = value;
                    break;
                //case TimeField : currentMessage.Timestamp = new DateTime(value);
                //    break;

                case EndMessage :  SendBalloonMessage (currentMessage);
                    break;
            }
        }

        private void TransmitSerialMessage(string message)
        {
            //serialPort.SendMessage(message);
        }

        private void SendBalloonMessage(BalloonMessage balloonMessage)
        {
            

        }

    }
}

