using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libmsclb2.Networking.Data
{
    /// <summary>
    /// Types of incoming packets. Not every packet needs to be translated by the login- or channelstage.
    /// </summary>
    public enum IncomingPacketType
    {
        NoHeader,
        StaticHeader,
        DynamicHeader
    }
}
