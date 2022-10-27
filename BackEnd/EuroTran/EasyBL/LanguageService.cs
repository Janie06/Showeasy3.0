using EasyBL.WebApi.Message;
using Entity.Sugar;
using HtmlAgilityPack;
using Newtonsoft.Json;
using SqlSugar.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace EasyBL
{
    public class LanguageService : ServiceBase
    {
        #region 獲取資料產生語系檔Json文件

        //產生語系檔
        public ResponseMessage CreateLangJson(RequestMessage i_crm)
        {
            ResponseMessage crm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            var DicLangData = new Dictionary<string, Dictionary<string, string>>();

            try
            {
                do
                {
                    var saLanguage = db.Queryable<OTB_SYS_Language>().Where(x => x.OrgID == i_crm.ORIGID).ToList();

                    // 分語言 oLanguageSource
                    foreach (OTB_SYS_Language oLanguageSource in saLanguage)
                    {
                        if (!DicLangData.ContainsKey(oLanguageSource.Country))
                        {
                            DicLangData.Add(oLanguageSource.Country, new Dictionary<string, string>());
                        }

                        if (!DicLangData[oLanguageSource.Country].ContainsKey(oLanguageSource.Type + "." + oLanguageSource.LangId))
                        {
                            DicLangData[oLanguageSource.Country].Add(oLanguageSource.Type + "." + oLanguageSource.LangId, oLanguageSource.LangName);
                        }
                    }

                    var dicLangResult = new Dictionary<string, string>();

                    foreach (string sLang in DicLangData.Keys)
                    {
                        // Make Json
                        var sOut = MakeJson(DicLangData[sLang], out sMsg);
                        if (sMsg != null)
                        {
                            break;
                        }
                        if (!dicLangResult.ContainsKey(sLang))
                        {
                            dicLangResult.Add(sLang, sOut);
                        }
                    }

                    if (sMsg != null)
                    {
                        break;
                    }

                    foreach (string sLang in dicLangResult.Keys)
                    {
                        var sFoder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scripts", "lang", i_crm.ORIGID);
                        Common.FnCreateDir(sFoder);
                        var sLangPath = Path.Combine(sFoder, $"{sLang}.json");
                        File.WriteAllText(sLangPath, dicLangResult[sLang], Encoding.UTF8);
                    }
                    crm = new SuccessResponseMessage(null, i_crm);
                }
                while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.LanguageService", "", "CreateLangJson（獲取資料產生語系檔Json文件）", "", "", "");
            }

            if (null != sMsg)
            {
                crm = new ErrorResponseMessage(sMsg, i_crm);
            }
            return crm;
        }

        #endregion 獲取資料產生語系檔Json文件

        #region 多語系初始化（依據文件）

        //多語系初始化（依據文件）
        public ResponseMessage InitializeLanguage(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            try
            {
                do
                {
                    rm = SugarBase.ExecTran(db =>
                    {
                        do
                        {
                            var dicI18ns = new Dictionary<string, string>();
                            var saFilesPath = new List<string>();
                            var sInitLngHTMLPath = Common.ConfigGetValue("", "InitLngHTMLPath");
                            var sInitLngJSPath = Common.ConfigGetValue("", "InitLngJSPath");

                            // 將虛擬路徑轉為實體路徑
                            sInitLngHTMLPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, sInitLngHTMLPath);
                            sInitLngJSPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, sInitLngJSPath);

                            foreach (var fi in Directory.GetFiles(sInitLngHTMLPath, "*.html", SearchOption.AllDirectories))
                            {
                                saFilesPath.Add(fi);
                            }
                            foreach (var fi in Directory.GetFiles(sInitLngJSPath, "*.js", SearchOption.AllDirectories))
                            {
                                saFilesPath.Add(fi);
                            }

                            foreach (string path in saFilesPath)
                            {
                                if (path.ToLower().EndsWith(".html"))
                                {
                                    var doc = new HtmlDocument();
                                    doc.Load(path, Encoding.UTF8);
                                    var allHn = GetI18nNode(doc);
                                    var templHn = doc.DocumentNode.Descendants().Where(x => x.Name == "script" && x.Attributes.Any(p => p.Value == "text/x-jsrender"));

                                    GetI18nKeyValue(allHn, dicI18ns);

                                    foreach (var nd in templHn)//處理html模版中的多語系
                                    {
                                        var docTempl = new HtmlDocument();
                                        docTempl.LoadHtml(nd.InnerHtml);
                                        var allTemplHn = GetI18nNode(docTempl);
                                        GetI18nKeyValue(allTemplHn, dicI18ns);
                                    }
                                }
                                else
                                {
                                    var sJSText = File.ReadAllText(path, Encoding.UTF8);
                                    var r = new Regex("╠"); // 定义一个Regex对象实例
                                    var mc = r.Matches(sJSText); // 在字符串中匹配
                                    foreach (Match m in mc)
                                    {
                                        var iIndex = m.Index;
                                        var sCur = sJSText.Substring(iIndex, sJSText.Length - iIndex);
                                        var _r = new Regex("╣");
                                        var _m = _r.Match(sCur); // 在字符串中第一個匹配項
                                        var sI18nArray = sCur.Substring(1, _m.Index - 1);
                                        if (sI18nArray.IndexOf("⇒") > -1)//如果不含⇒的就不添加
                                        {
                                            var saI18nText = sI18nArray.Split('⇒');
                                            if (saI18nText.Count() > 1 && !dicI18ns.Keys.Contains(saI18nText[0].ToString()))
                                            {
                                                dicI18ns.Add(saI18nText[0].ToString(), saI18nText[1].ToString());
                                            }
                                        }
                                    }
                                }
                            }

                            if (dicI18ns.Keys.Count > 0)
                            {
                                //List<OTB_SYS_Language> saLng_Del = new List<OTB_SYS_Language>();
                                var saLng_Add = new List<OTB_SYS_Language>();
                                var saLanguage = db.Queryable<OTB_SYS_Language>().Where(x => x.OrgID == i_crm.ORIGID).ToList();
                                var saModuleList = db.Queryable<OTB_SYS_ModuleList>().Where(x => x.OrgID == i_crm.ORIGID).ToList();
                                var saPrgList = db.Queryable<OTB_SYS_ProgramList>().Where(x => x.OrgID == i_crm.ORIGID).ToList();

                                foreach (var i18nkey in dicI18ns)
                                {
                                    if (!saLanguage.Any(x => i18nkey.Key == x.Type + "." + x.LangId))
                                    {
                                        var saLang = i18nkey.Key.Split('.');

                                        if (saLang.Count() > 1)
                                        {
                                            var oLng = new OTB_SYS_Language
                                            {
                                                OrgID = i_crm.ORIGID,
                                                Type = saLang[0].ToString(),
                                                LangId = saLang[1].ToString(),
                                                Country = "zh-TW",
                                                LangName = i18nkey.Value,
                                                Memo = i18nkey.Value,
                                                CreateUser = i_crm.USERID,
                                                CreateDate = DateTime.Now,
                                                ModifyUser = i_crm.USERID,
                                                ModifyDate = DateTime.Now
                                            };
                                            if (!saLng_Add.Any(x => x.OrgID == oLng.OrgID && x.Type == oLng.Type && x.Country == oLng.Country && x.LangId.ToLower() == oLng.LangId.ToLower()))
                                            {
                                                saLng_Add.Add(oLng);
                                            }
                                        }
                                    }
                                }
                                //系統程式多語系
                                foreach (var prg in saPrgList)
                                {
                                    var oLng = new OTB_SYS_Language
                                    {
                                        OrgID = i_crm.ORIGID,
                                        Type = "common",
                                        LangId = prg.ProgramID,
                                        Country = "zh-TW",
                                        LangName = prg.ProgramName,
                                        Memo = prg.ProgramName,
                                        CreateUser = i_crm.USERID,
                                        CreateDate = DateTime.Now,
                                        ModifyUser = i_crm.USERID,
                                        ModifyDate = DateTime.Now
                                    };
                                    if (!saLng_Add.Any(x => x.OrgID == oLng.OrgID && x.Type == oLng.Type && x.Country == oLng.Country && x.LangId.ToLower() == oLng.LangId.ToLower()) && !saLanguage.Any(x => x.OrgID == oLng.OrgID && x.Type == oLng.Type && x.Country == oLng.Country && x.LangId.ToLower() == oLng.LangId.ToLower()))
                                    {
                                        saLng_Add.Add(oLng);
                                    }
                                }
                                //系統模組多語系
                                foreach (var mod in saModuleList)
                                {
                                    var oLng = new OTB_SYS_Language
                                    {
                                        OrgID = i_crm.ORIGID,
                                        Type = "common",
                                        LangId = mod.ModuleID,
                                        Country = "zh-TW",
                                        LangName = mod.ModuleName,
                                        Memo = mod.ModuleName,
                                        CreateUser = i_crm.USERID,
                                        CreateDate = DateTime.Now,
                                        ModifyUser = i_crm.USERID,
                                        ModifyDate = DateTime.Now
                                    };
                                    if (!saLng_Add.Any(x => x.OrgID == oLng.OrgID && x.Type == oLng.Type && x.Country == oLng.Country && x.LangId.ToLower() == oLng.LangId.ToLower()) && !saLanguage.Any(x => x.OrgID == oLng.OrgID && x.Type == oLng.Type && x.Country == oLng.Country && x.LangId.ToLower() == oLng.LangId.ToLower()))
                                    {
                                        saLng_Add.Add(oLng);
                                    }
                                }

                                //if (saLng_Del.Count > 0)
                                //{
                                //    db.Deleteable(saLng_Del).ExecuteCommand();
                                //}
                                if (saLng_Add.Count > 0)
                                {
                                    db.Insertable(saLng_Add).ExecuteCommand();
                                }
                                rm = new SuccessResponseMessage(null, i_crm);
                                rm.DATA.Add(BLWording.REL, true);
                            }
                        } while (false);
                        return rm;
                    });
                }
                while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.LanguageService", "", "InitializeLanguage（多語系初始化（依據文件））", "", "", "");
            }

            if (null != sMsg)
            {
                rm = new ErrorResponseMessage(sMsg, i_crm);
            }
            return rm;
        }

        /// <summary>
        /// 篩選i18n標籤
        /// </summary>
        /// <param name="hd"></param>
        /// <returns></returns>
        private static IEnumerable<HtmlNode> GetI18nNode(HtmlDocument hd)
        {
            var saI18nNode = hd.DocumentNode.Descendants()
                        .Where(x => x.Attributes.Contains("data-i18n")
                                 || x.Attributes.Contains("placeholderid")
                                 || x.Attributes.Any(p => p.Name.StartsWith("data-msg")));
            return saI18nNode;
        }

        /// <summary>
        /// 獲取i18n鍵值對
        /// </summary>
        /// <param name="i18nNode">todo: describe i18nNode parameter on GetI18nKeyValue</param>
        /// <param name="dicI18">todo: describe dicI18 parameter on GetI18nKeyValue</param>
        /// <returns></returns>
        private static void GetI18nKeyValue(IEnumerable<HtmlNode> i18nNode, Dictionary<string, string> dicI18)
        {
            foreach (HtmlNode hn in i18nNode)//回圈所有節點尋找
            {
                var allAttr = hn.Attributes;
                foreach (var attr in allAttr)
                {
                    var sText = "*";
                    if (attr.Name == "data-i18n")
                    {
                        sText = hn.InnerText;
                        if (sText == "" && allAttr["type"] != null && allAttr["type"].Value == "button")
                        {
                            sText = allAttr["value"] == null ? "" : allAttr["value"].Value;
                        }
                    }
                    else if (attr.Name == "placeholderid")
                    {
                        var text = hn.Attributes["placeholder"];
                        sText = text == null ? "" : text.Value;
                    }
                    else if (attr.Name.StartsWith("data-msg"))
                    {
                        var text = hn.Attributes[attr.Name.Replace("-", "")];
                        sText = text == null ? "" : text.Value;
                    }

                    if (sText != "*" && !dicI18.Keys.Contains(attr.Value) && attr.Value.IndexOf("{{:") == -1)
                    {
                        dicI18.Add(attr.Value, sText);
                    }
                }
            }
        }

        #endregion 多語系初始化（依據文件）

        #region 產生Json格式

        //產生語系檔Json格式
        protected static string MakeJson(Dictionary<string, string> i_dicMap, out string o_sError)
        {
            string sErrorMsg = null;
            string sRes = null;
            do
            {
                var dicJSMap = new Dictionary<string, Dictionary<string, string>>();

                foreach (string sKey in i_dicMap.Keys)
                {
                    //oComparison
                    var oComparison = sKey.Split(".".ToCharArray());

                    if (oComparison.Length != 2)
                    {
                        sErrorMsg = "多語系配置格式有無！！";
                        break;
                    }

                    if (!dicJSMap.ContainsKey(oComparison[0]))
                    {
                        dicJSMap.Add(oComparison[0], new Dictionary<string, string>());
                    }

                    if (dicJSMap[oComparison[0]].ContainsKey(oComparison[1]))
                    {
                        sErrorMsg = "多語系ID重複！！";
                        break;
                    }

                    if (!dicJSMap[oComparison[0]].ContainsKey(oComparison[1]))
                    {
                        dicJSMap[oComparison[0]].Add(oComparison[1], i_dicMap[sKey]);
                    }
                }

                if (sErrorMsg != null)
                {
                    break;
                }

                sRes = JsonConvert.SerializeObject(dicJSMap);
            }
            while (false);

            o_sError = sErrorMsg;
            return sRes;
        }

        #endregion 產生Json格式
    }
}