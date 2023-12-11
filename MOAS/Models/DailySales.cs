namespace MOAS.Models
{
    public class DailySales
    {
        
       
        public string vbeln { get; set; }
        public string Posnr { get; set; }
        public DateTime FKDat { get; set; }
        public string fkart { get; set; }
        public string empcode { get; set; } 
        public string matnr { get; set; }   
        public string arktx { get; set; }
        public string kunag { get; set; }
        public string name1 { get; set; }

        public string spart { get; set; }   

        public string aubel { get; set; }

        public string zvgbel { get; set; }  
        public string vgpos { get; set; }
        public decimal TotalQty { get;set; }
        public string year_budat { get;set; }
        public string quarter_budat { get; set; }  
        public string month_budat { get; set; }

        public string SearchText
        {
            get
            {
                return ($"{vbeln} {kunag} {name1} {empcode} {matnr} {arktx} {zvgbel} {quarter_budat} {year_budat} ");
            }
        }




    }
}
