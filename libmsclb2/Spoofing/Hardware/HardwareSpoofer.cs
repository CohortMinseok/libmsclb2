using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace libmsclb2.Spoofing.Hardware
{   
    public static class HardwareSpoofer
    {
        /// <summary>
        /// Generates a static, unique hardware profile per account, based on the provided username.
        /// </summary>
        /// <param name="username">The username to create the fake data with.</param>
        /// <returns>A 'valid', spoofed hardware profile</returns>
        public static HardwareProfileModel GenerateProfileFromUsername(string username)
        {
            HardwareProfileModel profile = new HardwareProfileModel();
            profile.CookieString = GenerateCookieString();

            using (SHA1 sha1 = SHA1CryptoServiceProvider.Create())
            {
                Random rng = new Random(Environment.TickCount);

                byte[] hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(username));
                Array.Copy(hash, 0, profile.MACAddress, 0, 6);
                profile.HDDSerial = BitConverter.ToUInt32(hash, 6);
                profile.Checksum = BitConverter.ToUInt16(hash, 10);
                profile.LocalIP[0] = (byte)rng.Next(Byte.MinValue, Byte.MaxValue);
                profile.LocalIP[1] = (byte)rng.Next(Byte.MinValue, Byte.MaxValue);
                profile.LocalIP[2] = (byte)rng.Next(Byte.MinValue, Byte.MaxValue);
                profile.LocalIP[3] = (byte)rng.Next(Byte.MinValue, Byte.MaxValue);
            }

            return profile;
        }

        /// <summary>
        /// Generates a random cookie string consisting out of lowercase, uppercase and numerical characters
        /// </summary>
        /// <remarks>Shamelessly stolen from our lord and savior Benny</remarks>
        private static string GenerateCookieString()
        {
            const string lowercase = "abcdefghijklmnopqrstuvwxyz";
            const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string numbers = "1234567890";

            Random rng = new Random(Environment.TickCount);

            StringBuilder builder = new StringBuilder(23);

            for (int i = 0; i < builder.Capacity; i++)
            {
                int rand = rng.Next(2);

                switch (rand)
                {
                    case 0:
                        builder.Append(lowercase.ToCharArray()[rng.Next(25)]);
                        break;
                    case 1:
                        builder.Append(uppercase.ToCharArray()[rng.Next(25)]);
                        break;
                    case 2:
                        builder.Append(numbers.ToCharArray()[rng.Next(25)]);
                        break;
                    default:
                        builder.Append('?');
                        break;
                }
            }

            return builder.ToString();
        }
    }
}
