using Entity.Sugar;
using Newtonsoft.Json.Linq;
using SqlSugar;
using SqlSugar.Base;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Packaging;
using System.Linq;
using Newtonsoft.Json;
using EasyBL.WebApi.Message;

namespace EasyBL
{
    /// <summary>
    /// Common 的摘要描述
    /// </summary>
    public class CommonRPT
    {
        private static bool? ShowSource;

        public static string FilePath = @"Document\EurotranFile\RPT";
        public static string PassStatus = "2,4,5";

        public static bool RPTShow()
        {
            if (ShowSource == null)
            {
                ShowSource = !string.IsNullOrWhiteSpace(Common.GetAppSettings("RPTShowSource").Trim());
            }
            return ShowSource.Value;
        }
        /// <summary>
        /// 相對應組織的幣別單位
        /// </summary>
        /// <param name="OrgID"></param>
        /// <returns></returns>
        public static string GetCurrencyUnit(string OrgID)
        {
            if (OrgID.ToUpper() == "SG")
                return "RMB¥";
            return "NT$";
        }

        /// <summary>
        /// 相對應組織的進位
        /// </summary>
        /// <param name="OrgID"></param>
        /// <returns></returns>
        public static int GetRoundingPoint(string OrgID)
        {
            var RoundingPoint = 0;
            if (OrgID == "SG")
            {
                RoundingPoint = 2;
            }
            return RoundingPoint;
        }

        /// <summary>
        /// 取得負責的業務，若負責人員為空，則撈出該部門所有的人。
        /// </summary> 
        /// <param name="i_crm"></param>
        /// <param name="db"></param>
        /// <param name="sResponsibleDeptID"></param>
        /// <param name="sResponsiblePerson"></param>
        /// <returns></returns>
        public static string GetResponsiblePersons(SqlSugarClient db, string OrgID, string sResponsibleDeptID, string sResponsiblePerson)
        {
            var Result = sResponsiblePerson;
            if (string.IsNullOrEmpty(sResponsiblePerson) && !string.IsNullOrEmpty(sResponsibleDeptID))
            {
                var spOrgID = new SugarParameter("@OrgID", OrgID);
                var spDepartID = new SugarParameter("@DepartID", sResponsibleDeptID);

                var AllOfDepartMembers = db.Ado.SqlQuery<string>(@"select MemberID  from [dbo].[OVW_SYS_Members] 
                        where OrgID = @OrgID and DepartmentID in 
                        ( SELECT * FROM [dbo].[OFN_SYS_GetChilDepartmentIdByDepartmentId] (@OrgID, @DepartID)) ", spOrgID, spDepartID).ToArray();

                var AllOfDepartIDs = db.Ado.SqlQuery<string>(@"SELECT * FROM [dbo].[OFN_SYS_GetChilDepartmentIdByDepartmentId] (@OrgID, @DepartID)", spOrgID, spDepartID).ToArray();

                //找出進口/出口/其他/其他(TG)屬於該部門的業務人員
                var cmdExport = db.Queryable<OTB_OPM_ExportExhibition>()
                                .Where(x => x.OrgID == OrgID && AllOfDepartIDs.Contains(x.DepartmentID))
                                .Select(x => new OTB_OPM_ExportExhibition()
                                {
                                    ResponsiblePerson = x.ResponsiblePerson,
                                    DepartmentID = x.DepartmentID
                                });

                var cmdImport = db.Queryable<OTB_OPM_ImportExhibition>()
                                  .Where(x => x.OrgID == OrgID && AllOfDepartIDs.Contains(x.DepartmentID))
                                  .Select(x => new OTB_OPM_ExportExhibition()
                                  {
                                      ResponsiblePerson = x.ResponsiblePerson,
                                      DepartmentID = x.DepartmentID
                                  });

                var cmdOther = db.Queryable<OTB_OPM_OtherExhibition>()
                                  .Where(x => x.OrgID == OrgID && AllOfDepartIDs.Contains(x.DepartmentID))
                                  .Select(x => new OTB_OPM_ExportExhibition()
                                  {
                                      ResponsiblePerson = x.ResponsiblePerson,
                                      DepartmentID = x.DepartmentID
                                  });

                var cmdOtherTG = db.Queryable<OTB_OPM_OtherExhibitionTG>()
                                 .Where(x => x.OrgID == OrgID && AllOfDepartIDs.Contains(x.DepartmentID))
                                 .Select(x => new OTB_OPM_ExportExhibition()
                                 {
                                     ResponsiblePerson = x.ResponsiblePerson,
                                     DepartmentID = x.DepartmentID
                                 });


                List<OTB_OPM_ExportExhibition> lResponseDept = db.UnionAll(cmdExport, cmdImport, cmdOther, cmdOtherTG).ToList();

                List<string> lPersonList = AllOfDepartMembers.ToList(); //人員清單
                var MembersOfBills = lResponseDept.Select(p => p.ResponsiblePerson).Distinct().ToList();
                lPersonList = lPersonList.Concat(MembersOfBills).Distinct().ToList();

                Result = string.Join(",", lPersonList);
            }

            return Result;
        }
        public static string[] GetChildDepteList(SqlSugarClient db, string OrgID, string sResponsibleDeptID)
        {
            var Result = new string[] { };
            if (!string.IsNullOrEmpty(sResponsibleDeptID))
            {
                var spOrgID = new SugarParameter("@OrgID", OrgID);
                var spDepartID = new SugarParameter("@DepartID", sResponsibleDeptID);
                var AllOfDepartIDs = db.Ado.SqlQuery<string>(@"SELECT * FROM [dbo].[OFN_SYS_GetChilDepartmentIdByDepartmentId] (@OrgID, @DepartID)", spOrgID, spDepartID).ToArray();
                Result = AllOfDepartIDs;
            }
            return Result;
        }

        public static List<OVW_OPM_ALLExps> GetMatchedExps(SqlSugarClient db, string OrgID, string[] ResponsibleDeptIDs, string ResponsiblePerson)
        {
            return db.Queryable<OVW_OPM_ALLExps>()
                .Where(t1 => t1.OrgID == OrgID && t1.IsVoid == "N")
                .WhereIF(ResponsibleDeptIDs.Any(), t1 => SqlFunc.ContainsArray(ResponsibleDeptIDs, t1.DepartmentID))
                .WhereIF(!string.IsNullOrWhiteSpace(ResponsiblePerson), t1 => t1.ResponsiblePerson == ResponsiblePerson)
                .ToList();
        }


        /// <summary>
        /// 取得相對應excel
        /// </summary>
        /// <param name="RPTCode"></param>
        /// <param name="RPTName"></param>
        /// <returns></returns>

        public static Tuple<string, string> GetExcutePath(string RPTCode, string RPTName)
        {
            var sOutPut = Common.ConfigGetValue(@"", @"OutFilesPath");
            var sTempPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FilePath);
            sTempPath = Path.Combine(sTempPath, RPTCode + ".xlsx");
            var sBase = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"");
            sOutPut = sBase + sOutPut;
            Common.FnCreateDir(sOutPut);//如果不存在就創建文件夾
            var sFileName = RPTName + DateTime.Now.ToString(@"yyyy-MM-dd") + "_" + Guid.NewGuid();
            //建立臨時文件
            var sTempFile = Path.GetTempPath() + sFileName + @".xlsx";
            sOutPut += sFileName + @".xlsx";
            if (File.Exists(sTempFile))
            {
                File.Delete(sTempFile);
            }
            File.Copy(sTempPath, sTempFile);
            return new Tuple<string, string>(sOutPut, sTempFile);
        }



        /// <summary>
        /// 根據CBM占比平攤成本
        /// </summary>
        /// <param name="db"></param>
        /// <param name="_BillInfo"></param>
        /// <param name="actualCost"></param>
        /// <returns></returns>
        public static ShareCost GetSharedActualCost(SqlSugarClient db, OVW_OPM_BillInfo _BillInfo, string actualCost)
        {
            string PassStatus = CommonRPT.PassStatus;
            var sAuditVal = _BillInfo.AuditVal == null ? "" : _BillInfo.AuditVal;
            var UsedVolume = _BillInfo.Volume == null ? "0" : _BillInfo.Volume;
            double dUsedVolume = 0;
            double dActualCost = 0;
            double.TryParse(UsedVolume, out dUsedVolume);
            double.TryParse(actualCost, out dActualCost);

            if (PassStatus.Contains(sAuditVal) && dUsedVolume > 0)
            {
                var spOrgID = new SugarParameter("@OrgID", _BillInfo.OrgID);
                var spParentId = new SugarParameter("@ParentId", _BillInfo.ParentId);
                var spIsReturn = new SugarParameter("@IsRetn", _BillInfo.IsRetn);
                var sVolumes = db.Ado.SqlQuery<string>("SELECT Volume FROM OVW_OPM_BillInfo WHERE OrgID =@OrgID and ParentId = @ParentId and IsRetn = @IsRetn and AuditVal in('2','4','5')", spOrgID, spParentId, spIsReturn).ToArray();
                var dVolumes = sVolumes.Sum(c =>
                {
                    double.TryParse(c, out double Result);
                    return Result;
                });
                if (dVolumes == 0)
                    return new ShareCost();
                var Persent = dUsedVolume / dVolumes;
                var ShareCost = new ShareCost()
                {
                    Persent = Persent,
                    SharedActualCost = Math.Round(dActualCost * Persent, MidpointRounding.AwayFromZero)
                };
                return ShareCost;
            }
            else
            {
                return new ShareCost();
            }
        }

        public static List<CbmVolume> GetAllCBMUsages(SqlSugarClient db, string OrgID)
        {
            var PassStatus = new string[] { "2", "4", "5" };
            var CbmVolumes = db.Queryable<OVW_OPM_BillInfo>().Where(t1 => t1.OrgID == OrgID && SqlFunc.ContainsArray(PassStatus, t1.AuditVal))
                .Select(t1 => new CbmVolume() { OrgID = t1.OrgID, ParentID = t1.ParentId, BillNO = t1.BillNO, sVolumes = t1.Volume, IsReturn = t1.IsRetn, ReFlow = t1.ReFlow }).ToList();
            return CbmVolumes;
        }


        /// <summary>
        /// 取得帳單項目中的代墊款
        /// </summary>
        /// <param name="saPrepayFee"></param>
        /// <param name="bills"></param>
        /// <param name="bindToBillNo"></param>
        /// <returns></returns>
        public static double GetPrepayForCustomer(string saPrepayFee, string bills, string bindToBillNo = "")
        {
            if (!saPrepayFee.Any() || string.IsNullOrWhiteSpace(bills))
                return 0;
            var JABills = JArray.Parse(bills);
            var FoundPrepayFeeItem = JABills.Where(jo => jo["FinancialCode"] != null && saPrepayFee.Contains(jo["FinancialCode"].ToString()))
            .Select(jo =>
            {

                double FinancialAmount = 0;
                double FinancialTWAmount = 0;
                var FinancialCode = jo["FinancialCode"] == null ? "" : jo["FinancialCode"].ToString();
                if (saPrepayFee.Contains(FinancialCode))
                {
                    double.TryParse(jo["FinancialAmount"]?.ToString(), out FinancialAmount);
                    double.TryParse(jo["FinancialTWAmount"]?.ToString(), out FinancialTWAmount);
                }
                var FinancialBillNO = jo["BillNO"] == null ? "" : jo["BillNO"].ToString();
                var NoneEmpty = !string.IsNullOrWhiteSpace(FinancialBillNO) && !string.IsNullOrWhiteSpace(bindToBillNo);
                if (NoneEmpty && bindToBillNo != FinancialBillNO)
                {
                    return new { FinancialAmount = 0.0, FinancialTWAmount = 0.0 };
                }
                return new { FinancialAmount, FinancialTWAmount };
            }).ToList();
            return FoundPrepayFeeItem.Sum(f => f.FinancialTWAmount);
        }

        public static decimal GetShareCost(List<FeeItem> feeItems, List<CbmVolume> cbmVolumes, string AllocatedBillNO, string[] MatchFeeCode = null)
        {
            var Cost = decimal.Zero;
            var GeneralCBMPercent = CbmVolume.GetCBMPercent(cbmVolumes, AllocatedBillNO);
            if (MatchFeeCode != null && MatchFeeCode.Any())
            {
                feeItems = feeItems.Where(c => MatchFeeCode.Contains(c.FinancialCode)).ToList();
            }
            //分成1.不指定均攤、2.指定均攤
            foreach (var feeItem in feeItems)
            {
                var NoAllocated = feeItem.AllocatedToBillNOs.Count == 0;
                switch (NoAllocated)
                {
                    case true:
                        {
                            Cost += (feeItem.TWAmount * GeneralCBMPercent);
                        }
                        break;
                    case false:
                        {
                            var CheckSignedBillNO = feeItem.AllocatedToBillNOs.Any(c => c == AllocatedBillNO);
                            if (CheckSignedBillNO)
                            {
                                var AllocatedCBMVolumes = cbmVolumes.Where(c => feeItem.AllocatedToBillNOs.Contains(c.BillNO)).ToList();
                                var AllocatedCBMPercent = CbmVolume.GetCBMPercent(AllocatedCBMVolumes, AllocatedBillNO);
                                Cost += (feeItem.TWAmount * AllocatedCBMPercent.ObjToDecimal());
                            }
                        }
                        break;
                }
            }
            return Cost;
        }



        /// <summary>
        /// 轉換成FeeItems
        /// </summary>
        /// <param name="ItemsJsons"></param>
        /// <returns></returns>
        public static List<FeeItem> ToFeeItems(string ItemsJsons)
        {
            var FeeItems = new List<FeeItem>();
            var JABills = JArray.Parse(ItemsJsons);
            foreach (var JABill in JABills)
            {
                var FinancialCode = JABill["FinancialCode"].ObjToString();
                var FinancialAmount = JABill["FinancialAmount"].ObjToDecimal();
                var FinancialTWAmount = JABill["FinancialTWAmount"].ObjToDecimal();
                var sBillNOs = JABill["BillNO"].ObjToString();
                var BindToBillNOList = sBillNOs.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                FeeItems.Add(new FeeItem()
                {
                    FinancialCode = FinancialCode,
                    OriginalCurrencyAmount = FinancialAmount,
                    TWAmount = FinancialTWAmount,
                    AllocatedToBillNOs = BindToBillNOList,
                });
            }
            return FeeItems;
        }
        /// <summary>
        /// 取得進出口、其他、其他駒奕中
        /// 實際成本項目、實際成本
        /// </summary>
        /// <param name="db"></param>
        /// <param name="ActualCostFeeItems"></param>
        /// <param name="sActualCost"></param>
        /// <param name="sTransportationMode"></param>
        /// <param name="_sBillNO"></param>
        /// <param name="sId"></param>
        /// <param name="sIsRetn"></param>
        /// <param name="sReFlow"></param>
        /// <param name="sBillType"></param>
        public static void CalcuCostAndProfit(SqlSugarClient db, ref string ActualCostFeeItems, ref string sActualCost, ref string sTransportationMode, string _sBillNO, string sId, string sIsRetn, string sReFlow, string sBillType)
        {
            if (sBillType == "OtherBusiness_Upd")
            {
                // OTB_OPM_OtherExhibition.ActualCost
                var oOther = db.Queryable<OTB_OPM_OtherExhibition>().Single(it => it.Guid == sId);
                if (oOther != null)
                {
                    var joActualCost = (JObject)JsonConvert.DeserializeObject(oOther.ActualCost);
                    ActualCostFeeItems = joActualCost["FeeItems"] != null ? joActualCost["FeeItems"].ToString() : "";
                    if (joActualCost["AmountTaxSum"] != null)
                    {
                        sActualCost = joActualCost["AmountTaxSum"].ToString();
                    }
                }
            }
            else if (sBillType == "ExhibitionImport_Upd")
            {
                // OTB_OPM_ImportExhibition.ReturnBills   
                // OTB_OPM_ImportExhibition.ReImports
                // OTB_OPM_ImportExhibition.ActualCost
                // OTB_OPM_ImportExhibition.TransportationMode
                var oImport = db.Queryable<OTB_OPM_ImportExhibition>().Single(it => it.ImportBillNO == sId);
                if (oImport != null)
                {
                    if (sIsRetn == "Y")
                    {
                        var jaReturnBills = (JArray)JsonConvert.DeserializeObject(oImport.ReturnBills);
                        for (var idx = 0; idx < jaReturnBills.Count; idx++)
                        {
                            if (sReFlow == (idx + 1).ToString())
                            {
                                var joReturn = (JObject)jaReturnBills[idx];
                                ActualCostFeeItems = ((JObject)joReturn["ActualCost"])["FeeItems"] != null ? ((JObject)joReturn["ActualCost"])["FeeItems"].ToString() : "";
                                if (((JObject)joReturn["ActualCost"])["AmountTaxSum"] != null)
                                {
                                    sActualCost = ((JObject)joReturn["ActualCost"])["AmountTaxSum"].ToString();
                                }
                                break;
                            }
                        }
                        var jaReImports = (JArray)JsonConvert.DeserializeObject(oImport.ReImports);
                        for (var idx = 0; idx < jaReImports.Count; idx++)
                        {
                            if (sReFlow == idx.ToString())
                            {
                                var joReturn = (JObject)jaReImports[idx];
                                sTransportationMode = ((JObject)joReturn["ReImportData"])["TransportationMode"].ToString();
                                break;
                            }
                        }
                    }
                    else
                    {

                        var joActualCost = (JObject)JsonConvert.DeserializeObject(oImport.ActualCost);
                        ActualCostFeeItems = joActualCost["FeeItems"] != null ? joActualCost["FeeItems"].ToString() : "";
                        if (joActualCost["AmountTaxSum"] != null)
                        {
                            sActualCost = joActualCost["AmountTaxSum"].ToString();
                        }
                        sTransportationMode = oImport.TransportationMode;
                    }
                }
            }
            else if (sBillType == "ExhibitionExport_Upd")
            {
                // OTB_OPM_ExportExhibition.ReturnBills
                // OTB_OPM_ExportExhibition.Exhibitors
                // OTB_OPM_ExportExhibition.ActualCost
                // OTB_OPM_ExportExhibition.TransportationMode
                var oExport = db.Queryable<OTB_OPM_ExportExhibition>().Single(it => it.ExportBillNO == sId);
                if (oExport != null)
                {
                    if (sIsRetn == "Y")
                    {
                        var jaReturns = (JArray)JsonConvert.DeserializeObject(oExport.ReturnBills);
                        var sBillParentId = "";
                        foreach (JObject jo in jaReturns)
                        {
                            var jaReturnBills = (JArray)jo["Bills"];
                            foreach (JObject joBill in jaReturnBills)
                            {
                                if (joBill["BillNO"].ToString() == _sBillNO)
                                {
                                    ActualCostFeeItems = ((JObject)jo["ActualCost"])["FeeItems"] != null ? ((JObject)jo["ActualCost"])["FeeItems"].ToString() : "";
                                    if (((JObject)jo["ActualCost"])["AmountTaxSum"] != null)
                                    {
                                        sActualCost = ((JObject)jo["ActualCost"])["AmountTaxSum"].ToString();
                                    }
                                    sBillParentId = joBill["parentid"].ToString();
                                    break;
                                }
                            }
                            if (sActualCost != "")
                            {
                                break;
                            }
                        }
                        var jaExhibitors = (JArray)JsonConvert.DeserializeObject(oExport.Exhibitors);
                        foreach (JObject joExhibitor in jaExhibitors)
                        {
                            if (sBillParentId != "" && joExhibitor["guid"].ToString() == sBillParentId)
                            {
                                if (joExhibitor[nameof(sReFlow)] != null)
                                {
                                    if (((JObject)joExhibitor[nameof(sReFlow)])["TransportationMode"] != null)
                                    {
                                        sTransportationMode = ((JObject)joExhibitor[nameof(sReFlow)])["TransportationMode"].ToString();
                                    }
                                }
                                break;
                            }
                        }
                    }
                    else
                    {
                        var joActualCost = (JObject)JsonConvert.DeserializeObject(oExport.ActualCost);
                        ActualCostFeeItems = joActualCost["FeeItems"] != null ? joActualCost["FeeItems"].ToString() : "";
                        if (joActualCost["AmountTaxSum"] != null)
                        {
                            sActualCost = joActualCost["AmountTaxSum"].ToString();
                        }
                        sTransportationMode = oExport.TransportationMode;
                    }
                }
            }
        }

        public static void CalcuCostAndProfitFast(List<dynamic> db, ref string ActualCostFeeItems, ref string sActualCost, ref string sTransportationMode, string _sBillNO, string sId, string sIsRetn, string sReFlow, string sBillType)
        {
            if (sBillType == "OtherBusiness_Upd")
            {
                var oOther = db.Where(t1 => t1.ExFeild1 == sBillType).FirstOrDefault(it => it.Guid == sId);
                if (oOther != null)
                {
                    var joActualCost = (JObject)JsonConvert.DeserializeObject(oOther.ActualCost);
                    ActualCostFeeItems = joActualCost["FeeItems"] != null ? joActualCost["FeeItems"].ToString() : "";
                    if (joActualCost["AmountTaxSum"] != null)
                    {
                        sActualCost = joActualCost["AmountTaxSum"].ToString();
                    }
                }
            }
            else if (sBillType == "ExhibitionImport_Upd")
            {
                var oImport = db.Where(t1 => t1.ExFeild1 == sBillType).FirstOrDefault(it => it.ImportBillNO == sId);
                if (oImport != null)
                {
                    if (sIsRetn == "Y")
                    {
                        var jaReturnBills = (JArray)JsonConvert.DeserializeObject(oImport.ReturnBills);
                        for (var idx = 0; idx < jaReturnBills.Count; idx++)
                        {
                            if (sReFlow == (idx + 1).ToString())
                            {
                                var joReturn = (JObject)jaReturnBills[idx];
                                ActualCostFeeItems = ((JObject)joReturn["ActualCost"])["FeeItems"] != null ? ((JObject)joReturn["ActualCost"])["FeeItems"].ToString() : "";
                                if (((JObject)joReturn["ActualCost"])["AmountTaxSum"] != null)
                                {
                                    sActualCost = ((JObject)joReturn["ActualCost"])["AmountTaxSum"].ToString();
                                }
                                break;
                            }
                        }
                        var jaReImports = (JArray)JsonConvert.DeserializeObject(oImport.ReImports);
                        for (var idx = 0; idx < jaReImports.Count; idx++)
                        {
                            if (sReFlow == idx.ToString())
                            {
                                var joReturn = (JObject)jaReImports[idx];
                                sTransportationMode = ((JObject)joReturn["ReImportData"])["TransportationMode"].ToString();
                                break;
                            }
                        }
                    }
                    else
                    {

                        var joActualCost = (JObject)JsonConvert.DeserializeObject(oImport.ActualCost);
                        ActualCostFeeItems = joActualCost["FeeItems"] != null ? joActualCost["FeeItems"].ToString() : "";
                        if (joActualCost["AmountTaxSum"] != null)
                        {
                            sActualCost = joActualCost["AmountTaxSum"].ToString();
                        }
                        sTransportationMode = oImport.TransportationMode;
                    }
                }
            }
            else if (sBillType == "ExhibitionExport_Upd")
            {
                var oExport = db.Where(t1 => t1.ExFeild1 == sBillType).FirstOrDefault(it => it.ExportBillNO == sId);
                if (oExport != null)
                {
                    if (sIsRetn == "Y")
                    {
                        var jaReturns = (JArray)JsonConvert.DeserializeObject(oExport.ReturnBills);
                        var sBillParentId = "";
                        foreach (JObject jo in jaReturns)
                        {
                            var jaReturnBills = (JArray)jo["Bills"];
                            foreach (JObject joBill in jaReturnBills)
                            {
                                if (joBill["BillNO"].ToString() == _sBillNO)
                                {
                                    ActualCostFeeItems = ((JObject)jo["ActualCost"])["FeeItems"] != null ? ((JObject)jo["ActualCost"])["FeeItems"].ToString() : "";
                                    if (((JObject)jo["ActualCost"])["AmountTaxSum"] != null)
                                    {
                                        sActualCost = ((JObject)jo["ActualCost"])["AmountTaxSum"].ToString();
                                    }
                                    sBillParentId = joBill["parentid"].ToString();
                                    break;
                                }
                            }
                            if (sActualCost != "")
                            {
                                break;
                            }
                        }
                        var jaExhibitors = (JArray)JsonConvert.DeserializeObject(oExport.Exhibitors);
                        foreach (JObject joExhibitor in jaExhibitors)
                        {
                            if (sBillParentId != "" && joExhibitor["guid"].ToString() == sBillParentId)
                            {
                                if (joExhibitor[nameof(sReFlow)] != null)
                                {
                                    if (((JObject)joExhibitor[nameof(sReFlow)])["TransportationMode"] != null)
                                    {
                                        sTransportationMode = ((JObject)joExhibitor[nameof(sReFlow)])["TransportationMode"].ToString();
                                    }
                                }
                                break;
                            }
                        }
                    }
                    else
                    {
                        var joActualCost = (JObject)JsonConvert.DeserializeObject(oExport.ActualCost);
                        ActualCostFeeItems = joActualCost["FeeItems"] != null ? joActualCost["FeeItems"].ToString() : "";
                        if (joActualCost["AmountTaxSum"] != null)
                        {
                            sActualCost = joActualCost["AmountTaxSum"].ToString();
                        }
                        sTransportationMode = oExport.TransportationMode;
                    }
                }
            }
        }

        #region 取得部門資料
        public static Dictionary<string, Tuple<string, string, string, string, string>> GetDeptInfos(SqlSugarClient db, string OrgID = "TE")
        {
            var DeptDic = new Dictionary<string, Tuple<string, string, string, string, string>>();
            var spOrgID = new SugarParameter("@OrgID", OrgID);
            var spDeptID = new SugarParameter("@DeptID", "");
            var DataJArrays = db.Ado.SqlQueryDynamic(@"WITH SupperDeptID(OrgID,DepartmentID,DepartmentName,ParentDepartmentID,ParentDepartmentName,Level) 
                        as(
                        	select OrgID,DepartmentID,DepartmentName,ParentDepartmentID, DepartmentName as ParentDepartmentName, 0 as Level from OTB_SYS_Departments where Len(ParentDepartmentID) =0   
                        	UNION ALL
                        	Select sub.OrgID, sub.DepartmentID, sub.DepartmentName, sub.ParentDepartmentID, Supper.DepartmentName as ParentDepartmentName, Supper.Level +1 as Level
                        	from OTB_SYS_Departments Sub,SupperDeptID Supper
                        	WHere Sub.ParentDepartmentID = Supper.DepartmentID and sub.OrgID = Supper.OrgID 
                        )
                        Select *from SupperDeptID where OrgID = 'TE' Order By OrgID, Level", spOrgID, spDeptID);
            foreach (var row in DataJArrays)
            {

                var DepartmentID = row["DepartmentID"].ToString() ?? "";
                var DepartmentName = row["DepartmentName"].ToString() ?? "";
                var ParentDepartmentID = row["ParentDepartmentID"].ToString() ?? "";
                var ParentDepartmentName = row["ParentDepartmentName"].ToString() ?? "";
                var Level = row["Level"].ToString() ?? "";
                var DepInfo = new Tuple<string, string, string, string, string>(DepartmentID, DepartmentName, ParentDepartmentID, ParentDepartmentName, Level);
                DeptDic.Add(DepartmentID, DepInfo);
            }
            return DeptDic;
        }

        //public static string GetRelativeMember(SqlSugarClient db, string OrgID, string DeptID, string Member)
        //{
        //    var Result = "";
        //    if (string.IsNullOrEmpty(Member) && !string.IsNullOrEmpty(DeptID))
        //    {
        //        var spOrgID = new SugarParameter("@OrgID", OrgID);
        //        var spDepartID = new SugarParameter("@DepartID", DeptID);

        //        var AllOfDepartMembers = db.Ado.SqlQuery<string>(@"select MemberID  from [dbo].[OVW_SYS_Members] 
        //                where OrgID = @OrgID and DepartmentID in 
        //                ( SELECT * FROM [dbo].[OFN_SYS_GetChilDepartmentIdByDepartmentId] (@OrgID, @DepartID) ) ", 
        //                spOrgID, spDepartID).ToArray();

        //        Result = string.Join(",", AllOfDepartMembers);
        //    }
        //    return Result;
        //}
        #endregion

        public static decimal Rounding(decimal value, int digit)
        {
            if (digit <= 0)
                return Math.Round(value, MidpointRounding.AwayFromZero);
            else
                return Math.Round(value, digit, MidpointRounding.AwayFromZero);
        }
        public static decimal Rounding(double value, int digit)
        {
            double result = 0.0;
            if (digit <= 0)
                result = Math.Round(value, MidpointRounding.AwayFromZero);
            else
                result = Math.Round(value, digit, MidpointRounding.AwayFromZero);
            return Convert.ToDecimal(result);
        }

    }
}