using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos
{
    /// <summary>
    /// DLL to be called when informaiton needs to be encrypted or decrypted.
    /// </summary>
    internal class Encryption
    {


        /// <summary>
        /// Use when there are no keys inm Azure and you need to make the keys for the first time.
        /// </summary>
        public static EncryptionInformation GetAuthCodes()
        {
            return new EncryptionInformation();
        }

        /// <summary>
        /// Encryptes the passed in string.
        /// </summary>
        /// <param name="stringToEncrypt">the string that needs to be encrypted</param>
        /// <param name="encryptionInformation">the keys for the encryption.</param>
        /// <returns>A byte array representation of the encrypted string.</returns>
        public static byte[] Encrypt(string stringToEncrypt, EncryptionInformation encryptionInformation)
        {
            return Encrypt(Encoding.ASCII.GetBytes(stringToEncrypt), encryptionInformation);
        }

        /// <summary>
        /// Encrypts the passed in byte array using the keys from the encryptionInformation
        /// </summary>
        /// <param name="dataToEncrypt">What needs to be encrypted</param>
        /// <param name="encryptionInformation">The keys for the encryption</param>
        /// <returns>A byte representation of the encrypted byte array.</returns>
        public static byte[] Encrypt(byte[] dataToEncrypt, EncryptionInformation encryptionInformation)
        {
            if (DateTime.Now > encryptionInformation.expirationDate)
            {
                encryptionInformation.UpdateKeys();
            }

            return EncryptionHelper.SimpleEncrypt(dataToEncrypt, encryptionInformation.cryptographyKey,
                encryptionInformation.authorizationKey);
        }

        /// <summary>
        /// Decrypts the encrypted data.
        /// </summary>
        /// <param name="encryptedData">The data to decrypt</param>
        /// <param name="encryptionInformation">THe keys used to decrypt the data.</param>
        /// <returns>Abyte array representation of the decrypted data</returns>
        public static byte[] Decrypt(byte[] encryptedData, EncryptionInformation encryptionInformation)
        {
            return EncryptionHelper.SimpleDecrypt(encryptedData, Configuration.CryptographyLocation,
                encryptionInformation.authorizationKey);
        }
    }
}
