using System;
using System.IO;
using System.Net.Mime;
using System.Text;

namespace IBaseFramework.Utility.Helper.SendEmail
{
    /// <summary>
    /// Content type (plain or html)
    /// </summary>
    public enum ContentType
    {
        /// <summary>
        /// Plain text content
        /// </summary>
        Plain,
        /// <summary>
        /// HTML content
        /// </summary>
        Html,
    }

    /// <inheritdoc />
    ///  <summary>
    ///  Content part
    ///  </summary>
    ///  <remarks>
    /// 	The SecureSmtpCcontent class is a child class of the
    /// 	SecureSmtpMessage class. It holds the content plain text part,
    /// 	or the html part.
    ///  </remarks>
    public class SecureSmtpContent : SecureSmtpPart
    {
        /// <summary>
        /// Character set (override default utf-8)
        /// </summary>
        public string CharSet;
        /// <summary>
        /// Media type (override default  "text/html" or "text/plain")
        /// </summary>
        public string MediaType;
        /// <summary>
        /// Content transfer encoding (override QuotedPrintable)
        /// </summary>
        public TransferEncoding ContentTransferEncoding;

        private ContentType _type;
        private readonly string _contentString;

        /// <inheritdoc />
        /// <summary>
        /// Email content as a string
        /// </summary>
        /// <param name="type">Content media type</param>
        /// <param name="contentString">Content string</param>
        public SecureSmtpContent(ContentType type, string contentString)
        {
            this._contentString = contentString;
            ConstructorHelper(type);
            return;
        }

        /// <inheritdoc />
        /// <summary>
        /// Email content as a stream
        /// </summary>
        /// <param name="type">Content media type</param>
        /// <param name="contentStream">Content stream</param>
        public SecureSmtpContent(ContentType type, Stream contentStream)
        {
            // read content string from the stream
            using (var reader = new StreamReader(contentStream))
            {
                _contentString = reader.ReadToEnd();
            }
            ConstructorHelper(type);
            return;
        }

        ////////////////////////////////////////////////////////////////////
        // Constructor helper
        ////////////////////////////////////////////////////////////////////
        private void ConstructorHelper(ContentType type)
        {
            if (string.IsNullOrWhiteSpace(_contentString)) throw new ApplicationException("Content string is empty.");
            this._type = type;
            MediaType = type == ContentType.Html ? "text/html" : "text/plain";
            CharSet = "utf-8";
            ContentTransferEncoding = TransferEncoding.QuotedPrintable;
            return;
        }

        ////////////////////////////////////////////////////////////////////
        // Send content to output stream
        ////////////////////////////////////////////////////////////////////
        internal override void SendData(Stream writer)
        {
            // save output stream
            this.Writer = writer;

            // send content-type header
            SendDataLine($"content-type: {MediaType}; charset={CharSet}{(_type == ContentType.Html ? "" : "; format=flowed")}");

            // send content-transfer-encoding header
            SendDataLine($"content-transfer-encoding: {TransferEncodingText(ContentTransferEncoding)}");

            // send extra CRLF
            SendEndOfLine();

            // convert content string unicode to byte array
            var contentBytes = Encoding.UTF8.GetBytes(_contentString);
            char[] charBuffer = null;
            string encodedText = null;
            switch (ContentTransferEncoding)
            {
                case TransferEncoding.QuotedPrintable:
                    encodedText = QuotedPrintableEncode(contentBytes);
                    break;

                case TransferEncoding.Base64:
                    encodedText = Convert.ToBase64String(contentBytes, Base64FormattingOptions.InsertLineBreaks);
                    break;

                case TransferEncoding.SevenBit:
                    charBuffer = new char[contentBytes.Length];
                    for (var index = 0; index < contentBytes.Length; index++)
                    {
                        if ((contentBytes[index] & 0x80) != 0 || contentBytes[index] == 0) throw new ApplicationException("Seven bit transfer encoding failed.");
                        charBuffer[index] = (char)contentBytes[index];
                    }
                    encodedText = new string(charBuffer);
                    break;

                case TransferEncoding.EightBit:
                    charBuffer = new char[contentBytes.Length];
                    for (var index = 0; index < contentBytes.Length; index++)
                    {
                        if (contentBytes[index] == 0) throw new ApplicationException("Eight bit transfer encoding failed.");
                        charBuffer[index] = (char)contentBytes[index];
                    }
                    encodedText = new string(charBuffer);
                    break;
                case TransferEncoding.Unknown:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // send plain text view
            SendDataBlock(encodedText);
            return;
        }
    }
}
