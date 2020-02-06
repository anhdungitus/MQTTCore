using System;
using System.Security.Cryptography;
using System.Text;

namespace MQTT2.Utilities
{
    public class Cryptography
    {
        public static string CreateShaHash(string unencryptedString)
        {
            UTF8Encoding utf8Encoder = new UTF8Encoding();
            byte[] aryBytes = utf8Encoder.GetBytes(unencryptedString);
            SHA512Managed sha = new SHA512Managed();
            byte[] bHashedData = sha.ComputeHash(aryBytes);
            return BitConverter.ToString(bHashedData).Replace("-", "");
        }
    }
}