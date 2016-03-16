using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos
{
    /// <summary>
    /// Helper class to talk with COSMOS.
    /// At the moment, this is talking to a file on my desktop.
    /// </summary>
    public static class Communication
    {
        /// <summary>
        /// Encrypts than uploads the specified string to COSMOS
        /// </summary>
        /// <param name="stringToUpload">The string to encrypt and send to </param>
        /// <param name="encryptionInformation">Information about keys for encryption.</param>
        /// <returns>A guid used to find the record in COMOS</returns>
        public static Guid Upload(string stringToUpload)
        {
            byte[] encryptedData = Encryption.Encrypt(stringToUpload);
            return Upload(encryptedData);
        }

        /// <summary>
        /// Uploads data to COSMOS.
        /// </summary>
        /// <param name="dataToUpload">The bytes to stor in COSMOS</param>
        /// <returns>A guid used to get the data back from COSMOS</returns>
        public static Guid Upload(byte[] dataToUpload)
        {
            StreamWriter writer = new StreamWriter(@"C:\Users\Administrator\Desktop\EncryptedData.csv", true);

            Guid stringIdentifier = Guid.NewGuid();
            writer.WriteLine(stringIdentifier + "," + ByteArrayToString(dataToUpload));

            writer.Flush();
            writer.Dispose();

            return stringIdentifier;
        }

        /// <summary>
        /// Gets the bytes from COSMOS
        /// </summary>
        /// <param name="stringIdentifier">The identifier for the information you want.</param>
        /// <returns>A byte array representation</returns>
        public static byte[] Download(Guid stringIdentifier)
        {
            StringBuilder encryptedBytes = GetBytesFromFile(stringIdentifier);
            byte[] encryptedByteArray = TurnByteStringIntoByteArray(encryptedBytes);
            return Encryption.Decrypt(encryptedByteArray);

        }

        private static StringBuilder GetBytesFromFile(Guid identifier)
        {
            StreamReader reader = new StreamReader(@"C:\Users\Administrator\Desktop\EncryptedData.csv");
            bool foundString = false;
            StringBuilder encryptedBytes = new StringBuilder();
            reader.ReadLine(); //Skip header line
            while (!foundString && !reader.EndOfStream)
            {
                string currentLine = reader.ReadLine();

                string[] parts = currentLine.Split(',');

                if (Guid.Parse(parts[0]).Equals(identifier))
                {
                    encryptedBytes.Append(parts[1]);
                    foundString = true;
                }
            }

            reader.Dispose();

            return encryptedBytes;
        }

        private static byte[] TurnByteStringIntoByteArray(StringBuilder byteString)
        {
            if (byteString.Length == 0) //Return if their is nothing in the byte string
            {
                return new byte[0];
            }

            List<byte> byteList = new List<byte>();

            while (byteString.Length > 0)
            {
                byteList.Add(byte.Parse(byteString.ToString(0, 3)));
                byteString.Remove(0, 3);
            }

            return byteList.ToArray();
        }

        private static string ByteArrayToString(byte[] array)
        {
            StringBuilder builder = new StringBuilder();

            foreach (byte thisByte in array)
            {
                builder.Append(thisByte.ToString().PadLeft(3, '0'));
            }

            return builder.ToString();
        }

    }
}

