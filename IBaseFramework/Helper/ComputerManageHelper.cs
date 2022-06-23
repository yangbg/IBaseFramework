using System;
using System.Management;

namespace IBaseFramework.Helper
{
    public class ComputerManageHelper
    {
        #region 属性

        public static readonly string UnKnowInfo = "未知";

        /// <summary>
        ///  获取系统标识符和版本号
        /// </summary>
        public static string OsVersion => Environment.OSVersion.ToString();

        /// <summary>
        ///  获取映射到进程上下文的物理内存量
        /// </summary>
        public static string WorkingSet => Environment.WorkingSet.ToString();

        /// <summary>
        ///  获取系统启动后经过的毫秒数
        /// </summary>
        public static int TickCount => Environment.TickCount;

        /// <summary>
        ///  获取系统目录的完全限定路径
        /// </summary>
        public static string SystemDirectory => Environment.SystemDirectory;

        /// <summary>
        ///  获取此本地计算机的 NetBIOS 名称
        /// </summary>
        public static string MachineName => Environment.MachineName;

        /// <summary>
        ///  获取与当前用户关联的网络域名
        /// </summary>
        public static string UserDomainName => Environment.UserDomainName;

        /// <summary>
        ///  获取电脑名称
        /// </summary>
        /// <returns></returns>
        public static string GetComputerName()
        {
            try
            {
                return Environment.GetEnvironmentVariable("ComputerName");
            }
            catch
            {
                return UnKnowInfo;
            }
        }

        #endregion

        #region 方法

        /// <summary>
        ///  获取 Cpu 序列号。
        ///  通过 Win32_Processor 获取 CPUID 不正确，或者说 Win32_Processor 字段就不包含 CPU 编号信息
        /// </summary>
        /// <returns></returns>
        public static string GetCpuSerialNumber()
        {
            try
            {
                var cpuInfo = ""; //cpu序列号

                var mc = new ManagementClass("Win32_Processor");
                var moc = mc.GetInstances();
                foreach (var o in moc)
                {
                    var mo = (ManagementObject) o;
                    cpuInfo = mo.Properties["ProcessorId"].Value.ToString();
                }

                return cpuInfo;
            }
            catch
            {
                return UnKnowInfo;
            }
        }

        /// <summary>
        ///  获取主板序列号。
        ///  通过Win32_BaseBoard获取主板信息，但不是所有的主板都有编号，或者说不是能获取所有系统主板的编号。
        /// </summary>
        /// <returns></returns>
        public static string GetBaseBoardSerialNumber()
        {
            try
            {
                var motherBoardInfo = ""; //主板序列号
                var mc = new ManagementClass("Win32_BaseBoard");
                var moc = mc.GetInstances();
                foreach (var o in moc)
                {
                    var mo = (ManagementObject) o;
                    motherBoardInfo = mo.Properties["SerialNumber"].Value.ToString();
                }

                return motherBoardInfo;
            }
            catch
            {
                return UnKnowInfo;
            }
        }

        /// <summary>
        ///  获取 BIOS 序列号。
        ///  通过 Win32_BIOS 获取 BIOS 信息，基本和获取主板信息差不多。就是说：不是所有的主板 BIOS 信息都有编号
        /// </summary>
        /// <returns></returns>
        public static string GetBiosSerialNumber()
        {
            try
            {
                var motherBoardInfo = ""; //BIOS序列号
                var mc = new ManagementClass("Win32_BIOS");
                var moc = mc.GetInstances();
                foreach (var o in moc)
                {
                    var mo = (ManagementObject) o;
                    motherBoardInfo = mo.Properties["SerialNumber"].Value.ToString();
                }

                return motherBoardInfo;
            }
            catch
            {
                return UnKnowInfo;
            }
        }

        /// <summary>
        ///  获取网卡硬件地址
        /// </summary>
        /// <returns></returns>
        public static string GetMacAddress()
        {
            try
            {
                //获取网卡硬件地址
                var mac = "";
                var mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
                var moc = mc.GetInstances();
                foreach (var o in moc)
                {
                    var mo = (ManagementObject) o;
                    if ((bool) mo["IPEnabled"] != true) continue;

                    mac = mo["MacAddress"].ToString();
                    break;
                }

                return mac;
            }
            catch
            {
                return UnKnowInfo;
            }
        }

        /// <summary>
        ///  获取IP地址
        /// </summary>
        /// <returns></returns>
        public static string GetIpAddress()
        {
            try
            {
                //获取IP地址
                var st = "";
                var mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
                var moc = mc.GetInstances();
                foreach (var o in moc)
                {
                    var mo = (ManagementObject) o;
                    if ((bool)mo["IPEnabled"])
                    {
                        var arr = (Array)(mo.Properties["IpAddress"].Value);
                        st = arr.GetValue(0).ToString();
                        break;
                    }
                }

                return st;
            }
            catch
            {
                return UnKnowInfo;
            }
        }

        /// <summary>
        ///   获取硬盘ID
        /// </summary>
        /// <returns></returns>
        public static string GetDiskId()
        {
            try
            {
                //获取硬盘ID
                var diskId = "";
                var mc = new ManagementClass("Win32_DiskDrive");
                var moc = mc.GetInstances();
                foreach (var o in moc)
                {
                    var mo = (ManagementObject) o;
                    diskId = (string)mo.Properties["SerialNumber"].Value; //SerialNumber
                    if (diskId != null)
                    {
                        return diskId;
                    }
                }

                return diskId;
            }
            catch
            {
                return UnKnowInfo;
            }
        }

        /// <summary>
        ///   获取硬盘序列号。
        ///   网上有提到，用 Win32_DiskDrive，但是用 Win32_DiskDrive 获得的硬盘信息中并不包含 SerialNumber 属性
        /// </summary>
        /// <returns></returns>
        public static string GetDiskSerialNumber()
        {
            try
            {
                //获取硬盘ID
                var diskId = "";
                var mc = new ManagementClass("Win32_PhysicalMedia");
                var moc = mc.GetInstances();
                foreach (var o in moc)
                {
                    var mo = (ManagementObject) o;
                    diskId = (string)mo.Properties["SerialNumber"].Value;
                }

                return diskId;
            }
            catch
            {
                return UnKnowInfo;
            }
        }

        /// <summary>
        ///  操作系统的登录用户名
        /// </summary>
        /// <returns></returns>
        public static string GetUserName()
        {
            try
            {
                var st = "";
                var mc = new ManagementClass("Win32_ComputerSystem");
                var moc = mc.GetInstances();
                foreach (var o in moc)
                {
                    var mo = (ManagementObject) o;
                    st = mo["UserName"].ToString();
                }

                return st;
            }
            catch
            {
                return UnKnowInfo;
            }
        }

        /// <summary>
        ///  获取计算机操作系统类型
        /// </summary>
        /// <returns></returns>
        public static string GetSystemType()
        {
            try
            {
                var st = "";
                var mc = new ManagementClass("Win32_ComputerSystem");
                var moc = mc.GetInstances();
                foreach (var o in moc)
                {
                    var mo = (ManagementObject) o;
                    st = mo["SystemType"].ToString();
                }

                return st;
            }
            catch
            {
                return UnKnowInfo;
            }
        }

        /// <summary>
        ///  获取电脑物理内存
        /// </summary>
        /// <returns></returns>
        public static string GetTotalPhysicalMemory()
        {
            try
            {
                var st = "";
                var mc = new ManagementClass("Win32_ComputerSystem");
                var moc = mc.GetInstances();
                foreach (var o in moc)
                {
                    var mo = (ManagementObject) o;
                    st = mo["TotalPhysicalMemory"].ToString();
                }

                return st;
            }
            catch
            {
                return UnKnowInfo;
            }
        }

        #endregion
    }
}