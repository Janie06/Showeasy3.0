using EasyBL.WebApi.Message;
using Entity.Sugar;
using Entity.ViewModels;
using Microsoft.Office.Interop.Excel;
using Aspose.Cells;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SqlSugar;
using SqlSugar.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Drawing;
using EasyBL;

namespace EasyBL.WEBAPP.OPM
{
    public class BillsReportService : ServiceBase
    {
        #region 賬單利潤報表

        /// <summary>
        /// 賬單利潤報表
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage Report(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var sOutPut = Common.ConfigGetValue(@"", @"OutFilesPath");
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var sProjectNO = _fetchString(i_crm, @"ProjectNO");
                    var sPayer = _fetchString(i_crm, @"Payer");
                    var sResponsiblePerson = _fetchString(i_crm, @"ResponsiblePerson");
                    //var sBillNO = _fetchString(i_crm, @"BillNO");
                    var sBillNODateStart = _fetchString(i_crm, @"BillNODateStart");
                    var sBillNODateEnd = _fetchString(i_crm, @"BillNODateEnd");
                    var sOrderBy = _fetchString(i_crm, @"OrderBy");
                    var sFlag = _fetchString(i_crm, @"Flag");

                    var rBillNODateStart = new DateTime();
                    var rBillNODateEnd = new DateTime();
                    if (!string.IsNullOrEmpty(sBillNODateStart))
                    {
                        rBillNODateStart = SqlFunc.ToDate(sBillNODateStart);
                    }
                    if (!string.IsNullOrEmpty(sBillNODateEnd))
                    {
                        rBillNODateEnd = SqlFunc.ToDate(sBillNODateEnd).AddDays(1);
                    }

                    var view = db.Queryable<OVW_OPM_Bills, OTB_OPM_BillInfo>
                        ((t1, t2) =>
                        new object[] {
                                JoinType.Inner, t1.OrgID == t2.OrgID && t1.BillNO == t2.BillNO
                              }
                        )
                         .Where((t1, t2) => t1.OrgID == i_crm.ORIGID)
                         .WhereIF(!string.IsNullOrEmpty(sProjectNO), (t1) => t1.ProjectNumber.Contains(sProjectNO))
                         .WhereIF(!string.IsNullOrEmpty(sPayer), (t1) => t1.CustomerCode.Contains(sPayer))
                         .WhereIF(!string.IsNullOrEmpty(sResponsiblePerson), (t1) => t1.ResponsiblePersonFullCode.Contains(sResponsiblePerson))
                         //.WhereIF(!string.IsNullOrEmpty(sBillNO), (t1) => t1.BillNO.Contains(sBillNO))
                         .WhereIF(!string.IsNullOrEmpty(sBillNODateStart), (t1) => SqlFunc.ToDate(t1.CreateDate) >= rBillNODateStart.Date)
                         .WhereIF(!string.IsNullOrEmpty(sBillNODateEnd), (t1) => SqlFunc.ToDate(t1.CreateDate) <= rBillNODateEnd.Date);

                    var saBills = view.Select((t1, t2) =>
                                       new View_OPM_BillIReport
                                       {
                                           BillNO = t2.BillNO,
                                           BillType = t2.BillType,
                                           ParentId = t2.ParentId,
                                           ProjectNumber = t1.ProjectNumber,
                                           CustomerCode = t1.CustomerCode,
                                           ResponsiblePerson = t1.ResponsiblePersonFullCode,
                                           Currency = t2.Currency,
                                           ExchangeRate = t2.ExchangeRate,
                                           InCome = t1.TotalReceivable,
                                       })
                                      .MergeTable()
                                      .OrderBy(sOrderBy, "asc")
                                      .ToList();
                    var sProjectNumbers = view.Select((t1) => "," + t1.ProjectNumber + ",").ToJson();
                    var sCustomerCodes = view.Select((t1) => "," + t1.CustomerCode + ",").ToJson();
                    var sParentIds = view.Select((t1, t2) => "," + t2.ParentId + ",").ToJson();

                    var oProjectNumbers = db.Queryable<OTB_OPM_Exhibition>()
                                            .Where(x => x.OrgID == i_crm.ORIGID && sProjectNumbers.Contains(x.ExhibitionCode) && x.Effective == "Y")
                                            .Select<KeyValuePair<string, string>>("ExhibitionCode,ExhibitioShotName_TW")
                                            .ToList();
                    var oCustomers = db.Queryable<OTB_CRM_CustomersMST, OTB_CRM_Customers>
                                            ((t1, t2) =>
                                                new object[] {
                                                    JoinType.Inner, t1.OrgID == t2.OrgID && t1.customer_guid == t2.guid
                                                }
                                            )
                                            .Where((t1, t2) => t1.OrgID == i_crm.ORIGID && sCustomerCodes.Contains(t1.CustomerNO) && t2.Effective == "Y")
                                            .Select<KeyValuePair<string, string>>("t1.CustomerNO,t2.CustomerShotCName")
                                            .ToList();

                    var viewExpIm = db.Queryable<OTB_OPM_ImportExhibition>()
                                            .Where(x => x.OrgID == i_crm.ORIGID && sParentIds.Contains(x.ImportBillNO))
                                            .Select(x => new ExpInfo
                                            {
                                                Id = x.ImportBillNO,
                                                ActualCost = x.ActualCost,
                                                ReturnBills = x.ReturnBills,
                                                RefNumber = x.RefNumber,
                                                ExpNO = x.ExhibitionNO
                                            });
                    var viewExpEx = db.Queryable<OTB_OPM_ExportExhibition>()
                                            .Where(x => x.OrgID == i_crm.ORIGID && sParentIds.Contains(x.ExportBillNO))
                                            .Select(x => new ExpInfo
                                            {
                                                Id = x.ExportBillNO,
                                                ActualCost = x.ActualCost,
                                                ReturnBills = x.ReturnBills,
                                                RefNumber = x.RefNumber,
                                                ExpNO = x.ExhibitionNO
                                            });
                    var viewExpOth = db.Queryable<OTB_OPM_OtherExhibition>()
                                            .Where(x => x.OrgID == i_crm.ORIGID && sParentIds.Contains(x.Guid))
                                            .Select(x => new ExpInfo
                                            {
                                                Id = x.Guid,
                                                ActualCost = x.ActualCost,
                                                ReturnBills = "",
                                                RefNumber = "",
                                                ExpNO = x.ExhibitionNO
                                            });
                    var viewExpOthtG = db.Queryable<OTB_OPM_OtherExhibitionTG>()
                                            .Where(x => x.OrgID == i_crm.ORIGID && sParentIds.Contains(x.Guid))
                                            .Select(x => new ExpInfo
                                            {
                                                Id = x.Guid,
                                                ActualCost = x.ActualCost,
                                                ReturnBills = "",
                                                RefNumber = "",
                                                ExpNO = x.ExhibitionNO
                                            });

                    var saExps = db.UnionAll(viewExpIm, viewExpEx, viewExpOth, viewExpOthtG).ToList();

                    var sProjectName = "";
                    var sCustomerName = "";
                    if (!string.IsNullOrEmpty(sProjectNO))
                    {
                        sProjectName = db.Queryable<OTB_OPM_Exhibition>().Single(x => x.OrgID == i_crm.ORIGID && x.ExhibitionCode == sProjectNO).ExhibitioShotName_TW;
                    }
                    if (!string.IsNullOrEmpty(sPayer))
                    {
                        sCustomerName = db.Queryable<OTB_CRM_CustomersMST, OTB_CRM_Customers>
                                            ((t1, t2) =>
                                                new object[] {
                                                    JoinType.Inner, t1.OrgID == t2.OrgID && t1.customer_guid == t2.guid
                                                }
                                            )
                                            .Select((t1, t2) => new OTB_CRM_Customers()
                                            {
                                                CustomerNO = t1.CustomerNO,
                                                CustomerShotCName = t2.CustomerShotCName,
                                                CustomerShotEName = t2.CustomerShotEName
                                            })
                                            .Single((t1) => t1.OrgID == i_crm.ORIGID && t1.CustomerNO == sPayer)
                                            .CustomerShotCName;
                    }

                    var oTempl = db.Queryable<OTB_SYS_OfficeTemplate>().Single(it => it.OrgID == i_crm.ORIGID && it.TemplID == "BillReport");
                    if (oTempl == null)
                    {
                        sMsg = @"請檢查模版設定";
                        break;
                    }

                    var oFile = db.Queryable<OTB_SYS_Files>().Single(it => it.OrgID == i_crm.ORIGID && it.ParentID == oTempl.FileID);
                    if (oFile == null)
                    {
                        sMsg = @"系統找不到對應的報表模版";
                        break;
                    }

                    var sTempPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, oFile.FilePath);//Word模版路徑
                    var sBase = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"");
                    sOutPut = sBase + sOutPut;
                    Common.FnCreateDir(sOutPut);//如果不存在就創建文件夾
                    var sFileName = "利潤報表" + DateTime.Now.ToString(@"yyyy-MM-dd");
                    //建立臨時文件
                    var sTempFile = Path.GetTempPath() + sFileName + @".xlsx";
                    sOutPut += sFileName + @".xlsx";
                    if (File.Exists(sTempFile))
                    {
                        File.Delete(sTempFile);
                    }
                    File.Copy(sTempPath, sTempFile);
                    var excelApp = new ExcelEdit();
                    excelApp.Open(sTempFile);
                    excelApp.app.Visible = false;
                    excelApp.app.DisplayAlerts = false;
                    excelApp.ws = excelApp.wb.Worksheets[1];
                    var wsName = excelApp.ws.Name;

                    try
                    {
                        var iCurrRow = 5;
                        var iInCome_Total = 0;
                        var iCost_Total = 0;
                        var iProfit_Total = 0;
                        //展覽
                        excelApp.SetCellValue(wsName, 2, 3, sProjectName);
                        //客戶
                        excelApp.SetCellValue(wsName, 2, 8, sCustomerName);
                        //業務
                        excelApp.SetCellValue(wsName, 2, 15, sResponsiblePerson ?? "");
                        //賬單時間
                        excelApp.SetCellValue(wsName, 3, 3, sBillNODateStart + "~" + sBillNODateEnd);
                        //列表人
                        excelApp.SetCellValue(wsName, 3, 15, i_crm.USERID);
                        //列印時間
                        excelApp.SetCellValue(wsName, 3, 18, DateTime.Now.ToString(@"yyyy/MM/dd"));

                        Microsoft.Office.Interop.Excel.Range range = excelApp.ws.Range[excelApp.ws.Cells[5, 1], excelApp.ws.Cells[5, 19]];
                        Microsoft.Office.Interop.Excel.Range range_SubTotal = excelApp.ws.Range[excelApp.ws.Cells[6, 1], excelApp.ws.Cells[6, 19]];

                        var group = saBills.GroupBy(p => sOrderBy == "ProjectNumber" ? p.ProjectNumber : sOrderBy == "CustomerCode" ? p.CustomerCode : p.ResponsiblePerson);
                        foreach (IGrouping<string, View_OPM_BillIReport> bills in group)
                        {
                            var iInCome_Sub = 0;
                            var iCost_Sub = 0;
                            var iProfit_Sub = 0;
                            var _bills = bills.OrderBy(x => x.ProjectNumber).OrderBy(x => x.ParentId);
                            foreach (View_OPM_BillIReport bill in _bills)
                            {
                                var iInCome = bill.InCome * decimal.Parse(bill.ExchangeRate == "" ? "0" : bill.ExchangeRate);
                                var iCost = 0;
                                var iProfit = iInCome - iCost;
                                iInCome_Sub += int.Parse($@"{iInCome:N0}".Replace(",", ""));
                                iCost_Sub += int.Parse($@"{iCost:N0}".Replace(",", ""));
                                iProfit_Sub += int.Parse($@"{iProfit:N0}".Replace(",", ""));
                                var _projectName = oProjectNumbers.Any(x => x.Key == bill.ProjectNumber) ? oProjectNumbers.Single(x => x.Key == bill.ProjectNumber).Value : "";
                                var _customerName = oCustomers.Any(x => x.Key == bill.CustomerCode) ? oCustomers.Single(x => x.Key == bill.CustomerCode).Value : "";
                                excelApp.SetCellValue(wsName, iCurrRow, 2, bill.BillNO);
                                excelApp.SetCellValue(wsName, iCurrRow, 4, _projectName);
                                excelApp.SetCellValue(wsName, iCurrRow, 7, _customerName);
                                excelApp.SetCellValue(wsName, iCurrRow, 10, bill.ResponsiblePerson);
                                excelApp.SetCellValue(wsName, iCurrRow, 11, $@"{iInCome:N0}");
                                excelApp.SetCellValue(wsName, iCurrRow, 14, iCost == 0 ? "" : $@"{iCost:N0}");
                                excelApp.SetCellValue(wsName, iCurrRow, 17, $@"{iProfit:N0}");
                                //複製并添加行
                                range.Copy();
                                range.Insert(XlDirection.xlDown);
                                iCurrRow++;
                            }
                            if (sOrderBy == "ProjectNumber" || sOrderBy == "ResponsiblePerson")
                            {
                                var group_exp = bills.GroupBy(p => p.ParentId);
                                foreach (IGrouping<string, View_OPM_BillIReport> bill_exp in group_exp)
                                {
                                    var exp = saExps.Single(x => x.Id == bill_exp.Key);
                                    var exp_group = bill_exp.First();
                                    var iInCome_Cost = 0;
                                    var iCost_Cost = 0;

                                    var cost = (JObject)JsonConvert.DeserializeObject(exp.ActualCost);
                                    var iAmountTaxSum = double.Parse((cost.GetValue("AmountTaxSum") ?? "0").ToString().Replace(",", ""));
                                    iAmountTaxSum = ExhibitionHelper.Round(iAmountTaxSum, 0);
                                    iCost_Cost += int.Parse(iAmountTaxSum.ToString());
                                    if (exp_group.BillType == "ExhibitionImport_Upd" || exp_group.BillType == "ExhibitionExport_Upd")
                                    {
                                        var cost_rtns = (JArray)JsonConvert.DeserializeObject(exp.ReturnBills);
                                        foreach (var _cost_rtn in cost_rtns)
                                        {
                                            var _cost = (JObject)((JObject)_cost_rtn).GetValue("ActualCost");
                                            var iAmountTaxSum_ = double.Parse((_cost.GetValue("AmountTaxSum") ?? "0").ToString().Replace(",", ""));
                                            iAmountTaxSum_ = ExhibitionHelper.Round(iAmountTaxSum_, 0);
                                            iCost_Cost += int.Parse(iAmountTaxSum_.ToString());
                                        }
                                    }

                                    var iProfit_Cost = iInCome_Cost - iCost_Cost;
                                    iInCome_Sub += 0;
                                    iCost_Sub += iCost_Cost;
                                    iProfit_Sub += iProfit_Cost;
                                    if (iCost_Cost != 0)
                                    {
                                        var _projectName = oProjectNumbers.Any(x => x.Key == exp_group.ProjectNumber) ? oProjectNumbers.Single(x => x.Key == exp_group.ProjectNumber).Value : "";
                                        excelApp.SetCellValue(wsName, iCurrRow, 2, exp.RefNumber);
                                        excelApp.SetCellValue(wsName, iCurrRow, 4, _projectName);
                                        excelApp.SetCellValue(wsName, iCurrRow, 7, "");
                                        excelApp.SetCellValue(wsName, iCurrRow, 10, exp_group.ResponsiblePerson);
                                        excelApp.SetCellValue(wsName, iCurrRow, 11, "");
                                        excelApp.SetCellValue(wsName, iCurrRow, 14, $@"{iCost_Cost:N0}");
                                        excelApp.SetCellValue(wsName, iCurrRow, 17, $@"{iProfit_Cost:N0}");
                                        //複製并添加行
                                        range.Copy();
                                        range.Insert(XlDirection.xlDown);
                                        iCurrRow++;
                                    }
                                }
                            }

                            //添加一個空行
                            range.Insert(XlDirection.xlDown);
                            Microsoft.Office.Interop.Excel.Range range_prev = excelApp.ws.Range[excelApp.ws.Cells[iCurrRow, 1], excelApp.ws.Cells[iCurrRow, 19]];
                            //複製小計行
                            range_SubTotal.Copy(range_prev);
                            excelApp.SetCellValue(wsName, iCurrRow, 11, iInCome_Sub == 0 ? "" : $@"{iInCome_Sub:N0}");
                            excelApp.SetCellValue(wsName, iCurrRow, 14, iCost_Sub == 0 ? "" : $@"{iCost_Sub:N0}");
                            excelApp.SetCellValue(wsName, iCurrRow, 17, iProfit_Sub == 0 ? "" : $@"{iProfit_Sub:N0}");
                            iCurrRow++;
                            iInCome_Total += iInCome_Sub;
                            iCost_Total += iCost_Sub;
                            iProfit_Total += iProfit_Sub;
                        }

                        if (sOrderBy == "CustomerCode")
                        {
                            var group_exp = saBills.GroupBy(p => p.ParentId);
                            var iInCome_Sub = 0;
                            var iCost_Sub = 0;
                            var iProfit_Sub = 0;
                            foreach (IGrouping<string, View_OPM_BillIReport> bill_exp in group_exp)
                            {
                                var exp = saExps.Single(x => x.Id == bill_exp.Key);
                                var exp_group = bill_exp.First();
                                var iInCome_Cost = 0;
                                var iCost_Cost = 0;

                                var cost = (JObject)JsonConvert.DeserializeObject(exp.ActualCost);
                                var iAmountTaxSum = double.Parse((cost.GetValue("AmountTaxSum") ?? "0").ToString().Replace(",", ""));
                                iAmountTaxSum = ExhibitionHelper.Round(iAmountTaxSum, 0);
                                iCost_Cost += int.Parse(iAmountTaxSum.ToString());
                                if (exp_group.BillType == "ExhibitionImport_Upd" || exp_group.BillType == "ExhibitionExport_Upd")
                                {
                                    var cost_rtns = (JArray)JsonConvert.DeserializeObject(exp.ReturnBills);
                                    foreach (var _cost_rtn in cost_rtns)
                                    {
                                        var _cost = (JObject)((JObject)_cost_rtn).GetValue("ActualCost");
                                        var iAmountTaxSum_ = double.Parse((_cost.GetValue("AmountTaxSum") ?? "0").ToString().Replace(",", ""));
                                        iAmountTaxSum_ = ExhibitionHelper.Round(iAmountTaxSum_, 0);
                                        iCost_Cost += int.Parse(iAmountTaxSum_.ToString());
                                    }
                                }

                                var iProfit_Cost = iInCome_Cost - iCost_Cost;
                                iInCome_Sub += 0;
                                iCost_Sub += iCost_Cost;
                                iProfit_Sub += iProfit_Cost;
                                if (iCost_Cost != 0)
                                {
                                    var _projectName = oProjectNumbers.Any(x => x.Key == exp_group.ProjectNumber) ? oProjectNumbers.Single(x => x.Key == exp_group.ProjectNumber).Value : "";
                                    excelApp.SetCellValue(wsName, iCurrRow, 2, exp.RefNumber);
                                    excelApp.SetCellValue(wsName, iCurrRow, 4, _projectName);
                                    excelApp.SetCellValue(wsName, iCurrRow, 7, "");
                                    excelApp.SetCellValue(wsName, iCurrRow, 10, exp_group.ResponsiblePerson);
                                    excelApp.SetCellValue(wsName, iCurrRow, 11, "");
                                    excelApp.SetCellValue(wsName, iCurrRow, 14, $@"{iCost_Cost:N0}");
                                    excelApp.SetCellValue(wsName, iCurrRow, 17, $@"{iProfit_Cost:N0}");
                                    //複製并添加行
                                    range.Copy();
                                    range.Insert(XlDirection.xlDown);
                                    iCurrRow++;
                                }
                            }
                            //添加一個空行
                            range.Insert(XlDirection.xlDown);
                            Microsoft.Office.Interop.Excel.Range range_prev = excelApp.ws.Range[excelApp.ws.Cells[iCurrRow, 1], excelApp.ws.Cells[iCurrRow, 19]];
                            //複製小計行
                            range_SubTotal.Copy(range_prev);
                            excelApp.SetCellValue(wsName, iCurrRow, 11, iInCome_Sub == 0 ? "" : $@"{iInCome_Sub:N0}");
                            excelApp.SetCellValue(wsName, iCurrRow, 14, iCost_Sub == 0 ? "" : $@"{iCost_Sub:N0}");
                            excelApp.SetCellValue(wsName, iCurrRow, 17, iProfit_Sub == 0 ? "" : $@"{iProfit_Sub:N0}");
                            iCurrRow++;
                            iInCome_Total += iInCome_Sub;
                            iCost_Total += iCost_Sub;
                            iProfit_Total += iProfit_Sub;
                        }

                        range.Delete();
                        //range_SubTotal.Delete();
                        //總計
                        excelApp.SetCellValue(wsName, 3, 8, iProfit_Total == 0 ? "" : $@"{iProfit_Total:N0}");
                        if (iProfit_Total != 0)
                        {
                            excelApp.SetCellValue(wsName, iCurrRow, 10, "總計：");
                            excelApp.SetCellValue(wsName, iCurrRow, 11, iInCome_Total == 0 ? "" : $@"{iInCome_Total:N0}");
                            excelApp.SetCellValue(wsName, iCurrRow, 14, iCost_Total == 0 ? "" : $@"{iCost_Total:N0}");
                            excelApp.SetCellValue(wsName, iCurrRow, 17, iProfit_Total == 0 ? "" : $@"{iProfit_Total:N0}");
                        }
                        excelApp.Save();

                        if (File.Exists(sOutPut))
                        {
                            File.Delete(sOutPut);
                        }

                        if (sFlag == @"pdf")
                        {
                            sOutPut = sOutPut.Replace(@".xlsx", @".pdf").Replace(@".xls", @".pdf");
                            excelApp.SaveAsPdf(sOutPut);
                        }
                        else
                        {
                            File.Copy(sTempFile, sOutPut);
                        }
                        excelApp.Close();
                        File.Delete(sTempFile);   //刪除臨時文件
                        sOutPut = sOutPut.Replace(sBase, @"");
                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, sOutPut);
                    }
                    catch (Exception ex)
                    {
                        rm = new SuccessResponseMessage(null, i_crm);
                        sMsg = ex.Message;
                        excelApp.Close();
                        throw new Exception(ex.Message, ex);
                    }
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(BillsReportService), "", "Report（賬單利潤報表）", "", "", "");
            }
            finally
            {
                if (null != sMsg)
                {
                    rm = new ErrorResponseMessage(sMsg, i_crm);
                }
            }
            return rm;
        }

        #endregion 賬單利潤報表

        #region 賬單利潤報表

        /// <summary>
        /// 賬單利潤報表
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage ReportPro(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var sOutPut = Common.ConfigGetValue(@"", @"OutFilesPath");
            var db = SugarBase.GetIntance();
            var CurrencyName = CommonRPT.GetCurrencyUnit(i_crm.ORIGID);
            var RoundingPoint = CommonRPT.GetRoundingPoint(i_crm.ORIGID);
            try
            {
                do
                {
                    #region 篩選資料
                    var sResponsibleDeptID = _fetchString(i_crm, @"ResponsibleDeptID");
                    var sResponsiblePerson = _fetchString(i_crm, @"ResponsiblePerson");
                    var sProjectNO = _fetchString(i_crm, @"ProjectNO");
                    var sPayer = _fetchString(i_crm, @"Payer");
                    //var sBillNO = _fetchString(i_crm, @"BillNO");
                    var sBillAuditDateStart = _fetchString(i_crm, @"BillAuditDateStart");
                    var sBillAuditDateEnd = _fetchString(i_crm, @"BillAuditDateEnd");
                    var sBillWriteOffDateStart = _fetchString(i_crm, @"BillWriteOffDateStart");
                    var sBillWriteOffDateEnd = _fetchString(i_crm, @"BillWriteOffDateEnd");
                    var sOrderBy = _fetchString(i_crm, @"OrderBy");
                    var sFlag = _fetchString(i_crm, @"Flag");
                    var Filter = new CVPFilter();
                    var ChildDeptIDs = CommonRPT.GetChildDepteList(db, i_crm.ORIGID, sResponsibleDeptID);
                    var MatchedExps = CommonRPT.GetMatchedExps(db, i_crm.ORIGID, ChildDeptIDs, sResponsiblePerson);
                    var MatchedExpGuids = MatchedExps.Select(x => x.Guid).ToList().Distinct().ToArray();
                    Filter.SetBill(sBillAuditDateStart, sBillAuditDateEnd, sBillWriteOffDateStart, sBillWriteOffDateEnd);
                    var view = db.Queryable<OVW_OPM_Bills, OTB_OPM_BillInfo>
                        ((t1, t2) =>
                        new object[] {
                                JoinType.Inner, t1.OrgID == t2.OrgID && t1.BillNO == t2.BillNO
                              }
                        )
                         .Where((t1, t2) => t1.OrgID == i_crm.ORIGID && CommonRPT.PassStatus.Contains(t2.AuditVal))
                         .WhereIF(!string.IsNullOrEmpty(sProjectNO), (t1) => t1.ProjectNumber.Contains(sProjectNO))
                         .WhereIF(!string.IsNullOrEmpty(sPayer), (t1) => t1.CustomerCode.Contains(sPayer))
                         .WhereIF(!string.IsNullOrEmpty(sResponsibleDeptID) || !string.IsNullOrEmpty(sResponsiblePerson),
                            (t1, t2) => SqlFunc.ContainsArray(MatchedExpGuids, t2.ParentId))
                         .WhereIF(!string.IsNullOrEmpty(Filter.sBillAuditDateStart), (t1, t2) => SqlFunc.ToDate(t2.BillFirstCheckDate) >= Filter.rBillAuditDateStart.Date)
                         .WhereIF(!string.IsNullOrEmpty(Filter.sBillAuditDateEnd), (t1, t2) => SqlFunc.ToDate(t2.BillFirstCheckDate) < Filter.rBillAuditDateEnd.Date)
                         .WhereIF(!string.IsNullOrEmpty(Filter.sBillWriteOffDateStart), (t1, t2) => SqlFunc.ToDate(t2.BillWriteOffDate) >= Filter.rBillWriteOffDateStart.Date)
                         .WhereIF(!string.IsNullOrEmpty(Filter.sBillWriteOffDateEnd), (t1, t2) => SqlFunc.ToDate(t2.BillWriteOffDate) < Filter.rBillWriteOffDateEnd.Date);

                    //OVW_OPM_Bills, OTB_OPM_BillInfo
                    var saBills = view.Select((t1, t2) =>
                                       new View_OPM_BillIReport
                                       {
                                           BillNO = t2.BillNO,
                                           BillType = t2.BillType,
                                           ParentId = t2.ParentId,
                                           ProjectNumber = t1.ProjectNumber,
                                           CustomerCode = t1.CustomerCode,
                                           ResponsiblePerson = t1.ResponsiblePersonFullCode,
                                           Currency = t2.Currency,
                                           ExchangeRate = t2.ExchangeRate,
                                           InCome = t1.TWNOTaxAmount, //未稅總計
                                           OrgID = t2.OrgID,
                                           IsReturn = t2.IsRetn,
                                           Volume = t2.Volume,
                                           AuditVal = t2.AuditVal,
                                           ReFlow = t2.ReFlow,
                                           FeeItems = t2.FeeItems
                                       })
                                      .MergeTable()
                                      .OrderBy(sOrderBy, "asc")
                                      .ToList();
                    var sProjectNumbers = view.Select((t1) => "," + t1.ProjectNumber + ",").ToJson();
                    var sCustomerCodes = view.Select((t1) => "," + t1.CustomerCode + ",").ToJson();
                    var sParentIds = view.Select((t1, t2) => "," + t2.ParentId + ",").ToJson();

                    var oProjectNumbers = db.Queryable<OTB_OPM_Exhibition>()
                                            .Where(x => x.OrgID == i_crm.ORIGID && sProjectNumbers.Contains(x.ExhibitionCode) && x.Effective == "Y")
                                            .Select<KeyValuePair<string, string>>("ExhibitionCode,ExhibitioShotName_TW")
                                            .ToList();
                    var oCustomers = db.Queryable<OTB_CRM_CustomersMST, OTB_CRM_Customers>
                                            ((t1, t2) =>
                                                new object[] {
                                                    JoinType.Inner, t1.OrgID == t2.OrgID && t1.customer_guid == t2.guid
                                                }
                                            )
                                            .Where((t1, t2) => t1.OrgID == i_crm.ORIGID && sCustomerCodes.Contains(t1.CustomerNO) && t2.Effective == "Y")
                                            .Select<KeyValuePair<string, string>>("t1.CustomerNO,t2.CustomerShotCName")
                                            .ToList();
                    #endregion

                    //帳單代墊款代碼
                    var saBillPrepayFee = Common.GetSystemSetting(db, i_crm.ORIGID, "PrepayForCustomerCode");
                    var BillPrepayFeeList = saBillPrepayFee.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    //實際代墊款代碼
                    var saActualPrepayFee = Common.GetSystemSetting(db, i_crm.ORIGID, "ActualPrepayForCustomerCode");
                    var ActualPrepayFeeList = saActualPrepayFee.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);


                    var sProjectName = "";
                    var sCustomerName = "";
                    if (!string.IsNullOrEmpty(sProjectNO))
                    {
                        sProjectName = db.Queryable<OTB_OPM_Exhibition>().Single(x => x.OrgID == i_crm.ORIGID && x.ExhibitionCode == sProjectNO).ExhibitioShotName_TW;
                    }
                    if (!string.IsNullOrEmpty(sPayer))
                    {
                        sCustomerName = db.Queryable<OTB_CRM_CustomersMST, OTB_CRM_Customers>
                                            ((t1, t2) =>
                                                new object[] {
                                                    JoinType.Inner, t1.OrgID == t2.OrgID && t1.customer_guid == t2.guid
                                                }
                                            )
                                            .Select((t1, t2) => new OTB_CRM_Customers()
                                            {
                                                CustomerNO = t1.CustomerNO,
                                                CustomerShotCName = t2.CustomerShotCName,
                                                CustomerShotEName = t2.CustomerShotEName
                                            })
                                            .Single((t1) => t1.OrgID == i_crm.ORIGID && t1.CustomerNO == sPayer)
                                            .CustomerShotCName;
                    }

                    var oTempl = db.Queryable<OTB_SYS_OfficeTemplate>().Single(it => it.OrgID == i_crm.ORIGID && it.TemplID == "BillReportPro");
                    if (oTempl == null)
                    {
                        sMsg = @"請檢查模版設定";
                        break;
                    }

                    var oFile = db.Queryable<OTB_SYS_Files>().Single(it => it.OrgID == i_crm.ORIGID && it.ParentID == oTempl.FileID);
                    if (oFile == null)
                    {
                        sMsg = @"系統找不到對應的報表模版";
                        break;
                    }

                    var sTempPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, oFile.FilePath);//Word模版路徑
                    var sBase = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"");
                    sOutPut = sBase + sOutPut;
                    Common.FnCreateDir(sOutPut);//如果不存在就創建文件夾
                    var sFileName = "利潤報表" + DateTime.Now.ToString(@"yyyy-MM-dd") + "_" + Guid.NewGuid();
                    //建立臨時文件
                    var sTempFile = Path.GetTempPath() + sFileName + @".xlsx";
                    sOutPut += sFileName + @".xlsx";
                    if (File.Exists(sTempFile))
                    {
                        File.Delete(sTempFile);
                    }
                    File.Copy(sTempPath, sTempFile);
                    var cellsApp = new ExcelService(sTempFile);
                    var cells = cellsApp.sheet.Cells;//单元格
                    var iCurrRow = 4;
                    var iStartCurrRow = iCurrRow;
                    cells[1, 1].PutValue(sProjectName);//展覽
                    cells[1, 4].PutValue(sCustomerName);//客戶
                    cells[1, 7].PutValue(string.Join(",", ChildDeptIDs));//部門
                    cells[1, 9].PutValue(i_crm.USERID);//列表人
                    cells[2, 1].PutValue(sBillAuditDateStart + "~" + sBillAuditDateEnd);//賬單時間
                    cells[2, 4].PutValue(sBillWriteOffDateStart + "~" + sBillWriteOffDateEnd);//帳單銷帳區間
                    cells[2, 7].PutValue(sResponsiblePerson ?? "");//業務
                    cells[2, 9].PutValue(DateTime.Now.ToString(@"yyyy/MM/dd"));//列印時間
                    var AllofPorfits = new List<ProfitInfo>();
                    var AllCBMUsage = CommonRPT.GetAllCBMUsages(db, i_crm.ORIGID);
                    var group = saBills.GroupBy(p => sOrderBy == "ProjectNumber" ? p.ProjectNumber : sOrderBy == "CustomerCode" ? p.CustomerCode : p.ResponsiblePerson);
                    foreach (IGrouping<string, View_OPM_BillIReport> bills in group)
                    {
                        var iInCome_Sub = 0;
                        var iCost_Sub = 0;
                        var iProfit_Sub = 0;
                        var _bills = bills.OrderBy(x => x.BillType).ThenBy(x => x.ProjectNumber).ThenBy(x => x.ParentId);
                        var SubProfits = new List<ProfitInfo>();
                        foreach (View_OPM_BillIReport bill in _bills)
                        {
                            var ThisBillCBMUsage = AllCBMUsage.Where(t1 => t1.ParentID == bill.ParentId && t1.IsReturn == bill.IsReturn).ToList();
                            var iInCome = bill.InCome * decimal.Parse(bill.ExchangeRate == "" ? "0" : bill.ExchangeRate);
                            var iCost = 0;
                            var iProfit = iInCome - iCost;
                            iInCome_Sub += int.Parse($@"{iInCome:N0}".Replace(",", ""));
                            iCost_Sub += int.Parse($@"{iCost:N0}".Replace(",", ""));


                            var ActualCostFeeItemJson = "";
                            var sActualCost = "";
                            var sTransportationMode = "";

                            CommonRPT.CalcuCostAndProfit(db, ref ActualCostFeeItemJson, ref sActualCost, ref sTransportationMode, bill.BillNO, bill.ParentId, bill.IsReturn, bill.ReFlow, bill.BillType);
                            var ActualCostFeeItemList = CommonRPT.ToFeeItems(ActualCostFeeItemJson);
                            var BillFeeItemList = CommonRPT.ToFeeItems(bill.FeeItems);
                            var SharedActualCost = CommonRPT.GetShareCost(ActualCostFeeItemList, ThisBillCBMUsage, bill.BillNO);
                            var BillReimburseAmount = BillFeeItemList.Where(c => BillPrepayFeeList.Contains(c.FinancialCode)).Sum(c => c.TWAmount); //帳單內特定費用代碼資料
                            var ActualBillReimburseAmount = CommonRPT.GetShareCost(ActualCostFeeItemList, ThisBillCBMUsage, bill.BillNO, ActualPrepayFeeList);//抓實際成本的資料



                            iProfit_Sub += int.Parse($@"{iProfit:N0}".Replace(",", ""));
                            var _projectName = oProjectNumbers.Any(x => x.Key == bill.ProjectNumber) ? oProjectNumbers.Single(x => x.Key == bill.ProjectNumber).Value : "";
                            var _customerName = oCustomers.Any(x => x.Key == bill.CustomerCode) ? oCustomers.Single(x => x.Key == bill.CustomerCode).Value : "";

                            ProfitInfo profitInfo = new ProfitInfo()
                            {
                                BillNO = bill.BillNO,
                                ExhibitionName = _projectName,
                                CustomerName = _customerName,
                                MemberID = bill.ResponsiblePerson,
                                BillUntaxAmt = CommonRPT.Rounding(iInCome.Value, RoundingPoint),
                                SharedActualCost = CommonRPT.Rounding(SharedActualCost, RoundingPoint),
                                BillReimburseAmount = CommonRPT.Rounding(BillReimburseAmount, RoundingPoint),
                                ActualBillReimburseAmount = CommonRPT.Rounding(ActualBillReimburseAmount, RoundingPoint),
                            };
                            CellsSetValue(cellsApp.workbook, cells, iCurrRow, profitInfo, "");
                            iCurrRow++;
                            SubProfits.Add(profitInfo);
                        }
                        //添加一個小計
                        ProfitInfo SubtotalProfitInfo = new ProfitInfo()
                        {
                            MemberID = "小計：",
                            BillUntaxAmt = CommonRPT.Rounding(SubProfits.Sum(c => c.BillUntaxAmt), RoundingPoint),
                            SharedActualCost = CommonRPT.Rounding(SubProfits.Sum(c => c.SharedActualCost), RoundingPoint),
                            BillReimburseAmount = CommonRPT.Rounding(SubProfits.Sum(c => c.BillReimburseAmount), RoundingPoint),
                            ActualBillReimburseAmount = CommonRPT.Rounding(SubProfits.Sum(c => c.ActualBillReimburseAmount), RoundingPoint)
                        };
                        CellsSetValue(cellsApp.workbook, cells, iCurrRow, SubtotalProfitInfo, "sub");
                        AllofPorfits.Add(SubtotalProfitInfo);
                        iCurrRow++;
                    }

                    //總計
                    if (AllofPorfits.Any())
                    {
                        ProfitInfo TotalProfitInfo = new ProfitInfo()
                        {
                            MemberID = "總計：",
                            BillUntaxAmt = CommonRPT.Rounding(AllofPorfits.Sum(c => c.BillUntaxAmt), RoundingPoint),
                            SharedActualCost = CommonRPT.Rounding(AllofPorfits.Sum(c => c.SharedActualCost), RoundingPoint),
                            BillReimburseAmount = CommonRPT.Rounding(AllofPorfits.Sum(c => c.BillReimburseAmount), RoundingPoint),
                            ActualBillReimburseAmount = CommonRPT.Rounding(AllofPorfits.Sum(c => c.ActualBillReimburseAmount), RoundingPoint)
                        };
                        CellsSetValue(cellsApp.workbook, cells, iCurrRow, TotalProfitInfo, "total");
                    }
                    //cellsApp.sheet.AutoFitColumns();
                    cellsApp.sheet.AutoFitColumn(3, iStartCurrRow, cellsApp.sheet.Cells.Rows.Count);
                    cellsApp.sheet.AutoFitRow(iStartCurrRow, 0, 10);

                    //cellsApp.sheet.AutoFitRows(new AutoFitterOptions() { AutoFitMergedCells = true, IgnoreHidden = true, OnlyAuto = true });

                    if (File.Exists(sOutPut))
                    {
                        File.Delete(sOutPut);
                    }
                    //保存
                    cellsApp.workbook.Save(sOutPut);


                    if (sFlag == @"pdf")
                    {
                        var excelApp = new ExcelEdit();
                        try
                        {
                            excelApp.Open(sOutPut);
                            sOutPut = sOutPut.Replace(@".xlsx", @".pdf").Replace(@".xls", @".pdf");
                            excelApp.SaveAsPdf(sOutPut);
                            excelApp.Close();
                        }
                        catch (Exception ex)
                        {
                            Logger.Error($"Report（賬單利潤報表）ERROR:{sMsg}, ex.StackTrace:{ ex.StackTrace }");
                            excelApp.Close();
                            throw;
                        }
                    }
                    //File.Delete(sTempFile);   //刪除臨時文件
                    sOutPut = sOutPut.Replace(sBase, @"");
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, sOutPut);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(BillsReportService), "", "Report（賬單利潤報表）", "", "", "");
            }
            finally
            {
                if (null != sMsg)
                {
                    rm = new ErrorResponseMessage(sMsg, i_crm);
                }
            }
            return rm;
        }

        #endregion 賬單利潤報表

        #region 專案管理資料

        /// <summary>
        /// 專案管理資料
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage GetProjects(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var saExhibition = db.Queryable<OTB_OPM_Exhibition>()
                                         .Where(x => x.OrgID == i_crm.ORIGID)
                                         .Select(x => new { id = x.ExhibitionCode, text = "（" + x.ExhibitioShotName_TW + "）" + x.Exhibitioname_TW })
                                         .ToList();
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, saExhibition);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(BillsReportService), "", "GetProjects（專案管理資料）", "", "", "");
            }
            finally
            {
                if (null != sMsg)
                {
                    rm = new ErrorResponseMessage(sMsg, i_crm);
                }
            }
            return rm;
        }

        #endregion 專案管理資料

        #region 客戶資料

        /// <summary>
        /// 客戶資料
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage GetPayers(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var saCustomers = db.Queryable<OTB_CRM_Customers>()
                                         .Where(x => x.OrgID == i_crm.ORIGID && x.Effective == "Y")
                                         .Select(x => new { id = x.CustomerNO, text = "（" + x.CustomerShotCName + "）" + SqlFunc.IIF(x.CustomerCName == "", x.CustomerEName, x.CustomerCName) })
                                         .ToList();
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, saCustomers);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(BillsReportService), "", "GetPayers（客戶資料）", "", "", "");
            }
            finally
            {
                if (null != sMsg)
                {
                    rm = new ErrorResponseMessage(sMsg, i_crm);
                }
            }
            return rm;
        }

        #endregion 客戶資料

        private void CellsSetValue(Aspose.Cells.Workbook workbook, Cells cells, int irow, ProfitInfo profitInfo, string flag)
        {
            var style_Text = GetStyle(workbook, 0, false, TextAlignmentType.Left, Color.White, true);
            var style_Number = GetStyle(workbook, 0, false, TextAlignmentType.Right, Color.White, true);
            style_Number.Number = 4;
            var style = GetStyle(workbook, 0, true, TextAlignmentType.Right, Color.White, true);
            cells.SetRowHeight(irow, 20);
            cells[irow, 0].PutValue(profitInfo.BillNO);
            cells[irow, 0].SetStyle(style_Text);
            cells[irow, 1].PutValue(profitInfo.ExhibitionName);
            cells[irow, 1].SetStyle(style_Text);
            cells[irow, 2].PutValue(profitInfo.CustomerName);
            cells[irow, 2].SetStyle(style_Text);
            cells[irow, 3].PutValue(profitInfo.MemberID);
            cells[irow, 3].SetStyle(style_Text);
            cells[irow, 4].PutValue(profitInfo.BillUntaxAmt);
            cells[irow, 4].SetStyle(style_Number);
            cells[irow, 5].PutValue(profitInfo.SharedActualCost);
            cells[irow, 5].SetStyle(style_Number);
            cells[irow, 6].PutValue(profitInfo.GrossProfit);
            cells[irow, 6].SetStyle(style_Number);
            cells[irow, 7].PutValue(profitInfo.BillReimburseAmount);
            cells[irow, 7].SetStyle(style_Number);
            cells[irow, 8].PutValue(profitInfo.ActualBillReimburseAmount);
            cells[irow, 8].SetStyle(style_Number);
            cells[irow, 9].PutValue(profitInfo.NetProfit);
            cells[irow, 9].SetStyle(style_Number);
            //cells.Merge(irow, 1, 1, 2);//合并单元格(賬單號碼)
            ////依序:展覽簡稱、客戶簡稱、業務員、未稅金額、實際成本、毛利、帳單代墊款、實際代墊款、淨毛利
            //var ColumnIndex = new int[] {  3, 6, 10, 13, 16, 19, 22, 25 };
            //foreach (var ci in ColumnIndex)
            //{
            //    cells.Merge(irow, ci, 1, 3);//合并单元格(展覽簡稱)
            //}
            //cells.SetRowHeight(irow, 20);
            //var MaxColumnIndex = 1;//ColumnIndex.Last() + 3;
            //if (flag == "sub" || flag == "total")
            //{
            //    for (var index = 9; index < MaxColumnIndex; index++)
            //    {
            //        cells[irow, index].SetStyle(style);
            //    }
            //}
            //else
            //{
            //    for (var index = 1; index < MaxColumnIndex; index++)
            //    {
            //        if (index < 10)
            //        {
            //            cells[irow, index].SetStyle(style_L);//居左
            //        }
            //        else
            //        {
            //            cells[irow, index].SetStyle(style_R);//居右(INCOME)
            //        }
            //    }
            //}
        }

        /// <summary>
        /// 固定的樣式
        /// </summary>
        /// <param name="sFontSize"></param>
        /// <param name="bIsBold"></param>
        /// <param name="sAlign"></param>
        /// <param name="sBgColor"></param>
        /// <param name="bIsWrap"></param>
        /// <param name="workbook">todo: describe workbook parameter on GetStyle</param>
        /// <returns></returns>
        public static Aspose.Cells.Style GetStyle(Aspose.Cells.Workbook workbook, int sFontSize, bool bIsBold, TextAlignmentType sAlign, Color sBgColor, bool bIsWrap)
        {
            var style = workbook.CreateStyle();
            style.HorizontalAlignment = sAlign;//文字居左/中/右 ---TextAlignmentType.Center
            //style.Font.Color = Color.Blue;
            if (sFontSize != 0)
            {
                style.Font.Size = sFontSize;//文字大小 ----12
            }
            style.Font.IsBold = bIsBold;//粗体 ----false
            style.ForegroundColor = sBgColor;//背景顏色
            style.Pattern = BackgroundType.Solid;//设置背景類型
            style.IsTextWrapped = bIsWrap;//单元格内容自动换行

            // 邊線設置
            style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;
            return style;
        }

        public OVW_OPM_BillInfo ToBillInfo(View_OPM_BillIReport billIReport)
        {
            return new OVW_OPM_BillInfo()
            {
                OrgID = billIReport.OrgID,
                ParentId = billIReport.ParentId,
                IsRetn = billIReport.IsReturn,
                AuditVal = billIReport.AuditVal,
                Volume = billIReport.Volume,
            };
        }
    }
}