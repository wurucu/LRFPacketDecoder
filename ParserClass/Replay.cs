using LRFPacketDecoder.ParserClass;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PcapDecrypt.Json
{
   public class Replay
    {
        public string replayName { get; set; }
        public int accountId { get; set; }
        public List<Player> players { get; set; }
        public string serverAddress { get; set; }
        public int serverPort { get; set; }
        public string encryptionKey { get; set; }
        public string clientVersion { get; set; }
        public List<Packet> packets { get; set; }

        public List<WPacket> WPackets { get; set; }

        public void Decodes()
        {
            if (WPackets == null)
                this.WPackets = new List<WPacket>();
            else
                this.WPackets.Clear();

            foreach (var packet in packets)
            {
                var ps = packet.Decode(this.encryptionKey);
                if (ps != null)
                    this.WPackets.AddRange(ps);
            }
        }

        public void SaveWDC(string fileName)
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(this);
            File.WriteAllText(fileName, json);
        }
    }
}
