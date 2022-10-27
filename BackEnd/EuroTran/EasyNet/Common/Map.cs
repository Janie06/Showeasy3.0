using System.Collections;

namespace EasyNet.Common
{
    public class Map : Hashtable
    {
        public void Put(object key, object value)
        {
            if (this.ContainsKey(key)) this.Remove(key);
            this.Add(key, value);
        }

        public void SetParameter(string key, object value)
        {
            if (this.ContainsKey(key)) this.Remove(key);
            this.Add(key, value);
        }
    }
}