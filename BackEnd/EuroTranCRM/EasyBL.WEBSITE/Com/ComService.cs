using EasyBL.WebApi.Message;
using Entity.Sugar;
using Entity.ViewModels;
using SqlSugar;
using SqlSugar.Base;
using System;
using System.Collections.Generic;
using System.Web;

namespace EasyBL.WEBSITE.Com
{
    public class ComService : ServiceBase
    {
        #region 獲取組織信息

        /// <summary>
        /// 獲取組織信息
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage GetOrgInfo(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.DB;
            try
            {
                do
                {
                    var sdb = new SimpleClient<OTB_SYS_Organization>(db);
                    var oOrg = sdb.GetById(i_crm.ORIGID);
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, oOrg);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.WEBSITE.Com.ComService", "", "GetOrgInfo（獲取組織信息）", "", "", "");
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

        #endregion 獲取組織信息

        #region 官網設定（分頁查詢）

        /// <summary>
        /// 官網設定（分頁查詢）
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage GetWebSiteSettingPage(RequestMessage i_crm)
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

                    var sSetType = _fetchString(i_crm, @"SetType");
                    var sLangId = _fetchString(i_crm, @"LangId");
                    var sParentId = _fetchString(i_crm, @"ParentId");
                    var bOnlyParent = _fetchBool(i_crm, @"OnlyParent");

                    pml.DataList = db.Queryable<OTB_WSM_WebSiteSetting, OTB_SYS_Files, OTB_SYS_Files, OTB_SYS_Files>((t1, t2, t3, t4) =>
                        new object[] {
                                JoinType.Left, t1.OrgID == t2.OrgID && t1.IconId == t2.ParentID,
                                JoinType.Left, t1.OrgID == t3.OrgID && t1.SubIconId == t3.ParentID,
                                JoinType.Left, t1.OrgID == t4.OrgID && t1.CoverId == t4.ParentID
                              })
                         .Where((t1, t2, t3, t4) => t1.OrgID == i_crm.ORIGID && t1.SetType == sSetType && t1.LangId == i_crm.LANG && t1.Active)
                         .WhereIF(!string.IsNullOrEmpty(sParentId), (t1, t2, t3, t4) => t1.ParentId == sParentId)
                         .WhereIF(bOnlyParent, (t1, t2, t3, t4) => !SqlFunc.HasValue(t1.ParentId))
                         .Select((t1, t2, t3, t4) => new View_WSM_WebSiteSetting
                         {
                             Guid = SqlFunc.GetSelfAndAutoFill(t1.Guid),
                             IconFileName = t2.FileName,
                             IconFilePath = t2.FilePath,
                             SubIconFileName = t3.FileName,
                             SubIconFilePath = t3.FilePath,
                             CoverFileName = t4.FileName,
                             CoverPath = t4.FilePath
                         })
                         .OrderBy("t1.ParentId,t1.OrderByValue", "asc")
                         .ToPageList(pml.PageIndex, pml.PageSize, ref iPageCount);
                    pml.Total = iPageCount;

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, pml);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.WEBSITE.Com.ComService", "", "GetWebSiteSettingPage（官網設定（分頁查詢））", "", "", "");
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

        #endregion 官網設定（分頁查詢）

        #region 官網設定（多筆）

        /// <summary>
        /// 官網設定（多筆）
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage GetWebSiteSetting(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var sSetType = _fetchString(i_crm, @"SetType");
                    var sLangId = _fetchString(i_crm, @"LangId");
                    var sParentId = _fetchString(i_crm, @"ParentId");
                    var bHasChild = _fetchBool(i_crm, @"HasChild");
                    var bSingle = _fetchBool(i_crm, @"Single");

                    var saWebSiteSetting = db.Queryable<OTB_WSM_WebSiteSetting, OTB_SYS_Files, OTB_SYS_Files, OTB_SYS_Files>((t1, t2, t3, t4) =>
                        new object[] {
                                JoinType.Left, t1.OrgID == t2.OrgID && t1.IconId == t2.ParentID,
                                JoinType.Left, t1.OrgID == t3.OrgID && t1.SubIconId == t3.ParentID,
                                JoinType.Left, t1.OrgID == t4.OrgID && t1.CoverId == t4.ParentID
                              })
                         .Where((t1, t2, t3, t4) => t1.OrgID == i_crm.ORIGID && t1.SetType == sSetType && t1.LangId == sLangId && t1.Active)
                         .WhereIF(!bSingle && !string.IsNullOrEmpty(sParentId), (t1, t2, t3, t4) => t1.ParentId == sParentId)
                         .WhereIF(!bSingle && string.IsNullOrEmpty(sParentId), (t1, t2, t3, t4) => !SqlFunc.HasValue(t1.ParentId))
                         .WhereIF(bSingle, (t1, t2, t3, t4) => t1.Guid == sParentId)
                         .Select((t1, t2, t3, t4) => new View_WSM_WebSiteSetting
                         {
                             Guid = SqlFunc.GetSelfAndAutoFill(t1.Guid),
                             IconFileName = t2.FileName,
                             IconFilePath = t2.FilePath,
                             SubIconFileName = t3.FileName,
                             SubIconFilePath = t3.FilePath,
                             CoverFileName = t4.FileName,
                             CoverPath = t4.FilePath
                         })
                         .OrderBy("t1.ParentId,t1.OrderByValue", "asc")
                         .ToList();

                    if (bHasChild)
                    {
                        var saWebSiteSetting_Child = db.Queryable<OTB_WSM_WebSiteSetting, OTB_SYS_Files, OTB_SYS_Files, OTB_SYS_Files>((t1, t2, t3, t4) =>
                            new object[] {
                                JoinType.Left, t1.OrgID == t2.OrgID && t1.IconId == t2.ParentID,
                                JoinType.Left, t1.OrgID == t3.OrgID && t1.SubIconId == t3.ParentID,
                                JoinType.Left, t1.OrgID == t4.OrgID && t1.CoverId == t4.ParentID
                                  })
                             .Where((t1, t2, t3, t4) => t1.OrgID == i_crm.ORIGID && t1.SetType == sSetType && t1.LangId == sLangId && t1.Active && SqlFunc.HasValue(t1.ParentId))
                             .Select((t1, t2, t3, t4) => new View_WSM_WebSiteSetting
                             {
                                 Guid = SqlFunc.GetSelfAndAutoFill(t1.Guid),
                                 IconFileName = t2.FileName,
                                 IconFilePath = t2.FilePath,
                                 SubIconFileName = t3.FileName,
                                 SubIconFilePath = t3.FilePath,
                                 CoverFileName = t4.FileName,
                                 CoverPath = t4.FilePath
                             })
                             .OrderBy(t1 => t1.OrderByValue)
                             .ToList();

                        foreach (var setting in saWebSiteSetting)
                        {
                            setting.Infos = saWebSiteSetting_Child.FindAll(x => x.ParentId == setting.Guid);
                        }
                    }

                    rm = new SuccessResponseMessage(null, i_crm);
                    if (bSingle)
                    {
                        rm.DATA.Add(BLWording.REL, saWebSiteSetting[0]);
                    }
                    else
                    {
                        rm.DATA.Add(BLWording.REL, saWebSiteSetting);
                    }
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.WEBSITE.Com.ComService", "", "GetWebSiteSetting（官網設定（多筆））", "", "", "");
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

        #endregion 官網設定（多筆）

        #region 獲取最新消息分頁資料

        /// <summary>
        /// 獲取最新消息分頁資料
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage GetNewsPage(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.DB;
            try
            {
                do
                {
                    var pml = new PageModel
                    {
                        PageIndex = _fetchInt(i_crm, "pageIndex"),
                        PageSize = _fetchInt(i_crm, "pageSize")
                    };
                    var iPageCount = 0;
                    var sNewsType = _fetchString(i_crm, "NewsType");

                    pml.DataList = db.Queryable<OTB_WSM_News, OTB_SYS_Files>((t1, t2) =>
                    new object[] {
                          JoinType.Left,t1.OrgID==t2.OrgID && t1.News_Pic==t2.ParentID
                                 })
                        .OrderBy((t1) => t1.OrderByValue)
                        .Where((t1) => t1.OrgID == i_crm.ORIGID && t1.News_Type == sNewsType && t1.News_Show == "Y" && t1.News_LanguageType == i_crm.LANG)
                        .Select((t1, t2) => new { t1.SN, t1.News_Title, t1.CreateDate, t1.NewsContent, News_PicPath = t2.FilePath })
                        .ToPageList(pml.PageIndex, pml.PageSize, ref iPageCount);
                    pml.Total = iPageCount;

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, pml);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.WEBSITE.Com.ComService", "", "GetNewsPage（獲取最新消息分頁資料）", "", "", "");
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

        #endregion 獲取最新消息分頁資料

        #region 獲取展覽資訊分頁資料

        /// <summary>
        /// 獲取展覽資訊分頁資料
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage GetExhibitionPage(RequestMessage i_crm)
         {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.DB;
            try
            {
                do
                {
                    var pml = new PageModel
                    {
                        PageIndex = _fetchInt(i_crm, "pageIndex"),
                        PageSize = _fetchInt(i_crm, "pageSize")
                    };
                    var iPageCount = 0;
                    var sIsShowWebSim = _fetchString(i_crm, "IsShowWebSim");
                    var bTop = _fetchBool(i_crm, "Top");
                    var sKeyWords = _fetchString(i_crm, "KeyWords");
                    var sArea = _fetchString(i_crm, "Area");
                    var sDateStart = _fetchString(i_crm, "DateStart");
                    var sDateEnd = _fetchString(i_crm, "DateEnd");
                    var sNow = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
                    var sCategory = _fetchString(i_crm, "Category");
                    var sCode = _fetchString(i_crm, "Code");

                    pml.DataList = db.Queryable<OTB_OPM_Exhibition, OTB_SYS_Files, OTB_SYS_Arguments, OTB_SYS_Arguments>((t1, t2, t3, t4) => new object[] {
                          JoinType.Left,t1.OrgID == t2.OrgID && t1.LogoFileId == t2.ParentID,
                          JoinType.Left,t1.OrgID == t3.OrgID && t1.ExhibitionAddress == t3.ArgumentID && t3.ArgumentClassID == "Area" && t3.LevelOfArgument == 2,
                          JoinType.Left,t1.OrgID == t4.OrgID && t1.State == t4.ArgumentID && t4.ArgumentClassID == "Area" && t4.LevelOfArgument == 1})
                        .OrderBy((t1, t2) => t1.ExhibitionDateStart, OrderByType.Asc)
                        .Where((t1, t2) => t1.OrgID == i_crm.ORIGID && t1.IsShowWebSite == "Y" && t1.Effective == "Y")
                        //.WhereIF(!string.IsNullOrEmpty(sIsShowWebSim), (t1) => t1.OrgID == i_crm.ORIGID && t1.IsShowWebSim == "Y")
                        .WhereIF(bTop, (t1) => t1.ExhibitionDateEnd >= DateTime.Now.Date)
                        .WhereIF(!string.IsNullOrEmpty(sKeyWords), (t1) => (t1.Exhibitioname_TW.Contains(sKeyWords) || t1.Exhibitioname_EN.Contains(sKeyWords)))
                        .WhereIF(!string.IsNullOrEmpty(sArea), (t1) => sArea.Contains(t1.State))
                        .WhereIF(!string.IsNullOrEmpty(sDateStart) && string.IsNullOrEmpty(sDateEnd), (t1) => t1.ExhibitionDateEnd >= SqlFunc.ToDate(sDateStart))
                        .WhereIF(!string.IsNullOrEmpty(sDateEnd) && string.IsNullOrEmpty(sDateStart), (t1) => t1.ExhibitionDateStart <= SqlFunc.ToDate(sDateEnd))
                        .WhereIF(!string.IsNullOrEmpty(sDateStart) && !string.IsNullOrEmpty(sDateEnd), (t1) => t1.ExhibitionDateStart <= SqlFunc.ToDate(sDateEnd) && t1.ExhibitionDateEnd >= SqlFunc.ToDate(sDateStart))
                        .WhereIF(!string.IsNullOrEmpty(sCategory), (t1) => sCategory.Contains(t1.Industry))
                        .WhereIF(!string.IsNullOrEmpty(sCode), (t1) => t1.ExhibitionCode == sCode)
                        .Select((t1, t2, t3, t4) => new
                        {
                            t1.SN,
                            t1.State,
                            t1.ExhibitionAddress,
                            t1.Exhibitioname_TW,
                            t1.Exhibitioname_EN,
                            t1.ExhibitionDateStart,
                            t1.ExhibitionDateEnd,
                            ExhibitionAddressName = t3.ArgumentValue,
                            StateName = t4.ArgumentValue,
                            ExhibitionAddressName_EN = t3.ArgumentValue_EN,
                            StateName_EN = t4.ArgumentValue_EN,
                            LogoFilePath = t2.FilePath,
                            t1.SeaReceiveingDate,
                            t1.SeaClosingDate,
                            t1.AirReceiveingDate,
                            t1.AirClosingDate,
                            t1.Undertaker,
                            t1.Telephone,
                            t1.Email,
                            t1.WebsiteAdress
                        }).ToPageList(pml.PageIndex, pml.PageSize, ref iPageCount);
                    pml.Total = iPageCount;

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, pml);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.WEBSITE.Com.ComService", "", "GetExhibitionPage（獲取展覽資訊分頁資料）", "", "", "");
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

        #endregion 獲取展覽資訊分頁資料

        #region 獲取最新消息分明細

        /// <summary>
        /// 獲取最新消息分明細
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage GetNewsInfo(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var iId = _fetchInt(i_crm, "Id");
                    var bIncludeFiles = _fetchBool(i_crm, "IncludeFiles");
                    var saNews = db.Queryable<OTB_WSM_News>().Where(x => x.OrgID == i_crm.ORIGID && x.SN == iId).ToList();
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, saNews);
                    if (bIncludeFiles && saNews.Count > 0)
                    {
                        var saFiles = db.Queryable<OTB_SYS_Files>().Where(x => x.OrgID == i_crm.ORIGID && x.ParentID == saNews[0].PicShowId).ToList();
                        rm.DATA.Add("files", saFiles);
                    }
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.WEBSITE.Com.ComService", "", "GetNewsInfo（獲取最新消息分明細）", "", "", "");
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

        #endregion 獲取最新消息分明細

        #region 客戶寄送郵件

        /// <summary>
        /// 客戶寄送郵件
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage SendMail(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var bSend = false;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var sFlag = _fetchString(i_crm, "flag");
                    var sCaptcha = "";
                    if (HttpContext.Current.Session[BLWording.CAPTCHA + sFlag] != null)
                    {
                        sCaptcha = HttpContext.Current.Session[BLWording.CAPTCHA + sFlag].ToString();
                    }

                    var sValidcode = _fetchString(i_crm, "validcode");
                    if (sCaptcha != sValidcode)
                    {
                        sMsg = "驗證碼錯誤";
                        break;
                    }
                    var oWebSiteMailLog = new OTB_WSM_WebSiteMailLog
                    {
                        OrgID = i_crm.ORIGID,
                        Type = _fetchString(i_crm, "type"),
                        Ucomp = _fetchString(i_crm, "ucomp"),
                        Uname = _fetchString(i_crm, "uname"),
                        Utel = _fetchString(i_crm, "utel"),
                        Uemail = _fetchString(i_crm, "uemail"),
                        Title = _fetchString(i_crm, "utitle"),
                        Content = _fetchString(i_crm, "umailcontent"),
                        CreateDate = DateTime.Now,
                        ModifyDate = DateTime.Now
                    };

                    var sCusCommentsEmail = Common.GetSystemSetting(db, i_crm.ORIGID, "CusCommentsEmail");
                    //獲取Email郵件格式
                    var oEmailTempl = db.Queryable<OTB_SYS_Email>().Single(it => it.OrgID == i_crm.ORIGID && it.EmailID == "CusComments");

                    if (oEmailTempl != null)
                    {
                        //寄信開始
                        var sEmailBody = oEmailTempl.BodyHtml.Replace("{{:ucomp}}", oWebSiteMailLog.Ucomp)
                               .Replace("{{:uname}}", oWebSiteMailLog.Uname)
                               .Replace("{{:uemail}}", oWebSiteMailLog.Uemail)
                               .Replace("{{:utel}}", oWebSiteMailLog.Utel)
                               .Replace("{{:utitle}}", oWebSiteMailLog.Title)
                               .Replace("{{:umailcontent}}", oWebSiteMailLog.Content);

                        var oEmail = new Emails();
                        var saEmailTo = new List<EmailTo>();   //收件人
                        var oEmailTo = new EmailTo
                        {
                            ToUserID = sCusCommentsEmail,
                            ToUserName = sCusCommentsEmail,
                            ToEmail = sCusCommentsEmail,
                            Type = "to"
                        };
                        saEmailTo.Add(oEmailTo);

                        oEmail.FromUserName = "系統郵件";//取fonfig
                        oEmail.Title = oWebSiteMailLog.Type == null ? oEmailTempl.EmailSubject : (oWebSiteMailLog.Type == "C" ? "一般詢問" : "線上詢價");//取fonfig
                        oEmail.EmailBody = sEmailBody;
                        oEmail.IsCCSelf = false;
                        oEmail.Attachments = null;
                        oEmail.EmailTo = saEmailTo;

                        bSend = new MailService(i_crm.ORIGID, true).MailFactory(oEmail, out sMsg);
                        if (bSend || oWebSiteMailLog.Content.IndexOf("***TEST***") > -1)
                        {
                            db.Insertable(oWebSiteMailLog).ExecuteCommand();
                        }
                    }
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, bSend);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.WebSite.TE.ComService", "客戶寄送郵件", nameof(SendMail), "", "", "");
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

        #endregion 客戶寄送郵件

        #region 依據展覽獲取展覽報價規則

        /// <summary>
        /// 依據展覽獲取展覽報價規則
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage GetExhibitionRules(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var iId = _fetchInt(i_crm, "Id");

                    var oRules = db.Queryable<OTB_WSM_ExhibitionRules, OTB_OPM_Exhibition, OTB_SYS_Arguments>((t1, t2, t3) => t1.OrgID == t2.OrgID && t1.Guid == t2.CostRulesId && t1.OrgID == t3.OrgID && t1.Currency == t3.ArgumentID && t3.ArgumentClassID == "Currency")
                        .Where((t1, t2) => t2.SN == iId)
                        .Select((t1, t2, t3) => new View_WSM_ExhibitionRules
                        {
                            Guid = t1.Guid,
                            FileId_EN = t1.FileId_EN,
                            Title = t1.Title,
                            CostRules = t1.CostRules,
                            PackingPrice = t1.PackingPrice,
                            FeedingPrice = t1.FeedingPrice,
                            StoragePrice = t1.StoragePrice,
                            FeedingRequiredMinCBM = t1.FeedingRequiredMinCBM,
                            FeedingMinMode = t1.FeedingMinMode,
                            PackingRequiredMinCBM = t1.PackingRequiredMinCBM,
                            PackingMinMode = t1.PackingMinMode,
                            CostInstruction = t1.CostInstruction,
                            CostInstruction_EN = t1.CostInstruction_EN,
                            IsMerge = t1.IsMerge,
                            Memo = t1.Memo,
                            ServiceInstruction = t1.ServiceInstruction,
                            ServiceInstruction_EN = t1.ServiceInstruction_EN,
                            Currency = t1.Currency,
                            CurrencyName = t3.ArgumentValue,
                            CurrencyName_EN = t3.ArgumentValue_EN,
                        }).Single();
                    var saFiles = new List<OTB_SYS_Files>();

                    if (i_crm.LANG == "en")
                    {
                        saFiles = db.Queryable<OTB_SYS_Files>().OrderBy(x => x.OrderByValue).Where(x => x.ParentID == oRules.FileId_EN).ToList();
                    }
                    else
                    {
                        saFiles = db.Queryable<OTB_SYS_Files>().OrderBy(x => x.OrderByValue).Where(x => x.ParentID == oRules.Guid).ToList();
                    }
                    oRules.Files = saFiles;
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, oRules);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.WEBSITE.Com.ComService", "", "GetExhibitionRules（依據展覽獲取展覽報價規則）", "", "", "");
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

        #endregion 依據展覽獲取展覽報價規則

        #region 獲取預約服務展覽資訊

        /// <summary>
        /// 獲取預約服務展覽資訊
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage GetExhibitionAppoint(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.DB;
            try
            {
                do
                {
                    var pml = new PageModel
                    {
                        PageIndex = _fetchInt(i_crm, "pageIndex"),
                        PageSize = _fetchInt(i_crm, "pageSize")
                    };
                    var iPageCount = 0;
                    var sIsShowWebSim = _fetchString(i_crm, "IsShowWebSim");
                    var bTop = _fetchBool(i_crm, "Top");
                    var sKeyWords = _fetchString(i_crm, "KeyWords");
                    var sArea = _fetchString(i_crm, "Area");
                    var sDateStart = _fetchString(i_crm, "DateStart");
                    var sDateEnd = _fetchString(i_crm, "DateEnd");
                    var sNow = DateTime.Now.ToString("yyyy-MM-dd HH:mm");

                    pml.DataList = db.Queryable<OTB_OPM_Exhibition>()
                        .OrderBy(t1 => t1.ExhibitionDateStart, OrderByType.Asc)
                        .Where(t1 => t1.OrgID == i_crm.ORIGID && t1.IsShowWebSiteAppoint == "Y" && t1.Effective == "Y")
                        //.WhereIF(!string.IsNullOrEmpty(sIsShowWebSim), (t1) => t1.OrgID == i_crm.ORIGID && t1.IsShowWebSim == "Y")
                        .WhereIF(bTop, (t1) => t1.ExhibitionDateEnd >= DateTime.Now.Date)
                        .WhereIF(!string.IsNullOrEmpty(sKeyWords), (t1) => (t1.Exhibitioname_TW.Contains(sKeyWords) || t1.Exhibitioname_EN.Contains(sKeyWords)))
                        .WhereIF(!string.IsNullOrEmpty(sArea), (t1) => sArea.Contains(t1.State))
                        .WhereIF(!string.IsNullOrEmpty(sDateStart) && string.IsNullOrEmpty(sDateEnd), (t1) => t1.ExhibitionDateEnd >= SqlFunc.ToDate(sDateStart))
                        .WhereIF(!string.IsNullOrEmpty(sDateEnd) && string.IsNullOrEmpty(sDateStart), (t1) => t1.ExhibitionDateStart <= SqlFunc.ToDate(sDateEnd))
                        .WhereIF(!string.IsNullOrEmpty(sDateStart) && !string.IsNullOrEmpty(sDateEnd), (t1) => t1.ExhibitionDateStart <= SqlFunc.ToDate(sDateEnd) && t1.ExhibitionDateEnd >= SqlFunc.ToDate(sDateStart))
                        .Select(t1 => new
                        {
                            t1.SN,
                            t1.Exhibitioname_TW,
                            t1.Exhibitioname_EN
                        }).ToPageList(pml.PageIndex, pml.PageSize, ref iPageCount);
                    pml.Total = iPageCount;

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, pml);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.WEBSITE.Com.ComService", "", "GetExhibitionAppoint（獲取預約服務展覽資訊）", "", "", "");
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

        #endregion 獲取預約服務展覽資訊
    }
}