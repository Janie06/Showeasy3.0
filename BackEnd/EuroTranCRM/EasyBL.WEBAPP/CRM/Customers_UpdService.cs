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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EasyBL.WEBAPP.CRM
{
    public class Customers_UpdService : ServiceBase
    {
        #region 客戶管理編輯（單筆查詢）

        /// <summary>
        /// 客戶管理編輯（單筆查詢）
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
                    var sId = _fetchString(i_crm, @"guid");

                    var oEntity = db.Queryable<OTB_CRM_Customers, OTB_SYS_Members, OTB_SYS_Members>
                        ((t1, t2, t3) =>
                        new object[] {
                                JoinType.Left, t1.OrgID == t2.OrgID && t1.CreateUser == t2.MemberID,
                                JoinType.Left, t1.OrgID == t3.OrgID && t1.ModifyUser == t3.MemberID
                              }
                        )
                        .Where((t1, t2, t3) => t1.guid == sId)
                        //.WhereIF((i_crm.ORIGID == "SG" || i_crm.ORIGID == "SE"), (t1, t2, t3) => t1.OrgID == i_crm.ORIGID)
                        .WhereIF((i_crm.ORIGID != "TE" && i_crm.ORIGID != "TG"), (t1, t2, t3) => t1.OrgID == i_crm.ORIGID)
                        .WhereIF((i_crm.ORIGID == "TE" || i_crm.ORIGID == "TG"), (t1, t2, t3) => t1.OrgID == "TE" || t1.OrgID == "TG")
                        .Select((t1, t2, t3) => new OTB_CRM_Customers
                        {
                            guid = SqlFunc.GetSelfAndAutoFill(t1.guid),
                            CreateUserName = t2.MemberName,
                            ModifyUserName = t3.MemberName
                        })
                        .Single();

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, oEntity);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Customers_QryService), "", "QueryOne（客戶管理編輯（單筆查詢））", "", "", "");
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

        #endregion 客戶管理編輯（單筆查詢）
        
        #region 客戶管理編輯（單筆查詢）

        /// <summary>
        /// 客戶管理編輯（單筆查詢）
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage QueryCout(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var sId = _fetchString(i_crm, @"guid");
                    var sCustomerShotCName = _fetchString(i_crm, @"CustomerShotCName");
                    var sUniCode = _fetchString(i_crm, @"UniCode");

                    var iCount = db.Queryable<OTB_CRM_Customers>()
                        //.WhereIF((i_crm.ORIGID == "SG" || i_crm.ORIGID == "SE"), x => x.OrgID == i_crm.ORIGID)
                        .WhereIF((i_crm.ORIGID != "TE" && i_crm.ORIGID != "TG"), x => x.OrgID == i_crm.ORIGID)
                        .WhereIF((i_crm.ORIGID == "TE" || i_crm.ORIGID == "TG"), x => x.OrgID == "TE" || x.OrgID == "TG")
                        .WhereIF(!string.IsNullOrEmpty(sId), x => x.guid != sId)
                        .WhereIF(!string.IsNullOrEmpty(sCustomerShotCName), x => x.CustomerShotCName == sCustomerShotCName)
                        .WhereIF(!string.IsNullOrEmpty(sUniCode), x => x.UniCode == sUniCode)
                        .WhereIF((i_crm.ORIGID == "TE" || i_crm.ORIGID == "TG"), x => x.NotPassReason != "CustomerCombine")
                        .Count();

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, iCount);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Customers_QryService), "", "QueryOne（客戶管理編輯（單筆查詢））", "", "", "");
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

        #endregion 客戶管理編輯（單筆查詢）
        
        /// <summary>
        /// 檢查客戶簡稱(不能重複)與統一編號(不能重複)，組織不同視為不同
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        private Tuple<bool, string> CheckShortNameAndUniCode(RequestMessage i_crm)
        {
            var OrgID = i_crm.ORIGID;
            if (i_crm.TYPE == "CopySync")
                OrgID = _fetchString(i_crm, OTB_CRM_Customers.CN_ORGID);
            var db = SugarBase.GetIntance();
            var iCount = -1;
            try
            {
                var sTransactiontype = _fetchString(i_crm, @"TransactionType");
                var sId = _fetchString(i_crm, @"guid");
                var sCustomerShotCName = _fetchString(i_crm, @"CustomerShotCName");
                var sUniCode = _fetchString(i_crm, @"UniCode");
                if (string.IsNullOrWhiteSpace(sCustomerShotCName))
                    return new Tuple<bool, string>(false, "客戶簡稱不得為空。");
                var sTaxpayerOrgID = _fetchString(i_crm, @"TaxpayerOrgID");

                if (!string.IsNullOrWhiteSpace(sTransactiontype))
                {
                    var TypeAD = sTransactiontype.Any(c => c == 'D' || c == 'A');

                    if (OrgID == "TE" || OrgID == "TG")
                    {
                        var SqlType = @" (CustomerShotCName = @CustomerShotCName {1} )";
                        var UnicodeSql = "";
                        var ConvertedUnicode = sUniCode.Trim() ?? "";
                        var UnicodeLength = ConvertedUnicode.Length;
                        //Type A or D 一定要輸入統編
                        if (TypeAD && UnicodeLength < 8)
                            return new Tuple<bool, string>(false, "交易型態為A或D時，統一編號必填且為8碼。");

                        //有輸入統編的話 一定要8碼
                        if (UnicodeLength > 0 && UnicodeLength < 8)
                            return new Tuple<bool, string>(false, "統一編號需要8碼。");
                        if (UnicodeLength == 8)
                        {
                            UnicodeSql = "OR UniCode = @UniCode";
                        }
                        SqlType = SqlType.Replace("{1}", UnicodeSql);
                        iCount = db.Queryable<OTB_CRM_Customers>()
                            .Where(x => (x.OrgID == "TE" || x.OrgID == "TG") && x.NotPassReason != "CustomerCombine" && x.Effective == "Y")
                            .WhereIF(!string.IsNullOrEmpty(sId), x => x.guid != sId)
                            .Where(SqlType, new { CustomerShotCName = sCustomerShotCName, UniCode = sUniCode })
                            .Count();
                    }
                    else
                    {
                        var SqlType = @" (CustomerShotCName = @CustomerShotCName  {1} )";
                        var TaxpayerOrgIDSql = "";
                        var ConvertedTaxpayerOrgID = sTaxpayerOrgID.Trim() ?? "";
                        var TaxpayerOrgIDLength = ConvertedTaxpayerOrgID.Length;
                        //Type A or D 一定要輸入納稅人組織號
                        if (TypeAD && TaxpayerOrgIDLength > 18)
                        {
                            return new Tuple<bool, string>(false, "交易型態為A或D時，納稅人組織號必填且為限制18碼內。");
                        }
                        //有輸入統編的話 一定要8碼
                        if (TaxpayerOrgIDLength > 0 && TaxpayerOrgIDLength < 18)
                            return new Tuple<bool, string>(false, "納稅人組織號需要18碼。");

                        if (TaxpayerOrgIDLength == 18)
                        {
                            TaxpayerOrgIDSql = "OR TaxpayerOrgID = @TaxpayerOrgID";
                        }
                        SqlType = SqlType.Replace("{1}", TaxpayerOrgIDSql);

                        iCount = db.Queryable<OTB_CRM_Customers>()
                            .Where(x => x.OrgID == i_crm.ORIGID && x.Effective == "Y")
                            .WhereIF(!string.IsNullOrEmpty(sId), x => x.guid != sId)
                            .Where(SqlType, new { CustomerShotCName = sCustomerShotCName, TaxpayerOrgID = ConvertedTaxpayerOrgID })
                            .Count();
                    }
                }

            }
            catch (Exception ex)
            {
                var sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Customers_QryService), "", "CheckShortNameAndUniCode（客戶管理編輯（檢查客戶簡稱與統一編號））", "", "", "");

            }
            var TaxName = "統一編號";
            if (OrgID == "SG")
                TaxName = "納稅人組織號";
            switch (iCount)
            {
                case 0:
                    return new Tuple<bool, string>(true, "沒有找到重複" + TaxName + "或簡稱。");
                case -1:
                    return new Tuple<bool, string>(false, "尋找過程發生錯誤，請稍後嘗試。");
                default:
                    return new Tuple<bool, string>(false, TaxName + "或客戶簡稱重複。請重新檢查資料。");
            }
        }
        
        #region 客戶管理編輯（新增）

        /// <summary>
        /// 客戶管理編輯（新增）
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on UpdImportCustomers</param>
        /// <returns></returns>
        public ResponseMessage Insert(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            ////檢查統一編號或客戶簡稱要為唯一
            var CheckResult = CheckShortNameAndUniCode(i_crm);
            if (!CheckResult.Item1)
                return new ErrorResponseMessage(CheckResult.Item2, i_crm);
            try
            {
                rm = SugarBase.ExecTran(db =>
                {
                    do
                    {
                        string sCustomerGUID = Guid.NewGuid().ToString();

                        //客戶資料表身
                        var oEntity = _fetchEntity<OTB_CRM_Customers>(i_crm);
                        _setEntityBase(oEntity, i_crm);
                        oEntity.guid = sCustomerGUID;
                        oEntity.IsAudit = "N";
                        oEntity.Industry = oEntity.Industry.TrimEnd(',');

                        //if (i_crm.ORIGID == "SE" || i_crm.ORIGID == "SG")
                        if (i_crm.ORIGID != "TE" && i_crm.ORIGID != "TG")
                        {
                            oEntity.CustomerNO = SerialNumber.GetMaxNumberByType(i_crm.ORIGID, oEntity.CustomerNO, MaxNumberType.Empty, i_crm.USERID, 3);
                        } else
                        {
                            oEntity.CustomerNO = SerialNumber.GetMaxNumberByType("", oEntity.CustomerNO, MaxNumberType.Empty, i_crm.USERID, 5);
                        }

                        //客戶資料表頭
                        var oMstEntity = _fetchEntity<OTB_CRM_CustomersMST>(i_crm);
                        _setEntityBase(oMstEntity, i_crm);
                        oMstEntity.guid = Guid.NewGuid().ToString();
                        oMstEntity.CustomerNO = oEntity.CustomerNO;
                        oMstEntity.customer_guid = sCustomerGUID;
                        oMstEntity.Effective = "Y";

                        var iRel = db.Insertable(oEntity).ExecuteReturnEntity();
                        var iRelMst = db.Insertable(oMstEntity).ExecuteCommand();
                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, iRel);
                    } while (false);

                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Customers_UpdService), @"客戶管理編輯", @"Add（客戶管理編輯（新增））", @"", @"", @"");
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

        #endregion 客戶管理編輯（新增）

        #region 客戶管理編輯（修改）

        /// <summary>
        /// 客戶管理編輯（修改）
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on UpdImportCustomers</param>
        /// <returns></returns>
        public ResponseMessage Update(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;

            var CheckResult = CheckShortNameAndUniCode(i_crm);
            if (!CheckResult.Item1)
                return new ErrorResponseMessage(CheckResult.Item2, i_crm);
            try
            {
                rm = SugarBase.ExecTran(db =>
                {
                    do
                    {
                        var sId = _fetchString(i_crm, @"guid");

                        var oNewEntity = _fetchEntity<OTB_CRM_Customers>(i_crm);
                        _setEntityBase(oNewEntity, i_crm);
                        oNewEntity.IsAudit = "N";

                        oNewEntity.Industry = oNewEntity.Industry.TrimEnd(',');

                        string sOrgId = oNewEntity.OrgID;

                        string[] arrOrgId = { "TE", "TG", "SG", "SE", "HY" };

                        //if ((oNewEntity.CustomerNO.Length == 4 && (oNewEntity.CustomerNO.Substring(0,2) == "TE" || oNewEntity.CustomerNO.Substring(0, 2) == "TG" || oNewEntity.CustomerNO.Substring(0, 2) == "SG" || oNewEntity.CustomerNO.Substring(0, 2) == "SE")) ||
                        if ((oNewEntity.CustomerNO.Length == 4 && arrOrgId.Contains(oNewEntity.CustomerNO.Substring(0, 2))) ||
                            (oNewEntity.CustomerNO.Length == 2))
                        {
                            //if (i_crm.ORIGID != "SG" && i_crm.ORIGID != "SE" && oNewEntity.CustomerNO.Length == 2)
                            if ((i_crm.ORIGID == "TE" || i_crm.ORIGID == "TG") && oNewEntity.CustomerNO.Length == 2)
                            {
                                oNewEntity.CustomerNO = SerialNumber.GetMaxNumberByType("", oNewEntity.CustomerNO, MaxNumberType.Empty, i_crm.USERID, 5);
                            }
                            else
                            {
                                oNewEntity.CustomerNO = SerialNumber.GetMaxNumberByType(i_crm.ORIGID, oNewEntity.CustomerNO, MaxNumberType.Empty, i_crm.USERID, 3);
                            }

                            //查詢出該

                            //更新現有其他對應表頭資料為N
                            var oUpdMstEntity = _fetchEntity<OTB_CRM_CustomersMST>(i_crm);
                            db.Updateable(oUpdMstEntity).UpdateColumns(p => p.Effective == "N").Where(p => p.customer_guid == sId).ExecuteCommand();


                            //若有變更，新增一筆到表頭
                            var oInsertMstEntity = _fetchEntity<OTB_CRM_CustomersMST>(i_crm);
                            _setEntityBase(oInsertMstEntity, i_crm);
                            oInsertMstEntity.guid = Guid.NewGuid().ToString();
                            oInsertMstEntity.CustomerNO = oNewEntity.CustomerNO;
                            oInsertMstEntity.customer_guid = sId;
                            oInsertMstEntity.Effective = "Y";

                            db.Insertable(oInsertMstEntity).ExecuteCommand();
                        }

                        var iRel = db.Updateable(oNewEntity)
                                     .IgnoreColumns(x => new
                                     {
                                         x.IsApply,
                                         x.IsAudit,
                                         x.ToAuditer,
                                         x.NotPassReason,
                                         x.CreateUser,
                                         x.CreateDate,
                                         x.IndustryStatistics,
                                         x.OrgID
                                     }).ExecuteCommand();

                        var NewResult = db.Queryable<OTB_CRM_Customers>()
                                .Where(p => p.guid == oNewEntity.guid)
                                .Single();

                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, NewResult);
                    } while (false);

                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Customers_UpdService), @"客戶管理編輯", @"Update（客戶管理編輯（修改））", @"", @"", @"");
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

        #endregion 客戶管理編輯（修改）

        #region 客戶管理編輯（刪除）

        /// <summary>
        /// 客戶管理編輯（刪除）
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on UpdImportCustomers</param>
        /// <returns></returns>
        public ResponseMessage Delete(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            try
            {
                rm = SugarBase.ExecTran(db =>
                {
                    do
                    {
                        var sId = _fetchString(i_crm, @"guid");

                        var iRel = db.Deleteable<OTB_CRM_Customers>().Where(x => x.guid == sId).ExecuteCommand();
                        var iMstRel = db.Deleteable<OTB_CRM_CustomersMST>().Where(x => x.customer_guid == sId).ExecuteCommand();

                        //刪除聯絡人
                        iRel = db.Deleteable<OTB_CRM_Contactors>().Where(x => x.CustomerId == sId).ExecuteCommand();

                        //刪除展覽關聯
                        iRel = db.Deleteable<OTB_OPM_ExhibitionCustomers>().Where(x => x.CustomerId == sId).ExecuteCommand();

                        //刪除展覽聯絡人關聯
                        iRel = db.Deleteable<OTB_OPM_ExhibitionContactors>().Where(x => x.CustomerId == sId).ExecuteCommand();

                        //刪除Callout紀錄
                        iRel = db.Deleteable<OTB_CRM_Callout>().Where(x => x.CustomerId == sId).ExecuteCommand();

                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, iRel);
                    } while (false);

                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Customers_UpdService), @"客戶管理編輯", @"Delete（客戶管理編輯（刪除））", @"", @"", @"");
            }
            finally
            {
                if (null != sMsg)
                {
                    rm = new ErrorResponseMessage(sMsg, i_crm);
                }

                Logger.Debug(@"Customers_UpdService.Delete Debug（Param：" + JsonToString(i_crm) + @"；Response：" + JsonToString(rm) + @"）------------------");
            }
            return rm;
        }

        #endregion 客戶管理編輯（刪除）

        #region 客戶管理編輯（提交審核）

        /// <summary>
        /// 客戶管理編輯（提交審核）
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on ToAudit</param>
        /// <returns></returns>
        public ResponseMessage ToAudit(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var CheckResult = CheckShortNameAndUniCode(i_crm);
            if (!CheckResult.Item1)
                return new ErrorResponseMessage(CheckResult.Item2, i_crm);
            try
            {
                rm = SugarBase.ExecTran(db =>
                {
                    do
                    {
                        var sId = _fetchString(i_crm, EasyNetGlobalConstWord.GUID);
                        var sIsAudit = _fetchString(i_crm, @"IsAudit");
                        var sdb = new SimpleClient<OTB_CRM_Customers>(db);
                        var customer = sdb.GetById(sId);

                        if (customer == null)
                        {
                            sMsg = @"系統找不到對應的客戶資料，請核查！";
                            break;
                        }

                        var sTitle = @"客戶資料「" + (string.IsNullOrWhiteSpace(customer.CustomerCName) ? customer.CustomerEName : customer.CustomerCName) + @"」申請審核";
                        if (i_crm.LANG == @"zh")
                        {
                            sTitle = ChineseStringUtility.ToSimplified(sTitle);
                        }
                        //更新客戶資料
                        var oCustomersUpd = new OTB_CRM_Customers
                        {
                            IsAudit = sIsAudit,
                            ToAuditer = i_crm.USERID,
                            ModifyUser = i_crm.USERID,
                            ModifyDate = DateTime.Now
                        };
                        db.Updateable(oCustomersUpd)
                                .UpdateColumns(it => new { it.IsAudit, it.ToAuditer, it.ModifyUser, it.ModifyDate })
                                .Where(it => it.guid == sId).ExecuteCommand();
                        //更新代辦
                        SYS.Task_QryService.TaskStatusUpd(db, i_crm.ORIGID, sId);

                        var listTips = new List<OTB_SYS_Tips>();
                        var listTask = new List<OTB_SYS_Task>();
                        var lstCustomersAuditUsers = new List<string>();
                        var sCustomersAuditUsers = Common.GetSystemSetting(db, i_crm.ORIGID, @"CustomersAuditUsers");
                        if (sCustomersAuditUsers != @"")
                        {
                            var saCustomersAuditUsers = sCustomersAuditUsers.Split(new string[] { @";", @",", @"，", @"|" }, StringSplitOptions.RemoveEmptyEntries);
                            lstCustomersAuditUsers = saCustomersAuditUsers.Distinct<string>().ToList();
                            foreach (string user in lstCustomersAuditUsers)
                            {
                                //添加代辦
                                var oTaskAdd = SYS.Task_QryService.TaskAdd(i_crm, customer.guid, user, sTitle, i_crm.CUSTOMDATA[@"program_id"], @"?Action=Upd&guid=" + customer.guid);
                                listTask.Add(oTaskAdd);

                                //添加提醒消息
                                var oTipsAdd = SYS.Task_QryService.TipsAdd(i_crm, sTitle, user, i_crm.CUSTOMDATA[@"program_id"] + @"|?Action=Upd&guid=" + customer.guid, WebAppGlobalConstWord.BELL);
                                listTips.Add(oTipsAdd);
                            }
                        }
                        if (listTips.Count > 0)
                        {
                            db.Insertable(listTips).ExecuteCommand();
                        }
                        if (listTask.Count > 0)
                        {
                            db.Insertable(listTask).ExecuteCommand();
                        }

                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, lstCustomersAuditUsers);
                    } while (false);
                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Customers_UpdService), @"客戶管理編輯", @"ToAudit（客戶管理編輯（提交審核））", @"", @"", @"");
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

        #endregion 客戶管理編輯（提交審核）

        #region 客戶管理編輯（主管會計審核）

        /// <summary>
        /// 客戶管理編輯（主管會計審核）
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on Audit</param>
        /// <returns></returns>
        public ResponseMessage Audit(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            try
            {
                rm = SugarBase.ExecTran(db =>
                {
                    do
                    {
                        var sId = _fetchString(i_crm, EasyNetGlobalConstWord.GUID);
                        var sIsAudit = _fetchString(i_crm, @"IsAudit");
                        var sNotPassReason = _fetchString(i_crm, @"NotPassReason");

                        var sdb = new SimpleClient<OTB_CRM_Customers>(db);
                        var customer = sdb.GetById(sId);

                        if (customer == null)
                        {
                            sMsg = @"系統找不到對應的客戶資料，請核查！";
                            break;
                        }

                        //更新客戶資料
                        var oCustomersUpd = new OTB_CRM_Customers
                        {
                            IsAudit = sIsAudit,
                            NotPassReason = sNotPassReason,
                            ModifyUser = i_crm.USERID,
                            ModifyDate = DateTime.Now
                        };
                        db.Updateable(oCustomersUpd)
                            .UpdateColumns(it => new { it.IsAudit, it.NotPassReason, it.ModifyUser, it.ModifyDate })
                            .Where(it => it.guid == sId).ExecuteCommand();
                        //更新代辦
                        SYS.Task_QryService.TaskStatusUpd(db, i_crm.ORIGID, sId);

                        var oUserInfo = db.Queryable<OTB_SYS_Members>().Single(it => it.OrgID == i_crm.ORIGID && it.MemberID == i_crm.USERID);
                        var sTitle = (oUserInfo.MemberName ?? i_crm.USERID) + @"審核了您創建的客戶資料「" + (string.IsNullOrWhiteSpace(customer.CustomerCName) ? customer.CustomerEName : customer.CustomerCName) + @"」，審核結果：";
                        sTitle += sIsAudit == @"Y" ? @"通過" : @"不通過";
                        if (i_crm.LANG == @"zh")
                        {
                            sTitle = ChineseStringUtility.ToSimplified(sTitle);
                        }

                        //成功與否
                        var TipsType = sIsAudit == @"Y" ? WebAppGlobalConstWord.CHECK : WebAppGlobalConstWord.FAIL;

                        //添加提醒消息
                        var oTipsAdd = SYS.Task_QryService.TipsAdd(i_crm, sTitle, customer.ToAuditer, i_crm.CUSTOMDATA[@"program_id"] + @"|?Action=Upd&guid=" + customer.guid, TipsType);
                        db.Insertable(oTipsAdd).ExecuteCommand();
                        if (sIsAudit == @"Q")
                        {
                            //添加代辦
                            var oTaskAdd = SYS.Task_QryService.TaskAdd(i_crm, customer.guid, customer.ToAuditer, sTitle, i_crm.CUSTOMDATA[@"program_id"], @"?Action=Upd&guid=" + customer.guid);
                            db.Insertable(oTaskAdd).ExecuteCommand();
                        }
                        else
                        {
                            var oCustomersTransferAdd = new OTB_CRM_CustomersTransfer();
                            var sEventID = Guid.NewGuid().ToString();
                            oCustomersTransferAdd.OrgID = i_crm.ORIGID;
                            oCustomersTransferAdd.Feild01 = customer.CustomerNO;
                            oCustomersTransferAdd.Feild02 = @"0";
                            oCustomersTransferAdd.Feild03 = Common.CutByteString(customer.CustomerShotCName, 12);
                            oCustomersTransferAdd.Feild04 = Common.CutByteString(customer.CustomerCName == @"" ? customer.CustomerEName : customer.CustomerCName, 60);
                            oCustomersTransferAdd.Feild05 = @"";
                            oCustomersTransferAdd.Feild06 = @"";
                            oCustomersTransferAdd.Feild07 = customer.UniCode;
                            oCustomersTransferAdd.Feild08 = @"";
                            oCustomersTransferAdd.Feild09 = @"";
                            oCustomersTransferAdd.Feild10 = Common.CutByteString(customer.InvoiceAddress, 60);
                            oCustomersTransferAdd.Feild11 = Common.CutByteString(customer.Address, 60);
                            oCustomersTransferAdd.Feild12 = @"";
                            oCustomersTransferAdd.Feild13 = @"";
                            oCustomersTransferAdd.Feild14 = Common.CutByteString(customer.Telephone, 20);
                            oCustomersTransferAdd.Feild15 = @"";
                            oCustomersTransferAdd.Feild16 = Common.CutByteString(customer.FAX, 20);
                            oCustomersTransferAdd.Feild17 = @"";
                            oCustomersTransferAdd.Feild18 = @"";
                            oCustomersTransferAdd.Feild19 = @"";
                            oCustomersTransferAdd.Feild20 = @"";
                            oCustomersTransferAdd.Feild21 = @"";
                            oCustomersTransferAdd.Feild22 = @"";
                            oCustomersTransferAdd.Feild23 = Common.CutByteString(customer.Memo, 30);
                            oCustomersTransferAdd.Feild24 = @"100";
                            oCustomersTransferAdd.Feild25 = @"";
                            oCustomersTransferAdd.Feild26 = @"";
                            oCustomersTransferAdd.Feild27 = @"100";
                            oCustomersTransferAdd.Feild28 = @"";
                            oCustomersTransferAdd.Feild29 = Common.CutByteString(customer.CreateUser.Split('.')[0], 11);
                            oCustomersTransferAdd.Feild30 = @"";
                            oCustomersTransferAdd.Feild31 = @"";
                            oCustomersTransferAdd.Feild32 = @"";
                            oCustomersTransferAdd.Feild33 = @"";
                            oCustomersTransferAdd.Feild34 = @"";
                            oCustomersTransferAdd.Feild35 = @"";
                            oCustomersTransferAdd.Feild36 = @"";
                            oCustomersTransferAdd.Feild37 = @"B,C".IndexOf(customer.TransactionType) > -1 ? @"6" : @"5";
                            oCustomersTransferAdd.Feild38 = @"2";
                            oCustomersTransferAdd.Feild39 = @"";
                            oCustomersTransferAdd.Feild40 = @"";
                            oCustomersTransferAdd.Feild41 = @"1";
                            oCustomersTransferAdd.Feild42 = @"";
                            oCustomersTransferAdd.Feild43 = @"";
                            oCustomersTransferAdd.Feild44 = @"";
                            oCustomersTransferAdd.Feild45 = @"";
                            oCustomersTransferAdd.Feild46 = @"";
                            oCustomersTransferAdd.Feild47 = @"";
                            oCustomersTransferAdd.Feild48 = @"";
                            oCustomersTransferAdd.Feild49 = @"";
                            oCustomersTransferAdd.Feild50 = @"";
                            oCustomersTransferAdd.Feild51 = @"";
                            oCustomersTransferAdd.Feild52 = @"";
                            oCustomersTransferAdd.Feild53 = @"";
                            oCustomersTransferAdd.Feild54 = @"";
                            oCustomersTransferAdd.Feild55 = @"";
                            oCustomersTransferAdd.Feild56 = @"";
                            oCustomersTransferAdd.Feild57 = @"";
                            oCustomersTransferAdd.Feild58 = customer.CustomerNO;
                            oCustomersTransferAdd.Feild59 = @"";
                            oCustomersTransferAdd.Feild60 = @"";
                            oCustomersTransferAdd.Feild61 = @"";
                            oCustomersTransferAdd.Feild62 = @"";
                            oCustomersTransferAdd.Feild63 = @"";
                            oCustomersTransferAdd.Feild64 = @"";
                            oCustomersTransferAdd.Feild65 = @"";
                            oCustomersTransferAdd.Feild66 = @"";
                            oCustomersTransferAdd.Feild67 = @"";
                            oCustomersTransferAdd.Feild68 = @"";
                            oCustomersTransferAdd.Feild69 = @"";
                            oCustomersTransferAdd.Feild70 = @"";
                            oCustomersTransferAdd.Feild71 = @"";
                            oCustomersTransferAdd.Feild72 = @"";
                            oCustomersTransferAdd.Feild73 = @"";
                            oCustomersTransferAdd.Feild74 = @"";
                            oCustomersTransferAdd.Feild75 = @"";
                            oCustomersTransferAdd.Feild76 = @"";
                            oCustomersTransferAdd.Feild77 = @"";
                            oCustomersTransferAdd.Feild78 = @"";
                            oCustomersTransferAdd.Feild79 = @"";
                            oCustomersTransferAdd.Feild80 = @"";
                            oCustomersTransferAdd.Feild81 = @"";
                            oCustomersTransferAdd.Feild82 = Common.CutByteString(customer.CustomerEName, 120);
                            var sAddress = customer.Address;
                            var cn = new Regex(@"[一-龥]+");//正则表达式 表示汉字范围
                            if (cn.IsMatch(sAddress))
                            {
                                sAddress = @"";
                            }
                            oCustomersTransferAdd.Feild83 = Common.CutByteString(sAddress, 240);
                            oCustomersTransferAdd.Feild84 = @"";
                            oCustomersTransferAdd.Feild85 = @"";
                            oCustomersTransferAdd.Feild86 = @"";
                            oCustomersTransferAdd.Feild87 = @"";
                            oCustomersTransferAdd.Feild88 = @"";
                            oCustomersTransferAdd.Feild89 = @"";
                            oCustomersTransferAdd.Feild90 = @"";
                            oCustomersTransferAdd.Feild91 = @"0";
                            oCustomersTransferAdd.Feild92 = @"0";
                            oCustomersTransferAdd.Feild93 = @"";
                            oCustomersTransferAdd.Feild94 = @"";
                            oCustomersTransferAdd.Feild95 = @"";
                            oCustomersTransferAdd.Feild96 = @"";
                            oCustomersTransferAdd.Feild97 = @"";
                            oCustomersTransferAdd.Feild98 = @"";
                            oCustomersTransferAdd.Feild99 = @"";
                            db.Insertable(oCustomersTransferAdd).ExecuteCommand();

                            //若有對應的被合併客戶，轉換OrgID與CustomerNO進行拋轉
                            var oMatchCustomer = db.Queryable<OTB_CRM_CustomersMST>().Single(it => it.customer_guid == sId && it.Memo == "CustomerCombine");
                            if (oMatchCustomer != null)
                            {
                                if (i_crm.ORIGID == "TE")
                                {
                                    oCustomersTransferAdd.OrgID = "TG";
                                }
                                else if (i_crm.ORIGID == "TG")
                                {
                                    oCustomersTransferAdd.OrgID = "TE";
                                }
                                customer.CustomerNO = oMatchCustomer.CustomerNO;
                                db.Insertable(oCustomersTransferAdd).ExecuteCommand();
                            }
                            //若無對應的被合併客戶，多拋轉另一個組織別
                            else
                            {
                                if (i_crm.ORIGID == "TE")
                                {
                                    oCustomersTransferAdd.OrgID = "TG";
                                    db.Insertable(oCustomersTransferAdd).ExecuteCommand();
                                } else if (i_crm.ORIGID == "TG")
                                {
                                    oCustomersTransferAdd.OrgID = "TE";
                                    db.Insertable(oCustomersTransferAdd).ExecuteCommand();
                                }
                            }
                        }

                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, customer.ToAuditer);
                    } while (false);
                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Customers_UpdService), @"客戶管理編輯", @"Audit（客戶管理編輯（主管會計審核））", @"", @"", @"");
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

        #endregion 客戶管理編輯（主管會計審核）

        #region 客戶管理編輯（取消審核）

        /// <summary>
        /// 客戶管理編輯（取消審核）
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on CancelAudit</param>
        /// <returns></returns>
        public ResponseMessage CancelAudit(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            try
            {
                rm = SugarBase.ExecTran(db =>
                {
                    do
                    {
                        var sId = _fetchString(i_crm, EasyNetGlobalConstWord.GUID);
                        var sNotPassReason = _fetchString(i_crm, @"NotPassReason");
                        var sUserName = _fetchString(i_crm, @"UserName");
                        var sdb = new SimpleClient<OTB_CRM_Customers>(db);
                        var customer = sdb.GetById(sId);

                        if (customer == null)
                        {
                            sMsg = @"系統找不到對應的客戶資料，請核查！";
                            break;
                        }

                        var sTitle = sUserName + @"客戶管理取消審核了您創建的客戶資料「" + (customer.CustomerCName.Trim() == @"" ? customer.CustomerEName : customer.CustomerCName) + @"」";
                        if (i_crm.LANG == @"zh")
                        {
                            sTitle = ChineseStringUtility.ToSimplified(sTitle);
                        }
                        //更新客戶資料
                        var oCustomersUpd = new OTB_CRM_Customers
                        {
                            IsAudit = @"N",
                            NotPassReason = sNotPassReason,
                            ModifyUser = i_crm.USERID,
                            ModifyDate = DateTime.Now
                        };
                        db.Updateable(oCustomersUpd)
                            .UpdateColumns(it => new { it.IsAudit, it.NotPassReason, it.ModifyUser, it.ModifyDate })
                            .Where(it => it.guid == sId).ExecuteCommand();
                        //更新代辦
                        SYS.Task_QryService.TaskStatusUpd(db, i_crm.ORIGID, sId);

                        //添加提醒消息
                        var oTipsAdd = SYS.Task_QryService.TipsAdd(i_crm, sTitle, customer.ToAuditer, i_crm.CUSTOMDATA[@"program_id"] + @"|?Action=Upd&guid=" + customer.guid, WebAppGlobalConstWord.BELL);
                        db.Insertable(oTipsAdd).ExecuteCommand();
                        //添加代辦
                        var oTaskAdd = SYS.Task_QryService.TaskAdd(i_crm, customer.guid, customer.ToAuditer, sTitle, i_crm.CUSTOMDATA[@"program_id"], @"?Action=Upd&guid=" + customer.guid);
                        db.Insertable(oTaskAdd).ExecuteCommand();

                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, customer.ToAuditer);
                    } while (false);
                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Customers_UpdService), @"客戶管理編輯", @"CancelAudit（客戶管理編輯（取消審核））", @"", @"", @"");
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

        #endregion 客戶管理編輯（取消審核）

        #region 客戶管理編輯（提交申請修改）

        /// <summary>
        /// 客戶管理編輯（提交申請修改）
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on ApplyforUpdate</param>
        /// <returns></returns>
        public ResponseMessage ApplyforUpdate(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            try
            {
                rm = SugarBase.ExecTran(db =>
                {
                    do
                    {
                        var sId = _fetchString(i_crm, @"Guid");
                        var sNotPassReason = _fetchString(i_crm, @"NotPassReason");

                        var sdb = new SimpleClient<OTB_CRM_Customers>(db);
                        var oCustomers = sdb.GetById(sId);

                        if (oCustomers == null)
                        {
                            sMsg = @"系統找不到對應的請假資料，請核查！";
                            break;
                        }

                        //更新客戶資料
                        var oCustomersUpd = new OTB_CRM_Customers
                        {
                            IsAudit = @"Z",
                            NotPassReason = sNotPassReason,
                            ModifyUser = i_crm.USERID,
                            ModifyDate = DateTime.Now
                        };
                        db.Updateable(oCustomersUpd)
                            .UpdateColumns(it => new { it.IsAudit, it.NotPassReason, it.ModifyUser, it.ModifyDate }).Where(it => it.guid == sId).ExecuteCommand();
                        var oUserInfo = db.Queryable<OTB_SYS_Members>().Single(it => it.OrgID == i_crm.ORIGID && it.MemberID == i_crm.USERID);
                        var sTitle = @"客戶資料「" + (oCustomers.CustomerCName.Trim() == @"" ? oCustomers.CustomerEName : oCustomers.CustomerCName) + @"」已被" + (oUserInfo.MemberName ?? @"") + @"申請修改";

                        var listTips = new List<OTB_SYS_Tips>();
                        //List<OTB_SYS_Task> listTask = new List<OTB_SYS_Task>();
                        var lstCustomersAuditUsers = new List<string>();
                        var sCustomersAuditUsers = Common.GetSystemSetting(db, i_crm.ORIGID, @"CustomersAuditUsers");
                        if (sCustomersAuditUsers != @"")
                        {
                            var saCustomersAuditUsers = sCustomersAuditUsers.Split(new string[] { @";", @",", @"，", @"|" }, StringSplitOptions.RemoveEmptyEntries);
                            lstCustomersAuditUsers = saCustomersAuditUsers.Distinct<string>().ToList();
                            foreach (string user in lstCustomersAuditUsers)
                            {
                                //添加代辦
                                //OTB_SYS_Task oTaskAdd = SYS.TaskService.TaskAdd(i_crm, oCustomers.guid, user, sTitle, i_crm.CUSTOMDATA["program_id"], "?Action=Upd&guid=" + oCustomers.guid);
                                //listTask.Add(oTaskAdd);

                                //添加提醒消息
                                var oTipsAdd = SYS.Task_QryService.TipsAdd(i_crm, sTitle, user, i_crm.CUSTOMDATA[@"program_id"] + @"|?Action=Upd&guid=" + oCustomers.guid, WebAppGlobalConstWord.BELL);
                                listTips.Add(oTipsAdd);
                            }
                        }
                        if (listTips.Count > 0)
                        {
                            db.Insertable(listTips).ExecuteCommand();
                        }
                        //if (listTask.Count > 0)
                        //{
                        //    db.Insertable(listTask).ExecuteCommand();
                        //}

                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, lstCustomersAuditUsers);
                    } while (false);
                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Customers_UpdService), @"客戶管理編輯", @"ApplyforUpdate（客戶管理編輯（提交申請修改））", @"", @"", @"");
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

        #endregion 客戶管理編輯（提交申請修改）

        #region 客戶管理編輯（抓取參加展覽列表資料）

        /// <summary>
        /// 客戶管理編輯（抓取參加展覽列表資料）
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on GetExhibitionlist</param>
        /// <returns></returns>
        public ResponseMessage GetExhibitionlist(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var sId = _fetchString(i_crm, @"Guid");
                    var saExhibitions = new List<Map>();
                    //出口
                    var saExport = db.Queryable<OTB_OPM_Exhibition, OTB_OPM_ExportExhibition>((t1, t2) => t1.SN.ToString() == t2.ExhibitionNO && t1.OrgID == t2.OrgID && t1.Effective == "Y" && t2.IsVoid == "N")
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
                                    { @"Exhibitioname_CN", opm.Exhibitioname_CN }
                                };
                                saExhibitions.Add(m);
                            }
                        }
                    }
                    //進口
                    var saImport = db.Queryable<OTB_OPM_Exhibition, OTB_OPM_ImportExhibition>((t1, t2) => t1.SN.ToString() == t2.ExhibitionNO && t1.OrgID == t2.OrgID && t1.Effective == "Y" && t2.IsVoid == "N")
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
                                    { @"Exhibitioname_CN", opm.Exhibitioname_CN }
                                };
                                saExhibitions.Add(m);
                            }
                        }
                    }
                    //其他
                    var saOther = db.Queryable<OTB_OPM_Exhibition, OTB_OPM_OtherExhibition>((t1, t2) => t1.SN.ToString() == t2.ExhibitionNO && t1.OrgID == t2.OrgID && t1.Effective == "Y" && t2.IsVoid == "N")
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
                                    { @"Exhibitioname_CN", opm.Exhibitioname_CN }
                                };
                                saExhibitions.Add(m);
                            }
                        }
                    }
                    //其他
                    var saOtherTG = db.Queryable<OTB_OPM_Exhibition, OTB_OPM_OtherExhibitionTG>((t1, t2) => t1.SN.ToString() == t2.ExhibitionNO && t1.OrgID == t2.OrgID && t1.Effective == "Y" && t2.IsVoid == "N")
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
                                    { @"Exhibitioname_CN", opm.Exhibitioname_CN }
                                };
                                saExhibitions.Add(m);
                            }
                        }
                    }

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, saExhibitions);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Customers_UpdService), @"客戶管理編輯", @"GetCustomers（客戶管理編輯（抓取參加展覽列表資料））", @"", @"", @"");
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

        #endregion 客戶管理編輯（抓取參加展覽列表資料）

        #region 客戶管理編輯（（通過政府API）依據客戶編碼抓取基本資料）

        /// <summary>
        /// 客戶管理編輯（（通過政府API）依據客戶編碼抓取基本資料）
        /// </summary>
        /// <param name="i_crm">
        /// <returns></returns>
        public ResponseMessage GetCrmBaseDataByUniCode(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var sUniCode = _fetchString(i_crm, @"UniCode");
                    var sKeyWords = _fetchString(i_crm, @"KeyWords");
                    var sAPIUrl = @"https://data.gcis.nat.gov.tw/od/data/api/5F64D864-61CB-4D0D-8AD9-492047CC1EA6?$format=json&$filter=Business_Accounting_NO eq ";
                    if (sUniCode != "")
                    {
                        sAPIUrl += sUniCode;
                    }
                    else
                    {
                        sAPIUrl = @"https://data.gcis.nat.gov.tw/od/data/api/6BBA2268-1367-4B42-9CCA-BC17499EBE8C?$format=json&$filter=Company_Name like " + sKeyWords + " and Company_Status eq 01";
                    }
                    var client = new HttpWebClient(sAPIUrl);

                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    var sHtml = client.GetString();
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, sHtml);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Customers_UpdService), @"客戶管理編輯", @"GetCrmBaseDataByUniCode（客戶管理編輯（（通過政府API）依據客戶編碼抓取基本資料））", @"", @"", @"");
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

        #endregion 客戶管理編輯（（通過政府API）依據客戶編碼抓取基本資料）

        #region 客戶管理編輯（依據預約單號查詢匯入廠商）

        /// <summary>
        /// 客戶管理編輯（依據預約單號查詢匯入廠商）
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on GetCrmBaseDataByUniCode</param>
        /// <returns></returns>
        public ResponseMessage GetImportCustomersByAppointNO(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var sAppointNO = _fetchString(i_crm, @"AppointNO");
                    var oImportCustomers = db.Queryable<OTB_WSM_PackingOrder, OTB_CRM_ImportCustomers>(
                        (t1, t2) => t1.OrgID == t2.OrgID && t1.CustomerId == t2.guid)
                        .Where((t1, t2) => t1.AppointNO == sAppointNO)
                        //.WhereIF((i_crm.ORIGID == "SG" || i_crm.ORIGID == "SE"), x => x.OrgID == i_crm.ORIGID)
                        .WhereIF((i_crm.ORIGID != "TE" && i_crm.ORIGID != "TG"), x => x.OrgID == i_crm.ORIGID)
                        .WhereIF((i_crm.ORIGID == "TE" || i_crm.ORIGID == "TG"), x => x.OrgID == "TE" || x.OrgID == "TG")
                        .Select((t1, t2) => t2)
                        .Single();
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, oImportCustomers);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Customers_UpdService), @"客戶管理編輯", @"GetImportCustomersByAppointNO（客戶管理編輯（依據預約單號查詢匯入廠商））", @"", @"", @"");
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

        #endregion 客戶管理編輯（依據預約單號查詢匯入廠商）

        #region 客戶管理編輯（複製同步）

        /// <summary>
        /// 客戶管理編輯（複製同步）
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on UpdImportCustomers</param>
        /// <returns></returns>
        public ResponseMessage CopySync(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            ////檢查統一編號或客戶簡稱要為唯一
            var CheckResult = CheckShortNameAndUniCode(i_crm);
            if (!CheckResult.Item1)
                return new ErrorResponseMessage(CheckResult.Item2, i_crm);

            try
            {
                rm = SugarBase.ExecTran(db =>
                {
                    do
                    {
                        string sCustomerGUID = Guid.NewGuid().ToString();
                        string sOldCustomerGUID = string.Empty;
                        string sCurrentOrgID = _fetchString(i_crm, "currOrgID");
                        //客戶資料表身
                        string sOrgID = _fetchString(i_crm, OTB_CRM_Customers.CN_ORGID);

                        if (string.IsNullOrEmpty(sOrgID))
                        {
                            sMsg = "OrgID Error";
                            break;
                        }

                        var oEntity = _fetchEntity<OTB_CRM_Customers>(i_crm);
                        _setEntityBase(oEntity, i_crm);
                        sOldCustomerGUID = oEntity.guid;
                        oEntity.guid = sCustomerGUID;
                        oEntity.OrgID = sOrgID;
                        oEntity.CustomerNO = SerialNumber.GetMaxNumberByType(oEntity.OrgID, oEntity.CustomerNO, MaxNumberType.Empty, i_crm.USERID, 3);

                        //客戶資料表頭
                        var oMstEntity = _fetchEntity<OTB_CRM_CustomersMST>(i_crm);
                        _setEntityBase(oMstEntity, i_crm);
                        oMstEntity.guid = Guid.NewGuid().ToString();
                        oMstEntity.CustomerNO = oEntity.CustomerNO;
                        oMstEntity.customer_guid = sCustomerGUID;
                        oMstEntity.Effective = "Y";

                        //找出參展資料並複製到不同公司別
                        #region 出口

                        //var saExport = db.Queryable<OTB_OPM_Exhibition, OTB_OPM_ExportExhibition>((t1, t2) => t1.SN.ToString() == t2.ExhibitionNO && t1.OrgID == t2.OrgID)
                        //                 .Select((t1, t2) => new { t1.OrgID, t1.SN, t1.ExhibitionCode, t1.Exhibitioname_TW, t1.Exhibitioname_CN, t2.ExhibitionNO, t2.Exhibitors, t2.Agent }).MergeTable()
                        //                 .Where(it => it.ExhibitionNO != @"" && it.OrgID == sCurrentOrgID && (it.Exhibitors.Contains(sOldCustomerGUID) || it.Agent == sOldCustomerGUID))
                        //                 .ToList();

                        //if (saExport.Count > 0)
                        //{
                        //    List<OTB_OPM_Exhibition> lExh = db.Queryable<OTB_OPM_Exhibition>()
                        //                                    .Where(p => p.SN == saExport.Select(x => x.SN).First())
                        //                                    .ToList();

                        //    foreach (OTB_OPM_Exhibition data in lExh)
                        //    {
                        //        data.OrgID = sOrgID;
                        //        data.OrgID = sOrgID;
                        //        data.CreateUser = i_crm.USERID;
                        //        data.CreateDate = DateTime.Now;
                        //        data.ModifyUser = i_crm.USERID;
                        //        data.ModifyDate = DateTime.Now;
                        //    }

                        //    List<OTB_OPM_ExportExhibition> lExportExh = db.Queryable<OTB_OPM_ExportExhibition>()
                        //                                    .Where(p => p.ExhibitionNO == saExport.Select(x => x.SN).First().ToString())
                        //                                    .ToList();

                        //    var exData = db.Insertable<OTB_OPM_Exhibition>(lExh)
                        //                                  .IgnoreColumns(p => p == OTB_OPM_Exhibition.CN_SN)
                        //                                  .ExecuteReturnIdentity();

                        //    foreach (OTB_OPM_ExportExhibition data in lExportExh)
                        //    {
                        //        data.ExportBillNO = Guid.NewGuid().ToString();
                        //        data.ExhibitionNO = exData.ToString();
                        //        data.OrgID = sOrgID;
                        //        data.CreateUser = i_crm.USERID;
                        //        data.CreateDate = DateTime.Now;
                        //        data.ModifyUser = i_crm.USERID;
                        //        data.ModifyDate = DateTime.Now;

                        //        if (data.Exhibitors.IndexOf(sOldCustomerGUID) > -1)
                        //        {
                        //            data.Exhibitors.Replace(sOldCustomerGUID, sCustomerGUID);
                        //        }

                        //        if (data.Agent == sOldCustomerGUID)
                        //        {
                        //            data.Agent = sCustomerGUID;
                        //        }

                        //    }

                        //    db.Insertable<OTB_OPM_ExportExhibition>(lExportExh)
                        //                .ExecuteCommand();
                        //}

                        #endregion

                        #region 進口

                        //var saImport = db.Queryable<OTB_OPM_Exhibition, OTB_OPM_ImportExhibition>((t1, t2) => t1.SN.ToString() == t2.ExhibitionNO && t1.OrgID == t2.OrgID)
                        //                 .Select((t1, t2) => new { t1.OrgID, t1.SN, t1.ExhibitionCode, t1.Exhibitioname_TW, t1.Exhibitioname_CN, t2.ExhibitionNO, t2.Supplier, t2.Agent }).MergeTable()
                        //                 .Where(it => it.ExhibitionNO != @"" && it.OrgID == sCurrentOrgID && (it.Supplier.Contains(sOldCustomerGUID) || it.Agent == sOldCustomerGUID))
                        //                 .ToList();

                        //if (saImport.Count > 0)
                        //{
                        //    List<OTB_OPM_Exhibition> lExh = db.Queryable<OTB_OPM_Exhibition>()
                        //                                    .Where(p => p.SN == saImport.Select(x => x.SN).First())
                        //                                    .ToList();

                        //    foreach (OTB_OPM_Exhibition data in lExh)
                        //    {
                        //        data.OrgID = sOrgID;
                        //        data.OrgID = sOrgID;
                        //        data.CreateUser = i_crm.USERID;
                        //        data.CreateDate = DateTime.Now;
                        //        data.ModifyUser = i_crm.USERID;
                        //        data.ModifyDate = DateTime.Now;
                        //    }

                        //    List<OTB_OPM_ImportExhibition> lImportExh = db.Queryable<OTB_OPM_ImportExhibition>()
                        //                                    .Where(p => p.ExhibitionNO == saImport.Select(x => x.SN).First().ToString())
                        //                                    .ToList();

                        //    var exData = db.Insertable<OTB_OPM_Exhibition>(lExh)
                        //                                  .IgnoreColumns(p => p == OTB_OPM_Exhibition.CN_SN)
                        //                                  .ExecuteReturnIdentity();

                        //    foreach (OTB_OPM_ImportExhibition data in lImportExh)
                        //    {
                        //        data.ExportBillNO = Guid.NewGuid().ToString();
                        //        data.ExhibitionNO = exData.ToString();
                        //        data.OrgID = sOrgID;
                        //        data.CreateUser = i_crm.USERID;
                        //        data.CreateDate = DateTime.Now;
                        //        data.ModifyUser = i_crm.USERID;
                        //        data.ModifyDate = DateTime.Now;

                        //        if (data.Supplier.IndexOf(sOldCustomerGUID) > -1)
                        //        {
                        //            data.Supplier.Replace(sOldCustomerGUID, sCustomerGUID);
                        //        }

                        //        if (data.Agent == sOldCustomerGUID)
                        //        {
                        //            data.Agent = sCustomerGUID;
                        //        }

                        //    }

                        //    db.Insertable<OTB_OPM_ImportExhibition>(lImportExh)
                        //                .ExecuteCommand();
                        //}

                        #endregion

                        #region 其他

                        //var saOther = db.Queryable<OTB_OPM_Exhibition, OTB_OPM_OtherExhibition>((t1, t2) => t1.SN.ToString() == t2.ExhibitionNO && t1.OrgID == t2.OrgID)
                        //                 .Select((t1, t2) => new { t1.OrgID, t1.SN, t1.ExhibitionCode, t1.Exhibitioname_TW, t1.Exhibitioname_CN, t2.ExhibitionNO, t2.Supplier, t2.Agent }).MergeTable()
                        //                 .Where(it => it.ExhibitionNO != @"" && it.OrgID == sCurrentOrgID && (it.Supplier == sOldCustomerGUID || it.Agent == sOldCustomerGUID))
                        //                 .ToList();

                        //if (saOther.Count > 0)
                        //{
                        //    List<OTB_OPM_Exhibition> lExh = db.Queryable<OTB_OPM_Exhibition>()
                        //                                    .Where(p => p.SN == saOther.Select(x => x.SN).First())
                        //                                    .ToList();

                        //    foreach (OTB_OPM_Exhibition data in lExh)
                        //    {
                        //        data.OrgID = sOrgID;
                        //        data.CreateUser = i_crm.USERID;
                        //        data.CreateDate = DateTime.Now;
                        //        data.ModifyUser = i_crm.USERID;
                        //        data.ModifyDate = DateTime.Now;
                        //    }

                        //    List<OTB_OPM_OtherExhibition> lOtherExh = db.Queryable<OTB_OPM_OtherExhibition>()
                        //                                    .Where(p => p.ExhibitionNO == saOther.Select(x => x.SN).First().ToString())
                        //                                    .ToList();

                        //    var exData = db.Insertable<OTB_OPM_Exhibition>(lExh)
                        //                                  .IgnoreColumns(p => p == OTB_OPM_Exhibition.CN_SN)
                        //                                  .ExecuteReturnIdentity();

                        //    foreach (OTB_OPM_OtherExhibition data in lOtherExh)
                        //    {
                        //        data.Guid = Guid.NewGuid().ToString();
                        //        data.ExhibitionNO = exData.ToString();
                        //        data.OrgID = sOrgID;
                        //        data.CreateUser = i_crm.USERID;
                        //        data.CreateDate = DateTime.Now;
                        //        data.ModifyUser = i_crm.USERID;
                        //        data.ModifyDate = DateTime.Now;

                        //        if (data.Supplier == sOldCustomerGUID)
                        //        {
                        //            data.Supplier = sCustomerGUID;
                        //        }

                        //        if (data.Agent == sOldCustomerGUID)
                        //        {
                        //            data.Agent = sCustomerGUID;
                        //        }

                        //    }

                        //    db.Insertable<OTB_OPM_OtherExhibition>(lOtherExh)
                        //                .ExecuteCommand();
                        //}

                        #endregion

                        #region 其他(駒驛)

                        //var saOtherTG = db.Queryable<OTB_OPM_Exhibition, OTB_OPM_OtherExhibitionTG>((t1, t2) => t1.SN.ToString() == t2.ExhibitionNO && t1.OrgID == t2.OrgID)
                        //                 .Select((t1, t2) => new { t1.OrgID, t1.SN, t1.ExhibitionCode, t1.Exhibitioname_TW, t1.Exhibitioname_CN, t2.ExhibitionNO, t2.Exhibitors, t2.Agent }).MergeTable()
                        //                 .Where(it => it.ExhibitionNO != @"" && it.OrgID == sCurrentOrgID && (it.Exhibitors.Contains(sOldCustomerGUID) || it.Agent == sOldCustomerGUID))
                        //                 .ToList();

                        //if (saOtherTG.Count > 0)
                        //{
                        //    List<OTB_OPM_Exhibition> lExh = db.Queryable<OTB_OPM_Exhibition>()
                        //                                    .Where(p => p.SN == saOtherTG.Select(x => x.SN).First())
                        //                                    .ToList();

                        //    foreach (OTB_OPM_Exhibition data in lExh)
                        //    {

                        //        data.OrgID = sOrgID;
                        //        data.CreateUser = i_crm.USERID;
                        //        data.CreateDate = DateTime.Now;
                        //        data.ModifyUser = i_crm.USERID;
                        //        data.ModifyDate = DateTime.Now;
                        //    }

                        //    List<OTB_OPM_OtherExhibitionTG> lOtherTGExh = db.Queryable<OTB_OPM_OtherExhibitionTG>()
                        //                                    .Where(p => p.ExhibitionNO == saOtherTG.Select(x => x.SN).First().ToString())
                        //                                    .ToList();

                        //    var exData = db.Insertable<OTB_OPM_Exhibition>(lExh)
                        //                                  .IgnoreColumns(p => p == OTB_OPM_Exhibition.CN_SN)
                        //                                  .ExecuteReturnIdentity();

                        //    foreach (OTB_OPM_OtherExhibitionTG data in lOtherTGExh)
                        //    {
                        //        data.Guid = Guid.NewGuid().ToString();
                        //        data.ExhibitionNO = exData.ToString();
                        //        data.OrgID = sOrgID;
                        //        data.CreateUser = i_crm.USERID;
                        //        data.CreateDate = DateTime.Now;
                        //        data.ModifyUser = i_crm.USERID;
                        //        data.ModifyDate = DateTime.Now;

                        //        if (data.Exhibitors.IndexOf(sOldCustomerGUID) > -1)
                        //        {
                        //            data.Exhibitors.Replace(sOldCustomerGUID, sCustomerGUID);
                        //        }

                        //        if (data.Agent == sOldCustomerGUID)
                        //        {
                        //            data.Agent = sCustomerGUID;
                        //        }

                        //    }

                        //    db.Insertable<OTB_OPM_OtherExhibitionTG>(lOtherTGExh)
                        //                .ExecuteCommand();
                        //}

                        #endregion

                        var iRel = db.Insertable(oEntity).ExecuteReturnEntity();
                        var iRelMst = db.Insertable(oMstEntity).ExecuteCommand();
                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, iRel);
                    } while (false);

                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Customers_UpdService), @"客戶管理編輯", @"Update（客戶管理編輯（修改））", @"", @"", @"");
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

        #endregion 客戶管理編輯（修改）

        #region 客戶管理編輯（雙筆查詢）

        /// <summary>
        /// 客戶管理編輯（雙筆查詢）
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on Querytwo</param>
        /// <returns></returns>
        public ResponseMessage Querytwo(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var sId1 = _fetchString(i_crm, @"guid1");
                    var sId2 = _fetchString(i_crm, @"guid2");

                    var oEntity = db.Queryable<OTB_CRM_Customers, OTB_SYS_Members, OTB_SYS_Members>
                        ((t1, t2, t3) =>
                        new object[] {
                                JoinType.Left, t1.OrgID == t2.OrgID && t1.CreateUser == t2.MemberID,
                                JoinType.Left, t1.OrgID == t3.OrgID && t1.ModifyUser == t3.MemberID
                              }
                        )
                        .Where((t1, t2, t3) => t1.guid == sId1 || t1.guid == sId2)
                        //.WhereIF((i_crm.ORIGID == "SG" || i_crm.ORIGID == "SE"), x => x.OrgID == i_crm.ORIGID)
                        .WhereIF((i_crm.ORIGID != "TE" && i_crm.ORIGID != "TG"), x => x.OrgID == i_crm.ORIGID)
                        .WhereIF((i_crm.ORIGID == "TE" || i_crm.ORIGID == "TG"), x => x.OrgID == "TE" || x.OrgID == "TG")
                        .Select((t1, t2, t3) => new OTB_CRM_Customers
                        {
                            guid = SqlFunc.GetSelfAndAutoFill(t1.guid),
                            CreateUserName = t2.MemberName,
                            ModifyUserName = t3.MemberName
                        })
                        .ToList();

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, oEntity);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Customers_QryService), "", "QueryOne（客戶管理編輯（雙筆查詢））", "", "", "");
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

        #endregion 客戶管理編輯（雙筆查詢）

        #region 客戶管理編輯（確認合併客戶）

        /// <summary>
        /// 客戶管理編輯（確認合併客戶）
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on CombineCustomer</param>
        /// <returns></returns>
        public ResponseMessage CheckCombineCustomer(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var iRel = 2;

                    string strGuid1 = _fetchString(i_crm, "guid1");
                    string strGuid2 = _fetchString(i_crm, "guid2");

                    OTB_CRM_Customers oCustomer1 = db.Queryable<OTB_CRM_Customers>().Single(x => x.guid == strGuid1);
                    OTB_CRM_Customers oCustomer2 = db.Queryable<OTB_CRM_Customers>().Single(x => x.guid == strGuid2);

                    if((oCustomer1.IsAudit == "N" || oCustomer1.IsAudit == "Q") && (oCustomer2.IsAudit == "N" || oCustomer2.IsAudit == "Q"))
                    {
                        iRel = 1;
                    } else if ((oCustomer1.IsAudit == "N" || oCustomer1.IsAudit == "Q") && oCustomer2.IsAudit == "Y")
                    {
                        iRel = 2;
                    } else if ((oCustomer2.IsAudit == "N" || oCustomer2.IsAudit == "Q") && oCustomer1.IsAudit == "Y")
                    {
                        iRel = 2;
                    } else if (oCustomer1.IsAudit == "Y" && oCustomer2.IsAudit == "Y")
                    {
                        iRel = 3;
                    } else
                    {
                        if (oCustomer1.IsAudit == "A" || oCustomer2.IsAudit == "A")
                        {
                            iRel = 4;
                        } else if (oCustomer1.IsAudit == "P" || oCustomer2.IsAudit == "P")
                        {
                            iRel = 5;
                        }else if (oCustomer1.IsAudit == "Z" || oCustomer2.IsAudit == "Z")
                        {
                            iRel = 6;
                        } else {
                            iRel = 7;
                        }
                    }

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, iRel);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Customers_UpdService), "", "CheckCombineCustomer（客戶管理編輯（確認合併客戶））", "", "", "");
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

        #endregion 客戶管理編輯（確認合併客戶）

        #region 客戶管理編輯（合併客戶）

        /// <summary>
        /// 客戶管理編輯（合併客戶）
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on CombineCustomer</param>
        /// <returns></returns>
        public ResponseMessage CombineCustomer(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var iRel = 1;

                    string sUSERID = i_crm.USERID;
                    DateTime dtNow = DateTime.Now;

                    string strType = _fetchString(i_crm, "Type");

                    string strUpdateGuid = "";
                    string strDeleteGuid = "";

                    int intCount = 0;

                    if (strType == "1") //非正式與非正式合併
                    {
                        if (_fetchString(i_crm, "rdbCustomerNO") == "1")
                        {
                            strUpdateGuid = _fetchString(i_crm, "guid1");
                            strDeleteGuid = _fetchString(i_crm, "guid2");
                        }
                        else
                        {
                            strUpdateGuid = _fetchString(i_crm, "guid2");
                            strDeleteGuid = _fetchString(i_crm, "guid1");
                        }

                        //更新客戶資料
                        OTB_CRM_Customers oUpdateCustomer = new OTB_CRM_Customers
                        {
                            CustomerNO = _fetchString(i_crm, "CustomerNO" + _fetchString(i_crm, "rdbCustomerNO")),
                            IsBlackList = _fetchString(i_crm, "IsBlackList" + _fetchString(i_crm, "rdbIsBlackList")),
                            BlackListReason = _fetchString(i_crm, "BlackListReason" + _fetchString(i_crm, "rdbIsBlackList")),
                            IsGroupUnit = _fetchString(i_crm, "IsGroupUnit" + _fetchString(i_crm, "rdbIsGroupUnit")),
                            UniCode = _fetchString(i_crm, "UniCode" + _fetchString(i_crm, "rdbUniCode")),
                            CustomerShotCName = _fetchString(i_crm, "CustomerShotCName" + _fetchString(i_crm, "rdbCustomerShotCName")),
                            CustomerCName = _fetchString(i_crm, "CustomerCName" + _fetchString(i_crm, "rdbCustomerCName")),
                            CustomerEName = _fetchString(i_crm, "CustomerEName" + _fetchString(i_crm, "rdbCustomerEName")),
                            Telephone = _fetchString(i_crm, "Telephone" + _fetchString(i_crm, "rdbTelephone")),
                            Email = _fetchString(i_crm, "Email" + _fetchString(i_crm, "rdbEmail")),
                            FAX = _fetchString(i_crm, "FAX" + _fetchString(i_crm, "rdbFAX")),
                            State = _fetchString(i_crm, "State" + _fetchString(i_crm, "rdbState")),
                            Address = _fetchString(i_crm, "Address" + _fetchString(i_crm, "rdbAddress")),
                            InvoiceAddress = _fetchString(i_crm, "InvoiceAddress" + _fetchString(i_crm, "rdbInvoiceAddress")),
                            WebsiteAdress = _fetchString(i_crm, "WebsiteAddress" + _fetchString(i_crm, "rdbWebsiteAddress")),
                            Memo = _fetchString(i_crm, "Memo" + _fetchString(i_crm, "rdbMemo")),
                            ModifyUser = sUSERID,
                            ModifyDate = dtNow
                        };
                        iRel = db.Updateable(oUpdateCustomer).UpdateColumns(it => new {
                            it.CustomerNO, it.IsBlackList, it.BlackListReason, it.IsGroupUnit,
                            it.UniCode, it.CustomerShotCName, it.CustomerCName,  it.CustomerEName,
                            it.Telephone, it.Email, it.FAX, it.State,it.Address, it.InvoiceAddress, it.WebsiteAdress, it.Memo,
                            it.ModifyUser, it.ModifyDate
                        }).Where(x => x.guid == strUpdateGuid).ExecuteCommand();

                        OTB_CRM_CustomersMST oUpdateCustomerMST = new OTB_CRM_CustomersMST
                        {
                            CustomerNO = _fetchString(i_crm, "CustomerNO" + _fetchString(i_crm, "rdbCustomerNO")),
                            ModifyUser = sUSERID,
                            ModifyDate = dtNow
                        };
                        iRel = db.Updateable(oUpdateCustomerMST).UpdateColumns(it => new {
                            it.CustomerNO, it.ModifyUser, it.ModifyDate
                        }).Where(x => x.customer_guid == strUpdateGuid).ExecuteCommand();

                    } else if (strType == "2") //正式與非正式合併
                    {
                        intCount = db.Queryable<OTB_CRM_Customers>().Where(x => x.IsAudit == "Y" && x.guid == _fetchString(i_crm, "guid1")).Count();
                        if (intCount > 0)
                        {
                            strUpdateGuid = _fetchString(i_crm, "guid1");
                            strDeleteGuid = _fetchString(i_crm, "guid2");
                        } else
                        {
                            strUpdateGuid = _fetchString(i_crm, "guid2");
                            strDeleteGuid = _fetchString(i_crm, "guid1");
                        }
                    }

                    //聯絡人全部合併，更新聯絡人資料的CustomerId
                    //List<OTB_CRM_Contactors> listContactors1 = db.Queryable<OTB_CRM_Contactors>().Where(x => x.CustomerId == strUpdateGuid).ToList();
                    List<OTB_CRM_Contactors> listContactors2 = db.Queryable<OTB_CRM_Contactors>().Where(x => x.CustomerId == strDeleteGuid).ToList();
                    foreach (OTB_CRM_Contactors oContactors2 in listContactors2)
                    {
                        //intCount = 0;
                        //intCount = listContactors1.Where(x => x.ContactorName == oContactors2.ContactorName).Count();

                        //if (intCount == 0)
                        //{
                            //將聯絡人轉到要更新的客戶
                            OTB_CRM_Contactors oUpdateContactor = new OTB_CRM_Contactors
                            {
                                CustomerId = strUpdateGuid,
                                ModifyUser = sUSERID,
                                ModifyDate = dtNow
                            };
                            iRel = db.Updateable(oUpdateContactor).UpdateColumns(it => new {
                                it.CustomerId,
                                it.ModifyUser,
                                it.ModifyDate
                            }).Where(x => x.CustomerId == strDeleteGuid).ExecuteCommand();
                        //}
                    }

                    //處理展覽名單客戶關聯、聯絡人關聯
                    List<OTB_OPM_ExhibitionCustomers> listExhibitionCustomers = db.Queryable<OTB_OPM_ExhibitionCustomers>().Where(x => x.CustomerId == strDeleteGuid).ToList();
                    foreach (OTB_OPM_ExhibitionCustomers oExhibitionCustomers in listExhibitionCustomers)
                    {
                        intCount = 0;
                        intCount = db.Queryable<OTB_OPM_ExhibitionCustomers>().Where(x => x.ExhibitionNO == oExhibitionCustomers.ExhibitionNO && x.CustomerId == strUpdateGuid).Count();
                        if (intCount == 0)
                        {
                            //兩客戶沒有同時存在同一展覽，更新客戶關聯
                            OTB_OPM_ExhibitionCustomers oUpdateExhibitionCustomer = new OTB_OPM_ExhibitionCustomers
                            {
                                CustomerId = strUpdateGuid,
                                ModifyUser = sUSERID,
                                ModifyDate = dtNow
                            };
                            iRel = db.Updateable(oUpdateExhibitionCustomer).UpdateColumns(it => new {
                                it.CustomerId,
                                it.ModifyUser,
                                it.ModifyDate
                            }).Where(x => x.ExhibitionNO == oExhibitionCustomers.ExhibitionNO && x.CustomerId == strDeleteGuid).ExecuteCommand();

                            //更新聯絡人關聯
                            List<OTB_OPM_ExhibitionContactors> listExhibitionContactors = db.Queryable<OTB_OPM_ExhibitionContactors>().Where(x => x.ExhibitionNO == oExhibitionCustomers.ExhibitionNO && x.CustomerId == strDeleteGuid).ToList();
                            foreach (OTB_OPM_ExhibitionContactors oExhibitionContactors in listExhibitionContactors)
                            {
                                OTB_OPM_ExhibitionContactors oUpdateExhibitionContactor = new OTB_OPM_ExhibitionContactors
                                {
                                    CustomerId = strUpdateGuid,
                                    ModifyUser = sUSERID,
                                    ModifyDate = dtNow
                                };
                                iRel = db.Updateable(oUpdateExhibitionContactor).UpdateColumns(it => new {
                                    it.CustomerId,
                                    it.ModifyUser,
                                    it.ModifyDate
                                }).Where(x => x.ExhibitionNO == oExhibitionCustomers.ExhibitionNO && x.CustomerId == strDeleteGuid).ExecuteCommand();
                            }
                        } else
                        {
                            //兩客戶同時存在同一展覽時
                            //更新聯絡人關聯
                            List<OTB_OPM_ExhibitionContactors> listExhibitionContactors = db.Queryable<OTB_OPM_ExhibitionContactors>().Where(x => x.ExhibitionNO == oExhibitionCustomers.ExhibitionNO && x.CustomerId == strDeleteGuid).ToList();
                            foreach (OTB_OPM_ExhibitionContactors oExhibitionContactors in listExhibitionContactors)
                            {
                                OTB_OPM_ExhibitionContactors oUpdateExhibitionContactor = new OTB_OPM_ExhibitionContactors
                                {
                                    CustomerId = strUpdateGuid,
                                    ModifyUser = sUSERID,
                                    ModifyDate = dtNow
                                };
                                iRel = db.Updateable(oUpdateExhibitionContactor).UpdateColumns(it => new {
                                    it.CustomerId,
                                    it.ModifyUser,
                                    it.ModifyDate
                                }).Where(x => x.ExhibitionNO == oExhibitionCustomers.ExhibitionNO && x.CustomerId == strDeleteGuid).ExecuteCommand();
                            }

                            //刪除客戶關聯
                            iRel = db.Deleteable<OTB_OPM_ExhibitionCustomers>().Where(x => x.ExhibitionNO == oExhibitionCustomers.ExhibitionNO && x.CustomerId == strDeleteGuid).ExecuteCommand();
                            
                            //刪除聯絡人關聯
                            iRel = db.Deleteable<OTB_OPM_ExhibitionContactors>().Where(x => x.ExhibitionNO == oExhibitionCustomers.ExhibitionNO && x.CustomerId == strDeleteGuid).ExecuteCommand();
                        }
                    }

                    //處理Callout紀錄
                    OTB_CRM_Callout oUpdateCallout = new OTB_CRM_Callout
                    {
                        CustomerId = strUpdateGuid,
                        ModifyUser = sUSERID,
                        ModifyDate = dtNow
                    };
                    iRel = db.Updateable(oUpdateCallout).UpdateColumns(it => new {
                        it.CustomerId,
                        it.ModifyUser,
                        it.ModifyDate
                    }).Where(x => x.CustomerId == strDeleteGuid).ExecuteCommand();

                    //處理客訴
                    OTB_CRM_Complaint oUpdateComplaint = new OTB_CRM_Complaint
                    {
                        CustomerId = strUpdateGuid,
                        ModifyUser = sUSERID,
                        ModifyDate = dtNow
                    };
                    iRel = db.Updateable(oUpdateComplaint).UpdateColumns(it => new {
                        it.CustomerId,
                        it.ModifyUser,
                        it.ModifyDate
                    }).Where(x => x.CustomerId == strDeleteGuid).ExecuteCommand();

                    //處理滿意度問券
                    OTB_CRM_SatisfactionCustomer oUpdateSatisfactionCustomer = new OTB_CRM_SatisfactionCustomer
                    {
                        CustomerID = strUpdateGuid
                    };
                    iRel = db.Updateable(oUpdateSatisfactionCustomer).UpdateColumns(it => new {
                        it.CustomerID
                    }).Where(x => x.CustomerID == strDeleteGuid && x.CompareDB == "Y").ExecuteCommand();

                    //刪除客戶
                    iRel = db.Deleteable<OTB_CRM_Customers>().Where(x => x.guid == strDeleteGuid).ExecuteCommand();
                    iRel = db.Deleteable<OTB_CRM_CustomersMST>().Where(x => x.customer_guid == strDeleteGuid).ExecuteCommand();

                    //刪除聯絡人
                    iRel = db.Deleteable<OTB_CRM_Contactors>().Where(x => x.CustomerId == strDeleteGuid).ExecuteCommand();

                    iRel = 1;

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, iRel);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Customers_UpdService), "", "CombineCustomer（客戶管理編輯（合併客戶））", "", "", "");
            }
            finally
            {
                if (null != sMsg)
                {
                    rm = new ErrorResponseMessage(sMsg, i_crm);
                }

                Logger.Debug(@"Customers_UpdService.CombineCustomer Debug（Param：" + JsonToString(i_crm) + @"；Response：" + JsonToString(rm) + @"）------------------");
            }
            return rm;
        }

        #endregion 客戶管理編輯（合併客戶）

        #region 客戶管理編輯（抓取參加聯絡人列表資料）

        /// <summary>
        /// 客戶管理編輯（抓取參加聯絡人列表資料）
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on GetContactorlist</param>
        /// <returns></returns>
        public ResponseMessage GetContactorlist(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var sId = _fetchString(i_crm, @"Guid");
                    var sSortField = _fetchString(i_crm, @"sortField");
                    var sSortOrder = _fetchString(i_crm, @"sortOrder");

                    int iPageCount = 0;

                    if (string.IsNullOrEmpty(sSortField))
                    {
                        sSortField = "OrderByValue";
                    }
                    if (string.IsNullOrEmpty(sSortOrder))
                    {
                        sSortOrder = "asc";
                    }

                    List<OTB_CRM_Contactors> listContactors = db.Queryable<OTB_CRM_Contactors>().Where(x => x.CustomerId == sId).OrderBy(sSortField, sSortOrder).ToPageList(1, 999999999, ref iPageCount);

                    int? intOrderCount = listContactors.Max(x => x.OrderByValue);

                    List<View_CRM_Contactors> list = new List<View_CRM_Contactors>();

                    foreach (OTB_CRM_Contactors Contactors in listContactors)
                    {
                        View_CRM_Contactors oViewCon = new View_CRM_Contactors();
                        oViewCon.RowIndex = Contactors.RowIndex;
                        oViewCon.guid = Contactors.guid;
                        oViewCon.CustomerId = Contactors.CustomerId;
                        oViewCon.ContactorName = Contactors.ContactorName;
                        oViewCon.Call = Contactors.Call;
                        oViewCon.JobTitle = Contactors.JobTitle;
                        oViewCon.Telephone1 = Contactors.Telephone1;
                        oViewCon.Ext1 = Contactors.Ext1;
                        oViewCon.Email1 = Contactors.Email1;
                        oViewCon.Memo = Contactors.Memo;
                        oViewCon.OrderByValue = Contactors.OrderByValue;
                        oViewCon.OrderCount = intOrderCount;

                        list.Add(oViewCon);
                    }

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, list);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Customers_UpdService), @"客戶管理編輯", @"GetContactorList（客戶管理編輯（抓取參加聯絡人列表資料））", @"", @"", @"");
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

        #endregion 客戶管理編輯（抓取參加聯絡人列表資料）

        #region 客戶管理編輯（檢查聯絡人姓名是否重複）

        /// <summary>
        /// 客戶管理編輯（檢查聯絡人姓名是否重複）
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on checkContactorName</param>
        /// <returns></returns>
        public ResponseMessage checkContactorName(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    OTB_CRM_Contactors oEntity = _fetchEntity<OTB_CRM_Contactors>(i_crm);

                    var iRel = 1;

                    int iCount = db.Queryable<OTB_CRM_Contactors>()
                            .Where(x => x.CustomerId == oEntity.CustomerId && x.ContactorName == oEntity.ContactorName)
                            .Count();

                    if (iCount > 0)
                    {
                        iRel = 0;
                        sMsg = "已存在相同姓名之聯絡人，無法新增";
                    }

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, iRel);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Customers_UpdService), "", "checkContactorName（客戶管理編輯（檢查聯絡人姓名是否重複））", "", "", "");
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

        #endregion 客戶管理編輯（檢查聯絡人姓名是否重複）

        #region 客戶管理編輯（取得名單來源）

        /// <summary>
        /// 客戶管理編輯（取得名單來源）
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on GetContactorlist</param>
        /// <returns></returns>
        public ResponseMessage GetListSource(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    //var sId = _fetchString(i_crm, @"Guid");

                    var saList = new List<Map>();
                    //case isnull(CustomerShotCName,'') when '' then (case CustomerCName when '' then CustomerEName else CustomerCName end) else '('+CustomerShotCName+')'+(case CustomerCName when '' then CustomerEName else CustomerCName end)+'('+OrgID+')' end
                    //var saCustomers = db.Queryable<OTB_CRM_Customers>().Where(x => x.IsAudit == "Y" && (x.IsGroupUnit == "Y" || x.TransactionType == "C"))
                    //    .Select(t1 => new
                    //    {
                    //        t1.guid,
                    //        t1.CustomerShotCName,
                    //        text = t1.CustomerShotCName + "-" + t1.CustomerCName
                    //    }).ToList();

                    var spm = new SugarParameter(@"OrgID", i_crm.ORIGID);
                    System.Data.DataTable saCustomerlist = new System.Data.DataTable();
                    var saCustomers = db.Ado.GetDataTable(@"select guid id,case isnull(CustomerShotCName,'') when '' then (case CustomerCName when '' then CustomerEName else CustomerCName end) else '('+CustomerShotCName+')'+(case CustomerCName when '' then CustomerEName else CustomerCName end) end text,CustomerCName textcn, CustomerShotCName from OTB_CRM_Customers where IsAudit <> 'Q' AND IsAudit <> 'N' AND (IsGroupUnit = 'Y' OR TransactionType = 'C')", spm);

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, saCustomers);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Customers_UpdService), @"客戶管理編輯", @"GetListSource（客戶管理編輯（取得名單來源））", @"", @"", @"");
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

        #endregion 客戶管理編輯（取得名單來源）

        #region 客戶管理編輯（取得已成交展覽名單）

        /// <summary>
        /// 客戶管理編輯（取得已成交展覽名單）
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
                    } else
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
                                } else
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
                                } else
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
                    .OrderBy("ExhibitionDateStart", "desc")
                    .ToPageList(1, 999999999, ref iPageCount);
                    

                    string strRefNumber = "";

                    List<View_OPM_Exhibition> listExhibition = new List<View_OPM_Exhibition>();
                    foreach (View_OPM_Exhibition oExhibition in listDealExhibition)
                    {
                        strRefNumber = "";

                        foreach (OTB_OPM_ImportExhibition oImportExhibition in listImportExhibition.Where(x => x.ExhibitionNO == oExhibition.SN.ToString()))
                        {
                            strRefNumber = strRefNumber + "1;" + oImportExhibition.ImportBillNO + ";" + oImportExhibition.RefNumber + ";" + oImportExhibition.OrgID + ",";
                        }

                        foreach (OTB_OPM_ExportExhibition oExportExhibition in listExportExhibition.Where(x => x.ExhibitionNO == oExhibition.SN.ToString()))
                        {
                            var jaExhibitors = (JArray)JsonConvert.DeserializeObject(oExportExhibition.Exhibitors);

                            foreach (JObject joExhibitors in jaExhibitors)
                            {
                                if (joExhibitors["SupplierID"].ToString() == sId && joExhibitors["VoidContent"] == null)
                                {
                                    strRefNumber = strRefNumber + "2;" + oExportExhibition.ExportBillNO + ";" + oExportExhibition.RefNumber + ";" + oExportExhibition.OrgID + ",";
                                }
                            }
                        }
                        
                        foreach (OTB_OPM_OtherExhibition oOtherExhibition in listOtherExhibition.Where(x => x.ExhibitionNO == oExhibition.SN.ToString()))
                        {
                            strRefNumber = strRefNumber + "3;" + oOtherExhibition.Guid + ";" + oOtherExhibition.CreateDate + ";" + oOtherExhibition.OrgID + ",";
                        }
                        
                        foreach (OTB_OPM_OtherExhibitionTG oOtherExhibitionTG in listOtherExhibitionTG.Where(x => x.ExhibitionNO == oExhibition.SN.ToString()))
                        {
                            var jaExhibitors = (JArray)JsonConvert.DeserializeObject(oOtherExhibitionTG.Exhibitors);

                            foreach (JObject joExhibitors in jaExhibitors)
                            {
                                if (joExhibitors["SupplierID"].ToString() == sId && joExhibitors["VoidContent"] == null)
                                {
                                    strRefNumber = strRefNumber + "4;" + oOtherExhibitionTG.Guid + ";" + oOtherExhibitionTG.CreateDate + ";" + oOtherExhibitionTG.OrgID + ",";
                                }
                            }
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
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Customers_UpdService), @"客戶管理編輯", @"GetDealExhibitionlist（客戶管理編輯（取得已成交展覽名單））", @"", @"", @"");
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

        #endregion 客戶管理編輯（取得已成交展覽名單）

        #region 客戶管理編輯（取得未成交展覽名單）

        /// <summary>
        /// 客戶管理編輯（取得未成交展覽名單）
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
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Customers_UpdService), @"客戶管理編輯", @"GetUnDealExhibitionlist（客戶管理編輯（取得未成交展覽名單））", @"", @"", @"");
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

        #endregion 客戶管理編輯（取得未成交展覽名單）

        #region 客戶管理編輯（新增至展覽名單）

        /// <summary>
        /// 客戶管理編輯（新增至展覽名單）
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
                        oExhibitionCustomers.SourceType = "4";
                        oExhibitionCustomers.CreateUser = sUSERID;
                        oExhibitionCustomers.CreateDate = dtNow;
                        oExhibitionCustomers.ModifyUser = sUSERID;
                        oExhibitionCustomers.ModifyDate = dtNow;
                        db.Insertable(oExhibitionCustomers).ExecuteCommand();

                        //新增展覽客戶聯絡人關聯
                        JArray jaContactors = (JArray)JsonConvert.DeserializeObject(_fetchString(i_crm, @"Contactors"));
                        if (jaContactors.Count > 0)
                        {
                            foreach (JObject joContactors in jaContactors)
                            {
                                OTB_OPM_ExhibitionContactors oExhibitionContactors = new OTB_OPM_ExhibitionContactors();
                                oExhibitionContactors.ExhibitionNO = strExhibitionNO;
                                oExhibitionContactors.CustomerId = strCustomerId;
                                oExhibitionContactors.ContactorId = joContactors["guid"].ToString();
                                oExhibitionContactors.CreateUser = sUSERID;
                                oExhibitionContactors.CreateDate = dtNow;
                                oExhibitionContactors.ModifyUser = sUSERID;
                                oExhibitionContactors.ModifyDate = dtNow;
                                oExhibitionContactors.SourceType = "4";

                                db.Insertable(oExhibitionContactors).ExecuteCommand();
                            }
                        }

                        iRel = 1;
                    } else
                    {
                        sMsg = "已存在該展覽之名單，無法新增";
                    }

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, iRel);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Customers_UpdService), "", "InsertExhibitionList（客戶管理編輯（新增至展覽名單））", "", "", "");
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
        #endregion 客戶管理編輯（新增至展覽名單）

        #region 取得Callout紀錄

        /// <summary>
        /// 取得Callout紀錄
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on GetContactorlist</param>
        /// <returns></returns>
        public ResponseMessage GetCalloutlist(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    string sId = _fetchString(i_crm, @"guid");

                    List<View_OPM_Exhibition> listExhibition = db.Queryable<OTB_OPM_Exhibition, OTB_OPM_ExhibitionCustomers>
                        ((t1, t2) => new object[] {
                            JoinType.Inner, t1.SN.ToString() == t2.ExhibitionNO
                        })
                        .Where((t1, t2) => t2.CustomerId == sId)
                        .Select((t1, t2) => new View_OPM_Exhibition
                        {
                            SN = SqlFunc.GetSelfAndAutoFill(t1.SN),
                            ExhibitionCode = t1.ExhibitionCode,
                            ExhibitioShotName_TW = t1.ExhibitioShotName_TW,
                            Exhibitioname_TW = t1.Exhibitioname_TW,
                            Exhibitioname_EN = t1.Exhibitioname_EN,
                            ExhibitionDateStart = t1.ExhibitionDateStart,
                            ExhibitionDateEnd = t1.ExhibitionDateEnd,
                            CreateUserName = SqlFunc.MappingColumn(t1.OrgID, "dbo.[OFN_SYS_MemberNameByMemberIDwithoutOrgID](t2.CreateUser)"),
                            CreateDate = t2.CreateDate
                        }).OrderBy("t1.ExhibitionDateStart", "desc").ToList();

                    List<View_OPM_Exhibition> list = new List<View_OPM_Exhibition>();
                    View_OPM_Exhibition oEntity = new View_OPM_Exhibition();
                    int i = 1;
                    foreach (View_OPM_Exhibition oExhibition in listExhibition)
                    {
                        oEntity = oExhibition;
                        oEntity.RowIndex = i;
                        list.Add(oEntity);
                        i++;
                    }

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, listExhibition);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Callout_UpdService), @"客戶管理編輯", @"GetCalloutlist（取得Callout紀錄）", @"", @"", @"");
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

        #endregion 取得Callout紀錄

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
                    var sId = _fetchString(i_crm, @"guid");

                    //List<OTB_CRM_Complaint> listComplaint = db.Queryable<OTB_CRM_Complaint>().Where(x => x.CustomerId == sId).OrderBy("CreateDate", "desc").ToList();

                    var listComplaint = db.Queryable<OTB_CRM_Complaint, OTB_OPM_Exhibition>
                       ((t1, t2) =>
                       new object[] {
                                JoinType.Inner, t1.ExhibitionNO == t2.SN.ToString()
                             }
                       ).Where((t1, t2) => t1.CustomerId == sId)
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

        #region 取得滿意度列表

        /// <summary>
        /// 取得客訴列表
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on GetContactorlist</param>
        /// <returns></returns>
        public ResponseMessage GetSatisfactionCaselist(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var sId = _fetchString(i_crm, @"guid");

                    //List<OTB_CRM_Complaint> listComplaint = db.Queryable<OTB_CRM_Complaint>().Where(x => x.CustomerId == sId).OrderBy("CreateDate", "desc").ToList();

                    var listSatisfactionCase = db.Queryable<OTB_CRM_SatisfactionCase, OTB_CRM_SatisfactionCustomer, OTB_OPM_Exhibition>
                        ((t1, t2, t3) =>
                        new object[] {
                                JoinType.Inner, t1.SN == t2.CaseSN.ToString(),
                                JoinType.Inner, t1.ExhibitionNO == t3.SN.ToString()
                              }
                        ).Where((t1, t2, t3) => t2.CustomerID == sId)
                        .Select((t1, t2, t3) => new View_CRM_SatisfactionCase
                        {
                            SN = t1.SN,
                            CaseName = t1.CaseName,
                            ExhibitioShotName_TW = t3.ExhibitioShotName_TW,
                            Exhibitioname_TW = t3.Exhibitioname_TW,
                            CustomerSN = t2.SN,
                            Feild01 = t2.Feild01,
                            CreateUserName = SqlFunc.MappingColumn(t1.CaseName, "dbo.[OFN_SYS_MemberNameByMemberIDwithoutOrgID](t1.CreateUser)")
                        })
                        .MergeTable().OrderBy("SN", "desc").ToPageList(1, 999999999);

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, listSatisfactionCase);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Callout_UpdService), @"客戶管理編輯", @"GetSatisfactionCaselist（取得滿意度列表）", @"", @"", @"");
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

        #endregion 取得滿意度列表

        #region 客戶管理（更新聯絡人排序）

        /// <summary>
        /// 客戶管理（更新聯絡人排序）
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on UpdImportCustomers</param>
        /// <returns></returns>
        public ResponseMessage UpdateOrderByValue(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            try
            {
                rm = SugarBase.ExecTran(db =>
                {
                    do
                    {
                        string strCustomerId = _fetchString(i_crm, @"CustomerId");
                        var sId = _fetchString(i_crm, @"Id");
                        var iOldOrderByValue = _fetchInt(i_crm, @"OldOrderByValue");
                        var iNewOrderByValue = _fetchInt(i_crm, @"NewOrderByValue");

                        //var oOrderEntity = db.Queryable<OTB_SYS_ArgumentClass>().Single(x => x.OrgID == i_crm.ORIGID && x.ArgumentClassID == sId);

                        if (iNewOrderByValue > iOldOrderByValue)
                        {
                            var iRelUp = db.Updateable<OTB_CRM_Contactors>()
                                           .UpdateColumns(x => new OTB_CRM_Contactors { OrderByValue = x.OrderByValue - 1 })
                                           .Where(x => x.CustomerId == strCustomerId && x.OrderByValue <= iNewOrderByValue && x.OrderByValue > iOldOrderByValue)
                                           .ExecuteCommand();
                        }
                        else
                        {
                            var iRelDown = db.Updateable<OTB_CRM_Contactors>()
                                             .UpdateColumns(x => new OTB_CRM_Contactors { OrderByValue = x.OrderByValue + 1 })
                                             .Where(x => x.CustomerId == strCustomerId && x.OrderByValue >= iNewOrderByValue && x.OrderByValue < iOldOrderByValue)
                                             .ExecuteCommand();
                        }
                        var iRelSelf = db.Updateable(new OTB_CRM_Contactors { OrderByValue = iNewOrderByValue })
                                         .UpdateColumns(x => x.OrderByValue)
                                         .Where(x => x.CustomerId == strCustomerId && x.guid == sId).ExecuteCommand();

                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, iRelSelf);
                    } while (false);

                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Customers_UpdService), @"參數類別管理", @"UpdateOrderByValue（客戶管理（更新聯絡人排序））", @"", @"", @"");
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

        #endregion 客戶管理（更新聯絡人排序）
    }
}