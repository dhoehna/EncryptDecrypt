using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos
{
    /// <summary>
    /// Holds the keys to decrypt the encrypted data.
    /// </summary>
    public class EncryptionInformation
    {
        private string AUTHENTICATION_KEY_NAME = "authKey";

        private string CRYPTOGRAPHY_KEY_NAME = "cryptKey";
        /// <summary>
        /// Authorization key for the encryption.
        /// </summary>
        public byte[] authorizationKey { get; set; }

        /// <summary>
        /// The location in azure for the secret for the auth key.
        /// </summary>
        public string authorizationLocation { get; set; }

        /// <summary>
        /// cryptography key for the encryptino.
        /// </summary>
        public byte[] cryptographyKey { get; set; }

        /// <summary>
        /// The location in azure of the secret for the cryptography key.
        /// </summary>
        public string cryptographyLocation { get; set; }

        public DateTime expirationDate { get; set; }


        /// <summary>
        /// Use when getting the kes for the first time.  Like, when there are no keys.
        /// Sets up the authKey, authLocation, cryptKey, cryptLocation, and expireationDate
        /// </summary>
        public EncryptionInformation()
        {

            //Look in configuration file.  If those are blank, than make new ones.

            /*
            What will be in the configuration file?
            Client Id
            Client Secret
            Key Vault location
            HttpClient base address
            Auth key location
            Crypt key location. 
            */
            KeyVault keyVaultContext = new KeyVault();

            //Make the authorizationKey
            /* I changed using keys to using secrets.  I can't use keys because the key name is stored
            in the URL. THe key name is a string and I need to convert the bytes to a string in order to stro the key name.
            Unfortunantly, the converted key would often contain illigal URL character, thus making storing
            the key impossible.
            
            I also tried to convert each individual byte to a string and ped it left with 0's until it had three characters,
            than concatonate all those string together.  But the process of encoding and decoding the key that was was too
            troublesome.
            
            So, I chose secrets.  That way I can use Encoding.ASCII.GetString(bytes) for ths secret and have the name as 
            something else.*/
            byte[] authKey = EncryptionHelper.NewKey();
            this.authorizationKey = authKey;
            this.authorizationLocation = keyVaultContext.SetEncryptionKey(AUTHENTICATION_KEY_NAME, Encoding.ASCII.GetString(authKey));

            byte[] cryptKey = EncryptionHelper.NewKey();
            this.cryptographyKey = cryptKey;
            this.cryptographyLocation = keyVaultContext.SetEncryptionKey(CRYPTOGRAPHY_KEY_NAME, Encoding.ASCII.GetString(cryptKey));

            this.expirationDate = DateTime.Now.AddMinutes(30);
        }


        /// <summary>
        /// Makes a new instance of this class.
        /// </summary>
        /// <param name="authorizationKey">Auth key for encrypting and decrypting string.</param>
        /// <param name="authorizationLocation">THe location in azure for the auth key</param>
        /// <param name="cryptographyKey">Crypt key for encrypting and decrypting strings.</param>
        /// <param name="cryptographyLocation">Location in aure for the crypt key</param>
        /// <param name="expirationDate">THe next date that the keys will need to be retrieved from azure.</param>
        public EncryptionInformation(byte[] authorizationKey, byte[] cryptographyKey,
                                     DateTime? expirationDate = null)
        {
            this.authorizationKey = authorizationKey;
            this.authorizationLocation = authorizationLocation;
            this.cryptographyKey = cryptographyKey;
            this.cryptographyLocation = cryptographyLocation;

            if (expirationDate != null)
            {
                this.expirationDate = (DateTime)expirationDate;
            }
            else
            {
                this.expirationDate = DateTime.Now.AddMinutes(30);
            }
        }

        /// <summary>
        /// Updates the keys and expiration date of the keys.
        /// </summary>
        public void UpdateKeys()
        {
            KeyVault keyVaultContext = new KeyVault();

            this.authorizationKey = keyVaultContext.GetEncryptionKey(AUTHENTICATION_KEY_NAME);
            this.cryptographyKey = keyVaultContext.GetEncryptionKey(CRYPTOGRAPHY_KEY_NAME);
            this.expirationDate.AddMinutes(30);
        }
    }
}
