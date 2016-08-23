using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PcapDecrypt.Json
{
    public class Player
    {
        public int netid { get; set; }
        public int profilid { get; set; }
        public string name { get; set; }
        public string champion { get; set; }
        public int team { get; set; }

        public override string ToString()
        {
            return this.name + " - " + this.champion;
        }
    }
}
