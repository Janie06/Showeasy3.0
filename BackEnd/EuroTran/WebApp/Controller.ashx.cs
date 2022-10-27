using EasyBL;
using EasyBL.WEBAPP;
using Entity.Sugar;
using SqlSugar.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace WebApp
{
    /// <summary>
    /// Controller 的摘要说明
    /// </summary>
    public class Controller : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            //Handler action = null;
            var req = context.Request;
            var sAction = req["action"];
            switch (sAction)
            {
                case "downfile":
                    downFile(context);
                    break;

                case "getimg":
                    GetImg(context);
                    break;

                case "saveimg":
                    SaveImg(context);
                    break;

                case "upload":
                    Upload(context);
                    break;

                case "importfile":
                    PutImportFile(context);
                    break;

                default:
                    break;
            }
            //action.Process();
        }

        #region Upload 上傳文件

        public static void Upload(HttpContext c)
        {
            var req = c.Request;
            var sSource = req["source"];
            var sOrgID = req["orgid"];
            var sUserID = req["userid"];
            var sParentID = req["parentid"];
            var sServerPath = c.Server.MapPath("/");
            var sRoot = sServerPath + "Document\\EurotranFile";
            Common.FnCreateDir(sRoot + "\\" + sSource);//如果沒有該目錄就創建目錄
            if (req.Files.Count > 0)
            {
                var saFilesAdd = new List<OTB_SYS_Files>();
                for (int index = 0; index < req.Files.Count; index++)
                {
                    var file = req.Files[index];
                    var sFileSizeName = "";
                    var sFileID = Guid.NewGuid().ToString();//檔案ID
                    var sFileName = Path.GetFileName(file.FileName);//檔案名稱+文件格式名稱
                    var sFileType = file.ContentType;     //檔案類型
                    var iFileSize = file.ContentLength;      //檔案大小

                    var KBSize = Math.Round((decimal)iFileSize / 1024, 1);//單位KB

                    if (KBSize < 1024)
                    {
                        sFileSizeName = KBSize + "KB";
                    }
                    else
                    {
                        var ComparisonSize = Math.Round((decimal)KBSize / 1024, 1);//單位MB
                        sFileSizeName = ComparisonSize + "MB";
                    }

                    var sfileName = sFileName.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
                    var sSubFileName = sfileName.LastOrDefault();     //副檔名
                    var sNewFileName = sFileID + '.' + sSubFileName;

                    var sOutputPath = sRoot + "/" + sSource + "/" + sNewFileName;
                    sOutputPath = System.Text.RegularExpressions.Regex.Replace(sOutputPath, @"//|/", @"\");
                    file.SaveAs(sOutputPath);

                    var oFile = new OTB_SYS_Files
                    {
                        OrgID = sOrgID,
                        FileID = sFileID,
                        ParentID = sParentID,
                        SourceFrom = sSource,
                        FileName = sFileName,
                        SubFileName = sSubFileName,
                        FilePath = sOutputPath.Replace(sServerPath, ""),
                        FileType = sFileType,
                        FileSize = iFileSize,
                        FileSizeName = sFileSizeName,
                        CreateUser = sUserID,
                        CreateDate = DateTime.Now,
                        ModifyUser = sUserID,
                        ModifyDate = DateTime.Now
                    };
                    saFilesAdd.Add(oFile);
                }
                var db = SugarBase.DB;
                var iRes = db.Insertable(saFilesAdd).ExecuteCommand();
                if (iRes > 0)
                {
                    var rp = c.Response;
                    var sJsonText = ServiceBase.JsonToString(saFilesAdd);
                    rp.Write(sJsonText);
                    rp.End();
                }
            }
        }

        #endregion Upload 上傳文件

        #region downFile 下載文件

        public static void downFile(HttpContext c)
        {
            var req = c.Request;
            var sPath = req["path"];
            var sCusFileName = req["filename"];
            sPath = System.Text.RegularExpressions.Regex.Replace(sPath, @"//|/", @"\");
            using (var webService = new System.Web.Services.WebService())
            {
                var sServerPath = webService.Server.MapPath("/");
                sPath = sPath.Replace(sServerPath, "");
                using (var webService1 = new System.Web.Services.WebService())
                {
                    sPath = webService1.Server.MapPath(sPath);
                    var sFileName = "";
                    var sExtName = "";

                    if (sPath == null || !File.Exists(sPath))
                    {
                        return;
                    }

                    var index = 0;
                    index = sPath.IndexOf("OutFiles");
                    if (index == -1)
                    {
                        index = sPath.IndexOf("EurotranFile");
                    }
                    var sNewPath = sPath.Substring(index);
                    var saPath = sNewPath.Split('.');
                    var saNewPath = saPath[0].Split("\\/".ToCharArray());
                    sExtName = saPath[1];
                    sFileName = sCusFileName ?? saNewPath[saNewPath.Length - 1];

                    c.Response.ContentType = "application/octet-stream";
                    //设置响应的返回类型是文件流
                    c.Response.AddHeader("content-disposition", "attachment;filename=" + sFileName + "." + sExtName);
                    //设置相应的头以及被下载时候显示的文件名
                    var lFileInfo = new FileInfo(sPath);
                    //获取下载文件的文件流
                    c.Response.WriteFile(lFileInfo.FullName);
                    //返回要下载的文件流
                    c.Response.End();
                }
            }
        }

        #endregion downFile 下載文件

        #region GetImg 獲取圖片

        public static void GetImg(HttpContext c)
        {
            var sOrgId = c.Request["orgid"];
            var sId = c.Request["id"];
            var sFolder = c.Request["folder"];
            var sImgsrc = "";
            var sSubFileName = "";
            var rp = c.Response;
            try
            {
                var isValidGuId = Guid.TryParse(sId, out Guid OutPutGuId);
                if (!string.IsNullOrEmpty(sId) && isValidGuId) //不為空
                {
                    var db = SugarBase.GetIntance();
                    var oFile = db.Queryable<OTB_SYS_Files>()
                        .Where(it => it.ParentID == sId)
                        .WhereIF(!string.IsNullOrWhiteSpace(sOrgId), it => it.OrgID == sOrgId).Single();
                    if (oFile != null)
                    {
                        sImgsrc = oFile.FilePath;
                    }

                    if (!string.IsNullOrEmpty(sImgsrc))
                    {
                        //DB內找不到圖片
                        sSubFileName = oFile.SubFileName == null ? "jpeg" : oFile.SubFileName;
                        rp.ContentType = "image/" + sSubFileName;
                    }
                    else
                    {
                        sImgsrc = sFolder == "Members" ? WebAppGlobalConstWord.NOIMAGE : WebAppGlobalConstWord.NOIMG;
                        rp.ContentType = "image/.jpg";
                        rp.WriteFile(sImgsrc);
                    }
                    rp.WriteFile(sImgsrc);
                }
                else
                {
                    sImgsrc = sFolder == "Members" ? WebAppGlobalConstWord.NOIMAGE : WebAppGlobalConstWord.NOIMG;
                    rp.ContentType = "image/.jpg";
                    rp.WriteFile(sImgsrc);
                }
            }
            catch (Exception)
            {
                sImgsrc = sFolder == "Members" ? WebAppGlobalConstWord.NOIMAGE : WebAppGlobalConstWord.NOIMG;
                rp.ContentType = "image/.jpg";
                rp.WriteFile(sImgsrc);
            }
        }

        #endregion GetImg 獲取圖片

        #region SaveImg 儲存圖片

        public static void SaveImg(HttpContext c)
        {
            c.Response.ContentType = "text/plain";
            var strfilepath = HttpUtility.UrlDecode(c.Request.Form["filepath"], System.Text.Encoding.UTF8);
            var phy = HttpUtility.UrlDecode(c.Request.Form["phy"], System.Text.Encoding.UTF8);
            var filename = HttpUtility.UrlDecode(c.Request.Form["filename"], System.Text.Encoding.UTF8);
            var isrepeat = HttpUtility.UrlDecode(c.Request.Form["isrepeat"], System.Text.Encoding.UTF8);
            var user = HttpUtility.UrlDecode(c.Request.Form["user"], System.Text.Encoding.UTF8);
            var oDocument_model = new OTB_SYS_Document();
            var TopPath = "Document";
            var fileroot = "";
            try
            {
                if (phy == "false")
                {
                    //不是實體路徑
                    //取得實體路徑
                    strfilepath = HttpContext.Current.Request.MapPath(strfilepath);
                }

                fileroot = ("\\" + strfilepath.Substring(strfilepath.IndexOf(TopPath))).Replace("\\", "/");
                fileroot = fileroot.Substring(0, 1) != "/" ? "/" + fileroot : fileroot;                   //如果開頭沒有斜線就加上斜線
                fileroot = fileroot.Substring(fileroot.Length - 1) != "/" ? fileroot + "/" : fileroot;  //如果尾端沒有斜線就加上斜線
                var f_name = Path.GetFileNameWithoutExtension(filename);
                var f_extn = Path.GetExtension(filename);
                var gu_id = Guid.NewGuid().ToString();        //新增圖片GUID
                var stringToBase64 = "";                      //儲存縮小圖的base64字串
                oDocument_model.GUID = gu_id;
                oDocument_model.FileName = f_name;
                oDocument_model.SubFileName = f_extn;
                oDocument_model.FileRoot = fileroot;
                oDocument_model.FilePath = fileroot + filename;
                var newfile = new FileInfo(strfilepath + filename);
                oDocument_model.FileSize = (int)newfile.Length / 1024;    //圖片大小

                #region base64string 縮圖處理

                using (var ms = new MemoryStream())
                {
                    using (var fs = new FileStream(newfile.FullName, FileMode.Open))
                    {
                        using (var img = System.Drawing.Image.FromStream(fs))
                        {
                            oDocument_model.PixelW = img.Width;
                            oDocument_model.PixelH = img.Height;

                            var intW = 100;
                            var intH = 100;

                            if (img.Width > img.Height)
                            {
                                intW = 100;
                                intH = (int)100 * img.Height / img.Width;
                            }
                            else
                            {
                                intH = 100;
                                intW = (int)100 * img.Width / img.Height;
                            }

                            using (var NewImage = new System.Drawing.Bitmap(intW, intH))
                            {
                                using (var g = System.Drawing.Graphics.FromImage(NewImage))
                                {
                                    g.DrawImage(img, 0, 0, intW, intH);
                                    NewImage.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                                    var imageBytes = ms.ToArray();
                                    stringToBase64 = Convert.ToBase64String(imageBytes);
                                    oDocument_model.FileContent = "data:image/jpg;base64," + stringToBase64;
                                }
                            }
                        }
                    }
                }

                #endregion base64string 縮圖處理

                oDocument_model.IsProtected = "N";                   //是否能複製、移除
                oDocument_model.IsPublic = "N";                      //是否共用
                oDocument_model.Memo = (fileroot + filename).Replace("/", " ");   //關鍵字查詢
                oDocument_model.FileCreateDate = newfile.CreationTime;    //檔案創建日
                oDocument_model.CreateUser = !string.IsNullOrEmpty(user) ? user : "apadmin";                //創建人
                oDocument_model.ModifyUser = !string.IsNullOrEmpty(user) ? user : "apadmin";                //修改人

                var db = SugarBase.GetIntance();
                if (isrepeat == "true")
                {
                    //修改
                    var oDoc = db.Queryable<OTB_SYS_Document>().Single(it => it.FilePath == fileroot + filename && it.status != 99);
                    if (oDoc != null)
                    {
                        //有查到GUID才更新圖檔
                        var iEffect_Upd = db.Updateable(oDocument_model).Where(it => it.GUID == oDoc.GUID).ExecuteCommand();
                        if (iEffect_Upd > 0)
                        {
                            c.Response.Write(oDocument_model.GUID);    //儲存成功傳回圖檔GUID
                        }
                    }
                    else
                    {
                        //沒有找到相對GUID就新增新增
                        var iEffect_Add = db.Insertable(oDocument_model).ExecuteCommand();
                        if (iEffect_Add > 0)
                        {
                            c.Response.Write(oDocument_model.GUID);    //儲存成功傳回圖檔GUID
                        }
                    }
                }
                else
                {
                    //新增
                    var iEffect = db.Insertable(oDocument_model).ExecuteCommand();
                    if (iEffect > 0)
                    {
                        c.Response.Write(oDocument_model.GUID);    //儲存成功傳回圖檔GUID
                    }
                }
            }
            catch (Exception ex)
            {
                LogService.mo_Log.Error("Controller.SaveImg Error Message:" + ex.Message, ex);
                c.Response.Write("0");   //儲存成功傳回0
            }
        }

        #endregion SaveImg 儲存圖片

        #region PutImportFile 將匯入的文件放在服務器上

        public static void PutImportFile(HttpContext c)
        {
            var req = c.Request;
            var sFileId = c.Request["FileId"];
            var sServerPath = HttpContext.Current.Server.MapPath("/");
            var sRoot = sServerPath + "OutFiles\\Temporary";
            Common.FnCreateDir(sRoot);//如果沒有該目錄就創建目錄
            var CheckFile = req.Files[0].FileName;        //檔案名稱
            if (CheckFile != "")
            {
                var file = req.Files[0];
                var sFileName = Path.GetFileName(file.FileName);//檔案名稱+文件格式名稱
                var sfileName = sFileName.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
                var sSubFileName = sfileName.LastOrDefault();     //副檔名
                var sNewFileName = sFileId + '.' + sSubFileName;

                var sOutputPath = sRoot + "/" + sNewFileName;
                sOutputPath = System.Text.RegularExpressions.Regex.Replace(sOutputPath, @"//|/", @"\");
                if (File.Exists(sOutputPath))
                {
                    File.Delete(sOutputPath);
                }
                file.SaveAs(sOutputPath);
            }
        }

        #endregion PutImportFile 將匯入的文件放在服務器上

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}