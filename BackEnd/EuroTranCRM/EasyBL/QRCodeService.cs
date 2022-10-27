using EasyBL.WebApi.Message;
using System;
using System.Drawing;
using System.IO;
using ThoughtWorks.QRCode.Codec;

namespace EasyBL
{
    public class QRCodeService : ServiceBase
    {
        /// <summary>
        /// 產生二維碼
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on GetQRCode</param>
        /// <returns></returns>
        public ResponseMessage GetQRCode(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var sPath = @"Document/EurotranFile/QRCode";
            try
            {
                var sKey = _fetchString(i_crm, "guid");
                var sSize = _fetchString(i_crm, "size") ?? "8";
                var iSize = int.Parse(sSize);

                var bs = Create_ImgCode(sKey, iSize);
                var sImgPath = SaveImg(ref sPath, bs);
                if (!File.Exists(sImgPath))
                {
                    sMsg = "產生失敗";
                }
                rm = new SuccessResponseMessage(null, i_crm);
                rm.DATA.Add(BLWording.REL, sPath);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
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

        /// <summary>
        /// 生成二维码图片
        /// </summary>
        /// <param name="codeNumber">要生成二维码的字符串</param>
        /// <param name="size">大小尺寸</param>
        /// <returns>二维码图片</returns>
        public static Bitmap Create_ImgCode(string codeNumber, int size)
        {
            //创建二维码生成类
            var qrCodeEncoder = new QRCodeEncoder
            {
                //设置编码模式
                QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.BYTE,
                //设置编码测量度
                QRCodeScale = size,
                //设置编码版本
                QRCodeVersion = 0,
                //设置编码错误纠正
                QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.M
            };
            //生成二维码图片
            var image = qrCodeEncoder.Encode(codeNumber);
            return image;
        }

        /// <summary>
        /// 保存图片
        /// </summary>
        /// <param name="sPath">保存路径</param>
        /// <param name="img">图片</param>
        public string SaveImg(ref string sPath, Bitmap img)
        {
            var sSavePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, sPath);
            //保存图片到目录
            if (Directory.Exists(sSavePath))
            {
                //文件名称
                var guid = Guid.NewGuid().ToString().Replace("-", "") + ".png";
                img.Save(sSavePath + "/" + guid, System.Drawing.Imaging.ImageFormat.Png);
                sPath += "/" + guid;
                sSavePath += "/" + guid;
            }
            else
            {
                //当前目录不存在，则创建
                Directory.CreateDirectory(sSavePath);
                this.SaveImg(ref sPath, img);
            }
            return sSavePath;
        }
    }
}