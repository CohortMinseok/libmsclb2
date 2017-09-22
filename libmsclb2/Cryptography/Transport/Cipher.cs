using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libmsclb2.Cryptography.Transport
{
    public sealed class Cipher
    {
        /// <summary>
        /// The initialization vector, used as entropy for the AES transformer
        /// </summary>
        public InitializationVector IV { get; set; }

        /// <summary>
        /// Determines whether this CipherSuite is initialized
        /// </summary>
        public bool IsInitialized { get; set; }

        /// <summary>
        /// The major game version of the server
        /// </summary>
        /// <example>v180.1: 180 is the major version.</example>
        public ushort GameVersion { get; set; }

        /// <summary>
        /// Transforms blocks of data using the AES algorithm
        /// </summary>
        private FastAES AES { get; set; }
        
        /// <summary>
        /// Constructs a new Cipher, without initializing it
        /// </summary>
        public Cipher(ulong aesKey)
        {
            IV = new InitializationVector();
            AES = new FastAES(ExpandKey(aesKey));
        }

        /// <summary>
        /// Constructs and initializes a new Cipher
        /// </summary>
        /// <param name="iv">The initialization vector for this cipher</param>
        /// <param name="gameversion">The current, major game version</param>
        public Cipher(uint iv, ushort gameversion)
        {
            Initialize(iv, gameversion);
        }

        /// <summary>
        /// Initializes the underlying cryptographic services
        /// </summary>
        /// <param name="iv">The initialization vector for this cipher</param>
        /// <param name="gameversion">The current, major game version</param>
        public void Initialize(uint iv, ushort gameversion)
        {
            IV.Value = iv;
            GameVersion = gameversion;
            

            IsInitialized = true;
        }

        /// <summary>
        /// Encrypts outgoing data
        /// </summary>
        public void Encrypt(ref byte[] data, int offset)
        {
            if (!IsInitialized)
                throw new CryptoException("The cipher has not been initialized.", 1010);

            Transform(ref data, offset, 4);
        }

        /// <summary>
        /// Decrypts incoming data
        /// </summary>
        public void Decrypt(ref byte[] data)
        {
            if (!IsInitialized)
                throw new CryptoException("The cipher has not been initialized.", 1010);

            Transform(ref data, data.Length, 0);
        }

        /// <summary>
        /// Expands the key so it can be used
        /// </summary>
        private byte[] ExpandKey(ulong AESKey)
        {
            byte[] Expand = BitConverter.GetBytes(AESKey).Reverse().ToArray();
            byte[] Key = new byte[Expand.Length * 4];
            for (int i = 0; i < Expand.Length; i++)
                Key[i * 4] = Expand[i];
            return Key;
        }

        /// <summary>
        /// Applies Nexon's implementation of AES on the buffer
        /// </summary>
        /// <param name="buffer">The data to encrypt</param>
        /// <param name="amount">The amount of bytes to encrypt in the buffer</param>
        /// <param name="offset">The starting point in the buffer</param>
        private void Transform(ref byte[] buffer, int amount, int offset)
        {
            int remaining = amount - offset,
                length = 0x5B0,
                start = 0,
                index;

            byte[] realIV = new byte[sizeof(int) * 4],
                   IVBytes = IV.Bytes;

            while (remaining > 0)
            {
                for (index = 0; index < realIV.Length; ++index)
                    realIV[index] = IVBytes[index % 4];

                if (remaining < length) length = remaining;
                for (index = start; index < (start + length); ++index)
                {
                    if (((index - start) % realIV.Length) == 0)
                        AES.TransformBlock(ref realIV);

                    buffer[index + offset] ^= realIV[(index - start) % realIV.Length];
                }
                start += length;
                remaining -= length;
                length = 0x5B4;
            }
            IV.Shuffle();
        }
    }
}
