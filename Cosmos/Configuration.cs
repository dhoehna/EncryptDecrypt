using System;
using System.Xml.Linq;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
namespace Cosmos
{
    /// <summary>
    /// Class to handle all the configurable information.
    /// </summary>
    public static class Configuration
    {
        private const string CRYPTOGRAPHY_KEY_NAME = "cryptKey";
        private const string AUTHENTICATION_KEY_NAME = "authKey";

        private const string CONFIGURATION_FILE_NAME = "Configuration.xml";
        private const string CONFIGURATION_BASE_ELEMENT = "encryptdecryptconfiguration";
        private const string CLIENT_ID = "clientid";
        private const string CLIENT_SECRET = "clientsecret";
        private const string HTTP_BASE_ADDRESS = "httpclientbaseaddress";
        private const string KEY_VAULT_LOCATION = "keyvaultlocation";
        private const string AUTHENTICATION_KEY_LOCATION = "authenticationkeylocation";
        private const string CRYPTOGRAPHY_KEY_LOCATION = "cryptographykeylocation";
        private const string AUTHENTICATION_KEY = "authenticationkey";
        private const string CRYPTOGRAPHY_KEY = "cryptographykey";
        private const string EXPIRATION_DATE = "expirationdate";
        private const string EXPIRATION_DATE_FORMAT = "MM/dd/yyyy HH:mm:ss";
        private const string MINUTES_UNTIL_KEY_EXPIRATION = "minutesuntiltokensexpire";
        private const string REQUIRED_AT_STARTUP = "requiredatstartup";


        private static XDocument configuration { get; set; }
        /// <summary>
        /// The cryptography key used to encrypt and decrypt information.
        /// The program will automatically grab the keys from azure when the keys have
        /// expired.
        /// </summary>
        private static byte[] cryptographyKey = null;
        public static byte[] CryptographyKey
        {
            get
            {
                if (cryptographyKey == null || DateTime.Now > ExpirationDate)
                {
                    cryptographyKey = KeyVault.Instance.GetEncryptionKey(CRYPTOGRAPHY_KEY_NAME);
                    ExpirationDate = DateTime.Now.AddMinutes(MinutesUntilExpiration);
                    SetValue(EXPIRATION_DATE, ExpirationDate.ToString(EXPIRATION_DATE_FORMAT));
                }

                return cryptographyKey;
            }

            private set
            {
                cryptographyKey = value;
            }
        }

        /// <summary>
        /// The authentication key used to encrypt and decrypt data.
        /// The program will automatically get a new key when the expiration date is passed.
        /// </summary>
        private static byte[] authenticationKey = null;
        public static byte[] AuthenticationKey
        {
            get
            {
                if (authenticationKey == null || DateTime.Now > ExpirationDate)
                {
                    authenticationKey = KeyVault.Instance.GetEncryptionKey(AUTHENTICATION_KEY_NAME);
                    ExpirationDate = DateTime.Now.AddMinutes(MinutesUntilExpiration);
                    SetValue(EXPIRATION_DATE, ExpirationDate.ToString(EXPIRATION_DATE_FORMAT));
                }

                return authenticationKey;
            }

            private set
            {
                authenticationKey = value;
            }
        }


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

        /// <summary>
        /// THe date on when the keys expire and they will need to be fetched from azure again.
        /// </summary>
        public static DateTime ExpirationDate { get; private set; }

        /// <summary>
        /// How many minutes the program can keep using the same keys until 
        /// the keys have to be fetched from azure again.
        /// </summary>
        public static int MinutesUntilExpiration { get; private set; }

        static Configuration()
        {
            LoadConfiguration();
        }

        /// <summary>
        /// Loads the configuration file into memeory and makes sure all required componenets of the
        /// configuration file are present.
        /// This will also make the two encryption keys if they dont exist.
        /// </summary>
        /// <assumption>
        /// The key does not exist in Azure if the location is empty.
        /// </assumption>
        private static void LoadConfiguration()
        {
            //Make sure the file exists and has a base element
            if (!File.Exists(CONFIGURATION_FILE_NAME))
            {
                throw new InvalidOperationException("The configuration file does not exist at " + Path.GetFullPath(CONFIGURATION_FILE_NAME));
            }

            configuration = XDocument.Load(CONFIGURATION_FILE_NAME);

            if (configuration.Element(CONFIGURATION_BASE_ELEMENT) == null)
            {
                throw new InvalidOperationException("No base element in the configuration file.  Please make sure the " +
                    " base element exists and is called " + CONFIGURATION_BASE_ELEMENT);
            }

            MakeSureRequiredValuesExist();

            /* If we get to here, than the configuration file is correct.*/

            //Assign the values.
            AuthenticationLocation = GetValue(AUTHENTICATION_KEY_LOCATION);
            CryptographyLocation = GetValue(CRYPTOGRAPHY_KEY_LOCATION);
            HttpClientCaseAddress = GetValue(HTTP_BASE_ADDRESS);
            ClientId = GetValue(CLIENT_ID);
            ClientSecret = GetValue(CLIENT_SECRET);
            KeyVaultAddress = GetValue(KEY_VAULT_LOCATION);
            MinutesUntilExpiration = Int32.Parse(GetValue(MINUTES_UNTIL_KEY_EXPIRATION));
            SetExpirationDate();


            //Assign the keys
            /* The following code must be ran after all the other initilization code.
                If AuthenticationLocation is empty it will use KeyVault to make new keys, and KeyVault
                uses configuration vaules to store the keys.  
            */
            if (!AuthenticationLocation.Equals(string.Empty))
            {
                StringBuilder authKey = new StringBuilder(configuration.Element(CONFIGURATION_BASE_ELEMENT).Element(AUTHENTICATION_KEY).Element("value").Value);
                AuthenticationKey = TurnByteStringIntoByteArray(authKey);
            }
            else if (AuthenticationLocation.Equals(string.Empty))
            {
                AuthenticationKey = EncryptionHelper.NewKey();
                AuthenticationLocation = KeyVault.Instance.SetEncryptionKey(AUTHENTICATION_KEY_NAME,
                    Encoding.ASCII.GetString(AuthenticationKey));

                SetValue(AUTHENTICATION_KEY_LOCATION, AuthenticationLocation);
                SetValue(AUTHENTICATION_KEY, ByteArrayToString(AuthenticationKey));
                
            }

            CryptographyLocation = GetValue(CRYPTOGRAPHY_KEY_LOCATION);

            if (!CryptographyLocation.Equals(string.Empty))
            {
                StringBuilder cryptKey = new StringBuilder(configuration.Element(CONFIGURATION_BASE_ELEMENT).Element(AUTHENTICATION_KEY).Element("value").Value);
                CryptographyKey = TurnByteStringIntoByteArray(cryptKey);
            }
            else if (CryptographyLocation.Equals(string.Empty))
            {
                CryptographyKey = EncryptionHelper.NewKey();
                CryptographyLocation = KeyVault.Instance.SetEncryptionKey(CRYPTOGRAPHY_KEY_NAME, Encoding.ASCII.GetString(CryptographyKey));

                SetValue(CRYPTOGRAPHY_KEY_LOCATION, CryptographyLocation);
                SetValue(CRYPTOGRAPHY_KEY, ByteArrayToString(CryptographyKey));
            }
        }

        /// <summary>
        /// Sets the expiration date for the configruation.  Also,
        /// writes the expiration date if it does not exist already.
        /// </summary>
        /// <param name="configuration"></param>
        public static void SetExpirationDate()
        {
            string expirationDate = GetValue(EXPIRATION_DATE);

            if (!expirationDate.Equals(string.Empty))
            {
                ExpirationDate = DateTime.Parse(expirationDate);
            }
            else
            {
                ExpirationDate = DateTime.Now.AddMinutes(double.Parse(GetValue(MINUTES_UNTIL_KEY_EXPIRATION)));
                SetValue(EXPIRATION_DATE, ExpirationDate.ToString(EXPIRATION_DATE_FORMAT));
            }
        }

        /// <summary>
        /// Makes sure that all elements in the configuration with true for "requiredatstartup"
        /// have something in their child, value, element.
        /// If something is wrong, than an exception will be thrown.
        /// </summary>
        /// <param name="configuration">The configuration file.</param>
        private static void MakeSureRequiredValuesExist()
        {
            List<XElement> requiredValues = null;

            try
            {
                requiredValues = configuration.Element(CONFIGURATION_BASE_ELEMENT).Elements().Where(x => x.Attribute(REQUIRED_AT_STARTUP).Value.ToLower()
                .Equals("true")).Select(x => x.Element("value")).ToList();
            }
            catch(TypeInitializationException ex)
            {
                throw new Exception("An element is missing the " + REQUIRED_AT_STARTUP + "attribute", ex);
            }

            foreach(XElement requiredValue in requiredValues)
            {
                string value = GetValue(requiredValue.Parent.Name.ToString());

                if(value.Equals(string.Empty))
                {
                    ReportMissingElement(requiredValue.Parent.Name.ToString(), true);
                }
            }
        }

        /// <summary>
        /// Throws an exception that tells the user what element is missing.
        /// </summary>
        /// <param name="elementName">The name of the element that is missing from the configuration file.</param>
        private static void ReportMissingElement(string elementName, bool required)
        {
            if (required)
            {
                throw new InvalidOperationException("The required element " + elementName + " does not have a value.  " +
                    "Please fill in a value for the element " + elementName + " and try again");
            }
            else
            {
                throw new InvalidOperationException("There is no element in the configuration file with the title " +
                        " of " + elementName + ".  Make sure the element is typed correct in the program and xml file and try again");
            }
        }


        /// <summary>
        /// Gets a value from the configuration file.
        /// </summary>
        /// <param name="configuration">The configuration file.</param>
        /// <param name="elementName">THe name of the element to look for.</param>
        /// <returns>A string representation of the value in the element.</returns>
        private static string GetValue(string elementName)
        {
            XElement elementOfChoice = configuration.Element("encryptdecryptconfiguration").Element(elementName);

            if (elementOfChoice == null)
            {
                ReportMissingElement(elementName, false);
            }

            XElement value = elementOfChoice.Element("value");

            if (value == null)
            {
                throw new InvalidOperationException("The element " + elementName + "does not have a \"value\" element." +
                    "  Please make sure this element exists and try again");
            }
            string sValue = value.Value;

            if (string.IsNullOrEmpty(sValue) || string.IsNullOrWhiteSpace(sValue))
            {
                sValue = string.Empty;
            }

            return sValue;
        }

        /// <summary>
        /// Sets a value in the confiuration file for the specified element, than saves the file.
        /// </summary>
        /// <param name="configuration">The configuration file.</param>
        /// <param name="elementName">The name of the elment to update.</param>
        /// <param name="value">The vaule of the element.</param>
        private static void SetValue(string elementName, string value)
        {
            configuration.Element(CONFIGURATION_BASE_ELEMENT).Element(elementName).Element("value").Value = value;
            configuration.Save(CONFIGURATION_FILE_NAME);
        }

        /// <summary>
        /// Turns the byte array into a string representation of all the bytes.
        /// </summary>
        /// <param name="array">The byte array to turn into a string.</param>
        /// <returns></returns>
        private static string ByteArrayToString(byte[] array)
        {
            StringBuilder builder = new StringBuilder();

            foreach (byte thisByte in array)
            {
                builder.Append(thisByte.ToString().PadLeft(3, '0'));
            }

            return builder.ToString();
        }

        /// <summary>
        /// Converts a string into a byte array.
        /// </summary>
        /// <param name="byteString">The stringbuilder to convert.</param>
        /// <returns>A byte array.</returns>
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

    }
}
