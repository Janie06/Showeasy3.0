using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace EasyBL
{
    public class Util
    {
        public static string GetLastExceptionMsg(Exception e)
        {
            var sRes = "";

            var eCur = e;

            while (null != eCur.InnerException)
            {
                eCur = eCur.InnerException;
            }

            sRes = eCur.Message;
            return sRes;
        }

        public static string GetValueByPropertyName(object i_oTarget, string i_sPropertyName, out object o_oRes)
        {
            string sMsg = null;
            object oRes = null;
            try
            {
                do
                {
                    var pi = i_oTarget.GetType().GetProperty(i_sPropertyName);
                    if (null == pi)
                    {
                        sMsg = "NO PROPERTY(MAYBE ORM ENTITY NOT SYNC)";
                        break;
                    }

                    oRes = pi.GetValue(i_oTarget, null);
                }
                while (false);
            }
            catch (Exception ex)
            {
                sMsg = GetLastExceptionMsg(ex);
            }

            o_oRes = oRes;
            return sMsg;
        }

        public static string SetValueToInstByPropertyName(object i_oTarget, string i_sPropertyName, object i_v)
        {
            string sMsg = null;

            try
            {
                do
                {
                    var pi = i_oTarget.GetType().GetProperty(i_sPropertyName);
                    if (null == pi)
                    {
                        sMsg = "NO PROPERTY(MAYBE ORM ENTITY NOT SYNC)";
                        break;
                    }

                    var od = ConvertValue(pi, i_v);

                    pi.SetValue(i_oTarget, od, null);
                }
                while (false);
            }
            catch (Exception ex)
            {
                sMsg = GetLastExceptionMsg(ex);
            }
            return sMsg;
        }

        public static object ConvertValue(PropertyInfo i_pi, object i_oValue)
        {
            var tRealType = GetRealType(i_pi.PropertyType);
            return Convert.ChangeType(i_oValue, tRealType);
        }

        public static Type GetRealType(Type i_tTest)
        {
            return Nullable.GetUnderlyingType(i_tTest) ?? i_tTest;
        }

        public static object GetInstByType(Type i_tType)
        {
            return Activator.CreateInstance(i_tType);
        }

        /// <summary>
        /// Create Instance by name
        /// </summary>
        /// <param name="i_sTypeName"></param>
        /// <param name="o_oRes"></param>
        /// <returns></returns>
        public static string GetInstByClassName(string i_sTypeName, out object o_oRes)
        {
            object oRes = null;
            string sMsg = null;

            do
            {
                var tCur = GetTypeByTypeName(i_sTypeName);

                if (null == tCur)
                {
                    sMsg = "NO THIS ENTITY";
                    break;
                }

                oRes = Activator.CreateInstance(tCur);

                if (null == oRes)
                {
                    sMsg = "ENTITY CREATE FAIL";
                    break;
                }
            }
            while (false);

            o_oRes = oRes;
            return sMsg;
        }

        public static Type GetTypeByTypeName(string i_sTypeName)
        {
            Type tRes = null;
            var sCodebase = Assembly.GetExecutingAssembly().GetName().CodeBase;
            sCodebase = sCodebase.Substring(0, sCodebase.LastIndexOf("/"));
            var assAll = AppDomain.CurrentDomain.GetAssemblies().Where(f => false == f.IsDynamic).ToArray();
            var la = new List<Assembly>();

            foreach (Assembly a in assAll)
            {
                try
                {
                    if (null != a.CodeBase && a.CodeBase.StartsWith(sCodebase))
                    {
                        la.Add(a);
                    }
                }
                catch (Exception e)
                {
                    Console.Write(e.Message);
                }
            }

            foreach (Assembly a in la)
            {
                var at = a.GetTypes().ToArray<Type>();

                foreach (Type t in at)
                {
                    if (false == t.IsClass)
                    {
                        continue;
                    }

                    if (t.Name == i_sTypeName)
                    {
                        tRes = t;
                        break;
                    }
                }
                if (null != tRes)
                {
                    break;
                }
            }

            return tRes;
        }

        public static string XLSToDataTable(string i_sSrcExcel, out DataTable o_oRes)
        {
            DataTable dtRes = null;
            string sMsg = null;
            do
            {
                try
                {
                    using (var dtTemp = new DataTable())
                    {
                        if (!File.Exists(i_sSrcExcel))
                        {
                            sMsg = "FILE NOT EXIST";
                            break;
                        }

                        ISheet sheet = null;

                        var sr = new FileStream(i_sSrcExcel, FileMode.Open);

                        if (i_sSrcExcel.ToLower().EndsWith(".xls"))
                        {
                            var workbook = new HSSFWorkbook(sr);
                            sheet = workbook.GetSheetAt(0);
                        }
                        else if (i_sSrcExcel.ToLower().EndsWith(".xlsx"))
                        {
                            var workbook = new XSSFWorkbook(sr);
                            sheet = workbook.GetSheetAt(0);
                        }
                        var sSheetName = sheet.SheetName;

                        var headerRow = sheet.GetRow(0);
                        int cellCount = headerRow.LastCellNum;

                        for (int j = headerRow.FirstCellNum; j < cellCount; j++)
                        {
                            var dc = dtTemp.Columns.Add();
                            object oData = headerRow.GetCell(j);
                            dc.ColumnName = oData.ToString();
                            dc.DataType = typeof(string);
                        }

                        for (int i = (sheet.FirstRowNum + 1); i <= sheet.LastRowNum; i++)
                        {
                            var row = sheet.GetRow(i);
                            var dr = dtTemp.Rows.Add();

                            for (int j = row.FirstCellNum; j < cellCount; j++)
                            {
                                object oData = row.GetCell(j);
                                dr[j] = oData?.ToString();
                            }
                        }

                        sr.Close();
                        sheet = null;
                        dtRes = dtTemp;
                    }
                }
                catch (Exception ex)
                {
                    sMsg = GetLastExceptionMsg(ex);
                }
            }
            while (false);

            o_oRes = dtRes;
            return sMsg;
        }

        public static bool IsValidEmail(string i_sTest)
        {
            return Regex.IsMatch(i_sTest, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase);
        }

        public static string Format(string i_sFormat, object i_oObject)
        {
            foreach (PropertyDescriptor prop in TypeDescriptor.GetProperties(i_oObject))
            {
                i_sFormat = i_sFormat.Replace("{" + prop.Name + "}", (prop.GetValue(i_oObject) ?? "(null)").ToString());
            }
            return i_sFormat;
        }

        public static string ProcessCmd(string i_sProcessName, string i_sAgruments)
        {
            string sRes = null;

            try
            {
                var psi = new ProcessStartInfo
                {
                    Arguments = i_sAgruments,
                    FileName = i_sProcessName,

                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using (var p = new Process
                {
                    StartInfo = psi
                })
                {
                    p.Start();

                    sRes = p.StandardOutput.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                sRes = GetLastExceptionMsg(ex);
            }

            return sRes;
        }
    }
}