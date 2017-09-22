using libmsclb2.Cryptography.Transport;
using libmsclb2.Networking.Data;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace libmsclb2.Networking
{
    /// <summary>
    /// Provides a basic client for encrypted MapleStory network I/O
    /// </summary>
    public class MapleClient
    {
        #region Constants
        /// <summary>
        /// The length of a handshake header
        /// </summary>
        private const byte HandshakeHeaderLength = 2;

        /// <summary>
        /// The length of a normal packet header
        /// </summary>
        private const byte RegularHeaderLength = 4;

        /// <summary>
        /// The size of the buffer
        /// </summary>
        private const int BufferSize = 45000;
        #endregion

        /// <summary>
        /// The underlying socket for networking
        /// </summary>
        private Socket MapleSocket{ get; set; }

        /// <summary>
        /// The inner data buffer
        /// </summary>
        private byte[] DataBuffer { get; set; }

        /// <summary>
        /// The offset/position in the data buffer
        /// </summary>
        private int BufferOffset { get; set; }

        /// <summary>
        /// MapleStory's cryptographic implementation for outgoing data
        /// </summary>
        public Cipher LocalCipher { get; set; }

        /// <summary>
        /// MapleStory's cryptographic implementation for incoming data
        /// </summary>
        public Cipher RemoteCipher { get; set; }

        /// <summary>
        /// Determines whether the handshake with the server has been completed
        /// </summary>
        public bool Handshaken { get; set; }

        /// <summary>
        /// Function signature for the PacketReceived event
        /// </summary>
        /// <param name="reader">The constructed packet</param>
        public delegate void PacketReceivedDelegate(PacketReader reader);

        /// <summary>
        /// Raises when a packet has been received
        /// </summary>
        public event PacketReceivedDelegate PacketReceived;

        /// <summary>
        /// Function signature for the HandshakeReceived event
        /// </summary>
        /// <param name="reader"></param>
        public delegate void HandshakeReceivedDelegate(ushort version, ushort subversion, byte locale, byte newByte);

        /// <summary>
        /// Raises when a handshake has been received
        /// </summary>
        public event HandshakeReceivedDelegate HandshakeReceived;

        /// <summary>
        /// Determines whether this MapleClient is connected to an endpoint.
        /// </summary>
        /// <remarks>This indicator is always one step behind, as this only gets updated when the socket tries to either receive or send data.</remarks>
        public bool Connected
        {
            get
            {
                if (MapleSocket == null)
                    return false;
                else
                    return MapleSocket.Connected;
            }
        }

        /// <summary>
        /// Constructs a default MapleClient
        /// </summary>
        public MapleClient()
        {
            MapleSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            LocalCipher = new Cipher(Variables.AESKey);
            RemoteCipher = new Cipher(Variables.AESKey);
            DataBuffer = new byte[BufferSize];
        }

        /// <summary>
        /// Connects to the provided endpoint
        /// </summary>
        /// <param name="ep">The endpoint to connect to</param>
        public void Connect(IPEndPoint ep)
        {
            try
            {
                MapleSocket.Connect(ep);
            }
            catch(SocketException sEx)
            {
                //TODO: Handle exception
            }
        }

        /// <summary>
        /// Connects to the provided IP address and port
        /// </summary>
        /// <param name="ip">The IP address to connect to</param>
        /// <param name="port">The port on the IP address to connect to</param>
        public void Connect(string ip, ushort port)
        {
            IPAddress ipAddress;
            if (!IPAddress.TryParse(ip, out ipAddress))
                throw new NetworkingException("The provided IP address cannot be parsed.", 1001);

            try
            {
                MapleSocket.Connect(ipAddress, port);
            }
            catch (SocketException sEx)
            {
                //TODO: Handle exception
            }
        }

        /// <summary>
        /// Gracefully closes the network client
        /// </summary>
        public void Close()
        {
            if (MapleSocket != null)
                MapleSocket.Shutdown(SocketShutdown.Both);
        }

        /// <summary>
        /// Receives data from the underlying Socket
        /// </summary>
        public void Receive()
        {
            if(!Connected)
            {
                //TODO: Notify main thread of possible error
                return;
            }

            int received = 0;

            try
            {
                received = MapleSocket.Receive(DataBuffer, BufferOffset, DataBuffer.Length - BufferOffset, SocketFlags.None);
            }
            catch (SocketException sEx)
            {
                //TODO: Handle exception
            }

            if (received > 0)
                ProcessRawData(received);

        }

        /// <summary>
        /// Extracts packets from the DataBuffer
        /// </summary>
        private void ProcessRawData(int received)
        {
            int total = received + BufferOffset;
            int offset = 0;

            if (!Handshaken)
                offset += ParseHandshake();

            while (total - offset > RegularHeaderLength)
            {
                int length = GetPacketLength(DataBuffer, offset);

                if ((total - offset) < length)
                {
                    byte[] leftover = new ArraySegment<byte>(DataBuffer, offset, (total - offset)).ToArray();
                    Buffer.BlockCopy(leftover, 0, DataBuffer, 0, leftover.Length);
                    BufferOffset = leftover.Length;
                    return;
                }

                offset += RegularHeaderLength;

                byte[] packetData = new ArraySegment<byte>(DataBuffer, offset, length).ToArray();
                RemoteCipher.Decrypt(ref packetData);

                offset += length;

                PacketReceived?.Invoke(new PacketReader(ref packetData, IncomingPacketType.DynamicHeader));
            }
        }

        /// <summary>
        /// Parses the handshake from the received data
        /// </summary>
        private int ParseHandshake()
        {
            int offset = 0;

            ushort length = BitConverter.ToUInt16(DataBuffer, 0);
            offset = (HandshakeHeaderLength + length);

            byte[] handshakeData = new ArraySegment<byte>(DataBuffer, HandshakeHeaderLength, length).ToArray();

            PacketReader reader = new PacketReader(ref handshakeData, IncomingPacketType.NoHeader);
            ushort version = reader.ReadUInt16();
            ushort subversion = Convert.ToUInt16(reader.ReadMapleString());
            uint localVector = reader.ReadUInt32();
            uint remoteVector = reader.ReadUInt32();
            byte locale = reader.ReadInt8();
            byte isLoginContext = 255;

            if (reader.AbsoluteLength >= 15) //Decode CRC 'n shit
                isLoginContext = reader.ReadInt8();

            LocalCipher.Initialize(localVector, version);
            RemoteCipher.Initialize(remoteVector, version);

            HandshakeReceived?.Invoke(version, subversion, locale, isLoginContext);

            Handshaken = true;

            return offset;
        }

        /// <summary>
        /// Calculates the packet length
        /// </summary>
        /// <param name="buffer">The buffer the packet(s) are in</param>
        /// <param name="offset">The offset of the next packet</param>
        /// <returns></returns>
        private unsafe int GetPacketLength(byte[] buffer, int offset)
        {
            fixed (byte* pData = &buffer[offset])
            {
                return *(ushort*)pData ^ *((ushort*)pData + 1);
            }
        }

        /// <summary>
        /// Writes a MapleStory header to the packet
        /// </summary>
        public unsafe void WritePacketLength(ref byte[] data, int dataLength)
        {
            fixed (byte* pData = data)
            {
                *(ushort*)pData = (ushort)(LocalCipher.GameVersion ^ LocalCipher.IV.HIWORD);
                *((ushort*)pData + 1) = (ushort)(*(ushort*)pData ^ (dataLength - 4));
            }
        }

        /// <summary>
        /// Sends raw data onto the Socket
        /// </summary>
        public void Send(ref byte[] data, int length)
        {
            if (!MapleSocket.Connected)
                return;

            int sent = 0;

            while (sent < length)
            {
                try
                {
                    sent += MapleSocket.Send(data, length, SocketFlags.None);
                }
                catch (SocketException sEx)
                {
                    //TODO: Handle exception
                }
            }
        }

        /// <summary>
        /// Prepares a packet to be sent and sends it onto the Socket
        /// </summary>
        public void SendPacket(PacketWriter packet, bool reusePacket)
        {
            if (!Connected)
            {
                //TODO: Notify main thread of possible error
                return;
            }

            if (!reusePacket)
            {
                WritePacketLength(ref packet._DataBuffer, packet.Position);
                LocalCipher.Encrypt(ref packet._DataBuffer, packet.Position);

                Send(ref packet._DataBuffer, packet.Position);
            }
            else
            {
                byte[] data = packet;
                WritePacketLength(ref data, data.Length);
                LocalCipher.Encrypt(ref data, data.Length);

                Send(ref data, data.Length);
            }
        }
    }
}
