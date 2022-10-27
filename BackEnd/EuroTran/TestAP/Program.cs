using System;
using System.Configuration;
using EasyBL.WebApi.Message;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using TestAP.Helper;
using System.Linq;

namespace TestAP
{
    class Program
    {
        static void Main(string[] args)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            Dictionary<string, int> dicSucceccCount = new Dictionary<string, int>();
            try
            {
                do
                {
                    string sStartDate = ConfigurationManager.AppSettings["StartDate"];
                    string sEndDate = ConfigurationManager.AppSettings["EndDate"];
                    string sExport = ConfigurationManager.AppSettings["Export"];
                    string sExportReturn = ConfigurationManager.AppSettings["ExportReturn"];
                    string sImport = ConfigurationManager.AppSettings["Import"];
                    string sImportReturn = ConfigurationManager.AppSettings["ImportReturn"];
                    string sOther = ConfigurationManager.AppSettings["Other"];
                    string sOtherTG = ConfigurationManager.AppSettings["OtherTG"];
                    string sExhibtion = ConfigurationManager.AppSettings["Exhibtion"];
                    string sCustomer = ConfigurationManager.AppSettings["Customer"];
                    string sBillsNo = ConfigurationManager.AppSettings["BillsNo"];
                    string sExhibtionsNo = ConfigurationManager.AppSettings["ExhibtionsNo"];
                    string sCustomersNo = ConfigurationManager.AppSettings["CustomersNo"];
                    List<string> BillsNo = string.IsNullOrEmpty(sBillsNo) ? new List<string>() : sBillsNo.Split(',').ToList();
                    List<string> ExhibtionsNo = string.IsNullOrEmpty(sExhibtionsNo) ? new List<string>() : sExhibtionsNo.Split(',').ToList();
                    List<string> CustomersNo = string.IsNullOrEmpty(sCustomersNo) ? new List<string>() : sCustomersNo.Split(',').ToList();


                    ConvertData cv = new ConvertData();
                    cv.StartDate = sStartDate;
                    cv.EndDate = sEndDate;
                    cv.ExcExport = cv.StringConvertBool(sExport);
                    cv.ExcExportReturn = cv.StringConvertBool(sExportReturn);
                    cv.ExcImport = cv.StringConvertBool(sImport);
                    cv.ExcImportReturn = cv.StringConvertBool(sImportReturn);
                    cv.ExcOther = cv.StringConvertBool(sOther);
                    cv.ExcOtherTG = cv.StringConvertBool(sOtherTG);
                    cv.ExcExhibtion = cv.StringConvertBool(sExhibtion);
                    cv.ExcCustomer = cv.StringConvertBool(sCustomer);
                    cv.BillsNo = BillsNo;
                    cv.ExhibtionsNo = ExhibtionsNo;
                    cv.CustomersNo = CustomersNo;

                    dicSucceccCount = cv.StartConvert(out sMsg);

                    if (sMsg != null)
                    {
                        break;
                    }

                    Console.WriteLine("完成");
                }
                while (false);
            }
            catch (Exception ex)
            {
                sMsg = ex.Message;
            }

            if (sMsg != null)
            {
                Console.WriteLine(sMsg);
            }
            else
            {
                foreach (var item in dicSucceccCount)
                {
                    Console.WriteLine($"{ item.Key }:共新增 { item.Value } 筆");
                }

            }


            Console.ReadLine();
        }

        static string GetObjectValue(JObject i_oJObject, string i_sColumn)
        {
            string sReturnValue = string.Empty;

            if (i_oJObject[i_sColumn] != null)
            {
                sReturnValue = i_oJObject[i_sColumn].ToString();
            }

            return sReturnValue;
        }
    }
}
