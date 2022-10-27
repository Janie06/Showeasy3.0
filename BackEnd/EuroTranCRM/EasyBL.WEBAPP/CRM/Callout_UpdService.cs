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
    public class Callout_UpdService : ServiceBase 
    {
        #region 抓取展覽聯絡人列表資料

        /// <summary>
        /// 抓取展覽聯絡人列表資料
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on GetContactorlist</param>
        /// <returns></returns>
        public ResponseMessage GetExhibitionContactorslist(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var sExhibitionSN = _fetchString(i_crm, @"ExhibitionSN");
                    var strCustomerId = _fetchString(i_crm, @"CustomerId");
                    
                    var spExhibitionSN = new SugarParameter("@strExhibitionNO", sExhibitionSN);
                    var spCustomerId = new SugarParameter("@strCustomerId", strCustomerId);
                    var dt = db.Ado.UseStoredProcedure().GetDataTable("OSP_OTB_CRM_Callout_GetContactorsList", spExhibitionSN, spCustomerId);
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, dt);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Callout_UpdService), @"Callout紀錄", @"GetExhibitionContactorslist（抓取展覽聯絡人列表資料）", @"", @"", @"");
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

        #endregion 抓取展覽聯絡人列表資料

        #region 抓取Callout紀錄資料

        /// <summary>
        /// 抓取Callout紀錄資料
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on GetContactorlist</param>
        /// <returns></returns>
        public ResponseMessage GetCalloutData(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var sExhibitionSN = _fetchString(i_crm, @"ExhibitionSN");
                    var strCustomerId = _fetchString(i_crm, @"Guid");

                    List<View_OPM_ExhibitionCustomers> listExhibitionCustomers = 
                        db.Queryable<OTB_OPM_ExhibitionCustomers, OTB_OPM_Exhibition>((t1, t2) =>
                        new object[] {
                                JoinType.Inner, t1.ExhibitionNO == t2.SN.ToString()
                        })
                        .Where((t1, t2) => t1.CustomerId == strCustomerId)
                        .Select((t1, t2) => new View_OPM_ExhibitionCustomers
                        {
                            ExhibitionNO = t1.ExhibitionNO,
                            ExhibitionName = t2.Exhibitioname_TW,
                            TransportRequire = t1.TransportRequire,
                            TransportationMode = t1.TransportationMode,
                            ProcessingMode = t1.ProcessingMode,
                            VolumeForecasting = t1.VolumeForecasting,
                            Potential = t1.Potential,
                            BoothNumber = t1.BoothNumber,
                            NumberOfBooths = t1.NumberOfBooths,
                            CoopTrasportCompany = t1.CoopTrasportCompany,
                            CalloutLog = SqlFunc.MappingColumn(t1.ExhibitionNO, "dbo.[OFN_CRM_GetCalloutByExhibition](t1.ExhibitionNO, t1.CustomerId)"),
                            CreateDate = t1.CreateDate,
                            ExhibitionDateStart = t2.ExhibitionDateStart,
                            Memo = t1.Memo
                        }).MergeTable().OrderBy("ExhibitionDateStart", "desc").ToList();

                    List<View_OPM_ExhibitionCustomers> listResult = new List<View_OPM_ExhibitionCustomers>();

                    listResult.Add(listExhibitionCustomers.Where(x => x.ExhibitionNO == sExhibitionSN).Single());

                    foreach (View_OPM_ExhibitionCustomers oExhibitionCustomers in listExhibitionCustomers)
                    {
                        if (oExhibitionCustomers.ExhibitionNO != sExhibitionSN)
                        {
                            listResult.Add(oExhibitionCustomers);
                        }
                    }

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, listResult);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Callout_UpdService), @"Callout紀錄", @"GetCalloutData（抓取Callout紀錄資料）", @"", @"", @"");
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

        #endregion 抓取Callout紀錄資料

        #region 建立聯絡人

        /// <summary>
        /// 建立聯絡人
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on GetContactorlist</param>
        /// <returns></returns>
        public ResponseMessage CreateContactor(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    int iRel = 1;

                    string strExhibitionNO = _fetchString(i_crm, @"ExhibitionNO");
                    string strCustomerId = _fetchString(i_crm, @"CustomerId");
                    string strContactorId = _fetchString(i_crm, @"ContactorId");
                    string sUSERID = i_crm.USERID;
                    DateTime dtNow = DateTime.Now;

                    List<OTB_CRM_ContactorsTemp> listContactorsTemp = db.Queryable<OTB_CRM_ContactorsTemp>().Where(x => x.guid == strContactorId).ToList();

                    if (listContactorsTemp.Count != 0)
                    {
                        //暫存聯絡人新增至正式聯絡人
                        SugarParameter[] parameterValue = new SugarParameter[]
                        {
                            new SugarParameter("@Type", "3"),
                            new SugarParameter("@Guid", ""),
                            new SugarParameter("@CustomerId", ""),
                            new SugarParameter("@ContactorId", strContactorId),
                            new SugarParameter("@CreateUser", sUSERID),
                            new SugarParameter("@CreateDate", dtNow)
                        };
                        iRel = db.Ado.UseStoredProcedure().ExecuteCommand("OSP_OTB_CRM_Contactors_DBImportInsert", parameterValue);

                        //更新展覽聯絡人關聯
                        OTB_OPM_ExhibitionContactors oUpdateExhibitionContactors = new OTB_OPM_ExhibitionContactors
                        {
                            IsFormal = "Y",
                            ModifyUser = sUSERID,
                            ModifyDate = dtNow
                        };
                        iRel = db.Updateable(oUpdateExhibitionContactors).UpdateColumns(it => new { it.IsFormal, it.ModifyUser, it.ModifyDate })
                            .Where(x => x.ExhibitionNO == strExhibitionNO && x.CustomerId == strCustomerId && x.ContactorId == strContactorId).ExecuteCommand();

                        //刪除暫存聯絡人
                        iRel = db.Deleteable<OTB_CRM_ContactorsTemp>().Where(x => x.guid == strContactorId).ExecuteCommand();
                    } else
                    {
                        OTB_OPM_ExhibitionContactors ExhibitionContactors = new OTB_OPM_ExhibitionContactors();

                        ExhibitionContactors.ExhibitionNO = strExhibitionNO;
                        ExhibitionContactors.CustomerId = strCustomerId;
                        ExhibitionContactors.SourceType = "2";
                        ExhibitionContactors.IsFormal = "Y";
                        ExhibitionContactors.CreateUser = sUSERID;
                        ExhibitionContactors.CreateDate = dtNow;
                        ExhibitionContactors.ModifyUser = sUSERID;
                        ExhibitionContactors.ModifyDate = dtNow;

                        ExhibitionContactors.ContactorId = strContactorId;

                        iRel = db.Insertable(ExhibitionContactors).ExecuteCommand();
                    }

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, iRel);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Callout_UpdService), @"Callout紀錄", @"CreateContactor（建立聯絡人）", @"", @"", @"");
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

        #endregion 建立聯絡人

        #region 移除聯絡人

        /// <summary>
        /// 移除聯絡人
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on GetContactorlist</param>
        /// <returns></returns>
        public ResponseMessage RemoveContactor(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    int iRel = 1;

                    string strExhibitionNO = _fetchString(i_crm, @"ExhibitionNO");
                    string strCustomerId = _fetchString(i_crm, @"CustomerId");
                    string strContactorId = _fetchString(i_crm, @"ContactorId");

                    //刪除聯絡人關聯
                    iRel = db.Deleteable<OTB_OPM_ExhibitionContactors>().Where(x => x.ExhibitionNO == strExhibitionNO && x.CustomerId == strCustomerId && x.ContactorId == strContactorId).ExecuteCommand();

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, iRel);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Callout_UpdService), @"Callout紀錄", @"CreateContactor（移除聯絡人）", @"", @"", @"");
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

        #endregion 移除聯絡人

        #region 設定主要聯絡人

        /// <summary>
        /// 設定主要聯絡人
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on GetContactorlist</param>
        /// <returns></returns>
        public ResponseMessage SetContactorIsMain(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    int iRel = 1;

                    string strExhibitionNO = _fetchString(i_crm, @"ExhibitionNO");
                    string strCustomerId = _fetchString(i_crm, @"CustomerId");
                    string strContactorId = _fetchString(i_crm, @"ContactorId");

                    string sUSERID = i_crm.USERID;
                    DateTime dtNow = DateTime.Now;

                    //先全部拿掉
                    OTB_OPM_ExhibitionContactors oUpdateExhibitionContactors1 = new OTB_OPM_ExhibitionContactors
                    {
                        IsMain = "N",
                    };
                    iRel = db.Updateable(oUpdateExhibitionContactors1).UpdateColumns(it => new {
                        it.IsMain,
                    }).Where(x => x.ExhibitionNO == strExhibitionNO && x.CustomerId == strCustomerId).ExecuteCommand();

                    //設定主要聯絡人
                    OTB_OPM_ExhibitionContactors oUpdateExhibitionContactors2 = new OTB_OPM_ExhibitionContactors
                    {
                        IsMain = "Y",
                        ModifyUser = sUSERID,
                        ModifyDate = dtNow
                    };
                    iRel = db.Updateable(oUpdateExhibitionContactors2).UpdateColumns(it => new {
                        it.IsMain,
                        it.ModifyUser,
                        it.ModifyDate
                    }).Where(x => x.ExhibitionNO == strExhibitionNO && x.CustomerId == strCustomerId && x.ContactorId == strContactorId).ExecuteCommand();
                    
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, iRel);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Callout_UpdService), @"Callout紀錄", @"SetContactorIsMain（設定主要聯絡人）", @"", @"", @"");
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

        #endregion 設定主要聯絡人

        #region 取得三年內已成交展覽名單

        /// <summary>
        /// 取得三年內已成交展覽名單
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on GetContactorlist</param>
        /// <returns></returns>
        public ResponseMessage GetDealExhibitionlist(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var sId = _fetchString(i_crm, @"guid");

                    int iPageCount = 0;

                    string strDealExhibitionNO = "";

                    List<OTB_OPM_ExportExhibition> listExportExhibition = new List<OTB_OPM_ExportExhibition>();
                    List<OTB_OPM_OtherExhibitionTG> listOtherExhibitionTG = new List<OTB_OPM_OtherExhibitionTG>();
                    List<OTB_OPM_ImportExhibition> listImportExhibition = new List<OTB_OPM_ImportExhibition>();
                    List<OTB_OPM_OtherExhibition> listOtherExhibition = new List<OTB_OPM_OtherExhibition>();

                    var listMatchCustomer = db.Queryable<OTB_CRM_Customers, OTB_CRM_CustomersMST>((t1, t2) => new object[] { JoinType.Inner, t1.CustomerNO == t2.CustomerNO })
                            .Where((t1, t2) => t2.customer_guid == sId && t2.Memo == "CustomerCombine")
                            .Select((t1, t2) => new { t1.guid, t2.customer_guid, t2.Memo }).MergeTable()
                            .ToList();
                    string strMatchId = "";

                    if (listMatchCustomer.Count == 0)
                    {
                        listExportExhibition = db.Queryable<OTB_OPM_ExportExhibition>().Where(x => x.Exhibitors.Contains(sId)).ToList();
                        listOtherExhibitionTG = db.Queryable<OTB_OPM_OtherExhibitionTG>().Where(x => x.Exhibitors.Contains(sId)).ToList();
                        listImportExhibition = db.Queryable<OTB_OPM_ImportExhibition>().Where(x => x.Supplier == sId).ToList();
                        listOtherExhibition = db.Queryable<OTB_OPM_OtherExhibition>().Where(x => x.Supplier == sId).ToList();
                    }
                    else
                    {
                        strMatchId = listMatchCustomer[0].guid;
                        listExportExhibition = db.Queryable<OTB_OPM_ExportExhibition>().Where(x => x.Exhibitors.Contains(sId) || x.Exhibitors.Contains(strMatchId)).ToList();
                        listOtherExhibitionTG = db.Queryable<OTB_OPM_OtherExhibitionTG>().Where(x => x.Exhibitors.Contains(sId) || x.Exhibitors.Contains(strMatchId)).ToList();
                        listImportExhibition = db.Queryable<OTB_OPM_ImportExhibition>().Where(x => x.Supplier == sId || x.Supplier == strMatchId).ToList();
                        listOtherExhibition = db.Queryable<OTB_OPM_OtherExhibition>().Where(x => x.Supplier == sId || x.Supplier == strMatchId).ToList();
                    }

                    foreach (OTB_OPM_ExportExhibition oExportExhibition in listExportExhibition)
                    {
                        if (!strDealExhibitionNO.Contains(oExportExhibition.ExhibitionNO))
                        {
                            var jaExhibitors = (JArray)JsonConvert.DeserializeObject(oExportExhibition.Exhibitors);

                            foreach (JObject joExhibitors in jaExhibitors)
                            {
                                if (strMatchId != "")
                                {
                                    if ((joExhibitors["SupplierID"].ToString() == sId && joExhibitors["VoidContent"] == null) ||
                                        (joExhibitors["SupplierID"].ToString() == strMatchId && joExhibitors["VoidContent"] == null)
                                        )
                                    {
                                        strDealExhibitionNO = strDealExhibitionNO + oExportExhibition.ExhibitionNO + ",";
                                    }
                                }
                                else
                                {
                                    if (joExhibitors["SupplierID"].ToString() == sId && joExhibitors["VoidContent"] == null)
                                    {
                                        strDealExhibitionNO = strDealExhibitionNO + oExportExhibition.ExhibitionNO + ",";
                                    }
                                }
                            }
                        }
                    }

                    foreach (OTB_OPM_OtherExhibitionTG oOtherExhibitionTG in listOtherExhibitionTG)
                    {
                        if (!strDealExhibitionNO.Contains(oOtherExhibitionTG.ExhibitionNO))
                        {
                            var jaExhibitors = (JArray)JsonConvert.DeserializeObject(oOtherExhibitionTG.Exhibitors);

                            foreach (JObject joExhibitors in jaExhibitors)
                            {
                                if (strMatchId != "")
                                {
                                    if ((joExhibitors["SupplierID"].ToString() == sId && joExhibitors["VoidContent"] == null) ||
                                        (joExhibitors["SupplierID"].ToString() == strMatchId && joExhibitors["VoidContent"] == null)
                                        )
                                    {
                                        strDealExhibitionNO = strDealExhibitionNO + oOtherExhibitionTG.ExhibitionNO + ",";
                                    }
                                }
                                else
                                {
                                    if (joExhibitors["SupplierID"].ToString() == sId && joExhibitors["VoidContent"] == null)
                                    {
                                        strDealExhibitionNO = strDealExhibitionNO + oOtherExhibitionTG.ExhibitionNO + ",";
                                    }
                                }
                            }
                        }
                    }

                    foreach (OTB_OPM_ImportExhibition oImportExhibition in listImportExhibition)
                    {
                        if (!strDealExhibitionNO.Contains(oImportExhibition.ExhibitionNO))
                        {
                            strDealExhibitionNO = strDealExhibitionNO + oImportExhibition.ExhibitionNO + ",";
                        }
                    }

                    foreach (OTB_OPM_OtherExhibition oOtherExhibition in listOtherExhibition)
                    {
                        if (!strDealExhibitionNO.Contains(oOtherExhibition.ExhibitionNO))
                        {
                            strDealExhibitionNO = strDealExhibitionNO + oOtherExhibition.ExhibitionNO + ",";
                        }
                    }

                    strDealExhibitionNO = strDealExhibitionNO.TrimEnd(',');
                    string[] arrDealExhibitionNO = strDealExhibitionNO.Split(',');

                    DateTime dt3YearsAgo = DateTime.Today.AddYears(-3);

                    List<View_OPM_Exhibition> listDealExhibition = db.Queryable<OTB_OPM_Exhibition, OTB_SYS_Members>
                    ((t1, t2) =>
                    new object[] {
                        JoinType.Left, t1.OrgID == t2.OrgID && t1.CreateUser == t2.MemberID
                        }
                    )
                    .Where((t1, t2) => SqlFunc.ContainsArray(arrDealExhibitionNO, t1.SN.ToString()) && t1.ExhibitionDateEnd > dt3YearsAgo)
                    .Select((t1, t2) => new View_OPM_Exhibition
                    {
                        SN = t1.SN,
                        ExhibitioShotName_TW = t1.ExhibitioShotName_TW,
                        Exhibitioname_TW = t1.Exhibitioname_TW,
                        Exhibitioname_EN = t1.Exhibitioname_EN,
                        ExhibitionDateStart = t1.ExhibitionDateStart,
                        ExhibitionDateEnd = t1.ExhibitionDateEnd,
                        CreateUserName = t2.MemberName
                    })
                    .MergeTable()
                    .OrderBy("ExhibitionDateStart", "desc")
                    .ToPageList(1, 999999999, ref iPageCount);


                    string strRefNumber = "";

                    List<View_OPM_Exhibition> listExhibition = new List<View_OPM_Exhibition>();
                    int i = 1;
                    foreach (View_OPM_Exhibition oExhibition in listDealExhibition)
                    {
                        strRefNumber = "";

                        foreach (OTB_OPM_ImportExhibition oImportExhibition in listImportExhibition.Where(x => x.ExhibitionNO == oExhibition.SN.ToString()))
                        {
                            strRefNumber = strRefNumber + "1;" + oImportExhibition.ImportBillNO + ";" + oImportExhibition.RefNumber + ",";
                        }

                        foreach (OTB_OPM_ExportExhibition oExportExhibition in listExportExhibition.Where(x => x.ExhibitionNO == oExhibition.SN.ToString()))
                        {
                            strRefNumber = strRefNumber + "2;" + oExportExhibition.ExportBillNO + ";" + oExportExhibition.RefNumber + ",";
                        }

                        i = 1;
                        foreach (OTB_OPM_OtherExhibition oOtherExhibition in listOtherExhibition.Where(x => x.ExhibitionNO == oExhibition.SN.ToString()))
                        {
                            strRefNumber = strRefNumber + "3;" + oOtherExhibition.Guid + ";" + oOtherExhibition.CreateDate + ",";
                            i++;
                        }

                        i = 1;
                        foreach (OTB_OPM_OtherExhibitionTG oOtherExhibitionTG in listOtherExhibitionTG.Where(x => x.ExhibitionNO == oExhibition.SN.ToString()))
                        {
                            strRefNumber = strRefNumber + "4;" + oOtherExhibitionTG.Guid + ";" + oOtherExhibitionTG.CreateDate + ",";
                            i++;
                        }

                        View_OPM_Exhibition oViewExhibition = new View_OPM_Exhibition();
                        oViewExhibition.RowIndex = oExhibition.RowIndex;
                        oViewExhibition.SN = oExhibition.SN;
                        oViewExhibition.ExhibitioShotName_TW = oExhibition.ExhibitioShotName_TW;
                        oViewExhibition.Exhibitioname_TW = oExhibition.Exhibitioname_TW;
                        oViewExhibition.Exhibitioname_EN = oExhibition.Exhibitioname_EN;
                        oViewExhibition.ExhibitionDateStart = oExhibition.ExhibitionDateStart;
                        oViewExhibition.ExhibitionDateEnd = oExhibition.ExhibitionDateEnd;
                        oViewExhibition.CreateUserName = oExhibition.CreateUserName;
                        oViewExhibition.RefNumber = strRefNumber.TrimEnd(',');
                        oViewExhibition.IsDeal = "Y";

                        listExhibition.Add(oViewExhibition);
                    }

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, listExhibition);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Callout_UpdService), @"Callout紀錄", @"GetDealExhibitionlist（取得三年內已成交展覽名單）", @"", @"", @"");
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

        #endregion 取得三年內已成交展覽名單

        #region 取得三年內未成交展覽名單

        /// <summary>
        /// 取得三年內未成交展覽名單
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on GetContactorlist</param>
        /// <returns></returns>
        public ResponseMessage GetUnDealExhibitionlist(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var sId = _fetchString(i_crm, @"guid");

                    int iPageCount = 0;

                    string strDealExhibitionNO = "";
                    string strUnDealExhibitionNO = "";

                    List<OTB_OPM_ExhibitionCustomers> listExhibitionCustomers = db.Queryable<OTB_OPM_ExhibitionCustomers>().Where(x => x.CustomerId == sId).ToList();

                    List<OTB_OPM_ExportExhibition> listExportExhibition = new List<OTB_OPM_ExportExhibition>();
                    List<OTB_OPM_OtherExhibitionTG> listOtherExhibitionTG = new List<OTB_OPM_OtherExhibitionTG>();
                    List<OTB_OPM_ImportExhibition> listImportExhibition = new List<OTB_OPM_ImportExhibition>();
                    List<OTB_OPM_OtherExhibition> listOtherExhibition = new List<OTB_OPM_OtherExhibition>();

                    var listMatchCustomer = db.Queryable<OTB_CRM_Customers, OTB_CRM_CustomersMST>((t1, t2) => new object[] { JoinType.Inner, t1.CustomerNO == t2.CustomerNO })
                            .Where((t1, t2) => t2.customer_guid == sId && t2.Memo == "CustomerCombine")
                            .Select((t1, t2) => new { t1.guid, t2.customer_guid, t2.Memo }).MergeTable()
                            .ToList();

                    if (listMatchCustomer.Count == 0)
                    {
                        listExportExhibition = db.Queryable<OTB_OPM_ExportExhibition>().Where(x => x.Exhibitors.Contains(sId)).ToList();
                        listOtherExhibitionTG = db.Queryable<OTB_OPM_OtherExhibitionTG>().Where(x => x.Exhibitors.Contains(sId)).ToList();
                        listImportExhibition = db.Queryable<OTB_OPM_ImportExhibition>().Where(x => x.Supplier == sId).ToList();
                        listOtherExhibition = db.Queryable<OTB_OPM_OtherExhibition>().Where(x => x.Supplier == sId).ToList();
                    }
                    else
                    {
                        string strMatchId = listMatchCustomer[0].guid;
                        listExportExhibition = db.Queryable<OTB_OPM_ExportExhibition>().Where(x => x.Exhibitors.Contains(sId) || x.Exhibitors.Contains(strMatchId)).ToList();
                        listOtherExhibitionTG = db.Queryable<OTB_OPM_OtherExhibitionTG>().Where(x => x.Exhibitors.Contains(sId) || x.Exhibitors.Contains(strMatchId)).ToList();
                        listImportExhibition = db.Queryable<OTB_OPM_ImportExhibition>().Where(x => x.Supplier == sId || x.Supplier == strMatchId).ToList();
                        listOtherExhibition = db.Queryable<OTB_OPM_OtherExhibition>().Where(x => x.Supplier == sId || x.Supplier == strMatchId).ToList();
                    }

                    foreach (OTB_OPM_ExportExhibition oExportExhibition in listExportExhibition)
                    {
                        if (!strDealExhibitionNO.Contains(oExportExhibition.ExhibitionNO))
                        {
                            strDealExhibitionNO = strDealExhibitionNO + oExportExhibition.ExhibitionNO + ",";
                        }
                    }

                    foreach (OTB_OPM_OtherExhibitionTG oOtherExhibitionTG in listOtherExhibitionTG)
                    {
                        if (!strDealExhibitionNO.Contains(oOtherExhibitionTG.ExhibitionNO))
                        {
                            strDealExhibitionNO = strDealExhibitionNO + oOtherExhibitionTG.ExhibitionNO + ",";
                        }
                    }

                    foreach (OTB_OPM_ImportExhibition oImportExhibition in listImportExhibition)
                    {
                        if (!strDealExhibitionNO.Contains(oImportExhibition.ExhibitionNO))
                        {
                            strDealExhibitionNO = strDealExhibitionNO + oImportExhibition.ExhibitionNO + ",";
                        }
                    }

                    foreach (OTB_OPM_OtherExhibition oOtherExhibition in listOtherExhibition)
                    {
                        if (!strDealExhibitionNO.Contains(oOtherExhibition.ExhibitionNO))
                        {
                            strDealExhibitionNO = strDealExhibitionNO + oOtherExhibition.ExhibitionNO + ",";
                        }
                    }

                    foreach (OTB_OPM_ExhibitionCustomers oExhibitionCustomers in listExhibitionCustomers)
                    {
                        if (!strDealExhibitionNO.Contains(oExhibitionCustomers.ExhibitionNO))
                        {
                            strUnDealExhibitionNO = strUnDealExhibitionNO + oExhibitionCustomers.ExhibitionNO + ",";
                        }
                    }

                    strUnDealExhibitionNO = strUnDealExhibitionNO.TrimEnd(',');
                    string[] arrUnDealExhibitionNO = strUnDealExhibitionNO.Split(',');

                    DateTime dt3YearsAgo = DateTime.Today.AddYears(-3);

                    List<View_OPM_Exhibition> listUnDealExhibition = db.Queryable<OTB_OPM_Exhibition, OTB_SYS_Members>
                    ((t1, t2) =>
                    new object[] {
                        JoinType.Left, t1.OrgID == t2.OrgID && t1.CreateUser == t2.MemberID
                        }
                    )
                    .Where((t1, t2) => SqlFunc.ContainsArray(arrUnDealExhibitionNO, t1.SN.ToString()) && t1.ExhibitionDateEnd > dt3YearsAgo)
                    .Select((t1, t2) => new View_OPM_Exhibition
                    {
                        SN = t1.SN,
                        ExhibitioShotName_TW = t1.ExhibitioShotName_TW,
                        Exhibitioname_TW = t1.Exhibitioname_TW,
                        Exhibitioname_EN = t1.Exhibitioname_EN,
                        ExhibitionDateStart = t1.ExhibitionDateStart,
                        ExhibitionDateEnd = t1.ExhibitionDateEnd,
                        CreateUserName = t2.MemberName
                    })
                    .MergeTable()
                    .OrderBy("ExhibitionDateStart", "desc")
                    .ToPageList(1, 999999999, ref iPageCount);

                    List<View_OPM_Exhibition> listExhibition = new List<View_OPM_Exhibition>();

                    foreach (View_OPM_Exhibition oUnDealExhibition in listUnDealExhibition)
                    {
                        View_OPM_Exhibition oViewExhibition = new View_OPM_Exhibition();
                        oViewExhibition.RowIndex = oUnDealExhibition.RowIndex;
                        oViewExhibition.SN = oUnDealExhibition.SN;
                        oViewExhibition.ExhibitioShotName_TW = oUnDealExhibition.ExhibitioShotName_TW;
                        oViewExhibition.Exhibitioname_TW = oUnDealExhibition.Exhibitioname_TW;
                        oViewExhibition.Exhibitioname_EN = oUnDealExhibition.Exhibitioname_EN;
                        oViewExhibition.ExhibitionDateStart = oUnDealExhibition.ExhibitionDateStart;
                        oViewExhibition.ExhibitionDateEnd = oUnDealExhibition.ExhibitionDateEnd;
                        oViewExhibition.CreateUserName = oUnDealExhibition.CreateUserName;
                        oViewExhibition.IsDeal = "N";

                        listExhibition.Add(oViewExhibition);
                    }

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, listExhibition);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Callout_UpdService), @"Callout紀錄", @"GetUnDealExhibitionlist（取得三年內未成交展覽名單）", @"", @"", @"");
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

        #endregion 取得三年內未成交展覽名單

        #region 新增Callout紀錄

        /// <summary>
        /// 新增Callout紀錄
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on CombineCustomer</param>
        /// <returns></returns>
        public ResponseMessage CreateCalloutLog(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    string strExhibitionNO = _fetchString(i_crm, "ExhibitionNO");
                    string strCustomerId = _fetchString(i_crm, "CustomerId");
                    string strContactor = _fetchString(i_crm, "Contactor");
                    string strMemo = _fetchString(i_crm, "Memo");
                    string sUSERID = i_crm.USERID;
                    DateTime dtNow = DateTime.Now;

                    OTB_CRM_Callout oCallout = new OTB_CRM_Callout();
                    oCallout.ExhibitionNO = strExhibitionNO;
                    oCallout.CustomerId = strCustomerId;
                    oCallout.Contactor = strContactor;
                    oCallout.Memo = strMemo;
                    oCallout.CreateUser = sUSERID;
                    oCallout.CreateDate = dtNow;
                    oCallout.ModifyUser = sUSERID;
                    oCallout.ModifyDate = dtNow;

                    db.Insertable(oCallout).ExecuteCommand();

                    var spExhibitionSN = new SugarParameter("@strExhibitionNO", strExhibitionNO);
                    var spCustomerId = new SugarParameter("@strCustomerId", strCustomerId);
                    var dt = db.Ado.UseStoredProcedure().GetDataTable("OSP_OTB_CRM_Callout_GetLogList", spExhibitionSN, spCustomerId);
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, dt);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Callout_UpdService), "Callout紀錄", "CreateCalloutLog（新增Callout紀錄）", "", "", "");
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
        #endregion 新增Callout紀錄

        #region 選擇聯絡人

        /// <summary>
        /// 選擇聯絡人
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on CombineCustomer</param>
        /// <returns></returns>
        public ResponseMessage ChooseContactor(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    int iRel = 0;
                    string strExhibitionNO = _fetchString(i_crm, "SN");
                    string strCustomerId = _fetchString(i_crm, "guid");
                    string[] arrContactors = _fetchString(i_crm, "contactor").Replace("\r\n", "").Replace("\"", "").TrimStart('[').TrimEnd(']').Split(',');
                    string sUSERID = i_crm.USERID;
                    DateTime dtNow = DateTime.Now;

                    foreach (string strContactor in arrContactors)
                    {
                        OTB_OPM_ExhibitionContactors ExhibitionContactors = new OTB_OPM_ExhibitionContactors();

                        ExhibitionContactors.ExhibitionNO = strExhibitionNO;
                        ExhibitionContactors.CustomerId = strCustomerId;
                        ExhibitionContactors.SourceType = "2";
                        ExhibitionContactors.IsFormal = "Y";
                        ExhibitionContactors.CreateUser = sUSERID;
                        ExhibitionContactors.CreateDate = dtNow;
                        ExhibitionContactors.ModifyUser = sUSERID;
                        ExhibitionContactors.ModifyDate = dtNow;

                        ExhibitionContactors.ContactorId = strContactor.Trim();

                        iRel = db.Insertable(ExhibitionContactors).ExecuteCommand();
                    }

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, iRel);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Callout_UpdService), "Callout紀錄", "ChooseContactor（選擇聯絡人）", "", "", "");
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
        #endregion 選擇聯絡人

        #region 取得未來展覽名單

        /// <summary>
        /// 取得未來展覽名單
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on GetContactorlist</param>
        /// <returns></returns>
        public ResponseMessage GetFutureExhibitionlist(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var sId = _fetchString(i_crm, @"guid");

                    int iPageCount = 0;

                    string strExhibitionNO = "";

                    

                    List<OTB_OPM_ExhibitionCustomers> listExhibitionCustomers = db.Queryable<OTB_OPM_ExhibitionCustomers>().Where(x => x.CustomerId == sId).ToList();

                    foreach (OTB_OPM_ExhibitionCustomers oExhibitionCustomers in listExhibitionCustomers)
                    {
                        strExhibitionNO = strExhibitionNO + oExhibitionCustomers.ExhibitionNO + ",";
                    }
                    
                    strExhibitionNO = strExhibitionNO.TrimEnd(',');
                    string[] arrExhibitionNO = strExhibitionNO.Split(',');

                    DateTime dtNow = DateTime.Now;

                    List<View_OPM_Exhibition> listUnDealExhibition = db.Queryable<OTB_OPM_Exhibition, OTB_SYS_Members>
                    ((t1, t2) =>
                    new object[] {
                        JoinType.Left, t1.OrgID == t2.OrgID && t1.CreateUser == t2.MemberID
                        }
                    )
                    .Where((t1, t2) => SqlFunc.ContainsArray(arrExhibitionNO, t1.SN.ToString()) && t1.ExhibitionDateEnd > dtNow)
                    .Select((t1, t2) => new View_OPM_Exhibition
                    {
                        SN = t1.SN,
                        ExhibitioShotName_TW = t1.ExhibitioShotName_TW,
                        Exhibitioname_TW = t1.Exhibitioname_TW,
                        Exhibitioname_EN = t1.Exhibitioname_EN,
                        ExhibitionDateStart = t1.ExhibitionDateStart,
                        ExhibitionDateEnd = t1.ExhibitionDateEnd,
                        CreateUserName = t2.MemberName
                    })
                    .MergeTable()
                    .OrderBy("ExhibitionDateEnd", "desc")
                    .ToPageList(1, 999999999, ref iPageCount);

                    List<View_OPM_Exhibition> listExhibition = new List<View_OPM_Exhibition>();

                    foreach (View_OPM_Exhibition oUnDealExhibition in listUnDealExhibition)
                    {
                        View_OPM_Exhibition oViewExhibition = new View_OPM_Exhibition();
                        oViewExhibition.RowIndex = oUnDealExhibition.RowIndex;
                        oViewExhibition.SN = oUnDealExhibition.SN;
                        oViewExhibition.ExhibitioShotName_TW = oUnDealExhibition.ExhibitioShotName_TW;
                        oViewExhibition.Exhibitioname_TW = oUnDealExhibition.Exhibitioname_TW;
                        oViewExhibition.Exhibitioname_EN = oUnDealExhibition.Exhibitioname_EN;
                        oViewExhibition.ExhibitionDateStart = oUnDealExhibition.ExhibitionDateStart;
                        oViewExhibition.ExhibitionDateEnd = oUnDealExhibition.ExhibitionDateEnd;
                        oViewExhibition.CreateUserName = oUnDealExhibition.CreateUserName;

                        listExhibition.Add(oViewExhibition);
                    }

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, listExhibition);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Callout_UpdService), @"Callout紀錄", @"GetFutureExhibitionlist（取得未來展覽名單）", @"", @"", @"");
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

        #endregion 取得未來展覽名單

        #region 更新Callout資料

        /// <summary>
        /// 更新Callout資料
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on CombineCustomer</param>
        /// <returns></returns>
        public ResponseMessage UpdateCalloutData(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    int iRel = 0;
                    string strFormalCustomer = _fetchString(i_crm, "FormalCustomer");
                    string strExhibitionNO = _fetchString(i_crm, "ExhibitionNO");
                    string strCustomerId = _fetchString(i_crm, "CustomerId");
                    string sUSERID = i_crm.USERID;
                    DateTime dtNow = DateTime.Now;
                    
                    //更新展覽客戶關聯資料
                    OTB_OPM_ExhibitionCustomers oExhibitionCustomers = new OTB_OPM_ExhibitionCustomers
                    {
                        TransportRequire = _fetchString(i_crm, "TransportRequire"),
                        TransportationMode = _fetchString(i_crm, "TransportationMode"),
                        ProcessingMode = _fetchString(i_crm, "ProcessingMode"),
                        VolumeForecasting = _fetchString(i_crm, "VolumeForecasting"),
                        Potential = _fetchString(i_crm, "Potential"),
                        BoothNumber = _fetchString(i_crm, "BoothNumber"),
                        NumberOfBooths = _fetchString(i_crm, "NumberOfBooths"),
                        CoopTrasportCompany = _fetchString(i_crm, "CoopTrasportCompany").TrimEnd(','),
                        Memo = _fetchString(i_crm, "Memo"),
                        ModifyUser = sUSERID,
                        ModifyDate = dtNow
                    };
                    iRel = db.Updateable(oExhibitionCustomers).UpdateColumns(it => new {
                            it.TransportRequire, it.TransportationMode, it.ProcessingMode, it.VolumeForecasting, it.Potential,
                            it.BoothNumber, it.NumberOfBooths, it.CoopTrasportCompany, it.Memo, it.ModifyUser, it.ModifyDate
                    }).Where(x => x.ExhibitionNO == strExhibitionNO && x.CustomerId == strCustomerId).ExecuteCommand();

                    //處理已配合運輸公司
                    List<string> listString = new List<string>();
                    //string strCoopTrasportCompany = _fetchString(i_crm, "CoopTrasportCompany").TrimEnd(',');
                    string strCoop = "";
                    //string[] arrCoop = strCoopTrasportCompany.Split(',');
                    //listString = arrCoop.ToList();

                    List<OTB_OPM_ExhibitionCustomers> listCalloutData = db.Queryable<OTB_OPM_ExhibitionCustomers>().Where(x => x.CustomerId == strCustomerId).ToList();
                    foreach (OTB_OPM_ExhibitionCustomers oCalloutData in listCalloutData)
                    {
                        if (oCalloutData.CoopTrasportCompany != null)
                        {
                            if (oCalloutData.CoopTrasportCompany != "")
                            {
                                string[] arrTemp = oCalloutData.CoopTrasportCompany.Split(',');
                                foreach (string strTemp in arrTemp)
                                {
                                    if (!listString.Contains(strTemp))
                                    {
                                        listString.Add(strTemp);
                                    }
                                }
                            }
                        }
                    }

                    foreach (string strCompany in listString)
                    {
                        strCoop += strCompany + ",";
                    }
                    strCoop = strCoop.TrimEnd(',');

                    if (strFormalCustomer != "1")
                    {
                        //更新客戶資料
                        OTB_CRM_Customers oCustomer = new OTB_CRM_Customers
                        {
                            CustomerCName = _fetchString(i_crm, "CustomerCName"),
                            CustomerEName = _fetchString(i_crm, "CustomerEName"),
                            UniCode = _fetchString(i_crm, "UniCode"),
                            Telephone = _fetchString(i_crm, "Telephone"),
                            TransactionType = _fetchString(i_crm, "TransactionType"),
                            IsBlackList = _fetchString(i_crm, "IsBlackList"),
                            BlackListReason = _fetchString(i_crm, "BlackListReason"),
                            CoopTrasportCompany = strCoop,
                            Potential = _fetchString(i_crm, "Potential"),
                            IsImporter = _fetchString(i_crm, "IsImporter"),
                            ModifyUser = sUSERID,
                            ModifyDate = dtNow
                        };
                        iRel = db.Updateable(oCustomer).UpdateColumns(it => new {
                            it.CustomerCName,
                            it.CustomerEName,
                            it.UniCode,
                            it.Telephone,
                            it.TransactionType,
                            it.IsBlackList,
                            it.BlackListReason,
                            it.CoopTrasportCompany,
                            it.Potential,
                            it.IsImporter,
                            it.ModifyUser,
                            it.ModifyDate
                        }).Where(x => x.guid == strCustomerId).ExecuteCommand();
                    } else
                    {
                        //更新客戶資料
                        OTB_CRM_Customers oCustomer = new OTB_CRM_Customers
                        {
                            CoopTrasportCompany = strCoop,
                            Potential = _fetchString(i_crm, "Potential"),
                            IsImporter = _fetchString(i_crm, "IsImporter"),
                        };
                        iRel = db.Updateable(oCustomer).UpdateColumns(it => new {
                            it.CoopTrasportCompany,
                            it.Potential,
                            it.IsImporter,
                        }).Where(x => x.guid == strCustomerId).ExecuteCommand();
                    }

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, iRel);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Callout_UpdService), "Callout紀錄", "UpdateCalloutData（更新Callout資料）", "", "", "");
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
        #endregion 更新Callout資料
        
    }
}
