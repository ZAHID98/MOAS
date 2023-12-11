using DevExpress.XtraReports.UI;
using MOAS.Models.VM;
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;

namespace MOAS.Reports
{
    public partial class rptUOAByDateRange : DevExpress.XtraReports.UI.XtraReport
    {

        private VMResultData objdta { get; set; }
        public rptUOAByDateRange( DateTime Start, DateTime End,  VMResultData dt)
        {
            InitializeComponent();
            objdt.DataSource = dt;
            objdta = dt;
            this.Parameters["prmFromDate"].Value = Start;
            this.Parameters["prmToDate"].Value = End;
        }


        private void xrSubreport1_BeforePrint(object sender, CancelEventArgs e)
        {
            ((XRSubreport)sender).ReportSource.DataSource = objdta.ItemDatas.ToList();

        }

        private void xrSubreport2_BeforePrint(object sender, CancelEventArgs e)
        {
            ((XRSubreport)sender).ReportSource.DataSource = objdta.A_Datas.ToList();

        }


        private void xrSubreport3_BeforePrint(object sender, CancelEventArgs e)
        {
            ((XRSubreport)sender).ReportSource.DataSource = objdta.B_Datas.ToList();

        }


        private void xrSubreport4_BeforePrint(object sender, CancelEventArgs e)
        {
            ((XRSubreport)sender).ReportSource.DataSource = objdta.C_Datas.ToList();

        }

    }
}
