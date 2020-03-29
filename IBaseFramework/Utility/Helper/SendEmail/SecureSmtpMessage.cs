using System.Collections.Generic;
using System.Net.Mail;

namespace IBaseFramework.Utility.Helper.SendEmail
{
    /// <summary>
    /// Secure Smtp Message body
    /// </summary>
    /// <remarks>
    ///	The SecureSmtpMessage class represent the email message.
    /// </remarks>
    public class SecureSmtpMessage
    {
        /// <summary>
        /// Message subject line
        /// </summary>
        public string Subject;
        /// <summary>
        /// Message from address
        /// </summary>
        public MailAddress From;
        /// <summary>
        /// Message list of to addresses
        /// </summary>
        public List<MailAddress> To;
        /// <summary>
        /// Message list of CC addresses
        /// </summary>
        public List<MailAddress> Cc;
        /// <summary>
        /// Message list of BCC addresses
        /// </summary>
        public List<MailAddress> Bcc;
        /// <summary>
        /// Message root part. It can be content, attachment or multipart boundary.
        /// </summary>
        public SecureSmtpPart RootPart;

        /// <summary>
        /// Message default constructor
        /// </summary>
        public SecureSmtpMessage()
        {
            To = new List<MailAddress>();
            Cc = new List<MailAddress>();
            Bcc = new List<MailAddress>();
            return;
        }
    }
}
