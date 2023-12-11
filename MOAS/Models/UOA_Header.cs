namespace MOAS.Models
{
    public class UOA_Header
    {


        public long REQ_NO { get; set; }
        public long INS_LOT { get; set; }
        public long KUNNR { get; set; }
        public string CustomerName { get; set; }
        public string EQUIPMENT { get; set; }
        public string CUST_ADDRESS { get; set; }
        public string? EQUIPMENT_MAKE { get; set;}
        public string? EQUIPMENT_MODEL { get; set; }
        public string? EQUIPMENT_SERIAL { get; set; }
        public int OIL_CODE { get; set;}
        public string? OIL_NAME { get; set;} 
       
        public string? UserName { get; set;}
        public DateTime? created_on { get; set;}

        public string? Contact1_Name { get; set;}
        public string? Contact1_Desig { get; set;}   
       public string? Contact1_Phone { get; set;}   
        
      public string? Contact1_Email { get; set;}    
        

    }
}
