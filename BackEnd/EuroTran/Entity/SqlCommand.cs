namespace Entity
{
    public class SqlCommand
    {
        public static string GetSqlCommand(string type)
        {
            var sSql = "";
            switch (type.ToLower())
            {
                case "importexhibition_getlist":

                    sSql = "OSP_OTB_OPM_ImportExhibition_GetList";
                    break;

                case "exportexhibition_getlist":

                    sSql = "OSP_OTB_OPM_ExportExhibition_GetList";
                    break;

                case "otherbusiness_getlist":

                    sSql = "OSP_OTB_OPM_OtherBusiness_GetList";
                    break;

                case "otherexhibitiontg_getlist":

                    sSql = "OSP_OTB_OPM_OtherExhibitionTG_GetList";
                    break;

                case "billstatus_getlist":

                    sSql = "OSP_OTB_OPM_BillStatus_GetList";
                    break;

                case "wenzhong_getlist":

                    sSql = "OSP_OTB_EIP_WenZhong_GetList";
                    break;

                default:
                    break;
            }

            return sSql;
        }
    }
}