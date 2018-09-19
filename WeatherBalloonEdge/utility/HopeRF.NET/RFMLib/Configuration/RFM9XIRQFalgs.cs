using System;

namespace RFMLib.Configuration
{
    public class RFM9XIRQFalgs
    {
        private readonly TransceiverRegistry irqFlagsBank;

        public RFM9XIRQFalgs(ITransceiverSpiConnection connection)
        {
            this.irqFlagsBank = new TransceiverRegistry(connection, 0x12);
        }

        public int Value
        {
            get { return this.irqFlagsBank.Value; }
        }

        public bool CadDetected
        {
            get
            {
                return this.irqFlagsBank.GetBit(0);
            }
        }

        public bool FhssChangeChannel
        {
            get
            {
                return this.irqFlagsBank.GetBit(1);
            }
        }

        public bool CadDone
        {
            get
            {
                return this.irqFlagsBank.GetBit(2);
            }
        }

        public bool TxDone
        {
            get
            {
                return this.irqFlagsBank.GetBit(3);
            }
        }

        public bool ValidHeader
        {
            get
            {
                return this.irqFlagsBank.GetBit(4);
            }
        }

        public bool PayloadCrcError
        {
            get
            {
                return this.irqFlagsBank.GetBit(5);
            }
        }

        public bool RxDone
        {
            get
            {
                return this.irqFlagsBank.GetBit(6);
            }
        }

        public bool RxTimeout
        {
            get
            {
                return this.irqFlagsBank.GetBit(7);
            }
        }

        public void Clear()
        {
            this.irqFlagsBank.Value = 0xFF;
            this.irqFlagsBank.Write();
            this.irqFlagsBank.Read();
        }

        public void Read()
        {
            this.irqFlagsBank.Read();
        }

    }
}