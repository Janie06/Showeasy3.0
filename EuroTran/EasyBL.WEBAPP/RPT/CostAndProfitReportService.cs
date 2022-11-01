using Aspose.Cells;
using EasyBL.WebApi.Message;
using EasyNet.Common;
using Entity.Sugar;
using Entity.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SqlSugar;
using SqlSugar.Base;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Diagnostics;

namespace EasyBL.WEBAPP.RPT
{
    public class CostAndProfitReportService : ServiceBase
    {
        private bool ShowSource = false;
        private static int TimeOut = 120;
        #region 利潤明細表(依業務)

        public ResponseMessage CVPAnalysisBySaler(RequestMessage i_crm)
        {
            //帳單(拋轉)區間、銷帳區間、部門、業務
            ShowSource = CommonRPT.RPTShow();
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance(commandTimeOut: TimeOut);
            var FunctionName = nameof(CVPAnalysisBySaler);
            var ExcutePath = CommonRPT.GetExcutePath(FunctionName, "利潤明細表(依業務)");
            var sOutPut = ExcutePath.Item1;
            var sTempFile = ExcutePath.Item2;
            var sBase = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"");
            var CurrencyUnit = CommonRPT.GetCurrencyUnit(i_crm.ORIGID);
            var RoundingPoint = CommonRPT.GetRoundingPoint(i_crm.ORIGID);
            try
            {
                do
                {
                    var saBillPrepayFee = Common.GetSystemSetting(db, i_crm.ORIGID, "PrepayForCustomerCode");
                    var BillPrepayFeeList = saBillPrepayFee.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    var saActualPrepayFee = Common.GetSystemSetting(db, i_crm.ORIGID, "ActualPrepayForCustomerCode");
                    var ActualPrepayFeeList = saActualPrepayFee.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                    var sBillAuditDateStart = _fetchString(i_crm, @"BillAuditDateStart");
                    var sBillAuditDateEnd = _fetchString(i_crm, @"BillAuditDateEnd");
                    var sBillWriteOffDateStart = _fetchString(i_crm, @"BillWriteOffDateStart");
                    var sBillWriteOffDateEnd = _fetchString(i_crm, @"BillWriteOffDateEnd");
                    var sResponsibleDeptID = _fetchString(i_crm, @"ResponsibleDeptID");
                    var sResponsiblePerson = _fetchString(i_crm, @"ResponsiblePerson");
                    var SearchMatchedExps = !string.IsNullOrWhiteSpace(sResponsibleDeptID) || !string.IsNullOrWhiteSpace(sResponsiblePerson);
                    var ChildDeptIDs = CommonRPT.GetChildDepteList(db, i_crm.ORIGID, sResponsibleDeptID);
                    var MatchedExpList = CommonRPT.GetMatchedExps(db, i_crm.ORIGID, ChildDeptIDs, sResponsiblePerson);
                    var MatchedExpGuids = MatchedExpList.Select(t1 => t1.Guid).Distinct().ToArray();
                    var sFlag = _fetchString(i_crm, @"Flag");
                    var Filter = new CVPFilter();
                    Filter.SetBill(sBillAuditDateStart, sBillAuditDateEnd, sBillWriteOffDateStart, sBillWriteOffDateEnd);
                    //撈取符合的billguid
                    var view = db.Queryable<OVW_OPM_Bills, OTB_OPM_BillInfo>
                        ((t1, t2) =>
                        new object[] {
                                JoinType.Inner, t1.OrgID == t2.OrgID && t1.BillNO == t2.BillNO
                              }
                        )
                         .Where((t1, t2) => t1.OrgID == i_crm.ORIGID && Filter.PassStatus.Contains(t2.AuditVal))
                         .WhereIF(SearchMatchedExps, (t1, t2) => SqlFunc.ContainsArray(MatchedExpGuids, t2.ParentId))
                         .WhereIF(!string.IsNullOrEmpty(Filter.sBillAuditDateStart), (t1, t2) => SqlFunc.ToDate(t2.BillFirstCheckDate) >= Filter.rBillAuditDateStart.Date)
                         .WhereIF(!string.IsNullOrEmpty(Filter.sBillAuditDateEnd), (t1, t2) => SqlFunc.ToDate(t2.BillFirstCheckDate) < Filter.rBillAuditDateEnd.Date)
                         .WhereIF(!string.IsNullOrEmpty(Filter.sBillWriteOffDateStart), (t1, t2) => SqlFunc.ToDate(t2.BillWriteOffDate) >= Filter.rBillWriteOffDateStart.Date)
                         .WhereIF(!string.IsNullOrEmpty(Filter.sBillWriteOffDateEnd), (t1, t2) => SqlFunc.ToDate(t2.BillWriteOffDate) < Filter.rBillWriteOffDateEnd.Date);
                    var saBills = view.Select((t1, t2) =>
                                       new View_OPM_BillIReport
                                       {
                                           BillNO = t2.BillNO,
                                           BillType = t2.BillType,
                                           ParentId = t2.ParentId,
                                           ProjectNumber = t1.ProjectNumber,
                                           CustomerCode = t1.CustomerCode,
                                           ResponsiblePerson = t2.ResponsiblePerson,
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
                                      .OrderBy("ResponsiblePerson", "asc")
                                      .ToList();
                    var cellsApp = new ExcelService(sTempFile);
                    var cells = cellsApp.sheet.Cells;//单元格
                    var iCurrRow = 4;
                    cells[1, 1].PutValue(sBillAuditDateStart + "~" + sBillAuditDateEnd);//帳單區間
                    cells[1, 11].PutValue(i_crm.USERID);//列表人
                    cells[2, 1].PutValue(string.Join(",", ChildDeptIDs));//部門
                    cells[2, 11].PutValue(DateTime.Now.ToString(@"yyyy/MM/dd"));//列印時間
                    var AllofPorfits = new List<ProfitInfo>();
                    var group = saBills.GroupBy(c => c.ResponsiblePerson).OrderBy(c => c.Key);
                    var CellType = GetCellType(RPTEnum.CVPAnalysisBySaler, RoundingPoint, ShowSource);
                    var AllCBMUsage = CommonRPT.GetAllCBMUsages(db, i_crm.ORIGID);
                    foreach (IGrouping<string, View_OPM_BillIReport> bills in group)
                    {
                        var _bills = bills.OrderBy(x => x.BillType).ThenBy(x => x.ProjectNumber).ThenBy(x => x.ParentId);
                        var SubProfits = new List<ProfitInfo>();
                        foreach (View_OPM_BillIReport bill in _bills)
                        {
                            var ThisBillCBMUsage = AllCBMUsage.Where(t1 => t1.ParentID == bill.ParentId && t1.IsReturn == bill.IsReturn).ToList();
                            var BillUntaxAmt = bill.InCome * decimal.Parse(bill.ExchangeRate == "" ? "0" : bill.ExchangeRate);
                            var ActualCostFeeItemJson = "";
                            var sActualCost = "";
                            var sTransportationMode = "";

                            CommonRPT.CalcuCostAndProfit(db, ref ActualCostFeeItemJson, ref sActualCost, ref sTransportationMode, bill.BillNO, bill.ParentId, bill.IsReturn, bill.ReFlow, bill.BillType);
                            var ActualCostFeeItemList = CommonRPT.ToFeeItems(ActualCostFeeItemJson);
                            var BillFeeItemList = CommonRPT.ToFeeItems(bill.FeeItems);
                            var SharedActualCost = CommonRPT.GetShareCost(ActualCostFeeItemList, ThisBillCBMUsage, bill.BillNO);
                            var BillReimburseAmount = BillFeeItemList.Where(c => BillPrepayFeeList.Contains(c.FinancialCode)).Sum(c => c.TWAmount); //帳單內特定費用代碼資料
                            var ActualBillReimburseAmount = CommonRPT.GetShareCost(ActualCostFeeItemList, ThisBillCBMUsage, bill.BillNO, ActualPrepayFeeList);//抓實際成本的資料

                            ProfitInfo profitInfo = new ProfitInfo()
                            {
                                ExField = bill.IsReturn,
                                MemberID = bill.ResponsiblePerson,
                                BillUntaxAmt = BillUntaxAmt.Value,
                                SharedActualCost = SharedActualCost,
                                BillReimburseAmount = BillReimburseAmount,
                                ActualBillReimburseAmount = ActualBillReimburseAmount
                            };
                            if (ShowSource)
                            {
                                profitInfo.BillNO = $"類型:{bill.BillType}-單號:{bill.BillNO}-狀態:{bill.AuditVal}";
                            }
                            SubProfits.Add(profitInfo);
                        }
                        var GroupByLogistic = SubProfits.GroupBy(c => c.ExField);
                        ProfitInfo SubtotalProfitInfo = new ProfitInfo() { MemberID = _bills.FirstOrDefault()?.ResponsiblePerson };
                        foreach (var items in GroupByLogistic)
                        {
                            SubtotalProfitInfo.BillUntaxAmt += CommonRPT.Rounding(items.Sum(c => c.BillUntaxAmt), RoundingPoint);
                            SubtotalProfitInfo.SharedActualCost += CommonRPT.Rounding(items.Sum(c => c.SharedActualCost), RoundingPoint);
                            SubtotalProfitInfo.BillReimburseAmount += CommonRPT.Rounding(items.Sum(c => c.BillReimburseAmount), RoundingPoint);
                            SubtotalProfitInfo.ActualBillReimburseAmount += CommonRPT.Rounding(items.Sum(c => c.ActualBillReimburseAmount), RoundingPoint);
                        }
                        var CellColumns = CalcuCellValue(RPTEnum.CVPAnalysisBySaler, SubtotalProfitInfo).ToList();
                        if (ShowSource)
                        {
                            SubtotalProfitInfo.BillNO = string.Join("■", SubProfits.Select(test => test.BillNO));
                            CellColumns.Add(SubtotalProfitInfo.BillNO);
                        }
                        CellsSetValue(cellsApp.workbook, cells, iCurrRow, CellColumns, CellType);
                        AllofPorfits.Add(SubtotalProfitInfo);
                        iCurrRow++;
                    }
                    //總計
                    if (AllofPorfits.Any())
                    {
                        ProfitInfo TotalProfitInfo = new ProfitInfo()
                        {
                            MemberID = $"合計({CurrencyUnit})",
                            BillUntaxAmt = CommonRPT.Rounding(AllofPorfits.Sum(c => c.BillUntaxAmt), RoundingPoint),
                            SharedActualCost = CommonRPT.Rounding(AllofPorfits.Sum(c => c.SharedActualCost), RoundingPoint),
                            BillReimburseAmount = CommonRPT.Rounding(AllofPorfits.Sum(c => c.BillReimburseAmount), RoundingPoint),
                            ActualBillReimburseAmount = CommonRPT.Rounding(AllofPorfits.Sum(c => c.ActualBillReimburseAmount), RoundingPoint),
                        };
                        var CellColumns = CalcuCellValue(RPTEnum.CVPAnalysisBySaler, TotalProfitInfo);
                        CellsSetValue(cellsApp.workbook, cells, iCurrRow, CellColumns, CellType);
                    }
                    cellsApp.sheet.AutoFitColumns();
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
                            sMsg = ex.Message;
                            excelApp.Close();
                        }
                    }
                    File.Delete(sTempFile);   //刪除臨時文件
                    sOutPut = sOutPut.Replace(sBase, @"");
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, sOutPut);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(CostAndProfitReportService), "", FunctionName, "", "", "");
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

        #endregion
        #region 利潤明細表(依展覽)

        public ResponseMessage CVPAnalysisByExhibition(RequestMessage i_crm)
        {
            //展覽區間(第一天)、展覽名稱
            ShowSource = CommonRPT.RPTShow();
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance(commandTimeOut: TimeOut);
            var FunctionName = nameof(CVPAnalysisByExhibition);
            var ExcutePath = CommonRPT.GetExcutePath(FunctionName, "利潤明細表(依展覽)");
            var sOutPut = ExcutePath.Item1;
            var sTempFile = ExcutePath.Item2;
            var sBase = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"");
            var CurrencyUnit = CommonRPT.GetCurrencyUnit(i_crm.ORIGID);
            var RoundingPoint = CommonRPT.GetRoundingPoint(i_crm.ORIGID);
            try
            {
                do
                {
                    var saBillPrepayFee = Common.GetSystemSetting(db, i_crm.ORIGID, "PrepayForCustomerCode");
                    var BillPrepayFeeList = saBillPrepayFee.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    var saActualPrepayFee = Common.GetSystemSetting(db, i_crm.ORIGID, "ActualPrepayForCustomerCode");
                    var ActualPrepayFeeList = saActualPrepayFee.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                    var sExhibitionDateStart = _fetchString(i_crm, @"ExhibitionDateStart");
                    var sExhibitionDateEnd = _fetchString(i_crm, @"ExhibitionDateEnd");
                    var sResponsibleDeptID = _fetchString(i_crm, @"ResponsibleDeptID");
                    var sResponsiblePerson = _fetchString(i_crm, @"ResponsiblePerson");
                    var SearchMatchedExps = !string.IsNullOrWhiteSpace(sResponsibleDeptID) || !string.IsNullOrWhiteSpace(sResponsiblePerson);
                    var ChildDeptIDs = CommonRPT.GetChildDepteList(db, i_crm.ORIGID, sResponsibleDeptID);
                    var MatchedExpList = CommonRPT.GetMatchedExps(db, i_crm.ORIGID, ChildDeptIDs, sResponsiblePerson);
                    var MatchedExpGuids = MatchedExpList.Select(t1 => t1.Guid).Distinct().ToArray();
                    var Filter = new CVPFilter();
                    Filter.SetExhibition(sExhibitionDateStart, sExhibitionDateEnd);
                    //根據前端選擇
                    var sProjectNO = _fetchString(i_crm, @"ProjectNO"); //ProjectNO == OTB_OPM_Exhibition.ExhibitionCode
                    var sProjectName = _fetchString(i_crm, @"ProjectName");
                    var sFlag = _fetchString(i_crm, @"Flag");
                    //展覽區間依「展覽日期第一天」來定義區間
                    var MatchedExhibitions = db.Queryable<OTB_OPM_Exhibition>()
                        .Where(t1 => t1.OrgID == i_crm.ORIGID && t1.Effective == "Y")
                        .WhereIF(!string.IsNullOrWhiteSpace(Filter.sExhibitionDateStart), (t1) => t1.ExhibitionDateStart >= Filter.rExhibitionDateStart.Date)
                        .WhereIF(!string.IsNullOrWhiteSpace(Filter.sExhibitionDateEnd), (t1) => t1.ExhibitionDateStart < Filter.rExhibitionDateEnd.Date)
                        .WhereIF(!string.IsNullOrWhiteSpace(sProjectName), (t1) =>
                         t1.Exhibitioname_EN.Contains(sProjectName) || t1.Exhibitioname_TW.Contains(sProjectName) ||
                        t1.Exhibitioname_CN.Contains(sProjectName) || t1.ExhibitioShotName_TW.Contains(sProjectName) ||
                        t1.ExhibitioShotName_CN.Contains(sProjectName) || t1.ExhibitioShotName_EN.Contains(sProjectName))
                        .WhereIF(!string.IsNullOrWhiteSpace(sProjectNO), (t1) => t1.ExhibitionCode == sProjectNO)
                        .Select(ex => new { ex.ExhibitionCode, ex.ExhibitioShotName_TW, ex.ExhibitionDateStart, SN = ex.SN.ToString(), ProjectNumber = ex.SN.ToString() }).ToList();
                    var FindProjectNum = MatchedExhibitions.Select(c => c.ExhibitionCode).ToArray();
                    //挑調
                    var saBills = new List<View_OPM_BillIReport>();
                    //資料guid
                    if (FindProjectNum.Length != 0)
                    {
                        var view = db.Queryable<OVW_OPM_Bills, OTB_OPM_BillInfo>
                            ((t1, t2) => new object[] {
                                JoinType.Inner, t1.OrgID == t2.OrgID && t1.BillNO == t2.BillNO })
                            .Where((t1, t2) => t1.OrgID == i_crm.ORIGID && CommonRPT.PassStatus.Contains(t2.AuditVal))
                            .WhereIF(FindProjectNum.Any(), (t1, t2) => !SqlFunc.IsNullOrEmpty(t1.ProjectNumber) && SqlFunc.ContainsArray(FindProjectNum, t1.ProjectNumber))
                            .WhereIF(SearchMatchedExps, (t1, t2) => SqlFunc.ContainsArray(MatchedExpGuids, t2.ParentId));
                        saBills = view.Select((t1, t2) =>
                                           new View_OPM_BillIReport
                                           {
                                               BillNO = t2.BillNO,
                                               BillType = t2.BillType,
                                               ParentId = t2.ParentId,
                                               ProjectNumber = t1.ProjectNumber,
                                               CustomerCode = t1.CustomerCode,
                                               ResponsiblePerson = t2.ResponsiblePerson,
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
                                          .OrderBy("ResponsiblePerson", "asc")
                                          .ToList();
                    }
                    var cellsApp = new ExcelService(sTempFile);
                    var cells = cellsApp.sheet.Cells;//单元格
                    var iCurrRow = 4;
                    var iCurrRowOri = iCurrRow;
                    cells[1, 1].PutValue(sExhibitionDateStart + "~" + sExhibitionDateEnd);//展覽區間
                    cells[1, 11].PutValue(i_crm.USERID);//列表人
                    cells[2, 1].PutValue(string.Join(",", ChildDeptIDs));//部門
                    cells[2, 11].PutValue(DateTime.Now.ToString(@"yyyy/MM/dd"));//列印時間
                    var AllofPorfits = new List<ProfitInfo>();
                    var group = saBills.GroupBy(c => c.ProjectNumber).OrderBy(c => c.Key);
                    var AllCBMUsage = CommonRPT.GetAllCBMUsages(db, i_crm.ORIGID);
                    foreach (IGrouping<string, View_OPM_BillIReport> bills in group)
                    {
                        var _bills = bills.OrderBy(x => x.BillNO).ThenBy(x => x.BillType).ThenBy(x => x.ProjectNumber).ThenBy(x => x.ParentId);

                        var SubProfits = new List<ProfitInfo>();
                        foreach (View_OPM_BillIReport bill in _bills)
                        {
                            var ThisBillCBMUsage = AllCBMUsage.Where(t1 => t1.ParentID == bill.ParentId && t1.IsReturn == bill.IsReturn).ToList();
                            var BillUntaxAmt = bill.InCome * decimal.Parse(bill.ExchangeRate == "" ? "0" : bill.ExchangeRate);
                            var ActualCostFeeItemJson = "";
                            var sActualCost = "";
                            var sTransportationMode = "";

                            CommonRPT.CalcuCostAndProfit(db, ref ActualCostFeeItemJson, ref sActualCost, ref sTransportationMode, bill.BillNO, bill.ParentId, bill.IsReturn, bill.ReFlow, bill.BillType);
                            var ActualCostFeeItemList = CommonRPT.ToFeeItems(ActualCostFeeItemJson);
                            var BillFeeItemList = CommonRPT.ToFeeItems(bill.FeeItems);
                            var SharedActualCost = CommonRPT.GetShareCost(ActualCostFeeItemList, ThisBillCBMUsage, bill.BillNO);
                            var BillReimburseAmount = BillFeeItemList.Where(c => BillPrepayFeeList.Contains(c.FinancialCode)).Sum(c => c.TWAmount); //帳單內特定費用代碼資料
                            var ActualBillReimburseAmount = CommonRPT.GetShareCost(ActualCostFeeItemList, ThisBillCBMUsage, bill.BillNO, ActualPrepayFeeList);//抓實際成本的資料
                            ProfitInfo profitInfo = new ProfitInfo()
                            {
                                ExField = bill.IsReturn,
                                BillUntaxAmt = BillUntaxAmt.Value,
                                SharedActualCost = SharedActualCost,
                                BillReimburseAmount = BillReimburseAmount,
                                ActualBillReimburseAmount = ActualBillReimburseAmount
                            };
                            if (ShowSource)
                            {
                                profitInfo.BillNO = $"類型:{bill.BillType}-單號:{bill.BillNO}-狀態:{bill.AuditVal}";
                            }
                            SubProfits.Add(profitInfo);
                        }
                        var Exhibition = MatchedExhibitions.Find(c => c.ExhibitionCode == _bills.FirstOrDefault()?.ProjectNumber);
                        //分成正物流、逆物流計算
                        var GroupByLogistic = SubProfits.GroupBy(c => c.ExField);
                        ProfitInfo SubtotalProfitInfo = new ProfitInfo() { MemberID = Exhibition.ExhibitioShotName_TW };
                        foreach (var items in GroupByLogistic)
                        {
                            SubtotalProfitInfo.BillUntaxAmt += CommonRPT.Rounding(items.Sum(c => c.BillUntaxAmt), RoundingPoint);
                            SubtotalProfitInfo.SharedActualCost += CommonRPT.Rounding(items.Sum(c => c.SharedActualCost), RoundingPoint);
                            SubtotalProfitInfo.BillReimburseAmount += CommonRPT.Rounding(items.Sum(c => c.BillReimburseAmount), RoundingPoint);
                            SubtotalProfitInfo.ActualBillReimburseAmount += CommonRPT.Rounding(items.Sum(c => c.ActualBillReimburseAmount), RoundingPoint);
                        }
                        SubtotalProfitInfo.OrderValue = Exhibition.ExhibitionDateStart == null ? 0 : Exhibition.ExhibitionDateStart.Value.Ticks;
                        if (ShowSource)
                        {
                            SubtotalProfitInfo.BillNO = string.Join("■", SubProfits.Select(test => test.BillNO));
                        }
                        AllofPorfits.Add(SubtotalProfitInfo);
                        iCurrRow++;

                    }

                    //總計
                    if (AllofPorfits.Any())
                    {
                        List<object> CellColumns = null;
                        var CellType = GetCellType(RPTEnum.CVPAnalysisByExhibition, RoundingPoint, ShowSource);
                        //依展期先後順序排序
                        foreach (var SubtotalProfitInfo in AllofPorfits.OrderBy(c => c.OrderValue))
                        {

                            CellColumns = CalcuCellValue(RPTEnum.CVPAnalysisBySaler, SubtotalProfitInfo);
                            if (ShowSource)
                            {
                                CellColumns.Add(SubtotalProfitInfo.BillNO);
                            }
                            CellsSetValue(cellsApp.workbook, cells, iCurrRowOri, CellColumns, CellType);
                            ++iCurrRowOri;
                        }
                        //總計
                        ProfitInfo TotalProfitInfo = new ProfitInfo()
                        {
                            MemberID = $"合計({CurrencyUnit})",
                            BillUntaxAmt = CommonRPT.Rounding(AllofPorfits.Sum(c => c.BillUntaxAmt), RoundingPoint),
                            SharedActualCost = CommonRPT.Rounding(AllofPorfits.Sum(c => c.SharedActualCost), RoundingPoint),
                            BillReimburseAmount = CommonRPT.Rounding(AllofPorfits.Sum(c => c.BillReimburseAmount), RoundingPoint),
                            ActualBillReimburseAmount = CommonRPT.Rounding(AllofPorfits.Sum(c => c.ActualBillReimburseAmount), RoundingPoint),
                        };
                        CellColumns = CalcuCellValue(RPTEnum.CVPAnalysisBySaler, TotalProfitInfo);
                        CellsSetValue(cellsApp.workbook, cells, iCurrRow, CellColumns, CellType);
                    }
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
                            sMsg = ex.Message;
                            excelApp.Close();
                        }
                    }
                    File.Delete(sTempFile);
                    sOutPut = sOutPut.Replace(sBase, @"");
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, sOutPut);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(CostAndProfitReportService), "", FunctionName + "（報表模組）", "", "", "");
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

        #endregion
        #region 金流帳單

        public ResponseMessage CashFlow(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance(commandTimeOut: TimeOut);
            var FunctionName = nameof(CashFlow);
            var ExcutePath = CommonRPT.GetExcutePath(FunctionName, "金流帳單");
            var sOutPut = ExcutePath.Item1;
            var sTempFile = ExcutePath.Item2;
            var sBase = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"");
            var saBillPrepayFee = Common.GetSystemSetting(db, i_crm.ORIGID, "PrepayForCustomerCode");
            var BillPrepayFeeList = saBillPrepayFee.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            var saActualPrepayFee = Common.GetSystemSetting(db, i_crm.ORIGID, "ActualPrepayForCustomerCode");
            var ActualPrepayFeeList = saActualPrepayFee.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            var RoundingPoint = CommonRPT.GetRoundingPoint(i_crm.ORIGID);
            try
            {
                do
                {
                    #region BillType 帳單資料:「帳單號碼」、 「帳單狀態」 、 「帳單拋轉日期」、 「銷帳日期」(OTB_OPM_BillInfo )
                    var sBillNO = _fetchString(i_crm, @"BillNO");
                    var sBillStatus = _fetchString(i_crm, @"BillStatus");
                    var sBillAuditDateStart = _fetchString(i_crm, @"BillAuditDateStart");
                    var sBillAuditDateEnd = _fetchString(i_crm, @"BillAuditDateEnd");
                    var sBillWriteOffDateStart = _fetchString(i_crm, @"BillWriteOffDateStart");
                    var sBillWriteOffDateEnd = _fetchString(i_crm, @"BillWriteOffDateEnd");
                    var Filter = new CVPFilter();
                    Filter.SetBill(sBillAuditDateStart, sBillAuditDateEnd, sBillWriteOffDateStart, sBillWriteOffDateEnd, sBillStatus);
                    #endregion

                    #region ExhibitionType 展覽資料:「專案代號」、「展覽名稱」、「展覽區間(第1天)」、「組團單位」(OTB_OPM_BillInfo : ExhibitionNO => OTB_OPM_Exhibition.SN)
                    var sExhibitionCode = _fetchString(i_crm, @"ExhibitionCode");
                    var sExhibitionName = _fetchString(i_crm, @"ExhibitionName");
                    var sExhibitionSN = _fetchString(i_crm, @"ExhibitionSN");
                    var sExhibitionDateStart = _fetchString(i_crm, @"ExhibitionDateStart");
                    var sExhibitionDateEnd = _fetchString(i_crm, @"ExhibitionDateEnd");
                    var sOrganizerGuid = _fetchString(i_crm, @"OrganizerGuid");
                    Filter.SetExhibition(sExhibitionDateStart, sExhibitionDateEnd);
                    #endregion

                    #region CustomerType 顧客資料:「客戶名稱」、「客戶編號」、「客戶類別(國內 / 外)」(OTB_OPM_BillInfo : Payer  => OTB_CRM_Customers.guid)
                    var sCustomerName = _fetchString(i_crm, @"CustomerName");
                    var sCustomerGuid = _fetchString(i_crm, @"CustomerGuid");
                    var sCustomerNO = _fetchString(i_crm, @"CustomerNO");
                    var sTranType = _fetchString(i_crm, @"TransType");
                    #endregion

                    #region SalerType 業務資料:「負責部門、組別」、「負責業務」(OTB_OPM_BillInfo :ResponsiblePerson => OTB_SYS_Members.MemberID)
                    var sResponsibleDeptID = _fetchString(i_crm, @"ResponsibleDeptID");
                    var sResponsiblePerson = _fetchString(i_crm, @"ResponsiblePerson");
                    var ChildDeptIDs = CommonRPT.GetChildDepteList(db, i_crm.ORIGID, sResponsibleDeptID);
                    var MatchedExps = CommonRPT.GetMatchedExps(db, i_crm.ORIGID, ChildDeptIDs, sResponsiblePerson);
                    var MatchedExpGuids = MatchedExps.Select(x => x.Guid).Distinct().ToArray();
                    #endregion

                    #region Exhibition(四展覽)
                    var viewExpIm = db.Queryable<OTB_OPM_ImportExhibition>().Where(t1 => t1.OrgID == i_crm.ORIGID)
                        .Select((t1) => new ExpInfo
                        {
                            Id = t1.ImportBillNO,
                            ExhibitionType = "ExhibitionImport_Upd",
                            DeptOfResponsibleMember = t1.DepartmentID,
                        });
                    var viewExpEx = db.Queryable<OTB_OPM_ExportExhibition>().Where(t1 => t1.OrgID == i_crm.ORIGID)
                        .Select((t1) => new ExpInfo
                        {
                            Id = t1.ExportBillNO,
                            ExhibitionType = "ExhibitionExport_Upd",
                            DeptOfResponsibleMember = t1.DepartmentID,
                        });
                    var viewExpOth = db.Queryable<OTB_OPM_OtherExhibition>().Where(t1 => t1.OrgID == i_crm.ORIGID)
                        .Select((t1) => new ExpInfo
                        {
                            Id = t1.Guid,
                            ExhibitionType = "OtherBusiness_Upd",
                            DeptOfResponsibleMember = t1.DepartmentID,
                        });
                    var viewExpOthtG = db.Queryable<OTB_OPM_OtherExhibitionTG>().Where(t1 => t1.OrgID == i_crm.ORIGID)
                        .Select((t1) => new ExpInfo
                        {
                            Id = t1.Guid,
                            ExhibitionType = "OtherExhibitionTG_Upd",
                            DeptOfResponsibleMember = t1.DepartmentID,
                        });
                    var saExps = db.UnionAll(viewExpIm, viewExpEx, viewExpOth, viewExpOthtG).ToList();

                    #endregion
                    var saBills = db.Queryable<OTB_OPM_BillInfo, OTB_OPM_Exhibition, OTB_SYS_Arguments, OTB_CRM_Customers, OTB_CRM_CustomersMST, OTB_SYS_Members>((bill, exp, args, cus, cus_m, mber) => new object[] {
                        JoinType.Left, bill.OrgID == exp.OrgID && bill.ExhibitionNO == exp.SN.ToString(),
                            JoinType.Left, args.OrgID == "TE" && exp.Industry == args.ArgumentID && args.ArgumentClassID == "ExhibClass",
                        JoinType.Left, bill.OrgID == cus.OrgID && bill.Payer == cus.guid,
                            JoinType.Left, cus.OrgID == cus_m.OrgID && cus.guid == cus_m.customer_guid,
                        JoinType.Left, bill.OrgID == mber.OrgID && bill.ResponsiblePerson == mber.MemberID })
                        .Where(bill => bill.OrgID == i_crm.ORIGID)
                        //帳單資料:「帳單號碼」、 「帳單狀態」 、 「帳單拋轉日期」、 「銷帳日期」
                        .WhereIF(!string.IsNullOrWhiteSpace(sBillNO), bill => bill.BillNO.Contains(sBillNO))
                        .WhereIF(!string.IsNullOrWhiteSpace(Filter.PassStatus), bill => Filter.PassStatus.Contains(bill.AuditVal))
                        .WhereIF(!string.IsNullOrWhiteSpace(Filter.sBillAuditDateStart), bill => SqlFunc.ToDate(bill.BillFirstCheckDate) >= Filter.rBillAuditDateStart)
                        .WhereIF(!string.IsNullOrWhiteSpace(Filter.sBillAuditDateEnd), bill => SqlFunc.ToDate(bill.BillFirstCheckDate) < Filter.rBillAuditDateEnd)
                        .WhereIF(!string.IsNullOrWhiteSpace(Filter.sBillWriteOffDateStart), bill => SqlFunc.ToDate(bill.BillWriteOffDate) >= Filter.rBillWriteOffDateStart)
                        .WhereIF(!string.IsNullOrWhiteSpace(Filter.sBillWriteOffDateEnd), bill => SqlFunc.ToDate(bill.BillWriteOffDate) < Filter.rBillWriteOffDateEnd)
                        //展覽資料:「專案代號」、「展覽名稱」、「展覽區間(第1天)」、   「組團單位」?????
                        .WhereIF(!string.IsNullOrWhiteSpace(sExhibitionCode), (bill, exp) => exp.ExhibitionCode == sExhibitionCode)
                        .WhereIF(!string.IsNullOrWhiteSpace(sExhibitionName), (bill, exp) => exp.Exhibitioname_EN.Contains(sExhibitionName) || exp.Exhibitioname_TW.Contains(sExhibitionName) ||
                            exp.Exhibitioname_CN.Contains(sExhibitionName) || exp.ExhibitioShotName_TW.Contains(sExhibitionName) ||
                            exp.ExhibitioShotName_CN.Contains(sExhibitionName) || exp.ExhibitioShotName_EN.Contains(sExhibitionName))
                        .WhereIF(!string.IsNullOrWhiteSpace(sExhibitionSN), (bill, exp) => exp.SN.ToString() == sExhibitionSN)
                        .WhereIF(!string.IsNullOrWhiteSpace(Filter.sExhibitionDateStart), (bill, exp) => SqlFunc.ToDate(exp.ExhibitionDateStart) >= Filter.rExhibitionDateStart)
                        .WhereIF(!string.IsNullOrWhiteSpace(Filter.sExhibitionDateEnd), (bill, exp) => SqlFunc.ToDate(exp.ExhibitionDateStart) < Filter.rExhibitionDateEnd)
                        .WhereIF(!string.IsNullOrWhiteSpace(sOrganizerGuid), (bill) => bill.Organizer.Contains(sOrganizerGuid))
                        //顧客資料:「客戶名稱」、「客戶編號」、「客戶類別(國內 / 外)」
                        .WhereIF(!string.IsNullOrWhiteSpace(sCustomerName), (bill, exp, args, cus) => cus.CustomerCName.Contains(sCustomerName) || cus.CustomerEName.Contains(sCustomerName) ||
                            cus.CustomerShotCName.Contains(sCustomerName) || cus.CustomerShotEName.Contains(sCustomerName))
                        .WhereIF(!string.IsNullOrWhiteSpace(sCustomerNO), (bill, exp, args, cus, cus_m) => cus_m.CustomerNO.Contains(sCustomerNO))
                        .WhereIF(!string.IsNullOrWhiteSpace(sCustomerGuid), (bill, exp, args, cus, cus_m) => cus.guid == sCustomerGuid)
                        .WhereIF(!string.IsNullOrWhiteSpace(sTranType), (bill, exp, args, cus, cus_m) => sTranType.Contains(cus.TransactionType))
                        .Where((bill, exp, args, cus, cus_m) => cus_m.Effective == "Y")
                        //業務資料:「負責部門、組別」、「負責業務」
                        .WhereIF(!string.IsNullOrEmpty(sResponsibleDeptID) || !string.IsNullOrEmpty(sResponsiblePerson),
                            (bill, exp, args, cus, cus_m) => SqlFunc.ContainsArray(MatchedExpGuids, bill.ParentId))
                        .Select((bill, exp, args, cus, cus_m, mber) => new
                        {
                            bill.BillNO,
                            bill.AuditVal,
                            bill.BillWriteOffDate,
                            bill.BillFirstCheckDate,
                            bill.CreateDate,
                            exp.ExhibitionCode,
                            exp.Exhibitioname_TW,
                            exp.ExhibitioShotName_TW,
                            exp.Industry,
                            exp.ExhibitionDateStart,
                            sOrganizer = bill.Organizer,
                            cus_m.CustomerNO,
                            cus.CustomerCName,
                            cus.CustomerEName,
                            cus.CustomerShotCName,
                            cus.TransactionType,
                            bill.ResponsiblePerson,
                            bill.Volume,
                            bill.Currency,
                            bill.ExchangeRate,
                            bill.AmountSum, //(原幣別)
                            bill.TaxSum,
                            //拼湊資料用
                            bill.OrgID,
                            bill.FeeItems,
                            bill.ParentId,
                            bill.IsRetn,
                            bill.ReFlow,
                            bill.BillType
                        }).ToList();

                    var TransDic = new Dictionary<string, string>() { {"A","國內" },{"B","國外" },{"C","國外" },
                        { "D","國內" },{"E","其他" },{"F","其他" } };
                    var CustomerDic = db.Queryable<OTB_CRM_Customers>().Where(t1 => t1.OrgID == i_crm.ORIGID)
                        .Select((t1) => new OTB_CRM_Customers
                        {
                            guid = t1.guid,
                            CustomerCName = t1.CustomerCName
                        }).ToList().Distinct().ToList();
                    CustomerDic.Add(new OTB_CRM_Customers { guid = "SelfCome", CustomerCName = "自來" });
                    var DeptDic = CommonRPT.GetDeptInfos(db, i_crm.ORIGID);

                    var cellsApp = new ExcelService(sTempFile);
                    var cells = cellsApp.sheet.Cells;//单元格
                    var iCurrRow = 1;
                    saBills = saBills.OrderBy(c => c.OrgID).ThenBy(c => c.BillNO).ThenBy(c => c.AuditVal).ToList();
                    var CellType = GetCellType(RPTEnum.CashFlow, RoundingPoint, ShowSource);
                    var AllCBMUsage = CommonRPT.GetAllCBMUsages(db, i_crm.ORIGID);
                    var SearchList =  GetExpList(db);
                    foreach (var bill in saBills)
                    {

                        var OrganierCName = "";
                        if (!string.IsNullOrWhiteSpace(bill.sOrganizer))
                            OrganierCName = CustomerDic.FirstOrDefault(t1 => t1.guid == bill.sOrganizer.Trim())?.CustomerCName;
                        var UpperDeptID = "";// 負責部門 
                        var ResDeptID = "";//負責組別
                        var expInfo = saExps.Where(c => c.Id == bill.ParentId).FirstOrDefault(c => c.Id == bill.ParentId);
                        if (expInfo != null && DeptDic.TryGetValue(expInfo.DeptOfResponsibleMember, out var DeptInfo))
                        {
                            //DepartmentID, DepartmentName, ParentDepartmentID, ParentDepartmentName, Level
                            if (DeptInfo.Item5 == "2")
                            {
                                UpperDeptID = DeptInfo.Item4;
                                ResDeptID = DeptInfo.Item2;
                            }
                            else
                            {
                                UpperDeptID = DeptInfo.Item2;
                            }
                        }
                        var ExchangeRate = decimal.Parse(bill.ExchangeRate == "" ? "0" : bill.ExchangeRate);
                        TransDic.TryGetValue(bill.TransactionType, out var TransCategory);

                        var BillUntaxAmt = decimal.Parse(bill.AmountSum == "" ? "0" : bill.AmountSum) * ExchangeRate;
                        var BilltaxAmt = decimal.Parse(bill.TaxSum == "" ? "0" : bill.TaxSum) * ExchangeRate;
                        var ActualCostFeeItemJson = "";
                        var sActualCost = "";
                        var sTransportationMode = "";
                        var BillInfo = new OVW_OPM_BillInfo()
                        {
                            OrgID = bill.OrgID,
                            ParentId = bill.ParentId,
                            IsRetn = bill.IsRetn,
                            AuditVal = bill.AuditVal,
                            Volume = bill.Volume,
                        };
                        var ThisBillCBMUsage = AllCBMUsage.Where(t1 => t1.ParentID == bill.ParentId && t1.IsReturn == bill.IsRetn).ToList();
                        CommonRPT.CalcuCostAndProfitFast(SearchList, ref ActualCostFeeItemJson, ref sActualCost, ref sTransportationMode, bill.BillNO, bill.ParentId, bill.IsRetn, bill.ReFlow, bill.BillType);

                        var ActualCostFeeItemList = CommonRPT.ToFeeItems(ActualCostFeeItemJson);
                        var BillFeeItemList = CommonRPT.ToFeeItems(bill.FeeItems);
                        var SharedActualCost = CommonRPT.GetShareCost(ActualCostFeeItemList, ThisBillCBMUsage, bill.BillNO);
                        var BillReimburseAmount = BillFeeItemList.Where(c => BillPrepayFeeList.Contains(c.FinancialCode)).Sum(c => c.TWAmount); //帳單內特定費用代碼資料
                        var ActualBillReimburseAmount = CommonRPT.GetShareCost(ActualCostFeeItemList, ThisBillCBMUsage, bill.BillNO, ActualPrepayFeeList);//抓實際成本的資料

                        ProfitInfo profitInfo = new ProfitInfo()
                        {
                            BillUntaxAmt = CommonRPT.Rounding(BillUntaxAmt, RoundingPoint),
                            SharedActualCost = CommonRPT.Rounding(SharedActualCost, RoundingPoint),
                            BillReimburseAmount = CommonRPT.Rounding(BillReimburseAmount, RoundingPoint),
                            ActualBillReimburseAmount = CommonRPT.Rounding(ActualBillReimburseAmount, RoundingPoint)
                        };
                        var SummaryInfo = new List<object>
                        {
                            bill.BillNO,
                            bill.AuditVal,
                            bill.BillWriteOffDate,
                            bill.BillFirstCheckDate,
                            bill.CreateDate.ToString(),

                            bill.ExhibitionCode,
                            bill.Exhibitioname_TW,
                            bill.ExhibitioShotName_TW,
                            bill.Industry,
                            bill.ExhibitionDateStart.ObjToString(),
                            OrganierCName,

                            bill.CustomerNO,
                            bill.CustomerCName,
                            bill.CustomerEName,
                            bill.CustomerShotCName,
                            TransCategory,
                            bill.TransactionType,
                            UpperDeptID,
                            ResDeptID,
                            bill.ResponsiblePerson,
                            bill.Volume.ObjToDecimal(),
                            bill.Currency,
                            bill.ExchangeRate.ObjToDecimal(),
                            bill.AmountSum.ObjToDecimal(),

                            profitInfo.BillUntaxAmt,
                            BilltaxAmt,
                            profitInfo.SharedActualCost,
                            profitInfo.BillReimburseAmount,
                            profitInfo.ActualBillReimburseAmount,
                            profitInfo.GrossProfit,
                            profitInfo.NetProfit
                        };
                        CellsSetValue(cellsApp.workbook, cells, iCurrRow, SummaryInfo, CellType);
                        iCurrRow++;
                    }
                    //DATAS
                    cells.StandardWidth = 15;
                    cellsApp.sheet.AutoFitColumns();
                    cellsApp.sheet.AutoFitRows();
                    if (File.Exists(sOutPut))
                    {
                        File.Delete(sOutPut);
                    }
                    cellsApp.sheet.AutoFitColumns();
                    cellsApp.sheet.AutoFitRows();
                    //保存
                    cellsApp.workbook.Save(sOutPut);
                    File.Delete(sTempFile);   //刪除臨時文件
                    sOutPut = sOutPut.Replace(sBase, @"");
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, sOutPut);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(CostAndProfitReportService), "", FunctionName + "（報表模組）", "", "", "");
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

        private List<dynamic> GetExpList(SqlSugarClient db)
        {
            var SearchDatas = new List<dynamic>();
            var Others = db.Queryable<OTB_OPM_OtherExhibition>().Select(t1 => new OTB_OPM_OtherExhibition() { ActualCost = t1.ActualCost, ExFeild1 = "OtherBusiness_Upd", Guid = t1.Guid }).ToList();
            SearchDatas.AddRange(Others);
            var Imports = db.Queryable<OTB_OPM_ImportExhibition>().Select(t1 => new OTB_OPM_ImportExhibition() { ExFeild1 = "ExhibitionImport_Upd", ReturnBills = t1.ReturnBills, ReImports = t1.ReImports, ActualCost = t1.ActualCost, TransportationMode = t1.TransportationMode, ImportBillNO = t1.ImportBillNO }).ToList();
            SearchDatas.AddRange(Imports);
            var Exports = db.Queryable<OTB_OPM_ExportExhibition>().Select(t1 => new OTB_OPM_ExportExhibition() { ExFeild1 = "ExhibitionExport_Upd", ReturnBills = t1.ReturnBills, Exhibitors = t1.Exhibitors, ActualCost = t1.ActualCost, TransportationMode = t1.TransportationMode, ExportBillNO = t1.ExportBillNO }).ToList();
            SearchDatas.AddRange(Exports);
            return SearchDatas;
        }

        #endregion
        #region 貢獻度報表(代理)

        public ResponseMessage DegreeOfContributionByAgent(RequestMessage i_crm)
        {
            //帳單(拋轉)區間、銷帳區間、部門、業務 代理  ==> C
            ShowSource = CommonRPT.RPTShow();
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance(commandTimeOut: TimeOut);
            var FunctionName = nameof(DegreeOfContributionByAgent);
            var ExcutePath = CommonRPT.GetExcutePath(FunctionName, "貢獻度報表(代理)");
            var sOutPut = ExcutePath.Item1;
            var sTempFile = ExcutePath.Item2;
            var sBase = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"");
            var stransactiontype = "C";
            var RoundingPoint = CommonRPT.GetRoundingPoint(i_crm.ORIGID);
            try
            {
                do
                {
                    var sBillAuditDateStart = _fetchString(i_crm, @"BillAuditDateStart");
                    var sBillAuditDateEnd = _fetchString(i_crm, @"BillAuditDateEnd");
                    var sResponsibleDeptID = _fetchString(i_crm, @"ResponsibleDeptID");
                    var sResponsiblePerson = _fetchString(i_crm, @"ResponsiblePerson");
                    var ChildDeptIDs = CommonRPT.GetChildDepteList(db, i_crm.ORIGID, sResponsibleDeptID);
                    var sAgentName = _fetchString(i_crm, @"AgentName");
                    var sAgentGuid = _fetchString(i_crm, @"AgentGuid");
                    var sFlag = _fetchString(i_crm, @"Flag");
                    var Filter = new CVPFilter();

                    Filter.SetBill(sBillAuditDateStart, sBillAuditDateEnd, "", "");
                    //依序1.Customers => 2.Exhibition(四展覽) => 3.Billinfo  => 4.統計 
                    var CustomerKey = "";
                    var Customers = db.Queryable<OTB_CRM_Customers>()
                           .Where(t1 => t1.OrgID == i_crm.ORIGID && t1.Effective == "Y" && t1.TransactionType == stransactiontype)
                           .WhereIF(!string.IsNullOrWhiteSpace(sAgentName), (t1) =>
                            t1.CustomerCName.Contains(sAgentName) || t1.CustomerEName.Contains(sAgentName) ||
                            t1.CustomerShotCName.Contains(sAgentName) || t1.CustomerShotEName.Contains(sAgentName))
                            .WhereIF(!string.IsNullOrWhiteSpace(sAgentGuid), t1 => t1.guid == sAgentGuid);
                    CustomerKey = Customers.Select(t1 => t1.guid).ToJson();


                    #region Exhibition(四展覽)
                    var viewExpIm = db.Queryable<OTB_OPM_ImportExhibition, OTB_CRM_Customers>
                        ((t1, t2) => new object[] { JoinType.Left, t1.OrgID == t2.OrgID && t1.Agent == t2.guid })
                        .Where(t1 => t1.OrgID == i_crm.ORIGID && t1.IsVoid == "N" && !SqlFunc.IsNullOrEmpty(t1.Agent))
                        .WhereIF(!string.IsNullOrWhiteSpace(CustomerKey), (t1) => CustomerKey.Contains(t1.Agent))
                        .WhereIF(ChildDeptIDs.Any(), t1 => SqlFunc.ContainsArray(ChildDeptIDs, t1.DepartmentID))
                        .WhereIF(!string.IsNullOrWhiteSpace(sResponsiblePerson), t1 => t1.ResponsiblePerson == sResponsiblePerson)
                        .Select((t1, t2) => new ExpInfo
                        {
                            Id = t1.ImportBillNO,
                            ExhibitionType = "ExhibitionImport_Upd",
                            ActualCost = t1.ActualCost,
                            Bills = t1.Bills,
                            ReturnBills = t1.ReturnBills,
                            RefNumber = t1.RefNumber,
                            ExpNO = t1.ExhibitionNO,
                            Agent = t2.CustomerShotCName
                        });
                    var viewExpEx = db.Queryable<OTB_OPM_ExportExhibition, OTB_CRM_Customers>
                        ((t1, t2) => new object[] { JoinType.Left, t1.OrgID == t2.OrgID && t1.Agent == t2.guid })
                        .Where(t1 => t1.OrgID == i_crm.ORIGID && t1.IsVoid == "N" && !SqlFunc.IsNullOrEmpty(t1.Agent))
                        .WhereIF(!string.IsNullOrWhiteSpace(CustomerKey), (t1) => CustomerKey.Contains(t1.Agent))
                        .WhereIF(ChildDeptIDs.Any(), t1 => SqlFunc.ContainsArray(ChildDeptIDs, t1.DepartmentID))
                        .WhereIF(!string.IsNullOrWhiteSpace(sResponsiblePerson), t1 => t1.ResponsiblePerson == sResponsiblePerson)
                        .Select((t1, t2) => new ExpInfo
                        {
                            Id = t1.ExportBillNO,
                            ExhibitionType = "ExhibitionExport_Upd",
                            ActualCost = t1.ActualCost,
                            Bills = t1.Bills,
                            ReturnBills = t1.ReturnBills,
                            RefNumber = t1.RefNumber,
                            ExpNO = t1.ExhibitionNO,
                            Agent = t2.CustomerShotCName
                        });
                    var viewExpOth = db.Queryable<OTB_OPM_OtherExhibition, OTB_CRM_Customers>
                        ((t1, t2) => new object[] { JoinType.Left, t1.OrgID == t2.OrgID && t1.Agent == t2.guid })
                        .Where(t1 => t1.OrgID == i_crm.ORIGID && t1.IsVoid == "N" && !SqlFunc.IsNullOrEmpty(t1.Agent))
                        .WhereIF(!string.IsNullOrWhiteSpace(CustomerKey), (t1) => CustomerKey.Contains(t1.Agent))
                        .WhereIF(ChildDeptIDs.Any(), t1 => SqlFunc.ContainsArray(ChildDeptIDs, t1.DepartmentID))
                        .WhereIF(!string.IsNullOrWhiteSpace(sResponsiblePerson), t1 => t1.ResponsiblePerson == sResponsiblePerson)
                        .Select((t1, t2) => new ExpInfo
                        {
                            Id = t1.Guid,
                            ExhibitionType = "OtherBusiness_Upd",
                            ActualCost = t1.ActualCost,
                            Bills = t1.Bills,
                            ReturnBills = "",
                            RefNumber = "",
                            ExpNO = t1.ExhibitionNO,
                            Agent = t2.CustomerShotCName
                        });
                    var viewExpOthtG = db.Queryable<OTB_OPM_OtherExhibitionTG, OTB_CRM_Customers>
                        ((t1, t2) => new object[] { JoinType.Left, t1.OrgID == t2.OrgID && t1.Agent == t2.guid })
                        .Where(t1 => t1.OrgID == i_crm.ORIGID && t1.IsVoid == "N" && !SqlFunc.IsNullOrEmpty(t1.Agent))
                        .WhereIF(!string.IsNullOrWhiteSpace(CustomerKey), (t1) => CustomerKey.Contains(t1.Agent))
                        .WhereIF(ChildDeptIDs.Any(), t1 => SqlFunc.ContainsArray(ChildDeptIDs, t1.DepartmentID))
                        .WhereIF(!string.IsNullOrWhiteSpace(sResponsiblePerson), t1 => t1.ResponsiblePerson == sResponsiblePerson)
                        .Select((t1, t2) => new ExpInfo
                        {
                            Id = t1.Guid,
                            ExhibitionType = "OtherExhibitionTG_Upd",
                            ActualCost = t1.ActualCost,
                            Bills = t1.Bills,
                            ReturnBills = "",
                            RefNumber = "",
                            ExpNO = t1.ExhibitionNO,
                            Agent = t2.CustomerShotCName
                        });
                    var saExps = db.UnionAll(viewExpIm, viewExpEx, viewExpOth, viewExpOthtG).ToList();
                    var SearchedExp =!string.IsNullOrWhiteSpace(CustomerKey) || !string.IsNullOrWhiteSpace(sResponsibleDeptID) || !string.IsNullOrWhiteSpace(sResponsibleDeptID);
                    #endregion

                    #region 符合展覽的的帳單
                    var ExhibitionKeys = string.Join(",", saExps.Select(c => c.Id).ToList());
                    var view = db.Queryable<OVW_OPM_Bills, OTB_OPM_BillInfo>
                        ((t1, t2) =>
                        new object[] {
                                JoinType.Inner, t1.OrgID == t2.OrgID && t1.BillNO == t2.BillNO
                              }
                        )
                         .Where((t1, t2) => t1.OrgID == i_crm.ORIGID && CommonRPT.PassStatus.Contains(t2.AuditVal))
                         .WhereIF(SearchedExp, (t1, t2) => ExhibitionKeys.Contains(t2.ParentId))
                         .WhereIF(!string.IsNullOrEmpty(Filter.sBillAuditDateStart), (t1, t2) => SqlFunc.ToDate(t2.BillFirstCheckDate) >= Filter.rBillAuditDateStart.Date)
                         .WhereIF(!string.IsNullOrEmpty(Filter.sBillAuditDateEnd), (t1, t2) => SqlFunc.ToDate(t2.BillFirstCheckDate) < Filter.rBillAuditDateEnd.Date);

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
                                           Weight = t2.Weight,
                                           Volume = t2.Volume,
                                           AuditVal = t2.AuditVal,
                                           ReFlow = t2.ReFlow,
                                           FeeItems = t2.FeeItems
                                       })
                                      .MergeTable()
                                      .ToList();
                    #endregion

                    var cellsApp = new ExcelService(sTempFile);
                    var cells = cellsApp.sheet.Cells;//单元格
                    var iCurrRow = 6;
                    cells[1, 1].PutValue(sBillAuditDateStart + "~" + sBillAuditDateEnd);//帳單區間
                    cells[1, 7].PutValue(i_crm.USERID);//列表人
                    cells[2, 1].PutValue(string.Join(",", ChildDeptIDs));//部門
                    cells[2, 7].PutValue(DateTime.Now.ToString(@"yyyy/MM/dd"));//列印時間
                    cells[3, 1].PutValue(sResponsiblePerson);//業務
                    var saBillPrepayFee = Common.GetSystemSetting(db, i_crm.ORIGID, "PrepayForCustomerCode");
                    var BillPrepayFeeList = saBillPrepayFee.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    var saActualPrepayFee = Common.GetSystemSetting(db, i_crm.ORIGID, "ActualPrepayForCustomerCode");
                    var ActualPrepayFeeList = saActualPrepayFee.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    var AllofPorfits = new List<ProfitInfo>();
                    var BillGroup = saBills.GroupBy(c => c.ParentId);
                    var AllCBMUsage = CommonRPT.GetAllCBMUsages(db, i_crm.ORIGID);
                    foreach (var bills in BillGroup)
                    {
                        var SubProfits = new List<ProfitInfo>();
                        foreach (View_OPM_BillIReport bill in bills)
                        {
                            var ThisBillCBMUsage = AllCBMUsage.Where(t1 => t1.ParentID == bill.ParentId && t1.IsReturn == bill.IsReturn).ToList();
                            var iInCome = bill.InCome * decimal.Parse(bill.ExchangeRate == "" ? "0" : bill.ExchangeRate);
                            var ActualCostFeeItemJson = "";
                            var sActualCost = "";
                            var sTransportationMode = "";

                            CommonRPT.CalcuCostAndProfit(db, ref ActualCostFeeItemJson, ref sActualCost, ref sTransportationMode, bill.BillNO, bill.ParentId, bill.IsReturn, bill.ReFlow, bill.BillType);
                            var ActualCostFeeItemList = CommonRPT.ToFeeItems(ActualCostFeeItemJson);
                            var BillFeeItemList = CommonRPT.ToFeeItems(bill.FeeItems);
                            var SharedActualCost = CommonRPT.GetShareCost(ActualCostFeeItemList, ThisBillCBMUsage, bill.BillNO);
                            var BillReimburseAmount = BillFeeItemList.Where(c => BillPrepayFeeList.Contains(c.FinancialCode)).Sum(c => c.TWAmount); //帳單內特定費用代碼資料
                            var ActualBillReimburseAmount = CommonRPT.GetShareCost(ActualCostFeeItemList, ThisBillCBMUsage, bill.BillNO, ActualPrepayFeeList);//抓實際成本的資料
                            decimal.TryParse(bill.Volume, out var Volume);
                            decimal.TryParse(bill.Weight, out var Weight);
                            var CustomerName = saExps.FirstOrDefault(c => c.Id == bill.ParentId)?.Agent;
                            ProfitInfo profitInfo = new ProfitInfo()
                            {
                                ExhibitionName = bill.ProjectNumber,
                                CustomerName = CustomerName,
                                BillUntaxAmt = CommonRPT.Rounding(iInCome.Value, RoundingPoint),
                                SharedActualCost = SharedActualCost,
                                BillReimburseAmount = BillReimburseAmount,
                                ActualBillReimburseAmount = ActualBillReimburseAmount,
                                Weight = Weight,
                                Volume = Volume
                            };
                            if (ShowSource)
                            {
                                profitInfo.BillNO = $"類型:{bill.BillType}-單號:{bill.BillNO}-狀態:{bill.AuditVal}";
                            }
                            SubProfits.Add(profitInfo);
                        }
                        ProfitInfo SubtotalProfitInfo = new ProfitInfo()
                        {
                            ExhibitionName = SubProfits.First().ExhibitionName,
                            CustomerName = SubProfits.First().CustomerName,
                            BillUntaxAmt = SubProfits.Sum(c => c.BillUntaxAmt),
                            SharedActualCost = CommonRPT.Rounding(SubProfits.Sum(c => c.SharedActualCost), RoundingPoint),
                            BillReimburseAmount = CommonRPT.Rounding(SubProfits.Sum(c => c.BillReimburseAmount), RoundingPoint),
                            ActualBillReimburseAmount = CommonRPT.Rounding(SubProfits.Sum(c => c.ActualBillReimburseAmount), RoundingPoint),
                            Weight = SubProfits.Sum(c => c.Weight),
                            Volume = SubProfits.Sum(c => c.Volume)
                        };
                        if (ShowSource)
                        {
                            SubtotalProfitInfo.BillNO = string.Join("■", SubProfits.Select(test => test.BillNO));
                        }
                        AllofPorfits.Add(SubtotalProfitInfo);
                    }
                    //分類印出來
                    var AgentGroup = AllofPorfits.GroupBy(c => c.CustomerName).OrderBy(c => c.Key);
                    var CellType = GetCellType(RPTEnum.DegreeOfContributionByAgent, RoundingPoint, ShowSource);
                    foreach (var profitInfos in AgentGroup)
                    {
                        ProfitInfo profitInfo = new ProfitInfo()
                        {
                            ExField = profitInfos.Select(c => c.ExhibitionName).Distinct().Count(),
                            CustomerName = profitInfos.First().CustomerName,
                            BillUntaxAmt = CommonRPT.Rounding(profitInfos.Sum(c => c.BillUntaxAmt), RoundingPoint),
                            SharedActualCost = CommonRPT.Rounding(profitInfos.Sum(c => c.SharedActualCost), RoundingPoint),
                            BillReimburseAmount = CommonRPT.Rounding(profitInfos.Sum(c => c.BillReimburseAmount), RoundingPoint),
                            ActualBillReimburseAmount = CommonRPT.Rounding(profitInfos.Sum(c => c.ActualBillReimburseAmount), RoundingPoint),
                            Weight = profitInfos.Sum(c => c.Weight),
                            Volume = profitInfos.Sum(c => c.Volume)
                        };
                        var CellColumns = CalcuCellValue(RPTEnum.DegreeOfContributionByAgent, profitInfo).ToList();
                        if (ShowSource)
                        {
                            var Source = string.Join("", profitInfos.Select(test => test.BillNO));
                            CellColumns.Add(Source);
                        }

                        CellsSetValue(cellsApp.workbook, cells, iCurrRow, CellColumns, CellType);
                        iCurrRow++;

                    }
                    cellsApp.sheet.AutoFitColumns();
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
                            rm = new SuccessResponseMessage(null, i_crm);
                            sMsg = ex.Message;
                            excelApp.Close();
                        }
                    }
                    File.Delete(sTempFile);   //刪除臨時文件
                    sOutPut = sOutPut.Replace(sBase, @"");
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, sOutPut);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(CostAndProfitReportService), "", FunctionName + "（報表模組）", "", "", "");
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

        #endregion
        #region 貢獻度報表(客戶)

        public ResponseMessage DegreeOfContributionByCustomer(RequestMessage i_crm)
        {
            //帳單(拋轉)區間、銷帳區間、部門、業務 客戶  ==> A B
            ShowSource = CommonRPT.RPTShow();
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance(commandTimeOut: TimeOut);
            var FunctionName = nameof(DegreeOfContributionByCustomer);
            var ExcutePath = CommonRPT.GetExcutePath(FunctionName, "貢獻度報表(客戶)");
            var sOutPut = ExcutePath.Item1;
            var sTempFile = ExcutePath.Item2;
            var sBase = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"");
            var RoundingPoint = CommonRPT.GetRoundingPoint(i_crm.ORIGID);
            var sTransactiontype = "A,B";
            try
            {
                do
                {
                    var sBillAuditDateStart = _fetchString(i_crm, @"BillAuditDateStart");
                    var sBillAuditDateEnd = _fetchString(i_crm, @"BillAuditDateEnd");
                    var sResponsibleDeptID = _fetchString(i_crm, @"ResponsibleDeptID");
                    var sResponsiblePerson = _fetchString(i_crm, @"ResponsiblePerson");
                    var SearchMatchedExps = !string.IsNullOrWhiteSpace(sResponsibleDeptID) || !string.IsNullOrWhiteSpace(sResponsiblePerson);
                    var ChildDeptIDs = CommonRPT.GetChildDepteList(db, i_crm.ORIGID, sResponsibleDeptID);
                    var MatchedExpList = CommonRPT.GetMatchedExps(db, i_crm.ORIGID, ChildDeptIDs, sResponsiblePerson);
                    var MatchedExpGuids = MatchedExpList.Select(t1 => t1.Guid).Distinct().ToArray();
                    var sCustomerName = _fetchString(i_crm, @"CustomerName");
                    var sCustomerGuid = _fetchString(i_crm, @"CustomerGuid");
                    var sFlag = _fetchString(i_crm, @"Flag");

                    var Filter = new CVPFilter();
                    Filter.SetBill(sBillAuditDateStart, sBillAuditDateEnd, "", "");
                    //依序1.Customers => 2.Exhibition(四展覽) => 3.Billinfo  => 4.統計 
                    var CustomerKey = "";
                    var Customers = db.Queryable<OTB_CRM_Customers>()
                        .Where(t1 => t1.OrgID == i_crm.ORIGID && t1.Effective == "Y" && sTransactiontype.Contains(t1.TransactionType))
                        .WhereIF(!string.IsNullOrWhiteSpace(sCustomerName), (t1) =>
                            t1.CustomerCName.Contains(sCustomerName) || t1.CustomerEName.Contains(sCustomerName) ||
                            t1.CustomerShotCName.Contains(sCustomerName) || t1.CustomerShotEName.Contains(sCustomerName))
                        .WhereIF(!string.IsNullOrWhiteSpace(sCustomerGuid), t1 => t1.guid == sCustomerGuid);
                    //找不到也要篩選
                    CustomerKey = Customers.Select(t1 => t1.guid).ToJson();
                    #region 符合展覽的的帳單
                    var view = db.Queryable<OVW_OPM_Bills, OTB_OPM_BillInfo, OTB_CRM_Customers>
                        ((t1, t2, t3) =>
                        new object[] {
                                JoinType.Inner, t1.OrgID == t2.OrgID && t1.BillNO == t2.BillNO,
                                JoinType.Inner, t1.OrgID == t3.OrgID && t2.Payer == t3.guid &&  sTransactiontype.Contains(t3.TransactionType)
                              }
                        )
                         .Where((t1, t2) => t1.OrgID == i_crm.ORIGID && CommonRPT.PassStatus.Contains(t2.AuditVal))
                         .WhereIF(!string.IsNullOrWhiteSpace(CustomerKey), (t1, t2) => !SqlFunc.IsNullOrEmpty(t2.Payer) && CustomerKey.Contains(t2.Payer))
                         .WhereIF(SearchMatchedExps, (t1, t2) => SqlFunc.ContainsArray(MatchedExpGuids, t2.ParentId))
                         .WhereIF(!string.IsNullOrEmpty(Filter.sBillAuditDateStart), (t1, t2) => SqlFunc.ToDate(t2.BillFirstCheckDate) >= Filter.rBillAuditDateStart.Date)
                         .WhereIF(!string.IsNullOrEmpty(Filter.sBillAuditDateEnd), (t1, t2) => SqlFunc.ToDate(t2.BillFirstCheckDate) < Filter.rBillAuditDateEnd.Date);

                    var saBills = view.Select((t1, t2, t3) =>
                                       new View_OPM_BillIReport
                                       {
                                           BillNO = t2.BillNO,
                                           BillType = t2.BillType,
                                           ParentId = t2.ParentId,
                                           ProjectNumber = t1.ProjectNumber,
                                           CustomerName = t3.CustomerShotCName,
                                           ResponsiblePerson = t1.ResponsiblePersonFullCode,
                                           Currency = t2.Currency,
                                           ExchangeRate = t2.ExchangeRate,
                                           InCome = t1.TWNOTaxAmount, //未稅總計
                                           OrgID = t2.OrgID,
                                           IsReturn = t2.IsRetn,
                                           Weight = t2.Weight,
                                           Volume = t2.Volume,
                                           AuditVal = t2.AuditVal,
                                           ReFlow = t2.ReFlow,
                                           FeeItems = t2.FeeItems
                                       })
                                      .MergeTable()
                                      .ToList();
                    #endregion


                    var cellsApp = new ExcelService(sTempFile);
                    var cells = cellsApp.sheet.Cells;//单元格
                    var iCurrRow = 6;
                    cells[1, 1].PutValue(sBillAuditDateStart + "~" + sBillAuditDateEnd);//帳單區間
                    cells[1, 7].PutValue(i_crm.USERID);//列表人
                    cells[2, 1].PutValue(string.Join(",", ChildDeptIDs));//部門
                    cells[2, 7].PutValue(DateTime.Now.ToString(@"yyyy/MM/dd"));//列印時間
                    cells[3, 1].PutValue(sResponsiblePerson);//業務

                    var saBillPrepayFee = Common.GetSystemSetting(db, i_crm.ORIGID, "PrepayForCustomerCode");
                    var BillPrepayFeeList = saBillPrepayFee.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    var saActualPrepayFee = Common.GetSystemSetting(db, i_crm.ORIGID, "ActualPrepayForCustomerCode");
                    var ActualPrepayFeeList = saActualPrepayFee.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                    var AllofPorfits = new List<ProfitInfo>();
                    //統計帳單資料
                    var BillGroup = saBills.GroupBy(c => c.CustomerName).OrderBy(c => c.Key);
                    var CellType = GetCellType(RPTEnum.DegreeOfContributionByCustomer, RoundingPoint, ShowSource);
                    var AllCBMUsage = CommonRPT.GetAllCBMUsages(db, i_crm.ORIGID);
                    foreach (var bills in BillGroup)
                    {
                        var SubProfits = new List<ProfitInfo>();
                        foreach (View_OPM_BillIReport bill in bills)
                        {
                            var ThisBillCBMUsage = AllCBMUsage.Where(t1 => t1.ParentID == bill.ParentId && t1.IsReturn == bill.IsReturn).ToList();
                            var iInCome = bill.InCome * decimal.Parse(bill.ExchangeRate == "" ? "0" : bill.ExchangeRate);
                            var ActualCostFeeItemJson = "";
                            var sActualCost = "";
                            var sTransportationMode = "";

                            CommonRPT.CalcuCostAndProfit(db, ref ActualCostFeeItemJson, ref sActualCost, ref sTransportationMode, bill.BillNO, bill.ParentId, bill.IsReturn, bill.ReFlow, bill.BillType);
                            var ActualCostFeeItemList = CommonRPT.ToFeeItems(ActualCostFeeItemJson);
                            var BillFeeItemList = CommonRPT.ToFeeItems(bill.FeeItems);
                            var SharedActualCost = CommonRPT.GetShareCost(ActualCostFeeItemList, ThisBillCBMUsage, bill.BillNO);
                            var BillReimburseAmount = BillFeeItemList.Where(c => BillPrepayFeeList.Contains(c.FinancialCode)).Sum(c => c.TWAmount); //帳單內特定費用代碼資料
                            var ActualBillReimburseAmount = CommonRPT.GetShareCost(ActualCostFeeItemList, ThisBillCBMUsage, bill.BillNO, ActualPrepayFeeList);//抓實際成本的資料
                            decimal.TryParse(bill.Volume, out var Volume);
                            decimal.TryParse(bill.Weight, out var Weight);
                            ProfitInfo profitInfo = new ProfitInfo()
                            {
                                ExhibitionName = bill.ProjectNumber,
                                CustomerName = bill.CustomerName,
                                BillUntaxAmt = CommonRPT.Rounding(iInCome.Value, RoundingPoint),
                                SharedActualCost = SharedActualCost,
                                BillReimburseAmount = BillReimburseAmount,
                                ActualBillReimburseAmount = ActualBillReimburseAmount,
                                Weight = Weight,
                                Volume = Volume
                            };
                            if (ShowSource)
                            {
                                profitInfo.BillNO = $"類型:{bill.BillType}-單號:{bill.BillNO}-狀態:{bill.AuditVal}";
                            }
                            SubProfits.Add(profitInfo);
                        }
                        ProfitInfo SubtotalProfitInfo = new ProfitInfo()
                        {
                            //放展覽次數
                            ExField = SubProfits.Select(c => c.ExhibitionName).Distinct().Count(),
                            CustomerName = SubProfits.First().CustomerName,
                            BillUntaxAmt = CommonRPT.Rounding(SubProfits.Sum(c => c.BillUntaxAmt), RoundingPoint),
                            SharedActualCost = CommonRPT.Rounding(SubProfits.Sum(c => c.SharedActualCost), RoundingPoint),
                            BillReimburseAmount = CommonRPT.Rounding(SubProfits.Sum(c => c.BillReimburseAmount), RoundingPoint),
                            ActualBillReimburseAmount = CommonRPT.Rounding(SubProfits.Sum(c => c.ActualBillReimburseAmount), RoundingPoint),
                            Weight = SubProfits.Sum(c => c.Weight),
                            Volume = SubProfits.Sum(c => c.Volume)
                        };

                        var CellColumns = CalcuCellValue(RPTEnum.DegreeOfContributionByCustomer, SubtotalProfitInfo).ToList();
                        if (ShowSource)
                        {
                            var Source = string.Join("■", SubProfits.Select(test => test.BillNO));
                            CellColumns.Add(Source);
                        }

                        CellsSetValue(cellsApp.workbook, cells, iCurrRow, CellColumns, CellType);


                        AllofPorfits.AddRange(SubProfits);
                        iCurrRow++;
                    }

                    cells.StandardWidth = 15;
                    cellsApp.sheet.AutoFitColumn(7);
                    if (File.Exists(sOutPut))
                    {
                        File.Delete(sOutPut);
                    }
                    cellsApp.sheet.AutoFitColumns();
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
                            rm = new SuccessResponseMessage(null, i_crm);
                            sMsg = ex.Message;
                            excelApp.Close();
                        }
                    }
                    File.Delete(sTempFile);   //刪除臨時文件
                    sOutPut = sOutPut.Replace(sBase, @"");
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, sOutPut);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(CostAndProfitReportService), "", FunctionName + "（報表模組）", "", "", "");
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

        #endregion
        #region 現有客戶資訊

        public ResponseMessage ExistingCustomer(RequestMessage i_crm)
        {
            ShowSource = CommonRPT.RPTShow();
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance(commandTimeOut: TimeOut);
            var FunctionName = nameof(ExistingCustomer);
            var ExcutePath = CommonRPT.GetExcutePath(FunctionName, "現有客戶資訊");
            var sOutPut = ExcutePath.Item1;
            var sTempFile = ExcutePath.Item2;
            var sBase = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"");
            var RoundingPoint = CommonRPT.GetRoundingPoint(i_crm.ORIGID);
            try
            {
                do
                {
                    var sCustomerName = _fetchString(i_crm, @"CustomerName");
                    var sCustomerGuid = _fetchString(i_crm, @"CustomerGuid");
                    var sState = _fetchString(i_crm, @"State");
                    var sStartAttendeeTime = _fetchString(i_crm, @"StartAttendeeTime");
                    var sEndAttendeeTime = _fetchString(i_crm, @"EndAttendeeTime");
                    var sResponsibleDeptID = _fetchString(i_crm, @"ResponsibleDeptID");
                    var ChildDeptIDs = CommonRPT.GetChildDepteList(db, i_crm.ORIGID, sResponsibleDeptID);
                    var iStartAttendeeTime = 0;
                    var iEndAttendeeTime = 0;
                    int.TryParse(sStartAttendeeTime, out iStartAttendeeTime);
                    int.TryParse(sEndAttendeeTime, out iEndAttendeeTime);
                    var stransactiontype = _fetchString(i_crm, @"TransactionType");
                    var sFlag = _fetchString(i_crm, @"Flag");
                    //「客戶」、「交易型態」、「國家」、「參展次數」
                    var Customers = db.Queryable<OTB_CRM_Customers>()
                        .Where(t1 => t1.OrgID == i_crm.ORIGID && t1.Effective == "Y")
                        .WhereIF(!string.IsNullOrWhiteSpace(sCustomerName), (t1) =>
                         t1.CustomerCName.Contains(sCustomerName) || t1.CustomerEName.Contains(sCustomerName) ||
                        t1.CustomerShotCName.Contains(sCustomerName) || t1.CustomerShotEName.Contains(sCustomerName))
                        .WhereIF(!string.IsNullOrWhiteSpace(sCustomerGuid), (t1) => t1.guid == sCustomerGuid)
                        .WhereIF(!string.IsNullOrWhiteSpace(stransactiontype), (t1) => stransactiontype.Contains(t1.TransactionType))
                        .WhereIF(!string.IsNullOrWhiteSpace(sState), (t1) => t1.State == sState)
                        .ToList();
                    var ExistingCustomerInfo = new List<ExistingCustomerInfo>();
                    foreach (var customer in Customers)
                    {
                        var info = new ExistingCustomerInfo()
                        {
                            GUID = customer.guid,
                            ChName = customer.CustomerCName,
                            EnName = customer.CustomerEName,
                            TaxNumber = customer.UniCode,
                            TransType = customer.TransactionType,
                            State = customer.State,
                            AttendeeTimes = 0,
                            Address = customer.Address,
                            Website = customer.WebsiteAdress,
                            AttendeeExhibitions = new List<string>(),
                        };
                        #region 計算次數
                        var sId = customer.guid;
                        var saExhibitions = new List<Map>();
                        //出口
                        var saExport = db.Queryable<OTB_OPM_Exhibition, OTB_OPM_ExportExhibition>((t1, t2) => t1.SN.ToString() == t2.ExhibitionNO && t1.OrgID == t2.OrgID && t1.Effective == "Y" && t2.IsVoid == "N")
                            .WhereIF(!string.IsNullOrWhiteSpace(sResponsibleDeptID), (t1, t2) => SqlFunc.ContainsArray(ChildDeptIDs, t2.DepartmentID))
                            .Select((t1, t2) => new { t1.OrgID, t1.SN, t1.ExhibitionCode, t1.Exhibitioname_TW, t1.Exhibitioname_CN, t2.ExhibitionNO, t2.Exhibitors, t2.Agent }).MergeTable()
                            .Where(it => it.ExhibitionNO != @"" && it.OrgID == i_crm.ORIGID && (it.Exhibitors.Contains(sId) || it.Agent == sId)).ToList();
                        if (saExport.Count > 0)
                        {
                            foreach (var opm in saExport)
                            {
                                if (!saExhibitions.Any(x => (x[@"SN"].ToString() == opm.SN.ToString())))
                                {
                                    var m = new Map
                                    {
                                        { @"RowIndex", saExhibitions.Count + 1 },
                                        { @"SN", opm.SN },
                                        { @"ExhibitionCode", opm.ExhibitionCode },
                                        { @"Exhibitioname_TW", opm.Exhibitioname_TW },
                                        { @"Exhibitioname_CN", opm.Exhibitioname_CN },
                                        { @"Source", "OTB_OPM_ExportExhibition" }

                                    };
                                    saExhibitions.Add(m);
                                }
                            }
                        }
                        //進口
                        var saImport = db.Queryable<OTB_OPM_Exhibition, OTB_OPM_ImportExhibition>((t1, t2) => t1.SN.ToString() == t2.ExhibitionNO && t1.OrgID == t2.OrgID && t1.Effective == "Y" && t2.IsVoid == "N")
                            .WhereIF(!string.IsNullOrWhiteSpace(sResponsibleDeptID), (t1, t2) => SqlFunc.ContainsArray(ChildDeptIDs, t2.DepartmentID))
                            .Select((t1, t2) => new { t1.OrgID, t1.SN, t1.ExhibitionCode, t1.Exhibitioname_TW, t1.Exhibitioname_CN, t2.ExhibitionNO, t2.Suppliers, t2.Supplier, t2.Agent }).MergeTable()
                            .Where(it => it.ExhibitionNO != @"" && it.OrgID == i_crm.ORIGID && (it.Suppliers.Contains(sId) || it.Supplier == sId || it.Agent == sId)).ToList();
                        if (saImport.Count > 0)
                        {
                            foreach (var opm in saImport)
                            {
                                if (!saExhibitions.Any(x => (x[@"SN"].ToString() == opm.SN.ToString())))
                                {
                                    var m = new Map
                                    {
                                        { @"RowIndex", saExhibitions.Count + 1 },
                                        { @"SN", opm.SN },
                                        { @"ExhibitionCode", opm.ExhibitionCode },
                                        { @"Exhibitioname_TW", opm.Exhibitioname_TW },
                                        { @"Exhibitioname_CN", opm.Exhibitioname_CN },
                                        { @"Source", "OTB_OPM_ImportExhibition" }
                                    };
                                    saExhibitions.Add(m);
                                }
                            }
                        }
                        //其他
                        var saOther = db.Queryable<OTB_OPM_Exhibition, OTB_OPM_OtherExhibition>((t1, t2) => t1.SN.ToString() == t2.ExhibitionNO && t1.OrgID == t2.OrgID && t1.Effective == "Y" && t2.IsVoid == "N")
                            .WhereIF(!string.IsNullOrWhiteSpace(sResponsibleDeptID), (t1, t2) => SqlFunc.ContainsArray(ChildDeptIDs, t2.DepartmentID))
                            .Select((t1, t2) => new { t1.OrgID, t1.SN, t1.ExhibitionCode, t1.Exhibitioname_TW, t1.Exhibitioname_CN, t2.ExhibitionNO, t2.Supplier, t2.Agent }).MergeTable()
                            .Where(it => it.ExhibitionNO != @"" && it.OrgID == i_crm.ORIGID && (it.Supplier == sId || it.Agent == sId)).ToList();
                        if (saOther.Count > 0)
                        {
                            foreach (var opm in saOther)
                            {
                                if (!saExhibitions.Any(x => (x[@"SN"].ToString() == opm.SN.ToString())))
                                {
                                    var m = new Map
                                    {
                                        { @"RowIndex", saExhibitions.Count + 1 },
                                        { @"SN", opm.SN },
                                        { @"ExhibitionCode", opm.ExhibitionCode },
                                        { @"Exhibitioname_TW", opm.Exhibitioname_TW },
                                        { @"Exhibitioname_CN", opm.Exhibitioname_CN },
                                        { @"Source", "OTB_OPM_OtherExhibition" }
                                    };
                                    saExhibitions.Add(m);
                                }
                            }
                        }
                        //其他
                        var saOtherTG = db.Queryable<OTB_OPM_Exhibition, OTB_OPM_OtherExhibitionTG>((t1, t2) => t1.SN.ToString() == t2.ExhibitionNO && t1.OrgID == t2.OrgID && t1.Effective == "Y" && t2.IsVoid == "N")
                            .WhereIF(!string.IsNullOrWhiteSpace(sResponsibleDeptID), (t1, t2) => SqlFunc.ContainsArray(ChildDeptIDs, t2.DepartmentID))
                            .Select((t1, t2) => new { t1.OrgID, t1.SN, t1.ExhibitionCode, t1.Exhibitioname_TW, t1.Exhibitioname_CN, t2.ExhibitionNO, t2.Exhibitors, t2.Agent }).MergeTable()
                            .Where(it => it.ExhibitionNO != @"" && it.OrgID == i_crm.ORIGID && (it.Exhibitors.Contains(sId) || it.Agent == sId)).ToList();
                        if (saOtherTG.Count > 0)
                        {
                            foreach (var opm in saOtherTG)
                            {
                                if (!saExhibitions.Any(x => (x[@"SN"].ToString() == opm.SN.ToString())))
                                {
                                    var m = new Map
                                    {
                                        { @"RowIndex", saExhibitions.Count + 1 },
                                        { @"SN", opm.SN },
                                        { @"ExhibitionCode", opm.ExhibitionCode },
                                        { @"Exhibitioname_TW", opm.Exhibitioname_TW },
                                        { @"Exhibitioname_CN", opm.Exhibitioname_CN },
                                        { @"Source", "OTB_OPM_OtherExhibitionTG" }
                                    };
                                    saExhibitions.Add(m);
                                }
                            }
                        }
                        info.AttendeeTimes = saExhibitions.Count;
                        info.AttendeeExhibitions = saExhibitions.Select(t1 => t1["ExhibitionCode"].ObjToString()).ToList();
                        #endregion
                        if (!string.IsNullOrEmpty(customer.Contactors))
                        {
                            var jaContactors = (JArray)JsonConvert.DeserializeObject(customer.Contactors);
                            var ShowOneContactor = jaContactors.First;
                            if (ShowOneContactor != null)
                            {
                                info.FullName = ShowOneContactor["FullName"]?.ObjToString();
                                info.Email = ShowOneContactor["Email"]?.ObjToString();
                                info.JobtitleName = ShowOneContactor["JobtitleName"]?.ObjToString();
                                info.TEL = ShowOneContactor["TEL1"]?.ObjToString();
                            }
                        }

                        ExistingCustomerInfo.Add(info);
                    }

                    //篩選次數
                    if (!string.IsNullOrWhiteSpace(sStartAttendeeTime))
                        ExistingCustomerInfo = ExistingCustomerInfo.Where(e => e.AttendeeTimes >= iStartAttendeeTime).ToList();
                    if (!string.IsNullOrWhiteSpace(sEndAttendeeTime))
                        ExistingCustomerInfo = ExistingCustomerInfo.Where(e => e.AttendeeTimes <= iEndAttendeeTime).ToList();
                    var cellsApp = new ExcelService(sTempFile);
                    var cells = cellsApp.sheet.Cells;//单元格
                    var iCurrRow = 4;
                    ExistingCustomerInfo = ExistingCustomerInfo.OrderByDescending(e => e.AttendeeTimes).ToList();
                    var CellType = GetCellType(RPTEnum.ExistingCustomer, RoundingPoint, false);
                    foreach (var ECI in ExistingCustomerInfo)
                    {
                        var CellColumns = CalcuCellValue(RPTEnum.ExistingCustomer, ECI);
                        CellsSetValueWithAutoHeight(cellsApp.workbook, cells, iCurrRow, CellColumns, CellType);
                        ++iCurrRow;
                    }
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
                            sMsg = ex.Message;
                            excelApp.Close();
                        }
                    }
                    File.Delete(sTempFile);   //刪除臨時文件
                    sOutPut = sOutPut.Replace(sBase, @"");
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, sOutPut);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(CostAndProfitReportService), "", FunctionName + "（報表模組）", "", "", "");
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

        #endregion
        #region 展覽資訊

        public ResponseMessage ExhibitionInfo(RequestMessage i_crm)
        {
            ShowSource = CommonRPT.RPTShow();
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance(commandTimeOut: TimeOut);
            var FunctionName = nameof(ExhibitionInfo);
            var ExcutePath = CommonRPT.GetExcutePath(FunctionName, "展覽資訊");
            var sOutPut = ExcutePath.Item1;
            var sTempFile = ExcutePath.Item2;
            var sBase = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"");
            var RoundingPoint = CommonRPT.GetRoundingPoint(i_crm.ORIGID);
            try
            {
                do
                {

                    //「展覽名稱」、「展期」、「國家」、「展覽產業別」
                    var sExhibitionName = _fetchString(i_crm, @"ExhibitionName");
                    var sExhibitionCode = _fetchString(i_crm, @"ExhibitionCode");
                    var sExhibitionDateStart = _fetchString(i_crm, @"ExhibitionDateStart");
                    var sExhibitionDateEnd = _fetchString(i_crm, @"ExhibitionDateEnd");
                    var sResponsibleDeptID = _fetchString(i_crm, @"ResponsibleDeptID");
                    var ChildDeptIDs = CommonRPT.GetChildDepteList(db, i_crm.ORIGID, sResponsibleDeptID);
                    var sState = _fetchString(i_crm, @"State");
                    var sIndustry = _fetchString(i_crm, @"Industry");
                    var sFlag = _fetchString(i_crm, @"Flag");
                    var Filter = new CVPFilter();
                    Filter.SetExhibition(sExhibitionDateStart, sExhibitionDateEnd);
                    // => 篩選出展覽code 
                    //
                    var ActiveCustomers = db.Queryable<OTB_CRM_Customers>().Where(t1 => t1.Effective == "Y" && t1.OrgID == i_crm.ORIGID).Select(c => c.guid).ToList();
                    var MatchedExhibitions = db.Queryable<OTB_OPM_Exhibition, OTB_SYS_Arguments, OTB_SYS_Arguments>(((t1, t2, t3) => new object[] {
                               JoinType.Left, t1.OrgID == t2.OrgID && t1.State == t2.ArgumentID && t2.ArgumentClassID == "Area" && t2.DelStatus == "N" ,
                               JoinType.Left, t3.OrgID == "TE" && t1.Industry == t3.ArgumentID && t3.ArgumentClassID == "ExhibClass"&& t2.DelStatus == "N"  ,
                            }
                        ))
                        .Where(t1 => t1.OrgID == i_crm.ORIGID && t1.Effective == "Y")
                        .WhereIF(!string.IsNullOrWhiteSpace(sExhibitionName), t1 =>
                            t1.Exhibitioname_EN.Contains(sExhibitionName) || t1.Exhibitioname_TW.Contains(sExhibitionName) ||
                            t1.Exhibitioname_CN.Contains(sExhibitionName) || t1.ExhibitioShotName_TW.Contains(sExhibitionName) ||
                            t1.ExhibitioShotName_CN.Contains(sExhibitionName) || t1.ExhibitioShotName_EN.Contains(sExhibitionName))
                        .WhereIF(!string.IsNullOrWhiteSpace(sExhibitionCode), t1 => t1.ExhibitionCode == sExhibitionCode)
                        .WhereIF(!string.IsNullOrWhiteSpace(sIndustry), t1 => t1.Industry == sIndustry)
                        .WhereIF(!string.IsNullOrWhiteSpace(sState), t1 => t1.State == sState)
                        .WhereIF(!string.IsNullOrWhiteSpace(Filter.sExhibitionDateStart), t1 => SqlFunc.ToDate(t1.ExhibitionDateStart) >= Filter.rExhibitionDateStart.Date)
                        .WhereIF(!string.IsNullOrWhiteSpace(Filter.sExhibitionDateEnd), t1 => SqlFunc.ToDate(t1.ExhibitionDateStart) < Filter.rExhibitionDateEnd.Date);
                    var ExhibitionList = MatchedExhibitions.Select((t1, t2, t3) =>
                       new OTB_OPM_Exhibition()
                       {
                           Exhibitioname_TW = t1.Exhibitioname_TW,
                           SN = t1.SN,
                           ExhibitionDateStart = t1.ExhibitionDateStart,
                           ExhibitionDateEnd = t1.ExhibitionDateEnd,
                           State = t2.ArgumentValue,
                           Industry = t3.ArgumentValue,
                           ExFeild1 = t1.State,
                           ExFeild2 = t1.Industry,
                       }).ToList();
                    var ExhibitionKeys = string.Join("", ExhibitionList.Select(t1 => "|" + t1.SN + "|").ToList());
                    var ExhibitionDic = ExhibitionList.ToDictionary(t1 => t1.SN.ToString());
                    #region Exhibition(四展覽)

                    var viewExpIm = db.Queryable<OTB_OPM_ImportExhibition, OTB_CRM_Customers>((t1, t2) => new object[] {
                        JoinType.Left,t1.Agent==t2.guid && t1.OrgID==t2.OrgID && t2.Effective == "Y"})
                        .Where(t1 => t1.OrgID == i_crm.ORIGID && t1.IsVoid == "N")
                        .WhereIF(!string.IsNullOrWhiteSpace(sResponsibleDeptID), (t1) => SqlFunc.ContainsArray(ChildDeptIDs, t1.DepartmentID))
                        .Select((t1) => new ExpInfo
                        {
                            ExpNO = t1.ExhibitionNO,
                            ExhibitionType = "ImportExhibition",
                            Exhibitors = "",
                            Organizer = "",
                            Supplier = t1.Supplier,
                            Agent = t1.Agent,
                            ResponsibleMember = t1.ResponsiblePerson,
                            DeptOfResponsibleMember = t1.DepartmentID,
                            OrgID = t1.OrgID
                        });
                    var viewExpEx = db.Queryable<OTB_OPM_ExportExhibition, OTB_CRM_Customers>((t1, t2) => new object[] {
                          JoinType.Left,t1.Agent==t2.guid && t1.OrgID==t2.OrgID && t2.Effective == "Y"})
                        .Where(t1 => t1.OrgID == i_crm.ORIGID && t1.IsVoid == "N")
                        //Organizer、Exhibitors
                        .WhereIF(!string.IsNullOrWhiteSpace(sResponsibleDeptID), (t1) => SqlFunc.ContainsArray(ChildDeptIDs, t1.DepartmentID))
                        .Select((t1) => new ExpInfo
                        {
                            ExpNO = t1.ExhibitionNO,
                            ExhibitionType = "ExportExhibition",
                            Exhibitors = t1.Exhibitors,
                            Organizer = t1.Organizer,
                            Supplier = "",
                            Agent = t1.Agent,
                            ResponsibleMember = t1.ResponsiblePerson,
                            DeptOfResponsibleMember = t1.DepartmentID,
                            OrgID = t1.OrgID
                        });
                    var viewExpOth = db.Queryable<OTB_OPM_OtherExhibition, OTB_CRM_Customers>((t1, t2) => new object[] {
                          JoinType.Left,t1.Agent==t2.guid && t1.OrgID==t2.OrgID && t2.Effective == "Y"})
                        .Where(t1 => t1.OrgID == i_crm.ORIGID && t1.IsVoid == "N")
                        .WhereIF(!string.IsNullOrWhiteSpace(sResponsibleDeptID), (t1) => SqlFunc.ContainsArray(ChildDeptIDs, t1.DepartmentID))
                        .Select((t1) => new ExpInfo
                        {
                            ExpNO = t1.ExhibitionNO,
                            ExhibitionType = "OtherExhibition",
                            Exhibitors = "",
                            Organizer = "",
                            Supplier = t1.Supplier,
                            Agent = t1.Agent,
                            ResponsibleMember = t1.ResponsiblePerson,
                            DeptOfResponsibleMember = t1.DepartmentID,
                            OrgID = t1.OrgID
                        });
                    var viewExpOthtG = db.Queryable<OTB_OPM_OtherExhibitionTG, OTB_CRM_Customers>((t1, t2) => new object[] {
                          JoinType.Left,t1.Agent==t2.guid && t1.OrgID==t2.OrgID && t2.Effective == "Y"})
                        .Where(t1 => t1.OrgID == i_crm.ORIGID && t1.IsVoid == "N")
                        .WhereIF(!string.IsNullOrWhiteSpace(sResponsibleDeptID), (t1) => SqlFunc.ContainsArray(ChildDeptIDs, t1.DepartmentID))
                        .Select((t1) => new ExpInfo
                        {
                            ExpNO = t1.ExhibitionNO,
                            ExhibitionType = "OtherExhibitionTG",
                            Exhibitors = t1.Exhibitors,
                            Organizer = "",
                            Supplier = "",
                            Agent = t1.Agent,
                            ResponsibleMember = t1.ResponsiblePerson,
                            DeptOfResponsibleMember = t1.DepartmentID,
                            OrgID = t1.OrgID
                        });
                    var saExps = db.UnionAll(viewExpIm, viewExpEx, viewExpOth, viewExpOthtG).ToList();

                    #endregion

                    var cellsApp = new ExcelService(sTempFile);
                    var cells = cellsApp.sheet.Cells;//单元格

                    var iCurrRow = 1;
                    var ExhibitionGroup = saExps.Where(c => ExhibitionKeys.Contains("|" + c.ExpNO.Trim() + "|"))
                        .GroupBy(c => c.ExpNO).OrderBy(c => c.Key);
                    var CellType = GetCellType(RPTEnum.ExhibitionInfo, RoundingPoint, ShowSource);
                    if (ExhibitionGroup.Any())
                    {
                        var DeptDic = CommonRPT.GetDeptInfos(db, i_crm.ORIGID);

                        foreach (var Exhibitions in ExhibitionGroup)
                        {
                            var ExhibitionCode = Exhibitions.First().ExpNO;
                            ExhibitionDic.TryGetValue(ExhibitionCode, out var ExhibitionInfo);
                            var AgentCount = Exhibitions.Where(c => !string.IsNullOrWhiteSpace(c.Agent) && ActiveCustomers.Any(ac => ac == c.Agent))
                                .Select(c => c.Agent).Distinct().Count();
                            var OrganizerList = new List<string>();
                            var SuppliersList = Exhibitions.Where(c => !string.IsNullOrWhiteSpace(c.Supplier))
                                .Select(c => c.Supplier.Trim()).ToList();
                            var Organizers = Exhibitions.Where(c => !string.IsNullOrWhiteSpace(c.Organizer)).Select(c => c.Organizer).ToList();
                            var Exhibitors = Exhibitions.Where(c => !string.IsNullOrWhiteSpace(c.Exhibitors)).Select(c => c.Exhibitors).ToList();
                            //組團單位數
                            if (Organizers.Any())
                            {
                                var SingleOrganizers = Organizers.Where(c => c.Length == 36).ToList();
                                OrganizerList.AddRange(SingleOrganizers);
                                //出口展覽的組團單位包含舊資料(guid)、新資料(json後的guids) 
                                var MultipleOrganizer = Organizers.Where(c => c.Length > 36).ToList();
                                foreach (var sOrganizer in MultipleOrganizer)
                                {
                                    var jaContactors = (JArray)JsonConvert.DeserializeObject(sOrganizer);
                                    foreach (var jaContactor in jaContactors)
                                    {
                                        var sContactor = jaContactor.ObjToString();
                                        if (!OrganizerList.Any(c => c == sContactor))
                                        {
                                            OrganizerList.Add(sContactor);
                                        }
                                    }
                                }
                            }
                            //參加廠商家數
                            if (Exhibitors.Any())
                            {
                                var ExhibitorsList = new List<string>() { };
                                foreach (var sExhibitor in Exhibitors)
                                {
                                    var jaExhibitors = (JArray)JsonConvert.DeserializeObject(sExhibitor);
                                    var SupplierIDs = jaExhibitors.Where(c => !string.IsNullOrWhiteSpace(c["SupplierID"].ObjToString())).Select(c => c["SupplierID"].ToString()).Distinct().ToList();
                                    ExhibitorsList.AddRange(SupplierIDs);
                                }
                                SuppliersList.AddRange(ExhibitorsList);
                            }
                            //依序:展覽名稱 展期  國家 展覽產業別   組團公司 國外代理    部門 組別  負責業務 參展廠商家數
                            #region 取相關業務資料

                            var ResponsibleMembersList = Exhibitions.Select(c => c.ResponsibleMember).Distinct().ToList();
                            var DeptOfResponsibleMembers = Exhibitions.Select(c => c.DeptOfResponsibleMember).Distinct().ToList();
                            var TotalResponsibleMembers = string.Join(",", ResponsibleMembersList);
                            var ResDeptID = "";
                            var UpperDeptID = "";
                            if (ResponsibleMembersList.Count >= 2 && DeptOfResponsibleMembers.Count > 2)
                            {
                                UpperDeptID = DeptOfResponsibleMembers.Count.ToString();
                            }
                            else
                            {
                                //只有兩層代表是只顯示部門
                                if (DeptDic.TryGetValue(DeptOfResponsibleMembers.First(), out var DeptInfo))
                                {
                                    if (DeptInfo.Item5 == "2")
                                    {
                                        ResDeptID = DeptInfo.Item2;
                                        UpperDeptID = DeptInfo.Item4;
                                    }
                                    else
                                    {
                                        UpperDeptID = DeptInfo.Item2;
                                    }
                                }
                            }

                            #endregion
                            var DateStart = ExhibitionInfo.ExhibitionDateStart.HasValue ? ExhibitionInfo.ExhibitionDateStart.Value.ToString("yyyy/MM/dd") : "";
                            var DateEnd = ExhibitionInfo.ExhibitionDateEnd.HasValue ? ExhibitionInfo.ExhibitionDateEnd.Value.ToString("yyyy/MM/dd") : "";
                            OrganizerList = OrganizerList.Distinct().ToList();
                            SuppliersList = SuppliersList.Distinct().ToList();
                            var SummaryInfo = new List<object>
                            {
                                ExhibitionInfo.Exhibitioname_TW.ObjToString() ?? "",
                                DateStart + "-" + DateEnd,
                                ExhibitionInfo.State.ObjToString() ?? "",
                                ExhibitionInfo.Industry.ObjToString() ?? "",
                                OrganizerList.Count,
                                AgentCount.ObjToInt(),
                                UpperDeptID.ObjToString(),
                                ResDeptID.ObjToString(),
                                TotalResponsibleMembers.ObjToString(),
                                SuppliersList.Count
                            };
                            CellsSetValueWithAutoHeight(cellsApp.workbook, cells, iCurrRow, SummaryInfo, CellType);
                            ++iCurrRow;
                        }
                    }
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
                            rm = new SuccessResponseMessage(null, i_crm);
                            sMsg = ex.Message;
                            excelApp.Close();
                        }
                    }
                    File.Delete(sTempFile);   //刪除臨時文件
                    sOutPut = sOutPut.Replace(sBase, @"");
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, sOutPut);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(CostAndProfitReportService), "", FunctionName + "（報表模組）", "", "", "");
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

        #endregion

        #region 展覽資訊

        public ResponseMessage CostFeeItemReport(RequestMessage i_crm)
        {
            ShowSource = CommonRPT.RPTShow();
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance(commandTimeOut: TimeOut);
            var FunctionName = nameof(CostFeeItemReport);
            var ExcutePath = CommonRPT.GetExcutePath(FunctionName, "成本費用報表");
            var sOutPut = ExcutePath.Item1;
            var sTempFile = ExcutePath.Item2;
            var sBase = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"");
            var RoundingPoint = CommonRPT.GetRoundingPoint(i_crm.ORIGID);
            try
            {
                do
                {
                    //「展覽名稱」(下拉)、「展期」、「部門」、「費用項目」、
                    var sExhibitionCode = _fetchString(i_crm, @"ProjectNO");
                    var sExhibitionName = "";
                    var sExhibitionDateStart = _fetchString(i_crm, @"ExhibitionDateStart");
                    var sExhibitionDateEnd = _fetchString(i_crm, @"ExhibitionDateEnd");
                    var sResponsibleDeptID = _fetchString(i_crm, @"ResponsibleDeptID");
                    var sFeeClass = _fetchString(i_crm, @"FeeClass");
                    var ChildDeptIDs = CommonRPT.GetChildDepteList(db, i_crm.ORIGID, sResponsibleDeptID);
                    var sFlag = _fetchString(i_crm, @"Flag");
                    var Filter = new CVPFilter();
                    Filter.SetExhibition(sExhibitionDateStart, sExhibitionDateEnd);
                    // => 篩選出展覽code 
                    var MatchedExhibitions = db.Queryable<OTB_OPM_Exhibition>()
                        .Where(t1 => t1.OrgID == i_crm.ORIGID && t1.Effective == "Y")
                        .WhereIF(!string.IsNullOrWhiteSpace(sExhibitionCode), t1 => t1.ExhibitionCode == sExhibitionCode)
                        .WhereIF(!string.IsNullOrWhiteSpace(Filter.sExhibitionDateStart), t1 => SqlFunc.ToDate(t1.ExhibitionDateStart) >= Filter.rExhibitionDateStart.Date)
                        .WhereIF(!string.IsNullOrWhiteSpace(Filter.sExhibitionDateEnd), t1 => SqlFunc.ToDate(t1.ExhibitionDateStart) < Filter.rExhibitionDateEnd.Date);
                    var ExpSNs = MatchedExhibitions
                        .Select(t1 => t1.SN.ToString()).ToList().ToArray();
                    var saActualCosts = db.Queryable<OVW_OPM_ALLExps>()
                        .Where((t1) => t1.OrgID == i_crm.ORIGID)
                        .WhereIF(!string.IsNullOrWhiteSpace(sExhibitionCode) || !string.IsNullOrWhiteSpace(Filter.sExhibitionDateStart)
                         || !string.IsNullOrWhiteSpace(Filter.sExhibitionDateEnd), t1 => SqlFunc.ContainsArray(ExpSNs, t1.ExhibitionSN))
                        .WhereIF(!string.IsNullOrWhiteSpace(sResponsibleDeptID), t1 => SqlFunc.ContainsArray(ChildDeptIDs, t1.DepartmentID))
                        .ToList();
                    if (!string.IsNullOrWhiteSpace(sExhibitionCode))
                    {
                        sExhibitionName = db.Queryable<OTB_OPM_Exhibition>()
                            .Where(t1 => t1.OrgID == "TE" && t1.Effective == "Y" && t1.ExhibitionCode == sExhibitionCode)
                            .Select(t1 => t1.Exhibitioname_TW).First();
                    }


                    var ActualCostSummary = db.Queryable<OTB_SYS_Arguments>()
                        .Where(t1 => t1.OrgID == i_crm.ORIGID && t1.ArgumentClassID == "FeeClass" && t1.DelStatus == "N")
                        .WhereIF(!string.IsNullOrWhiteSpace(sFeeClass), t1 => t1.ArgumentID == sFeeClass)
                        .Select(t1 => new CostFeeItem
                        {
                            ArgumentID = t1.ArgumentID,
                            ArgumentDescription = t1.ArgumentValue,
                        })
                        .ToList().Where(t1 => t1.ArgumentID.IndexOf("99") == 0).ToList();
                    var ToAnalysisList = saActualCosts.Select(t1 => t1.ActualCost).ToList();
                    foreach (var ReturnBill in saActualCosts.Select(t1 => t1.ReturnBills).ToList())
                    {
                        var JADatas = JArray.Parse(ReturnBill);
                        if (JADatas.Any())
                        {
                            foreach (var jToken in JADatas)
                            {
                                var sActualCost = jToken["ActualCost"];
                                if (sActualCost != null)
                                {
                                    ToAnalysisList.Add(sActualCost.ToString());
                                }
                            }
                        }
                    }
                    foreach (string sJsons in ToAnalysisList)
                    {
                        var ActualCostBills = JToken.Parse(sJsons);
                        var ActualCosts = ActualCostBills["FeeItems"];
                        if (ActualCostBills.Any() && ActualCosts != null && ActualCosts.Any())
                        {
                            foreach (var ActualCost in ActualCosts)
                            {
                                var FinancialCode = ActualCost["FinancialCode"].ObjToString();
                                var AlreadyExist = ActualCostSummary.FirstOrDefault(c => c.ArgumentID == FinancialCode);
                                if (AlreadyExist != null)
                                {
                                    AlreadyExist.Amount += ActualCost["FinancialTWAmount"].ObjToDecimal();
                                }
                            }
                        }
                    }
                    var CellType = GetCellType(RPTEnum.CostFeeItemReport, RoundingPoint);
                    var cellsApp = new ExcelService(sTempFile);
                    var cells = cellsApp.sheet.Cells;//单元格
                    var iCurrRow = 5;
                    cells[1, 0].PutValue("展覽區間:" + sExhibitionDateStart + "~" + sExhibitionDateEnd);
                    cells[1, 2].PutValue("部門:" + string.Join(",", ChildDeptIDs));
                    cells[2, 0].PutValue("展覽名稱:" + sExhibitionName);
                    cells[2, 2].PutValue("列表人:" + i_crm.USERID);
                    cells[3, 2].PutValue("列印時間:" + DateTime.Now.ToString(@"yyyy/MM/dd"));
                    foreach (var costFee in ActualCostSummary)
                    {
                        var CellColumns = CalcuCellValue(RPTEnum.CostFeeItemReport, costFee);
                        CellsSetValue(cellsApp.workbook, cells, iCurrRow, CellColumns, CellType);
                        ++iCurrRow;
                    }
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
                            sMsg = ex.Message;
                            excelApp.Close();
                        }
                    }
                    File.Delete(sTempFile);   //刪除臨時文件
                    sOutPut = sOutPut.Replace(sBase, @"");
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, sOutPut);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(CostAndProfitReportService), "", FunctionName + "（報表模組）", "", "", "");
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

        #endregion
        private void CellsSetValueWithAutoHeight(Aspose.Cells.Workbook workbook, Cells cells, int irow, List<object> values, List<int> NumberTypes = null)
        {
            var DefaultHeight = 20;
            var LengthList = values.Select(t1 =>
            {
                var st1 = t1.ObjToString();
                if (!string.IsNullOrWhiteSpace(st1))
                {
                    byte[] Dbyte = System.Text.Encoding.Default.GetBytes(st1);
                    return Math.Ceiling((Dbyte.Length / 10.0)) * 10 + 10;
                }
                else
                    return DefaultHeight;
            });
            CellsSetValue(workbook, cells, irow, values, NumberTypes, LengthList.Max());
        }
        
        private void CellsSetValue(Aspose.Cells.Workbook workbook, Cells cells, int irow, List<object> values, List<int> NumberTypes = null, double SetRowHeight = 20)
        {
            var styleForNumber = GetStyle(workbook, 12, false, TextAlignmentType.Right, Color.White, true);
            var styleForText = GetStyle(workbook, 12, false, TextAlignmentType.Left, Color.White, true);
            styleForNumber.Number = 4;
            cells.SetRowHeight(irow, SetRowHeight);
            var SetRowValueNumber = NumberTypes != null && values.Count == NumberTypes.Count;
            for (int i = 0; i < values.Count; i++)
            {
                if (values[i] is string)
                {
                    cells[irow, i].PutValue(values[i]);
                    cells[irow, i].SetStyle(styleForText);
                }
                else
                {
                    cells[irow, i].PutValue(values[i]);
                    if (SetRowValueNumber && NumberTypes[i] != -1)
                    {
                        styleForNumber.Number = NumberTypes[i];
                    }
                    cells[irow, i].SetStyle(styleForNumber);
                }

            }
        }


        public List<object> CalcuCellValue(RPTEnum type, object Data, int RoundingPoint = 0)
        {
            switch (type)
            {
                case RPTEnum.CVPAnalysisBySaler:
                case RPTEnum.CVPAnalysisByExhibition:
                    {
                        ProfitInfo profitInfo = (ProfitInfo)Data;
                        return new List<object>
                        {
                            profitInfo.MemberID,//名稱
                            profitInfo.BillUntaxAmt,//收入(A)
                            profitInfo.SharedActualCost,//成本 (B)
                            profitInfo.GrossProfit,//毛利(C)=(A)-(B)
                            profitInfo.GrossProfitPercent, //毛利率(C)/(A)
                            profitInfo.BillReimburseAmount,//帳單代墊款(D)
                            profitInfo.ActualBillReimburseAmount, //實際代墊款(E)
                            profitInfo.NetIncome,//淨收入(F) =(A)-(D)
                            profitInfo.NetCost, //淨成本(G) =(B)-(E)
                            profitInfo.NetProfit, //淨毛利(H)=(F)-(G)
                            profitInfo.NetProfitPercent//淨毛利率(H)/(F)
                        };
                    }
                case RPTEnum.CashFlow:
                    break;
                case RPTEnum.DegreeOfContributionByCustomer:
                case RPTEnum.DegreeOfContributionByAgent:
                    {
                        ProfitInfo profitInfo = (ProfitInfo)Data;
                        return new List<object>()
                        {
                            profitInfo.CustomerName,//(代理/客戶)簡稱
                            profitInfo.ExField,//次數
                            profitInfo.Weight,//重量
                            CommonRPT.Rounding(profitInfo.Volume, RoundingPoint),//材積
                            CommonRPT.Rounding(profitInfo.BillUntaxAmt, RoundingPoint),//收入(A)
                            CommonRPT.Rounding(profitInfo.GrossProfit, RoundingPoint),//毛利(C)=(A)-(B)
                            CommonRPT.Rounding(profitInfo.NetIncome, RoundingPoint),//淨收入(F) =(A)-(D)
                            CommonRPT.Rounding(profitInfo.NetProfit, RoundingPoint), //淨毛利(H)=(F)-(G)
                        };
                    }
                case RPTEnum.ExistingCustomer:
                    {
                        ExistingCustomerInfo eci = (ExistingCustomerInfo)Data;
                        return new List<object>()
                        {
                            eci.ChName ,
                            eci.EnName ,
                            eci.TaxNumber ,
                            eci.TransType ,
                            eci.State ,
                            eci.AttendeeTimes,
                            eci.FullName ,
                            eci.JobtitleName ,
                            eci.TEL ,
                            eci.Email ,
                            eci.Address ,
                            eci.Website ,
                        };
                    }
                case RPTEnum.ExhibitionInfo:
                    break;
                case RPTEnum.CostFeeItemReport:
                    {
                        CostFeeItem CFI = (CostFeeItem)Data;
                        return new List<object>()
                        {
                            CFI.ArgumentID,
                            CFI.ArgumentDescription,
                            CFI.Amount
                        };
                    }
                default:
                    break;
            }
            return new List<object> { };
        }

        public List<int> GetCellType(RPTEnum type, int RoundingPoint = 0, bool showSource = false)
        {
            //Ref:https://docs.aspose.com/display/cellsnet/List+of+Supported+Number+Formats
            var CurrencyType = 5;
            if (RoundingPoint != 0)
                CurrencyType = 4;
            var Types = new List<int>();
            switch (type)
            {
                case RPTEnum.CVPAnalysisBySaler:
                case RPTEnum.CVPAnalysisByExhibition:
                    {
                        Types = new List<int>
                        {
                            -1,//名稱
                            CurrencyType,//收入(A)
                            CurrencyType,//成本 (B)
                            CurrencyType,//毛利(C)=(A)-(B)
                            10, //毛利率(C)/(A)
                            CurrencyType,//帳單代墊款(D)
                            CurrencyType, //實際代墊款(E)
                            CurrencyType,//淨收入(F) =(A)-(D)
                            CurrencyType, //淨成本(G) =(B)-(E)
                            CurrencyType, //淨毛利(H)=(F)-(G)
                            10//淨毛利率(H)/(F)
                        };
                    }
                    break;
                case RPTEnum.CashFlow:
                    {
                        Types = new List<int>
                        {
                            -1,//帳單號碼
                            -1,//帳單狀態
                            -1,//銷帳日期
                            -1,//帳單拋轉日期
                            -1,//帳單創建日期
                            -1,//專案代號
                            -1,//展覽名稱
                            -1,//展覽簡稱
                            -1,//展覽類別
                            -1,//展期第一天
                            -1,//組團單位
                            -1,//客戶編號
                            -1,//客戶中文名
                            -1,//客戶英文名
                            -1,//客戶簡稱
                            -1,//國內/國外
                            -1,//交易型態
                            -1,//負責部門
                            -1,//負責組別
                            -1,//負責業務
                            4,//材積(CBM)
                            -1,//帳單幣別
                            4,//匯率
                            4,//未稅金額（原幣別）
                            CurrencyType,//未稅金額（收入）
                            CurrencyType,//稅額
                            CurrencyType,//成本
                            CurrencyType,//帳單代墊款
                            CurrencyType,//實際代墊款
                            CurrencyType,//毛利
                            CurrencyType,//淨毛利
                        };
                    }
                    break;
                case RPTEnum.DegreeOfContributionByCustomer:
                case RPTEnum.DegreeOfContributionByAgent:
                    {
                        Types = new List<int>
                        {
                            -1,//(代理/客戶)簡稱
                            1,//次數
                            4,//重量
                            4,//材積
                            CurrencyType,//收入(A)
                            CurrencyType,//毛利(C)=(A)-(B)
                            CurrencyType,//淨收入(F) =(A)-(D)
                            CurrencyType, //淨毛利(H)=(F)-(G)
                        };
                    }
                    break;
                case RPTEnum.ExistingCustomer:
                    {

                        Types = new List<int>
                        {
                            -1,//客戶中文名稱
                            -1,//客戶英文名稱
                            -1,//統一編號
                            -1,//交易型態
                            -1,//國家
                             1,//參展次數
                            -1,  //聯絡人
                            -1, //職稱
                            -1, //電話
                            -1, //EMAIL
                            -1, //地址
                            -1, //網址
                        };
                    }
                    break;
                case RPTEnum.ExhibitionInfo:
                    {
                        Types = new List<int>
                        {
                            -1,//展覽名稱
                            -1 ,//展期
                            -1 ,//國家
                            -1 ,//展覽產業別
                            1 ,//組團公司(數字)
                            1,//國外代理(數字)
                            -1,//部門
                            -1 ,//組別
                            -1 ,//負責業務
                            1 ,//參展廠商家數
                        };
                    }
                    break;
                case RPTEnum.CostFeeItemReport:
                    {
                        Types = new List<int>
                        {
                            -1,//參數值
                            -1 ,//費用項目
                            CurrencyType//金額(NT$)
                        };
                    }
                    break;
                default:
                    break;
            }
            if (showSource)
            {
                Types.Add(-1);
            }
            return Types;
        }
        /// <summary>
        /// 固定的樣式
        /// </summary>
        /// <param name="sFontSize"></param>
        /// <param name="bIsBold"></param>
        /// <param name="sAlign"></param>
        /// <param name="sBgColor"></param>
        /// <param name="bIsWrap"></param>
        /// <param name="workbook"></param>
        /// <returns></returns>
        public static Aspose.Cells.Style GetStyle(Aspose.Cells.Workbook workbook, int sFontSize, bool bIsBold, TextAlignmentType sAlign, Color sBgColor, bool bIsWrap)
        {
            var style = workbook.CreateStyle();
            style.HorizontalAlignment = sAlign;//文字居左/中/右 ---TextAlignmentType.Center
            style.VerticalAlignment = TextAlignmentType.Top;
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

        public void CalcAttendeeAsCustomerGuid(string ExhibitionNO, string CustomerGuid, List<ExistingCustomerInfo> existingCustomerInfos)
        {
            if (!string.IsNullOrWhiteSpace(CustomerGuid))
            {
                var ECI = existingCustomerInfos.FirstOrDefault(c => c.GUID == CustomerGuid);
                if (ECI != null && !ECI.AttendeeExhibitions.Any(e => e == ExhibitionNO))
                {
                    ECI.AttendeeExhibitions.Add(ExhibitionNO);
                    ++ECI.AttendeeTimes;

                }
            }
        }

    }
}