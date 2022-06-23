using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace IBaseFramework.Helper.SendEmail
{
    // must be in that order
    /// <summary>
    /// Multipart type
    /// </summary>
    public enum MultipartType
    {
        /// <summary>
        /// Mixed
        /// </summary>
        Mixed,
        /// <summary>
        /// Alternative
        /// </summary>
        Alternative,
        /// <summary>
        /// Related
        /// </summary>
        Related,
    }

    /// <inheritdoc />
    ///  <summary>
    ///  Multipart constructor
    ///  </summary>
    ///  <remarks>
    /// 	The SecureSmtpMultipart class is a child class of SecureSmtpMessage
    /// 	class. It holds multipart boundary information.
    ///  </remarks>
    public class SecureSmtpMultipart : SecureSmtpPart
    {
        private readonly MultipartType _type;
        private string _boundary;
        private readonly List<SecureSmtpPart> _children;

        private static readonly string[] MultipartText = { "mixed", "alternative", "related" };
        private static readonly string[] MultipartCode = { "MIX", "ALT", "REL" };

        /// <inheritdoc />
        /// <summary>
        /// Multipart constructor
        /// </summary>
        /// <param name="type">Multipart type: Mixed, Alternative, Related</param>
        public SecureSmtpMultipart(MultipartType type)
        {
            _type = type;
            _children = new List<SecureSmtpPart>();
        }

        /// <summary>
        /// Add child part to list of parts
        /// </summary>
        /// <param name="part">Derived class from SecureSmtpPart</param>
        public void AddPart(SecureSmtpPart part)
        {
            _children.Add(part);
        }

        ////////////////////////////////////////////////////////////////////
        // Create string with random hex digits
        ////////////////////////////////////////////////////////////////////
        private static string RandomString(int length)
        {
            var byteArray = new byte[length];
            using (var randNumGen = new RNGCryptoServiceProvider())
            {
                randNumGen.GetBytes(byteArray);
            }
            var str = new StringBuilder();
            foreach (var oneByte in byteArray)
            {
                str.Append(Hex[oneByte >> 4]);
                str.Append(Hex[oneByte & 0x0F]);
            }
            return (str.ToString());
        }

        ////////////////////////////////////////////////////////////////////
        // Send multipart headers to output stream
        ////////////////////////////////////////////////////////////////////
        internal override void SendData(Stream writer)
        {
            // save output writer stream
            Writer = writer;

            // no children
            if (_children.Count == 0) return;

            // create boundary
            _boundary = $"--------{MultipartCode[(int)_type]}-{RandomString(12)}";

            // send multipart header
            SendDataLine(string.Format("Content-Type: multipart/{0};\r\n boundary=\"{1}\"\r\n\r\n--{1}", MultipartText[(int)_type], _boundary));

            // send children data
            for (var index = 0; index < _children.Count; index++)
            {
                // send data of next child
                _children[index].SendData(writer);

                // send boundry line or last boundry line
                var boundaryLine = "\r\n--" + _boundary;
                if (index == _children.Count - 1) boundaryLine = boundaryLine + "--";
                SendDataLine(boundaryLine);
            }
        }
    }
}
