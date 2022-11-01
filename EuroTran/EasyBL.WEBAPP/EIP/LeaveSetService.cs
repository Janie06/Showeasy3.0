using EasyBL.WebApi.Message;
using Entity.Sugar;
using Newtonsoft.Json;
using SqlSugar;
using SqlSugar.Base;
using System;
using System.Collections.Generic;

namespace EasyBL.WEBAPP.EIP
{
    public class LeaveSetService : ServiceBase
    {
        #region 初始化差勤設定

        /// <summary>
        /// 初始化差勤設定
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage InitLeaveSet(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var sCurrentYear = _fetchString(i_crm, @"CurrentYear");
                    var saMembers = db.Queryable<OTB_SYS_Members>()
                        .Where(t => t.OrgID == i_crm.ORIGID && t.Effective == @"Y" && t.ServiceCode != @"API").ToList();

                    var saArguments = db.Queryable<OTB_SYS_Arguments>()
                        .Where(t => t.OrgID == i_crm.ORIGID && t.Effective == @"Y" && t.ArgumentClassID == @"LeaveType").ToList();

                    var saLeaveSet = new List<OTB_EIP_LeaveSet>();
                    foreach (OTB_SYS_Members user in saMembers)
                    {
                        var iCount = db.Queryable<OTB_EIP_LeaveSet>()
                            .Count(t => t.OrgID == i_crm.ORIGID && t.UserID == user.MemberID && t.TYear == sCurrentYear);
                        if (iCount == 0)
                        {
                            var oLeaveSet = new OTB_EIP_LeaveSet
                            {
                                Guid = Guid.NewGuid().ToString(),
                                OrgID = i_crm.ORIGID,
                                UserID = user.MemberID,
                                TYear = sCurrentYear,
                                CreateUser = @"Transfer",
                                CreateDate = DateTime.Now
                            };
                            var dicSetInfo = new List<Dictionary<string, object>>();
                            foreach (OTB_SYS_Arguments arg in saArguments)
                            {
                                var dicInfo = new Dictionary<string, object>
                                {
                                    { @"Id", arg.ArgumentID },
                                    { @"Name", arg.ArgumentValue },
                                    { @"PaymentHours", arg.Correlation ?? @"" },
                                    { @"UsedHours", @"0" },
                                    { @"RemainHours", arg.Correlation ?? @"0" },
                                    { @"Memo", arg.Memo }
                                };
                                dicSetInfo.Add(dicInfo);
                            }
                            oLeaveSet.SetInfo = JsonConvert.SerializeObject(dicSetInfo, Formatting.Indented);
                            saLeaveSet.Add(oLeaveSet);
                        }
                    }
                    if (saLeaveSet.Count > 0)
                    {
                        var iRel = db.Insertable<OTB_EIP_LeaveSet>(saLeaveSet).ExecuteCommand();
                    }

                    rm = new SuccessResponseMessage(null, i_crm);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(LeaveSetService), @"差勤設定", @"GetImportFeeitems（初始化差勤設定）", @"", @"", @"");
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

        #endregion 初始化差勤設定

        #region 修改特休假時數

        /// <summary>
        /// 修改特休假時數
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage UpdateLeaveTX(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            try
            {
                rm = SugarBase.ExecTran(db =>
                {
                    do
                    {
                        var sUserId = _fetchString(i_crm, @"UserId");
                        var sAddLeaveHours = _fetchString(i_crm, @"AddLeaveHours");
                        var sRemainHours = _fetchString(i_crm, @"RemainHours");
                        var sSouseId = _fetchString(i_crm, @"SouseId");
                        var sMmeo = _fetchString(i_crm, @"Memo");
                        var sCurYear = DateTime.Now.Year;

                        var iAddLeaveHours = -decimal.Parse(sAddLeaveHours);

                        var saWenZhong1 = db.Queryable<OTB_EIP_WenZhong>()
                                           .Where(x => x.OrgID == i_crm.ORIGID && x.UserID == sUserId).OrderBy(x => x.EnableDate)
                                           .ToList();

                        var saWenZhong = db.Queryable<OTB_EIP_WenZhong>()
                                           .Where(x => x.OrgID == i_crm.ORIGID && x.UserID == sUserId && (x.EnableDate.Value.Year == sCurYear || x.ExpirationDate.Value.Year == sCurYear)).OrderBy(x => x.EnableDate)
                                           .ToList();
                        var saWenZhong_Upd = new List<OTB_EIP_WenZhong>();
                        decimal? iOffer = iAddLeaveHours;
                        foreach (var item in saWenZhong)
                        {
                            if (item.RemainHours < iOffer)
                            {
                                iOffer -= item.RemainHours;
                                item.UsedHours += item.RemainHours;
                                item.RemainHours = 0;
                            }
                            else
                            {
                                item.RemainHours = item.RemainHours - iOffer;
                                item.UsedHours += iOffer;
                                iOffer = 0;
                            }
                            item.ModifyUser = i_crm.USERID;
                            item.ModifyDate = DateTime.Now;
                            saWenZhong_Upd.Add(item);
                            if (iOffer <= 0)
                            {
                                break;
                            }
                        }
                        db.Updateable(saWenZhong_Upd).UpdateColumns(x => new
                        {
                            x.RemainHours,
                            x.UsedHours
                        }).ExecuteCommand();

                        var logInfo = new OTB_SYS_LogInfo
                        {
                            OrgID = i_crm.ORIGID,
                            SouseId = sSouseId,
                            LogType = "leavesetchange"
                        };
                        Dictionary<string, string> dic = new Dictionary<string, string>();
                        decimal? iRemainHoursr = decimal.Parse(sRemainHours);
                        var sChangeRecord = $"特休假可用時數：{iRemainHoursr + iAddLeaveHours}小時 => {sRemainHours}小時";
                        if (i_crm.LANG == @"zh")
                        {
                            sChangeRecord = ChineseStringUtility.ToSimplified(sChangeRecord);
                        }
                        dic.Add("ChangeRecord", sChangeRecord);
                        var member = db.Queryable<OTB_SYS_Members>().Single(x => x.OrgID == i_crm.ORIGID && x.MemberID == i_crm.USERID);
                        dic.Add("ChangeUserName", member.MemberName);
                        dic.Add("Memo", sMmeo);
                        logInfo.LogInfo = JsonToString(dic);
                        _setEntityBase(logInfo, i_crm);
                        db.Insertable(logInfo).ExecuteCommand();
                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, "");
                    } while (false);
                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(LeaveSetService), @"差勤設定", @"GetImportFeeitems（修改特休假時數）", @"", @"", @"");
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

        public ResponseMessage GetChangLog(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var slookupGuid = _fetchString(i_crm, @"SouseId");
                    var sLogType = _fetchString(i_crm, @"LogType");
                    var logInfos = db.Queryable<OTB_SYS_LogInfo>()
                        .Where(x => x.OrgID == i_crm.ORIGID && x.SouseId == slookupGuid && sLogType.Contains(x.LogType) ).OrderBy(x => x.CreateDate)
                        .ToList();

                    rm = new SuccessResponseMessage(null, i_crm);
                    var sJson = JsonConvert.SerializeObject(logInfos, Formatting.Indented); //把list轉成Json字串
                    rm.DATA.Add(BLWording.REL, sJson);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(LeaveSetService), @"差勤設定", @"GetChangLog（取得變動紀錄）", @"", @"", @"");
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

        #endregion 修改特休假時數

        #region 獲取請假規則設定

        /// <summary>
        /// 獲取請假規則設定
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage GetLeaveSetting(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.DB;
            try
            {
                do
                {
                    var saLeaveRuelsSetting = db.Queryable<OTB_SYS_Arguments, OTB_SYS_ArgumentsRelated>((a, b) =>
                                    new object[] {
                                              JoinType.Left,a.OrgID==b.OrgID && a.ArgumentClassID==b.ArgumentClassID && a.ArgumentID==b.ArgumentID
                                    })
                                       .Where((a, b) => a.OrgID == i_crm.ORIGID && a.ArgumentClassID == "LeaveType" && a.Effective == "Y")
                                       .Select((a, b) => new OTB_SYS_Arguments
                                       {
                                           ArgumentID = SqlFunc.GetSelfAndAutoFill(a.ArgumentID),
                                           ExFeild1 = b.Field1,
                                           ExFeild2 = b.Field2,
                                           ExFeild3 = b.Field3,
                                           ExFeild4 = b.Field4
                                       })
                                       .OrderBy(a => a.OrderByValue)
                                       .ToList();
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, saLeaveRuelsSetting);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(LeaveSetService), @"出勤設定", @"GetLeaveSetting（獲取請假規則設定）", @"", @"", @"");
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

        #endregion 獲取請假規則設定

        #region 請假規則設定（更新）

        /// <summary>
        /// 請假規則設定（更新）
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on UpdImportCustomers</param>
        /// <returns></returns>
        public ResponseMessage UpdateLeaveSetting(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            try
            {
                rm = SugarBase.ExecTran(db =>
                {
                    do
                    {
                        var oEntity = _fetchEntity<OTB_SYS_Arguments>(i_crm);
                        _setEntityBase(oEntity, i_crm);

                        var oArgumentsRelated = new OTB_SYS_ArgumentsRelated
                        {
                            OrgID = oEntity.OrgID,
                            ArgumentClassID = oEntity.ArgumentClassID,
                            ArgumentID = oEntity.ArgumentID,
                            Field1 = oEntity.ExFeild1,
                            Field2 = oEntity.ExFeild2,
                            Field3 = oEntity.ExFeild3,
                            Field4 = oEntity.ExFeild4,
                        };
                        _setEntityBase(oArgumentsRelated, i_crm);

                        var iRelUp = db.Updateable(oEntity)
                            .UpdateColumns(x => new
                            {
                                x.Memo,
                                x.Correlation,
                                x.ModifyUser,
                                x.ModifyDate
                            }).ExecuteCommand();

                        var bExsit = db.Queryable<OTB_SYS_ArgumentsRelated>()
                                       .Where(x => x.OrgID == i_crm.ORIGID && x.ArgumentClassID == oArgumentsRelated.ArgumentClassID && x.ArgumentID == oArgumentsRelated.ArgumentID)
                                       .Any();

                        if (bExsit)
                        {
                            iRelUp += db.Updateable(oArgumentsRelated)
                                .UpdateColumns(x => new
                                {
                                    x.Field1,
                                    x.Field2,
                                    x.Field3,
                                    x.Field4,
                                    x.ModifyUser,
                                    x.ModifyDate
                                }).ExecuteCommand();
                        }
                        else
                        {
                            iRelUp += db.Insertable(oArgumentsRelated).ExecuteCommand();
                        }

                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, iRelUp);
                    } while (false);

                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(LeaveSetService), @"出勤設定", @"UpdateLeaveSetting（請假規則設定（更新））", @"", @"", @"");
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

        #endregion 請假規則設定（更新）

        #region 依據請假類別獲取請假規則設定

        /// <summary>
        /// 依據請假類別獲取請假規則設定
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage GetLeaveSettingByType(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.DB;
            try
            {
                do
                {
                    var sLeaveType = _fetchString(i_crm, @"LeaveType");
                    var oLeaveRuelsSetting = db.Queryable<OTB_SYS_Arguments, OTB_SYS_ArgumentsRelated>((a, b) =>
                                    new object[] {
                                              JoinType.Left,a.OrgID==b.OrgID && a.ArgumentClassID==b.ArgumentClassID && a.ArgumentID==b.ArgumentID
                                    })
                                       .Where((a, b) => a.OrgID == i_crm.ORIGID && a.ArgumentClassID == "LeaveType" && a.ArgumentID == sLeaveType && a.Effective == "Y")
                                       .Select((a, b) => new OTB_SYS_Arguments
                                       {
                                           ArgumentID = SqlFunc.GetSelfAndAutoFill(a.ArgumentID),
                                           ExFeild1 = b.Field1,
                                           ExFeild2 = b.Field2,
                                           ExFeild3 = b.Field3,
                                           ExFeild4 = b.Field4
                                       })
                                       .OrderBy(a => a.OrderByValue)
                                       .Single();
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, oLeaveRuelsSetting);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(LeaveSetService), @"出勤設定", @"GetLeaveSettingByType（依據請假類別獲取請假規則設定）", @"", @"", @"");
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

        #endregion 依據請假類別獲取請假規則設定
    }
}