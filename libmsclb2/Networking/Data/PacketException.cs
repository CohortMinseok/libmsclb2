using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libmsclb2.Networking.Data
{
    /// <summary>
    /// Custom exception occuring during reading or writing from/to packets
    /// </summary>
    public class PacketException : Exception
    {
        public PacketException(string message) : base(message)
        { }

        public PacketException(string message, int errorCode) : base(message)
        {
            HResult = errorCode;
        }
    }
}
