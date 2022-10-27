using System;
using System.IO.Compression;
using System.Web;

namespace EasyBL.WebApi.Helper
{
    public class CompressUtils
    {
        public static void CompressCssAndJavascript(HttpApplication app)
        {
            var contentType = app.Response.ContentType;

            if (contentType == "application/x-javascript" || contentType == "text/javascript" || contentType == "application/javascript")
            {
                app.Response.Cache.VaryByHeaders["Accept-Encoding"] = true;
                Console.WriteLine(contentType);
                var acceptEncoding = app.Request.Headers["Accept-Encoding"];

                if (acceptEncoding == null || acceptEncoding.Length == 0) return;

                acceptEncoding = acceptEncoding.ToLower();

                if (acceptEncoding.Contains("gzip"))
                {
                    app.Response.Filter = new GZipStream(app.Response.Filter, CompressionMode.Compress);
                    app.Response.AppendHeader("Content-Encoding", "gzip");
                }
                else if (acceptEncoding.Contains("deflate") || acceptEncoding == "*")
                {
                    app.Response.Filter = new DeflateStream(app.Response.Filter, CompressionMode.Compress);
                    app.Response.AppendHeader("Content-Encoding", "deflate");
                }
            }
        }
    }
}