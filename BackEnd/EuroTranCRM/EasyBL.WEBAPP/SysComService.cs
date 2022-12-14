using EasyBL.WebApi.Message;
using Entity.Sugar;
using Newtonsoft.Json.Linq;
using SqlSugar;
using SqlSugar.Base;
using System;
using System.Collections.Generic;
using System.Data;

namespace EasyBL.WEBAPP
{
    public class SysComService : ServiceBase
    {
        #region 查詢系統所有功能資料

        /// <summary>
        /// 查詢系統所有功能資料
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage GetSysFNList(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.DB;
            try
            {
                do
                {
                    var spOrgID = new SugarParameter("@OrgID", i_crm.ORIGID);
                    var spUserID = new SugarParameter("@UserID", i_crm.USERID);
                    var dt = db.Ado.UseStoredProcedure().GetDataTable("OSP_Common_GetSysFnListByUserID", spOrgID, spUserID);
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, dt);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(SysComService), "", @"GetSysFNList(查詢系統所有功能資料)", @"", @"", @"");
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

        #endregion 查詢系統所有功能資料

        #region 查詢系統所模組

        /// <summary>
        /// 查詢系統所模組
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage GetModuleList(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.DB;
            try
            {
                do
                {
                    var spOrgID = new SugarParameter("@OrgID", i_crm.ORIGID);
                    var dt = db.Ado.UseStoredProcedure().GetDataTable("OSP_Common_GetModuleList", spOrgID);
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, dt);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(SysComService), "", @"GetModuleList(查詢系統所模組)", @"", @"", @"");
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

        #endregion 查詢系統所模組

        #region 查詢所有有效的正式客戶資料

        /// <summary>
        /// 查詢所有有效的正式客戶資料
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage GetCustomerlist(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.DB;
            try
            {
                do
                {
                    var spm = new SugarParameter(@"OrgID", i_crm.ORIGID);
                    System.Data.DataTable saCustomerlist = new System.Data.DataTable();
                    if (i_crm.ORIGID == "TG" || i_crm.ORIGID == "TE")
                    {
                        saCustomerlist = db.Ado.GetDataTable(@"select guid id,CustomerNO CusNO,case isnull(CustomerShotCName,'') when '' then (case CustomerCName when '' then CustomerEName else CustomerCName end) else '('+CustomerShotCName+')'+(case CustomerCName when '' then CustomerEName else CustomerCName end)+'('+OrgID+')' end text,CustomerCName textcn,CustomerEName texteg,UniCode,dbo.OFN_CRM_GetContactorByCustomerID(guid) Contactors,Email,Telephone,IsAudit,case Effective when 'Y' then 'N' else 'Y' end disabled from OTB_CRM_Customers where OrgID IN ('TE','TG') AND IsAudit <> 'Q' AND IsAudit <> 'N' AND (Effective = 'Y' OR NotPassReason = 'CustomerCombine') order by CustomerCName desc", spm);
                    } else
                    {
                        saCustomerlist = db.Ado.GetDataTable(@"select guid id,CustomerNO CusNO,case isnull(CustomerShotCName,'') when '' then (case CustomerCName when '' then CustomerEName else CustomerCName end) else '('+CustomerShotCName+')'+(case CustomerCName when '' then CustomerEName else CustomerCName end)+'('+OrgID+')' end text,CustomerCName textcn,CustomerEName texteg,UniCode,dbo.OFN_CRM_GetContactorByCustomerID(guid) Contactors,Email,Telephone,IsAudit from OTB_CRM_Customers where OrgID = '" + i_crm.ORIGID + "' AND Effective = 'Y' AND IsAudit <> 'Q' AND IsAudit <> 'N' order by CustomerCName desc", spm);
                    }
                    //else if(i_crm.ORIGID == "SG")
                    //{
                    //    saCustomerlist = db.Ado.GetDataTable(@"select guid id,CustomerNO CusNO,case isnull(CustomerShotCName,'') when '' then (case CustomerCName when '' then CustomerEName else CustomerCName end) else '('+CustomerShotCName+')'+(case CustomerCName when '' then CustomerEName else CustomerCName end)+'('+OrgID+')' end text,CustomerCName textcn,CustomerEName texteg,UniCode,dbo.OFN_CRM_GetContactorByCustomerID(guid) Contactors,Email,Telephone,IsAudit from OTB_CRM_Customers where OrgID = 'SG' AND Effective = 'Y' AND IsAudit <> 'Q' AND IsAudit <> 'N' order by CustomerCName desc", spm);
                    //}
                    //else if (i_crm.ORIGID == "SE")
                    //{
                    //    saCustomerlist = db.Ado.GetDataTable(@"select guid id,CustomerNO CusNO,case isnull(CustomerShotCName,'') when '' then (case CustomerCName when '' then CustomerEName else CustomerCName end) else '('+CustomerShotCName+')'+(case CustomerCName when '' then CustomerEName else CustomerCName end)+'('+OrgID+')' end text,CustomerCName textcn,CustomerEName texteg,UniCode,dbo.OFN_CRM_GetContactorByCustomerID(guid) Contactors,Email,Telephone,IsAudit from OTB_CRM_Customers where OrgID = 'SE' AND Effective = 'Y' AND IsAudit <> 'Q' AND IsAudit <> 'N' order by CustomerCName desc", spm);
                    //}

                    foreach (DataRow dr in saCustomerlist.Rows)
                    {
                        if (dr["Contactors"].ToString() != "")
                        {
                            dr["Contactors"] = dr["Contactors"].ToString().Replace("\r\n", " ").Replace("\t", " ");
                        }
                    }

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, saCustomerlist);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(SysComService), "", @"GetCustomerlist(查詢所有有效的客戶資料)", @"", @"", @"");
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

        #endregion 查詢所有有效的正式客戶資料

        #region 查詢系統所有部門

        /// <summary>
        /// 查詢系統所有部門
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage GetDepartmentList(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.DB;
            try
            {
                do
                {
                    var sDeptID = _fetchString(i_crm, @"DeptID");
                    var spOrgID = new SugarParameter("@OrgID", i_crm.ORIGID);
                    var spDeptID = new SugarParameter("@DeptID", sDeptID);
                    var dt = db.Ado.UseStoredProcedure().GetDataTable("OSP_Common_GetDepartmentList", spOrgID, spDeptID);
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, dt);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(SysComService), "", @"GetDepartmentList(查詢系統所有部門)", @"", @"", @"");
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

        #endregion 查詢系統所有部門

        public ResponseMessage GetDepartmentListNoVoid(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.DB;
            try
            {
                do
                {
                    var sDeptID = _fetchString(i_crm, @"DeptID");
                    var spOrgID = new SugarParameter("@OrgID", i_crm.ORIGID);
                    var spDeptID = new SugarParameter("@DeptID", sDeptID);
                    var dt = db.Ado.UseStoredProcedure().GetDataTable("OSP_Common_GetDepartmentListNoVoid", spOrgID, spDeptID);
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, dt);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(SysComService), "", @"GetDepartmentList(查詢系統所有部門)", @"", @"", @"");
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
        #region 查詢系統所有人員

        /// <summary>
        /// 查詢系統所有人員
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage GetAllMembersByUserId(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.DB;
            try
            {
                do
                {
                    var spOrgID = new SugarParameter("@OrgID", i_crm.ORIGID);
                    var spUserID = new SugarParameter("@UserID", i_crm.USERID);
                    var dt = db.Ado.UseStoredProcedure().GetDataTable("OSP_Common_GetAllMemberListByUserID", spOrgID, spUserID);
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, dt);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(SysComService), "", @"GetAllMembersByUserId(查詢系統所有人員)", @"", @"", @"");
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

        #endregion 查詢系統所有人員

        #region 查詢個人信息

        /// <summary>
        /// 查詢個人信息
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage GetUserInfo(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.DB;
            try
            {
                do
                {
                    var saSpm = new List<SugarParameter>();
                    var spm1 = new SugarParameter(@"OrgID", i_crm.ORIGID);
                    saSpm.Add(spm1);
                    var spm2 = new SugarParameter(@"UserID", i_crm.USERID);
                    saSpm.Add(spm2);
                    var saUserInfo = db.Ado.GetDataTable(@"SELECT MemberID,MemberName,Email,j.JobtitleName,m.DepartmentID,d.DepartmentName,m.Effective,CalColor,MemberPic,SysShowMode,Country,ServiceCode,Address
                      ,(SELECT RuleID+',' FROM dbo.OTB_SYS_MembersToRule WHERE MemberID=@UserID and OrgID=@OrgID FOR XML PATH('')) AS roles,
                      ImmediateSupervisor+','+(select ISNULL(ChiefOfDepartmentID,'') from OTB_SYS_Departments where DepartmentID=m.DepartmentID and OrgID=m.OrgID) as Supervisors,
                      ISNULL((select MemberID+',' FROM OTB_SYS_Members where DepartmentID in (select DepartmentID from OTB_SYS_Departments where ChiefOfDepartmentID=m.MemberID and OrgID=m.OrgID) and OrgID=@OrgID for xml path('')),'') as UsersDown,
                      ISNULL((select MemberID+',' FROM OTB_SYS_Members c where c.ImmediateSupervisor=m.MemberID  and c.OrgID=m.OrgID  for xml path('')),'') as UsersBranch
                      FROM OTB_SYS_Members m
                      inner join OTB_SYS_Jobtitle j on m.JobTitle=j.JobtitleID and m.OrgID=j.OrgID
                      inner join OTB_SYS_Departments d on m.DepartmentID=d.DepartmentID and m.OrgID=d.OrgID
                      WHERE MemberID=@UserID and m.OrgID=@OrgID", saSpm);
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, saUserInfo.Rows[0]);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(SysComService), "", @"GetUserInfo(查詢個人信息)", @"", @"", @"");
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

        #endregion 查詢個人信息

        #region 查詢所有有效的出口資料

        /// <summary>
        /// 查詢所有有效的出口資料
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage GetExhibitionExportlist(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.DB;
            try
            {
                do
                {
                    var spm = new SugarParameter(@"OrgID", i_crm.ORIGID);

                    string strCommand = "";

                    if (i_crm.ORIGID == "TE")
                    {
                        strCommand = @"select ExportBillNO id, ExportBillName + ' (' + RefNumber + ')' text, ExportBillName + ' (' + RefNumber + ')' texten from OTB_OPM_ExportExhibition where IsVoid = 'N'";
                    } else  if (i_crm.ORIGID == "TG")
                    {
                        strCommand = @"select Guid id, ImportBillName + '(' + CONVERT(VARCHAR, CreateDate, 111) + ')' text, ImportBillEName + '(' + CONVERT(VARCHAR, CreateDate, 111) + ')' texten from OTB_OPM_OtherExhibitionTG where IsVoid = 'N'";
                    }

                    var saCustomerlist = db.Ado.GetDataTable(strCommand, spm);
                    
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, saCustomerlist);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(SysComService), "", @"GetExhibitionExportlist(查詢所有有效的出口資料)", @"", @"", @"");
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

        #endregion 查詢所有有效的出口資料

        #region 查詢所有有效的客戶資料

        /// <summary>
        /// 查詢所有有效的客戶資料
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage GetAllCustomerlist(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.DB;
            try
            {
                do
                {
                    var spm = new SugarParameter(@"OrgID", i_crm.ORIGID);
                    System.Data.DataTable saCustomerlist = new System.Data.DataTable();
                    if (i_crm.ORIGID == "TG" || i_crm.ORIGID == "TE")
                    {
                        saCustomerlist = db.Ado.GetDataTable(@"select guid id,CustomerNO CusNO,case isnull(CustomerShotCName,'') when '' then (case CustomerCName when '' then CustomerEName else CustomerCName end) else '('+CustomerShotCName+')'+(case CustomerCName when '' then CustomerEName else CustomerCName end)+'('+OrgID+')' end text,CustomerCName textcn,CustomerEName texteg,UniCode,Email,Telephone,IsAudit,case Effective when 'Y' then 'N' else 'Y' end disabled from OTB_CRM_Customers where OrgID IN ('TE','TG') AND (Effective = 'Y' OR NotPassReason = 'CustomerCombine') order by CustomerCName desc", spm);
                    } else
                    {
                        saCustomerlist = db.Ado.GetDataTable(@"select guid id,CustomerNO CusNO,case isnull(CustomerShotCName,'') when '' then (case CustomerCName when '' then CustomerEName else CustomerCName end) else '('+CustomerShotCName+')'+(case CustomerCName when '' then CustomerEName else CustomerCName end)+'('+OrgID+')' end text,CustomerCName textcn,CustomerEName texteg,UniCode,Email,Telephone,IsAudit from OTB_CRM_Customers where OrgID = '" + i_crm.ORIGID + "' AND Effective = 'Y' order by CustomerCName desc", spm);
                    }

                    //else if (i_crm.ORIGID == "SG")
                    //{
                    //    saCustomerlist = db.Ado.GetDataTable(@"select guid id,CustomerNO CusNO,case isnull(CustomerShotCName,'') when '' then (case CustomerCName when '' then CustomerEName else CustomerCName end) else '('+CustomerShotCName+')'+(case CustomerCName when '' then CustomerEName else CustomerCName end)+'('+OrgID+')' end text,CustomerCName textcn,CustomerEName texteg,UniCode,Email,Telephone,IsAudit from OTB_CRM_Customers where OrgID = 'SG' AND Effective = 'Y' order by CustomerCName desc", spm);
                    //}
                    //else if (i_crm.ORIGID == "SE")
                    //{
                    //    saCustomerlist = db.Ado.GetDataTable(@"select guid id,CustomerNO CusNO,case isnull(CustomerShotCName,'') when '' then (case CustomerCName when '' then CustomerEName else CustomerCName end) else '('+CustomerShotCName+')'+(case CustomerCName when '' then CustomerEName else CustomerCName end)+'('+OrgID+')' end text,CustomerCName textcn,CustomerEName texteg,UniCode,Email,Telephone,IsAudit from OTB_CRM_Customers where OrgID = 'SE' AND Effective = 'Y' order by CustomerCName desc", spm);
                    //}

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, saCustomerlist);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(SysComService), "", @"GetAuditCustomerlist(查詢所有有效的客戶資料)", @"", @"", @"");
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

        #endregion 查詢所有有效的客戶資料
    }
}