using Microsoft.Azure.KeyVault;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AzureHelper
{
    /// <summary>
    /// Class to handle the getting and setting of keys in the azure key vault.
    /// </summary>
    public class KeyVault
    {
        //TODO: Find a better place to put this configuration information.
        //I was thinking of Azure, but we would need to know the secret name.  But as long as
        //  the secret name dstay the same across enviorments, we could do that.
        //Another idea is to store the data in an xml file and have the DLL access that.
        private const string CLIENT_ID = "1df87e4b-2863-4367-80e1-471138d9b7f0";
        private const string CLIENT_SECRET = "p3xavavcXKu3qCFo1kmnVb/O3YXAxFS9zqbRZd+MG7o=";
        private const string VAULT_LOCATION = "https://practicekeyvault.vault.azure.net/";

        /// <summary>
        /// THe keyVault client used to get ans set keys.
        /// </summary>
        private KeyVaultClient keyVaultClient;

        /// <summary>
        /// Credentials of the application that will access the azure keyvault.
        /// </summary>
        private ClientCredential clientCredentials;

        /// <summary>
        /// The URI of the key vault.
        /// </summary>
        private string vaultLocation;


        /// <summary>
        /// The encryption method used to encrypt the keys.
        /// </summary>
        private const string ENCRYPTION_METHOD = "RSA1_5";

        public KeyVault()
        {
            this.vaultLocation = GetVaultLoation();
            /* The two lines need to be ran in this order.*/
            this.clientCredentials = new ClientCredential(GetClientId(), GetClientSecret());
            this.keyVaultClient = new KeyVaultClient(GetAccessToken, GetHttpClient());
        }

        /// <summary>
        /// Makes a secret in Azure with the name and value.
        /// </summary>
        /// <param name="keyName">The name of the key (secret) for azure.</param>
        /// <param name="keyValue">The key used to encrypt and decrypt string.</param>
        /// <returns>A string representation of the location of the secret.</returns>
        public string SetEncryptionKey(string keyName, string keyValue)
        {
            Task<Secret> secretTask = keyVaultClient.SetSecretAsync(vaultLocation, keyName, keyValue);
            secretTask.Wait();

            Secret secret = secretTask.Result;

            return secret.Id;
        }

        /// <summary>
        /// Gets the key from azure with the specified location and name.
        /// </summary>
        /// <param name="keyName">The name of the key (Techinically the secret name)</param>
        /// <returns>A byte array representation of the key.</returns>
        public byte[] GetEncryptionKey(string keyName)
        {

            Task<Secret> secretTask = keyVaultClient.GetSecretAsync(vaultLocation, keyName);
            secretTask.Wait();

            Secret secret = secretTask.Result;

            return Encoding.ASCII.GetBytes(secret.Value);
        }

        /// <summary>
        /// Gets the HeepClient to use to connect to the keyvault.
        /// </summary>
        /// <returns>An HttpClient that is connected to the keyvult.</returns>
        private HttpClient GetHttpClient()
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://practicekeyvault.vault.azure.net/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return client;
        }

        /// <summary>
        /// Gets the access token
        /// </summary>
        /// <param name="authority"> Authority </param>
        /// <param name="resource"> Resource </param>
        /// <param name="scope"> scope </param>
        /// <returns> token </returns>
        public async Task<string> GetAccessToken(string authority, string resource, string scope)
        {
            var context = new AuthenticationContext(authority, TokenCache.DefaultShared);
            var result = await context.AcquireTokenAsync(resource, clientCredentials);

            return result.AccessToken;
        }

        /// <summary>
        /// Gets the id of the application that is registered in AAD that will communicate with
        /// Azure key vault
        /// </summary>
        /// <returns>A string representation of the client id</returns>
        private string GetClientId()
        {
            return CLIENT_ID;
        }

        /// <summary>
        /// Gets the client secret.  This is the secret when assigning a WebApi in the AAD
        /// </summary>
        /// <returns>A string representation of the client secret</returns>
        private string GetClientSecret()
        {
            return CLIENT_SECRET;
        }

        /// <summary>
        /// Gets the location of the vault.
        /// </summary>
        /// <returns>A string representation of the URI of the vault location</returns>
        private string GetVaultLoation()
        {
            return VAULT_LOCATION;
        }


    }
}

/* in case we want to go back to keys.*/

///// <summary>
///// Sets the specified key into azure and encryptes the key.
///// </summary>
///// <param name="vaultLocation">The location of the vault to store the key.</param>
///// <param name="keyName">The name of the key to store.</param>
///// <returns>A string representation of the URI to the encrypted key</returns>
//public string SetKey(string textToStore)
//{
//    /* Set the key */

//    Task<KeyBundle> keyBundleTask = keyVaultClient.CreateKeyAsync(vaultLocation, textToStore, "RSA");
//    keyBundleTask.Wait();

//    KeyBundle setKeyBundle = keyBundleTask.Result;

//    /* Encrypt the key*/
//    //Task<KeyOperationResult> encryptedKeyTask = keyVaultClient.EncryptAsync(setKeyBundle.KeyIdentifier.ToString(), ENCRYPTION_METHOD,
//    //    Encoding.ASCII.GetBytes(textToBeStored));
//    //encryptedKeyTask.Wait();
//    //KeyOperationResult encryptedKey = encryptedKeyTask.Result;


//    return setKeyBundle.KeyIdentifier.Identifier;

//}

///// <summary>
///// Gets a key from azure key vault
///// </summary>
///// <param name="locationOfKey">The URI of where the key resides.</param>
///// <param name="encryptedKey">The encrypted version of the key, which will be decrypted</param>
///// <returns>A byte array representation of the retrieved key</returns>
//public byte[] GetKey(string locationOfKey, byte[] encryptedKey)
//{
//    Task<KeyBundle> getKeyTask = keyVaultClient.GetKeyAsync(locationOfKey);
//    getKeyTask.Wait();

//    KeyBundle getKey = getKeyTask.Result;

//    List<byte> key = new List<byte>();
//    StringBuilder keyIdentifier = new StringBuilder(getKey.KeyIdentifier.Identifier);

//    while(keyIdentifier.Length > 0)
//    {
//        string keyByte = keyIdentifier.Remove(0, 3).ToString();
//        key.Add(byte.Parse(keyByte));
//    }
//    //Task<KeyOperationResult> decryptKeyTask = keyVaultClient.DecryptAsync(locationOfKey, ENCRYPTION_METHOD, encryptedKey);
//    //decryptKeyTask.Wait();

//    //KeyOperationResult decryptedKey = decryptKeyTask.Result;
//    //var hi = getKey.KeyIdentifier.
//    return key.ToArray();
//}