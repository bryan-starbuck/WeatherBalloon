#! /usr/bin/python
# Data Acquisition script for GPS and BME 280
# installed to run on startup: 
# crontab -e
# @reboot python3 /home/pi/Documents/GPSAcquisition.py > /balloon_data/gps-log.txt &

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
  gpsp.start() # start it up

  while True:
    try:
      #It may take a second or two to get good data
      #print gpsd.fix.latitude,', ',gpsd.fix.longitude,'  Time: ',gpsd.utc

      payload = {'lat':gpsd.fix.latitude if not math.isnan(gpsd.fix.latitude) else 0,
                 'long':gpsd.fix.longitude if not math.isnan(gpsd.fix.longitude) else 0,
                'time': str(gpsd.utc),
                 'alt':gpsd.fix.altitude if not math.isnan(gpsd.fix.altitude) else 0,
                 'speed':gpsd.fix.speed if not math.isnan(gpsd.fix.speed) else 0,
                 'climb':gpsd.fix.climb if not math.isnan(gpsd.fix.climb) else 0,
                 'track':gpsd.fix.track if not math.isnan(gpsd.fix.track) else 0,
                 'mode' :gpsd.fix.mode,
                 'temp' :bme280.temperature,
                 'humidity' : bme280.humidity,
                 'pressure' : bme280.pressure
                 }

      print(json.dumps(payload))
      r = requests.post("http://localhost:8080/data", json=payload)
      print(r.status_code, r.reason)
      time.sleep(60)
    except requests.exceptions.RequestException as e:
      print(e)
      time.sleep(60)
    except (KeyboardInterrupt, SystemExit): #when you press ctrl+c
      print("\nKilling Thread...")
      exit(1)

      gpsp.running = False
      gpsp.join() # wait for the thread to finish what it's doing

  print ("Done.\nExiting.")
