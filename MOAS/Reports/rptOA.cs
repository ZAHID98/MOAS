using DevExpress.XtraReports.UI;
using MOAS.Models.VM;
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;

namespace MOAS.Reports
{
    public partial class rptOA : DevExpress.XtraReports.UI.XtraReport
    {
        private VMResultData objdt { get; set; }
        public rptOA( VMResultData dt )
        {

            InitializeComponent();
            objdt = dt;

           objUOA.DataSource = dt;
        }

        private void xrSubreport1_BeforePrint(object sender, CancelEventArgs e)
        {
            ((XRSubreport)sender).ReportSource.DataSource = objdt.ItemDatas.ToList();
           
        }

        private void xrSubreport2_BeforePrint(object sender, CancelEventArgs e)
        {
            
            ((XRSubreport)sender).ReportSource.DataSource = objdt.A_Datas.ToList();
        }

        private void xrSubreport3_BeforePrint(object sender, CancelEventArgs e)
        {

            ((XRSubreport)sender).ReportSource.DataSource = objdt.B_Datas.ToList();
        }

        private void xrSubreport4_BeforePrint(object sender, CancelEventArgs e)
        {

            ((XRSubreport)sender).ReportSource.DataSource = objdt.C_Datas.ToList();
        }
    }
}
