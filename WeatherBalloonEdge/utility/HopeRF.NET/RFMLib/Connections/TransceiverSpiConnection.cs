using System.Threading;
using Raspberry.IO;
using Raspberry.IO.SerialPeripheralInterface;

namespace RFMLib.Connections
{
    public class TransceiverSpiConnection : ITransceiverSpiConnection
    {
        private readonly INativeSpiConnection spiConnection;
        private readonly IOutputBinaryPin ssPin;
        private readonly IOutputBinaryPin resetPin;

        public TransceiverSpiConnection(INativeSpiConnection spiConnection, IOutputBinaryPin ssPin, IOutputBinaryPin resetPin)
        {
            this.spiConnection = spiConnection;
            this.ssPin = ssPin;
            this.resetPin = resetPin;
        }

        public void Reset()
        {
            this.resetPin.Write(false);
            Thread.Sleep(100);

            this.resetPin.Write(true);
            Thread.Sleep(100);
        }

        public void WriteRegister(byte address, byte value)
        {
            this.ssPin.Write(false);

            SpiTransferBuffer buffer = new SpiTransferBuffer(2, SpiTransferMode.ReadWrite);
            buffer.Delay = 0;
            buffer.Speed = 500000;
            buffer.Tx.Write(0, (byte)(address | 0x80));
            buffer.Tx.Write(1, value);

            this.spiConnection.Transfer(buffer);

            this.ssPin.Write(true);
        }

        public byte ReadRegister(byte address)
        {
            this.ssPin.Write(false);

            SpiTransferBuffer buffer = new SpiTransferBuffer(2, SpiTransferMode.ReadWrite);
            buffer.Delay = 0;
            buffer.Speed = 500000;
            buffer.Tx.Write(0, (byte)(address & 0x7F));

            this.spiConnection.Transfer(buffer);
            this.ssPin.Write(true);

            return buffer.Rx.Read(1);
        }
    }
}