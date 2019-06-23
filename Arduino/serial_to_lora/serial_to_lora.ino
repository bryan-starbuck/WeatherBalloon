// Feather9x_TX
// -*- mode: C++ -*-
// Example sketch showing how to create a simple messaging client (transmitter)
// with the RH_RF95 class. RH_RF95 class does not provide for addressing or
// reliability, so you should only use RH_RF95 if you do not need the higher
// level messaging abilities.
// It is designed to work with the other example Feather9x_RX
 
#include <SPI.h>
#include <RH_RF95.h>
 
// for feather32u4 
#define RFM95_CS 8
#define RFM95_RST 4
#define RFM95_INT 7

 
/* for feather m0  
#define RFM95_CS 8
#define RFM95_RST 4
#define RFM95_INT 3
*/
 
/* for shield 
#define RFM95_CS 10
#define RFM95_RST 9
#define RFM95_INT 7
*/
 
/* Feather 32u4 w/wing
#define RFM95_RST     11   // "A"
#define RFM95_CS      10   // "B"
#define RFM95_INT     2    // "SDA" (only SDA/SCL/RX/TX have IRQ!)
*/
 
/* Feather m0 w/wing 
#define RFM95_RST     11   // "A"
#define RFM95_CS      10   // "B"
#define RFM95_INT     6    // "D"
*/
 
#if defined(ESP8266)
  /* for ESP w/featherwing */ 
  #define RFM95_CS  2    // "E"
  #define RFM95_RST 16   // "D"
  #define RFM95_INT 15   // "B"
 
#elif defined(ESP32)  
  /* ESP32 feather w/wing */
  #define RFM95_RST     27   // "A"
  #define RFM95_CS      33   // "B"
  #define RFM95_INT     12   //  next to A
 
#elif defined(NRF52)  
  /* nRF52832 feather w/wing */
  #define RFM95_RST     7   // "A"
  #define RFM95_CS      11   // "B"
  #define RFM95_INT     31   // "C"
  
#elif defined(TEENSYDUINO)
  /* Teensy 3.x w/wing */
  #define RFM95_RST     9   // "A"
  #define RFM95_CS      10   // "B"
  #define RFM95_INT     4    // "C"
#endif
 
 
// Change to 434.0 or other frequency, must match RX's freq!
#define RF95_FREQ 915.0
 
// Singleton instance of the radio driver
RH_RF95 rf95(RFM95_CS, RFM95_INT);
 
void setup() 
{
  pinMode(RFM95_RST, OUTPUT);
  digitalWrite(RFM95_RST, HIGH);
 
  Serial.begin(115200);
  while (!Serial) {
    delay(1);
  }
 
  delay(100);
 
  Serial.println("-Feather LoRa TX Test!");
 
  // manual reset
  digitalWrite(RFM95_RST, LOW);
  delay(10);
  digitalWrite(RFM95_RST, HIGH);
  delay(10);
 
  while (!rf95.init()) {
    Serial.println("-LoRa radio init failed");
    while (1);
  }
  Serial.println("-LoRa radio init OK!");
 
  // Defaults after init are 434.0MHz, modulation GFSK_Rb250Fd250, +13dbM
  if (!rf95.setFrequency(RF95_FREQ)) {
    Serial.println("-setFrequency failed");
    while (1);
  }
  Serial.print("-Set Freq to: "); 
  Serial.println(RF95_FREQ);
  
  rf95.setSignalBandwidth(125000);
  rf95.setCodingRate4(1);
  rf95.setSpreadingFactor(8);
   
  //rf95.setModemConfig(RH_RF95::Bw125Cr48Sf4096);  //set for pre-configured long range
  rf95.setTxPower(20);  //set for 100mw

  // Defaults after init are 434.0MHz, 13dBm, Bw = 125 kHz, Cr = 4/5, Sf = 128chips/symbol, CRC on
 
  // The default transmitter power is 13dBm, using PA_BOOST.
  // If you are using RFM95/96/97/98 modules which uses the PA_BOOST transmitter pin, then 
  // you can set transmitter powers from 5 to 23 dBm:
  //rf95.setTxPower(26, false);
//  rf95.setModemConfig(RH_RF95::ModemConfigChoice::Bw31_25Cr48Sf512);
}

const byte max_length = 128;
char receivedChars[max_length];   // an array to store the received data
bool newData = false; 
 
void loop()
{
  readSerialMessage();

  if (newData)
  {
    Serial.println("-Transmitting...");
    rf95.send((uint8_t *)receivedChars, strlen(receivedChars));

    //Serial.println("-Waiting for packet to complete..."); 
    //delay(10);
    //rf95.waitPacketSent();
    // Now wait for a reply
    //uint8_t buf[RH_RF95_MAX_MESSAGE_LEN];
    //uint8_t len = sizeof(buf);
   
    //Serial.println("Waiting for reply...");
    //if (rf95.waitAvailableTimeout(1000))
    //{ 
      // Should be a reply message for us now   
    //  if (rf95.recv(buf, &len))
     // {
        //Serial.print("Got reply: ");
        //Serial.println((char*)buf);
        //Serial.print("RSSI: ");
        //Serial.println(rf95.lastRssi(), DEC);    
      //}
      //else
      //{
        //Serial.println("Receive failed");
      //}
    //}
  }
    
  newData = false;
}


void readSerialMessage() 
{
    static byte ndx = 0;
    char endMarker = '\n';
    char rc;
    
    while (Serial.available() > 0 && newData == false) {
        rc = Serial.read();

        if (rc != endMarker) {
            receivedChars[ndx] = rc;
            ndx++;
            if (ndx >= max_length) {
              // message too long - truncating
              ndx = max_length - 1;
            }
        }
        else {
            receivedChars[ndx] = '\0'; // terminate the string
            ndx = 0;
            newData = true;
        }
    }
}
