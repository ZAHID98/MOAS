using MOAS.Models;

namespace MOAS.Models
{

    using Microsoft.AspNetCore.Mvc.Rendering;
    using MOAS.Repositories;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Numerics;

    public class UOA_Item
    {

        [Key, Column(Order = 0)]
        public long REQ_NO { get; set; }
        [Key, Column(Order = 1)]
        public long SAMPLE_NO { get; set; }
        public long INS_LOT { get; set; }
        public long KUNNR { get; set; }
        public string CutomerName{get;set;}
        public string? EQUIPMENT { get; set; } 
        public int ITEM_NO { get; set;}

        public EStatus status { get; set; }
        public decimal EQP_RUN_HOUR { get; set;}    
        public decimal OIL_RUN_HOUR { get;set;}
        public decimal DAILY_OIL_TOP { get; set; }
        public decimal FILT_RUN_HOUR { get;set; }
        public DateTime? LAST_OILCHNG_DT { get; set; }
        public DateTime? SAMPLE_DATE {get; set;}
        public DateTime? SAMPLE_REC_DATE { get; set;}
        public decimal SAMP_QTY { get; set;}    
        public string? SAMP_UNIT { get;set;}    
        public string? REMARKS { get; set;}
        public string? CREATED_BY { get; set;}
        public string? Comments { get; set; } 
        public EResultSt? EResultSt { get; set; }    
        public DateTime? ApprrovedDt { get; set; }       

        //public DateTime CREATED_ON { get; set;}


        public string SearchText
        {
            get
            {
                return ($"{KUNNR} {EQUIPMENT}{CutomerName} {REQ_NO} {SAMPLE_NO}");
            }
        }

    }
}
