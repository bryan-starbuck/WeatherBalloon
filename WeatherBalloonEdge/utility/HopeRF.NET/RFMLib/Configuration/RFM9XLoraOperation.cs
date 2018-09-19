using System;

namespace RFMLib.Configuration
{
    public class RFM9XLoraOperation
    {
        private readonly TransceiverRegistry modeBank;

        public RFM9XLoraOperation(ITransceiverSpiConnection connection)
        {
            this.modeBank = new TransceiverRegistry(connection, 0x01);
        }

        public TransceiverMode Mode
        {
            get
            {
                return (TransceiverMode) (this.modeBank.Value & 0x07); // 00000111
            }
            set
            {
                this.modeBank.Value = (byte)((this.modeBank.Value & 0xF8) | (byte)value); // 11111000
            }
        }

        public bool IsLongRange
        {
            get
            {
                return this.modeBank.GetBit(7);
            }
            set
            {
                this.modeBank.SetBit(7, value);
            }
        }


        public bool IsLowFrequencyModeOn
        {
            get
            {
                return this.modeBank.GetBit(3);
            }
            set
            {
                this.modeBank.SetBit(3, value);
            }
        }

        public void Read()
        {
            this.modeBank.Read();
        }

        public void Write()
        {
            this.modeBank.Write();
        }
    }
}