using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace IBaseFramework.Helper.SendEmail
{
    /// <summary>
    /// OAuth2 interface
    /// </summary>
    public interface IOAuth2
    {
        /// <summary>
        /// Get OAuth2 password
        /// </summary>
        /// <returns>OAuth2 password</returns>
        string OAuth2Password();
    }

    /// <summary>
    /// Connection method
    /// </summary>
    public enum ConnectMethod
    {
        /// <summary>
        /// Unsecure (not using SSL stream)
        /// </summary>
        Unsecure,
        /// <summary>
        /// Secure (using SSL stream)
        /// </summary>
        Secure,
    }

    /// <inheritdoc />
    ///  <summary>
    ///  Class for handling an SMTP connection.
    ///  </summary>
    ///  <remarks>
    /// 	The SecureSmtpClient class handles the communication
    /// 	between your application and the mail server.
    ///  </remarks>
    public class SecureSmtpClient : IDisposable
    {
        // Authentication method
        private enum Authentication
        {
            PlainText,
            OAuth2,
        }

        // List of SMTP reply codes used by this program
        private enum SmtpReplyCode
        {
            IgnoreReply = 0,
            Ready = 220,
            ClosingChannel = 221,
            AuthenticationSuccessful = 235,
            RequestCompleted = 250,
            SendUserAndPassword = 334,
            StartInput = 354,
        }
        [Flags]
        private enum AuthenticationMethod
        {
            None = 0,
            PlainText = 1,
            XoAuth2 = 2,
        }

        // constructor properties
        private readonly Authentication _authMethod;
        private readonly ConnectMethod _connMethod;
        private readonly string _host;
        private int _port = 465;
        private readonly string _userName;
        private readonly string _userPassword;
        private int _timeoutMs = 20000;
        private readonly IOAuth2 _oAuth2;

        // properties
        private List<string> _replyStrings;

        public string Certificate { get; private set; }
        public string PolicyErrors { get; private set; }
        private TcpClient _socket;
        private StreamReader _reader;
        private Stream _writer;
        private AuthenticationMethod _supportedAuth;

        /// <summary>
        /// Secure SMTP Client constructor for plain text authorization
        /// </summary>
        /// <param name="connMethod">Connect method (secure/Unsecure)</param>
        /// <param name="host">Email server host name</param>
        /// <param name="userName">User name (host login id)</param>
        /// <param name="userPassword">User password (host login password)</param>
        public SecureSmtpClient(ConnectMethod connMethod, string host, string userName, string userPassword)
        {
            // test all required values
            if (string.IsNullOrWhiteSpace(host)) throw new ApplicationException("Host name is missing");
            if (string.IsNullOrWhiteSpace(userName)) throw new ApplicationException("User name is missing");
            if (string.IsNullOrWhiteSpace(userPassword)) throw new ApplicationException("User password is missing");

            // save arguments
            _connMethod = connMethod;
            _host = host;
            _userName = userName;
            _userPassword = userPassword;
            _authMethod = Authentication.PlainText;
            if (connMethod == ConnectMethod.Unsecure) _port = 587;
        }

        /// <summary>
        /// Secure SMTP Client constructor for OAuth2 authorization
        /// </summary>
        /// <param name="host">Email server host name</param>
        /// <param name="userName">User name (host login id)</param>
        /// <param name="oAuth2">Class implementing IOAuth2 interface</param>
        public SecureSmtpClient(string host, string userName, IOAuth2 oAuth2)
        {
            // test all required values
            if (string.IsNullOrWhiteSpace(host)) throw new ApplicationException("Host name is missing");
            if (string.IsNullOrWhiteSpace(userName)) throw new ApplicationException("User name is missing");

            // save arguments
            _host = host;
            _userName = userName;
            _oAuth2 = oAuth2 ?? throw new ApplicationException("Authorization value is missing");
            _authMethod = Authentication.OAuth2;
            _connMethod = ConnectMethod.Secure;
        }

        /// <summary>
        /// Host port number (override default value)
        /// </summary>
        public int PortNo
        {
            set
            {
                if (value <= 0 || value >= 65536) throw new ApplicationException("Invalid port number. Normally SSL=465, NonSSL=587");
                _port = value;
            }
        }

        /// <summary>
        /// Connection timeout (override default value)
        /// </summary>
        public int Timeout
        {
            set
            {
                if (value < 5 || value > 600) throw new ApplicationException("Invalid timeout (5-600 seconds)");
                _timeoutMs = 1000 * value;
            }
        }

        /// <summary>
        /// Send the email message to the mail server
        /// </summary>
        /// <param name="message">Email message</param>
        public void SendMail(SecureSmtpMessage message)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(message.Subject)) throw new ApplicationException("Email subject is missing");
                if (string.IsNullOrWhiteSpace(message.From?.Address)) throw new ApplicationException("From address is missing or in error");
                if (message.To.Count == 0) throw new ApplicationException("At least one mail to address is required");
                if (message.To.Any(ma => string.IsNullOrWhiteSpace(ma.Address)))
                {
                    throw new ApplicationException("To mail address address is missing");
                }
                if (message.Cc.Any(ma => string.IsNullOrWhiteSpace(ma.Address)))
                {
                    throw new ApplicationException("CC mail address address is missing");
                }
                if (message.Bcc.Any(ma => string.IsNullOrWhiteSpace(ma.Address)))
                {
                    throw new ApplicationException("To mail address address is missing");
                }

                // open connection
                OpenConnection();

                // send host name and expect request completed 250
                SendCommand("EHLO " + _host, SmtpReplyCode.RequestCompleted);

                // get supported authorization from last reply
                SupportedAuthorizations();

                // plain text authentication method
                if (_authMethod == Authentication.PlainText)
                {
                    if ((_supportedAuth & AuthenticationMethod.PlainText) == 0) throw new ApplicationException("Email server does not support plain text authorization");
                    SendCommand("AUTH LOGIN", SmtpReplyCode.SendUserAndPassword);
                    SendCommand(Convert.ToBase64String(Encoding.UTF8.GetBytes(_userName)), SmtpReplyCode.SendUserAndPassword);
                    SendCommand(Convert.ToBase64String(Encoding.UTF8.GetBytes(_userPassword)), SmtpReplyCode.AuthenticationSuccessful);
                }

                // OAuth2 authentication method
                else
                {
                    if ((_supportedAuth & AuthenticationMethod.XoAuth2) == 0) throw new ApplicationException("Email server does not support XOAUTH2 authorization");
                    var oAuth2Password = _oAuth2.OAuth2Password();
                    var authStr = $"user={_userName}\u0001auth=Bearer {oAuth2Password}\u0001\u0001";
                    var authBin = new byte[authStr.Length];
                    for (var x = 0; x < authStr.Length; x++) authBin[x] = (byte)authStr[x];
                    SendCommand("AUTH XOAUTH2 " + Convert.ToBase64String(authBin), SmtpReplyCode.AuthenticationSuccessful);
                }

                // from address
                SendCommand($"MAIL FROM: <{message.From.Address}>", SmtpReplyCode.RequestCompleted);

                // send the addresses of all recipients
                SendRecipientAddress(message.To);
                SendRecipientAddress(message.Cc);
                SendRecipientAddress(message.Bcc);

                // start data block
                SendCommand("DATA", SmtpReplyCode.StartInput);

                // send from
                SendDataLine($"From: {message.From.DisplayName} <{message.From.Address}>");

                // send to list
                SendAddressTo("To", message.To);
                SendAddressTo("Cc", message.Cc);

                // send subject
                SendDataLine("Subject:" + message.Subject);

                // send date and time
                SendDataLine($"Date: {DateTime.UtcNow:r}");

                // send mime vertion
                SendDataLine("MIME-Version: 1.0");

                // send all data parts
                message.RootPart.SendData(_writer);

                // data terminating dot
                SendCommand("\r\n.", SmtpReplyCode.RequestCompleted);

                // terminate connection
                SendCommand("QUIT", SmtpReplyCode.ClosingChannel);

                // dispose connection
                Dispose();
            }

            catch (Exception exp)
            {
                try
                {
                    SendCommand("QUIT", SmtpReplyCode.IgnoreReply);
                }
                catch
                {
                    // ignored
                }

                // dispose connection
                Dispose();
                throw new Exception(exp.Message, exp);
            }
        }

        // Open connection to host on port.
        private void OpenConnection()
        {
            _replyStrings = new List<string>();
            _socket = new TcpClient
            {
                SendTimeout = _timeoutMs,
                ReceiveTimeout = _timeoutMs
            };

            // connect to mail server
            _socket.Connect(_host, _port);

            if (_connMethod == ConnectMethod.Secure)
            {
                // create SSL stream
                var sslStream = new SslStream(_socket.GetStream(), true, ServerValidationCallback, ClientCertificateSelectionCallback, EncryptionPolicy.AllowNoEncryption);

                // autheticate client
                sslStream.AuthenticateAsClient(_host);

                // create read and write streams
                _writer = sslStream;// new StreamWriter(sslStream, Encoding.UTF8);
                _reader = new StreamReader(sslStream, Encoding.UTF8);
            }
            else
            {
                var netStream = _socket.GetStream();
                _writer = netStream;// new StreamWriter(netStream, Encoding.UTF8);
                _reader = new StreamReader(netStream, Encoding.UTF8);
            }

            // Code 220 means that service is up and working
            var replyCode = GetReply();
            if (replyCode != SmtpReplyCode.Ready)
            {
                throw new ApplicationException("Connect to mail server failed");
            }
        }

        ////////////////////////////////////////////////////////////////////
        // Send Recipient  Address
        ////////////////////////////////////////////////////////////////////
        private void SendRecipientAddress(List<MailAddress> addressList)
        {
            // send one address at a time
            foreach (var ma in addressList)
            {
                SendCommand($"RCPT TO: <{ma.Address}>", SmtpReplyCode.RequestCompleted);
            }
        }

        ////////////////////////////////////////////////////////////////////
        // Send Address To
        ////////////////////////////////////////////////////////////////////
        private void SendAddressTo(string type, List<MailAddress> addressList)
        {
            // nothing to do
            if (addressList.Count == 0) return;

            // build list
            var list = new StringBuilder();

            // send one address at a time
            foreach (var ma in addressList)
            {
                if (list.Length == 0)
                {
                    // send first item of the list
                    list.AppendFormat("{0}: {1} <{2}>", type, ma.DisplayName, ma.Address);
                }
                else
                {
                    // append all others
                    list.AppendFormat(",\r\n {0} <{1}>", ma.DisplayName, ma.Address);
                }
            }

            // send the list
            SendDataLine(list.ToString());
        }

        ////////////////////////////////////////////////////////////////////
        // read supported authorization available from reply
        ////////////////////////////////////////////////////////////////////
        internal void SupportedAuthorizations()
        {
            _supportedAuth = AuthenticationMethod.None;
            foreach (var replyString in _replyStrings)
            {
                // reply string starts with AUTH
                if (string.Compare(replyString, 4, "AUTH ", 0, 5, StringComparison.OrdinalIgnoreCase) != 0) continue;

                // remove the AUTH text including the following character 
                // to ensure that split only gets the modules supported
                var authTypes = replyString.Substring(9).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var authType in authTypes)
                {
                    if (string.Compare(authType, "PLAIN", StringComparison.OrdinalIgnoreCase) == 0) _supportedAuth |= AuthenticationMethod.PlainText;
                    else if (string.Compare(authType, "XOAUTH2", StringComparison.OrdinalIgnoreCase) == 0) _supportedAuth |= AuthenticationMethod.XoAuth2;
                }
            }
        }

        private bool ServerValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors policyErrors)
        {
            // save for debugging
            Certificate = certificate.ToString();
            PolicyErrors = policyErrors.ToString();

            // certificate is accepted
            return true;
        }

        private X509Certificate ClientCertificateSelectionCallback(object sender, string targethost, X509CertificateCollection localcertificates, X509Certificate remotecertificate, string[] acceptableissuers)
        {
            // ignore
            return null;
        }

        ////////////////////////////////////////////////////////////////////
        // Send command to the server.
        ////////////////////////////////////////////////////////////////////
        private void SendCommand(string command, SmtpReplyCode expectedReply)
        {
            // send command
            var bytes = Encoding.UTF8.GetBytes(command + "\r\n");
            _writer.Write(bytes, 0, bytes.Length);
            _writer.Flush();

            // test reply
            if (expectedReply == SmtpReplyCode.IgnoreReply) return;
            var replyCode = GetReply();
            if (replyCode == expectedReply) return;

            throw new ApplicationException("Command error: " + command);
        }

        ////////////////////////////////////////////////////////////////////
        // Send a line of text to the server.
        ////////////////////////////////////////////////////////////////////
        private void SendDataLine(string text)
        {
            // send command
            var bytes = Encoding.UTF8.GetBytes(text + "\r\n");
            _writer.Write(bytes, 0, bytes.Length);
        }

        ////////////////////////////////////////////////////////////////////
        // Send end of line to the server.
        ////////////////////////////////////////////////////////////////////
        private void SendEndOfLine()
        {
            // note: WriteLine is not used to make sure eol is CRLF
            var bytes = Encoding.UTF8.GetBytes("\r\n");
            _writer.Write(bytes, 0, bytes.Length);
            // flush
            _writer.Flush();
        }

        ////////////////////////////////////////////////////////////////////
        // Get the reply message from the server.
        ////////////////////////////////////////////////////////////////////
        private SmtpReplyCode GetReply()
        {
            // reset reply
            _replyStrings.Clear();

            // get reply
            var str = _reader.ReadLine();

            // end of file
            if (str == null) throw new ApplicationException("Response from server: timeout");

            // save reply
            _replyStrings.Add(str);

            // reply must be at least 4 char long
            if (str.Length < 4) throw new ApplicationException("Response from server is too short");

            // read more lines if required
            while (str[3] == '-')
            {
                str = _reader.ReadLine();
                if (str == null) break;
                _replyStrings.Add(str);
            }

            if (!int.TryParse(_replyStrings[0].Substring(0, 3), out var code) || code <= 0) code = -1;
            return (SmtpReplyCode)code;
        }

        /// <inheritdoc />
        /// <summary>
        /// Close SecureSmtpClient object
        /// </summary>
        public void Dispose()
        {
            if (_reader != null)
            {
                _reader.Close();
                _reader = null;
            }
            if (_writer != null)
            {
                _writer.Flush();
                _writer.Close();
                _writer = null;
            }
            if (_socket == null) return;

            _socket.Close();
            _socket = null;
        }
    }
}
