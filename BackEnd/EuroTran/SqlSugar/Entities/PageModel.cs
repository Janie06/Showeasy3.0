using System;

namespace SqlSugar
{
    public class PageModel
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int Total { get; set; }
        public Object DataList { get; set; }
    }
}