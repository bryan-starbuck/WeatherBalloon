using System;
using System.Linq;

namespace RFMLib.Configuration
{
    public class TransceiverRegistry
    {
        private readonly ITransceiverSpiConnection connection;

        public TransceiverRegistry(ITransceiverSpiConnection connection, byte address)
        {
            this.connection = connection;
            this.address = address;

            this.Read();
        }


        private readonly byte address;
        private byte dataByte;
        private byte initialDataByte;

        public bool HasChanges
        {
            get
            {
                return this.dataByte != this.initialDataByte;
            }
        }

        public void SetBit(int position, bool value)
        {
            int bit = (1 << position);
            int result = (this.Value & (0xFF - bit));

            if (value)
            {
                result = result | bit;
            }

            this.Value = (byte) result;
        }

        public bool GetBit(int position)
        {
            int bit = (1 << position);

            return (this.Value & bit) == bit;
        }

        public byte Value
        {
            get
            {
                return this.dataByte;
            }
            set
            {
                this.dataByte = value;
            }
        }

        public virtual void Read()
        {
            this.dataByte = this.connection.ReadRegister(this.address);
            this.initialDataByte = this.dataByte;
        }

        public virtual void Write()
        {
            if (!this.HasChanges)
            {
                return;
            }

            this.connection.WriteRegister(this.address, this.dataByte);
            this.initialDataByte = this.dataByte;
        }

        public virtual void Write(byte value)
        {
            this.connection.WriteRegister(this.address, value);
            this.initialDataByte = this.dataByte = value;
        }
    }
}