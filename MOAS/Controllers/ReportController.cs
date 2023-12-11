using MOAS.Interfaces;
using MOAS.Models;
using MOAS.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;

namespace MOAS.Controllers
{
    public class ReportController : Controller
    {
        private ISetupRepository setup { get; set; }

        public ReportController(ISetupRepository _setup)
        {
            setup = _setup;
        }




        [Authorize(Roles = "Admin,User")]
        public IActionResult Index()
        {
            //var UI = UserInfo.GetUserInfo(User.Identity.Name);
            if (TempData["Error"] != null)
            {
                ViewBag.Error = TempData["Error"];
            }
            else if (TempData["Info"] != null)
            {
                ViewBag.Info = TempData["Info"];
            }
            ViewBag.cust = setup.GetCustomerSelectlist();
            var list = setup.GetCustomerSelectlist();


            //if (user.CheckRoleExistant(UI.User.UserID, "Report"))
            //{
            //    ViewBag.Employees = user.GetSelectList(UI);
            //    ViewBag.Customers = customer.GetSelectListByJob(UI);
            //}


            return View(new ReportParameter { Start = DateTime.Today, End = DateTime.Today });
        }

        public JsonResult getEQP(long CID)
        {

            var list = setup.ELGetSelectList(CID);
            return Json(list);
        }

        [Authorize(Roles = "Admin, User")]
        public IActionResult PreviewReport(long IID)
        {
            var report = setup.GetVMResultData(IID);
            var rp = new rptOA(report);
            return View("PreviewReport", rp);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,User")]
        public IActionResult ReportPreviw(ReportParameter par)
        {
            //var UI = UserInfo.GetUserInfo(User.Identity.Name);
            try
            {
                switch (par.ReportID)
                {
                    case 1:
                        {
                          

                            var Dmeal = setup.GetVMResultByDateRang(par.Start, par.End,par.CID, par.EquipmentID);

                            rptUOAByDateRange rp = new rptUOAByDateRange(par.Start, par.End, Dmeal);
                            return View("PreviewReport", rp);
                        }


                }
            }
            catch (Exception err)
            {
                TempData["Error"] = err.Message.ToString();
            }
            return RedirectToAction("Index");
        }
    }
    }