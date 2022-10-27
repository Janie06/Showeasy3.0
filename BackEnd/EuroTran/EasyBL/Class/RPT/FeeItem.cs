
using System.Collections.Generic;

namespace EasyBL
{
    public class FeeItem
    {
        public string FinancialCode { set; get; }
        public decimal OriginalCurrencyAmount { set; get; }
        public decimal TWAmount { set; get; }
        public List<string> AllocatedToBillNOs { get; set; }
        public FeeItem()
        {
            AllocatedToBillNOs = new List<string>();
            OriginalCurrencyAmount = decimal.Zero;
            TWAmount = decimal.Zero;
        }
    }
}