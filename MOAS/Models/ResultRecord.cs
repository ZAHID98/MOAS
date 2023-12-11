using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MOAS.Models
{
    public class ResultRecord
    {
        [Key,Column(Order =0)]
        public long REQ_NO { get; set; }
        [Key,Column(Order =1)]
        public long SAMPLE_NO { get; set; }
        [Key,Column(Order =2)]
        public int VERWMERKM { get; set; }
        public string? KURZTEXT { get; set; }
        public DateTime Erstelldat { get; set; }
        public string? DUMMY10 { get; set; }     
        public string? MERKNR { get; set; }  
        
    
        public decimal Result { get; set; }

       
        public decimal Mresult { get; set; }
        public string? MBEWERTG { get; set; }    
        public string? CHAR_GROUP { get; set; } 
        public string? CHAR_PART3 { get; set; }  
        public string? GRAPH { get; set;}    

    }
}
