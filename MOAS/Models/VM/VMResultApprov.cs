using DevExpress.Office.Utils;
using System.ComponentModel.DataAnnotations;

namespace MOAS.Models.VM
{
    public class VMResultApprov
    {

        public long REQ_NO { get; set; }
        public long SAMPLE_NO { get; set; }
        public long INS_LOT { get; set; }
        public int KUNNR { get; set; }
        public string CutomerName { get; set; }
        public string? EQUIPMENT { get; set; }
        public int ITEM_NO { get; set; }
        public string CUST_ADDRESS { get; set; }
        public string? EQUIPMENT_MAKE { get; set; }
        public string? EQUIPMENT_MODEL { get; set; }
        public string? EQUIPMENT_SERIAL { get; set; }
        public string? OIL_NAME { get; set; }
        public EStatus status { get; set; }
        public decimal EQP_RUN_HOUR { get; set; }
        public decimal OIL_RUN_HOUR { get; set; }
        public decimal DAILY_OIL_TOP { get; set; }
        public decimal FILT_RUN_HOUR { get; set; }
        
        public string? LAST_OILCHNG_DT { get; set; }
     
        public string? SAMPLE_DATE { get; set; }
        
        public string? SAMPLE_REC_DATE { get; set; }
        public decimal SAMP_QTY { get; set; }
        public string? SAMP_UNIT { get; set; }
        public string? REMARKS { get; set; }
        public string? CREATED_BY { get; set; }
        public string? Comments { get; set; }
        public EResultSt? EResultSt { get; set; }
        public DateTime? ApprrovedDt { get; set; }
        public List<ResultRecord> results { get; set; }

     
    }
}
