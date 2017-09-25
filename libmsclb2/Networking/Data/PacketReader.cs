using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace libmsclb2.Networking.Data
{
    /// <summary>
    /// Packet wrapper for reading data
    /// </summary>
    public sealed class PacketReader : Packet
    {
        public unsafe override ushort ExternalHeader
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
        /// Constructs a new reader and tries to determine the internal header from the packet.
        /// </summary>
        /// <param name="data">The raw packet data</param>
        /// <param name="type">The type of incoming packet</param>
        public PacketReader(ref byte[] data, IncomingPacketType type)
        {
            DataBuffer = data;
            Type = type;

            switch (type)
            {                                
                case IncomingPacketType.StaticHeader:
                    InternalHeader = ExternalHeader;
                    Position += 2;
                    break;
                case IncomingPacketType.DynamicHeader:
                    Position += 2;
                    break;
                case IncomingPacketType.NoHeader:
                default:
                    InternalHeader = 0xFFFF;
                    break;
            }
        }

        /// <summary>
        /// Resets the virtual reader to the start of the packet data
        /// </summary>
        public void Reset()
        {
            switch (Type)
            {
                case IncomingPacketType.NoHeader:
                    Position = 0;
                    break;
                case IncomingPacketType.StaticHeader:
                case IncomingPacketType.DynamicHeader:
                    Position = 2;
                    break;
            }
        }

        /// <summary>
        /// Skips a certain amount of bytes from the current position
        /// </summary>
        /// <param name="amount">The amount of bytes to skip</param>
        public void Skip(int amount)
        {
            if (amount < 1)
                throw new PacketException("Can't skip less than one byte.", 3000);

            if (Position + amount > AbsoluteLength)
                throw new PacketException("The new, calculated position is outside the bounds of the packet.", 3001);
            else
                Position += amount;
        }

        /// <summary>
        /// Places the virtual reader at a certain position in the packet
        /// </summary>
        /// <param name="index">The position to set the virtual reader at</param>
        public void Goto(int index)
        {
            if (index < 0 || index > AbsoluteLength)
                throw new PacketException("The new, calculated position is outside the bounds of the packet.", 3001);

            if (Type != IncomingPacketType.NoHeader && index < 2)
                Position = 2;
            else
                Position = index;
        }

        /// <summary>
        /// Reads a byte from the buffer
        /// </summary>
        /// <returns></returns>
        public unsafe byte ReadInt8()
        {
            if (AbsoluteLength - Position < sizeof(byte))
                throw new IndexOutOfRangeException("Requested position is negative or bigger than the amount of bytes in the packet!");

            fixed (byte* ptr = DataBuffer)
            {
                byte value = *(ptr + Position);
                Position++;
                return value;
            }
        }

        public unsafe sbyte ReadSByte()
        {
            if (AbsoluteLength - Position < sizeof(sbyte))
                throw new IndexOutOfRangeException("Requested position is negative or bigger than the amount of bytes in the packet!");

            fixed (byte* ptr = DataBuffer)
            {
                sbyte value = *(sbyte*)(ptr + Position);
                Position++;
                return value;
            }
        }

        /// <summary>
        /// Reads an array of bytes from the buffer
        /// </summary>
        /// <param name="amount">The amount of bytes to read</param>
        /// <returns></returns>
        public unsafe byte[] ReadBytes(int amount)
        {
            if (amount < 1)
                return new byte[0];

            if (AbsoluteLength - Position < amount)
                throw new IndexOutOfRangeException("Requested position is negative or bigger than the amount of bytes in the packet!");

            fixed (byte* ptr = DataBuffer)
            {
                byte[] value = new byte[amount];
                Marshal.Copy((IntPtr)ptr + Position, value, 0, amount);
                Position += amount;

                return value;
            }
        }

        /// <summary>
        /// Reads a bool from the buffer
        /// </summary>
        /// <returns></returns>
        public unsafe bool ReadBool()
        {
            if (AbsoluteLength - Position < sizeof(bool))
                throw new IndexOutOfRangeException("Requested position is negative or bigger than the amount of bytes in the packet!");

            fixed (byte* ptr = DataBuffer)
            {
                bool value = *(bool*)(ptr + Position);
                Position += sizeof(bool);
                return value;
            }
        }

        /// <summary>
        /// Reads an short from the buffer
        /// </summary>
        /// <returns></returns>
        public unsafe short ReadInt16()
        {
            if (AbsoluteLength - Position < sizeof(short))
                throw new IndexOutOfRangeException("Requested position is negative or bigger than the amount of bytes in the packet!");

            fixed (byte* ptr = DataBuffer)
            {
                short value = *(short*)(ptr + Position);
                Position += sizeof(short);
                return value;
            }
        }

        /// <summary>
        /// Reads an ushort from the buffer
        /// </summary>
        /// <returns></returns>
        public unsafe ushort ReadUInt16()
        {
            if (AbsoluteLength - Position < sizeof(ushort))
                throw new IndexOutOfRangeException("Requested position is negative or bigger than the amount of bytes in the packet!");

            fixed (byte* ptr = DataBuffer)
            {
                ushort value = *(ushort*)(ptr + Position);
                Position += sizeof(ushort);
                return value;
            }
        }

        /// <summary>
        /// Reads an int from the buffer
        /// </summary>
        /// <returns></returns>
        public unsafe int ReadInt32()
        {
            if (AbsoluteLength - Position < sizeof(int))
                throw new IndexOutOfRangeException("Requested position is negative or bigger than the amount of bytes in the packet!");

            fixed (byte* ptr = DataBuffer)
            {
                int value = *(int*)(ptr + Position);
                Position += sizeof(int);
                return value;
            }
        }

        /// <summary>
        /// Reads an uint from the buffer
        /// </summary>
        /// <returns></returns>
        public unsafe uint ReadUInt32()
        {
            if (AbsoluteLength - Position < sizeof(int))
                throw new IndexOutOfRangeException("Requested position is negative or bigger than the amount of bytes in the packet!");

            fixed (byte* ptr = DataBuffer)
            {
                uint value = *(uint*)(ptr + Position);
                Position += sizeof(uint);
                return value;
            }
        }

        /// <summary>
        /// Reads a long from the buffer
        /// </summary>
        /// <returns></returns>
        public unsafe long ReadInt64()
        {
            if (AbsoluteLength - Position < sizeof(long))
                throw new IndexOutOfRangeException("Requested position is negative or bigger than the amount of bytes in the packet!");

            fixed (byte* ptr = DataBuffer)
            {
                long value = *(long*)(ptr + Position);
                Position += sizeof(long);
                return value;
            }
        }

        /// <summary>
        /// Reads an ulong from the buffer
        /// </summary>
        /// <returns></returns>
        public unsafe ulong ReadUInt64()
        {
            if (AbsoluteLength - Position < sizeof(ulong))
                throw new IndexOutOfRangeException("Requested position is negative or bigger than the amount of bytes in the packet!");

            fixed (byte* ptr = DataBuffer)
            {
                ulong value = *(ulong*)(ptr + Position);
                Position += sizeof(ulong);
                return value;
            }
        }

        /// <summary>
        /// Reads a string from the buffer with a variable length
        /// </summary>
        /// <returns></returns>
        public unsafe string ReadMapleString()
        {
            ushort length = ReadUInt16();

            if (length == 0)
                return string.Empty;

            if (AbsoluteLength - Position < length)
                throw new IndexOutOfRangeException("Requested position is negative or bigger than the amount of bytes in the packet!");

            byte[] raw = ReadBytes(length);

            return Encoding.UTF8.GetString(raw);
        }

        /// <summary>
        /// Reads a string from the buffer with a predefined length
        /// </summary>
        /// <param name="length">The predefined length of the string</param>
        /// <param name="trim">Apply TRIM to the read string</param>
        /// <returns></returns>
        public unsafe string ReadString(int length, bool trim)
        {
            if (AbsoluteLength - Position < length)
                throw new IndexOutOfRangeException("Requested position is negative or bigger than the amount of bytes in the packet!");

            byte[] raw = ReadBytes(length);
            string s = Encoding.UTF8.GetString(raw);

            if (!trim)
                return s;
            else
                return s.Replace("\0", string.Empty);
        }

        /// <summary>
        /// Converts the packet, including the header, to a byte array
        /// </summary>
        /// <returns></returns>
        public override byte[] ToArray()
        {
            //TODO: Hide allocated space for serverheader
            if (InternalHeader == 0xFFFF)
            {
                return new ArraySegment<byte>(DataBuffer, 0, Position).ToArray();
            }
            else
            {
                return new ArraySegment<byte>(DataBuffer, 2, DataLength).ToArray();
            }
        }
    }
}
