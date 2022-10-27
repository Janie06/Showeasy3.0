using Entity.Sugar;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyBL
{
    public class SuperAuditor
    {
        public static bool CheckAuditable()
        {
            return false;
        }

        public static OTB_EIP_InvoiceApplyInfo AgreeAll(string strCheckFlows, string SuperAuditor, string Opinion, out List<string> Notification)
        {
            Notification = new List<string>();
            var CheckFlows = (JArray)JsonConvert.DeserializeObject(strCheckFlows);
            foreach (var flowInfo in CheckFlows)
            {
                var SignedDecision = flowInfo["SignedDecision"].ObjToString();
                var SignedOpinion = flowInfo["SignedOpinion"].ObjToString();
                var SignedDate = flowInfo["SignedDate"].ObjToString();
                bool NotSigned = string.IsNullOrWhiteSpace(SignedDecision) || string.IsNullOrWhiteSpace(SignedOpinion) && string.IsNullOrWhiteSpace(SignedDate);
                if (NotSigned)
                {
                    flowInfo["SignedOpinion"] = SuperAuditor + "(簽核全部)。意見:" + Opinion;
                    flowInfo["SignedDate"] = DateTime.Now.ToString("s");
                    var SignedWay = flowInfo["SignedWay"].ObjToString();
                    switch (SignedWay)
                    {
                        case "flow1":
                        case "flow2":
                        case "flow3":
                            {
                                flowInfo["SignedDecision"] = "Y";
                            }
                            break;
                        case "flow4":
                            {
                                var SignedId = flowInfo["SignedId"].ObjToString();
                                if(!string.IsNullOrWhiteSpace(SignedId))
                                {
                                    Notification.Add(SignedId);
                                }
                                flowInfo["SignedDecision"] = "R";
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            var ModifiedCheckFlows = JsonConvert.SerializeObject(CheckFlows);

            return new OTB_EIP_InvoiceApplyInfo() { Status = "E", CheckFlows = ModifiedCheckFlows, ModifyDate = DateTime.Now, ModifyUser = SuperAuditor };
        }

        public static OTB_EIP_InvoiceApplyInfo Disagree(string strCheckFlows, string SuperAuditor, string Opinion)
        {

            var CheckFlows = (JArray)JsonConvert.DeserializeObject(strCheckFlows);

            foreach (var flowInfo in CheckFlows)
            {
                var SignedDecision = flowInfo["SignedDecision"].ObjToString();
                var SignedOpinion = flowInfo["SignedOpinion"].ObjToString();
                var SignedDate = flowInfo["SignedDate"].ObjToString();
                bool NotSigned = string.IsNullOrWhiteSpace(SignedDecision) || string.IsNullOrWhiteSpace(SignedOpinion) && string.IsNullOrWhiteSpace(SignedDate);
                if (NotSigned)
                {
                    flowInfo["SignedOpinion"] = SuperAuditor + "(簽核全部)。意見:" + Opinion;
                    flowInfo["SignedDate"] = DateTime.Now.ToString("s");
                    var SignedWay = flowInfo["SignedWay"].ObjToString();
                    switch (SignedWay)
                    {
                        case "flow1":
                        case "flow2":
                        case "flow3":
                            {
                                flowInfo["SignedDecision"] = "N";
                            }
                            break;
                        case "flow4":
                            {
                                //flowInfo.SignedDecision = "R";
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            var ModifiedCheckFlows = JsonConvert.SerializeObject(CheckFlows);
            return new OTB_EIP_InvoiceApplyInfo() { Status = "D-O", CheckFlows = ModifiedCheckFlows, ModifyDate = DateTime.Now, ModifyUser = SuperAuditor };
        }

        public static Tuple<string,string> GetNotification(OTB_EIP_InvoiceApplyInfo oEip, OTB_SYS_Members Auditor, OTB_SYS_Members Applicant, bool Decision = true)
        {
            var PayeeType = GetPayeeName(oEip.PayeeType);
            var AuditorNotification = Auditor.MemberName + @"審批了" + Applicant.MemberName + "的"+ PayeeType + "「" + oEip.KeyNote + @"」簽呈編號：" + oEip.SignedNumber;
            var AuditorDecision = "審批結果：同意";
            if (!Decision)
                AuditorDecision = "審批結果：不同意";
            return new Tuple<string, string>(AuditorNotification, AuditorDecision);
        }
        

        private static string GetPayeeName(string PayeeType)
        {
            var Result = "";
            switch (PayeeType)
            {
                case "P":
                    Result = "請款單（客戶）";
                    break;
                case "C":
                    Result = "請款單（廠商）";
                    break;
                case "B":
                    Result = "帳單更改申請單";
                    break;
                default:
                    break;
            }
            return Result;
        }
    }
}
