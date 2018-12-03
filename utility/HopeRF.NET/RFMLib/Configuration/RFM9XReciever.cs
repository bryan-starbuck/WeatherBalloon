namespace RFMLib.Configuration
{
    public class RFM9XReciever
    {
        private readonly TransceiverRegistry bytesRecievedBank;
        private readonly TransceiverRegistry fifoCurrentPacketAddressBank;
        private readonly TransceiverRegistry fifoPointerAddressBank;
        private readonly TransceiverRegistry fifoBank;
        private readonly TransceiverRegistry packetRSSSIBank;
        private readonly TransceiverRegistry fifoRxBaseAddress;

        public RFM9XReciever(ITransceiverSpiConnection connection)
        {
            this.bytesRecievedBank = new TransceiverRegistry(connection, 0x13);
            this.fifoCurrentPacketAddressBank = new TransceiverRegistry(connection, 0x10);
            this.fifoPointerAddressBank = new TransceiverRegistry(connection, 0x0D);
            this.fifoBank = new TransceiverRegistry(connection, 0x00);
            this.packetRSSSIBank = new TransceiverRegistry(connection, 0x1A);
            this.fifoRxBaseAddress = new TransceiverRegistry(connection, 0x0F);
        }

        public void Reset()
        {
            this.fifoRxBaseAddress.Write(0x00);
        }

        public int GetPacketRSSI()
        {
            this.packetRSSSIBank.Read();
            return this.packetRSSSIBank.Value - 137;
        }

        public byte[] ReadPacketBuffer()
        {
            this.fifoCurrentPacketAddressBank.Read();
            this.bytesRecievedBank.Read();

            this.fifoPointerAddressBank.Write(this.fifoCurrentPacketAddressBank.Value);

            byte[] bytes = new byte[this.bytesRecievedBank.Value];

            for (int i = 0; i < this.bytesRecievedBank.Value; i++)
            {
                this.fifoBank.Read();
                bytes[i] = this.fifoBank.Value;
            }

            return bytes;
        }

        public void Write()
        {
            this.fifoPointerAddressBank.Write();
        }
    }
}