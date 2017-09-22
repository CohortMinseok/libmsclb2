using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libmsclb2.Networking
{
    /// <summary>
    /// Custom exception for notable errors occuring during network or network related activities
    /// </summary>
    public class NetworkingException : Exception
    {
        public NetworkingException(string message) : base(message)
        { }

        public NetworkingException(string message, int errorCode) : base(message)
        {
            HResult = errorCode;
        }

        public NetworkingException(string message, Exception inner) : base(message, inner)
        { }

        public NetworkingException(string message, Exception inner, int errorCode) : base(message, inner)
        {
            HResult = errorCode;
        }
    }
}
