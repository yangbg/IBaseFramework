using System;
using System.IO;
using System.Net.Mime;

namespace IBaseFramework.Utility.Helper.SendEmail
{
    /// <summary>
    /// Email attachment type
    /// </summary>
    public enum AttachmentType
    {
        /// <summary>
        /// Attachment file
        /// </summary>
        Attachment,
        /// <summary>
        /// Inline attachment. i.e. image for html content
        /// </summary>
        Inline,
    }

    /// <inheritdoc />
    ///  <summary>
    /// 	Email attachement
    ///  </summary>
    ///  <remarks>
    /// 	The SecureSmtpAttachment class is a child class of the
    /// 	SecureSmtpMessage class. It holds an email attachement.
    ///  </remarks>
    public class SecureSmtpAttachment : SecureSmtpPart
    {
        /// <summary>
        /// Content ID for linking attachment to html element
        /// </summary>
        public string ContentId;
        /// <summary>
        /// Attachment name
        /// </summary>
        public string Name;
        /// <summary>
        /// Attachement file name
        /// </summary>
        public string FileName;
        /// <summary>
        /// Medial type string i.e. text/html
        /// </summary>
        public string MediaType;
        /// <summary>
        /// Transfer encoding i.e. Base64 or QuotedPrintable
        /// </summary>
        public TransferEncoding ContentTransferEncoding;

        private AttachmentType _type;
        private readonly Stream _attachmentStream;

        /// <inheritdoc />
        /// <summary>
        /// Attachment from disk file
        /// </summary>
        /// <param name="type">Type of attachment</param>
        /// <param name="attachmentFileName">Attachment file name</param>
        /// <param name="contentId">Html content id</param>
        public SecureSmtpAttachment(AttachmentType type, string attachmentFileName, string contentId = null)
        {
            // load contents from stream
            _attachmentStream = new FileStream(attachmentFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            var ptr = attachmentFileName.LastIndexOf('\\');
            ConstructorHelper(type, attachmentFileName.Substring(ptr + 1), contentId);
            return;
        }

        /// <inheritdoc />
        /// <summary>
        /// Attachment from byte array
        /// </summary>
        /// <param name="type">Attachment type</param>
        /// <param name="byteArray">Byte array</param>
        /// <param name="name">Attachment name</param>
        /// <param name="contentId">Html content id</param>
        public SecureSmtpAttachment(AttachmentType type, byte[] byteArray, string name, string contentId = null)
        {
            _attachmentStream = new MemoryStream(byteArray);
            ConstructorHelper(type, name, contentId);
            return;
        }

        /// <inheritdoc />
        /// <summary>
        /// Attachment from stream
        /// </summary>
        /// <param name="type">Attachment type</param>
        /// <param name="attachmentStream">Attachment stream</param>
        /// <param name="name">Attachment name</param>
        /// <param name="contentId">Html content id</param>
        public SecureSmtpAttachment(AttachmentType type, Stream attachmentStream, string name, string contentId = null)
        {
            this._attachmentStream = attachmentStream;
            ConstructorHelper(type, name, contentId);
            return;
        }

        ////////////////////////////////////////////////////////////////////
        // Constructor helper
        ////////////////////////////////////////////////////////////////////
        private void ConstructorHelper(AttachmentType type, string name, string contentId)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ApplicationException("Attachment name is empty.");
            if (type == AttachmentType.Inline && string.IsNullOrWhiteSpace(contentId)) throw new ApplicationException("Content ID is empty.");
            this._type = type;
            this.Name = name;
            FileName = name;
            this.ContentId = contentId;
            MediaType = "application/octet";
            ContentTransferEncoding = TransferEncoding.Base64;
            return;
        }

        ////////////////////////////////////////////////////////////////////
        // Send data to server
        ////////////////////////////////////////////////////////////////////
        internal override void SendData(Stream writer)
        {
            this.Writer = writer;

            // all valid application subtypes as per IANA organization
            // http://www.iana.org/assignments/media-types/media-types.xhtml

            if (string.IsNullOrWhiteSpace(Name))
            {
                if (string.IsNullOrWhiteSpace(FileName)) throw new ApplicationException("Both Name and FileName are empty.");
                Name = FileName;
            }
            else if (string.IsNullOrWhiteSpace(FileName)) FileName = Name;

            // send content-type header
            SendDataLine($"content-type: {MediaType}; name={Name}");

            // send content-transfer-encoding header
            SendDataLine($"content-transfer-encoding: {TransferEncodingText(ContentTransferEncoding)}");

            // send content-disposition header
            SendDataLine($"content-disposition: {(_type == AttachmentType.Inline ? "inline" : "attachment")}; filename={FileName}");

            // send content-transfer-encoding header
            if (_type == AttachmentType.Inline)
            {
                SendDataLine($"content-id: <{ContentId}>");
            }

            // send extra CRLF
            SendEndOfLine();

            // content stream to base64
            const int bufLen = 57 * 1024;
            using (var reader = new BinaryReader(_attachmentStream))
            {
                for (; ; )
                {
                    // read first buffer
                    var buf = reader.ReadBytes(bufLen);
                    if (buf.Length == 0) break;
                    var strBuf = Convert.ToBase64String(buf, Base64FormattingOptions.InsertLineBreaks);
                    SendDataBlock(strBuf);
                }
            }

            // send plain text view
            return;
        }
    }
}
