#! /usr/bin/python
import os
from time import *
import time
import json
import requests
from datetime import datetime


# Test data

dataPoints = [(1537819740,33.3687,-111.924,0),
  (1537819790,33.3697,-111.924,250),
  (1537819840,33.3707,-111.923,500),
  (1537819890,33.3717,-111.922,750),
  (1537819940,33.3729,-111.922,1000),
  (1537819990,33.3742,-111.921,1250),
  (1537820040,33.3755,-111.92,1500),
  (1537820090,33.3769,-111.92,1750),
  (1537820140,33.3785,-111.919,2000),
  (1537820190,33.3801,-111.919,2250),
  (1537820240,33.3819,-111.918,2500),
  (1537820290,33.3838,-111.917,2750),
  (1537820340,33.386,-111.916,3000),
  (1537820390,33.3884,-111.915,3250),
  (1537820440,33.3912,-111.914,3500),
  (1537820490,33.3945,-111.913,3750),
  (1537820540,33.3979,-111.912,4000),
  (1537820590,33.4013,-111.91,4250),
  (1537820640,33.4042,-111.908,4500),
  (1537820690,33.4064,-111.906,4750),
  (1537820740,33.4078,-111.903,5000),
  (1537820790,33.4086,-111.9,5250),
  (1537820840,33.409,-111.897,5500),
  (1537820890,33.4096,-111.893,5750),
  (1537820940,33.4103,-111.889,6000),
  (1537820990,33.4113,-111.885,6250),
  (1537821040,33.4127,-111.88,6500),
  (1537821090,33.4144,-111.875,6750),
  (1537821140,33.4162,-111.87,7000),
  (1537821190,33.4181,-111.864,7250),
  (1537821240,33.42,-111.857,7500),
  (1537821290,33.4221,-111.85,7750),
  (1537821340,33.4242,-111.842,8000),
  (1537821390,33.4264,-111.834,8250),
  (1537821440,33.4288,-111.825,8500),
  (1537821490,33.4314,-111.816,8750),
  (1537821540,33.4342,-111.806,9000),
  (1537821590,33.4372,-111.795,9250),
  (1537821640,33.4404,-111.784,9500),
  (1537821690,33.4438,-111.773,9750),
  (1537821740,33.4472,-111.76,10000),
  (1537821790,33.4509,-111.747,10250),
  (1537821840,33.4547,-111.734,10500),
  (1537821890,33.4589,-111.719,10750),
  (1537821940,33.4633,-111.703,11000),
  (1537821990,33.4682,-111.687,11250),
  (1537822040,33.4734,-111.669,11500),
  (1537822090,33.479,-111.652,11750),
  (1537822140,33.4848,-111.634,12000),
  (1537822190,33.4908,-111.616,12250),
  (1537822240,33.497,-111.598,12500),
  (1537822290,33.503,-111.58,12750),
  (1537822340,33.509,-111.563,13000),
  (1537822390,33.5148,-111.547,13250),
  (1537822440,33.5204,-111.531,13500),
  (1537822490,33.5258,-111.515,13750),
  (1537822540,33.5309,-111.5,14000),
  (1537822590,33.5359,-111.486,14250),
  (1537822640,33.5404,-111.473,14500),
  (1537822690,33.5447,-111.461,14750),
  (1537822740,33.5486,-111.449,15000),
  (1537822790,33.5521,-111.438,15250),
  (1537822840,33.5554,-111.428,15500),
  (1537822890,33.5582,-111.419,15750),
  (1537822940,33.5608,-111.412,16000),
  (1537822990,33.563,-111.405,16250),
  (1537823040,33.5649,-111.4,16500),
  (1537823090,33.5665,-111.395,16750),
  (1537823140,33.5681,-111.391,17000),
  (1537823190,33.5695,-111.387,17250),
  (1537823240,33.5709,-111.383,17500),
  (1537823290,33.5723,-111.38,17750),
  (1537823340,33.5735,-111.377,18000),
  (1537823390,33.5747,-111.374,18250),
  (1537823440,33.5758,-111.372,18500),
  (1537823490,33.5769,-111.369,18750),
  (1537823540,33.5778,-111.368,19000),
  (1537823590,33.5784,-111.366,19250),
  (1537823640,33.5789,-111.364,19500),
  (1537823690,33.5791,-111.363,19750),
  (1537823740,33.5791,-111.362,20000),
  (1537823790,33.5789,-111.361,20250),
  (1537823840,33.5785,-111.36,20500),
  (1537823890,33.5779,-111.359,20750),
  (1537823940,33.5772,-111.359,21000),
  (1537823990,33.5766,-111.359,21250),
  (1537824040,33.5761,-111.359,21500),
  (1537824090,33.5758,-111.359,21750),
  (1537824140,33.5755,-111.359,22000),
  (1537824190,33.5754,-111.359,22250),
  (1537824240,33.5754,-111.36,22500),
  (1537824290,33.5756,-111.36,22750),
  (1537824340,33.5758,-111.361,23000),
  (1537824390,33.5762,-111.362,23250),
  (1537824440,33.5767,-111.363,23500),
  (1537824490,33.5773,-111.364,23750),
  (1537824540,33.5781,-111.365,24000),
  (1537824590,33.5789,-111.366,24250),
  (1537824640,33.5798,-111.368,24500),
  (1537824690,33.5806,-111.369,24750),
  (1537824740,33.5816,-111.371,25000),
  (1537824790,33.5825,-111.372,25250),
  (1537824840,33.5835,-111.374,25500),
  (1537824890,33.5845,-111.376,25750),
  (1537824940,33.5856,-111.378,26000),
  (1537824990,33.5867,-111.38,26250),
  (1537825040,33.5878,-111.382,26500),
  (1537825090,33.5889,-111.385,26750),
  (1537825140,33.59,-111.387,27000),
  (1537825190,33.5911,-111.389,27250),
  (1537825240,33.5923,-111.392,27500),
  (1537825290,33.5934,-111.394,27750),
  (1537825340,33.5945,-111.397,28000),
  (1537825390,33.5956,-111.399,28250),
  (1537825440,33.5967,-111.402,28500),
  (1537825490,33.5978,-111.404,28750),
  (1537825540,33.5988,-111.407,29000),
  (1537825590,33.5999,-111.41,29250),
  (1537825640,33.601,-111.413,29500),
  (1537825690,33.602,-111.416,29750),
  (1537825740,33.6031,-111.419,30000),
  (1537825790,33.6041,-111.421,28066.9),
  (1537825840,33.6052,-111.424,26398.8),
  (1537825890,33.6062,-111.425,24936.2),
  (1537825940,33.607,-111.427,23639.3),
  (1537825990,33.6073,-111.427,22462.4),
  (1537826040,33.6071,-111.428,21385.1),
  (1537826090,33.6065,-111.427,20392),
  (1537826140,33.6063,-111.426,19470.8),
  (1537826190,33.6068,-111.424,18611.8),
  (1537826240,33.6078,-111.422,17807),
  (1537826290,33.6092,-111.418,17050.2),
  (1537826340,33.611,-111.413,16335.8),
  (1537826390,33.6135,-111.405,15659.4),
  (1537826440,33.6167,-111.395,15017.1),
  (1537826490,33.6208,-111.383,14405.7),
  (1537826540,33.6257,-111.368,13822.3),
  (1537826590,33.6312,-111.352,13264.4),
  (1537826640,33.6371,-111.334,12730),
  (1537826690,33.6431,-111.316,12217.1),
  (1537826740,33.6487,-111.298,11724),
  (1537826790,33.6537,-111.28,11249.4),
  (1537826840,33.6578,-111.264,10791.4),
  (1537826890,33.6613,-111.249,10346.6),
  (1537826940,33.6644,-111.236,9913.86),
  (1537826990,33.6672,-111.224,9492.39),
  (1537827040,33.6698,-111.213,9081.49),
  (1537827090,33.6723,-111.202,8680.52),
  (1537827140,33.6746,-111.193,8288.9),
  (1537827190,33.6769,-111.184,7906.13),
  (1537827240,33.6791,-111.176,7531.72),
  (1537827290,33.6813,-111.169,7165.23),
  (1537827340,33.6834,-111.163,6806.26),
  (1537827390,33.6852,-111.158,6454.45),
  (1537827440,33.6865,-111.153,6109.44),
  (1537827490,33.6874,-111.148,5770.93),
  (1537827540,33.6881,-111.144,5438.62),
  (1537827590,33.6889,-111.141,5112.23),
  (1537827640,33.69,-111.138,4791.52),
  (1537827690,33.6916,-111.136,4476.24),
  (1537827740,33.6934,-111.134,4166.17),
  (1537827790,33.6953,-111.132,3861.11),
  (1537827840,33.697,-111.13,3560.86),
  (1537827890,33.6986,-111.129,3265.24),
  (1537827940,33.7001,-111.127,2974.06),
  (1537827990,33.7015,-111.125,2687.19),
  (1537828040,33.7031,-111.123,2404.45),
  (1537828090,33.7046,-111.12,2125.7),
  (1537828140,33.7062,-111.118,1850.82),
  (1537828190,33.7077,-111.116,1579.66),
  (1537828240,33.7092,-111.113,1312.11),
  (1537828290,33.7107,-111.111,1048.05),
  (1537828340,33.7121,-111.109,787.368),
  (1537828390,33.7135,-111.107,529.962),
  (1537828440,33.7149,-111.104,275.731),
  (1537828490,33.7163,-111.102,24.5809),
  (1537828495,33.7164,-111.102,-0.368208)]


if __name__ == '__main__':

  count = len(dataPoints)
  print(count)
  index = 0
   
  try:
    while True:
      #It may take a second or two to get good data
      #print gpsd.fix.latitude,', ',gpsd.fix.longitude,'  Time: ',gpsd.utc

      

      payload = {'lat':dataPoints[index][1],
                 'long':dataPoints[index][2],
                'time': str(datetime.utcnow()),
                 'alt':dataPoints[index][3],
                 'speed':0,
                 'climb':1,
                 'track':105.23,
                 'mode' :3
                 }

      print(json.dumps(payload))
      r = requests.post("http://localhost:8080/data", json=payload)
      print(r.status_code, r.reason)

      index = index + 1
      if ( index >= count ):
        index = 0

##      print
##      print ' GPS reading'
##      print '----------------------------------------'
##      print 'latitude    ' , gpsd.fix.latitude
##      print 'longitude   ' , gpsd.fix.longitude
##      print 'time utc    ' , gpsd.utc,' + ', gpsd.fix.time
##      print 'altitude (m)' , gpsd.fix.altitude
##      print 'eps         ' , gpsd.fix.eps
##      print 'epx         ' , gpsd.fix.epx
##      print 'epv         ' , gpsd.fix.epv
##      print 'ept         ' , gpsd.fix.ept
##      print 'speed (m/s) ' , gpsd.fix.speed
##      print 'climb       ' , gpsd.fix.climb
##      print 'track       ' , gpsd.fix.track
##      print 'mode        ' , gpsd.fix.mode
##      print
##      print 'sats        ' , gpsd.satellites

      time.sleep(60) #set to whatever
  except (KeyboardInterrupt, SystemExit): #when you press ctrl+c
    print "\nKilling Thread..."
  print "Done.\nExiting."

