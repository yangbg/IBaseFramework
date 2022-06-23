using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace IBaseFramework.Helper.SendEmail
{
    /// <summary>
    ///	The SecureSmtpRefreshToken class holds the refresh token info.
    /// </summary>
    public class SecureSmtpRefreshToken
    {
        /// <summary>
        /// client ID
        /// </summary>
        public string ClientId;

        /// <summary>
        /// client secret
        /// </summary>
        public string ClientSecret;

        /// <summary>
        /// refresh token
        /// </summary>
        public string RefreshToken;

        /// <summary>
        /// Save Program State
        /// </summary>
        /// <param name="fileName">Refresh token file name.</param>
        public void SaveState(string fileName)
        {
            // create a new program state file
            using (var textFile = new XmlTextWriter(fileName, null))
            {
                // create xml serializing object
                var xmlFile = new XmlSerializer(typeof(SecureSmtpRefreshToken));

                // serialize the program state
                xmlFile.Serialize(textFile, this);
            }

            // exit
        }

        ////////////////////////////////////////////////////////////////////
        // Load Program State
        ////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Load Program State
        /// </summary>
        /// <param name="fileName">Refresh token file name</param>
        /// <returns>Refresh token class</returns>
        public static SecureSmtpRefreshToken LoadState(string fileName)
        {
            // program state file does not exist
            if (!File.Exists(fileName)) return (null);

            XmlTextReader textFile = null;
            SecureSmtpRefreshToken state;
            try
            {
                // read program state file
                textFile = new XmlTextReader(fileName);

                // create xml serializing object
                var xmlFile = new XmlSerializer(typeof(SecureSmtpRefreshToken));

                // deserialize the program state
                state = (SecureSmtpRefreshToken)xmlFile.Deserialize(textFile);
            }
            catch
            {
                state = null;
            }

            // close the file
            textFile?.Close();

            // exit
            return state;
        }
    }
}
