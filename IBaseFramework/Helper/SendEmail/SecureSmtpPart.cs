using System;
using System.IO;
using System.Net.Mime;
using System.Text;

namespace IBaseFramework.Helper.SendEmail
{
    /// <summary>
    /// Secure SMTP Part base class for message parts
    /// </summary>
    public class SecureSmtpPart
    {
        /// <summary>
        /// Stream writer
        /// </summary>
        protected Stream Writer;

        /// <summary>
        /// Hex conversion
        /// </summary>
        protected static char[] Hex = new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

        /// <summary>
        /// Default constructor
        /// </summary>
        protected SecureSmtpPart() { }

        ////////////////////////////////////////////////////////////////////
        // Send data
        ////////////////////////////////////////////////////////////////////
        internal virtual void SendData(Stream writer)
        {
            throw new ApplicationException("Unimplemented SendData");
        }

        /// <summary>
        /// Send data line
        /// </summary>
        /// <param name="text">Text to send</param>
        protected void SendDataLine(string text)
        {
            var bytes = Encoding.UTF8.GetBytes(text + "\r\n");
            Writer.Write(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Send end of line
        /// </summary>
        protected void SendEndOfLine()
        {
            var bytes = Encoding.UTF8.GetBytes("\r\n");
            Writer.Write(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Send data block
        /// </summary>
        /// <param name="text">Text block</param>
        protected void SendDataBlock(string text)
        {
            var bytes = Encoding.UTF8.GetBytes(text + "\r\n");
            Writer.Write(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Transfer encoding to text
        /// </summary>
        /// <param name="transEncoding">Transfer encoding code</param>
        /// <returns>Transfer encoding text</returns>
        protected string TransferEncodingText(TransferEncoding transEncoding)
        {
            switch (transEncoding)
            {
                case TransferEncoding.QuotedPrintable: return "quoted-printable";
                case TransferEncoding.Base64: return "base64";
                case TransferEncoding.SevenBit: return "7bit";
                case TransferEncoding.EightBit: return "8bit";
            }

            throw new ApplicationException("Unknown transfer encoding.");
        }

        /// <summary>
        /// Quoted Printable Encoding
        /// </summary>
        /// <param name="input">Byte array input</param>
        /// <returns>String output</returns>
        protected static string QuotedPrintableEncode(byte[] input)
        {
            // input is empty
            if (input == null || input.Length == 0) return null;

            // create output string
            var str = new StringBuilder();

            // maximum line length
            const int maxLineLen = 76;

            // convert string to quoted printable
            var inputLen = input.Length;
            var start = 0;
            for (var index = 0; index < inputLen; index++)
            {
                // get next character from input line
                int chr = input[index];

                // printable ascii character except = and space or tab
                if (chr >= ' ' && chr < '=' || chr > '=' && chr <= '~' || chr == '\t')
                {
                    // line length is less than max allowed 76
                    if (str.Length - start < maxLineLen - 1)
                    {
                        // append character
                        str.Append((char)chr);
                        continue;
                    }

                    // line length is at max-1 (75) and there is no end of line at the input string
                    // add soft end of line and append character
                    if (inputLen - index < 3 || input[index + 1] != '\r' || input[index + 2] != '\n')
                    {
                        // add soft end of line
                        str.Append("=\r\n");
                        start = str.Length;
                        str.Append((char)chr);
                        continue;
                    }

                    // line length is at max-1 (75) and there is hard end of line at the input string
                    // last character is not space or tab
                    if (chr != ' ' && chr != '\t')
                    {
                        // append character and hard end of line
                        str.Append((char)chr);
                        str.Append("\r\n");
                    }

                    // append soft end of line followed by space or tab and hard end of line
                    else
                    {
                        str.Append(chr == ' ' ? "=\r\n=20\r\n" : "=\r\n=09\r\n");
                    }
                    // skip the hard end of line in input stream
                    index += 2;
                    start = str.Length;
                    continue;
                }

                // test for end of line
                if (chr == '\r' && inputLen - index > 1 && input[index + 1] == '\n')
                {
                    // get last character in output string
                    var lastChar = str.Length > 0 ? str[str.Length - 1] : 0;

                    // last character is not space or tab
                    if (lastChar != ' ' && lastChar != '\t')
                    {
                        str.Append("\r\n");
                    }

                    // last character is space or tab and there is enough space for =20 or =09
                    else if (str.Length - start <= maxLineLen - 2)
                    {
                        // remove last space or tab and append soft end of line followed by space and hard end of line
                        str.Length--;
                        str.Append(lastChar == ' ' ? "=20\r\n" : "=09\r\n");
                    }

                    // not enough room for escaped space or tab
                    else
                    {
                        // remove last space or tab and append soft end of line followed by space and hard end of line
                        str.Length--;
                        str.Append(lastChar == ' ' ? "=\r\n=20\r\n" : "=\r\n=09\r\n");
                    }
                    index++;
                    start = str.Length;
                    continue;
                }

                // all other 8 bit characters 
                // not enough room for three characters plus a spare one
                if (str.Length - start > maxLineLen - 4)
                {
                    // add soft return
                    str.Append("=\r\n");
                    start = str.Length;
                }

                // convert to hex formal =XX
                str.Append('=');
                str.Append(Hex[chr >> 4]);
                str.Append(Hex[chr & 0x0F]);
            }

            // if last char is not end of line, add soft end of line
            if (str[str.Length - 1] != '\n')
            {
                // add soft return
                str.Append("=\r\n");
            }

            // exit
            return str.ToString();
        }
    }
}
