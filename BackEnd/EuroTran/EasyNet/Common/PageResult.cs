using System;
using System.Collections.Generic;

namespace EasyNet.Common
{
    public class PageResult
    {
        /// <summary>
        /// 分页查询中总记录数
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// 分页查询中结果集合
        /// </summary>
        public Object DataList { get; set; }
    }

    public class PageResult<T>
    {
        /// <summary>
        /// 分页查询中总记录数
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// 分页查询中结果集合
        /// </summary>
        public List<T> DataList { get; set; }
    }
}