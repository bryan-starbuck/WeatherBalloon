using System;
using System.Diagnostics;

namespace WeatherBalloon.LoraModule
{
    /// <summary>
    /// Wrapper around calling the RadioHead Rf95 C++ as a executable
    /// </summary>
    public class RF95_Client 
    {
        public const string TransmitCommand = "rf95_client";
        public const string ReceiverCommand = "rf95_server";

        public static bool Transmit (string message)
        {
            try 
            {
                Console.WriteLine($"Commandline: {TransmitCommand} \"{message}\"");
                var process = Process.Start(TransmitCommand, $"\"{message}\"");

                process.WaitForExit();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed transmit: "+ ex.Message);
                return false;
            }
        }


        public static string Receive()
        {
            try 
            {
                Console.WriteLine($"Receiver Commandline: {ReceiverCommand}");

                Process process = new Process();  
                process.StartInfo.UseShellExecute = false;  
                process.StartInfo.RedirectStandardOutput = true;  
                process.StartInfo.FileName = ReceiverCommand;  
                process.Start();      

                string result = process.StandardOutput.ReadToEnd();   
                process.WaitForExit();
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed receiver process: "+ ex.Message);
                return null;
            }

        }


    }



}
