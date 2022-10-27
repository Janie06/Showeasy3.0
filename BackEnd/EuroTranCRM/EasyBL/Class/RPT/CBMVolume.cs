
using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyBL
{
    public class CbmVolume
    {
        public string OrgID { set; get; }

        public string ParentID { set; get; }

        public string BillNO { set; get; }

        public string sVolumes { set; get; }
        public double Volumes
        {
            get
            {
                if (double.TryParse(sVolumes, out double Result))
                    return Result;
                else
                    return 0;
            }
        }

        public string IsReturn { set; get; }

        public static decimal GetCBMPercent(List<CbmVolume> feeItems, string AllocatedBillNO)
        {
            if (feeItems.Count == 1)
                return 1;
            var CBMPercent = decimal.Zero;
            var CBMUsed = feeItems.FirstOrDefault(c => c.BillNO == AllocatedBillNO);
            var CBMTotal = feeItems.Sum(c => c.Volumes);
            if (CBMUsed != null && CBMTotal >0)
                CBMPercent = Convert.ToDecimal(CBMUsed.Volumes / feeItems.Sum(c => c.Volumes));
            return CBMPercent;
        }

        public string ReFlow { set; get; }
    }
}