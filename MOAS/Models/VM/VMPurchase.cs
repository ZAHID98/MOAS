using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MOAS.Models
{
    public class VMSales
    {
        [JsonProperty("EMPID")]
        public string EMPID { get; set; }
        [JsonProperty("FKDAT")]
        public DateTime FKDAT { get; set; }
        [JsonProperty("KUNNR")]
        public string KUNNR { get; set; }
        [JsonProperty("FKIMG")]
        public decimal FKIMG { get; set; }
       

    }
}