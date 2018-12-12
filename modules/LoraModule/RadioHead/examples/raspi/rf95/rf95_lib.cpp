#include <bcm2835.h>
#include <stdio.h>
#include <signal.h>
#include <unistd.h>

#include <RH_RF69.h>
#include <RH_RF95.h>

// Dragino Raspberry PI hat
// see https://github.com/dragino/Lora
#define BOARD_DRAGINO_PIHAT

// Now we include RasPi_Boards.h so this will expose defined 
// constants with CS/IRQ/RESET/on board LED pins definition
#include "../RasPiBoards.h"

// Our RFM95 Configuration 
#define RF_FREQUENCY  915.0
#define RF_GATEWAY_ID 1 
#define RF_NODE_ID    10

#define RF_CS_PIN 7

// Create an instance of a driver
RH_RF95 rf95(RF_CS_PIN, RF_IRQ_PIN);


extern "C" bool Init ()
{
    if (!bcm2835_init()) {
        fprintf( stderr, "%s bcm2835_init() Failed\n\n", __BASEFILE__ );
        return false;
    }

#ifdef RF_IRQ_PIN
    printf( ", IRQ=GPIO%d", RF_IRQ_PIN );
  // IRQ Pin input/pull down 
    pinMode(RF_IRQ_PIN, INPUT);
    bcm2835_gpio_set_pud(RF_IRQ_PIN, BCM2835_GPIO_PUD_DOWN);
#endif

#ifdef RF_RST_PIN
    printf( ", RST=GPIO%d", RF_RST_PIN );
    // Pulse a reset on module
    pinMode(RF_RST_PIN, OUTPUT);
    digitalWrite(RF_RST_PIN, LOW );
    bcm2835_delay(150);
    digitalWrite(RF_RST_PIN, HIGH );
    bcm2835_delay(100);
#endif

    if (!rf95.init()) {
        fprintf( stderr, "\nRF95 module init failed, Please verify wiring/module\n" );
        return false;
    } 
    else 
    {
        printf( "\nRF95 module seen OK!\r\n");
    }

#ifdef RF_IRQ_PIN
    // Since we may check IRQ line with bcm_2835 Rising edge detection
    // In case radio already have a packet, IRQ is high and will never
    // go to low so never fire again 
    // Except if we clear IRQ flags and discard one if any by checking
    rf95.available();

    // Now we can enable Rising edge detection
    bcm2835_gpio_ren(RF_IRQ_PIN);
#endif

    // Defaults after init are 434.0MHz, 13dBm, Bw = 125 kHz, Cr = 4/5, Sf = 128chips/symbol, CRC on

    // The default transmitter power is 13dBm, using PA_BOOST.
    // If you are using RFM95/96/97/98 modules which uses the PA_BOOST transmitter pin, then 
    // you can set transmitter powers from 5 to 23 dBm:
    //rf95.setTxPower(23, false); 
    // If you are using Modtronix inAir4 or inAir9,or any other module which uses the
    // transmitter RFO pins and not the PA_BOOST pins
    // then you can configure the power transmitter power for -1 to 14 dBm and with useRFO true. 
    // Failure to do that will result in extremely low transmit powers.
    //rf95.setTxPower(14, true);

    rf95.setTxPower(14, false); 

    // You can optionally require this module to wait until Channel Activity
    // Detection shows no activity on the channel before transmitting by setting
    // the CAD timeout to non-zero:
    //rf95.setCADTimeout(10000);

    // Adjust Frequency
    rf95.setFrequency( RF_FREQUENCY );

    // This is our Node ID
    rf95.setThisAddress(RF_NODE_ID);
    rf95.setHeaderFrom(RF_NODE_ID);
    
    // Where we're sending packet
    rf95.setHeaderTo(RF_GATEWAY_ID);  



    return true;
}

extern "C" void Close()
{
    printf( "\n%s closinging\n" );
    bcm2835_close();  
}

extern "C"  void Transmit (uint8_t* data_to_send, uint8_t length)
{
#ifdef RF_LED_PIN
    digitalWrite(RF_LED_PIN, HIGH);
#endif
        
    printf("Sending %02d bytes to node #%d => ", length, RF_GATEWAY_ID );
    printbuffer(data_to_send, length);
    printf("\n" );
    rf95.send(data_to_send, length);
    rf95.waitPacketSent();

#ifdef RF_LED_PIN
    digitalWrite(RF_LED_PIN, LOW );
#endif

}

extern "C" void Receive (uint8_t* data_received, uint8_t* length)
{

#ifdef RF_IRQ_PIN
    // We have a IRQ pin ,pool it instead reading
    // Modules IRQ registers from SPI in each loop
      
    // Rising edge fired ?
    if (bcm2835_gpio_eds(RF_IRQ_PIN)) 
    {
        // Now clear the eds flag by setting it to 1
        bcm2835_gpio_set_eds(RF_IRQ_PIN);
        //printf("Packet Received, Rising event detect for pin GPIO%d\n", RF_IRQ_PIN);
#endif

        if (rf95.available()) 
        { 
#ifdef RF_LED_PIN
            
            digitalWrite(RF_LED_PIN, HIGH);
#endif
            // Should be a message for us now
            uint8_t buf[RH_RF95_MAX_MESSAGE_LEN];
            uint8_t len  = sizeof(buf);
            uint8_t from = rf95.headerFrom();
            uint8_t to   = rf95.headerTo();
            uint8_t id   = rf95.headerId();
            uint8_t flags= rf95.headerFlags();;
            int8_t rssi  = rf95.lastRssi();
          
            if (rf95.recv(buf, &len)) 
            {
                printf("Packet[%02d] #%d => #%d %ddB: ", length, from, to, rssi);
                printbuffer(buf, len);
            } 
            else 
            {
                Serial.print("receive failed");
            }
            printf("\n");

            data_received = buf;
            *length = len;
        }
        
#ifdef RF_IRQ_PIN
    }
#endif

}

