using System.Collections.Generic;

namespace EasyBL.WebApi.Message
{
    public class FlexigridResponseMessage : ResponseMessage
    {
        #region Flexigrid

        public class FlexigirdColumn
        {
            public string Display { get; set; }
            public string Name { get; set; }
            public int Width { get; set; }
            public bool Sortable { get; set; }
            public string Align { get; set; }
            public bool Hide { get; set; }

            public FlexigirdColumn()
            {
                Hide = false;
            }

            public static string GetJsonString(FlexigirdColumn i_fc)
            {
                return $"{{ display: '{i_fc.Display}', name: '{i_fc.Name}', width: {i_fc.Width}, sortable: {((i_fc.Sortable) ? "true" : "false")}, align: '{i_fc.Align}',  hide : {((i_fc.Hide) ? "true" : "false")} }}";
            }
        }

        public class FlexigridRow
        {
            public string id;
            public Dictionary<string, string> cell = new Dictionary<string, string>();
        }

        /// <summary>
        /// 目前所在的頁數，從1開始
        /// </summary>
        public int page;

        /// <summary>
        /// 總筆數
        /// </summary>
        public long total;

        public List<FlexigridRow> Rows { get; private set; }

        #endregion Flexigrid

        public FlexigridResponseMessage(RequestMessage i_crm = null)
        {
            RESULT = ResponseResult.RR_TRUE;

            if (null != i_crm)
            {
                PROJECT = i_crm.PROJECT;
                PROJECTVER = i_crm.PROJECTVER;
                TYPE = i_crm.TYPE;
            }

            Rows = new List<FlexigridRow>();
        }
    }
}