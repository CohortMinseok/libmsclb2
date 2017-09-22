using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libmsclb2.Cryptography
{
    /// <summary>
    /// Custom exceptions occuring in one of the cryptographic services
    /// </summary>
    public class CryptoException : Exception
    {
        public CryptoException(string message) : base(message)
        { }

        public CryptoException(string message, int errorCode) : base(message)
        {
            HResult = errorCode;
        }
    }
}
