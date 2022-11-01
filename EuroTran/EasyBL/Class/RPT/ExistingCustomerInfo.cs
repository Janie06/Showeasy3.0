using SqlSugar;
using System;
using System.Collections.Generic;

namespace EasyBL
{
    public class ExistingCustomerInfo
    {
        public string GUID { set; get; }
        //客戶中文名稱	客戶英文名稱	統一編號	交易型態	國家	參展次數	聯絡人	職稱	電話	EMAIL	地址	網址
        public string ChName { set; get; }
        public string EnName { set; get; }
        public string TaxNumber { set; get; }
        public string TransType { set; get; }
        public string State { set; get; }
        public int AttendeeTimes { set; get; }
        public string FullName { set; get; }
        public string JobtitleName { set; get; }
        public string TEL { set; get; }
        public string Email { set; get; }
        public string Address { set; get; }
        public string Website { set; get; }

        public List<string> AttendeeExhibitions { set; get; }
    }
}