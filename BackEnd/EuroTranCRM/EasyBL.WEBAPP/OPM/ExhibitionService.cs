using Aspose.Cells;
using EasyBL.WebApi.Message;
using Entity.Sugar;
using Newtonsoft.Json.Linq;
using SqlSugar;
using SqlSugar.Base;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace EasyBL.WEBAPP.OPM
{
    public class ExhibitionService : ServiceBase
    {
        #region 匯入報價費用項目

        /// <summary>
        /// 匯入報價費用項目
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on GetImportFeeitems</param>
        /// <returns></returns>
        public ResponseMessage GetImportFeeitems(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.DB;
            var tb_FeeItems = new DataTable(@"tb");
            try
            {
                do
                {
                    var sFileId = _fetchString(i_crm, @"FileId");
                    var sFileName = _fetchString(i_crm, @"FileName");
                    var sRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"OutFiles\Temporary\");//Word模版路徑
                    var sfileName = sFileName.Split(new string[] { @"." }, StringSplitOptions.RemoveEmptyEntries);
                    var sSubFileName = sfileName.LastOrDefault();     //副檔名
                    sFileName = sRoot + sFileId + @"." + sSubFileName;
                    tb_FeeItems.Columns.Add(@"guid", typeof(String));//費用代號
                    tb_FeeItems.Columns.Add(@"FinancialCode", typeof(String));//費用代號
                    tb_FeeItems.Columns.Add(@"FinancialCostStatement", typeof(String));//費用說明
                    tb_FeeItems.Columns.Add(@"FinancialCurrency", typeof(String));//幣別
                    tb_FeeItems.Columns.Add(@"FinancialUnitPrice", typeof(String));//單價
                    tb_FeeItems.Columns.Add(@"FinancialNumber", typeof(String));//FinancialNumber
                    tb_FeeItems.Columns.Add(@"FinancialUnit", typeof(String));//單位
                    tb_FeeItems.Columns.Add(@"FinancialAmount", typeof(String));//金額
                    tb_FeeItems.Columns.Add(@"FinancialExchangeRate", typeof(String));//匯率
                    tb_FeeItems.Columns.Add(@"FinancialTWAmount", typeof(String));//台幣金額
                    tb_FeeItems.Columns.Add(@"FinancialTaxRate", typeof(String));//稅率
                    tb_FeeItems.Columns.Add(@"FinancialTax", typeof(String));//稅額
                    tb_FeeItems.Columns.Add(@"Memo", typeof(String));//稅額
                    tb_FeeItems.Columns.Add(@"CreateUser", typeof(String));//創建人
                    tb_FeeItems.Columns.Add(@"CreateDate", typeof(String));//創建時間

                    var book = new Workbook(sFileName);
                    //book.Open(sFileName);
                    var sheet = book.Worksheets[0];
                    var cells = sheet.Cells;
                    var tbFeeItems = cells.ExportDataTableAsString(1, 0, cells.MaxDataRow - 2, cells.MaxDataColumn + 1, false);

                    if (tbFeeItems.Rows.Count > 0)
                    {
                        foreach (DataRow row in tbFeeItems.Rows)
                        {
                            try
                            {
                                var sFinancialCode = row[@"Column2"].ToString();
                                if (IsInt(row[@"Column1"].ToString()) && sFinancialCode != @"")
                                {
                                    var bDefult = sFinancialCode == @"TE001";
                                    var iCount = 0;
                                    if (!bDefult)
                                    {
                                        iCount = db.Queryable<OTB_SYS_Arguments>()
                                            .Count(it => it.ArgumentClassID == @"FeeClass" && it.ArgumentID == sFinancialCode && it.OrgID == i_crm.ORIGID);
                                    }

                                    if (bDefult || iCount > 0)
                                    {
                                        var tb_row = tb_FeeItems.NewRow();
                                        tb_row[@"guid"] = Guid.NewGuid();
                                        tb_row[@"FinancialCode"] = bDefult ? @"TE001" : row[@"Column2"].ToString();
                                        tb_row[@"FinancialCostStatement"] = row[@"Column3"].ToString();
                                        tb_row[@"Memo"] = row[@"Column4"].ToString();
                                        tb_row[@"FinancialCurrency"] = row[@"Column5"].ToString();
                                        tb_row[@"FinancialUnitPrice"] = row[@"Column6"].ToString().Replace(@",", @"");
                                        tb_row[@"FinancialUnitPrice"] = tb_row[@"FinancialUnitPrice"].ToString() == @"" ? @"0" : tb_row[@"FinancialUnitPrice"].ToString().Replace(@",", @"");
                                        tb_row[@"FinancialNumber"] = row[@"Column7"].ToString();
                                        tb_row[@"FinancialUnit"] = row[@"Column8"].ToString();
                                        tb_row[@"FinancialAmount"] = row[@"Column10"].ToString().Replace(@",", @"");
                                        tb_row[@"FinancialAmount"] = tb_row[@"FinancialAmount"].ToString() == @"" ? @"0" : tb_row[@"FinancialAmount"].ToString().Replace(@",", @"");
                                        tb_row[@"FinancialExchangeRate"] = row[@"Column11"].ToString();
                                        tb_row[@"FinancialTWAmount"] = row[@"Column13"].ToString().Replace(@",", @"");
                                        tb_row[@"FinancialTWAmount"] = tb_row[@"FinancialTWAmount"].ToString() == @"" ? @"0" : tb_row[@"FinancialTWAmount"].ToString().Replace(@",", @"");
                                        tb_row[@"FinancialTaxRate"] = row[@"Column14"].ToString();// row["Column1"].ToString();
                                        tb_row[@"FinancialTax"] = decimal.Parse(row[@"Column13"].ToString().Replace(@",", @"")) * decimal.Parse(row[@"Column14"].ToString().Replace(@"%", @"")) / 100;// row["Column1"].ToString();
                                        tb_row[@"FinancialTax"] = tb_row[@"FinancialTax"].ToString() == @"" ? @"0" : tb_row[@"FinancialTax"];
                                        tb_row[@"CreateUser"] = i_crm.USERID ?? @"";
                                        tb_row[@"CreateDate"] = DateTime.Now.ToString(@"yyyy/MM/dd HH:mm:ss");
                                        tb_FeeItems.Rows.Add(tb_row);
                                    }
                                }
                            }
                            catch { }
                        }
                    }
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, tb_FeeItems);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(ExhibitionService), @"展覽管理", @"GetImportFeeitems（匯入報價費用項目）", @"", @"", @"");
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

        #endregion 匯入報價費用項目

        #region 拋轉專案

        /// <summary>
        /// 拋轉專案
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on Transfer</param>
        /// <returns></returns>
        public ResponseMessage Transfer(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            try
            {
                rm = SugarBase.ExecTran(db =>
                {
                    do
                    {
                        var sId = _fetchString(i_crm, @"SN");
                        var oExhibition = db.Queryable<OTB_OPM_Exhibition>().Single(it => it.SN == sId.ObjToInt());
                        if (oExhibition == null)
                        {
                            sMsg = @"系統找不到對應的專案（展覽）資料，請核查！";
                            break;
                        }
                        //更新客戶資料
                        var oExhibitionUpd = new OTB_OPM_Exhibition
                        {
                            IsTransfer = @"Y",
                            LastTransfer_Time = DateTime.Now,
                            ModifyUser = i_crm.USERID,
                            ModifyDate = DateTime.Now
                        };
                        db.Updateable(oExhibitionUpd)
                            .UpdateColumns(it => new { it.IsTransfer, it.LastTransfer_Time, it.ModifyUser, it.ModifyDate })
                            .Where(it => it.SN == sId.ObjToInt()).ExecuteCommand();

                        var oExhibitionsTransferUpd = new OTB_OPM_ExhibitionsTransfer
                        {
                            OrgID = i_crm.ORIGID,
                            PrjNO = oExhibition.ExhibitionCode,
                            PrjName = Common.CutByteString(oExhibition.ExhibitioShotName_TW, 60)
                        };
                        var sCreateUser = oExhibition.CreateUser.Split('.')[0];
                        oExhibitionsTransferUpd.PrjCharger = Common.CutByteString(sCreateUser, 11);
                        oExhibitionsTransferUpd.EndDate = @"";
                        db.Insertable(oExhibitionsTransferUpd).ExecuteCommand();

                        if (i_crm.ORIGID == "TE")
                        {
                            oExhibitionsTransferUpd.OrgID = "TG";
                            db.Insertable(oExhibitionsTransferUpd).ExecuteCommand();
                        } else if (i_crm.ORIGID == "TG")
                        {
                            oExhibitionsTransferUpd.OrgID = "TE";
                            db.Insertable(oExhibitionsTransferUpd).ExecuteCommand();
                        }

                        rm = new SuccessResponseMessage(null, i_crm);
                    } while (false);
                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(ExhibitionService), @"展覽管理", @"Transfer（拋轉專案）", @"", @"", @"");
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

        #endregion 拋轉專案

        #region 刪除帳單明細

        /// <summary>
        /// 刪除帳單明細
        /// </summary>
        /// <param name="i_crm">帳單資料</param>
        /// <returns></returns>
        public ResponseMessage DeleteBillInfo(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var sBillNO = _fetchString(i_crm, @"BillNO");
                    var oBillInfo = db.Queryable<OTB_OPM_BillInfo>()
                        .Single(it => it.OrgID == i_crm.ORIGID && it.BillNO == sBillNO);
                    if (oBillInfo != null)
                    {
                        var sdb = new SimpleClient<OTB_OPM_BillInfo>(SugarBase.DB);
                        var bRes = sdb.DeleteById(oBillInfo.SN);
                    }

                    rm = new SuccessResponseMessage(null, i_crm);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(OtherBusiness_UpdService), @"其他", @"UpdateBillInfo（刪除帳單明細）", @"", @"", @"");
            }
            finally
            {
                if (null != sMsg)
                {
                    rm = new ErrorResponseMessage(sMsg, i_crm);
                }
                Logger.Debug(@"OtherBusiness_UpdService.UpdateBillInfo Debug（Param：" + JsonToString(i_crm) + @"；Response：" + JsonToString(rm) + @"）------------------");
            }
            return rm;
        }

        #endregion 刪除帳單明細

        /// <summary>
        /// </summary>
        /// <param name="sNumber"></param>
        /// <returns></returns>
        public static bool IsInt(string sNumber)
        {
            var objNotNumberPattern = new Regex(@"[^0-9.-]");
            var objTwoDotPattern = new Regex(@"[0-9]*[.][0-9]*[.][0-9]*");
            var objTwoMinusPattern = new Regex(@"[0-9]*[-][0-9]*[-][0-9]*");
            var strValidRealPattern = @"^([-]|[.]|[-.]|[0-9])[0-9]*[.]*[0-9]+$";
            var strValidIntegerPattern = @"^([-]|[0-9])[0-9]*$";
            var objNumberPattern = new Regex(@"(" + strValidRealPattern + @")|(" + strValidIntegerPattern + @")");

            return !objNotNumberPattern.IsMatch(sNumber) &&
                   !objTwoDotPattern.IsMatch(sNumber) &&
                   !objTwoMinusPattern.IsMatch(sNumber) &&
                   objNumberPattern.IsMatch(sNumber);
        }

        /// <summary>
        /// </summary>
        /// <param name="bill"></param>
        /// <returns></returns>
        public static OTB_OPM_BillInfo GetNewBillInfo(JObject bill)
        {
            const string CN_ORGANIZER = "BillOrganizer";
            var oBill = new OTB_OPM_BillInfo
            {
                BillGuid = bill[@"guid"].ToString(),
                BillNO = bill[OTB_OPM_BillInfo.CN_BILLNO].ToString(),
                AuditVal = bill[OTB_OPM_BillInfo.CN_AUDITVAL].ToString(),
                BillCreateDate = bill[OTB_OPM_BillInfo.CN_BILLCREATEDATE].ToString(),
                BillFirstCheckDate = bill[OTB_OPM_BillInfo.CN_BILLFIRSTCHECKDATE] == null ? bill[OTB_OPM_BillInfo.CN_BILLCHECKDATE].ToString() : bill[OTB_OPM_BillInfo.CN_BILLFIRSTCHECKDATE].ToString(),
                BillCheckDate = bill[OTB_OPM_BillInfo.CN_BILLCHECKDATE].ToString(),
                Currency = bill[OTB_OPM_BillInfo.CN_CURRENCY].ToString(),
                ExchangeRate = bill[OTB_OPM_BillInfo.CN_EXCHANGERATE].ToString(),
                Advance = bill[OTB_OPM_BillInfo.CN_ADVANCE].ToString(),
                FeeItems = bill[OTB_OPM_BillInfo.CN_FEEITEMS].ToString(),
                Memo = bill[OTB_OPM_BillInfo.CN_MEMO] == null ? @"" : bill[OTB_OPM_BillInfo.CN_MEMO].ToString(),
                InvoiceNumber = bill[OTB_OPM_BillInfo.CN_INVOICENUMBER].ToString(),
                InvoiceDate = bill[OTB_OPM_BillInfo.CN_INVOICEDATE].ToString(),
                ReceiptNumber = bill[OTB_OPM_BillInfo.CN_RECEIPTNUMBER].ToString(),
                ReceiptDate = bill[OTB_OPM_BillInfo.CN_RECEIPTDATE].ToString(),
                Payer = bill[OTB_OPM_BillInfo.CN_PAYER].ToString(),
                IsRetn = bill[OTB_OPM_BillInfo.CN_ISRETN] == null ? @"N" : bill[OTB_OPM_BillInfo.CN_ISRETN].ToString(),
                Number = bill[OTB_OPM_BillInfo.CN_NUMBER] == null ? @"" : bill[OTB_OPM_BillInfo.CN_NUMBER].ToString(),
                Unit = bill[OTB_OPM_BillInfo.CN_UNIT] == null ? @"" : bill[OTB_OPM_BillInfo.CN_UNIT].ToString(),
                Weight = bill[OTB_OPM_BillInfo.CN_WEIGHT] == null ? @"" : bill[OTB_OPM_BillInfo.CN_WEIGHT].ToString(),
                Volume = bill[OTB_OPM_BillInfo.CN_VOLUME] == null ? @"" : bill[OTB_OPM_BillInfo.CN_VOLUME].ToString(),
                ContactorName = bill[OTB_OPM_BillInfo.CN_CONTACTORNAME] == null ? @"" : bill[OTB_OPM_BillInfo.CN_CONTACTORNAME].ToString(),
                Telephone = bill[OTB_OPM_BillInfo.CN_TELEPHONE] == null ? @"" : bill[OTB_OPM_BillInfo.CN_TELEPHONE].ToString(),
                ReFlow = bill[OTB_OPM_BillInfo.CN_REFLOW] == null ? @"" : bill[OTB_OPM_BillInfo.CN_REFLOW].ToString(),
                Index = bill[OTB_OPM_BillInfo.CN_INDEX] == null ? @"" : bill[OTB_OPM_BillInfo.CN_INDEX].ToString(),
                AmountSum = bill[OTB_OPM_BillInfo.CN_AMOUNTSUM] == null ? @"0" : bill[OTB_OPM_BillInfo.CN_AMOUNTSUM].ToString(),
                TaxSum = bill[OTB_OPM_BillInfo.CN_TAXSUM] == null ? @"0" : bill[OTB_OPM_BillInfo.CN_TAXSUM].ToString(),
                AmountTaxSum = bill[OTB_OPM_BillInfo.CN_AMOUNTTAXSUM] == null ? @"0" : bill[OTB_OPM_BillInfo.CN_AMOUNTTAXSUM].ToString(),
                TotalReceivable = bill[OTB_OPM_BillInfo.CN_TOTALRECEIVABLE] == null ? @"0" : bill[OTB_OPM_BillInfo.CN_TOTALRECEIVABLE].ToString(),
                NotPassReason = bill[OTB_OPM_BillInfo.CN_NOTPASSREASON] == null ? @"" : bill[OTB_OPM_BillInfo.CN_NOTPASSREASON].ToString(),
                BillWriteOffDate = bill[OTB_OPM_BillInfo.CN_BILLWRITEOFFDATE] == null ? @"" : bill[OTB_OPM_BillInfo.CN_BILLWRITEOFFDATE].ToString(),
                Organizer = bill[CN_ORGANIZER] == null ? @"" : bill[CN_ORGANIZER].ToString()
            };
            return oBill;
        }
    }
}