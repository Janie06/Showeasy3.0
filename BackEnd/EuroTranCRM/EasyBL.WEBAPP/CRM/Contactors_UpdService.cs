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
    public class Contactors_UpdService : ServiceBase
    {
        #region 新增聯絡人資料
        /// <summary>
        /// 新增聯絡人資料
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
                    string strExhibitionNO = _fetchString(i_crm, @"ExhibitionNO");

                    OTB_CRM_Contactors oContactors = _fetchEntity<OTB_CRM_Contactors>(i_crm);
                    oContactors.CreateDate = DateTime.Now;
                    oContactors.CreateUser = i_crm.USERID;
                    var iRel = db.Insertable(oContactors).ExecuteCommand();

                    if (strExhibitionNO != "" & strExhibitionNO != null)
                    {
                        OTB_OPM_ExhibitionContactors oExhibitionContactor = new OTB_OPM_ExhibitionContactors();
                        oExhibitionContactor.ExhibitionNO = strExhibitionNO;
                        oExhibitionContactor.CustomerId = oContactors.CustomerId;
                        oExhibitionContactor.ContactorId = oContactors.guid;
                        oExhibitionContactor.SourceType = "1";
                        oExhibitionContactor.IsFormal = "Y";
                        oExhibitionContactor.CreateUser = oContactors.CreateUser;
                        oExhibitionContactor.CreateDate = oContactors.CreateDate;
                        iRel = db.Insertable(oExhibitionContactor).ExecuteCommand();
                    }

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, iRel);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.WEBAPP.OPM.Contactors_UpdService", @"聯絡人管理", @"Add（新增聯絡人資料）", @"", @"", @"");
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

        #region 刪除聯絡人資料
        /// <summary>
        /// 刪除聯絡人資料
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
                    string sId = _fetchString(i_crm, @"Guid");

                    var iRel = db.Deleteable<OTB_CRM_Contactors>().Where(x => x.guid == sId).ExecuteCommand();

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, iRel);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.WEBAPP.OPM.Contactors_UpdService", @"聯絡人管理", @"Delete（刪除聯絡人資料）", @"", @"", @"");
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

        #region 修改聯絡人資料
        /// <summary>
        /// 修改聯絡人資料
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on InsertImportCustomers</param>
        /// <returns></returns>
        public ResponseMessage Upd(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    OTB_CRM_Contactors oContactors = _fetchEntity<OTB_CRM_Contactors>(i_crm);

                    oContactors.ModifyDate = DateTime.Now;
                    oContactors.ModifyUser = i_crm.USERID;

                    var iRel = db.Updateable(oContactors).Where(x => x.guid == oContactors.guid).IgnoreColumns(x => new
                    {
                        x.CustomerId,
                        x.OrderByValue,
                        x.CreateUser,
                        x.CreateDate
                    }).ExecuteCommand();

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, iRel);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.WEBAPP.OPM.Contactors_UpdService", @"聯絡人管理", @"Upd（修改聯絡人資料）", @"", @"", @"");
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

        #region 
        /// <summary>
        /// 
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on GetCustomers</param>
        /// <returns></returns>
        public ResponseMessage GetExhibitionContactors_Y(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    string sGuid = _fetchString(i_crm, @"Guid");

                    var saExhibitionContactors = new List<Dictionary<string, object>>();
                    var IsDealList = db.Queryable<OTB_CRM_Contactors, OTB_OPM_ExhibitionContactors, OTB_OPM_Exhibition, OTB_OPM_ImportExhibition>
                        ((t1, t2, t3, t4) =>
                        new object[] {
                                JoinType.Left, t1.guid == t2.ContactorId,
                                JoinType.Left, t2.ExhibitionNO == t3.SN.ToString(),
                                JoinType.Left, t3.SN.ToString() == t4.ExhibitionNO
                              }
                        )
                        .Select((t1, t2, t3, t4) => new
                        {
                            t1.guid,
                            t4.RefNumber,
                            t3.ExhibitioShotName_TW,
                            t3.Exhibitioname_TW,
                            t3.Exhibitioname_EN,
                            t3.ExhibitionDateStart,
                            t3.ExhibitionDateEnd,
                            IsDeal = SqlFunc.MappingColumn(t1.guid, "dbo.[OFN_OPM_CheckExhibitionDeal](t3.SN)"),
                            t3.CreateUser
                        }).MergeTable()
                        .Where(it => it.guid == sGuid && it.IsDeal == "Y").ToList();

                    if (IsDealList.Count > 0)
                    {
                        foreach (var opm in IsDealList)
                        {
                            var dic = new Dictionary<string, object>
                            {
                                { @"RowIndex", saExhibitionContactors.Count + 1 },
                                { @"guid", opm.guid },
                                { @"RefNumber", opm.RefNumber },
                                { @"ExhibitioShotName_TW", opm.ExhibitioShotName_TW },
                                { @"Exhibitioname_TW", opm.Exhibitioname_TW },
                                { @"Exhibitioname_EN", opm.Exhibitioname_EN },
                                { @"ExhibitionDateStart", opm.ExhibitionDateStart },
                                { @"ExhibitionDateEnd", opm.ExhibitionDateEnd },
                                { @"CreateUser", opm.CreateUser }
                            };
                            saExhibitionContactors.Add(dic);
                        }
                    }

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, saExhibitionContactors);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Contactors_UpdService), @"展覽管理", @"GetCustomers（獲取參加該展覽的所有廠商）", @"", @"", @"");
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

        #region 
        /// <summary>
        /// 
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on GetCustomers</param>
        /// <returns></returns>
        public ResponseMessage GetExhibitionContactors_N(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    string sGuid = _fetchString(i_crm, @"Guid");

                    var saExhibitionContactors = new List<Dictionary<string, object>>();
                    var IsDealList = db.Queryable<OTB_CRM_Contactors, OTB_OPM_ExhibitionContactors, OTB_OPM_Exhibition, OTB_OPM_ImportExhibition>
                        ((t1, t2, t3, t4) =>
                        new object[] {
                                JoinType.Left, t1.guid == t2.ContactorId,
                                JoinType.Left, t2.ExhibitionNO == t3.SN.ToString(),
                                JoinType.Left, t3.SN.ToString() == t4.ExhibitionNO
                              }
                        )
                        .Select((t1, t2, t3, t4) => new
                        {
                            t1.guid,
                            t4.RefNumber,
                            t3.ExhibitioShotName_TW,
                            t3.Exhibitioname_TW,
                            t3.Exhibitioname_EN,
                            t3.ExhibitionDateStart,
                            t3.ExhibitionDateEnd,
                            IsDeal = SqlFunc.MappingColumn(t1.guid, "dbo.[OFN_OPM_CheckExhibitionDeal](t3.SN)"),
                            t3.CreateUser
                        }).MergeTable()
                        .Where(it => it.guid == sGuid && it.IsDeal == "Y").ToList();

                    if (IsDealList.Count > 0)
                    {
                        foreach (var opm in IsDealList)
                        {
                            var dic = new Dictionary<string, object>
                            {
                                { @"RowIndex", saExhibitionContactors.Count + 1 },
                                { @"guid", opm.guid },
                                { @"RefNumber", opm.RefNumber },
                                { @"ExhibitioShotName_TW", opm.ExhibitioShotName_TW },
                                { @"Exhibitioname_TW", opm.Exhibitioname_TW },
                                { @"Exhibitioname_EN", opm.Exhibitioname_EN },
                                { @"ExhibitionDateStart", opm.ExhibitionDateStart },
                                { @"ExhibitionDateEnd", opm.ExhibitionDateEnd },
                                { @"CreateUser", opm.CreateUser }
                            };
                            saExhibitionContactors.Add(dic);
                        }
                    }

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, saExhibitionContactors);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Contactors_UpdService), @"展覽管理", @"GetCustomers（獲取參加該展覽的所有廠商）", @"", @"", @"");
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

        #region 從名單移除

        /// <summary>
        /// 合併客戶
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter</param>
        /// <returns></returns>
        public ResponseMessage RemoveFromList(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var iRel = 1;

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, iRel);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.WEBAPP.OPM.Contactors_UpdService", @"展覽管理", @"RemoveFromList（從名單移除）", @"", @"", @"");
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

        #endregion 從名單移除

        #region 新增名單-資料庫匯入

        /// <summary>
        /// 新增名單-資料庫匯入
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter</param>
        /// <returns></returns>
        public ResponseMessage AddListDB(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var iRel = 1;

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, iRel);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.WEBAPP.OPM.Contactors_UpdService", @"展覽管理", @"AddListDB（新增名單-資料庫匯入）", @"", @"", @"");
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

        #endregion 新增名單-資料庫匯入

        #region 名單表格下載

        /// <summary>
        /// 名單表格下載
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter</param>
        /// <returns></returns>
        public ResponseMessage DownloadTemplate(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var oTempl = db.Queryable<OTB_SYS_OfficeTemplate>().Single(it => it.OrgID == i_crm.ORIGID && it.TemplID == "ExhibitionCustomerList");
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

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, oFile.FilePath);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.WEBAPP.OPM.Contactors_UpdService", @"展覽管理", @"DownloadTemplate（名單表格下載）", @"", @"", @"");
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

        #endregion 名單表格下載

        #region 合併聯絡人
        /// <summary>
        /// 合併聯絡人
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on CombineCustomer</param>
        /// <returns></returns>
        public ResponseMessage CombineContactor(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    string sUSERID = i_crm.USERID;
                    DateTime dtNow = DateTime.Now;
                    
                    string strUpdateGuid = "";
                    string strDeleteGuid = "";
                    string strCustomerId = _fetchString(i_crm, "CustomerId");
                    if (_fetchString(i_crm, "rdbContactorName") == "1")
                    {
                        strUpdateGuid = _fetchString(i_crm, "guid1");
                        strDeleteGuid = _fetchString(i_crm, "guid2");
                    }
                    else
                    {
                        strUpdateGuid = _fetchString(i_crm, "guid2");
                        strDeleteGuid = _fetchString(i_crm, "guid1");
                    }

                    string strContactorName = _fetchString(i_crm, "txtContactorName" + _fetchString(i_crm, "rdbContactorName"));
                    string strTelephone = _fetchString(i_crm, "txtTelephone1" + _fetchString(i_crm, "rdbTelephone1"));
                    string strEmail = _fetchString(i_crm, "txtEmail1" + _fetchString(i_crm, "rdbEmail1"));

                    //更新聯絡人資料
                    OTB_CRM_Contactors oContactors = new OTB_CRM_Contactors
                    {
                        Call = _fetchString(i_crm, "ddlCall" + _fetchString(i_crm, "rdbCall")),
                        ContactorName = _fetchString(i_crm, "txtContactorName" + _fetchString(i_crm, "rdbContactorName")),
                        NickName = _fetchString(i_crm, "txtNickName" + _fetchString(i_crm, "rdbNickName")),
                        Birthday = _fetchString(i_crm, "txtBirthday" + _fetchString(i_crm, "rdbBirthday")),
                        MaritalStatus = _fetchString(i_crm, "ddlMaritalStatus" + _fetchString(i_crm, "rdbMaritalStatus")),
                        PersonalMobilePhone = _fetchString(i_crm, "txtPersonalMobilePhone" + _fetchString(i_crm, "rdbPersonalMobilePhone")),
                        PersonalEmail = _fetchString(i_crm, "txtPersonalEmail" + _fetchString(i_crm, "rdbPersonalEmail")),
                        LINE = _fetchString(i_crm, "txtLINE" + _fetchString(i_crm, "rdbLINE")),
                        WECHAT = _fetchString(i_crm, "txtWECHAT" + _fetchString(i_crm, "rdbWECHAT")),
                        Personality = _fetchString(i_crm, "txtPersonality" + _fetchString(i_crm, "rdbPersonality")),
                        Preferences = _fetchString(i_crm, "txtPreferences" + _fetchString(i_crm, "rdbPreferences")),
                        PersonalAddress = _fetchString(i_crm, "txtPersonalAddress" + _fetchString(i_crm, "rdbPersonalAddress")),
                        Memo = _fetchString(i_crm, "txtMemo" + _fetchString(i_crm, "rdbMemo")),
                        CustomerId = strCustomerId,
                        JobTitle = _fetchString(i_crm, "txtJobTitle" + _fetchString(i_crm, "rdbJobTitle")),
                        Department = _fetchString(i_crm, "txtDepartment" + _fetchString(i_crm, "rdbDepartment")),
                        ImmediateSupervisor = _fetchString(i_crm, "ddlImmediateSupervisor" + _fetchString(i_crm, "rdbImmediateSupervisor")),
                        Telephone1 = _fetchString(i_crm, "txtTelephone1" + _fetchString(i_crm, "rdbTelephone1")),
                        Telephone2 = _fetchString(i_crm, "txtTelephone2" + _fetchString(i_crm, "rdbTelephone2")),
                        Email1 = _fetchString(i_crm, "txtEmail1" + _fetchString(i_crm, "rdbEmail1")),
                        Email2 = _fetchString(i_crm, "txtEmail2" + _fetchString(i_crm, "rdbEmail2")),
                        Ext1 = _fetchString(i_crm, "txtExt1" + _fetchString(i_crm, "rdbExt1")),
                        Ext2 = _fetchString(i_crm, "txtExt2" + _fetchString(i_crm, "rdbExt2")),
                        ChoseReason = _fetchString(i_crm, "txtChoseReason" + _fetchString(i_crm, "rdbChoseReason")),
                        ModifyUser = sUSERID,
                        ModifyDate = dtNow
                    };
                    
                    // 聯絡人
                    var iRel = db.Updateable(oContactors).UpdateColumns(
                        x => new
                        {
                            x.Call,
                            x.ContactorName,
                            x.NickName,
                            x.Birthday,
                            x.MaritalStatus,
                            x.PersonalMobilePhone,
                            x.PersonalEmail,
                            x.LINE,
                            x.WECHAT,
                            x.Personality,
                            x.Preferences,
                            x.PersonalAddress,
                            x.Memo,
                            x.CustomerId,
                            x.JobTitle,
                            x.Department,
                            x.ImmediateSupervisor,
                            x.Telephone1,
                            x.Telephone2,
                            x.Email1,
                            x.Email2,
                            x.Ext1,
                            x.Ext2,
                            x.ChoseReason,
                            x.ModifyUser,
                            x.ModifyDate
                        }).Where(x => x.guid == strUpdateGuid).ExecuteCommand();

                    // 展覽客戶聯絡人關聯資料表
                    List<OTB_OPM_ExhibitionCustomers> listExhibitionCustomer = db.Queryable<OTB_OPM_ExhibitionCustomers>().Where(x => x.CustomerId == strCustomerId).ToList();
                    List<OTB_OPM_ExhibitionContactors> listExhibitionContactors = db.Queryable<OTB_OPM_ExhibitionContactors>().Where(x => x.CustomerId == strCustomerId).ToList();

                    foreach (OTB_OPM_ExhibitionCustomers oExhibitionCustomer in listExhibitionCustomer)
                    {
                        int intCount1 = listExhibitionContactors.Where(x => x.ExhibitionNO == oExhibitionCustomer.ExhibitionNO && x.ContactorId == strUpdateGuid).Count();
                        int intCount2 = listExhibitionContactors.Where(x => x.ExhibitionNO == oExhibitionCustomer.ExhibitionNO && x.ContactorId == strDeleteGuid).Count();
                        
                        if (intCount1 > 0 && intCount2 > 0)
                        {
                            //刪除被合併的聯絡人關聯
                            iRel = db.Deleteable<OTB_OPM_ExhibitionContactors>().Where(x => x.ExhibitionNO == oExhibitionCustomer.ExhibitionNO && x.ContactorId == strDeleteGuid).ExecuteCommand();
                        }
                        else if(intCount1 == 0 && intCount2 > 0)
                        {
                            //更新被合併的聯絡人關聯
                            OTB_OPM_ExhibitionContactors oExhibitionContactors = new OTB_OPM_ExhibitionContactors();
                            oExhibitionContactors.ContactorId = strUpdateGuid;
                            iRel = db.Updateable(oExhibitionContactors).UpdateColumns(it => it.ContactorId).Where(x => x.ContactorId == strDeleteGuid).ExecuteCommand();
                        }
                    }
                    
                    // 進口資料
                    OTB_OPM_ImportExhibition oImportExhibition = new OTB_OPM_ImportExhibition();
                    oImportExhibition.Contactor = strUpdateGuid;
                    oImportExhibition.ContactorName = strContactorName;
                    oImportExhibition.Telephone = strTelephone;
                    oImportExhibition.SupplierEamil = strEmail;
                    iRel = db.Updateable(oImportExhibition).UpdateColumns(it => new
                    {
                        it.Contactor,
                        it.ContactorName,
                        it.Telephone,
                        it.SupplierEamil
                    }).Where(x => x.Contactor == strDeleteGuid).ExecuteCommand();

                    // 其他資料
                    OTB_OPM_OtherExhibition oOtherExhibition = new OTB_OPM_OtherExhibition();
                    oOtherExhibition.Contactor = strUpdateGuid;
                    oOtherExhibition.ContactorName = strContactorName;
                    oOtherExhibition.Telephone = strTelephone;
                    oOtherExhibition.SupplierEamil = strEmail;
                    iRel = db.Updateable(oOtherExhibition).UpdateColumns(it => new
                    {
                        it.Contactor,
                        it.ContactorName,
                        it.Telephone,
                        it.SupplierEamil
                    }).Where(x => x.Contactor == strDeleteGuid).ExecuteCommand();

                    //出口
                    List<OTB_OPM_ExportExhibition> listExportExhibition = db.Queryable<OTB_OPM_ExportExhibition>().Where(x => x.Exhibitors.Contains(strDeleteGuid)).ToList();
                    foreach (OTB_OPM_ExportExhibition oExportExhibition in listExportExhibition)
                    {
                        var jaExhibitors = (JArray)JsonConvert.DeserializeObject(oExportExhibition.Exhibitors);
                        OTB_OPM_ExportExhibition oExport = new OTB_OPM_ExportExhibition();
                        oExport.Exhibitors = oExportExhibition.Exhibitors;
                        foreach (JObject joExhibitors in jaExhibitors)
                        {
                            if (joExhibitors["Contactor"].ToString() == strDeleteGuid)
                            {
                                string strOldValue =
                                    (joExhibitors["Telephone"] == null ? "" : ("\"Telephone\":\"" + joExhibitors["Telephone"].ToString() + "\",")) +
                                    (joExhibitors["Email"] == null ? "" : ("\"Email\":\"" + joExhibitors["Email"].ToString() + "\",")) +
                                    (joExhibitors["ContactorName"] == null ? "" : ("\"ContactorName\":\"" + joExhibitors["ContactorName"].ToString() + "\",")) +
                                    (joExhibitors["CreateUser"] == null ? "" : ("\"CreateUser\":\"" + joExhibitors["CreateUser"].ToString() + "\",")) +
                                    (joExhibitors["CreateDate"] == null ? "" : ("\"CreateDate\":\"" + joExhibitors["CreateDate"].ToString() + "\",")) +
                                    "\"Contactor\":\"" + joExhibitors["Contactor"].ToString();

                                string strNewValeu =
                                    (joExhibitors["Telephone"] == null ? "" : ("\"Telephone\":\"" + strTelephone + "\",")) +
                                    (joExhibitors["Email"] == null ? "" : ("\"Email\":\"" + strEmail + "\",")) +
                                    (joExhibitors["ContactorName"] == null ? "" : ("\"ContactorName\":\"" + strContactorName + "\",")) +
                                    (joExhibitors["CreateUser"] == null ? "" : ("\"CreateUser\":\"" + joExhibitors["CreateUser"].ToString() + "\",")) +
                                    (joExhibitors["CreateDate"] == null ? "" : ("\"CreateDate\":\"" + joExhibitors["CreateDate"].ToString() + "\",")) +
                                    "\"Contactor\":\"" + strUpdateGuid;

                                oExport.Exhibitors = oExport.Exhibitors.Replace(strOldValue, strNewValeu);
                            }
                        }
                        iRel = db.Updateable(oExport).UpdateColumns(it => new
                        {
                            it.Exhibitors
                        }).Where(x => x.ExportBillNO == oExportExhibition.ExportBillNO).ExecuteCommand();
                    }

                    //其他駒驛
                    List<OTB_OPM_OtherExhibitionTG> listOtherExhibitionTG = db.Queryable<OTB_OPM_OtherExhibitionTG>().Where(x => x.Exhibitors.Contains(strDeleteGuid)).ToList();
                    foreach (OTB_OPM_OtherExhibitionTG oOtherExhibitionTG in listOtherExhibitionTG)
                    {
                        var jaExhibitors = (JArray)JsonConvert.DeserializeObject(oOtherExhibitionTG.Exhibitors);
                        OTB_OPM_OtherExhibitionTG oExport = new OTB_OPM_OtherExhibitionTG();
                        oExport.Exhibitors = oOtherExhibitionTG.Exhibitors;
                        foreach (JObject joExhibitors in jaExhibitors)
                        {
                            if (joExhibitors["Contactor"].ToString() == strDeleteGuid)
                            {
                                string strOldValue =
                                    (joExhibitors["Telephone"] == null ? "" : ("\"Telephone\":\"" + joExhibitors["Telephone"].ToString() + "\",")) +
                                    (joExhibitors["Email"] == null ? "" : ("\"Email\":\"" + joExhibitors["Email"].ToString() + "\",")) +
                                    (joExhibitors["ContactorName"] == null ? "" : ("\"ContactorName\":\"" + joExhibitors["ContactorName"].ToString() + "\",")) +
                                    (joExhibitors["CreateUser"] == null ? "" : ("\"CreateUser\":\"" + joExhibitors["CreateUser"].ToString() + "\",")) +
                                    (joExhibitors["CreateDate"] == null ? "" : ("\"CreateDate\":\"" + joExhibitors["CreateDate"].ToString() + "\",")) +
                                    "\"Contactor\":\"" + joExhibitors["Contactor"].ToString();

                                string strNewValeu =
                                    (joExhibitors["Telephone"] == null ? "" : ("\"Telephone\":\"" + strTelephone + "\",")) +
                                    (joExhibitors["Email"] == null ? "" : ("\"Email\":\"" + strEmail + "\",")) +
                                    (joExhibitors["ContactorName"] == null ? "" : ("\"ContactorName\":\"" + strContactorName + "\",")) +
                                    (joExhibitors["CreateUser"] == null ? "" : ("\"CreateUser\":\"" + joExhibitors["CreateUser"].ToString() + "\",")) +
                                    (joExhibitors["CreateDate"] == null ? "" : ("\"CreateDate\":\"" + joExhibitors["CreateDate"].ToString() + "\",")) +
                                    "\"Contactor\":\"" + strUpdateGuid;

                                oExport.Exhibitors = oExport.Exhibitors.Replace(strOldValue, strNewValeu);
                            }
                        }
                        iRel = db.Updateable(oExport).UpdateColumns(it => new
                        {
                            it.Exhibitors
                        }).Where(x => x.Guid == oOtherExhibitionTG.Guid).ExecuteCommand();
                    }

                    // 出口資料、其他業務資料表、其他展覽(駒驛)資料表
                    //SugarParameter[] parameterValue = new SugarParameter[]
                    //{
                    //    new SugarParameter("@UpdateGuid", strUpdateGuid),
                    //    new SugarParameter("@DeleteGuid", strDeleteGuid)
                    //};

                    //iRel = db.Ado.UseStoredProcedure().ExecuteCommand("OSP_OTB_CRM_ContactorCombine", parameterValue);

                    //刪除聯絡人資料
                    iRel = db.Deleteable<OTB_CRM_Contactors>().Where(x => x.guid == strDeleteGuid).ExecuteCommand();

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, iRel);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Contactors_UpdService), "", "CombineContactor（合併聯絡人）", "", "", "");
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

        #endregion 合併聯絡人

        #region 取得已成交展覽名單

        /// <summary>
        /// 取得已成交展覽名單
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

                    List<OTB_OPM_ExportExhibition> listExportExhibition = db.Queryable<OTB_OPM_ExportExhibition>().Where(x => x.Exhibitors.Contains(sId)).ToList();
                    List<OTB_OPM_OtherExhibitionTG> listOtherExhibitionTG = db.Queryable<OTB_OPM_OtherExhibitionTG>().Where(x => x.Exhibitors.Contains(sId)).ToList();
                    List<OTB_OPM_ImportExhibition> listImportExhibition = db.Queryable<OTB_OPM_ImportExhibition>().Where(x => x.Contactor == sId).ToList();
                    List<OTB_OPM_OtherExhibition> listOtherExhibition = db.Queryable<OTB_OPM_OtherExhibition>().Where(x => x.Contactor == sId).ToList();

                    foreach (OTB_OPM_ExportExhibition oExportExhibition in listExportExhibition)
                    {
                        if (!strDealExhibitionNO.Contains(oExportExhibition.ExhibitionNO))
                        {
                            var jaExhibitors = (JArray)JsonConvert.DeserializeObject(oExportExhibition.Exhibitors);

                            foreach (JObject joExhibitors in jaExhibitors)
                            {
                                if (joExhibitors["Contactor"].ToString() == sId && joExhibitors["VoidContent"] == null)
                                {
                                    strDealExhibitionNO = strDealExhibitionNO + oExportExhibition.ExhibitionNO + ",";
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
                                if (joExhibitors["Contactor"].ToString() == sId && joExhibitors["VoidContent"] == null)
                                {
                                    strDealExhibitionNO = strDealExhibitionNO + oOtherExhibitionTG.ExhibitionNO + ",";
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

                    List<View_OPM_Exhibition> listDealExhibition = db.Queryable<OTB_OPM_Exhibition, OTB_SYS_Members>
                    ((t1, t2) =>
                    new object[] {
                        JoinType.Left, t1.OrgID == t2.OrgID && t1.CreateUser == t2.MemberID
                        }
                    )
                    .Where((t1, t2) => SqlFunc.ContainsArray(arrDealExhibitionNO, t1.SN.ToString()))
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
                    .OrderBy("SN", "asc")
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
                            var jaExhibitors = (JArray)JsonConvert.DeserializeObject(oExportExhibition.Exhibitors);

                            foreach (JObject joExhibitors in jaExhibitors)
                            {
                                if (joExhibitors["Contactor"].ToString() == sId && joExhibitors["VoidContent"] == null)
                                {
                                    strRefNumber = strRefNumber + "2;" + oExportExhibition.ExportBillNO + ";" + oExportExhibition.RefNumber + ",";
                                }
                            }
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
                            var jaExhibitors = (JArray)JsonConvert.DeserializeObject(oOtherExhibitionTG.Exhibitors);

                            foreach (JObject joExhibitors in jaExhibitors)
                            {
                                if (joExhibitors["Contactor"].ToString() == sId && joExhibitors["VoidContent"] == null)
                                {
                                    strRefNumber = strRefNumber + "4;" + oOtherExhibitionTG.Guid + ";" + oOtherExhibitionTG.CreateDate + ",";
                                }
                            }
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
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Customers_UpdService), @"聯絡人管理編輯", @"GetDealExhibitionlist（取得已成交展覽名單）", @"", @"", @"");
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

        #endregion 取得已成交展覽名單

        #region 取得未成交展覽名單

        /// <summary>
        /// 取得未成交展覽名單
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

                    OTB_CRM_Contactors oContactors = db.Queryable<OTB_CRM_Contactors>().Single(x => x.guid == sId);

                    List<OTB_OPM_ExhibitionContactors> listExhibitionContactors = db.Queryable<OTB_OPM_ExhibitionContactors>().Where(x => x.ContactorId == sId).ToList();

                    List<OTB_OPM_ExportExhibition> listExportExhibition = db.Queryable<OTB_OPM_ExportExhibition>().Where(x => x.Exhibitors.Contains(sId)).ToList();
                    List<OTB_OPM_OtherExhibitionTG> listOtherExhibitionTG = db.Queryable<OTB_OPM_OtherExhibitionTG>().Where(x => x.Exhibitors.Contains(sId)).ToList();
                    List<OTB_OPM_ImportExhibition> listImportExhibition = db.Queryable<OTB_OPM_ImportExhibition>().Where(x => x.Contactor == sId).ToList();
                    List<OTB_OPM_OtherExhibition> listOtherExhibition = db.Queryable<OTB_OPM_OtherExhibition>().Where(x => x.Contactor == sId).ToList();

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

                    foreach (OTB_OPM_ExhibitionContactors oExhibitionContactors in listExhibitionContactors)
                    {
                        if (!strDealExhibitionNO.Contains(oExhibitionContactors.ExhibitionNO))
                        {
                            strUnDealExhibitionNO = strUnDealExhibitionNO + oExhibitionContactors.ExhibitionNO + ",";
                        }
                    }

                    strUnDealExhibitionNO = strUnDealExhibitionNO.TrimEnd(',');
                    string[] arrUnDealExhibitionNO = strUnDealExhibitionNO.Split(',');

                    List<View_OPM_Exhibition> listUnDealExhibition = db.Queryable<OTB_OPM_Exhibition, OTB_SYS_Members>
                    ((t1, t2) =>
                    new object[] {
                        JoinType.Left, t1.OrgID == t2.OrgID && t1.CreateUser == t2.MemberID
                        }
                    )
                    .Where((t1, t2) => SqlFunc.ContainsArray(arrUnDealExhibitionNO, t1.SN.ToString()))
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
                    .OrderBy("SN", "asc")
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
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Customers_UpdService), @"聯絡人管理編輯", @"GetUnDealExhibitionlist（取得未成交展覽名單）", @"", @"", @"");
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

        #endregion 取得未成交展覽名單

        #region 新增至展覽名單

        /// <summary>
        /// 新增至展覽名單
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on CombineCustomer</param>
        /// <returns></returns>
        public ResponseMessage InsertExhibitionList(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var iRel = 0;
                    int intCount = 0;

                    string strExhibitionNO = _fetchString(i_crm, "ExhibitionNO");
                    string strCustomerId = _fetchString(i_crm, "CustomerId");
                    string strContactorId = _fetchString(i_crm, "ContactorId");
                    string sUSERID = i_crm.USERID;
                    DateTime dtNow = DateTime.Now;

                    //判斷此客戶是否已存在該展覽之名單中
                    intCount = db.Queryable<OTB_OPM_ExhibitionCustomers>().Where(x => x.ExhibitionNO == strExhibitionNO && x.CustomerId == strCustomerId).Count();

                    if (intCount == 0)
                    {
                        //新增展覽客戶關聯
                        OTB_OPM_ExhibitionCustomers oExhibitionCustomers = new OTB_OPM_ExhibitionCustomers();
                        oExhibitionCustomers.ExhibitionNO = strExhibitionNO;
                        oExhibitionCustomers.CustomerId = strCustomerId;
                        oExhibitionCustomers.SourceType = "5";
                        oExhibitionCustomers.CreateUser = sUSERID;
                        oExhibitionCustomers.CreateDate = dtNow;
                        oExhibitionCustomers.ModifyUser = sUSERID;
                        oExhibitionCustomers.ModifyDate = dtNow;
                        db.Insertable(oExhibitionCustomers).ExecuteCommand();

                        //新增展覽客戶聯絡人關聯
                        OTB_OPM_ExhibitionContactors oExhibitionContactors = new OTB_OPM_ExhibitionContactors();
                        oExhibitionContactors.ExhibitionNO = strExhibitionNO;
                        oExhibitionContactors.CustomerId = strCustomerId;
                        oExhibitionContactors.ContactorId = strContactorId;
                        oExhibitionContactors.CreateUser = sUSERID;
                        oExhibitionContactors.CreateDate = dtNow;
                        oExhibitionContactors.ModifyUser = sUSERID;
                        oExhibitionContactors.ModifyDate = dtNow;
                        oExhibitionContactors.SourceType = "5";

                        db.Insertable(oExhibitionContactors).ExecuteCommand();

                        iRel = 1;
                    }
                    else
                    {
                        intCount = 0;
                        //判斷此聯絡人是否已存在該展覽之名單中
                        intCount = db.Queryable<OTB_OPM_ExhibitionContactors>().Where(x => x.ExhibitionNO == strExhibitionNO && x.CustomerId == strCustomerId && x.ContactorId == strContactorId).Count();

                        if (intCount == 0)
                        {
                            //新增展覽客戶聯絡人關聯
                            OTB_OPM_ExhibitionContactors oExhibitionContactors = new OTB_OPM_ExhibitionContactors();
                            oExhibitionContactors.ExhibitionNO = strExhibitionNO;
                            oExhibitionContactors.CustomerId = strCustomerId;
                            oExhibitionContactors.ContactorId = strContactorId;
                            oExhibitionContactors.CreateUser = sUSERID;
                            oExhibitionContactors.CreateDate = dtNow;
                            oExhibitionContactors.ModifyUser = sUSERID;
                            oExhibitionContactors.ModifyDate = dtNow;
                            oExhibitionContactors.SourceType = "5";

                            db.Insertable(oExhibitionContactors).ExecuteCommand();

                            iRel = 1;
                        }
                        else
                        {
                            sMsg = "已存在該展覽之名單，無法新增";
                        }
                    }

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, iRel);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Customers_UpdService), "", "InsertExhibitionList（新增至展覽名單）", "", "", "");
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
        #endregion 新增至展覽名單

        #region 取得相同客戶的聯絡人名單(不包含自己) - 下拉選單用
        /// <summary>
        /// 取得相同客戶的聯絡人名單(不包含自己) - 下拉選單用
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage GetImmediateSupervisor(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.DB;
            try
            {
                do
                {
                    var sId = _fetchString(i_crm, @"Guid");
                    var sCustomerId = _fetchString(i_crm, @"CustomerId");

                    var strGuid1 = _fetchString(i_crm, @"Guid1");
                    var strGuid2 = _fetchString(i_crm, @"Guid2");


                    string strCustomerId = "";

                    if (sCustomerId != null)
                    {
                        strCustomerId = sCustomerId;
                        var listImmediateSupervisor =
                            db.Queryable<OTB_CRM_Contactors>()
                            .Where(x => x.CustomerId == strCustomerId)
                            .Select(x => new
                            {
                                id = x.guid,
                                text = x.ContactorName
                            }).ToList();

                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, listImmediateSupervisor);
                    }
                    else if (sId != null)
                    {
                        strCustomerId = db.Queryable<OTB_CRM_Contactors>().Single(x => x.guid == sId).CustomerId;
                        var listImmediateSupervisor =
                            db.Queryable<OTB_CRM_Contactors>()
                            .Where(x => x.CustomerId == strCustomerId && x.guid != sId)
                             .Select(x => new
                             {
                                 id = x.guid,
                                 text = x.ContactorName
                             }).ToList();

                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, listImmediateSupervisor);
                    }
                    else if (strGuid1 != null && strGuid2 != null)
                    {
                        strCustomerId = db.Queryable<OTB_CRM_Contactors>().Single(x => x.guid == strGuid1).CustomerId;
                        var listImmediateSupervisor =
                            db.Queryable<OTB_CRM_Contactors>()
                            .Where(x => x.CustomerId == strCustomerId)
                             .Select(x => new
                             {
                                 id = x.guid,
                                 text = x.ContactorName
                             }).ToList();

                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, listImmediateSupervisor);
                    }
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.CommonService", "公用API", "GetImmediateSupervisor（取得相同客戶的聯絡人名單(不包含自己) - 下拉選單用）", "", "", "");
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
        #endregion 取得相同客戶的聯絡人名單(不包含自己) - 下拉選單用


        #region 取得客戶名單 - 下拉選單用
        /// <summary>
        /// 取得客戶名單 - 下拉選單用
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage GetCustomerCName(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.DB;
            try
            {
                do
                {
                    var listCustomerId =
                        db.Queryable<OTB_CRM_Customers>()
                        .Select(x => new
                        {
                            id = x.guid,
                            text = x.CustomerCName
                        }).ToList();
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, listCustomerId);

                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.CommonService", "公用API", "GetCustomerCName（ 取得客戶名單 - 下拉選單用）", "", "", "");
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

        #region 取得客訴列表

        /// <summary>
        /// 取得客訴列表
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on GetContactorlist</param>
        /// <returns></returns>
        public ResponseMessage GetComplaintlist(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var sContactorName = _fetchString(i_crm, @"ContactorName");
                    var sCustomerId = _fetchString(i_crm, @"CustomerId");

                    var listComplaint = db.Queryable<OTB_CRM_Complaint, OTB_OPM_Exhibition>
                       ((t1, t2) =>
                       new object[] {
                                JoinType.Inner, t1.ExhibitionNO == t2.SN.ToString()
                             }
                       ).Where((t1, t2) => t1.CustomerId == sCustomerId && t1.Complainant == sContactorName)
                       .Select((t1, t2) => new View_CRM_Complaint
                       {
                           Guid = t1.Guid,
                           ComplaintNumber = t1.ComplaintNumber,
                           ComplaintTitle = t1.ComplaintTitle,
                           ExhibitioShotName_TW = t2.ExhibitioShotName_TW,
                           ExhibitionName = t2.Exhibitioname_TW,
                           ComplaintType = t1.ComplaintType,
                           Description = t1.Description,
                           DataType = t1.DataType,
                           CreateUser = t1.CreateUser,
                           CreateUserName = SqlFunc.MappingColumn(t1.Guid, "dbo.[OFN_SYS_MemberNameByMemberIDwithoutOrgID](t1.CreateUser)")
                       })
                       .MergeTable().OrderBy("ComplaintNumber", "desc").ToPageList(1, 999999999);

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, listComplaint);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Callout_UpdService), @"客戶管理編輯", @"GetComplaintlist（取得客訴列表）", @"", @"", @"");
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

        #endregion 取得客訴列表
    }
}
