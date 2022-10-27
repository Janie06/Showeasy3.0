using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace EasyBL
{
    public class HttpWebClient
    {
        private readonly List<HttpUploadingFile> files = new List<HttpUploadingFile>();
        private readonly Dictionary<string, string> postingData = new Dictionary<string, string>();
        private WebHeaderCollection responseHeaders;

        #region events

        public event EventHandler<StatusUpdateEventArgs> StatusUpdate;

        private void OnStatusUpdate(StatusUpdateEventArgs e)
        {
            StatusUpdate?.Invoke(this, e);
        }

        #endregion events

        #region properties

        /// <summary>
        /// 是否自动在不同的请求间保留Cookie, Referer
        /// </summary>
        public bool KeepContext { get; set; }

        /// <summary>
        /// 期望的回应的语言
        /// </summary>
        public string DefaultLanguage { get; set; } = "zh-CN";

        /// <summary>
        /// GetString()如果不能从HTTP头或Meta标签中获取编码信息,则使用此编码来获取字符串
        /// </summary>
        public Encoding DefaultEncoding { get; set; } = Encoding.UTF8;

        /// <summary>
        /// 指示发出Get请求还是Post请求
        /// </summary>
        public HttpVerb Verb { get; set; } = HttpVerb.GET;

        /// <summary>
        /// 要上传的文件.如果不为空则自动转为Post请求
        /// </summary>
        public List<HttpUploadingFile> Files
        {
            get { return files; }
        }

        /// <summary>
        /// 要发送的Form表单信息
        /// </summary>
        public Dictionary<string, string> PostingData
        {
            get { return postingData; }
        }

        /// <summary>
        /// 获取或设置请求资源的地址
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 用于在获取回应后,暂时记录回应的HTTP头
        /// </summary>
        public WebHeaderCollection ResponseHeaders
        {
            get { return responseHeaders; }
        }

        /// <summary>
        /// 获取或设置期望的资源类型
        /// </summary>
        public string Accept { get; set; } = "*/*";

        /// <summary>
        /// 获取或设置请求中的Http头User-Agent的值
        /// </summary>
        public string UserAgent { get; set; } = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; SV1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";

        /// <summary>
        /// 获取或设置Cookie及Referer
        /// </summary>
        public HttpClientContext Context { get; set; }

        /// <summary>
        /// 获取或设置获取内容的起始点,用于断点续传,多线程下载等
        /// </summary>
        public int StartPoint { get; set; }

        /// <summary>
        /// 获取或设置获取内容的结束点,用于断点续传,多下程下载等. 如果为0,表示获取资源从StartPoint开始的剩余内容
        /// </summary>
        public int EndPoint { get; set; }

        #endregion properties

        #region constructors

        /// <summary>
        /// 构造新的HttpClient实例
        /// </summary>
        public HttpWebClient()
            : this(null)
        {
        }

        /// <summary>
        /// 构造新的HttpClient实例
        /// </summary>
        /// <param name="url">要获取的资源的地址</param>
        public HttpWebClient(string url)
            : this(url, null)
        {
        }

        /// <summary>
        /// 构造新的HttpClient实例
        /// </summary>
        /// <param name="url">要获取的资源的地址</param>
        /// <param name="context">Cookie及Referer</param>
        public HttpWebClient(string url, HttpClientContext context)
            : this(url, context, false)
        {
        }

        /// <summary>
        /// 构造新的HttpClient实例
        /// </summary>
        /// <param name="url">要获取的资源的地址</param>
        /// <param name="context">Cookie及Referer</param>
        /// <param name="keepContext">是否自动在不同的请求间保留Cookie, Referer</param>
        public HttpWebClient(string url, HttpClientContext context, bool keepContext)
        {
            this.Url = url;
            this.Context = context;
            this.KeepContext = keepContext;
            if (this.Context == null)
                this.Context = new HttpClientContext();
        }

        #endregion constructors

        #region AttachFile

        /// <summary>
        /// 在请求中添加要上传的文件
        /// </summary>
        /// <param name="fileName">要上传的文件路径</param>
        /// <param name="fieldName">文件字段的名称(相当于&lt;input type=file name=fieldName&gt;)里的fieldName)</param>
        public void AttachFile(string fileName, string fieldName)
        {
            var file = new HttpUploadingFile(fileName, fieldName);
            files.Add(file);
        }

        /// <summary>
        /// 在请求中添加要上传的文件
        /// </summary>
        /// <param name="data">要上传的文件内容</param>
        /// <param name="fileName">文件名</param>
        /// <param name="fieldName">文件字段的名称(相当于&lt;input type=file name=fieldName&gt;)里的fieldName)</param>
        public void AttachFile(byte[] data, string fileName, string fieldName)
        {
            var file = new HttpUploadingFile(data, fileName, fieldName);
            files.Add(file);
        }

        #endregion AttachFile

        /// <summary>
        /// 清空PostingData, Files, StartPoint, EndPoint, ResponseHeaders, 并把Verb设置为Get. 在发出一个包含上述信息的请求后,必须调用此方法或手工设置相应属性以使下一次请求不会受到影响.
        /// </summary>
        public void Reset()
        {
            Verb = HttpVerb.GET;
            files.Clear();
            postingData.Clear();
            responseHeaders = null;
            StartPoint = 0;
            EndPoint = 0;
        }

        private HttpWebRequest CreateRequest()
        {
            var req = (HttpWebRequest)WebRequest.Create(Url);
            req.AllowAutoRedirect = false;
            req.CookieContainer = new CookieContainer();
            req.Headers.Add("Accept-Language", DefaultLanguage);
            req.Accept = Accept;
            req.UserAgent = UserAgent;
            req.KeepAlive = false;

            if (Context.Cookies != null)
                req.CookieContainer.Add(Context.Cookies);
            if (!string.IsNullOrEmpty(Context.Referer))
                req.Referer = Context.Referer;

            if (Verb == HttpVerb.HEAD)
            {
                req.Method = "HEAD";
                return req;
            }

            if (postingData.Count > 0 || files.Count > 0)
                Verb = HttpVerb.POST;

            if (Verb == HttpVerb.POST)
            {
                req.Method = "POST";

                var memoryStream = new MemoryStream();
                using (var writer = new StreamWriter(memoryStream))
                {
                    if (files.Count > 0)
                    {
                        const string newLine = "\r\n";
                        var boundary = Guid.NewGuid().ToString().Replace("-", "");
                        req.ContentType = "multipart/form-data; boundary=" + boundary;

                        foreach (string key in postingData.Keys)
                        {
                            writer.Write("--" + boundary + newLine);
                            writer.Write("Content-Disposition: form-data; name=\"{0}\"{1}{1}", key, newLine);
                            writer.Write(postingData[key] + newLine);
                        }

                        foreach (HttpUploadingFile file in files)
                        {
                            writer.Write("--" + boundary + newLine);
                            writer.Write(
                                "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"{2}",
                                file.FieldName,
                                file.FileName,
                                newLine
                                );
                            writer.Write("Content-Type: application/octet-stream" + newLine + newLine);
                            writer.Flush();
                            memoryStream.Write(file.Data, 0, file.Data.Length);
                            writer.Write(newLine);
                            writer.Write("--" + boundary + newLine);
                        }
                    }
                    else
                    {
                        req.ContentType = "application/x-www-form-urlencoded";
                        var sb = new StringBuilder();
                        foreach (string key in postingData.Keys)
                        {
                            sb.AppendFormat("{0}={1}&", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(postingData[key]));
                        }
                        if (sb.Length > 0)
                            sb.Length--;
                        writer.Write(sb.ToString());
                    }

                    writer.Flush();

                    using (Stream stream = req.GetRequestStream())
                    {
                        memoryStream.WriteTo(stream);
                    }
                }
            }

            if (StartPoint != 0 && EndPoint != 0)
                req.AddRange(StartPoint, EndPoint);
            else if (StartPoint != 0 && EndPoint == 0)
                req.AddRange(StartPoint);

            return req;
        }

        /// <summary>
        /// 发出一次新的请求,并返回获得的回应 调用此方法永远不会触发StatusUpdate事件.
        /// </summary>
        /// <returns>相应的HttpWebResponse</returns>
        public HttpWebResponse GetResponse()
        {
            var req = CreateRequest();
            var res = (HttpWebResponse)req.GetResponse();
            responseHeaders = res.Headers;
            if (KeepContext)
            {
                Context.Cookies = res.Cookies;
                Context.Referer = Url;
            }
            return res;
        }

        /// <summary>
        /// 发出一次新的请求,并返回回应内容的流 调用此方法永远不会触发StatusUpdate事件.
        /// </summary>
        /// <returns>包含回应主体内容的流</returns>
        public Stream GetStream()
        {
            return GetResponse().GetResponseStream();
        }

        /// <summary>
        /// 发出一次新的请求,并以字节数组形式返回回应的内容 调用此方法会触发StatusUpdate事件
        /// </summary>
        /// <returns>包含回应主体内容的字节数组</returns>
        public byte[] GetBytes()
        {
            var res = GetResponse();
            var length = (int)res.ContentLength;

            using (var memoryStream = new MemoryStream())
            {
                var buffer = new byte[0x100];
                var rs = res.GetResponseStream();
                for (int i = rs.Read(buffer, 0, buffer.Length); i > 0; i = rs.Read(buffer, 0, buffer.Length))
                {
                    memoryStream.Write(buffer, 0, i);
                    OnStatusUpdate(new StatusUpdateEventArgs((int)memoryStream.Length, length));
                }
                rs.Close();

                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// 发出一次新的请求,以Http头,或Html Meta标签,或DefaultEncoding指示的编码信息对回应主体解码 调用此方法会触发StatusUpdate事件
        /// </summary>
        /// <returns>解码后的字符串</returns>
        public string GetString()
        {
            var data = GetBytes();
            var encodingName = GetEncodingFromHeaders();

            if (encodingName == null)
                encodingName = GetEncodingFromBody(data);

            Encoding encoding;
            if (encodingName == null)
                encoding = DefaultEncoding;
            else
            {
                try
                {
                    encoding = Encoding.GetEncoding(encodingName);
                }
                catch (ArgumentException)
                {
                    encoding = DefaultEncoding;
                }
            }
            return encoding.GetString(data);
        }

        /// <summary>
        /// 发出一次新的请求,对回应的主体内容以指定的编码进行解码 调用此方法会触发StatusUpdate事件
        /// </summary>
        /// <param name="encoding">指定的编码</param>
        /// <returns>解码后的字符串</returns>
        public string GetString(Encoding encoding)
        {
            var data = GetBytes();
            return encoding.GetString(data);
        }

        private string GetEncodingFromHeaders()
        {
            string encoding = null;
            var contentType = responseHeaders["Content-Type"];
            if (contentType != null)
            {
                var i = contentType.IndexOf("charset=");
                if (i != -1)
                {
                    encoding = contentType.Substring(i + 8);
                }
            }
            return encoding;
        }

        private static string GetEncodingFromBody(byte[] data)
        {
            string encodingName = null;
            var dataAsAscii = Encoding.ASCII.GetString(data);
            if (dataAsAscii != null)
            {
                var i = dataAsAscii.IndexOf("charset=");
                if (i != -1)
                {
                    var j = dataAsAscii.IndexOf("\"", i);
                    if (j != -1)
                    {
                        var k = i + 8;
                        encodingName = dataAsAscii.Substring(k, (j - k) + 1);
                        var chArray = new char[2] { '>', '"' };
                        encodingName = encodingName.TrimEnd(chArray);
                    }
                }
            }
            return encodingName;
        }

        /// <summary>
        /// 发出一次新的Head请求,获取资源的长度 此请求会忽略PostingData, Files, StartPoint, EndPoint, Verb
        /// </summary>
        /// <returns>返回的资源长度</returns>
        public int HeadContentLength()
        {
            Reset();
            var lastVerb = Verb;
            Verb = HttpVerb.HEAD;
            using (HttpWebResponse res = GetResponse())
            {
                Verb = lastVerb;
                return (int)res.ContentLength;
            }
        }

        /// <summary>
        /// 发出一次新的请求,把回应的主体内容保存到文件 调用此方法会触发StatusUpdate事件 如果指定的文件存在,它会被覆盖
        /// </summary>
        /// <param name="fileName">要保存的文件路径</param>
        public void SaveAsFile(string fileName)
        {
            SaveAsFile(fileName, FileExistsAction.Overwrite);
        }

        /// <summary>
        /// 发出一次新的请求,把回应的主体内容保存到文件 调用此方法会触发StatusUpdate事件
        /// </summary>
        /// <param name="fileName">要保存的文件路径</param>
        /// <param name="existsAction">指定的文件存在时的选项</param>
        /// <returns>是否向目标文件写入了数据</returns>
        public bool SaveAsFile(string fileName, FileExistsAction existsAction)
        {
            var data = GetBytes();
            switch (existsAction)
            {
                case FileExistsAction.Overwrite:
                    using (BinaryWriter writer = new BinaryWriter(new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write)))
                        writer.Write(data);
                    return true;

                case FileExistsAction.Append:
                    using (BinaryWriter writer = new BinaryWriter(new FileStream(fileName, FileMode.Append, FileAccess.Write)))
                        writer.Write(data);
                    return true;

                default:
                    if (!File.Exists(fileName))
                    {
                        using (
                            BinaryWriter writer =
                                new BinaryWriter(new FileStream(fileName, FileMode.Create, FileAccess.Write)))
                            writer.Write(data);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
            }
        }
    }

    public class HttpClientContext
    {
        public CookieCollection Cookies { get; set; }

        public string Referer { get; set; }
    }

    public enum HttpVerb
    {
        GET,
        POST,
        HEAD,
    }

    public enum FileExistsAction
    {
        Overwrite,
        Append,
        Cancel,
    }

    public class HttpUploadingFile
    {
        public string FileName { get; set; }

        public string FieldName { get; set; }

        public byte[] Data { get; set; }

        public HttpUploadingFile(string fileName, string fieldName)
        {
            this.FileName = fileName;
            this.FieldName = fieldName;
            using (FileStream stream = new FileStream(fileName, FileMode.Open))
            {
                var inBytes = new byte[stream.Length];
                stream.Read(inBytes, 0, inBytes.Length);
                Data = inBytes;
            }
        }

        public HttpUploadingFile(byte[] data, string fileName, string fieldName)
        {
            this.Data = data;
            this.FileName = fileName;
            this.FieldName = fieldName;
        }
    }

    public class StatusUpdateEventArgs : EventArgs
    {
        private readonly int bytesGot;
        private readonly int bytesTotal;

        public StatusUpdateEventArgs(int got, int total)
        {
            bytesGot = got;
            bytesTotal = total;
        }

        /// <summary>
        /// 已经下载的字节数
        /// </summary>
        public int BytesGot
        {
            get { return bytesGot; }
        }

        /// <summary>
        /// 资源的总字节数
        /// </summary>
        public int BytesTotal
        {
            get { return bytesTotal; }
        }
    }
}