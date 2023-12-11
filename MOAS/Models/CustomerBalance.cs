namespace MOAS.Models
{
    public class CustomerBalance
    {
        public int Kunnr { get; set; }
        public string Name1 { get; set; }

        public string CustAddress { get; set; }
        public string EmpID { get; set; }
        public string EMP_Name { get; set; }
      

        public decimal t_curr_bal { get; set; }    
        public string loc_currcy { get; set; }

    }
}
