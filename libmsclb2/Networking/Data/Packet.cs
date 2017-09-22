using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libmsclb2.Networking.Data
{
    /// <summary>
    /// Universal packet properties for reading and writing
    /// </summary>
    public abstract class Packet
    {
        /// <summary>
        /// The header used for internal routing
        /// </summary>
        /// <remarks>When reading packets only the internal header should be used.</remarks>
        public ushort InternalHeader { get; set; }

        /// <summary>
        /// The dynamic header assigned by the server
        /// </summary>
        public unsafe ushort ExternalHeader
        {
            get
            {
                fixed (byte* ptr = _DataBuffer)
                {
                    return *(ushort*)(ptr);
                }
            }
            set
            {
                fixed (byte* ptr = _DataBuffer)
                {
                    *(ushort*)(ptr) = value;
                }
            }
        }

        /// <summary>
        /// The type of incoming packet.
        /// </summary>
        public IncomingPacketType Type { get; set; }

        public byte[] _DataBuffer;

        /// <summary>
        /// The raw packet data
        /// </summary>
        public byte[] DataBuffer
        {
            get { return _DataBuffer; }
            set { _DataBuffer = value; }
        }

        /// <summary>
        /// The position in the databuffer of this packet
        /// </summary>
        public int Position { get; set; }

        /// <summary>
        /// The absolute length of the packet
        /// </summary>
        public int AbsoluteLength
        {
            get
            {
                return DataBuffer.Length;
            }
        }

        /// <summary>
        /// The data length of the packet
        /// </summary>
        public int DataLength
        {
            get { return (DataBuffer.Length <= 0) ? 0 : DataBuffer.Length - 2; }
        }

        /// <summary>
        /// Converts the packet, including the header, to a byte array
        /// </summary>
        /// <returns>A complete raw packet</returns>
        public abstract byte[] ToArray();

        /// <summary>
        /// Converts the packet to a readable hex string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            byte[] data = ToArray();
            return BitConverter.ToString(data.Skip(2).ToArray()).Replace("-", " ");
        }

        /// <summary>
        /// Allows packets to be implicitly used as byte[], without needing an explicit cast or conversion
        /// </summary>
        public static implicit operator byte[] (Packet packet)
        {
            return packet.ToArray();
        }
    }
}
