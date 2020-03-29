using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using IBaseFramework.Utility.Helper.SendEmail;

namespace IBaseFramework.Utility.Helper
{
    /// <summary>
    /// 邮件发送类
    /// </summary>
    public class EmailHelper
    {
        public static bool SendEmailSecure(string host, int port, string accountEmail, string password, string displayName, string subject, string body, List<string> toAddress, byte[] file = null, string fileName = null)
        {

            try
            {
                SendEmail(1, host, port, accountEmail, password, subject, body, accountEmail, displayName, toAddress, null, file, fileName);
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }
        public static bool SendEmailUnsecure(string host, int port, string accountEmail, string password, string displayName, string subject, string body, List<string> toAddress, byte[] file = null, string fileName = null)
        {
            try
            {
                SendEmail(2, host, port, accountEmail, password, subject, body, accountEmail, displayName, toAddress, null, file, fileName);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool SendEmailOAuth2(int type, string host, int port, string accountEmail, string displayName, string subject, string body, List<string> toAddress, string refreshToken, byte[] file = null, string fileName = null)
        {
            try
            {
                SendEmail(0, host, port, accountEmail, null, subject, body, accountEmail, displayName, toAddress, refreshToken, file, fileName);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #region SendEmail
        private static void SendEmail(int type, string host, int port, string accountEmail, string password, string subject, string body, string fromAddress, string fromUserName, List<string> toAddress, string refreshToken = null, byte[] file = null, string fileName = null)
        {
            try
            {
                // create one of three possible connection classes
                // the SecureSmtpClient is re-useable. If you send more than one email,
                // you can reuse the class. It is of real benefit for gmail.
                SecureSmtpClient connection = null;
                switch (type)
                {
                    case 0:
                        connection = new SecureSmtpClient(host, accountEmail, new SecureSmtpOAuth2(refreshToken));
                        break;
                    case 1:
                        connection = new SecureSmtpClient(ConnectMethod.Secure, host, accountEmail, password);
                        break;
                    case 2:
                        connection = new SecureSmtpClient(ConnectMethod.Unsecure, host, accountEmail, password);
                        break;
                }

                if (connection == null) throw new Exception("SecureSmtpClient can not NULL");

                connection.PortNo = port;
                connection.Timeout = 20;

                // create related boundary
                var related = new SecureSmtpMultipart(MultipartType.Related);
                // add html mail body content
                related.AddPart(new SecureSmtpContent(ContentType.Html, body));

                // add inline image attachment.
                // NOTE image id is set to IMAGE001 this id must match the html image id in HtmlView text.
                //related.AddPart(new SecureSmtpAttachment(AttachmentType.Inline, image, DateTime.Now.ToString("yyyyMMddHHmmss"))
                //{
                //    MediaType = "image/jpg"
                //});


                // create alternative boundary
                var alternative = new SecureSmtpMultipart(MultipartType.Alternative);
                //Add plain text mail body contents.
                //alternative.AddPart(new SecureSmtpContent(ContentType.Plain, body));
                alternative.AddPart(related);

                // create mixed multipart boundary
                var mixed = new SecureSmtpMultipart(MultipartType.Mixed);
                mixed.AddPart(alternative);

                if (file != null && file.Length > 0 && !string.IsNullOrEmpty(fileName))
                {
                    // add file attachment to the email.
                    // The recipient of the email will be able to save it as a file.
                    mixed.AddPart(new SecureSmtpAttachment(AttachmentType.Attachment, file, fileName));
                }

                // create mail message object
                var message = new SecureSmtpMessage
                {
                    Subject = subject, // Set subject
                    From = new MailAddress(fromAddress, fromUserName), // Set mail from address and display name.
                    RootPart = mixed, // create mixed multipart boundary
                    To = toAddress.Select(address => new MailAddress(address, address)).ToList() // Add minimum one or more recipients.
                };

                // send mail
                connection.SendMail(message);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
    }
}
