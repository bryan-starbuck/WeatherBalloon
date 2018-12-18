# High Altitude Weather Balloon

The goal is to collect telemetry from the weather balloon at altitude via Long Range Radio (Lora) to trackers on the ground.  The tracker devices will then send the messages to the Azure cloud where it will be used to predict where the balloon will land.

[![Build status](https://starbuckdevops.visualstudio.com/WeatherBalloon/_apis/build/status/BalloonEdge-CI)](https://starbuckdevops.visualstudio.com/WeatherBalloon/_build/latest?definitionId=5)

Technology:
- IoT Edge
- Python
- C# .NET Core 2.1
- Raspberry Pi
- Docker
- Radiohead (LoRa Library)
- Azure Functions
- CosmosDB
- PowerBi
- Azure Logic App
- XUnit
- QEMU - build Raspberry Pi images in Azure [Link](http://www.hotblackrobotics.com/en/blog/2018/01/22/docker-images-arm/)

# Architecture
![alt text](WeatherBalloon.png "Architecture")
