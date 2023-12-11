namespace MOAS.Models
{
    public class DailySalesTTY
    {
        public DateTime invdt {  get; set; }  
        public string WorkArea { get; set; }
        public string PCd {  get; set; }
        public string ProductID { get; set; }
        public decimal TP {  get; set; }   
        public string Vat { get; set; }

        public int Sales_Qty { get; set; }
        public decimal Sales_Val { get; set; }
        public int Retn_Qty { get; set; }
        public int ActualSales_Qty {  get; set; }   
        public string DeptCd { get; set; }    
        public int Period { get; set; }
        public string? MsrCd { get; set; }   
        public string Supervisor { get; set;}
        public string SSupervisor { get; set;}
        public string SSSupervisor { get; set; }    
        public string SSSSupervisor {  get; set; }  
      
        public string Company { get; set; }
        public string RegionCode { get; set; }
        public string ZoneCode { get; set; }
        public string SMAreaCode { get; set; }  
        public string GMAreaCode { get; set; }


    }
}
