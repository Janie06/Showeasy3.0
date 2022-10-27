using Aspose.Cells;
using EasyBL.WebApi.Message;
using Entity.Sugar;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SqlSugar;
using SqlSugar.Base;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using EasyBL;
using Entity.ViewModels;

namespace EasyBL.WEBAPP.CRM
{
    public class SatisfactionCase_UpdService : ServiceBase
    {
        #region 新增滿意度案件
        /// <summary>
        /// 新增滿意度案件
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on InsertImportCustomers</param>
        /// <returns></returns>
        public ResponseMessage Add(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    OTB_CRM_SatisfactionCase oSatisfactionCase = _fetchEntity<OTB_CRM_SatisfactionCase>(i_crm);

                    var iRel = db.Insertable(oSatisfactionCase).ExecuteReturnIdentity();
                        //.ExecuteCommand();

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, iRel);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(SatisfactionCase_UpdService), @"滿意度案件", @"Add（新增滿意度案件）", @"", @"", @"");
            }
            finally
            {
                if (null != sMsg)
                {
                    if (i_crm.LANG == @"zh")
                    {
                        sMsg = ChineseStringUtility.ToSimplified(sMsg);
                    }
                    rm = new ErrorResponseMessage(sMsg, i_crm);
                }
            }
            return rm;
        }
        #endregion

        #region 匯入問卷
        /// <summary>
        /// 匯入問卷
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on ImportCustomers</param>
        /// <returns></returns>
        public ResponseMessage ImportFile(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    int iRel = 0;

                    var sFileId = _fetchString(i_crm, @"FileId");
                    var sFileName = _fetchString(i_crm, @"FileName");
                    var iSN = _fetchString(i_crm, @"SN");
                    var sRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"OutFiles\Temporary\");//Word模版路徑
                    var sfileName = sFileName.Split(new string[] { @"." }, StringSplitOptions.RemoveEmptyEntries);
                    var sSubFileName = sfileName.LastOrDefault();     //副檔名
                    sFileName = sRoot + sFileId + @"." + sSubFileName;

                    var book = new Workbook(sFileName);

                    var sheet = book.Worksheets[0];
                    var cells = sheet.Cells;
                    var tbFeeItems = cells.ExportDataTableAsString(1, 0, cells.MaxDataRow, cells.MaxDataColumn + 1, false);

                    int i = 1;

                    if (tbFeeItems.Rows.Count > 0)
                    {
                        List<OTB_CRM_SatisfactionCustomer> listSatisfactionCustomer = new List<OTB_CRM_SatisfactionCustomer>();
                        foreach (DataRow row in tbFeeItems.Rows)
                        {
                            try
                            {
                                if (i == 1)
                                {
                                    OTB_CRM_SatisfactionCustomer oSatisfactionCustomer = new OTB_CRM_SatisfactionCustomer();
                                    oSatisfactionCustomer.CaseSN = iSN;
                                    oSatisfactionCustomer.CompareDB = "N";
                                    oSatisfactionCustomer.CustomerName = row[@"Column1"].ToString();// 客戶名稱
                                    oSatisfactionCustomer.FillerName = row[@"Column2"].ToString();// 填寫人名稱
                                    oSatisfactionCustomer.Phone = row[@"Column4"].ToString();// 聯絡電話
                                    oSatisfactionCustomer.Email = row[@"Column3"].ToString();// EMAIL
                                    oSatisfactionCustomer.Feild01 = row[@"Column5"].ToString();// 奕達提供整體服務品質的滿意度？
                                    oSatisfactionCustomer.Feild02 = row[@"Column6"].ToString();// 奕達提供的價格是否合理？
                                    oSatisfactionCustomer.Feild03 = row[@"Column7"].ToString();// 展品送達時間是否滿意？
                                    oSatisfactionCustomer.Feild04 = row[@"Column8"].ToString();// 現場人員的專業技能與服務態度是否滿意？
                                    oSatisfactionCustomer.Feild05 = row[@"Column9"].ToString();// 承辦同仁的配合度及服務態度是否滿意？
                                    oSatisfactionCustomer.Feild06 = row[@"Column10"].ToString();// 「貨況線上查詢系統」是否滿意？
                                    oSatisfactionCustomer.Feild07 = row[@"Column12"].ToString();// 為何選擇奕達？
                                    oSatisfactionCustomer.Feild08 = row[@"Column13"].ToString();// 貴公司年度平均參與海外展會活動次數？
                                    oSatisfactionCustomer.Feild09 = row[@"Column16"].ToString();// 您是否會推薦奕達給合作夥伴？
                                    oSatisfactionCustomer.Feild10 = row[@"Column15"].ToString();// 其他建議
                                    oSatisfactionCustomer.Memo = "";
                                    oSatisfactionCustomer.CreateDate = DateTime.Now;
                                    oSatisfactionCustomer.CreateUser = i_crm.USERID;

                                    listSatisfactionCustomer.Add(oSatisfactionCustomer);

                                }

                            }
                            catch(Exception ex)
                            {

                            }
                        }
                        if (listSatisfactionCustomer.Count > 0)
                        {
                            iRel = db.Deleteable<OTB_CRM_SatisfactionCustomer>().Where(x => x.CaseSN == iSN).ExecuteCommand();
                            iRel = db.Insertable(listSatisfactionCustomer).ExecuteCommand();
                        }
                    }
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, iRel);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(SatisfactionCase_UpdService), @"滿意度案件", @"ImportFile（匯入問卷）", @"", @"", @"");
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

        #region 單筆修改
        /// <summary>
        /// 單筆修改
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on ImportCustomers</param>
        /// <returns></returns>
        public ResponseMessage Update(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    OTB_CRM_SatisfactionCustomer oSatisfactionCustomer = _fetchEntity<OTB_CRM_SatisfactionCustomer>(i_crm);

                    oSatisfactionCustomer.ModifyDate = DateTime.Now;
                    oSatisfactionCustomer.ModifyUser = i_crm.USERID;

                    var iSN = _fetchString(i_crm, @"SN");

                    var iRel = db.Updateable(oSatisfactionCustomer).UpdateColumns(
                        x => new
                        {
                            x.CustomerName,
                            x.FillerName,
                            x.Email,
                            x.Phone,
                            x.Memo,
                            x.ModifyUser,
                            x.ModifyDate
                        }).Where(x => x.SN == iSN).ExecuteCommand();

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, iRel);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(SatisfactionCase_UpdService), @"滿意度案件", @"Update（單筆修改）", @"", @"", @"");
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

        #region 刪除滿意度案件資料
        /// <summary>
        /// 刪除滿意度案件資料
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on InsertImportCustomers</param>
        /// <returns></returns>
        public ResponseMessage Delete(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    string sSN = _fetchString(i_crm, @"SN");

                    var iRel = db.Deleteable<OTB_CRM_SatisfactionCase>().Where(x => x.SN == sSN).ExecuteCommand();
                    iRel = db.Deleteable<OTB_CRM_SatisfactionCustomer>().Where(x => x.CaseSN == sSN).ExecuteCommand();

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, iRel);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(SatisfactionCase_UpdService), @"滿意度案件", @"Delete（刪除滿意度案件資料）", @"", @"", @"");
            }
            finally
            {
                if (null != sMsg)
                {
                    if (i_crm.LANG == @"zh")
                    {
                        sMsg = ChineseStringUtility.ToSimplified(sMsg);
                    }
                    rm = new ErrorResponseMessage(sMsg, i_crm);
                }
            }
            return rm;
        }
        #endregion

        #region 刪除滿意度案件問卷資料
        /// <summary>
        /// 刪除滿意度案件問卷資料
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on InsertImportCustomers</param>
        /// <returns></returns>
        public ResponseMessage DeleteSatisfactionCustomer(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    string sSN = _fetchString(i_crm, @"SN");

                    var iRel = db.Deleteable<OTB_CRM_SatisfactionCustomer>().Where(x => x.SN == sSN).ExecuteCommand();

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, iRel);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(SatisfactionCase_UpdService), @"滿意度案件", @"DeleteSatisfactionCustomer（刪除滿意度案件問卷資料）", @"", @"", @"");
            }
            finally
            {
                if (null != sMsg)
                {
                    if (i_crm.LANG == @"zh")
                    {
                        sMsg = ChineseStringUtility.ToSimplified(sMsg);
                    }
                    rm = new ErrorResponseMessage(sMsg, i_crm);
                }
            }
            return rm;
        }
        #endregion

        #region 抓取滿意度案件問卷資料
        /// <summary>
        /// 抓取滿意度案件問卷資料
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on InsertImportCustomers</param>
        /// <returns></returns>
        public ResponseMessage GetSatisfactionCaseData(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    string sSN = _fetchString(i_crm, @"SN");

                    var iRel = db.Queryable<OTB_CRM_SatisfactionCustomer>().Single(x => x.SN == sSN);

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, iRel);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(SatisfactionCase_UpdService), @"滿意度案件", @"GetSatisfactionCaseData（抓取滿意度案件問卷資料）", @"", @"", @"");
            }
            finally
            {
                if (null != sMsg)
                {
                    if (i_crm.LANG == @"zh")
                    {
                        sMsg = ChineseStringUtility.ToSimplified(sMsg);
                    }
                    rm = new ErrorResponseMessage(sMsg, i_crm);
                }
            }
            return rm;
        }
        #endregion

        #region 抓取滿意度案件問卷列表資料
        /// <summary>
        /// 抓取滿意度案件問卷列表資料
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage GetSatisfactionList(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var pml = new PageModel
                    {
                        PageIndex = _fetchInt(i_crm, @"pageIndex"),
                        PageSize = _fetchInt(i_crm, @"pageSize")
                    };
                    var iPageCount = 0;
                    var sSortField = _fetchString(i_crm, @"sortField");
                    var sSortOrder = _fetchString(i_crm, @"sortOrder");
                    var bExcel = _fetchBool(i_crm, @"Excel");
                    var sExcelType = _fetchString(i_crm, @"ExcelType");

                    var saSatisfactionCase = new List<Dictionary<string, object>>();
                    var sSN = _fetchString(i_crm, @"SN");

                    pml.DataList = db.Queryable<OTB_CRM_SatisfactionCustomer>()
                         .Where(it => it.CaseSN == sSN)
                         .OrderBy(sSortField, sSortOrder)
                        .ToPageList(pml.PageIndex, bExcel ? 100000 : pml.PageSize, ref iPageCount); ;

                    pml.Total = iPageCount;

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, pml);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(SatisfactionCase_UpdService), @"滿意度案件", "GetSatisfactionList（抓取滿意度案件問卷列表資料）", "", "", "");
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

        #region 重新比對
        /// <summary>
        /// 重新比對
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage CompareDB(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var pml = new PageModel
                    {
                        PageIndex = _fetchInt(i_crm, @"pageIndex"),
                        PageSize = _fetchInt(i_crm, @"pageSize")
                    };
                    var iPageCount = 0;
                    var sSortField = _fetchString(i_crm, @"sortField");
                    var sSortOrder = _fetchString(i_crm, @"sortOrder");
                    var bExcel = _fetchBool(i_crm, @"Excel");
                    var sExcelType = _fetchString(i_crm, @"ExcelType");

                    var saSatisfactionCase = new List<Dictionary<string, object>>();
                    var sSN = _fetchString(i_crm, @"SN");

                    var listSatisfactionCustomer = db.Queryable<OTB_CRM_SatisfactionCustomer, OTB_CRM_Customers>
                        ((t1, t2) =>
                            new object[] {
                                JoinType.Left, t1.CustomerName == t2.CustomerCName
                            }
                        )
                        .Select((t1, t2) => new View_CRM_SatisfactionCustomer
                        {
                            guid = t2.guid,
                            SN = t1.SN,
                            CaseSN = t1.CaseSN,
                            //CompareDB = SqlFunc.IF(t2.guid != "").Return("Y").End("N"),
                            CompareDB = t1.CompareDB,
                            CustomerName = t1.CustomerName,
                            FillerName = t1.FillerName,
                            Phone = t1.Phone,
                            Email = t1.Email,
                            Feild01 = t1.Feild01
                        })
                        .MergeTable()
                        .Where(x => x.CaseSN == sSN && x.guid != null).ToList();

                    foreach (View_CRM_SatisfactionCustomer data in listSatisfactionCustomer)
                    {
                        OTB_CRM_SatisfactionCustomer oSatisfactionCustomer =  new OTB_CRM_SatisfactionCustomer();

                        oSatisfactionCustomer.CustomerID = data.guid;
                        oSatisfactionCustomer.CompareDB = "Y";

                        var iRel = db.Updateable(oSatisfactionCustomer)
                                .UpdateColumns(
                                x => new {
                                    x.CompareDB,
                                    x.CustomerID
                                })
                                .Where(x => x.SN == data.SN).ExecuteCommand();
                    }


                    pml.DataList = db.Queryable<OTB_CRM_SatisfactionCustomer, OTB_CRM_Customers>
                        ((t1, t2) =>
                            new object[] {
                                JoinType.Left, t1.CustomerName == t2.CustomerCName
                            }
                        )
                        .Select((t1,t2)=> new View_CRM_SatisfactionCustomer
                        {
                            SN = t1.SN,
                            CaseSN = t1.CaseSN,
                            //CompareDB = SqlFunc.IF(t2.guid != "").Return("Y").End("N"),
                            CompareDB = t1.CompareDB,
                            CustomerName = t1.CustomerName,
                            FillerName = t1.FillerName,
                            Phone = t1.Phone,
                            Email = t1.Email,
                            Feild01 = t1.Feild01
                        })
                        .MergeTable()
                        .Where(x => x.CaseSN == sSN)
                        .OrderBy(sSortField, sSortOrder)
                        .ToPageList(pml.PageIndex, bExcel ? 100000 : pml.PageSize, ref iPageCount); ;

                    pml.Total = iPageCount;

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, pml);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(SatisfactionCase_UpdService), @"滿意度案件", @"CompareDB（重新比對）", "", "", "");
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

        #region 對應正式客戶
        /// <summary>
        /// 對應正式客戶
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage CorrespondFormalCus(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    int iRel = 0;
                    
                    string strSN = _fetchString(i_crm, @"SN");
                    string strCustomerId = _fetchString(i_crm, @"CustomerId");

                    string sUSERID = i_crm.USERID;
                    DateTime dtNow = DateTime.Now;

                    OTB_CRM_Customers oCustomer = db.Queryable<OTB_CRM_Customers>().Where(x => x.guid == strCustomerId).Single();

                    OTB_CRM_SatisfactionCustomer oUpdateSatisfactionCustomer = new OTB_CRM_SatisfactionCustomer
                    {
                        CustomerID = oCustomer.guid,
                        CustomerName = oCustomer.CustomerCName,
                        CompareDB = "Y",
                        ModifyUser = sUSERID,
                        ModifyDate = dtNow
                    };
                    iRel = db.Updateable(oUpdateSatisfactionCustomer).UpdateColumns(it => new {
                        it.CustomerID,
                        it.CustomerName,
                        it.CompareDB,
                        it.ModifyUser,
                        it.ModifyDate
                    }).Where(x => x.SN == strSN).ExecuteCommand();

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, iRel);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(SatisfactionCase_UpdService), @"滿意度案件", @"CorrespondFormalCus（對應正式客戶）", "", "", "");
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

        #region 修改滿意度案件
        /// <summary>
        /// 修改滿意度案件
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on ImportCustomers</param>
        /// <returns></returns>
        public ResponseMessage UpdateCase(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    OTB_CRM_SatisfactionCase oSatisfactionCase = new OTB_CRM_SatisfactionCase();

                    oSatisfactionCase.ModifyDate = DateTime.Now;
                    oSatisfactionCase.ModifyUser = i_crm.USERID;

                    var iSN = _fetchString(i_crm, @"SN");
                    oSatisfactionCase.CaseName = _fetchString(i_crm, @"CaseName");
                    oSatisfactionCase.ExhibitionNO = _fetchString(i_crm, @"ExhibitionNO");

                    var iRel = db.Updateable(oSatisfactionCase).UpdateColumns(
                        x => new
                        {
                            x.CaseName,
                            x.ExhibitionNO,
                            x.ModifyUser,
                            x.ModifyDate
                        }).Where(x => x.SN == iSN).ExecuteCommand();

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, iRel);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(SatisfactionCase_UpdService), @"滿意度案件", @"UpdateCase（修改滿意度案件）", @"", @"", @"");
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
    }
}
