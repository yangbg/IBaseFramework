//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Reflection;
//using System.Text;
//using System.Text.RegularExpressions;
//using System.Web;

//namespace IFramework.Utility.Helper
//{
//    /// <summary>
//    /// Excel操作类
//    /// </summary>
//    public class ExcelHelper
//    {
//        #region Excel导入

//        /// <summary>
//        /// 从Excel取数据并记录到List集合里
//        /// </summary>
//        /// <param name="cellHeard">单元头的值和名称：{ { "UserName", "姓名" }, { "Age", "年龄" } };</param>
//        /// <param name="filePath">保存文件绝对路径</param>
//        /// <param name="errorMsg">错误信息</param>
//        /// <returns>转换后的List对象集合</returns>
//        public static List<T> ExcelToEntityList<T>(Dictionary<string, string> cellHeard, string filePath,
//            out StringBuilder errorMsg) where T : new()
//        {
//            List<T> enlist = new List<T>();
//            errorMsg = new StringBuilder();
//            try
//            {
//                if (Regex.IsMatch(filePath, ".xls$")) // 2003
//                {
//                    enlist = Excel2003ToEntityList<T>(cellHeard, filePath,null, out errorMsg);
//                }
//                else if (Regex.IsMatch(filePath, ".xlsx$")) // 2007
//                {
//                    //return FailureResultMsg("请选择Excel文件"); // 未设计
//                }
//                return enlist;
//            }
//            catch (Exception ex)
//            {
//                throw ex;
//            }
//        }

//        /// <summary>
//        /// 传入数据流
//        /// </summary>
//        /// <typeparam name="T"></typeparam>
//        /// <param name="cellHeard"></param>
//        /// <param name="stream"></param>
//        /// <param name="errorMsg"></param>
//        /// <returns></returns>
//        public static List<T> ExcelToEntityList<T>(Dictionary<string, string> cellHeard, Stream stream,
//            out StringBuilder errorMsg) where T : new()
//        {
//            var enlist = new List<T>();
//            errorMsg = new StringBuilder();
//            try
//            {
//                if (stream != null) // 2003
//                {
//                    enlist = Excel2003ToEntityList<T>(cellHeard, string.Empty, stream, out errorMsg);
//                }

//                return enlist;
//            }
//            catch (Exception ex)
//            {
//                throw ex;
//            }
//        }

//        #endregion Excel导入

//        #region Excel导出

//        /// <summary>
//        /// 实体类集合导出到EXCLE2003
//        /// </summary>
//        /// <param name="cellHeard">单元头的Key和Value：{ { "UserName", "姓名" }, { "Age", "年龄" } };</param>
//        /// <param name="enList">数据源</param>
//        /// <param name="sheetName">工作表名称</param>
//        /// <returns>文件的下载地址</returns>
//        public static string EntityListToExcel(Dictionary<string, string> cellHeard, IList enList, string sheetName, string[] items = null)
//        {
//            try
//            {
//                string fileName = sheetName + "-" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".xls"; // 文件名称
//                string urlPath = "UpFiles/ExcelFiles/" + fileName; // 文件下载的URL地址，供给前台下载
//                string filePath = HttpContext.Current.Server.MapPath("\\" + urlPath); // 文件路径

//                // 1.检测是否存在文件夹，若不存在就建立个文件夹
//                string directoryName = Path.GetDirectoryName(filePath);
//                if (!Directory.Exists(directoryName))
//                {
//                    Directory.CreateDirectory(directoryName);
//                }

//                // 2.解析单元格头部，设置单元头的中文名称
//                HSSFWorkbook workbook = new HSSFWorkbook(); // 工作簿
//                ISheet sheet = workbook.CreateSheet(sheetName); // 工作表
//                IRow row = sheet.CreateRow(0);
//                List<string> keys = cellHeard.Keys.ToList();
//                for (int i = 0; i < keys.Count; i++)
//                {
//                    row.CreateCell(i).SetCellValue(cellHeard[keys[i]]); // 列名为Key的值
//                }

//                //有下拉选项，增加下拉表头
//                if (items != null)
//                {
//                    var regions = new CellRangeAddressList(1, 65535, 0, 0);
//                    var constraint = DVConstraint.CreateExplicitListConstraint(items); //这是下拉选项值
//                    var dataValidate = new HSSFDataValidation(regions, constraint);

//                    sheet.AddValidationData(dataValidate);
//                }

//                // 3.List对象的值赋值到Excel的单元格里
//                int rowIndex = 1; // 从第二行开始赋值(第一行已设置为单元头)
//                foreach (var en in enList)
//                {
//                    IRow rowTmp = sheet.CreateRow(rowIndex);
//                    for (int i = 0; i < keys.Count; i++) // 根据指定的属性名称，获取对象指定属性的值
//                    {
//                        string cellValue = ""; // 单元格的值
//                        object properotyValue = null; // 属性的值
//                        PropertyInfo properotyInfo = null; // 属性的信息

//                        // 3.1 若属性头的名称包含'.',就表示是子类里的属性，那么就要遍历子类，eg：UserEn.UserName
//                        if (keys[i].IndexOf(".") >= 0)
//                        {
//                            // 3.1.1 解析子类属性(这里只解析1层子类，多层子类未处理)
//                            string[] properotyArray = keys[i].Split(new string[] {"."},
//                                StringSplitOptions.RemoveEmptyEntries);
//                            string subClassName = properotyArray[0]; // '.'前面的为子类的名称
//                            string subClassProperotyName = properotyArray[1]; // '.'后面的为子类的属性名称
//                            PropertyInfo subClassInfo = en.GetType().GetProperty(subClassName);
//                                // 获取子类的类型
//                            if (subClassInfo != null)
//                            {
//                                // 3.1.2 获取子类的实例
//                                var subClassEn = en.GetType().GetProperty(subClassName).GetValue(en, null);
//                                // 3.1.3 根据属性名称获取子类里的属性类型
//                                properotyInfo = subClassInfo.PropertyType.GetProperty(subClassProperotyName);
//                                if (properotyInfo != null)
//                                {
//                                    properotyValue = properotyInfo.GetValue(subClassEn, null); // 获取子类属性的值
//                                }
//                            }
//                        }
//                        else
//                        {
//                            // 3.2 若不是子类的属性，直接根据属性名称获取对象对应的属性
//                            properotyInfo = en.GetType().GetProperty(keys[i]);
//                            if (properotyInfo != null)
//                            {
//                                properotyValue = properotyInfo.GetValue(en, null);
//                            }
//                        }

//                        // 3.3 属性值经过转换赋值给单元格值
//                        if (properotyValue != null)
//                        {
//                            cellValue = properotyValue.ToString();
//                            // 3.3.1 对时间初始值赋值为空
//                            if (cellValue.Trim() == "0001/1/1 0:00:00" || cellValue.Trim() == "0001/1/1 23:59:59")
//                            {
//                                cellValue = "";
//                            }
//                        }

//                        // 3.4 填充到Excel的单元格里
//                        rowTmp.CreateCell(i).SetCellValue(cellValue);
//                    }
//                    rowIndex++;
//                }

//                // 4.生成文件
//                FileStream file = new FileStream(filePath, FileMode.Create);
//                workbook.Write(file);
//                file.Close();

//                // 5.返回下载路径
//                return urlPath;
//            }
//            catch (Exception ex)
//            {
//                throw ex;
//            }
//        }

//        #endregion Excel导出

//        #region 下载带下拉选项的模板

//        /// <summary>
//        /// 下载带下拉选项的模板
//        /// </summary>
//        /// <param name="cellHeard"></param>
//        /// <param name="items">下拉选项</param>
//        /// <param name="sheetName">sheet名称</param>
//        /// <param name="excelName">excel名称</param>
//        /// <param name="description">描述</param>
//        /// <returns></returns>
//        public static string CreateDropdownExcel(Dictionary<string, string> cellHeard, string[] items,string sheetName,string excelName,string description)
//        {
//            HSSFWorkbook wk = new HSSFWorkbook();
//            DocumentSummaryInformation dsi = PropertySetFactory.CreateDocumentSummaryInformation();
//            dsi.Company = "启明合创";
//            wk.DocumentSummaryInformation = dsi;

//            //创建一个名称为mySheet的表
//            ISheet tb = wk.CreateSheet(sheetName);
//            ICellStyle cellStyle = wk.CreateCellStyle();
//            //导入说明sheet
//            try
//            {
//                IDataFormat textFormat = wk.CreateDataFormat();
//                cellStyle.DataFormat = textFormat.GetFormat("text");

//                IRow row = tb.CreateRow(0);
//                List<string> keys = cellHeard.Keys.ToList();
//                for (int i = 0; i < keys.Count; i++)
//                {
//                    row.CreateCell(i).SetCellValue(cellHeard[keys[i]]); // 列名为Key的值

//                    //第一列选择框除外，其余都设为文本 todo 根据header的类型来设置
//                    if (i > 0)
//                        tb.SetDefaultColumnStyle(i, cellStyle);
//                }

//                CellRangeAddressList regions = new CellRangeAddressList(1, 65535, 0, 0);
//                DVConstraint constraint = DVConstraint.CreateExplicitListConstraint(items);//这是下拉选项值
//                HSSFDataValidation dataValidate = new HSSFDataValidation(regions, constraint);
//                tb.AddValidationData(dataValidate);

//                //设置
//            }
//            catch (Exception e)
//            {
//                throw e;
//            }

//            string fileName = excelName + "-" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".xls"; // 文件名称
//            string urlPath = "UpFiles/ExcelFiles/" + fileName; // 文件下载的URL地址，供给前台下载
//            string filePath = HttpContext.Current.Server.MapPath("\\" + urlPath); // 文件路径

//            // 1.检测是否存在文件夹，若不存在就建立个文件夹
//            string directoryName = Path.GetDirectoryName(filePath);
//            if (!Directory.Exists(directoryName))
//            {
//                Directory.CreateDirectory(directoryName);
//            }

//            FileStream file = new FileStream(filePath, FileMode.Create);
//            wk.Write(file);
//            file.Close();

//            return filePath;
//        }

//        #endregion

//        #region 私有

//        /// <summary>
//        /// 从Excel2003取数据并记录到List集合里
//        /// </summary>
//        /// <param name="cellHeard">单元头的Key和Value：{ { "UserName", "姓名" }, { "Age", "年龄" } };</param>
//        /// <param name="filePath">保存文件绝对路径</param>
//        /// <param name="stream"></param>
//        /// <param name="errorMsg">错误信息</param>
//        /// <returns>转换好的List对象集合</returns>
//        private static List<T> Excel2003ToEntityList<T>(Dictionary<string, string> cellHeard, string filePath,Stream stream,
//            out StringBuilder errorMsg) where T : new()
//        {
//            errorMsg = new StringBuilder(); // 错误信息,Excel转换到实体对象时，会有格式的错误信息
//            List<T> enlist = new List<T>(); // 转换后的集合
//            List<string> keys = cellHeard.Keys.ToList(); // 要赋值的实体对象属性名称
//            try
//            {
//                if (!string.IsNullOrEmpty(filePath))
//                {
//                    using (var fs = File.OpenRead(filePath))
//                    {
//                        ReadFromStream(cellHeard, errorMsg, fs, keys, enlist);
//                    }
//                }
//                else if (stream != null)
//                {
//                    ReadFromStream(cellHeard, errorMsg, stream, keys, enlist);
//                }

//                return enlist;
//            }
//            catch (Exception ex)
//            {
//                throw ex;
//            }
//        }

//        private static void ReadFromStream<T>(Dictionary<string, string> cellHeard, StringBuilder errorMsg, Stream fs, List<string> keys,
//            List<T> enlist) where T : new()
//        {
//            HSSFWorkbook workbook = new HSSFWorkbook(fs);
//            HSSFSheet sheet = (HSSFSheet) workbook.GetSheetAt(0); // 获取此文件第一个Sheet页
//            for (int i = 1; i <= sheet.LastRowNum; i++) // 从1开始，第0行为单元头
//            {
//                // 1.判断当前行是否空行，若空行就不在进行读取下一行操作，结束Excel读取操作
//                if (sheet.GetRow(i) == null)
//                {
//                    break;
//                }

//                T en = new T();
//                string errStr = ""; // 当前行转换时，是否有错误信息，格式为：第1行数据转换异常：XXX列；
//                for (int j = 0; j < keys.Count; j++)
//                {
//                    // 2.若属性头的名称包含'.',就表示是子类里的属性，那么就要遍历子类，eg：UserEn.TrueName
//                    if (keys[j].IndexOf(".") >= 0)
//                    {
//                        // 2.1解析子类属性
//                        string[] properotyArray = keys[j].Split(new string[] {"."},
//                            StringSplitOptions.RemoveEmptyEntries);
//                        string subClassName = properotyArray[0]; // '.'前面的为子类的名称
//                        string subClassProperotyName = properotyArray[1]; // '.'后面的为子类的属性名称
//                        PropertyInfo subClassInfo = en.GetType().GetProperty(subClassName);
//                        // 获取子类的类型
//                        if (subClassInfo != null)
//                        {
//                            // 2.1.1 获取子类的实例
//                            var subClassEn = en.GetType().GetProperty(subClassName).GetValue(en, null);
//                            // 2.1.2 根据属性名称获取子类里的属性信息
//                            PropertyInfo properotyInfo =
//                                subClassInfo.PropertyType.GetProperty(subClassProperotyName);
//                            if (properotyInfo != null)
//                            {
//                                try
//                                {
//                                    // Excel单元格的值转换为对象属性的值，若类型不对，记录出错信息
//                                    properotyInfo.SetValue(subClassEn,
//                                        GetExcelCellToProperty(properotyInfo.PropertyType,
//                                            sheet.GetRow(i).GetCell(j)), null);
//                                }
//                                catch (Exception e)
//                                {
//                                    if (errStr.Length == 0)
//                                    {
//                                        errStr = "第" + i + "行数据转换异常：";
//                                    }
//                                    errStr += cellHeard[keys[j]] + "列；";
//                                }
//                            }
//                        }
//                    }
//                    else
//                    {
//                        // 3.给指定的属性赋值
//                        PropertyInfo properotyInfo = en.GetType().GetProperty(keys[j]);
//                        if (properotyInfo != null)
//                        {
//                            try
//                            {
//                                // Excel单元格的值转换为对象属性的值，若类型不对，记录出错信息
//                                properotyInfo.SetValue(en,
//                                    GetExcelCellToProperty(properotyInfo.PropertyType,
//                                        sheet.GetRow(i).GetCell(j)), null);
//                            }
//                            catch (Exception e)
//                            {
//                                if (errStr.Length == 0)
//                                {
//                                    errStr = "第" + i + "行数据转换异常：";
//                                }
//                                errStr += cellHeard[keys[j]] + "列；";
//                            }
//                        }
//                    }
//                }
//                // 若有错误信息，就添加到错误信息里
//                if (errStr.Length > 0)
//                {
//                    errorMsg.AppendLine(errStr);
//                }
//                enlist.Add(en);
//            }
//        }

//        /// <summary>
//        /// 从Excel获取值传递到对象的属性里
//        /// </summary>
//        /// <param name="distanceType">目标对象类型</param>
//        /// <param name="sourceCell">对象属性的值</param>
//        private static Object GetExcelCellToProperty(Type distanceType, ICell sourceCell)
//        {
//            object rs = distanceType.IsValueType ? Activator.CreateInstance(distanceType) : null;

//            // 1.判断传递的单元格是否为空
//            if (sourceCell == null || string.IsNullOrEmpty(sourceCell.ToString()))
//            {
//                return rs;
//            }

//            // 2.Excel文本和数字单元格转换，在Excel里文本和数字是不能进行转换，所以这里预先存值
//            object sourceValue = null;
//            switch (sourceCell.CellType)
//            {
//                case CellType.Blank:
//                    break;

//                case CellType.Boolean:
//                    break;

//                case CellType.Error:
//                    break;

//                case CellType.Formula:
//                    break;

//                case CellType.Numeric:
//                    sourceValue = sourceCell.NumericCellValue;
//                    break;

//                case CellType.String:
//                    sourceValue = sourceCell.StringCellValue;
//                    break;

//                case CellType.Unknown:
//                    break;

//                default:
//                    break;
//            }

//            string valueDataType = distanceType.Name;

//            // 在这里进行特定类型的处理
//            switch (valueDataType.ToLower()) // 以防出错，全部小写
//            {
//                case "string":
//                    rs = sourceValue.ToString();
//                    break;
//                case "int":
//                case "int16":
//                case "int32":
//                    rs = (int)Convert.ChangeType(sourceCell.NumericCellValue.ToString(), distanceType);
//                    break;
//                case "float":
//                case "single":
//                    rs = (float)Convert.ChangeType(sourceCell.NumericCellValue.ToString(), distanceType);
//                    break;
//                case "datetime":
//                    rs = sourceCell.DateCellValue;
//                    break;
//                case "guid":
//                    rs = (Guid)Convert.ChangeType(sourceCell.NumericCellValue.ToString(), distanceType);
//                    return rs;
//            }
//            return rs;
//        }

//        /// <summary>
//        /// 保存Excel文件
//        /// <para>Excel的导入导出都会在服务器生成一个文件</para>
//        /// <para>路径：UpFiles/ExcelFiles</para>
//        /// </summary>
//        /// <param name="file">传入的文件对象</param>
//        /// <returns>如果保存成功则返回文件的位置;如果保存失败则返回空</returns>
//        private static string SaveExcelFile(HttpPostedFile file)
//        {
//            try
//            {
//                var fileName = file.FileName.Insert(file.FileName.LastIndexOf('.'),
//                    "-" + DateTime.Now.ToString("yyyyMMddHHmmssfff"));
//                var filePath = Path.Combine(HttpContext.Current.Server.MapPath("~/UpFiles/ExcelFiles"), fileName);
//                string directoryName = Path.GetDirectoryName(filePath);
//                if (!Directory.Exists(directoryName))
//                {
//                    Directory.CreateDirectory(directoryName);
//                }
//                file.SaveAs(filePath);
//                return filePath;
//            }
//            catch
//            {
//                return string.Empty;
//            }
//        }
//        #endregion

//    }
//}
