using System.ComponentModel.DataAnnotations;
using System.Numerics;

namespace MOAS.Models.VM
{
    public class VMOrderData
    {
   

        public int MANDT { get; set; }
        public long APP_REF_NO { get; set; }    
        public string MATERIAL { get; set; }
        public string SalesOrg { get; set; }    
        public string DISTR_CHAN { get; set; }  
        public string DIVISION { get; set; }    
        public string SO_TYPE { get; set; }
        public string CUSTOMER { get; set; }   
        public decimal QUANTITY { get; set; }   
        public string SU {get; set; }
        public string PLANT { get; set;}
        public string ROUTE { get; set; }
        public DateTime DELVY_DATE { get; set; }       
        public string ZZTER { get; set; }
        public DateTime CREATED_ON { get; set; }
        public string SO_NUM { get; set; }
        public string DEL_IND { get; set;}

    }
}
