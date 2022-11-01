<%@ WebHandler Language="C#" Class="WebHandler" %>

using System;
using System.Web;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Linq;
using EasyBL;
using EasyNet;
using Entity.Sugar;
using SqlSugar.Base;
using System.Web.SessionState;

public class WebHandler : IHttpHandler, IRequiresSessionState
{
    string default_w = "120";
    string default_h = "76";
    int iwidth;
    int iheight;
    bool parseOk = false;
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
            case "saveimg":
                SaveImg(context);
                break;
            case "uploadfile":
                UploadFile(context);
                break;
            case "securityimg":
                SecurityImg(context);
                break;
            default:
                break;

        }
        //action.Process();
    }

    #region UploadFile 上傳文件

    public void UploadFile(HttpContext context)
    {
        var req = context.Request;
        var sSource = req["source"];
        var sOrgID = req["orgid"];
        var sUserID = req["userid"];
        var sParentID = req["parentid"];
        var sServerPath = HttpContext.Current.Server.MapPath("/");
        var sRoot = sServerPath + "Document\\EurotranFile";
        Common.FnCreateDir(sRoot + "\\" + sSource);//如果沒有該目錄就創建目錄
        var CheckFile = req.Files[0].FileName;        //檔案名稱
        if (CheckFile != "")
        {
            var list_Add = new List<OTB_SYS_Files>();
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
                list_Add.Add(oFile);
            }
            var db = SugarBase.DB;
            var iRes = db.Insertable(list_Add.ToArray()).ExecuteCommand();
        }
    }

    #endregion

    #region downFile 下載文件

    public void downFile(HttpContext c)
    {
        var req = c.Request;
        var sPath = req["path"];
        var sCusFileName = req["filename"];
        sPath = System.Text.RegularExpressions.Regex.Replace(sPath, @"//|/", @"\");
        var sServerPath = c.Server.MapPath("/");
        sPath = sPath.Replace(sServerPath, "");
        sPath = c.Server.MapPath(sPath);
        var sFileName = "";
        var sExtName = "";

        if (!File.Exists(sPath))
        {
            return;
        }

        if (sPath != null)
        {
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
        }

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
    #endregion

    #region SaveImg 儲存圖片

    public void SaveImg(HttpContext c)
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
            #endregion
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
    #endregion

    #region SecurityImg 產生驗證碼

    public void SecurityImg(HttpContext c)
    {
        var txt = c.Response;
        try
        {
            var flag = string.IsNullOrEmpty(c.Request["flag"]) ? "" : c.Request["flag"].ToString();
            var swidth = string.IsNullOrEmpty(c.Request["w"]) ? default_w : c.Request["w"].ToString();
            var sheight = string.IsNullOrEmpty(c.Request["h"]) ? default_h : c.Request["h"].ToString();
            c.Response.Expires = -1;
            parseOk = int.TryParse(swidth, out iwidth);
            if (!parseOk)
            {
                iwidth = Convert.ToInt16(default_w);
            }
            parseOk = int.TryParse(sheight, out iheight);
            if (!parseOk)
            {
                iheight = Convert.ToInt16(default_h);
            }

            switch (flag)
            {
                case "cap1":
                case "cap2":
                case "cap3":
                case "cap4":
                    captcha_recon(c, iwidth, iheight, flag);
                    break;
                case "cap11":
                    captchaImg_sum(c, iwidth, iheight, flag);
                    break;
                default:
                    break;
            }
        }
        catch (Exception ex)
        {
            LogService.mo_Log.Error("Controller.SecurityImg Error Message:" + ex.Message);
        }
    }
    #endregion

    void captchaImg_sum(HttpContext context, int w, int h, string flag)
    {
        captchaGen(context, flag);
        var bmpOut = new Bitmap(w, h);
        var g = Graphics.FromImage(bmpOut);
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        var x = 0;
        g.FillRectangle(Brushes.Black, x, 0, w, h);

        float fontsize = 6;
        var sizeSetupCompleted = false;
        var text = Convert.ToString(context.Session["imgText"]);
        while (!sizeSetupCompleted)
        {
            var mySize = g.MeasureString(text, new Font("Verdana", fontsize, FontStyle.Bold));
            if (mySize.Width < w || mySize.Height < h)
            {
                fontsize += float.Parse("0.1");
            }
            else
            {
                sizeSetupCompleted = true;
            }
        }

        for (var i = 0; i < text.Length; i++)
        {
            if (i.ToString() == context.Session["no1"].ToString() || i.ToString() == context.Session["no2"].ToString())
            {
                g.DrawString(text.Substring(i, 1), new Font("Verdana", fontsize), new SolidBrush(Color.Red), x, 0);
            }
            else
            {
                g.DrawString(text.Substring(i, 1), new Font("Verdana", fontsize), new SolidBrush(Color.White), x, 0);
            }
            x += Convert.ToInt32(fontsize / 2);
        }
        var ms = new MemoryStream();

        bmpOut.Save(ms, ImageFormat.Png);

        var bmpBytes = ms.GetBuffer();

        bmpOut.Dispose();

        ms.Close();

        context.Response.BinaryWrite(bmpBytes);
        HttpContext.Current.ApplicationInstance.CompleteRequest();
        //context.Response.End();
    }

    void captcha_recon(HttpContext context, int w, int h, string flag)
    {
        // Create a random code and store it in the Session object.
        var sRandomCode = SecurityUtil.GetRandomNumber(4);
        context.Session[BLWording.CAPTCHA + flag] = sRandomCode;
        // Create a CAPTCHA image using the text stored in the Session object.
        var ci = new RandomImage(sRandomCode, w, h);
        // Change the response headers to output a JPEG image.
        context.Response.Clear();
        context.Response.ContentType = "image/jpeg";

        var ms = new MemoryStream();
        ci.Image.Save(ms, ImageFormat.Jpeg);

        var bmpBytes = ms.GetBuffer();

        ci.Dispose();

        ms.Close();

        context.Response.BinaryWrite(bmpBytes);
        HttpContext.Current.ApplicationInstance.CompleteRequest();
        //context.Response.End();
    }

    void captchaGen(HttpContext context, string flag)
    {
        var ran = new Random();
        var no = "";

        while (no.Length < 6)
        {
            no = ran.Next(100000, 1000000).ToString();
        }

        context.Session["no1"] = ran.Next(5);
        context.Session["no2"] = ran.Next(5);

        while (context.Session["no1"].ToString() == context.Session["no2"].ToString())
        {
            context.Session["no2"] = ran.Next(5);
        }
        context.Session["imgText"] = no;
        context.Session[BLWording.CAPTCHA + flag] = int.Parse(no.Substring(int.Parse(context.Session["no1"].ToString()), 1)) + int.Parse(no.Substring(int.Parse(context.Session["no2"].ToString()), 1));

    }
    public class RandomImage
    {
        //Default Constructor
        public RandomImage() { }
        //property
        public string Text
        {
            get { return this.text; }
        }
        public Bitmap Image
        {
            get { return this.image; }
        }
        public int Width
        {
            get { return this.width; }
        }
        public int Height
        {
            get { return this.height; }
        }
        //Private variable
        private string text;
        private int width;
        private int height;
        private Bitmap image;
        private Random random = new Random();
        //Methods declaration
        public RandomImage(string s, int width, int height)
        {
            this.text = s;
            this.SetDimensions(width, height);
            this.GenerateImage();
        }
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            this.Dispose(true);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                this.image.Dispose();
        }
        private void SetDimensions(int width, int height)
        {
            if (width <= 0)
                throw new ArgumentOutOfRangeException("width", width,
                    "Argument out of range, must be greater than zero.");
            if (height <= 0)
                throw new ArgumentOutOfRangeException("height", height,
                    "Argument out of range, must be greater than zero.");
            this.width = width;
            this.height = height;
        }
        private void GenerateImage()
        {
            var bitmap = new Bitmap
              (this.width, this.height, PixelFormat.Format32bppArgb);
            var g = Graphics.FromImage(bitmap);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = new Rectangle(0, 0, this.width, this.height);
            var hatchBrush = new HatchBrush(HatchStyle.SmallConfetti,
                Color.LightGray, Color.White);
            g.FillRectangle(hatchBrush, rect);
            //SizeF size;
            float fontSize = rect.Height + 1;
            Font font;

            float fontsize = 6;
            var sizeSetupCompleted = false;
            var text = this.text;
            while (!sizeSetupCompleted)
            {
                var mySize = g.MeasureString(text, new Font("Verdana", fontsize, FontStyle.Bold));
                if (mySize.Width < rect.Width || mySize.Height < rect.Height)
                {
                    fontsize += float.Parse("0.1");
                }
                else
                {
                    sizeSetupCompleted = true;
                }
            }
            font = new Font(FontFamily.GenericSansSerif, fontsize, FontStyle.Bold);
            using (var format = new StringFormat())
            {
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;
                var path = new GraphicsPath();
                //path.AddString(this.text, font.FontFamily, (int) font.Style,
                //    font.Size, rect, format);
                path.AddString(this.text, font.FontFamily, (int)font.Style, font.Size, rect, format);
                var v = 4F;
                PointF[] points =
            {
                new PointF(this.random.Next(rect.Width) / v, this.random.Next(
                   rect.Height) / v),
                new PointF(rect.Width - this.random.Next(rect.Width) / v,
                    this.random.Next(rect.Height) / v),
                new PointF(this.random.Next(rect.Width) / v,
                    rect.Height - this.random.Next(rect.Height) / v),
                new PointF(rect.Width - this.random.Next(rect.Width) / v,
                    rect.Height - this.random.Next(rect.Height) / v)
          };
                var matrix = new Matrix();
                matrix.Translate(0F, 0F);
                path.Warp(points, rect, matrix, WarpMode.Perspective, 0F);
                hatchBrush = new HatchBrush(HatchStyle.Percent10, Color.Black, Color.SkyBlue);

                var m = Math.Max(rect.Width, rect.Height);
                for (var i = 0; i < (int)(rect.Width * rect.Height / 30F); i++)
                {
                    var x = this.random.Next(rect.Width);
                    var y = this.random.Next(rect.Height);
                    var w = this.random.Next(m / 50);
                    var h = this.random.Next(m / 50);
                    g.FillEllipse(hatchBrush, x, y, w, h);
                }
                hatchBrush = new HatchBrush(HatchStyle.Percent10, Color.Black, Color.OrangeRed);
                g.FillPath(hatchBrush, path);
                font.Dispose();
                hatchBrush.Dispose();
                g.Dispose();
                this.image = bitmap;
            }
        }
    }

    public bool IsReusable
    {
        get
        {
            return false;
        }
    }
}