using Raspberry.IO.GeneralPurpose;
using Raspberry.IO.Interop;
using Raspberry.IO.SerialPeripheralInterface;
using RFMLib.Connections;

namespace RFMLib
{
    public class TrancieverConnectionFactory
    {
        public ITransceiverSpiConnection CreateForDragino(string spiPath = "/dev/spidev0.0")
        {
            return this.Create(ConnectorPin.P1Pin22, ConnectorPin.P1Pin13, spiPath);
        }

        public ITransceiverSpiConnection Create(ConnectorPin slaveSelectPin, ConnectorPin resetPin, string spiPath = "/dev/spidev0.0")
        {
            NativeSpiConnection spiConnection = new NativeSpiConnection(new SpiControlDevice(new UnixFile(spiPath, UnixFileMode.ReadWrite)));
            spiConnection.SetDelay(0);
            spiConnection.SetMaxSpeed(500000);
            spiConnection.SetBitsPerWord(8);

            IGpioConnectionDriver driver = GpioConnectionSettings.DefaultDriver;

            return new TransceiverSpiConnection(spiConnection, driver.Out(slaveSelectPin), driver.Out(resetPin));
        }
    }
}