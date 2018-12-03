using System.Runtime.InteropServices;

namespace WeatherBalloon.LoraModule
{
    /// <summary>
    /// Wrapper around unmanaged C code in rf95_lib.so
    ///   !!! Currently doesn't work !!!
    /// </summary>
    public class RF95_Wrapper
    {
        [DllImport("rf95_lib")]
        public static extern bool init ();


        [DllImport("rf95_lib")]
        public static extern void Transmit (byte[] data_to_send, int length);

        [DllImport("rf95_lib")]
        public static extern void Receive (byte[] data_recieved, int length);

        [DllImport("rf95_lib")]
        public static extern void Close();



    }



}