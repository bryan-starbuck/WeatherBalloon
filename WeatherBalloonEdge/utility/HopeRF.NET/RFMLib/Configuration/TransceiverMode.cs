namespace RFMLib.Configuration
{
    public enum TransceiverMode
    {
        Sleep = 0,
        StandBy = 1,
        FrequencySynthesisTransmit = 2,
        Transmit = 3,
        FrequencySynthesisReceive = 4,
        ReceiveContinuous = 5,
        ReceiveSingle = 6,
        ChannelActivityDetection = 7
    }
}