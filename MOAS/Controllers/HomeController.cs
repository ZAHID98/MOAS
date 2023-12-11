using MOAS.Interfaces;
using MOAS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Diagnostics;
using MOAS.Models.VM;
using DevExpress.Xpo;
using DevExpress.PivotGrid.OLAP.Mdx;

namespace MOAS.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private ISetupRepository setup { get; set; }
        public HomeController(ILogger<HomeController> logger, ISetupRepository _setup)
        {
            _logger = logger;
            setup = _setup;
        }
        [Authorize]
        public IActionResult Index()
        {
            return View();
        }
        [Authorize]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }



        [Authorize]
        public ActionResult MissingOrderData ()
        {
            var dt = DateTime.Now;
            var list = setup.GetOrderList(dt);
            return View(list);
        }

        [Authorize]
        public ActionResult getSalesTTY()
        {
            var dt = DateTime.Now;
            var list = setup.GetAllDailySalesTTY();

                //setup.GetDailySalesTTY().ToList();

            //var tobeadded = (from row in punchlog
            //                 where !(from prow in Gu.GetPuchAll() select prow.id).Contains(row.id)
            //                 select row).ToList();

            //foreach (var plog in list)
            //{


            //    var _plog = new DailySalesTTY();

            //    _plog.invdt = plog.invdt;
            //    _plog.DeptCd = plog.DeptCd;
            //    _plog.Period = plog.Period;
            //    _plog.ProductID = plog.ProductID;
            //    _plog.Sales_Qty = plog.Sales_Qty;
            //    _plog.ActualSales_Qty = plog.ActualSales_Qty;
            //    _plog.Company = plog.Company;
            //    _plog.WorkArea = plog.WorkArea;
            //    _plog.MsrCd = plog.MsrCd;
            //    _plog.Sales_Val = plog.Sales_Val;
            //    _plog.SMAreaCode = plog.SMAreaCode;
            //    _plog.SSSupervisor = plog.SSSupervisor;
            //    _plog.Supervisor = plog.Supervisor;
            //    _plog.SSupervisor = plog.SSupervisor;
            //    _plog.SSSSupervisor = plog.SSSSupervisor;
            //    _plog.TP = plog.TP;
            //    _plog.Retn_Qty = plog.Retn_Qty; 
            //    _plog.ZoneCode = plog.ZoneCode;
            //    _plog.Vat = plog.Vat;
            //    _plog.RegionCode = plog.RegionCode;
            //    _plog.SMAreaCode = plog.SMAreaCode;
            //    _plog.GMAreaCode = plog.GMAreaCode;
            //    _plog.PCd = plog.PCd;   


            //    //var emply = from employee in db.GeneralUsers
            //    //            where employee.EmployeeID == _plog.GeneralUserID.ToString() && employee.Exist == true

            //    //            select employee;


            //        setup.AddDailySalesTTY(_plog);
            //    setup.Save();

            //}
            //setup.Save();
            //ViewBag.Info = "New Addition: ";


            return View(list);
        }
        [Authorize]


        public async Task<ActionResult> SyncDailySales()

        {
            //var UI = UserInfo.GetUserInfo(User.Identity.Name);
            try
            {
                //var orgs = setup.OSGetAll(UI.User.BU);
                //UI.Host = Request.ServerVariables["REMOTE_HOST"];
                //foreach (var org in orgs)
                //{
                var sales = await setup.SyncSales();
                foreach (var pur in sales)
                {
                    await setup.DSAdd(pur);
                }
                //purchase.Save();

                ////------------update stock----------
                //foreach (var pur in purchases)
                //{
                //    foreach (var item in pur.Items)
                //    {
                //        master.UpdateStock(pur.DealerCode, item.MaterialCode, "Purchase");
                //    }
                //}
                await setup.Save();
                TempData["Info"] = "Date sucessfully synced";
            }


            catch (Exception err)
            {
                TempData["Error"] = err.Message.ToString();
            }
            return RedirectToAction("Index");
        }



        [Authorize]


        public async Task<ActionResult> SyncCustBaL()

        {
            var loggedUser = setup.UserGetByName(User.Identity!.Name!);
            List<CustomerBalance> dta = new List<CustomerBalance>();

            try
            {
                //var orgs = setup.OSGetAll(UI.User.BU);
                //UI.Host = Request.ServerVariables["REMOTE_HOST"];
                //foreach (var org in orgs)
                //{
                var sales = await setup.SynGetCustBAl_DATA(loggedUser);

                dta = sales.ToList();
                ViewBag.CustBaL = dta;

                //foreach (var pur in sales)
                //{
                //    await setup.CustBALLAdd(pur);
                //}
                //purchase.Save();

                ////------------update stock----------
                //foreach (var pur in purchases)
                //{
                //    foreach (var item in pur.Items)
                //    {
                //        master.UpdateStock(pur.DealerCode, item.MaterialCode, "Purchase");
                //    }
                //}
                //await setup.Save();
                TempData["Info"] = "Date sucessfully synced";
            }


            catch (Exception err)
            {
                TempData["Error"] = err.Message.ToString();
            }
            return View ();
        }



        [Authorize]
        public async Task<ActionResult> SyncUOA_H_Dt()

        {
            //var UI = UserInfo.GetUserInfo(User.Identity.Name);
            try
            {

                var uoa = await setup.SynGetUOA_DATA();
                foreach (var dt in uoa)
                {
                    await setup.AddUOAMaster(dt);
                }

                await setup.Save();
                TempData["Info"] = "Date sucessfully synced";
            }


            catch (Exception err)
            {
                TempData["Error"] = err.Message.ToString();
            }
            return RedirectToAction("Index");
        }



        [Authorize]
        public async Task<ActionResult> SyncUOA_I_Dt()

        {
            //var UI = UserInfo.GetUserInfo(User.Identity.Name);
            try
            {

                var uoa = await setup.SynGetUOA_Item_DATA();
                foreach (var dt in uoa)
                {
                    await setup.AddUOAItem(dt);
                }

                await setup.Save();
                TempData["Info"] = "Date sucessfully synced";
            }


            catch (Exception err)
            {
                TempData["Error"] = err.Message.ToString();
            }
            return RedirectToAction("Index");
        }




        [Authorize]
        public async Task<ActionResult> SyncResult()

        {
            //var UI = UserInfo.GetUserInfo(User.Identity.Name);
            try
            {

                var uoa = await setup.SynGetResult_DATA();
                foreach (var dt in uoa)
                {
                    await setup.AddResult(dt);
                }

                await setup.Save();
                TempData["Info"] = "Date sucessfully synced";
            }


            catch (Exception err)
            {
                TempData["Error"] = err.Message.ToString();
            }
            return RedirectToAction("Index");
        }


        [Authorize(Roles = "Admin")]
        public IActionResult PendingUOAIndex(string q = "")
        {

            if (TempData["Error"] != null)
            {
                ViewBag.Error = TempData["Error"];
            }
            else if (TempData["Info"] != null)
            {
                ViewBag.Info = TempData["Info"];
            }

            if (q.Length > 0)
            {
                var SearchResult = setup.GetAllPendingUOA().AsEnumerable().Where(e =>
                    e.SearchText.ToLower().Contains(q.ToLower()))
                    .OrderByDescending(e => e.REQ_NO);
                ViewBag.SearchText = q;

                return View(SearchResult);
            }

            return View(setup.GetAllPendingUOA().OrderByDescending(e => e.SAMPLE_DATE));
        }


       
        public IActionResult EMPSalesIndex( DateTime? Start, DateTime? End, string q = "")
        {

            var loggedUser = setup.UserGetByName(User.Identity!.Name!);


            if (TempData["Error"] != null)
            {
                ViewBag.Error = TempData["Error"];
            }
            else if (TempData["Info"] != null)
            {
                ViewBag.Info = TempData["Info"];
            }

            DateTime date = DateTime.Now;
            DateTime firstDayOfMonth = new DateTime(date.Year, date.Month, 1);

            if (!Start.HasValue && !End.HasValue)
            {
                Start = firstDayOfMonth;
                End = DateTime.Today;
            }
            ViewBag.StartDate = Start.Value.ToString("yyyy-MM-dd");
            ViewBag.EndDate = End.Value.ToString("yyyy-MM-dd");


            if (q != null)
            {
                var SearchResult = setup.GetAllSales().Where(m => m.FKDat >= Start && m.FKDat <= End &&  m.empcode == loggedUser.UserName).AsEnumerable().Where(e =>
                    e.SearchText.ToLower().Contains(q.ToLower()))
                    .OrderByDescending(e => e.FKDat);
                ViewBag.SearchText = q;

                return View(SearchResult);
            }
            else
            {
                return View(setup.GetAllSales().Where(d => d.empcode == loggedUser.UserName).OrderByDescending(e => e.FKDat));

            }
           

           
        }



        //#region Issue Detail
        [Authorize(Roles = "Admin")]
        public IActionResult VieWDetail(long IID)
        {
            if (TempData["Error"] != null)
            {
                ViewBag.Error = TempData["Error"];
            }
            else if (TempData["Info"] != null)
            {
                ViewBag.Info = TempData["Info"];
            }
            List<ResultRecord> records = new List<ResultRecord>();

            VMResultApprov resApp = new VMResultApprov();
          

            var con = setup.GetResult(IID);
            ViewBag.sample = setup.GetUOAItem(IID);
            var itm = setup.GetUOAItem(IID).FirstOrDefault();
            var hdrdta = setup.GetHeaderByCustomer(itm.KUNNR,itm.EQUIPMENT).FirstOrDefault();

            var item = ViewBag.sample;
            resApp.OIL_NAME = hdrdta.OIL_NAME;
            resApp.EQUIPMENT_MODEL = hdrdta.EQUIPMENT_MODEL;
            resApp.EQUIPMENT_SERIAL = hdrdta.EQUIPMENT_SERIAL;
            resApp.CUST_ADDRESS = hdrdta.CUST_ADDRESS;
            resApp.SAMPLE_DATE = itm.SAMPLE_DATE.ToString();
            resApp.SAMPLE_NO = itm.SAMPLE_NO;
            resApp.SAMP_QTY = itm.SAMP_QTY;
            resApp.REMARKS = itm.REMARKS;
            resApp.OIL_RUN_HOUR = itm.OIL_RUN_HOUR;
            resApp.DAILY_OIL_TOP = itm.DAILY_OIL_TOP;
            resApp.LAST_OILCHNG_DT = itm.LAST_OILCHNG_DT.ToString();
            resApp.REQ_NO = itm.REQ_NO;
            resApp.SAMPLE_REC_DATE = itm.SAMPLE_REC_DATE.ToString();
            resApp.CREATED_BY = itm.CREATED_BY;
            resApp.EQP_RUN_HOUR = itm.EQP_RUN_HOUR;
            resApp.CutomerName = itm.CutomerName;
            resApp.EResultSt = itm.EResultSt;
            resApp.FILT_RUN_HOUR = itm.FILT_RUN_HOUR;
            resApp.EQUIPMENT = itm.EQUIPMENT;

            foreach( var res in con )
            {

                var result = new ResultRecord();
                if ( res != null )
                {
                    result.REQ_NO = res.REQ_NO;
                    result.SAMPLE_NO = res.SAMPLE_NO;
                    result.CHAR_GROUP = res.CHAR_GROUP;
                    result.MBEWERTG = res.MBEWERTG;
                    result.CHAR_GROUP = res.CHAR_GROUP;
                    result.CHAR_PART3 = res.CHAR_PART3;
                    result.VERWMERKM = res.VERWMERKM;
                    result.Result   =   res.Result;
                    result.Mresult = res.Mresult;
                    result.Erstelldat = res.Erstelldat;
                    result.DUMMY10 = res.DUMMY10;
                    result.KURZTEXT = res.KURZTEXT;
                    result.GRAPH = res.GRAPH;   
                    result.MERKNR = res.MERKNR;
                    
                   
                }
                records.Add(result);
            }

            resApp.results = records;


            if (con == null)
            {
                TempData["Error"] = "No issue selected";
                return RedirectToAction("Index");
            }

            //ViewBag.Categories = EC.ECGetSelectList();

            return View(resApp);
        }




        [HttpPost]
        public IActionResult UpdateResult(VMResultApprov obj)
        {

            if (obj != null)
            {

               foreach( var item in obj.results)
                {
                    setup.updateResult(item);   
                }

              UOA_Item newItem = new UOA_Item();    
                newItem.INS_LOT = obj.INS_LOT;
                newItem.SAMPLE_NO = obj.SAMPLE_NO;
                newItem.REQ_NO = obj.REQ_NO;
                newItem.Comments = obj.Comments;
                newItem.EResultSt = obj.EResultSt;
                
                setup.updateItem(newItem);  






            }



            return Redirect("PendingUOAIndex");
        }
    }
}