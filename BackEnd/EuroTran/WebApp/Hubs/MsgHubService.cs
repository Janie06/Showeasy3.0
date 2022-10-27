using Entity.Sugar;
using Newtonsoft.Json;
using SqlSugar;
using SqlSugar.Base;
using System.Collections.Generic;

namespace WebApp.Hubs
{
    public class MsgHubService
    {
        /// <summary>
        /// </summary>
        /// <param name="sOrgId">todo: describe sOrgId parameter on GetBillsString</param>
        /// <param name="sId">todo: describe sId parameter on GetBillsString</param>
        /// <returns></returns>
        public static string GetBillsString(string sOrgId, string sId)
        {
            var sBills = "";
            var db = SugarBase.DB;
            var saBills = db.Queryable<OTB_OPM_Bills>()
                .Where(it => it.OrgID == sOrgId)
                .WhereIF(sId != "", it => it.BillNO == sId).ToList();
            sBills = JsonConvert.SerializeObject(saBills, Formatting.Indented);
            return sBills;
        }

        /// <summary>
        /// </summary>
        /// <param name="sOrgId">todo: describe sOrgId parameter on GetCustomersString</param>
        /// <param name="sId">todo: describe sId parameter on GetCustomersString</param>
        /// <returns></returns>
        public static string GetCustomersString(string sOrgId, string sId)
        {
            var sCustomers = "";
            var db = SugarBase.DB;
            var saCustomers = db.Queryable<OTB_CRM_CustomersTransfer>()
                .Where(it => it.OrgID == sOrgId)
                .WhereIF(sId != "", it => it.Feild01 == sId).ToList();
            sCustomers = JsonConvert.SerializeObject(saCustomers, Formatting.Indented);
            return sCustomers;
        }

        /// <summary> </summary> <param name="sOrgId">todo: describe sOrgId parameter on
        /// GetExhibitionsString</param> <param name="sId">todo: describe sId parameter on
        /// GetExhibitionsString</param> <returns></ returns>
        public static string GetExhibitionsString(string sOrgId, string sId)
        {
            var sExhibitions = "";
            var db = SugarBase.DB;
            var saExhibitions = db.Queryable<OTB_OPM_ExhibitionsTransfer>()
                .Where(it => it.OrgID == sOrgId)
                .WhereIF(sId != "", it => it.PrjNO == sId).ToList();
            sExhibitions = JsonConvert.SerializeObject(saExhibitions, Formatting.Indented);
            return sExhibitions;
        }

        /// <summary>
        /// </summary>
        /// <param name="data">todo: describe data parameter on RemoveBills</param>
        /// <returns></returns>
        public static void RemoveBills(string data)
        {
            var listBills = JsonConvert.DeserializeObject<List<OTB_OPM_Bills>>(data);
            var db = SugarBase.DB;
            var sdb = new SimpleClient<OTB_OPM_Bills>(db);
            var iRel = db.Deleteable(listBills).ExecuteCommand();
        }

        /// <summary>
        /// </summary>
        /// <param name="data">todo: describe data parameter on RemoveCustomers</param>
        /// <returns></returns>
        public static void RemoveCustomers(string data)
        {
            var listCustomers = JsonConvert.DeserializeObject<List<OTB_CRM_CustomersTransfer>>(data);
            var db = SugarBase.DB;
            var sdb = new SimpleClient<OTB_CRM_CustomersTransfer>(db);
            var iRel = db.Deleteable(listCustomers).ExecuteCommand();
        }

        /// <summary>
        /// </summary>
        /// <param name="data">todo: describe data parameter on RemoveExhibitions</param>
        /// <returns></returns>
        public static void RemoveExhibitions(string data)
        {
            var listExhibitions = JsonConvert.DeserializeObject<List<OTB_OPM_ExhibitionsTransfer>>(data);
            var db = SugarBase.DB;
            var sdb = new SimpleClient<OTB_OPM_ExhibitionsTransfer>(db);
            var iRel = db.Deleteable(listExhibitions).ExecuteCommand();
        }
    }
}