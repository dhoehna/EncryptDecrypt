using System;
using System.Xml.Linq;

namespace Cosmos
{
    public static class Configuration
    {
        private static byte[] cryptographyKey;
        public static byte[] CryptographyKey
        {
            get
            {
                if(DateTime.Now > expirationDate)
                {
                    KeyVault keyVaultClient = new KeyVault();

                    cryptographyKey = keyVaultClient.GetEncryptionKey("cryptKey");
                }

                return cryptographyKey;
            }

            private set
            {
                cryptographyKey = value;
            }
        }

        private static byte[] authenticationKey;
        public static byte[] AuthenticationKey
        {
            get
            {
                if(DateTime.Now > expirationDate)
                {
                    KeyVault keyVaultClient = new KeyVault();

                    authenticationKey = keyVaultClient.GetEncryptionKey("authKey");
                }

                return authenticationKey;
            }

            private set
            {
                authenticationKey = value;
            }
        }

        public static DateTime expirationDate { get; private set; }
        /// <summary>
        /// The URI for the base address for the HttpClient when calling Azure.
        /// Since this application is only calling azure keyvault this should be set to
        /// the location for the key vault API.
        /// </summary>
        /// <requiredatinitiliation>
        /// Yes
        /// </requiredatinitiliation>
        public static string HttpClientCaseAddress { get; private set; }

        /// <summary>
        /// The location in azure of the authentication key.
        /// </summary>
        /// <requiredatinitiliation>
        /// No
        /// </requiredatinitiliation>
        public static string AuthenticationLocation { get; private set; }

        /// <summary>
        /// The location of the cryptography key in azure.
        /// </summary>
        /// <requiredatinitiliation>
        /// No
        /// </requiredatinitiliation>
        public static string CryptographyLocation { get; private set; }

        /// <summary>
        /// The client id that azure gives out when this application is registered in the
        /// AAD.
        /// </summary>
        /// <requiredatinitiliation>
        /// yes
        /// </requiredatinitiliation>
        public static string ClientId { get; private set; }

        /// <summary>
        /// The client secret that Azure gives out when the application is regestered in the 
        /// AAD.
        /// </summary>
        /// <requiredatinitiliation>
        /// yes
        /// </requiredatinitiliation>
        public static string ClientSecret { get; private set; }

        /// <summary>
        /// The address in Azure where the key vault is.
        /// </summary>
        /// <requiredatinitiliation>
        /// Yes
        /// </requiredatinitiliation>
        public static string KeyVaultAddress { get; private set; }

        static Configuration()
        {
            LoadConfiguration();
        }

        private static void LoadConfiguration()
        {
            expirationDate = DateTime.Now.AddMinutes(30); //In case one of the keys are referenced before the date is.
            XDocument configuration = XDocument.Load("Configuration.xml");

            if(configuration.Element("encryptdecryptconfiguration") == null)
            {
                throw new InvalidOperationException("No base element in the configuration file.  Please make sure the " +
                    " base elemtn is called encryptdecryptconfiguration");
            }

            string clientId = GetValue(configuration, "clientid");
            string clientSecret = GetValue(configuration, "clientsecret");
            string httpBaseAddress = GetValue(configuration, "httpclientBaseAddress");
            string keyVaultLocation = GetValue(configuration, "keyvaultlocation");


            if(clientId.Equals(string.Empty))
            {
                ReportMissingRequiredItem("clientid");
            }

            if(clientSecret.Equals(string.Empty))
            {
                ReportMissingRequiredItem("clientsecret");
            }

            if(httpBaseAddress.Equals(string.Empty))
            {
                ReportMissingRequiredItem("httpclientbaseaddress");
            }

            if(keyVaultLocation.Equals(string.Empty))
            {
                ReportMissingRequiredItem("keyvaultlocation");
            }

            AuthenticationLocation = GetValue(configuration, "authenticationkeylocation");
            CryptographyLocation = GetValue(configuration, "cryptographykeylocation");
            HttpClientCaseAddress = httpBaseAddress;
            ClientId = clientId;
            ClientSecret = clientSecret;
            KeyVaultAddress = keyVaultLocation;
        }

        private static void ReportMissingRequiredItem(string elementName)
        {
            throw new InvalidOperationException(elementName + " does not exist in the configuration.  " +
                " Please add a/an " + elementName + "element and try again");
        }

        private static string GetValue(XDocument configuration, string elementName)
        {
            XElement elementOfChoice = configuration.Element("EncryptDecryptConfiguration").Element(elementName);

            if(elementOfChoice == null)
            {
                throw new InvalidOperationException("There is no element in the configuration file with the title " +
                    " of " + elementName + ".  Make sure the element is typed correct in the program and xml file and try again");
            }

            XElement value = elementOfChoice.Element("value");

            if(value == null)
            {
                throw new InvalidOperationException("The element " + elementName + "does not have a \"value\" element." +
                    "  Please make sure this element exists and try again");
            }
            string sValue =  value.Value;

            if(string.IsNullOrEmpty(sValue) || string.IsNullOrWhiteSpace(sValue))
            {
                sValue = string.Empty;
            }

            return sValue;
        }
    }
}
