using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MOAS.Models
{
    public class VMReportParameter
    {

         [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }
        [Display(Name = "Metting Room Name")]
        public int? MID { get; set; }
        
        [Display(Name="Format")]
        public EReportTypes ReportType { get; set; }

        public VMReportParameter()
        {
            StartDate = DateTime.Today;
            EndDate = DateTime.Today;
        }
    }

    public class ReportParameter
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public long CID { get; set; }
        public int EquipmentID { set; get; }
        public string? SLNo { get; set; }
        public String? Model { get; set; }
        public int ReportID { get; set; }


    }

    





    

    



    

    public class ReportSizing
    {
        public string TopMargin { get; set; }
        public string LeftMargin { get; set; }
        public string RightMargin { get; set; }
        public string BottomMargin { get; set; }
        public string PageWidth { get; set; }
        public string PageHeight { get; set; }

        public ReportSizing()
        {
            TopMargin = "0.3in";
            RightMargin = "0.3in";
            BottomMargin = "0.3in";
            LeftMargin = "0.3in";
            TurnToPotrait();
        }

        public void TurnToPotrait()
        {
            PageHeight = "11.69in";
            PageWidth = "8.27in";
        }

        public void TurnToLandscape()
        {
            PageHeight = "8.27in";
            PageWidth = "11.69in";
        }
    }
}
    
