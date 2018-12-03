namespace RFMLib
{
    public interface ITransceiverSpiConnection
    {
        void Reset();
        void WriteRegister(byte address, byte value);
        byte ReadRegister(byte address);
    }
}