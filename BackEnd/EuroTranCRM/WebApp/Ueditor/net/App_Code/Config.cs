using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Web;

/// <summary>
/// Config 的摘要说明
/// </summary>
public static class Config
{
    private static bool noCache = true;

    private static JObject BuildItems()
    {
        var json = File.ReadAllText(HttpContext.Current.Server.MapPath("config.json"));
        return JObject.Parse(json);
    }

    public static JObject Items
    {
        get
        {
            if (noCache || _Items == null)
            {
                _Items = BuildItems();
                foreach (JProperty o in _Items.Properties())
                {
                    var jv = o.Value;
                    if (o.Value.ToString() == "baseurl")
                    {
                        var url = HttpContext.Current.Request.Url.ToString();//"http://localhost:10553/Ueditor/net/"
                        var idx = url.IndexOf("Ueditor");
                        var sBaseUrl = url.Substring(0, idx);
                        o.Value = sBaseUrl + "Ueditor/net/";
                        //o.Value = "http://localhost:10553/Ueditor/net/";
                    }
                }
            }
            return _Items;
        }
    }

    private static JObject _Items;

    public static T GetValue<T>(string key)
    {
        return Items[key].Value<T>();
    }

    public static String[] GetStringList(string key)
    {
        return Items[key].Select(x => x.Value<String>()).ToArray();
    }

    public static String GetString(string key)
    {
        return GetValue<String>(key);
    }

    public static int GetInt(string key)
    {
        return GetValue<int>(key);
    }
}