using EasyBL.WebApi.Message;
using Entity.Sugar;
using SqlSugar;
using SqlSugar.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EasyBL.WEBAPP.SYS
{
    public class Language_SetService : ServiceBase
    {
        #region 多語系管理（分頁查詢）

        /// <summary>
        /// 多語系管理（分頁查詢）
        /// </summary>
        /// <param name="i_crm"></param>
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
                    var pml = new PageModel
                    {
                        PageIndex = _fetchInt(i_crm, @"pageIndex"),
                        PageSize = _fetchInt(i_crm, @"pageSize")
                    };
                    var iPageCount = 0;
                    var sSortField = _fetchString(i_crm, @"sortField");
                    var sSortOrder = _fetchString(i_crm, @"sortOrder");

                    var sLanguageType = _fetchString(i_crm, @"LanguageType");
                    var sCountry = _fetchString(i_crm, @"Country");
                    var sLanguageId = _fetchString(i_crm, @"LanguageId");
                    var sLanguageName = _fetchString(i_crm, @"LanguageName");
                    var bExcel = _fetchBool(i_crm, @"Excel");

                    pml.DataList = db.Queryable<OTB_SYS_Language>()
                        .Where(x => x.OrgID == i_crm.ORIGID && x.LangId.Contains(sLanguageId) && x.LangName.Contains(sLanguageName))
                        .WhereIF(!string.IsNullOrEmpty(sLanguageType), x => x.Type == sLanguageType)
                        .WhereIF(!string.IsNullOrEmpty(sCountry), x => x.Country == sCountry)
                        .OrderBy(sSortField, sSortOrder)
                        .ToPageList(pml.PageIndex, bExcel ? 100000 : pml.PageSize, ref iPageCount);
                    pml.Total = iPageCount;

                    rm = new SuccessResponseMessage(null, i_crm);
                    if (bExcel)
                    {
                    }
                    else
                    {
                        rm.DATA.Add(BLWording.REL, pml);
                    }
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.WEBAPP.SYS.ArgumentClassMaintain_QryService", "", "QueryPage（多語系管理（分頁查詢））", "", "", "");
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

        #endregion 多語系管理（分頁查詢）

        #region 多語系管理（新增）

        /// <summary>
        /// 多語系管理（新增）
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on UpdImportCustomers</param>
        /// <returns></returns>
        public ResponseMessage GridInsert(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            try
            {
                rm = SugarBase.ExecTran(db =>
                {
                    do
                    {
                        var oEntity = _fetchEntity<OTB_SYS_Language>(i_crm);

                        if (db.Queryable<OTB_SYS_Language>().Any(x => x.OrgID == i_crm.ORIGID && x.Country == oEntity.Country && x.Type == oEntity.Type && x.LangId == oEntity.LangId))
                        {
                            sMsg = @"該語系ID已存在！";
                            break;
                        }
                        _setEntityBase(oEntity, i_crm);
                        var iRel = db.Insertable(oEntity).ExecuteCommand();
                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, iRel);
                    } while (false);

                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Language_SetService), @"多語系管理", @"GridInsert（多語系管理（新增））", @"", @"", @"");
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

        #endregion 多語系管理（新增）

        #region 多語系管理（修改）

        /// <summary>
        /// 多語系管理（修改）
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on UpdImportCustomers</param>
        /// <returns></returns>
        public ResponseMessage GridUpdate(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            try
            {
                rm = SugarBase.ExecTran(db =>
                {
                    do
                    {
                        var oNewEntity = _fetchEntity<OTB_SYS_Language>(i_crm);
                        _setEntityBase(oNewEntity, i_crm);
                        var iRel = db.Updateable(oNewEntity)
                            .IgnoreColumns(x => new
                            {
                                x.NO,
                                x.CreateUser,
                                x.CreateDate
                            }).ExecuteCommand();
                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, iRel);
                    } while (false);

                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Language_SetService), @"多語系管理", @"GridUpdate（多語系管理（修改））", @"", @"", @"");
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

        #endregion 多語系管理（修改）

        #region 多語系管理（刪除）

        /// <summary>
        /// 多語系管理（刪除）
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on UpdImportCustomers</param>
        /// <returns></returns>
        public ResponseMessage GridDelete(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            try
            {
                rm = SugarBase.ExecTran(db =>
                {
                    do
                    {
                        var iNO = _fetchInt(i_crm, @"NO");
                        var iRel = db.Deleteable<OTB_SYS_Language>()
                                     .Where(x => x.OrgID == i_crm.ORIGID && x.NO == iNO).ExecuteCommand();
                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, iRel);
                    } while (false);

                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Language_SetService), @"多語系管理", @"GridDelete（多語系管理（刪除））", @"", @"", @"");
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

        #endregion 多語系管理（刪除）

        #region 查詢系統所有html文件路徑

        /// <summary>
        /// 函式名稱:GetSysHtmlPath
        /// 函式說明:查詢系統所有html文件路徑
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on GetSysHtmlPath</param>
        /// <returns>
        /// 回傳 ENTITYS(Object)：查詢數據（list），總比數，狀態...
        ///</returns>

        public ResponseMessage GetSysHtmlPath(RequestMessage i_crm)
        {
            ResponseMessage crm = null;
            string sMsg = null;
            var GetFilesPath = new List<Dictionary<string, string>>();
            try
            {
                do
                {
                    var sProgramPath = _fetchString(i_crm, BLWording.FILEPATH);

                    // 將虛擬路徑轉為實體路徑
                    sProgramPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, sProgramPath);

                    // 真實邏輯
                    foreach (var fi in Directory.GetFiles(sProgramPath, @"*.html", SearchOption.AllDirectories))
                    {
                        if (fi.ToLower().IndexOf(@"page") > -1)
                        {
                            var dic = new Dictionary<string, string>();
                            var sPath = fi.Replace(AppDomain.CurrentDomain.BaseDirectory, @"\").Replace(@"\", @"/");
                            dic.Add(BLWording.ID, sPath);
                            dic.Add(BLWording.NAME, sPath);
                            GetFilesPath.Add(dic);
                        }
                    }
                    foreach (var fi in Directory.GetFiles(sProgramPath, @"*.aspx", SearchOption.AllDirectories))
                    {
                        if (fi.ToLower().IndexOf(@"page") > -1)
                        {
                            var dic = new Dictionary<string, string>();
                            var sPath = fi.Replace(AppDomain.CurrentDomain.BaseDirectory, @"\").Replace(@"\", @"/");
                            dic.Add(BLWording.ID, sPath);
                            dic.Add(BLWording.NAME, sPath);
                            GetFilesPath.Add(dic);
                        }
                    }

                    crm = new SuccessResponseMessage(null, i_crm);

                    // 填寫回傳
                    crm.DATA.Add(BLWording.REL, GetFilesPath);
                }
                while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Language_SetService), @"多語系設定", @"GetSysHtmlPath（查詢系統所有html文件路徑）", @"", @"", @"");
            }

            if (null != sMsg)
            {
                crm = new ErrorResponseMessage(sMsg, i_crm);
            }

            return crm;
        }

        #endregion 查詢系統所有html文件路徑

        #region 複製語系檔案

        /// <summary>
        /// 函式名稱:CopyLanguage
        /// 函式說明:複製語系檔案
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on CopyLanguage</param>
        /// <returns>
        /// 回傳 rm(Object)
        ///</returns>
        public ResponseMessage CopyLanguage(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var sLangFrom = _fetchString(i_crm, @"LangFrom");
                    var sLangTo = _fetchString(i_crm, @"LangTo");
                    var saLangFrom = db.Queryable<OTB_SYS_Language>().Where(x => x.OrgID == i_crm.ORIGID && x.Country == sLangFrom).ToList();
                    var saLangTo = db.Queryable<OTB_SYS_Language>().Where(x => x.OrgID == i_crm.ORIGID && x.Country == sLangTo).ToList();

                    if (saLangFrom.Count == 0)
                    {
                        sMsg = @"1";
                        break;
                    }

                    var ListAdd = new List<OTB_SYS_Language>();

                    foreach (OTB_SYS_Language oLanguage in saLangFrom)
                    {
                        if (!saLangTo.Any(p => (p.LangId == oLanguage.LangId && p.Type == oLanguage.Type)))
                        {
                            ListAdd.Add(oLanguage);
                        }
                    }
                    if (ListAdd.Count > 0)
                    {
                        foreach (OTB_SYS_Language oLanguage in ListAdd)
                        {
                            oLanguage.Country = sLangTo;
                            oLanguage.Memo = oLanguage.LangName;
                            oLanguage.LangName = sLangTo == @"zh" ? ChineseStringUtility.ToSimplified(oLanguage.LangName) : @"";
                            oLanguage.CreateUser = i_crm.USERID;
                            oLanguage.CreateDate = DateTime.Now;
                            oLanguage.ModifyUser = i_crm.USERID;
                            oLanguage.ModifyDate = DateTime.Now;
                        }

                        var iRel = db.Insertable(ListAdd).ExecuteCommand();

                        if (iRel == 0)   // 複製失敗
                        {
                            sMsg = @"0";
                            break;
                        }
                    }
                    rm = new SuccessResponseMessage(null, i_crm);
                    //rm.DATA.Add(BLWording.REL, bSend);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Language_SetService), @"多語系設定", @"CopyLanguage（複製語系檔案）", @"", @"", @"");
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

        #endregion 複製語系檔案
    }
}