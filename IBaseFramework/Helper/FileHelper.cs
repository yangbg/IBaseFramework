﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using IBaseFramework.Logger.Logging;

namespace IBaseFramework.Helper
{
    /// <summary>
    /// 文件辅助类
    /// </summary>
    public class FileHelper
    {
        private static Mutex _mut;

        /// <summary>
        /// 获取新的文件名
        /// </summary>
        /// <param name="ext">扩展名</param>
        /// <returns></returns>
        public static string GetNewName(string ext)
        {
            var i = Guid.NewGuid().ToByteArray().Aggregate<byte, long>(1, (current, b) => current * ((int)b + 1));
            var guid16 = $"{i - DateTimeOffset.Now.Ticks:x}";
            return guid16 + ext;
        }

        /// <summary>
        /// 检查路径文件是否存在，若不存在则创建
        /// </summary>
        /// <param name="path"></param>
        /// <param name="create"></param>
        public static bool CheckDirectory(string path, bool create)
        {
            var dir = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(dir))
                return false;
            if (Directory.Exists(dir)) return true;
            if (create)
                Directory.CreateDirectory(dir);
            return false;
        }

        /// <summary>
        /// 拷贝文件夹
        /// </summary>
        /// <param name="oldPath">旧文件夹路径</param>
        /// <param name="newPath">新文件夹</param>
        public static bool CopyDirectory(string oldPath, string newPath)
        {
            try
            {
                if (!Directory.Exists(oldPath))
                    return false;

                if (newPath[newPath.Length - 1] != Path.DirectorySeparatorChar)
                    newPath += Path.DirectorySeparatorChar;

                if (!Directory.Exists(newPath))
                    Directory.CreateDirectory(newPath);

                var files = Directory.GetFileSystemEntries(oldPath);

                foreach (var file in files)
                {
                    var thePath = newPath + Path.GetFileName(file);
                    if (Directory.Exists(file))
                    {
                        //递归Copy该目录下面的文件
                        CopyDirectory(file, thePath);
                    }
                    else // 否则直接copy文件
                    {
                        File.Copy(file, thePath, true);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                var logger = LoggerManager.Logger<FileHelper>();
                logger.Error(ex.Message, ex);

                return false;
            }
        }

        /// <summary>
        /// 移动文件
        /// </summary>
        /// <param name="oldPath"></param>
        /// <param name="newPath"></param>
        /// <param name="delOld"></param>
        /// <returns></returns>
        public static bool MoveFile(string oldPath, string newPath, bool delOld)
        {
            if (!File.Exists(oldPath))
                return false;
            StreamReader sr = null;
            StreamWriter sw = null;
            try
            {
                sr = new StreamReader(oldPath);
                sw = new StreamWriter(newPath, false);
                sw.Write(sr.ReadToEnd());
            }
            catch (Exception ex)
            {
                var logger = LoggerManager.Logger<FileHelper>();
                logger.Error(ex.Message, ex);

                return false;
            }
            finally
            {
                sr?.Close();
                sw?.Close();
            }
            if (delOld)
                File.Delete(oldPath);
            return true;
        }

        /// <summary>
        /// 合并文件，合并到文件1
        /// </summary>
        /// <param name="path1">文件1</param>
        /// <param name="path2">文件2</param>
        public static void ComboFile(string path1, string path2)
        {
            StreamReader sr = null;
            StreamWriter sw = null;
            try
            {
                sr = new StreamReader(path2, Encoding.Default);
                sw = new StreamWriter(path1, true, Encoding.Default);
                var str = sr.ReadLine();
                while (!string.IsNullOrEmpty(str))
                {
                    sw.WriteLine(str);
                    str = sr.ReadLine();
                }
            }
            catch (Exception ex)
            {
                var logger = LoggerManager.Logger<FileHelper>();
                logger.Error(ex.Message, ex);
            }
            finally
            {
                sr?.Close();
                sw?.Close();
            }
        }

        /// <summary>
        /// 写文件
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="str">字符</param>
        /// <param name="append">是否追加</param>
        /// <param name="code">编码</param>
        public static void WriteFile(string path, IEnumerable<string> str, bool append, Encoding code)
        {
            if (_mut == null)
                _mut = new Mutex();

            _mut.WaitOne();

            StreamWriter sw = null;
            try
            {
                sw = new StreamWriter(path, append, code);
                foreach (var s in str)
                {
                    sw.WriteLine(s);
                    sw.Flush();
                }
            }
            catch (Exception ex)
            {
                var logger = LoggerManager.Logger<FileHelper>();
                logger.Error(ex.Message, ex);
            }
            finally
            {
                sw?.Close();
                _mut.ReleaseMutex();
            }
        }

        public static void WriteFile(string path, IEnumerable<string> str, Encoding code)
        {
            WriteFile(path, str, true, code);
        }

        public static void WriteFile(string path, IEnumerable<string> str, bool append)
        {
            WriteFile(path, str, append, Encoding.Default);
        }

        public static void WriteFile(string path, IEnumerable<string> str)
        {
            WriteFile(path, str, true, Encoding.Default);
        }

        /// <summary>
        /// 写文件
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="str">写入值</param>
        /// <param name="append">是否追加</param>
        /// <param name="code">编码</param>
        public static void WriteFile(string path, string str, bool append, Encoding code)
        {
            var list = new[] { str };
            WriteFile(path, list, append, code);
        }

        /// <summary>
        /// 写文件(简化)
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="str">写入值</param>
        /// <param name="append">是否追加</param>
        public static void WriteFile(string path, string str, bool append)
        {
            WriteFile(path, str, append, Encoding.Default);
        }

        /// <summary>
        /// 写文件(简化),默认追加
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="str">写入值</param>
        /// <param name="code">编码</param>
        public static void WriteFile(string path, string str, Encoding code)
        {
            WriteFile(path, str, true, code);
        }

        /// <summary>
        /// 写文件(简化),默认追加
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="str">写入值</param>
        public static void WriteFile(string path, string str)
        {
            WriteFile(path, str, true, Encoding.Default);
        }
    }
}
