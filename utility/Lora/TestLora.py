#!/usr/bin/python

import sys, os

# Add path to pyRadioHeadiRF95 module
sys.path.append(os.path.dirname(__file__) + "/../")

import pyRadioHeadRF95 as radio
import time

rf95 = radio.RF95()

rf95.init()

rf95.setTxPower(14, False)
rf95.setFrequency(868)

rf95.setSignalBandwidth(rf95.Bandwidth500KHZ)
rf95.setSpreadingFactor(rf95.SpreadingFactor12)
rf95.setCodingRate4(rf95.CodingRate4_8)

print "StartUp Done!"

while True:
    msg = "Hello\0"
    print "Sending message..."
    rf95.send(msg, len(msg))
    rf95.waitPacketSent()
    time.sleep(10)
