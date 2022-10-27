using System;
using System.Configuration;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace EasyNet
{
    /// <summary>
    /// 安全處理通用類
    /// </summary>
    public class SecurityUtil
    {
        private static readonly char[] constantN = {
            '0','1','2','3','4','5','6','7','8','9'
          };

        private static readonly char[] constantU = {
            '0','1','2','3','4','5','6','7','8','9',
            'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z'
          };

        private static readonly char[] constantL = {
            '0','1','2','3','4','5','6','7','8','9',
            'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z'
          };

        private static readonly char[] constantUL = {
            '0','1','2','3','4','5','6','7','8','9',
            'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z',
            'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z'
          };

        #region Des

        //加密範例
        public static string DESEncrypt(string data, string sKey, string sIV)
        {
            //將key轉成utf8編碼 byte array
            var tmpkey = Encoding.UTF8.GetBytes(sKey);

            //將iv轉成utf8編碼 byte ayyay
            var tmpIV = Encoding.UTF8.GetBytes(sIV);

            using (var mD5Provider = new MD5CryptoServiceProvider())
            {
                var key = mD5Provider.ComputeHash(tmpkey);
                var iv = mD5Provider.ComputeHash(tmpIV);

                //將data轉成utf8編碼 byte ayyay
                var byteData = Encoding.UTF8.GetBytes(data);

                //加密
                using (var aesProvider = new RijndaelManaged())
                {
                    var aesEncrypt = aesProvider.CreateEncryptor(key, iv);
                    var result = aesEncrypt.TransformFinalBlock(byteData, 0, byteData.Length);

                    //轉成base64字串
                    return Convert.ToBase64String(result);
                }
            }
        }

        //解密範例

        public static string DESDecrypt(string data, string sKey, string sIV)
        {
            //將key轉成utf8編碼 byte array
            var tmpkey = Encoding.UTF8.GetBytes(sKey);

            //將iv轉成utf8編碼 byte array
            var tmpIV = Encoding.UTF8.GetBytes(sIV);

            using (var mD5Provider = new MD5CryptoServiceProvider())
            {
                var key = mD5Provider.ComputeHash(tmpkey);
                var iv = mD5Provider.ComputeHash(tmpIV);

                //將base64字串轉成byte array
                var encryptData = Convert.FromBase64String(data);

                //解密
                using (var aesProvider = new RijndaelManaged())
                {
                    var aesDecrypt = aesProvider.CreateDecryptor(key, iv);
                    var result = aesDecrypt.TransformFinalBlock(encryptData, 0, encryptData.Length);

                    //將解密後的內容還原成utf8編碼的字串

                    return Encoding.UTF8.GetString(result);
                }
            }
        }

        #endregion Des

        #region md5

        /// <summary>
        /// 字符串MD5加密
        /// </summary>
        /// <param name="sData">todo: describe sData parameter on MD5</param>
        /// <param name="sLen">todo: describe sLen parameter on MD5</param>
        /// <returns>密文</returns>
        public static string MD5(string sData, string sLen)
        {
            if (sLen == "16")
            {
                using (var md5 = new MD5CryptoServiceProvider())
                {
                    var text16 = BitConverter.ToString(md5.ComputeHash(UTF8Encoding.Default.GetBytes(sData)), 4, 8);
                    text16 = text16.Replace("-", "");
                    return text16;
                }
            }

            var bytes = Encoding.Default.GetBytes(sData);
            using (var mD5CryptoServiceProvider = new MD5CryptoServiceProvider())
            {
                bytes = mD5CryptoServiceProvider.ComputeHash(bytes);
                var text = "";
                var builder = new StringBuilder();
                builder.Append(text);
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x").PadLeft(2, '0'));
                }
                text = builder.ToString();
                return text;
            }
        }

        #endregion md5

        #region Shr256

        public static string SHA256(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);
            using (var managed = new SHA256Managed())
            {
                return Convert.ToBase64String(managed.ComputeHash(bytes));
            }
        }

        #endregion Shr256

        #region Aes

        /// <summary>
        /// AES 加密
        /// </summary>
        /// <param name="Data">明碼字符串</param>
        /// <param name="sKey">密匙</param>
        /// <param name="sIV">初始化向量</param>
        /// <returns>加密字符串</returns>
        public static string AESEncrypt(string Data, string sKey, string sIV)
        {
            try
            {
                var _data = Encoding.UTF8.GetBytes(Data);
                var Key = Encoding.UTF8.GetBytes(sKey);
                var tmpIV = Encoding.UTF8.GetBytes(sIV);
                var RijndaelAlg = Rijndael.Create();
                var memory = new MemoryStream();
                using (var cStream = new CryptoStream(memory,
                    RijndaelAlg.CreateEncryptor(Key, tmpIV),
                    CryptoStreamMode.Write))
                {
                    try
                    {
                        cStream.Write(_data, 0, _data.Length);
                        cStream.FlushFinalBlock();
                        return Convert.ToBase64String(memory.ToArray());
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                    finally
                    {
                        memory.Close();
                        cStream.Close();
                    }
                }
            }
            catch (CryptographicException e)
            {
                throw new Exception("some reason to rethrow", e);
            }
        }

        /// <summary>
        /// AES 解密
        /// </summary>
        /// <param name="Data">密文</param>
        /// <param name="sKey">todo: describe sKey parameter on AESDecrypt</param>
        /// <param name="sIV">todo: describe sIV parameter on AESDecrypt</param>
        /// <returns>明文</returns>
        public static string AESDecrypt(string Data, string sKey, string sIV)
        {
            try
            {
                var _data = Encoding.UTF8.GetBytes(Data);
                var Key = Encoding.UTF8.GetBytes(sKey);
                var tmpIV = Encoding.UTF8.GetBytes(sIV);
                var RijndaelAlg = Rijndael.Create();
                var memory = new MemoryStream(_data);
                using (var cStream = new CryptoStream(memory,
                    RijndaelAlg.CreateDecryptor(Key, tmpIV),
                    CryptoStreamMode.Read))
                {
                    string val = null;
                    try
                    {
                        // 明文存储区
                        using (MemoryStream originalMemory = new MemoryStream())
                        {
                            var Buffer = new Byte[1024];
                            var readBytes = 0;
                            while ((readBytes = cStream.Read(Buffer, 0, Buffer.Length)) > 0)
                            {
                                originalMemory.Write(Buffer, 0, readBytes);
                            }

                            val = Encoding.UTF8.GetString(originalMemory.ToArray());
                        }
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                    finally
                    {
                        cStream.Close();
                    }

                    return val;
                }
            }
            catch (CryptographicException e)
            {
                throw new Exception("some reason to rethrow", e);
            }
        }

        #endregion Aes

        #region 亂數取值

        //Validate Code
        public static string GetRandomString(int Stringleng)
        {
            var k = 0;
            var strRd = string.Empty;
            var rd = new Random(unchecked((int)DateTime.Now.Ticks));
            var builder = new StringBuilder();
            builder.Append(strRd);

            for (k = 0; k < Stringleng; k++)       // 亂數產生驗證文字
            {
                builder.Append(constantU[rd.Next(35)]);
            }
            strRd = builder.ToString();

            return strRd;
        }

        /// <summary>
        /// 生成亂數碼（純數字）
        /// </summary>
        /// <param name="iLength">todo: describe iLength parameter on GetRandomNumber</param>
        /// <returns></returns>
        public static string GetRandomNumber(int iLength = 10)
        {
            var newRandom = new StringBuilder(iLength);
            var rd = new Random();
            for (int i = 0; i < iLength; i++)
            {
                newRandom.Append(constantN[rd.Next(10)]);
            }
            return newRandom.ToString();
        }

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Encrypt(string str)
        {
            var sEncrypt = SecurityUtil.DESEncrypt(str, ConfigurationManager.AppSettings["DefaultCryptionKey"].Trim(), ConfigurationManager.AppSettings["DefaultCryptionIV"].Trim());
            return sEncrypt;
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Decrypt(string str)
        {
            var sDecrypt = SecurityUtil.DESDecrypt(str, ConfigurationManager.AppSettings["DefaultCryptionKey"].Trim(), ConfigurationManager.AppSettings["DefaultCryptionIV"].Trim());
            return sDecrypt;
        }

        #endregion 亂數取值
    }
}