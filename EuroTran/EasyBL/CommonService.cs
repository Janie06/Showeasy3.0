using System;
using System.Collections.Generic;
using EasyNet.DBUtility;
using EasyNet;
using Entity.Sugar;
using EasyBL.WebApi.Message;
using SqlSugar.Base;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using SqlSugar;
using System.Net.Sockets;
using System.Linq;

namespace EasyBL
{
    public class CommonService : ServiceBase
    {
        #region 查詢單筆資料

        /// <summary>
        /// 查詢單筆資料
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage QueryOne(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            try
            {
                do
                {
                    var rel = DBHelper.QueryOne("", i_crm.DATA);
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, rel);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.CommonService", "公用API", "QueryOne(查詢單筆資料)", "", "", "");
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

        #endregion 查詢單筆資料

        #region 獲取流水號

        /// <summary>
        /// 獲取流水號
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage GetSerialNumber(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var list = new List<OTB_CRM_Customers>();
            try
            {
                do
                {
                    var iLen = 0;
                    var sType = _fetchString(i_crm, "Type");
                    var sFlag = _fetchString(i_crm, "Flag");
                    var sLen = _fetchString(i_crm, "Len");
                    var sStr = _fetchString(i_crm, "Str");
                    var sAddType = _fetchString(i_crm, "AddType");
                    var sPlusType = _fetchString(i_crm, "PlusType");
                    if (!string.IsNullOrWhiteSpace(sLen))
                    {
                        iLen = int.Parse(sLen);
                    }
                    var sSerialNumber = SerialNumber.GetMaxNumberByType(i_crm.ORIGID, sType, SerialNumber.GetMaxNumberType(sFlag), i_crm.USERID, iLen, sStr, sAddType);    //獲取最大編號
                    if (sPlusType == "checkcode")
                    {
                        sSerialNumber += SerialNumber.Pcheck(sSerialNumber);
                    }
                    else if (sPlusType.StartsWith("randomcode_"))
                    {
                        var saPlusType = sPlusType.Split('_');
                        var iLen_Chid = int.Parse(saPlusType[1].ToString());
                        sSerialNumber += SecurityUtil.GetRandomNumber(iLen_Chid);
                    }
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, sSerialNumber);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.CommonService", "公用API", "GetSerialNumber(獲取流水號)", "", "", "");
            }
            finally
            {
                if (null != sMsg)
                {
                    rm = new ErrorResponseMessage(sMsg, i_crm);
                }
                LogService.mo_Log.Debug("CommonService.GetSerialNumber Debug（Param：" + JsonToString(i_crm) + "；Response：" + JsonToString(rm) + "）---------------");
            }
            return rm;
        }

        #endregion 獲取流水號

        #region 獲取單個系統配置值

        /// <summary>
        /// 獲取單個系統配置值
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage GetSysSet(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var sSetItemID = _fetchString(i_crm, "SetItemID");
                    var oSetItem = new object();
                    if (string.IsNullOrEmpty(sSetItemID))
                    {
                        oSetItem = db.Queryable<OTB_SYS_SystemSetting>()
                            .Where(x => x.OrgID == i_crm.ORIGID && x.Effective == "Y")
                            .Select(x => new { x.SettingItem, x.SettingValue })
                            .ToList();
                    }
                    else
                    {
                        oSetItem = db.Queryable<OTB_SYS_SystemSetting>()
                            .Where(x => x.OrgID == i_crm.ORIGID && x.SettingItem == sSetItemID)
                            .Select(x => x.SettingValue)
                            .Single();
                    }

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, oSetItem);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.CommonService", "公用API", "GetSysSet（獲取單個系統配置值）", "", "", "");
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

        #endregion 獲取單個系統配置值

        #region 獲取參數值

        /// <summary>
        /// 獲取參數值
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage GetArguments(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.DB;
            try
            {
                do
                {
                    var sArgClassID = _fetchString(i_crm, "ArgClassID");
                    var sParentID = _fetchString(i_crm, "ParentID");
                    var sArgIDs = _fetchString(i_crm, "ArgIDs");
                    var sOrderBy = _fetchString(i_crm, "OrderBy") ?? "OrderByValue";
                    var sOrderType = _fetchString(i_crm, "OrderType") ?? "asc";
                    var iLevelOfArgument = _fetchInt(i_crm, "LevelOfArgument");
                    var sOrgID = _fetchString(i_crm, "OrgID");
                    var UsingOrgID = string.IsNullOrWhiteSpace(sOrgID) ? i_crm.ORIGID: sOrgID;
                    var saArguments = db.Queryable<OTB_SYS_Arguments>()
                        .OrderBy(sOrderBy, sOrderType)
                        .Where(x => x.OrgID == UsingOrgID && x.DelStatus == "N" && x.Effective == "Y")
                        .Where(x => x.ArgumentClassID == sArgClassID)
                        .WhereIF(iLevelOfArgument != -1, x => x.LevelOfArgument == iLevelOfArgument)
                        .WhereIF(!string.IsNullOrEmpty(sArgIDs), x => sArgIDs.Contains(x.ArgumentID))
                        .WhereIF(!string.IsNullOrEmpty(sParentID), x => SqlFunc.IsNull(x.ParentArgument, "") == sParentID)
                        .Select(x => new
                        {
                            id = x.ArgumentID,
                            text = x.ArgumentValue,
                            text_cn = x.ArgumentValue_CN,
                            text_en = x.ArgumentValue_EN,
                            label = x.ArgumentID + "-" + x.ArgumentValue,
                            x.Correlation
                        })
                        .ToList();
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, saArguments);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.CommonService", "公用API", "GetArguments（獲取參數值）", "", "", "");
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

        #endregion 獲取參數值

        #region 獲取人員資料

        /// <summary>
        /// 獲取人員資料
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage GetUserList(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.DB;
            try
            {
                do
                {
                    var sDepartmentID = _fetchString(i_crm, "DepartmentID");
                    var sMemberID = _fetchString(i_crm, "MemberID");
                    var sUserIDs = _fetchString(i_crm, "UserIDs");
                    var sNotUserIDs = _fetchString(i_crm, "NotUserIDs");
                    var sServiceCode = _fetchString(i_crm, "ServiceCode");
                    var sEffective = _fetchString(i_crm, "Effective");

                    var saQueryMembers = db.Queryable<OTB_SYS_Members, OTB_SYS_Departments, OTB_SYS_Jobtitle>
                        ((t1, t2, t3) =>
                        new object[] {
                                JoinType.Inner, t1.OrgID == t2.OrgID && t1.DepartmentID == t2.DepartmentID,
                                JoinType.Inner, t1.OrgID == t3.OrgID && t1.JobTitle == t3.JobtitleID
                              }
                        )
                        .OrderBy((t1, t2, t3) => t1.MemberID)
                        .Where((t1, t2, t3) => t1.OrgID == i_crm.ORIGID && t1.ServiceCode != "API")
                        .WhereIF(!string.IsNullOrEmpty(sDepartmentID), (t1, t2, t3) => t1.DepartmentID == sDepartmentID)
                        .WhereIF(!string.IsNullOrEmpty(sMemberID), (t1, t2, t3) => t1.MemberID == sMemberID)
                        .WhereIF(!string.IsNullOrEmpty(sUserIDs), (t1, t2, t3) => sUserIDs.Contains(t1.MemberID))
                        .WhereIF(!string.IsNullOrEmpty(sNotUserIDs), (t1, t2, t3) => !sNotUserIDs.Contains(t1.MemberID))
                        .WhereIF(!string.IsNullOrEmpty(sServiceCode), (t1, t2, t3) => sServiceCode.Contains(t1.ServiceCode))
                        .WhereIF(!string.IsNullOrEmpty(sEffective), (t1, t2, t3) => t1.Effective == sEffective)
                        .Select((t1, t2, t3) => new
                        {
                            t1.OrgID,
                            t1.MemberID,
                            t1.MemberName,
                            t1.Email,
                            t1.OutlookAccount,
                            t1.ServiceCode,
                            t1.DepartmentID,
                            t1.MemberPic,
                            t1.Effective,
                            t2.DepartmentName,
                            t3.JobtitleName
                        })
                        .ToList();
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, saQueryMembers);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.CommonService", "公用API", "GetUserList（獲取人員資料）", "", "", "");
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

        #endregion 獲取人員資料

        #region 獲取Office模版

        /// <summary>
        /// 獲取Office模版
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage GetOfficeTempls(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.DB;
            try
            {
                do
                {
                    var sTemplID = _fetchString(i_crm, "TemplID");

                    var saOfficeTemplate = db.Queryable<OTB_SYS_OfficeTemplate>()
                        .OrderBy(x => x.TemplID)
                        .Where(x => x.OrgID == i_crm.ORIGID)
                        .WhereIF(!string.IsNullOrEmpty(sTemplID), x => sTemplID.Contains(x.TemplID))
                        //.Select(x => new { x.MemberID, x.MemberName, x.ServiceCode, x.DepartmentName, x.JobtitleName })
                        .ToList();
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, saOfficeTemplate);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.CommonService", "公用API", "GetOfficeTempls（ 獲取Office模版）", "", "", "");
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

        #endregion 獲取Office模版

        #region 獲取首頁公告

        /// <summary>
        /// 獲取首頁公告
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage GetAnnlist(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.DB;
            try
            {
                do
                {
                    var saAnnlist = db.Queryable<OTB_SYS_Announcement, OTB_SYS_Arguments, OTB_SYS_Members, OTB_SYS_Announcement_Read>((a, b, c, d) => new object[] {
                         JoinType.Inner, a.OrgID==b.OrgID && a.Ann_Type==b.ArgumentID&&b.ArgumentClassID=="Ann_Type",
                         JoinType.Inner, a.OrgID==c.OrgID && a.CreateUser==c.MemberID,
                         JoinType.Left, a.AnnouncementID==d.AnnouncementID && d.CreateUser==i_crm.USERID
                    })
                           .Where((a) => a.OrgID == i_crm.ORIGID && a.StartDateTime <= DateTime.Now && a.EndDateTime >= DateTime.Now)
                           .Select((a, b, c, d) =>
                           new
                           {
                               a.AnnouncementID,
                               a.Ann_Type,
                               a.Title,
                               a.Description,
                               a.FontColor,
                               a.StartDateTime,
                               a.EndDateTime,
                               a.CreateUser,
                               a.CreateDate,
                               a.GoTop,
                               a.GoTop_Time,
                               Ann_TypeName = b.ArgumentValue,
                               CreateUserName = c.MemberName,
                               IsAlert = d.SN
                           }).ToList();
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, saAnnlist);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.CommonService", "公用API", "GetAnnlist（獲取首頁公告）", "", "", "");
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

        #endregion 獲取首頁公告


        #region 取得上週缺勤

        public ResponseMessage GetAbsenceFromLastWeek(RequestMessage i_crm)
        {
            var StartDt = DateTime.Now;
            if (StartDt.DayOfWeek != DayOfWeek.Monday)
                return new ResponseMessage();
            ResponseMessage rm = null;
            string sMsg = null;
            StartDt = StartDt.AddDays(-7).Date;
            var EndDt = StartDt.AddDays(4).Date.AddSeconds( 24*60 * 60  -1);

            var db = SugarBase.DB;
            try
            {
                do
                {
                    var ToBeNotification = new List<OTB_EIP_Attendance>();
                    var saAttendances = db.Queryable<OTB_EIP_Attendance>()
                    .Where( x => x.CardDate >= StartDt  && x.CardDate < EndDt && !SqlFunc.IsNullOrEmpty(x.Memo) && x.OrgID == i_crm.ORIGID && x.UserID == i_crm.USERID)
                    .ToList();
                    //取得所有差勤異常(by 日子)
                    var saAttendanceDiffStatus = new string[] { @"B", @"E", @"H-O" };
                    var saAttendanceDiff = db.Queryable<OTB_EIP_AttendanceDiff>().Where(it => saAttendanceDiffStatus.Contains(it.Status) && it.AskTheDummy == i_crm.USERID).ToList();
                    //取得請假資料
                    var saLeaveStatus = new string[] { @"B", @"E", @"H-O" };
                    var saLeave = db.Queryable<OTB_EIP_Leave>().Where(it => saLeaveStatus.Contains(it.Status) && it.AskTheDummy == i_crm.USERID).ToList();
                    foreach (var Ad in saAttendances)
                    {
                        var ThisDay = Ad.CardDate.Date;
                        var FoundInAttendaceDiff = saAttendanceDiff.Any(c => c.FillBrushDate.Value.Date == ThisDay);
                        var FoundInLeave = saLeave.Any(c => c.StartDate.Value.Date <= ThisDay && c.EndDate.Value.Date >= ThisDay);
                        if (!FoundInAttendaceDiff && !FoundInLeave)
                            ToBeNotification.Add(Ad);

                    }
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, ToBeNotification);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.CommonService", "公用API", nameof(GetAbsenceFromLastWeek) + "（取得上週缺勤）", "", "", "");
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

        #region 獲取上傳文件（附件）

        /// <summary>
        /// 獲取上傳文件（附件）
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage GetUploadFiles(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.DB;
            try
            {
                do
                {
                    var sParentID = _fetchString(i_crm, "ParentID");

                    var saFiles = db.Queryable<OTB_SYS_Files>()
                        .OrderBy(x => x.OrderByValue)
                        .Where(x => x.ParentID == sParentID && x.OrgID == i_crm.ORIGID)
                        .ToList();
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, saFiles);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.CommonService", "公用API", "GetUploadFiles（獲取上傳文件（附件））", "", "", "");
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

        #endregion 獲取上傳文件（附件）

        #region 編輯文件（附件）

        /// <summary>
        /// 編輯文件（附件）
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage EditFile(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var oEntity = _fetchEntity<OTB_SYS_Files>(i_crm);
                    _setEntityBase(oEntity, i_crm);
                    var iRel = db.Updateable(oEntity)
                        .UpdateColumns(x => new
                        {
                            x.FileName,
                            x.Link,
                            x.Description,
                            x.ModifyDate,
                            x.ModifyUser
                        }).ExecuteCommand();
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, iRel);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.CommonService", "公用API", "DelFile（刪除文件（附件））", "", "", "");
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

        #endregion 編輯文件（附件）

        #region 刪除文件（附件）

        /// <summary>
        /// 刪除文件（附件）
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage DelFile(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var sFileID = _fetchString(i_crm, "FileID");
                    var sIdType = _fetchString(i_crm, "IDType");
                    var bParent = sIdType == "parent";
                    var bDel = false;

                    var saFiles = db.Queryable<OTB_SYS_Files>()
                        .Where(x => x.OrgID == i_crm.ORIGID)
                        .WhereIF(bParent, it => it.ParentID == sFileID)
                        .WhereIF(!bParent, it => it.FileID == sFileID).ToList();

                    if (saFiles.Count > 0)
                    {
                        bDel = db.Deleteable(saFiles).ExecuteCommandHasChange();
                        if (bDel)
                        {
                            var Pdf = new PdfService();
                            foreach (var file in saFiles)
                            {
                                bDel = PdfService.DelFile(file.FilePath);
                            }
                        }
                    }
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, bDel);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.CommonService", "公用API", "DelFile（刪除文件（附件））", "", "", "");
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

        #endregion 刪除文件（附件）

        #region 获得當前公网ip

        /// <summary>
        /// 获得當前公网ip
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage GetPublicIP(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            try
            {
                do
                {
                    var sIp = GetPubIP();
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, sIp);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.CommonService", "公用方法", "GetPublicIP(获得當前公网ip)", "", "", "");
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

        #endregion 获得當前公网ip

        #region 获得公网ip信息

        /// <summary>
        /// 获得公网ip信息
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage GetIPInfo(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            try
            {
                do
                {
                    var sIp = _fetchString(i_crm, "ip");
                    var sValue = String.Empty;
                    var strUrl = "http://ip.taobao.com/service/getIpInfo.php?ip=" + sIp; //获得IP的网址
                    var uri = new Uri(strUrl);
                    var wr = WebRequest.Create(uri);
                    var s = wr.GetResponse().GetResponseStream();
                    using (var sr = new StreamReader(s, Encoding.UTF8))
                    {
                        var sInfo = sr.ReadToEnd(); //读取网站的数据
                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, sInfo);
                    }
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.CommonService", "公用方法", "GetIPInfo(获得公网ip信息)", "", "", "");
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

        #endregion 获得公网ip信息

        public static string GetPubIP()
        {
            string sIp;
            try
            {
                const string strUrl = "http://www.taobao.com/help/getip.php"; //获得IP的网址
                var uri = new Uri(strUrl);
                var wr = WebRequest.Create(uri);
                var s = wr.GetResponse().GetResponseStream();
                using (var sr = new StreamReader(s, Encoding.Default))
                {
                    var all = sr.ReadToEnd(); //读取网站的数据
                    Match match;
                    const string pattern = "(\\d+)\\.(\\d+)\\.(\\d+)\\.(\\d+)";
                    match = Regex.Match(all, pattern, RegexOptions.IgnoreCase);
                    sIp = match.ToString();
                    return sIp;
                }
            }
            catch (Exception ex)
            {
                LogService.mo_Log.Error("CommonService.GetIP Error." + ex.Message, ex);
                return "";
            }
        }
    }
}