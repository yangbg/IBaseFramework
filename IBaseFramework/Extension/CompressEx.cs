using System.IO;
using System.IO.Compression;
using System.Text;

namespace IBaseFramework.Extension
{
    public static class CompressEx
    {
        /// <summary>
        /// 
        /// 30：使用操作系统当前的ANSI编码格式压缩字符串。返回压缩后的字节数组。
        ///   如果字符串为 Null、空、空白，则返回 null
        /// </summary>
        /// <param name="this">扩展对象</param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static byte[] CompressByGZip(this string @this, Encoding encoding = null)
        {
            if (string.IsNullOrWhiteSpace(@this))
                return null;

            var byteArr = encoding == null ? Encoding.Default.GetBytes(@this) : encoding.GetBytes(@this);
            using (var memoryStream = new MemoryStream())
            {
                using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress))
                {
                    gZipStream.Write(byteArr, 0, byteArr.Length);
                    gZipStream.Close();

                    return memoryStream.ToArray();
                }
            }
        }

        /// <summary>
        ///   自定义扩展方法：使用操作系统当前的ANSI编码格式解压缩。返回解压缩后的字符串。
        ///   如果字节数组为 Null 或者 长度为0，则返回空字符串
        /// </summary>
        /// <param name="this">扩展对象。8位无符号整数数组</param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string DecompressByGZip(this byte[] @this, Encoding encoding = null)
        {
            if (@this == null || @this.Length == 0)
                return string.Empty;

            const int bufferSize = 1024;
            using (var memoryStream = new MemoryStream(@this))
            {
                using (var zipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                {
                    // 内存流 用于接收解压的字节
                    using (var outStream = new MemoryStream())
                    {
                        var buffer = new byte[bufferSize];
                        var totalBytes = 0;
                        int readBytes;
                        while ((readBytes = zipStream.Read(buffer, 0, bufferSize)) > 0)
                        {
                            outStream.Write(buffer, 0, readBytes);
                            totalBytes += readBytes;
                        }
                        return encoding == null ? Encoding.Default.GetString(outStream.GetBuffer(), 0, totalBytes) : encoding.GetString(outStream.GetBuffer(), 0, totalBytes);
                    }
                }
            }
        }
    }
}