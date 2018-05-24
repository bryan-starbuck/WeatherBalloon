using System;
using System.Collections.Generic;
using System.Text;

namespace APRSFunction.Entities
{
    public class Entry
    {
        public string @class { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public UInt64 time { get; set; }
        public UInt64 lasttime { get; set; }
        public double lat { get; set; }
        public double lng { get; set; }
        public double altitude { get; set; }
        public int course { get; set; }
        public double speed { get; set; }
        public string symbol { get; set; }
        public string srccall { get; set; }
        public string dstcall { get; set; }
        public string mice_msg { get; set; }
        public string comment { get; set; }
        public string status { get; set; }
        public UInt64 status_lasttime { get; set; }
        public string path { get; set; }
    }

    public class APRSResponse
    {
        public string command { get; set; }
        public string result { get; set; }
        public string what { get; set; }
        public int found { get; set; }
        public List<Entry> entries { get; set; }
    }
    
}
