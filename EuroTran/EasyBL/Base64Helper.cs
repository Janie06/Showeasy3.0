using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace EasyBL
{
    public class Base64Helper
    {
        /// <summary>
        /// </summary>
        /// <param name="str">todo: describe str parameter on Deserialize</param>
        /// <returns></returns>
        public static object Deserialize(string str)
        {
            return Base64Helper.DeserializeBase64(str);
        }

        /// <summary>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string Serialize(object obj)
        {
            return Base64Helper.SerializeBase64(obj);
        }

        public static string SerializeBase64(object o)
        {
            // Serialize to a base 64 string
            byte[] bytes;
            long length = 0;
            var ws = new MemoryStream();
            var sf = new BinaryFormatter();
            sf.Serialize(ws, o);
            length = ws.Length;
            bytes = ws.GetBuffer();
            var encodedData = bytes.Length + ":" + Convert.ToBase64String(bytes, 0, bytes.Length, Base64FormattingOptions.None);
            return encodedData;
        }

        public static object DeserializeBase64(string s)
        {
            // We need to know the exact length of the string - Base64 can sometimes pad us by a byte
            // or two
            var p = s.IndexOf(':');
            var length = Convert.ToInt32(s.Substring(0, p));

            // Extract data from the base 64 string!
            var memorydata = Convert.FromBase64String(s.Substring(p + 1));
            var rs = new MemoryStream(memorydata, 0, length);
            var sf = new BinaryFormatter();
            var o = sf.Deserialize(rs);
            return o;
        }

        public static string ToBase64(string str)
        {
            return EncodingString(str);
        }

        /// <summary>
        /// 将字符串使用base64算法加密
        /// </summary>
        /// <param name="SourceString">待加密的字符串</param>
        /// <param name="Ens">Encoding 对象，如创建中文编码集对象： Encoding.GetEncoding("gb2312")</param>
        /// <returns>编码后的文本字符串</returns>
        public static string EncodingString(string SourceString, Encoding Ens)
        {
            return Convert.ToBase64String(Ens.GetBytes(SourceString));
        }

        /// <summary>
        /// 使用缺省的代码页将字符串使用base64算法加密
        /// </summary>
        /// <param name="SourceString">待加密的字符串</param>
        /// <returns>加密后的文本字符串</returns>
        public static string EncodingString(string SourceString)
        {
            return EncodingString(SourceString, Encoding.Default);
        }

        /// <summary>
        /// 从base64编码的字符串中还原字符串，支持中文
        /// </summary>
        /// <param name="Base64String">Base64加密后的字符串</param>
        /// <param name="Ens">Encoding对象，如创建中文编码集对象： Encoding.Default</param>
        /// <returns>还原后的文本字符串</returns>
        public static string DecodingString(string Base64String, Encoding Ens)
        {
            return Ens.GetString((Convert.FromBase64String(Base64String)));
        }

        /// <summary>
        ///使用缺省的代码页从Base64编码的字符串中还原字符串，支持中文
        /// </summary>
        /// <param name="Base64String">Base64加密后的字符串</param>
        /// <returns>还原后的文本字符串</returns>
        public static string DecodingString(string Base64String)
        {
            return DecodingString(Base64String, Encoding.Default);
        }

        /// <summary>
        /// 对一个文件进行Base64编码，并返回编码后的字符串
        /// </summary>
        /// <param name="strFileName">文件的路径和文件名</param>
        /// <returns>对文件进行Base64编码后的字符串</returns>
        public static string EncodingFileToString(string strFileName)
        {
            var fs = File.OpenRead(strFileName);
            using (var br = new BinaryReader(fs))
            {
                var Base64String = Convert.ToBase64String(br.ReadBytes((int)fs.Length));

                br.Close();
                fs.Close();
                return Base64String;
            }
        }

        /// <summary>
        /// 对一个文件进行Base64编码，并将编码后的内容写入到一个文件
        /// </summary>
        /// <param name="strSourceFileName">要编码的文件地址，支持任何类型的文件</param>
        /// <param name="strSaveFileName">要写入的文件路径</param>
        /// <returns>如果写入成功，则返回真</returns>
        public static bool EncodingFileToFile(string strSourceFileName, string strSaveFileName)
        {
            var strBase64 = EncodingFileToString(strSourceFileName);

            using (var fs = new StreamWriter(strSaveFileName))
            {
                fs.Write(strBase64);
                fs.Close();
                return true;
            }
        }

        /// <summary>
        /// 将Base64编码字符串解码并存储到一个文件中
        /// </summary>
        /// <param name="Base64String">经过Base64编码后的字符串</param>
        /// <param name="strSaveFileName">要输出的文件路径，如果文件存在，将被重写</param>
        /// <returns>如果操作成功，则返回True</returns>
        public static bool DecodingFileFromString(string Base64String, string strSaveFileName)
        {
            var fs = new FileStream(strSaveFileName, FileMode.Create);
            using (var bw = new BinaryWriter(fs))
            {
                bw.Write(Convert.FromBase64String(Base64String));
                //bw.Write(Convert.ToBase64String)
                bw.Close();
                fs.Close();
                return true;
            }
        }

        /// <summary>
        /// 将一个由Base64编码产生的文件解码并存储到一个文件
        /// </summary>
        /// <param name="strBase64FileName">以Base64编码格式存储的文件</param>
        /// <param name="strSaveFileName">要输出的文件路径，如果文件存在，将被重写</param>
        /// <returns>如果操作成功，则返回True</returns>
        public static bool DecodingFileFromFile(string strBase64FileName, string strSaveFileName)
        {
            using (var fs = new StreamReader(strBase64FileName, Encoding.ASCII))
            {
                var base64CharArray = new char[fs.BaseStream.Length];
                fs.Read(base64CharArray, 0, (int)fs.BaseStream.Length);
                var Base64String = new string(base64CharArray);
                fs.Close();
                return DecodingFileFromString(Base64String, strSaveFileName);
            }
        }

        /// <summary>
        /// 从网络地址一取得文件并转化为base64编码
        /// </summary>
        /// <param name="strURL">文件的URL地址,必须是绝对URL地址</param>
        /// <param name="objWebClient">System.Net.WebClient 对象</param>
        /// <returns>返回经过Base64编码的Web资源字符串</returns>
        public static string EncodingWebFile(string strURL, System.Net.WebClient objWebClient)
        {
            return Convert.ToBase64String(objWebClient.DownloadData(strURL));
        }

        /// <summary>
        /// 从网络地址一取得文件并转化为base64编码
        /// </summary>
        /// <param name="strURL">文件的URL地址,必须是绝对URL地址</param>
        /// <returns>返回经过Base64编码的Web资源字符串</returns>
        public static string EncodingWebFile(string strURL) => EncodingWebFile(strURL, new System.Net.WebClient());
    }
}