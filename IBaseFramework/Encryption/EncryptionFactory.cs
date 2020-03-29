using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace IBaseFramework.Encryption
{
    public class EncryptionFactory
    {
        #region 不可逆加密

        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="strText">被加密字符串</param>
        /// <returns>加密后字符串</returns>
        public static string Md5Encrypt(string strText)
        {
            var mD = MD5.Create();
            var array = mD.ComputeHash(Encoding.UTF8.GetBytes(strText));
            var stringBuilder = new StringBuilder();
            var array2 = array;
            for (var i = 0; i < array2.Length; i++)
            {
                var b = array2[i];
                var text = b.ToString("x");
                if (text.Length == 1)
                {
                    stringBuilder.Append("0");
                }
                stringBuilder.Append(text);
            }
            return stringBuilder.ToString();
        }

        #endregion

        #region 非对称加密

        /// <summary>
        /// RSA字符串加密
        /// </summary>
        /// <param name="source">源字符串 明文</param>
        /// <param name="key">密匙</param>
        /// <returns>加密遇到错误将会返回原字符串</returns>
        public static string RsaEncrypt(string source, string keyFilePath)
        {
            var en = Encoding.UTF8.GetBytes(source);

            var param = PemConverter.GetPemPublicKey(keyFilePath);
            var rsasp = new RSACryptoServiceProvider();
            rsasp.ImportParameters(param);
            var encryptedData = rsasp.Encrypt(en, false);
            return Convert.ToBase64String(encryptedData);
        }

        /// <summary>
        /// RSA字符串解密
        /// </summary>
        /// <param name="encryptString">密文</param>
        /// <param name="key">密钥</param>
        /// <returns>遇到解密失败将会返回原字符串</returns>
        public static string RsaDecrypt(string encryptString, string keyFilePath)
        {
            var de = Convert.FromBase64String(encryptString);

            var param = PemConverter.GetPemPrivateKey(keyFilePath);
            var rsa = new RSACryptoServiceProvider();
            rsa.ImportParameters(param);

            var decryptedData = rsa.Decrypt(de, false);

            return Encoding.UTF8.GetString(decryptedData);
        }

        #endregion

        #region 对称加密

        /// <summary>
        /// DES加密字符串
        /// </summary>
        /// <param name="encryptString">待加密的字符串</param>
        /// <param name="encryptKey">加密密钥,要求为8位</param>
        /// <param name="key">密钥</param>
        /// <returns>加密成功返回加密后的字符串，失败返回源串</returns>
        public static string DesEncrypt(string encryptString, string encryptKey, string key)
        {
            try
            {
                var rgbKey = Encoding.UTF8.GetBytes(encryptKey.Substring(0, 8));
                var rgbIv = Encoding.Unicode.GetBytes(key);
                var inputByteArray = Encoding.UTF8.GetBytes(encryptString);
                var dCsp = new DESCryptoServiceProvider();
                var mStream = new MemoryStream();
                var cStream = new CryptoStream(mStream, dCsp.CreateEncryptor(rgbKey, rgbIv), CryptoStreamMode.Write);
                cStream.Write(inputByteArray, 0, inputByteArray.Length);
                cStream.FlushFinalBlock();
                var keyString = Convert.ToBase64String(mStream.ToArray());
                return HttpUtility.UrlEncode(keyString);
            }
            catch
            {
                return encryptString;
            }
        }

        /// <summary>
        /// DES解密字符串
        /// </summary>
        /// <param name="decryptString">待解密的字符串</param>
        /// <param name="decryptKey">解密密钥,要求为8位,和加密密钥相同</param>
        /// <param name="key">密钥</param>
        /// <returns>解密成功返回解密后的字符串，失败返源串</returns>
        public static string DesDecrypt(string decryptString, string decryptKey, string key)
        {
            try
            {
                decryptString = HttpUtility.UrlDecode(decryptString);
                var rgbKey = Encoding.UTF8.GetBytes(decryptKey.Substring(0, 8));
                var rgbIv = Encoding.Unicode.GetBytes(key);
                var inputByteArray = Convert.FromBase64String(decryptString);
                var dcsp = new DESCryptoServiceProvider();
                var mStream = new MemoryStream();
                var cStream = new CryptoStream(mStream, dcsp.CreateDecryptor(rgbKey, rgbIv), CryptoStreamMode.Write);
                cStream.Write(inputByteArray, 0, inputByteArray.Length);
                cStream.FlushFinalBlock();
                return Encoding.UTF8.GetString(mStream.ToArray());
            }
            catch
            {
                return decryptString;
            }
        }

        #endregion

        /// <summary>
        /// 编码密码，目前Sha1
        /// </summary>
        /// <param name="password">明文密码</param>
        /// <param name="key">solt</param>
        /// <param name="appVer">应用版本，可能会Sha2加密</param>
        /// <returns></returns>
        public static string ShaEncode(string password, string key, int appVer = 0)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(key))
                return string.Empty;
            var hmacsha1 = new HMACSHA1(Encoding.UTF8.GetBytes(key));
            var encodePw = hmacsha1.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(encodePw);
        }
    }
}
