using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace IBaseFramework.Utility.Helper.SendEmail
{
    /// <inheritdoc />
    ///  <summary>
    ///  Expose OAuth2Password member
    ///  </summary>
    ///  <remarks>
    /// 	The SecureSmtpOAuth2 class calculates the host login password
    /// 	from the GMail refresh token file.
    ///  </remarks>
    public class SecureSmtpOAuth2 : IOAuth2
    {
        private readonly SecureSmtpRefreshToken _refreshToken;
        private string _accessToken;
        private DateTime _accessTokenExpireTime;

        // get new token from refreash authorization code
        private const string RefreshTokenUrl = "https://www.googleapis.com/oauth2/v4/token";

        // get token from authorization code URI
        private const string TokenUri = "https://accounts.google.com/o/oauth2/token";

        // authorization scope send email using gmail
        private const string Scope = "https://mail.google.com/";

        /// <summary>
        /// Secure SMTP OAuth2 constructor
        /// </summary>
        /// <param name="refreshTokenFileName">Refresh token file name</param>
        public SecureSmtpOAuth2(string refreshTokenFileName)
        {
            // no file name 
            if (string.IsNullOrWhiteSpace(refreshTokenFileName))
            {
                throw new ApplicationException("Refresh token file name is missing or empty");
            }

            // look for refresh token
            _refreshToken = SecureSmtpRefreshToken.LoadState(refreshTokenFileName);
            if (_refreshToken == null)
            {
                throw new ApplicationException("SecureSmtpRefreshToken.xml file is missing or invalid");
            }
            return;
        }

        /// <inheritdoc />
        /// <summary>
        /// get OAuth2 password
        /// </summary>
        /// <returns>OAuth2 password</returns>
        public string OAuth2Password()
        {
            // not the first time
            if (_accessToken != null)
            {
                // token did not expire
                if (DateTime.UtcNow < _accessTokenExpireTime) return _accessToken;
            }

            var tokenRequestStr = string.Empty;
            var responseText = string.Empty;
            try
            {
                // Creates a redirect URI using an available port on the loopback address.
                var redirectUri = $"http://{IPAddress.Loopback}:{GetRandomUnusedPort()}/";
                var escRedirectUri = Uri.EscapeDataString(redirectUri);

                // builds the request string
                tokenRequestStr = $"redirect_uri={escRedirectUri}" + $"&client_id={_refreshToken.ClientId}" +
                                  $"&client_secret={_refreshToken.ClientSecret}" + $"&refresh_token={_refreshToken.RefreshToken}" +
                                  "&grant_type=refresh_token";

                // convert string to byte array
                var tokenRequestBin = Encoding.UTF8.GetBytes(tokenRequestStr);

                // sends the request
                var tokenRequest = (HttpWebRequest)WebRequest.Create(RefreshTokenUrl);
                tokenRequest.Method = "POST";
                tokenRequest.ContentType = "application/x-www-form-urlencoded";
                tokenRequest.Accept = "Accept=text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
                tokenRequest.ContentLength = tokenRequestBin.Length;
                var stream = tokenRequest.GetRequestStream();
                stream.Write(tokenRequestBin, 0, tokenRequestBin.Length);
                stream.Close();

                // gets the response
                var tokenResponse = tokenRequest.GetResponse();
                using (var reader = new StreamReader(tokenResponse.GetResponseStream() ?? throw new InvalidOperationException()))
                {
                    // reads response body
                    responseText = reader.ReadToEnd();

                    // get access token
                    _accessToken = GetResponseValue(responseText, "access_token");

                    // get expires in value (3600)
                    var expiresInStr = GetResponseValue(responseText, "expires_in");
                    var expiersIn = int.Parse(expiresInStr);

                    // set expire time
                    _accessTokenExpireTime = DateTime.UtcNow.AddSeconds(expiersIn - 300);
                }

                // return password
                return _accessToken;
            }

            catch (Exception exp)
            {
                throw new ApplicationException("Get password exception:\r\nToken request:\r\n" + tokenRequestStr + "\r\nResponse:\r\n" + responseText + "Exception message:\r\n" + exp.Message);
            }
        }

        ////////////////////////////////////////////////////////////////////
        // Get available port number
        ////////////////////////////////////////////////////////////////////
        private static int GetRandomUnusedPort()
        {
            // port number is set to zero
            // the system will set available port number between 1024 and 5000
            // See TcpListener Constructor(IPAddress, Int32) remarks section
            // ref https://msdn.microsoft.com/en-us/library/c6z86e63(v=vs.110).aspx
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        ////////////////////////////////////////////////////////////////////
        // get response value.
        ////////////////////////////////////////////////////////////////////
        private static string GetResponseValue(string response, string key)
        {
            // look for key
            var ptr = response.IndexOf(key, StringComparison.Ordinal);
            if (ptr < 0) throw new ApplicationException("Response value not found (1): " + key);

            // skip to colon
            ptr = response.IndexOf(':', ptr);
            if (ptr < 0) throw new ApplicationException("Response value not found (2): " + key);

            // skip space
            for (ptr++; ptr < response.Length && response[ptr] == ' '; ptr++) ;
            if (ptr == response.Length) throw new ApplicationException("Response value not found (3): " + key);

            // start and end of value
            int start;
            int end;

            // value is quoted text
            if (response[ptr] == '"')
            {
                // find end of value
                start = ptr + 1;
                end = response.IndexOf('"', start);
                if (end < 0 || end <= start) throw new ApplicationException("Response value not found (4): " + key);
            }

            // value is a number
            else
            {
                start = ptr;
                for (end = ptr; end < response.Length && char.IsDigit(response[end]); end++) ;
            }

            // make sure value is not empty
            if (end < 0 || end <= start) throw new ApplicationException("Response value not found (5): " + key);

            // exit
            return response.Substring(start, end - start);
        }
    }
}
