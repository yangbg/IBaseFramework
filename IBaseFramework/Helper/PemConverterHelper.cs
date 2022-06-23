using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace IBaseFramework.Helper
{
    public class PemConverterHelper
    {
        /// <summary>
        /// 将pem格式公钥(1024 or 2048)转换为RSAParameters
        /// </summary>
        /// <param name="pemFileContent">pem公钥内容</param>
        /// <returns>转换得到的RSAParameters</returns>
        private static RSAParameters ConvertFromPemPublicKey(string pemFileContent)
        {
            if (string.IsNullOrEmpty(pemFileContent))
            {
                throw new ArgumentNullException(nameof(pemFileContent), "pem文件不存在");
            }
            pemFileContent = pemFileContent.Replace("-----BEGIN PUBLIC KEY-----", "").Replace("-----END PUBLIC KEY-----", "").Replace("\n", "").Replace("\r", "");
            var keyData = Convert.FromBase64String(pemFileContent);
            var keySize1024 = (keyData.Length == 162);
            var keySize2048 = (keyData.Length == 294);
            if (!(keySize1024 || keySize2048))
            {
                throw new ArgumentException("pem文件格式不正确, 只支持1024位或2048位密钥");
            }
            var pemModulus = (keySize1024 ? new byte[128] : new byte[256]);
            var pemPublicExponent = new byte[3];
            Array.Copy(keyData, (keySize1024 ? 29 : 33), pemModulus, 0, (keySize1024 ? 128 : 256));
            Array.Copy(keyData, (keySize1024 ? 159 : 291), pemPublicExponent, 0, 3);
            var para = new RSAParameters {Modulus = pemModulus, Exponent = pemPublicExponent};
            return para;
        }

        /// <summary>
        /// 将pem格式私钥(1024 or 2048)转换为RSAParameters
        /// </summary>
        /// <param name="pemFileContent">pem私钥内容</param>
        /// <returns>转换得到的RSAParameters</returns>
        private static RSAParameters ConvertFromPemPrivateKey(string pemFileContent)
        {
            if (string.IsNullOrEmpty(pemFileContent))
            {
                throw new ArgumentNullException(nameof(pemFileContent), "pem文件不存在");
            }
            pemFileContent = pemFileContent.Replace("-----BEGIN RSA PRIVATE KEY-----", "").Replace("-----END RSA PRIVATE KEY-----", "").Replace("\n", "").Replace("\r", "");
            var keyData = Convert.FromBase64String(pemFileContent);

            var keySize1024 = (keyData.Length == 609 || keyData.Length == 610);

            var index = (keySize1024 ? 11 : 12);
            var pemModulus = (keySize1024 ? new byte[128] : new byte[256]);
            Array.Copy(keyData, index, pemModulus, 0, pemModulus.Length);

            index += pemModulus.Length;
            index += 2;
            var pemPublicExponent = new byte[3];
            Array.Copy(keyData, index, pemPublicExponent, 0, 3);

            index += 3;
            index += 4;
            if (keyData[index] == 0)
            {
                index++;
            }
            var pemPrivateExponent = (keySize1024 ? new byte[128] : new byte[256]);
            Array.Copy(keyData, index, pemPrivateExponent, 0, pemPrivateExponent.Length);

            index += pemPrivateExponent.Length;
            index += (keySize1024 ? ((int)keyData[index + 1] == 64 ? 2 : 3) : ((int)keyData[index + 2] == 128 ? 3 : 4));
            var pemPrime1 = (keySize1024 ? new byte[64] : new byte[128]);
            Array.Copy(keyData, index, pemPrime1, 0, pemPrime1.Length);

            index += pemPrime1.Length;
            index += (keySize1024 ? ((int)keyData[index + 1] == 64 ? 2 : 3) : ((int)keyData[index + 2] == 128 ? 3 : 4));
            var pemPrime2 = (keySize1024 ? new byte[64] : new byte[128]);
            Array.Copy(keyData, index, pemPrime2, 0, pemPrime2.Length);

            index += pemPrime2.Length;
            index += (keySize1024 ? ((int)keyData[index + 1] == 64 ? 2 : 3) : ((int)keyData[index + 2] == 128 ? 3 : 4));
            var pemExponent1 = (keySize1024 ? new byte[64] : new byte[128]);
            Array.Copy(keyData, index, pemExponent1, 0, pemExponent1.Length);

            index += pemExponent1.Length;
            index += (keySize1024 ? ((int)keyData[index + 1] == 64 ? 2 : 3) : ((int)keyData[index + 2] == 128 ? 3 : 4));
            var pemExponent2 = (keySize1024 ? new byte[64] : new byte[128]);
            Array.Copy(keyData, index, pemExponent2, 0, pemExponent2.Length);

            index += pemExponent2.Length;
            index += (keySize1024 ? ((int)keyData[index + 1] == 64 ? 2 : 3) : ((int)keyData[index + 2] == 128 ? 3 : 4));
            var pemCoefficient = (keySize1024 ? new byte[64] : new byte[128]);
            Array.Copy(keyData, index, pemCoefficient, 0, pemCoefficient.Length);

            var para = new RSAParameters
            {
                Modulus = pemModulus,
                Exponent = pemPublicExponent,
                D = pemPrivateExponent,
                P = pemPrime1,
                Q = pemPrime2,
                DP = pemExponent1,
                DQ = pemExponent2,
                InverseQ = pemCoefficient
            };
            return para;
        }

        /// <summary>
        /// 获取pem文件内容
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>文件内容</returns>
        private static string GetPemContent(string filePath)
        {
            var content = File.ReadAllText(filePath, Encoding.ASCII);
            return content;
        }

        /// <summary>
        /// 获取pem文件并转换成RSAParameter
        /// </summary>
        /// <param name="filePath">文件地址</param>
        /// <returns>RSAParameters</returns>
        public static RSAParameters GetPemPublicKey(string filePath)
        {
            var publicKey = GetPemContent(filePath);
            return ConvertFromPemPublicKey(publicKey);
        }

        /// <summary>
        /// 获取pem文件并转换成RSAParameter
        /// </summary>
        /// <param name="filePath">文件地址</param>
        /// <returns>RSAParameters</returns>
        public static RSAParameters GetPemPrivateKey(string filePath)
        {
            var privateKey = GetPemContent(filePath);
            return ConvertFromPemPrivateKey(privateKey);
        }
    }
}
