using Cosmos;
using EncryptDecrypt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EncryptDecryptApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            //Get the authcodes.
            EncryptionInformation encryptionInformtion = Encryption.GetAuthCodes();

            //Encrypt some information.
            byte[] encryptedString = Encryption.Encrypt("Hello, I have been encrypted before", encryptionInformtion);

            //Upload data to cosmos
            //CommunicationHelper.Upload(encryptedString);

            Thread.Sleep(2000); /* Sending data to COSMOS*/

            //download data from cosmos
            //CommunicationHelper.Download("Here is my location");

            byte[] decryptedData = Encryption.Decrypt(encryptedString, encryptionInformtion);

            Console.WriteLine("Is your message: " + Encoding.ASCII.GetString(decryptedData));



            /*
            Changes
            Make a key if it does not exist (1st time only)
            Use keys API, not secret
            see if keys API gives something to encrypt the keys.
            Encrypt string not files
            after a pre-determined amount of time, retrieve the key from azure again.
            */

        }
    }
}
