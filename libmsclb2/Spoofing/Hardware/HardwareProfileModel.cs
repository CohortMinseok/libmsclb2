using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libmsclb2.Spoofing.Hardware
{
    /// <summary>
    /// Collection of computer properties that Nexon uses to identify and track players
    /// </summary>
    public sealed class HardwareProfileModel
    {
        /// <summary>
        /// Unique identifier assigned to network interfaces for communications on the physical network segment
        /// </summary>
        public byte[] MACAddress { get; set; }

        /// <summary>
        /// Serial number of the hard disk
        /// </summary>
        public uint HDDSerial { get; set; }

        /// <summary>
        /// IP Address used in the local NAT network
        /// </summary>
        public byte[] LocalIP { get; set; }

        /// <summary>
        /// Cookie string
        /// </summary>
        public string CookieString { get; set; }

        /// <summary>
        /// Checksum based on the HardwareId
        /// </summary>
        public ushort Checksum { get; set; }

        public HardwareProfileModel()
        {
            MACAddress = new byte[6];
            LocalIP = new byte[4];
        }
    }
}
