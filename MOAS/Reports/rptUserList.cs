using MOAS.Models;
using DevExpress.XtraReports.UI;
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;

namespace MOAS.Reports
{
    public partial class rptUserList : DevExpress.XtraReports.UI.XtraReport
    {
        public rptUserList(IEnumerable<User> list)
        {
            InitializeComponent();
            objUser.DataSource= list.ToList();
        }
    }
}
