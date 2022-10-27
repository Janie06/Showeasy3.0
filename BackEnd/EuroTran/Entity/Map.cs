using System.Collections;

namespace Entity
{
    public class Map : Hashtable
    {
        public void Put(object key, object value)
        {
            if (this.ContainsKey(key)) this.Remove(key);
            this.Add(key, value);
        }

        public object Get(object key)
        {
            if (this.ContainsKey(key))
            {
                return this[key] ?? "";
            };

            return "";
        }

        public void SetParameter(string key, object value)
        {
            if (this.ContainsKey(key)) this.Remove(key);
            this.Add(key, value);
        }
    }
}