using PcapDecrypt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRFPacketDecoder.ParserClass
{
    public class WPacket
    {
        public PacketCmdS2C Head { get; set; }
        public float Time { get; set; }
        public int Length { get; set; }
        public byte[] Bytes { get; set; }

        //Listview için  
        public string SHead
        {
            get { return Head.ToString(); }
        }
        public string STime
        {
            get
            {
                var ts = TimeSpan.FromMilliseconds(Convert.ToInt32(this.Time));
                return ts.Hours.ToString("00") + ":" + ts.Minutes.ToString("00") + ":" + ts.Seconds.ToString("00");
            }
        }
        //
    }
}
