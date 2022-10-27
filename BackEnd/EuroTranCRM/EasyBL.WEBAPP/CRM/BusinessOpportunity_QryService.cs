using EasyBL.WebApi.Message;
using EasyNet;
using Entity;
using Entity.Sugar;
using Entity.ViewModels;
using JumpKick.HttpLib;
using SqlSugar;
using SqlSugar.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Aspose.Cells;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.IO;
using EasyBL;


namespace EasyBL.WEBAPP.CRM
{
    public class BusinessOpportunity_QryService : ServiceBase
    {
        #region 潛在商機頁面查詢

        /// <summary>
        /// 潛在商機頁面查詢
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on QueryPage</param>
        /// <returns></returns>
        public ResponseMessage QueryPage(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var pm1 = new PageModel
                    {
                        PageIndex = _fetchInt(i_crm, @"pageIndex"),
                        PageSize = _fetchInt(i_crm, @"pageSize")
                    };
                    var bExcel = _fetchBool(i_crm, @"Excel");
                    var iPageCount = 0;
                    var sSortField = _fetchString(i_crm, @"sortField");
                    var sSortOrder = _fetchString(i_crm, @"sortOrder");
                    var sCustomerName = _fetchString(i_crm, @"CustomerName");
                    var sExhibitionName = _fetchString(i_crm, @"ExhibitionName");
                    var sDateType = _fetchString(i_crm, @"DateType");
                    var sCreateUser = _fetchString(i_crm, @"CreateUser");
                    var sContactorName = _fetchString(i_crm, @"ContactorName");
                    var sIndustry = _fetchString(i_crm, @"Industry");
                    var sState = _fetchString(i_crm, @"State");
                    var sStatus = _fetchString(i_crm, @"Effective");
                    var sDateStart = _fetchString(i_crm, @"DateStart");
                    var sDateEnd = _fetchString(i_crm, @"DateEnd");
                    
                    string[] saStatus = null;
                    if (!string.IsNullOrEmpty(sStatus))
                    {
                        saStatus = sStatus.Split(',');
                    }
                    var rDateStart = new DateTime();
                    var rDateEnd = new DateTime();
                    if (!string.IsNullOrEmpty(sDateStart))
                    {
                        rDateStart = SqlFunc.ToDate(sDateStart);
                    }
                    if (!string.IsNullOrEmpty(sDateEnd))
                    {
                        rDateEnd = SqlFunc.ToDate(sDateEnd).AddDays(1);
                    }
                    pm1.DataList = db.Queryable<OTB_CRM_BusinessOpportunity>()
                        .Where(x => (x.CustomerName.Contains(sCustomerName) && x.Contactor.Contains(sContactorName)) && (x.ExhibitionName.Contains(sExhibitionName) || x.ExhibitionName_EN.Contains(sExhibitionName) || x.ExhibitionShotName.Contains(sExhibitionName)))//少聯絡人
                        .WhereIF(!string.IsNullOrEmpty(sDateStart) && sDateType == "1", x => x.DateEnd >= rDateStart.Date)
                        .WhereIF(!string.IsNullOrEmpty(sDateEnd) && sDateType == "1", x => x.DateStart <= rDateEnd.Date)
                        .WhereIF(!string.IsNullOrEmpty(sDateStart) && sDateType == "2", x => x.CreateDate >= rDateStart.Date)
                        .WhereIF(!string.IsNullOrEmpty(sDateEnd) && sDateType == "2", x => x.CreateDate <= rDateEnd.Date)
                        .WhereIF(!string.IsNullOrEmpty(sDateStart) && sDateType == "3", x => x.ModifyDate >= rDateStart.Date)
                        .WhereIF(!string.IsNullOrEmpty(sDateEnd) && sDateType == "3", x => x.ModifyDate <= rDateEnd.Date)
                        .WhereIF(!string.IsNullOrEmpty(sIndustry), x => x.Industry == sIndustry)
                        .WhereIF(!string.IsNullOrEmpty(sCreateUser), x => x.CreateUser == sCreateUser)
                        .WhereIF(!string.IsNullOrEmpty(sState), x => x.State == sState)
                        .WhereIF(!string.IsNullOrEmpty(sStatus), x => x.Effective == sStatus)
                        .Select(x => new View_CRM_BusinessOpportunity{
                            SN = x.SN,
                            Year = x.Year,
                            ExhibitionShotName = x.ExhibitionShotName,
                            ExhibitionName = x.ExhibitionName,
                            ExhibitionName_EN = x.ExhibitionName_EN,
                            State = x.State,
                            Industry = x.Industry,
                            CustomerName = x.CustomerName,
                            Contactor = x.Contactor,
                            Effective = x.Effective,
                            CreateUser = x.CreateUser,
                            CreateDate = x.CreateDate,
                            ModifyUser = x.ModifyUser,
                            ModifyDate = x.ModifyDate,
                            DateStart = x.DateStart,
                            DateEnd = x.DateEnd
                        })
                        .OrderBy(sSortField, sSortOrder)
                        .ToPageList(pm1.PageIndex, bExcel ? 100000 : pm1.PageSize, ref iPageCount);
                    pm1.Total = iPageCount;

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, pm1);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(BusinessOpportunity_QryService), @"潛在商機管理", @"QueryPage（潛在商機頁面查詢）", @"", @"", @"");
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

        #region 潛在商機單筆查詢

        /// <summary>
        /// 潛在商機單筆查詢
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage QueryOne(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var sSN = _fetchInt(i_crm, @"SN");

                    var QueryData = db.Queryable<OTB_CRM_BusinessOpportunity>()
                        .Where(x =>x.SN == sSN)
                        .Single();

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, QueryData);
                } while (false);
            }
            catch (Exception ex)

            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(BusinessOpportunity_QryService), "", "QueryOne（潛在商機（單筆查詢））", "", "", "");
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

        #endregion 潛在商機單筆查詢

        #region 潛在商機展覽對應

        /// <summary>
        /// 潛在商機展覽對應
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on QueryExhibition</param>
        /// <returns></returns>
        public ResponseMessage QueryExhibition(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var saExhibitionCode = _fetchInt(i_crm, @"ExhibitionCode");
                    var QueryData = db.Queryable<OTB_OPM_Exhibition>()
                            .Where(x => x.SN == saExhibitionCode)
                            .ToList();
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, QueryData);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(OTB_OPM_Exhibition), @"潛在商機展覽對應", @"QueryExhibition（潛在商機展覽對應）", @"", @"", @"");
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

        #region 潛在商機作廢
        /// <summary>
        /// 潛在商機作廢
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage UpdateByGuid(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var sSN = _fetchInt(i_crm, @"SN");
                    string sUSERID = i_crm.USERID;
                    DateTime dtNow = DateTime.Now;

                    var UpdateData = db.Updateable<OTB_CRM_BusinessOpportunity>()
                                        .UpdateColumns(x => new OTB_CRM_BusinessOpportunity() { Effective = "0", ModifyUser = sUSERID, ModifyDate = dtNow })
                                        .Where(x => x.SN == sSN).ExecuteCommand();

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, UpdateData);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(BusinessOpportunity_QryService), "", "UpdateByGuid（潛在商機作廢）", "", "", "");
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

        #endregion 潛在商機作廢

        /// <summary>
        /// 檢查展覽是否存在
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        private Tuple<bool, string> CheckExhibition(RequestMessage i_crm)
        {
            var db = SugarBase.GetIntance();
            var iCount = -1;
            try
            {
                var sExhibitionName = _fetchString(i_crm, @"ExhibitionName");
                iCount = db.Queryable<OTB_CRM_BusinessOpportunity>()
                            .Where(x =>x.ExhibitionName == sExhibitionName)
                            .Count();
            }
            catch (Exception ex)
            {
                var sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Customers_QryService), "", "CheckExhibition（客戶管理編輯（檢查客戶簡稱與統一編號））", "", "", "");

            }
            switch (iCount)
            {
                case 0:
                    return new Tuple<bool, string>(false, "沒有找到重複");
                case -1:
                    return new Tuple<bool, string>(true, "尋找過程發生錯誤，請稍後嘗試。");
                default:
                    return new Tuple<bool, string>(true, "展覽重複。請重新檢查資料。");
            }
        }

        #region 新增潛在商機

        /// <summary>
        /// 新增潛在商機
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on QueryPage</param>
        /// <returns></returns>
        public ResponseMessage Insert(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            try
            {
                rm = SugarBase.ExecTran(db =>
                {
                    do
                    {
                        //變數設定
                        int iRel = 1;
                        string strExhibitionNO = "";
                        string strCustomerId = ""; 
                        var sCustomerName = _fetchString(i_crm, @"CustomerName");
                        var sContactor = _fetchString(i_crm, @"Contactor");
                        var sDepartment = _fetchString(i_crm, @"Department");
                        var sJobTitle = _fetchString(i_crm, @"JobTitle");
                        var sEmail1 = _fetchString(i_crm, @"Email1");
                        var sEmail2 = _fetchString(i_crm, @"Email2");
                        var sTelephone1 = _fetchString(i_crm, @"Telephone1");
                        var sTelephone2 = _fetchString(i_crm, @"Telephone2");

                        strExhibitionNO = _fetchString(i_crm, @"ExhibitionCode");
                        strCustomerId = _fetchString(i_crm, @"CustomerId");

                        //新增潛在商機表
                        OTB_CRM_BusinessOpportunity oBusinessOpportunity = _fetchEntity<OTB_CRM_BusinessOpportunity>(i_crm);
                        oBusinessOpportunity.CreateDate = DateTime.Now;
                        oBusinessOpportunity.ExhibitionNO = strExhibitionNO;
                        oBusinessOpportunity.ModifyDate = DateTime.Now;
                        var iRelMst = db.Insertable(oBusinessOpportunity).ExecuteCommand();

                        int intCount = 0;

                        if (strExhibitionNO != null && strExhibitionNO != "")
                        {
                            //判斷客戶是否存在
                            List<OTB_CRM_Customers> listCustomers = db.Queryable<OTB_CRM_Customers>().Where(t1 => t1.CustomerCName == sCustomerName).ToList();

                            if (listCustomers.Count == 0)
                            {
                                OTB_CRM_Customers oCustomer = _fetchEntity<OTB_CRM_Customers>(i_crm);
                                _setEntityBase(oCustomer, i_crm);

                                //新增客戶資料
                                strCustomerId = Guid.NewGuid().ToString();
                                oCustomer.guid = strCustomerId;
                                oCustomer.TransactionType = "";
                                oCustomer.CustomerNO = "    ";
                                oCustomer.IsAudit = "N";
                                oCustomer.Effective = "Y";
                                oCustomer.UniCode = "";
                                oCustomer.CustomerCName = sCustomerName;

                                //客戶資料表頭
                                OTB_CRM_CustomersMST oMstEntity = _fetchEntity<OTB_CRM_CustomersMST>(i_crm);
                                _setEntityBase(oMstEntity, i_crm);
                                oMstEntity.guid = Guid.NewGuid().ToString();
                                oMstEntity.CustomerNO = "    ";
                                oMstEntity.customer_guid = strCustomerId;
                                oMstEntity.Effective = "Y";

                                iRel = db.Insertable(oCustomer).IgnoreColumns(it => new { it.IsGroupUnit, it.Industry, it.IsBlackList, it.CoopTrasportCompany, it.BlackListReason }).ExecuteCommand();

                                if (iRel == 1)
                                {
                                    iRel = db.Insertable(oMstEntity).ExecuteCommand();
                                }
                            } else
                            {
                                strCustomerId = listCustomers[0].guid;
                                iRel = 1;
                                intCount = db.Queryable<OTB_OPM_ExhibitionCustomers>().Where(t1 => t1.ExhibitionNO == strExhibitionNO && t1.CustomerId == strCustomerId).Count();
                            }

                            if (iRel == 1 && intCount == 0)
                            {
                                //處理展覽客戶關聯資料表
                                OTB_OPM_ExhibitionCustomers oExCustomerEntity = _fetchEntity<OTB_OPM_ExhibitionCustomers>(i_crm);
                                _setEntityBase(oExCustomerEntity, i_crm);
                                oExCustomerEntity.CustomerId = strCustomerId;
                                oExCustomerEntity.ExhibitionNO = strExhibitionNO;
                                oExCustomerEntity.SourceType = "5";
                                iRel = db.Insertable(oExCustomerEntity).IgnoreColumns(it => new { it.TransportRequire, it.TransportationMode, it.ProcessingMode, it.VolumeForecasting, it.Potential }).ExecuteCommand();
                            }

                            string strContactorId = "";
                            if (iRel == 1)
                            {
                                //新增聯絡人資料
                                OTB_CRM_ContactorsTemp Contactors = new OTB_CRM_ContactorsTemp();
                                _setEntityBase(Contactors, i_crm);

                                Contactors.guid = Guid.NewGuid().ToString();
                                strContactorId = Contactors.guid;
                                Contactors.CustomerId = strCustomerId;
                                Contactors.ContactorName = sContactor;
                                Contactors.Department = sDepartment;
                                Contactors.JobTitle = sJobTitle;
                                Contactors.Email1 = sEmail1;
                                Contactors.Email2 = sEmail2;
                                Contactors.Telephone1 = sTelephone1;
                                Contactors.Telephone2 = sTelephone2;
                                int iReljaAddContactors = db.Insertable(Contactors).ExecuteCommand();

                                //新增展覽客戶聯絡人關聯資料表
                                OTB_OPM_ExhibitionContactors ExhibitionContactors = new OTB_OPM_ExhibitionContactors();
                                _setEntityBase(ExhibitionContactors, i_crm);

                                ExhibitionContactors.ExhibitionNO = strExhibitionNO;
                                ExhibitionContactors.CustomerId = strCustomerId;
                                ExhibitionContactors.SourceType = "5";
                                ExhibitionContactors.ContactorId = strContactorId;
                                ExhibitionContactors.IsFormal = "N";

                                int iRelExhibitionContactors = db.Insertable(ExhibitionContactors).ExecuteCommand();
                            }
                        }
                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, iRelMst);
                    } while (false);

                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(BusinessOpportunity_QryService), @"新增潛在商機", @"Insert（新增潛在商機）", @"", @"", @"");
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

        #region 編輯潛在商機

        /// <summary>
        /// 編輯潛在商機
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on Update</param>
        /// <returns></returns>
        public ResponseMessage Update(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            try
            {
                rm = SugarBase.ExecTran(db =>
                {
                    do
                    {
                        var sSN = _fetchInt(i_crm, @"SN");
                        var sCustomerName = _fetchString(i_crm, @"CustomerName");
                        var sContactor = _fetchString(i_crm, @"Contactor");
                        var strExhibitionNO = _fetchString(i_crm, @"ExhibitionNO");
                        var strCustomerId = _fetchString(i_crm, @"CustomerId");
                        var sDepartment = _fetchString(i_crm, @"Department");
                        var sJobTitle = _fetchString(i_crm, @"JobTitle");
                        var sEmail1 = _fetchString(i_crm, @"Email1");
                        var sEmail2 = _fetchString(i_crm, @"Email2");
                        var sTelephone1 = _fetchString(i_crm, @"Telephone1");
                        var sTelephone2 = _fetchString(i_crm, @"Telephone2");
                        var QueryExhibition = db.Queryable<OTB_CRM_BusinessOpportunity>()
                        .Where(t1 => t1.SN == sSN)
                        .Select(t1 => t1.ExhibitionNO)
                        .ToList();
                        var SqlExhibitionNO = QueryExhibition[0];

                        //修改潛在商機表
                        OTB_CRM_BusinessOpportunity oBusinessOpportunity = _fetchEntity<OTB_CRM_BusinessOpportunity>(i_crm);
                        oBusinessOpportunity.ModifyUser = oBusinessOpportunity.CreateUser;
                        oBusinessOpportunity.ModifyDate = DateTime.Now;

                        var oEntity = _fetchEntity<OTB_CRM_BusinessOpportunity>(i_crm);
                        var iRel = db.Updateable(oBusinessOpportunity).IgnoreColumns(it => new { it.CreateUser, it.CreateDate}).WhereColumns(x=>x.SN).ExecuteCommand();

                        int intCount = 0;

                        if (strExhibitionNO != "" && (SqlExhibitionNO == "" || (strExhibitionNO != SqlExhibitionNO)))
                        {
                            //判斷客戶是否存在
                            List<OTB_CRM_Customers> listCustomers = db.Queryable<OTB_CRM_Customers>().Where(t1 => t1.CustomerCName == sCustomerName).ToList();

                            if (listCustomers.Count == 0)
                            {
                                OTB_CRM_Customers oCustomer = _fetchEntity<OTB_CRM_Customers>(i_crm);
                                _setEntityBase(oCustomer, i_crm);

                                //新增客戶資料
                                strCustomerId = Guid.NewGuid().ToString();
                                oCustomer.guid = strCustomerId;
                                oCustomer.TransactionType = "";
                                oCustomer.CustomerNO = "    ";
                                oCustomer.IsAudit = "N";
                                oCustomer.Effective = "Y";
                                oCustomer.UniCode = "";
                                oCustomer.CustomerCName = sCustomerName;

                                //客戶資料表頭
                                OTB_CRM_CustomersMST oMstEntity = _fetchEntity<OTB_CRM_CustomersMST>(i_crm);
                                _setEntityBase(oMstEntity, i_crm);
                                oMstEntity.guid = Guid.NewGuid().ToString();
                                oMstEntity.CustomerNO = "    ";
                                oMstEntity.customer_guid = strCustomerId;
                                oMstEntity.Effective = "Y";

                                iRel = db.Insertable(oCustomer).IgnoreColumns(it => new { it.IsGroupUnit, it.Industry, it.IsBlackList, it.CoopTrasportCompany, it.BlackListReason }).ExecuteCommand();

                                if (iRel == 1)
                                {
                                    iRel = db.Insertable(oMstEntity).ExecuteCommand();
                                }
                            } else
                            {
                                strCustomerId = listCustomers[0].guid;
                                iRel = 1;
                                intCount = db.Queryable<OTB_OPM_ExhibitionCustomers>().Where(t1 => t1.ExhibitionNO == strExhibitionNO && t1.CustomerId == strCustomerId).Count();
                            }

                            if (iRel == 1 && intCount == 0)
                            {
                                //處理展覽客戶關聯資料表
                                OTB_OPM_ExhibitionCustomers oExCustomerEntity = _fetchEntity<OTB_OPM_ExhibitionCustomers>(i_crm);
                                _setEntityBase(oExCustomerEntity, i_crm);
                                oExCustomerEntity.CustomerId = strCustomerId;
                                oExCustomerEntity.ExhibitionNO = strExhibitionNO;
                                oExCustomerEntity.SourceType = "5";
                                iRel = db.Insertable(oExCustomerEntity).IgnoreColumns(it => new { it.TransportRequire, it.TransportationMode, it.ProcessingMode, it.VolumeForecasting, it.Potential }).ExecuteCommand();
                            }

                            string strContactorId = "";
                            if (iRel == 1)
                            {
                                //新增聯絡人資料
                                OTB_CRM_ContactorsTemp Contactors = new OTB_CRM_ContactorsTemp();
                                _setEntityBase(Contactors, i_crm);

                                Contactors.guid = Guid.NewGuid().ToString();
                                strContactorId = Contactors.guid;
                                Contactors.CustomerId = strCustomerId;
                                Contactors.ContactorName = sContactor;
                                Contactors.Department = sDepartment;
                                Contactors.JobTitle = sJobTitle;
                                Contactors.Email1 = sEmail1;
                                Contactors.Email2 = sEmail2;
                                Contactors.Telephone1 = sTelephone1;
                                Contactors.Telephone2 = sTelephone2;
                                int iReljaAddContactors = db.Insertable(Contactors).ExecuteCommand();

                                //新增展覽客戶聯絡人關聯資料表
                                OTB_OPM_ExhibitionContactors ExhibitionContactors = new OTB_OPM_ExhibitionContactors();
                                _setEntityBase(ExhibitionContactors, i_crm);

                                ExhibitionContactors.ExhibitionNO = strExhibitionNO;
                                ExhibitionContactors.CustomerId = strCustomerId;
                                ExhibitionContactors.SourceType = "5";
                                ExhibitionContactors.ContactorId = strContactorId;
                                ExhibitionContactors.IsFormal = "N";

                                int iRelExhibitionContactors = db.Insertable(ExhibitionContactors).ExecuteCommand();
                            }
                        }
                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, iRel);
                    } while (false);

                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(BusinessOpportunity_QryService), @"編輯潛在商機", @"Update（編輯潛在商機）", @"", @"", @"");
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

        #region 設定展覽下拉

        /// <summary>
        /// 設定展覽下拉
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on GetExp</param>
        /// <returns></returns>
        public ResponseMessage GetExp(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var checkResult = CheckExhibition(i_crm);
            try
            {
                rm = SugarBase.ExecTran(db =>
                {
                    do
                    {
                        var QueryData = db.Queryable<OTB_OPM_Exhibition>()
                        .Select(x => new {
                                x.Exhibitioname_TW,
                                x.ExhibitionCode
                            }
                        )
                        .ToList();

                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, QueryData);
                    } while (false);

                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(BusinessOpportunity_QryService), @"設定展覽下拉", @"GetExp（設定展覽下拉）", @"", @"", @"");
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

        #region 取得產業別全名

        /// <summary>
        /// 取得產業別全名
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on GetExp</param>
        /// <returns></returns>
        public ResponseMessage GetIndustry(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            try
            {
                rm = SugarBase.ExecTran(db =>
                {
                    do
                    {
                        var sIndustry = _fetchString(i_crm, @"Industry");
                        var QueryData = db.Queryable<OTB_SYS_Arguments>()
                        .Where(x=>x.ArgumentID == sIndustry && x.ArgumentClassID == "ExhibClass")
                        .Select(x => new {
                                x.ArgumentValue
                            }
                        )
                        .Single();

                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, QueryData);
                    } while (false);

                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(BusinessOpportunity_QryService), @"取得產業別全名", @"GetIndustry（取得產業別全名）", @"", @"", @"");
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
