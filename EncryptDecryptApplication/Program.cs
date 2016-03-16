using Cosmos;
using System;
using System.Text;

namespace EncryptDecryptApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            EncryptionInformation information = new EncryptionInformation();
            Guid identifier = Communication.Upload("Hello, it is me");
            byte[] stuff = Communication.Download(identifier, information);

            Console.WriteLine(Encoding.ASCII.GetString(stuff));
           // //Get the authcodes.
           // EncryptionInformation encryptionInformtion = Encryption.GetAuthCodes();

           // //Encrypt some information.
           // byte[] encryptedString = Encryption.Encrypt("Hello, I have been encrypted before", encryptionInformtion);

           // //Upload data to cosmos
           //Guid location = CommunicationHelper.Upload(encryptedString);

           // Thread.Sleep(2000); /* Sending data to COSMOS*/

           // //download data from cosmos
           // byte[] encryptedData = CommunicationHelper.Download(location);

           // byte[] decryptedData = Encryption.Decrypt(encryptedData, encryptionInformtion);

           // Console.WriteLine("Is your message: " + Encoding.ASCII.GetString(decryptedData));



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
