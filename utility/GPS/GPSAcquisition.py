#! /usr/bin/python
# Data Acquisition script for GPS and BME 280
import os
from gps import *
from time import *
import time
import threading
import json
import requests

import board
import  busio
import adafruit_bme280

i2c = busio.I2C(board.SCL, board.SDA)
bme280 = adafruit_bme280.Adafruit_BME280_I2C(i2c)
  
gpsd = None #setting the global variable

os.system('clear') #clear the terminal (optional)

class GpsPoller(threading.Thread):
  def __init__(self):
    threading.Thread.__init__(self)
    global gpsd #bring it in scope
    gpsd = gps(mode=WATCH_ENABLE) #starting the stream of info
    self.current_value = None
    self.running = True #setting the thread running to true

  def run(self):
    global gpsd
    while gpsp.running:
      gpsd.next() #this will continue to loop and grab EACH set of gpsd info to clear the buffer

if __name__ == '__main__':
  gpsp = GpsPoller() # create the thread
  try:
    gpsp.start() # start it up
    while True:
      #It may take a second or two to get good data
      #print gpsd.fix.latitude,', ',gpsd.fix.longitude,'  Time: ',gpsd.utc

      payload = {'lat':gpsd.fix.latitude,
                 'long':gpsd.fix.longitude,
                'time': str(gpsd.utc),
                 'alt':gpsd.fix.altitude,
                 'speed':gpsd.fix.speed,
                 'climb':gpsd.fix.climb,
                 'track':gpsd.fix.track,
                 'mode' :gpsd.fix.mode, 
                 'temp' :bme280.temperature,
                 'humidity' : bme280.humidity,
                 'pressure' : bme280.pressure
                 }

      print(json.dumps(payload))
      r = requests.post("http://localhost:8080/data", json=payload)
      print(r.status_code, r.reason)

      time.sleep(60) 
  except (KeyboardInterrupt, SystemExit): #when you press ctrl+c
    print "\nKilling Thread..."
    gpsp.running = False
    gpsp.join() # wait for the thread to finish what it's doing
  print "Done.\nExiting."


