using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Idellio.Networking.Packets
{
    [MessagePackObject]
    class NetworkMessage
    {
        [Key(0)]
        public string Type { get; set; }

        [Key(1)]
        public byte[] Buffer { get; set; }
    }
}
