using System;
using System.Collections.Generic;
using System.Text;

namespace APRSFunction.Entities
{
    public class IoTMessage
    {
        #region Properties

        public string Class { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public DateTime Time { get; set; }
        public DateTime LastTime { get; set; }
        public Double Lat { get; set; }
        public Double Long { get; set; }
        public Double Altitude { get; set; }
        public int Course { get; set; }
        public double Speed { get; set; }
        public string Symbol { get; set; }
        public string SrcCall { get; set; }
        public string DstCall { get; set; }
        public string Mice_Msg { get; set; }
        public string Comment { get; set; }
        public string Status { get; set; }
        public DateTime Status_LastTime { get; set; }
        public string Path { get; set; }
        public DateTime ReadTime { get; set; }

        #endregion

        public static List<IoTMessage> CreateMessages(APRSResponse aprsResponse)
        {
            if ((aprsResponse == null) || (aprsResponse.entries.Count < 1))
            {
                // todo - handle more gracefully
                return null;
            }

            var iotMessages = new List<IoTMessage>();

            foreach (var entry in aprsResponse.entries)
            {
                var iotMessage = new IoTMessage()
                {
                    Class = entry.@class,
                    Name = entry.name,
                    Type = entry.type,
                    Time = UnixTimeStampToDateTime(entry.time),
                    LastTime = UnixTimeStampToDateTime(entry.lasttime),
                    Lat = entry.lat,
                    Long = entry.lng,
                    Altitude = entry.altitude,
                    Course = entry.course,
                    Speed = entry.speed,
                    Symbol = entry.symbol,
                    SrcCall = entry.srccall,
                    DstCall = entry.dstcall,
                    Mice_Msg = entry.mice_msg,
                    Comment = entry.comment,
                    Status = entry.status,
                    Status_LastTime = UnixTimeStampToDateTime(entry.status_lasttime),
                    Path = entry.path,
                    ReadTime = DateTime.Now
                };

                iotMessages.Add(iotMessage);
            }

            return iotMessages;
        }

        private static DateTime UnixTimeStampToDateTime(UInt64 unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
    
    }
}
