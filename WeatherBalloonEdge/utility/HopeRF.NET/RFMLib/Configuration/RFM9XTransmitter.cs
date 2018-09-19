using System.Threading;

namespace RFMLib.Configuration
{
    public class RFM9XTransmitter
    {
        private readonly TransceiverRegistry fifoPointerAddressRegistry;
        private readonly TransceiverRegistry fifoRegistry;
        private readonly TransceiverRegistry payloadLengthRegistry;
        private readonly TransceiverRegistry fifoTxBaseAddress;

        public RFM9XTransmitter(ITransceiverSpiConnection connection)
        {
            this.fifoPointerAddressRegistry = new TransceiverRegistry(connection, 0x0D);
            this.fifoRegistry = new TransceiverRegistry(connection, 0x00);
            this.payloadLengthRegistry = new TransceiverRegistry(connection, 0x22);
            this.fifoTxBaseAddress = new TransceiverRegistry(connection, 0x22);
        }

        public void WritePacketBuffer(byte[] buffer)
        {
            this.fifoPointerAddressRegistry.Write(0x00);

            foreach (byte b in buffer)
            {
                this.fifoRegistry.Write(b);
            }

            this.payloadLengthRegistry.Write((byte)buffer.Length);
        }

        public void Reset()
        {
            this.fifoTxBaseAddress.Write(0x00);
        }
    }
}