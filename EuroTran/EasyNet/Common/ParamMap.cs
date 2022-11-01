using EasyNet.DBUtility;
using System;
using System.Collections.Generic;
using System.Data;

namespace EasyNet.Common
{
    public class ParamMap : Map
    {
        private bool isPage;

        public ParamMap()
        {
            isPage = true;
        }

        public string Entity { get; set; }

        public string OrderFields { get; set; }

        public string OrderType { get; set; } = "Desc";

        public bool ParamNull { get; set; } = true;

        public int PageIndex { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public bool IsDesc { get; set; } = true;

        public static ParamMap NewMap()
        {
            return new ParamMap();
        }

        public bool IsPage
        {
            get
            {
                return isPage;
            }
        }

        public int PageOffset
        {
            get
            {
                var pageIndex = this.PageIndex;
                var pageSize = this.PageSize;
                if (pageIndex <= 0) pageIndex = 1;
                if (pageSize <= 0) pageSize = 1;

                return (pageIndex - 1) * pageSize;
            }
        }

        public int PageLimit
        {
            get
            {
                return this.PageSize;
            }
        }

        public int GetInt(string key)
        {
            var value = this[key];
            return Convert.ToInt32(value);
        }

        public String GetString(string key)
        {
            var value = this[key];
            return Convert.ToString(value);
        }

        public Double ToDouble(string key)
        {
            var value = this[key];
            return Convert.ToDouble(value);
        }

        public Int64 ToLong(string key)
        {
            var value = this[key];
            return Convert.ToInt64(value);
        }

        public Decimal ToDecimal(string key)
        {
            var value = this[key];
            return Convert.ToDecimal(value);
        }

        public DateTime ToDateTime(string key)
        {
            var value = this[key];
            return Convert.ToDateTime(value);
        }

        public void SetOrderFields(string orderFields, bool isDesc, string orderType)
        {
            this.OrderFields = orderFields;
            this.IsDesc = isDesc;
            this.OrderType = orderType;
        }

        /// <summary>
        /// 分頁參數設置
        /// </summary>
        /// <param name="page">第幾頁，從0開始</param>
        /// <param name="limit">每頁最多顯示幾條資料</param>
        public void SetPageParamters()
        {
            SetPages();
        }

        /// <summary>
        /// 分頁參數設置
        /// </summary>
        /// <param name="opm">todo: describe opm parameter on SetPageParamters</param>
        public void SetPageParamters(Object opm)
        {
            SetParamters(opm);
            SetPages();
        }

        /// <summary>
        /// 分頁參數設置
        /// </summary>
        /// <param name="opm">參數隊列</param>
        public void Paramters(Object opm)
        {
            SetParamters(opm);
        }

        /// <summary>
        /// 分頁參數設置
        /// </summary>
        /// <param name="opm">參數隊列</param>
        public void ParamtersForPrc(Object opm)
        {
            if (opm is Dictionary<string, object>)
            {
                var dic = opm as Dictionary<string, object>;
                if (dic.Keys.Count > 0)
                {
                    switch (AdoHelper.DbType)
                    {
                        case DatabaseType.MYSQL:

                            break;

                        case DatabaseType.SQLSERVER:
                            foreach (string key in dic.Keys)
                            {
                                this.Add(key, dic[key]);
                            }
                            break;

                        case DatabaseType.ACCESS:

                            break;

                        default:
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// 分頁參數設置
        /// </summary>
        /// <param name="page">第幾頁，從0開始</param>
        /// <param name="limit">每頁最多顯示幾條資料</param>
        public void SetPageParamters(int page, int limit)
        {
            this.PageIndex = page;
            this.PageIndex = limit;
            SetPages();
        }

        private void SetPages()
        {
            this.isPage = true;
            switch (AdoHelper.DbType)
            {
                case DatabaseType.MYSQL:

                    this["offset"] = this.PageOffset;
                    this["limit"] = this.PageLimit;

                    break;

                case DatabaseType.SQLSERVER:

                    var pageIndex = this.PageIndex;
                    var pageSize = this.PageSize;
                    if (pageIndex <= 0) pageIndex = 1;
                    if (pageSize <= 0) pageSize = 1;

                    this["pageStart"] = (pageIndex - 1) * pageSize + 1;
                    this["pageEnd"] = pageIndex * pageSize;

                    break;

                case DatabaseType.ACCESS:

                    var pageIndex_ac = this.PageIndex;
                    var pageSize_ac = this.PageSize;

                    this["offset"] = pageIndex_ac * pageSize_ac;
                    this["limit"] = pageSize_ac;

                    break;

                default:
                    break;
            }

            //int start = (pageIndex-1) * pageSize + 1;
            //int end = pageIndex * pageSize;
        }

        private void SetParamters(Object p)
        {
            if (p is Dictionary<string, object>)
            {
                var dic = p as Dictionary<string, object>;
                if (dic.Keys.Count > 0)
                {
                    this.isPage = true;
                    switch (AdoHelper.DbType)
                    {
                        case DatabaseType.MYSQL:

                            break;

                        case DatabaseType.SQLSERVER:
                            foreach (string key in dic.Keys)
                            {
                                switch (key)
                                {
                                    case nameof(Entity):
                                        {
                                            this.Entity = dic[key].ToString();
                                            break;
                                        }
                                    case nameof(PageIndex):
                                        {
                                            this.PageIndex = Convert.ToInt32(dic[key]);
                                            break;
                                        }
                                    case nameof(PageSize):
                                        {
                                            this.PageSize = Convert.ToInt32(dic[key]);
                                            break;
                                        }
                                    case nameof(OrderFields):
                                        {
                                            this.OrderFields = dic[key].ToString();
                                            break;
                                        }
                                    case nameof(OrderType):
                                        {
                                            this.OrderType = dic[key].ToString();
                                            break;
                                        }
                                    case nameof(IsDesc):
                                        {
                                            this.IsDesc = (Boolean)dic[key];
                                            break;
                                        }
                                    default:
                                        {
                                            if (key.StartsWith("ISBLANK_") || key.StartsWith("ISNULL_"))
                                            {
                                                this.Add(key, dic[key]);
                                            }
                                            else if (dic[key] != null && dic[key].ToString() != "")
                                            {
                                                this.Add(key, dic[key]);
                                            }
                                            break;
                                        }
                                }
                            }
                            break;

                        case DatabaseType.ACCESS:

                            break;
                    }
                }
            }
        }

        public IDbDataParameter[] ToDbParameters()
        {
            var i = 0;
            var paramArr = DbFactory.CreateDbParameters(this.Keys.Count);
            foreach (string key in this.Keys)
            {
                if (!string.IsNullOrEmpty(key.Trim()))
                {
                    var value = this[key];
                    if (value == null) value = DBNull.Value;
                    paramArr[i].ParameterName = key;
                    paramArr[i].Value = value;
                    i++;
                }
            }

            return paramArr;
        }
    }
}