using System;
using System.Collections.Generic;
using System.Text;

namespace WazeBotDiscord.Tiles
{
    public class TilesResult
    {
        public string NATileDate { get; set; }
        public string NAUpdatePerformed { get; set; }
        public long NAUnixTimestamp { get; set; }
        public string INTLTileDate { get; set; }
        public string INTLUpdatePerformed { get; set; }
        public long INTLUnixTimestamp { get; set; }
        public string ILTileDate { get; set; }
        public string ILUpdatePerformed { get; set; }
        public long ILUnixTimestamp { get; set; }
    }
}
