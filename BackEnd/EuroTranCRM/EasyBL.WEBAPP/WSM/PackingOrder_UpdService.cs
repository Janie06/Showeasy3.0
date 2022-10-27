using EasyBL.WebApi.Message;
using Entity.Sugar;
using Entity.ViewModels;
using SqlSugar.Base;
using System;
using System.Linq;

namespace EasyBL.WEBAPP.WSM
{
    public class PackingOrder_UpdService : ServiceBase
    {
        #region 依據展覽獲取展覽報價規則

        /// <summary>
        /// 依據展覽獲取展覽報價規則
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage GetExhibitionRules(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var iId = _fetchInt(i_crm, "Id");

                    var oRules = db.Queryable<CusExhibitionRules, OTB_OPM_Exhibition>((t1, t2) => t1.Guid == t2.CostRulesId)
                        .Where((t1, t2) => t2.SN == iId)
                        .Select((t1, t2) => new CusExhibitionRules
                        {
                            Guid = t1.Guid,
                            Title = t1.Title,
                            ExhibitionCode = t2.ExhibitionCode,
                            CostRules = t1.CostRules,
                            PackingPrice = t1.PackingPrice,
                            FeedingPrice = t1.FeedingPrice,
                            StoragePrice = t1.StoragePrice,
                            CostInstruction = t1.CostInstruction,
                            Currency = t1.Currency,
                        }).Single();

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, oRules);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.WEBAPP.WSM.PackingOrder_UpdService", "", "GetExhibitionRules（依據展覽獲取展覽報價規則）", "", "", "");
            }
            finally
            {
                if (null != sMsg)
                {
                    rm = new ErrorResponseMessage(sMsg, i_crm);
                }
            }
            return rm;
        }

        #endregion 依據展覽獲取展覽報價規則

        #region 設置匯入廠商下拉單

        /// <summary>
        /// 設置匯入廠商下拉單
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage SetImpCusDrop(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var sId = _fetchString(i_crm, "Id");
                    //var saImportCustomers = db.Queryable<OTB_CRM_ImportCustomers>()
                    //    .Select(x => new { x.guid, x.CustomerCName, x.ExhibitionNO })
                    //    .Where(x => x.ExhibitionNO == iId).ToList();
                    var saImportCustomers = db.Queryable<OTB_OPM_ExhibitionCustomers, OTB_CRM_Customers>(
                            (t1, t2) => t1.CustomerId == t2.guid
                        )
                        .Select((t1,t2) => new { t2.guid, t2.CustomerCName, t1.ExhibitionNO })
                        .Where(t1 => t1.ExhibitionNO == sId).ToList();

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, saImportCustomers);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.WEBAPP.WSM.PackingOrder_UpdService", "", "SetImpCusDrop（設置匯入廠商下拉單）", "", "", "");
            }
            finally
            {
                if (null != sMsg)
                {
                    rm = new ErrorResponseMessage(sMsg, i_crm);
                }
            }
            return rm;
        }

        #endregion 設置匯入廠商下拉單

        #region 獲取匯入廠商資料

        /// <summary>
        /// 獲取匯入廠商資料
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage GetImpCusData(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var sId = _fetchString(i_crm, "Id");
                    var sExhibitionNO = _fetchString(i_crm, "ExhibitionNO");
                    //var oImportCustomers = db.Queryable<OTB_CRM_ImportCustomers>().Single(x => x.OrgID == i_crm.ORIGID && x.guid == sId);

                    var oExhibitionCustomers = db.Queryable<OTB_OPM_ExhibitionCustomers, OTB_CRM_Customers>(
                            (t1, t2) => t1.CustomerId == t2.guid
                        )
                        .Select((t1, t2) => new { t1.ExhibitionNO, t1.BoothNumber, t1.CustomerId, t2.UniCode, Contactor = "", t2.Email, t2.Telephone, t2.EXT })
                        .Where(t1 => t1.ExhibitionNO == sExhibitionNO && t1.CustomerId == sId).Single();

                    View_CRM_ImportCustomers oImportCustomers = new View_CRM_ImportCustomers();

                    oImportCustomers.MuseumMumber = oExhibitionCustomers.BoothNumber;
                    oImportCustomers.UniCode = oExhibitionCustomers.UniCode;
                    oImportCustomers.Contactor = "";
                    oImportCustomers.Email = "";
                    oImportCustomers.Telephone = "";
                    oImportCustomers.Ext = "";

                    if (oExhibitionCustomers != null)
                    {
                        var listContactor = db.Queryable<OTB_OPM_ExhibitionContactors, OTB_CRM_Contactors>((t1, t2) => t1.ContactorId==t2.guid)
                           .Select((t1, t2) => new { t1.ExhibitionNO, t1.CustomerId, t2.ContactorName, t2.Email1, t2.Telephone1, t2.Ext1, t1.IsMain, t2.OrderByValue }).MergeTable()
                           .Where(t1 => t1.ExhibitionNO == sExhibitionNO && t1.CustomerId == sId).OrderBy("OrderByValue").ToList();

                        if (listContactor.Count > 0)
                        {
                            var listMain = listContactor.Where(t => t.IsMain == "Y").ToList();
                            if (listMain.Count == 0)
                            {
                                listMain = listContactor;
                            }
                            oImportCustomers.Contactor = listMain[0].ContactorName;
                            oImportCustomers.Email = listMain[0].Email1;
                            oImportCustomers.Telephone = listMain[0].Telephone1;
                            oImportCustomers.Ext = listMain[0].Ext1;
                        }
                    }

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, oImportCustomers);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.WEBAPP.WSM.PackingOrder_UpdService", "", "GetImpCusData（獲取匯入廠商資料）", "", "", "");
            }
            finally
            {
                if (null != sMsg)
                {
                    rm = new ErrorResponseMessage(sMsg, i_crm);
                }
            }
            return rm;
        }

        #endregion 獲取匯入廠商資料

        #region 對應匯入廠商

        /// <summary>
        /// 對應匯入廠商
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage CorrespondImpCus(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var sId = _fetchString(i_crm, "Id");
                    var sCustomerId = _fetchString(i_crm, "CustomerId");
                    //var oImportCustomers = db.Queryable<OTB_CRM_ImportCustomers>().Single(x => x.OrgID == i_crm.ORIGID && x.guid == sCustomerId);
                    var oImportCustomers = db.Queryable<OTB_CRM_Customers>().Single(x => x.guid == sCustomerId);
                    var iRel = db.Updateable(
                        new OTB_WSM_PackingOrder
                        {
                            CustomerId = sCustomerId,
                            CompName = oImportCustomers.CustomerCName,
                            Unicode = oImportCustomers.UniCode
                        }
                        )
                        .UpdateColumns(x => new { x.CustomerId, x.CompName })
                        .Where(x => x.OrgID == i_crm.ORIGID && x.AppointNO == sId).ExecuteCommand();

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, iRel > 0 ? true : false);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.WEBAPP.WSM.PackingOrder_UpdService", "", "CorrespondImpCus（對應匯入廠商）", "", "", "");
            }
            finally
            {
                if (null != sMsg)
                {
                    rm = new ErrorResponseMessage(sMsg, i_crm);
                }
            }
            return rm;
        }

        #endregion 對應匯入廠商
    }

    public class CusExhibitionRules : OTB_WSM_ExhibitionRules
    {
        public CusExhibitionRules()
        {
            ExhibitionCode = "";
        }

        public string ExhibitionCode { get; set; }
    }
}