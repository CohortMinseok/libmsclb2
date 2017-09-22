using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace libmsclb2.Networking.Data
{
    /// <summary>
    /// Packet wrapper for writing data
    /// </summary>
    public sealed class PacketWriter : Packet
    {
        /// <summary>
        /// Constructs a packet with the default size of 64 bytes
        /// </summary>
        public PacketWriter(dynamic header)
        {
            DataBuffer = new byte[64];

            ushort pHeader = 0xFFFF;

            try
            {
                pHeader = (ushort)header;
            }
            catch
            {
                throw new PacketException("Failed to parse the provided header to UInt16.", 3001);
            }

            ExternalHeader = pHeader;
        }

        /// <summary>
        /// Constructs a packet with a variable size
        /// </summary>
        /// <param name="size">The size of the packet in bytes</param>
        public PacketWriter(dynamic header, int size)
        {
            if (size < 6)
                throw new PacketException("Size is too low. A packet must be 6 bytes or bigger.", 1000);

            DataBuffer = new byte[size];

            ushort pHeader = 0xFFFF;

            try
            {
                pHeader = (ushort)header;
            }
            catch
            {
                throw new PacketException("Failed to parse the provided header to UInt16.", 3001);
            }

            ExternalHeader = pHeader;
        }

        /// <summary>
        /// Allocates (more) space to a packet in blocks of 64 bytes
        /// </summary>
        /// <param name="size">The minimal amount of space to allocate in bytes</param>
        private void AllocateSpace(ushort size)
        {
            while (Position + size >= DataBuffer.Length)
            {
                byte[] temp = DataBuffer;
                DataBuffer = new byte[temp.Length + 64];
                Buffer.BlockCopy(temp, 0, DataBuffer, 0, temp.Length);
            }
        }

        /// <summary>
        /// Writes a byte to the packet
        /// </summary>
        /// <param name="b"></param>
        public unsafe void WriteInt8(byte b)
        {
            if (DataBuffer.Length - Position < sizeof(byte))
                AllocateSpace(sizeof(byte));

            fixed (byte* ptr = DataBuffer)
            {
                *(ptr + Position++) = b;
            }
        }

        /// <summary>
        /// Writes a signed byte to the packet
        /// </summary>
        /// <param name="b"></param>
        public unsafe void WriteSByte(sbyte b)
        {
            if (DataBuffer.Length - Position < sizeof(sbyte))
                AllocateSpace(sizeof(sbyte));

            fixed (byte* ptr = DataBuffer)
            {
                *(ptr + Position++) = *(byte*)b;
            }
        }

        /// <summary>
        /// Writes a byte array to the packet
        /// </summary>
        /// <param name="bytes"></param>
        public unsafe void WriteBytes(byte[] bytes)
        {
            if (DataBuffer.Length - Position < bytes.Length)
                AllocateSpace((ushort)bytes.Length);

            fixed (byte* ptr = DataBuffer)
            {
                byte* ptr2 = (ptr + Position);
                Marshal.Copy(bytes, 0, (IntPtr)ptr2, bytes.Length);
                Position += bytes.Length;
            }
        }

        /// <summary>
        /// Writes a bool to the packet
        /// </summary>
        /// <param name="b"></param>
        public unsafe void WriteBool(bool b)
        {
            if (DataBuffer.Length - Position < sizeof(bool))
                AllocateSpace(sizeof(bool));

            fixed (byte* ptr = DataBuffer)
            {
                *(bool*)(ptr + Position) = b;
                Position += sizeof(bool);
            }
        }

        /// <summary>
        /// Writes a short to the packet
        /// </summary>
        /// <param name="s"></param>
        public unsafe void WriteInt16(short s)
        {
            if (DataBuffer.Length - Position < sizeof(short))
                AllocateSpace(sizeof(short));

            fixed (byte* ptr = DataBuffer)
            {
                *(short*)(ptr + Position) = s;
                Position += sizeof(short);
            }
        }

        /// <summary>
        /// Writes an unsigned short to the packet
        /// </summary>
        /// <param name="s"></param>
        public unsafe void WriteUInt16(ushort s)
        {
            if (DataBuffer.Length - Position < sizeof(ushort))
                AllocateSpace(sizeof(ushort));

            fixed (byte* ptr = DataBuffer)
            {
                *(ushort*)(ptr + Position) = s;
                Position += sizeof(ushort);
            }
        }

        /// <summary>
        /// Writes an int to the packet
        /// </summary>
        /// <param name="i"></param>
        public unsafe void WriteInt32(int i)
        {
            if (DataBuffer.Length - Position < sizeof(int))
                AllocateSpace(sizeof(int));

            fixed (byte* ptr = DataBuffer)
            {
                *(int*)(ptr + Position) = i;
                Position += sizeof(int);
            }
        }

        /// <summary>
        /// Writes an unsigned int to the packet
        /// </summary>
        /// <param name="i"></param>
        public unsafe void WriteUInt32(uint i)
        {
            if (DataBuffer.Length - Position < sizeof(uint))
                AllocateSpace(sizeof(uint));

            fixed (byte* ptr = DataBuffer)
            {
                *(uint*)(ptr + Position) = i;
                Position += sizeof(uint);
            }
        }

        /// <summary>
        /// Writes a long to the packet
        /// </summary>
        /// <param name="i"></param>
        public unsafe void WriteInt64(long i)
        {
            if (DataBuffer.Length - Position < sizeof(long))
                AllocateSpace(sizeof(long));

            fixed (byte* ptr = DataBuffer)
            {
                *(long*)(ptr + Position) = i;
                Position += sizeof(long);
            }
        }

        /// <summary>
        /// Writes an unsigned long to the packet
        /// </summary>
        /// <param name="i"></param>
        public unsafe void WriteUInt64(ulong i)
        {
            if (DataBuffer.Length - Position < sizeof(ulong))
                AllocateSpace(sizeof(ulong));

            fixed (byte* ptr = DataBuffer)
            {
                *(ulong*)(ptr + Position) = i;
                Position += sizeof(ulong);
            }
        }

        /// <summary>
        /// Writes a 'MapleString' to the packet
        /// </summary>
        /// <remarks>length[2] string[n] 00</remarks>
        /// <param name="s"></param>
        public unsafe void WriteMapleString(string s)
        {
            if (s.Length > UInt16.MaxValue)
            {
                throw new PacketException("The provided string is larger than currently is supported by MapleStory.", 2000);
            }

            byte[] raw = Encoding.UTF8.GetBytes(s);

            WriteUInt16((ushort)s.Length);
            WriteBytes(raw);
        }

        /// <summary>
        /// Writes a string with a static length to the packet. Unused space is filled with zeros.
        /// </summary>
        /// <param name="s">The string to write to the packet</param>
        /// <param name="size">The static size of the string</param>
        public unsafe void WriteStaticString(string s, int size)
        {
            if (s.Length > size)
            {
                throw new PacketException("The provided string is larger than expected.", 2001);
            }

            byte[] raw = Encoding.UTF8.GetBytes(s);

            WriteBytes(raw);

            for (int i = size - s.Length; i > 0; i--)
            {
                WriteInt8(0);
            }
        }

        /// <summary>
        /// Converts the packet, including the header, to a byte array
        /// </summary>
        /// <returns>A complete raw packet</returns>
        public override byte[] ToArray()
        {
            if (InternalHeader == 0xFFFF)
            {
                return new ArraySegment<byte>(DataBuffer, 0, Position).ToArray();
            }
            else
            {
                byte[] rawPacket = new byte[Position];
                Buffer.BlockCopy(new ArraySegment<byte>(DataBuffer, 0, Position).ToArray(), 0, rawPacket, 0, Position);

                return rawPacket;
            }
        }
    }
}
