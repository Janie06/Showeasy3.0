using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace EasyBL
{
    public class WebApiUtil
    {
        /// <summary>
        /// HttpClient实现Post请求(异步)
        /// </summary>
        private static async void DooPost()
        {
            var url = "http://localhost:52824/api/register";
            //设置HttpClientHandler的AutomaticDecompression
            var handler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip };
            //创建HttpClient（注意传入HttpClientHandler）
            using (var http = new HttpClient(handler))
            {
                //使用FormUrlEncodedContent做HttpContent
                var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {    {"Id","6"},
             {"Name","添加zzl"},
             {"Info", "添加动作"}//键名必须为空
         });

                //await异步等待回应

                var response = await http.PostAsync(url, content);
                //确保HTTP成功状态值
                response.EnsureSuccessStatusCode();
                //await异步读取最后的JSON（注意此时gzip已经被自动解压缩了，因为上面的AutomaticDecompression = DecompressionMethods.GZip）
                Console.WriteLine(await response.Content.ReadAsStringAsync());
            }
        }

        /// <summary>
        /// HttpClient实现Get请求(异步)
        /// </summary>
        private static async void DooGet()
        {
            var url = "http://localhost:52824/api/register?id=1";
            //创建HttpClient（注意传入HttpClientHandler）
            var handler = new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip };

            using (var http = new HttpClient(handler))
            {
                //await异步等待回应
                var response = await http.GetAsync(url);
                //确保HTTP成功状态值
                response.EnsureSuccessStatusCode();

                //await异步读取最后的JSON（注意此时gzip已经被自动解压缩了，因为上面的AutomaticDecompression = DecompressionMethods.GZip）
                Console.WriteLine(await response.Content.ReadAsStringAsync());
            }
        }

        /// <summary>
        /// HttpClient实现Put请求(异步)
        /// </summary>
        private static async void DooPut()
        {
            var userId = 1;
            var url = "http://localhost:52824/api/register?userid=" + userId;

            //设置HttpClientHandler的AutomaticDecompression
            var handler = new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip };
            //创建HttpClient（注意传入HttpClientHandler）
            using (var http = new HttpClient(handler))
            {
                //使用FormUrlEncodedContent做HttpContent
                var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
           {"Name","修改zzl"},
           {"Info", "Put修改动作"}//键名必须为空
        });

                //await异步等待回应

                var response = await http.PutAsync(url, content);
                //确保HTTP成功状态值
                response.EnsureSuccessStatusCode();
                //await异步读取最后的JSON（注意此时gzip已经被自动解压缩了，因为上面的AutomaticDecompression = DecompressionMethods.GZip）
                Console.WriteLine(await response.Content.ReadAsStringAsync());
            }
        }
    }
}