# HopeRF.NET
C# .NET/mono client for RFM95 / RFM98 / SX1276 / SX1278 LORA modules for RaspberryPi and dragino shield

## Out of the box support for Dragino GPS/Lora shield without resoldering!
![dragino](http://wiki.dragino.com/images/d/d6/Lora_GPS_HAT.png)

## Requirements
* mono 4.4.2 or newer
* enabled SPI
* wiringpi
* latest raspberry-sharp-io

## Tested with
* Raspbian Jessie
* Raspberry Pi 2
* Raspberry Pi 3

## Install from zero
coming soon!

## Simple transmitter
```cs
TrancieverConnectionFactory trancieverConnectionFactory = new TrancieverConnectionFactory();
ITransceiverSpiConnection spiConnection = trancieverConnectionFactory.CreateForDragino();

RFM9XLoraTransceiver rfm9XLoraTransceiver = new RFM9XLoraTransceiver(spiConnection);
rfm9XLoraTransceiver.Initialize();

while (true)
{
    Console.Write("Sending..");

    Task<bool> transmitTask = rfm9XLoraTransceiver.Transmit(Encoding.ASCII.GetBytes("All your base are belogn to us!"));
    transmitTask.Wait();

    Console.WriteLine("OK");

    Thread.Sleep(1000);
}
```

## Simple reciever
```cs
TrancieverConnectionFactory trancieverConnectionFactory = new TrancieverConnectionFactory();
ITransceiverSpiConnection spiConnection = trancieverConnectionFactory.CreateForDragino();

RFM9XLoraTransceiver rfm9XLoraTransceiver = new RFM9XLoraTransceiver(spiConnection);
rfm9XLoraTransceiver.Initialize();

while (true)
{
    Task<RawData> recieveTask = rfm9XLoraTransceiver.Recieve();

    recieveTask.Wait();

    if (recieveTask.Result != null)
    {
        Console.WriteLine(Encoding.ASCII.GetString(recieveTask.Result.Buffer));
    }
}


```
