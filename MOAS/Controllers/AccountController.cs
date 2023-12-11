using MOAS.Interfaces;
using MOAS.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using MOAS.Models.VM;
using Microsoft.AspNetCore.Http.Extensions;
using System;

namespace MOAS.Controllers
{
    public class AccountController : Controller
    {
        //
        // GET: /Account/LogOn

        private ISetupRepository setup { get; set; }

        public AccountController(ISetupRepository _setup)
        {
            setup = _setup;
        }

        [Authorize(Roles = "Admin, User")]
        public IActionResult Index(string q = "")
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
                var SearchResult = setup.UserGetAll().AsEnumerable().Where(e =>
                    e.SearchText.ToLower().Contains(q.ToLower()))
                    .OrderByDescending(e => e.UserName);
                ViewBag.SearchText = q;

                return View(SearchResult);
            }

            return View(setup.UserGetAll().OrderByDescending(e => e.UserName));
        }


        [Authorize(Roles = "Admin, User")]
        public IActionResult NotExist()
        {
            if (TempData["Error"] != null)
            {
                ViewBag.Error = TempData["Error"];
            }
            else if (TempData["Info"] != null)
            {
                ViewBag.Error = TempData["Info"];
            }
            return View(setup.UserGetAll().Where(m => m.Exist == false).OrderBy(i => i.UserName));
        }
        public IActionResult LogOn()
        {
            if (TempData["Error"] != null)
            {
                ViewBag.Error = TempData["Error"];
            }
            
            return View(new LogOnModel());
        }

        //
        // POST: /Account/LogOn

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOn(LogOnModel model)
        {
            try
            {
                if (await setup.ValidateUser(model.UserName, model.Password))
                {
                    string cookieval = model.UserName;
                    var user =  setup.UserGetByName(model.UserName);
                    var roles = user.Roles.ToList();
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, cookieval)
                    };
                    foreach (var role in roles)
                    {
                        var claim = new Claim(ClaimTypes.Role, role.Name);
                        claims.Add(claim);
                    }
                    var claimsIdentity = new ClaimsIdentity(claims, "Login");

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
                    
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    TempData["Error"] = "User Name or Password not correct!!";
                }

            }
            catch (Exception err)
            {
                TempData["Error"] = err.Message.ToString();
            }
            // If we got this far, something failed, redisplay form
            return RedirectToAction("LogOn", "Account");
        }

        //
        // GET: /Account/LogOff

        public async Task<IActionResult> LogOff()
        {
            await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("LogOn", "Account");
        }

        //
        // GET: /Account/Register

        [Authorize(Roles = "Admin, User")]
        public IActionResult Register()
        {

            User model = new User
            {
                Exist = true
            };
            return View(model);
        }

        //
        // POST: /Account/Register

        [Authorize(Roles = "Admin, User")]
        [HttpPost]
        public async Task<IActionResult> Register(User obj)
        {
            try
            {
                obj.LastUpdate = DateTime.Now;
                obj.HostName = HttpContext.Connection.RemoteIpAddress!.ToString();
                //obj.CreateDate = DateTime.Now;
                //obj.HostName = HttpContext.Connection.RemoteIpAddress!.ToString();
                await setup.UserAdd(obj);
                await setup.Save();
                
            }
            catch (Exception exception)
            {
                return Json(new { flag = 'n', msg = exception.Message.ToString() }); 
            }
            return Json(new { flag = 'y', msg = "Data Saved Successfully" });
        }



        //
        // GET: /Account/ChangePassword

        [Authorize()]
        public IActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Account/ChangePassword

        [Authorize()]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordModel model)
        {
            if (ModelState.IsValid)
            {
                var loggedUser =  setup.UserGetByName(User.Identity!.Name!);
                if (await setup.ValidateUser(loggedUser.UserName, model.OldPassword))
                {
                    setup.SetPassword(loggedUser, model.NewPassword);
                    ViewBag.Info = "Password Changed Successfully";
                    return View(new ChangePasswordModel());
                }
                else
                    ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }


        [Authorize(Roles = "Admin, User")]
        public async Task<IActionResult> ActiveDeactive(int UserID)
        {
            
            try
            {
                await setup.ActiveDeactive(UserID);
                await setup.Save();
                
            }
            catch (Exception err)
            {
                TempData["Error"] = err.Message;
            }
            return RedirectToAction("Index",new {q=UserID});
        }

        [Authorize(Roles = "Admin, User")]
        [HttpPost]
        public async Task<IActionResult> ResetPassword(int UserID)
        {
            string msg = "";
            try
            {
                var user = await setup.UserGet(UserID);
                user.Password = CommonMethod.GetMD5("1234");
                user.ConfirmPassword = user.Password;
                await setup.Save();
                msg = $"Password Reset for Emp: {user.DisplayText} to 1234";

            }
            catch (Exception err)
            {
                return Json(new { flag='n',msg= err.Message.ToString() } );
            }


            // If we got this far, something failed, redisplay form
            return Json(new {flag='y' ,msg=msg });
        }

        [Authorize(Roles = "Admin, User")]
        public async Task<IActionResult> Edit(int UserID)
        {
            if (TempData["Error"] != null)
            {
                ViewBag.Error = TempData["Error"];
            }
            else if (TempData["Info"] != null)
            {
                ViewBag.Info = TempData["Info"];
            }
            
            var oldone = await setup.UserGet(UserID);

            return View(oldone);
        }

        [Authorize(Roles = "Admin, User")]
        [HttpPost]
        public async Task<ActionResult> Edit(User obj)
        {
            
            try
            {
                //obj.LastUpdate = DateTime.Now;
                //obj.HostName = HttpContext.Connection.RemoteIpAddress!.ToString();
                await setup.UserUpdate(obj);
                await setup.Save();
                
            }
            catch (Exception exception)
            {
                return Json(new { flag = 'n', msg = exception.Message.ToString() });
            }
            return Json(new { flag = 'y', msg = "Data Saved Successfully" });
        }
        [HttpGet]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> SetPermission(int UserID)
        {
            Permission _Permission;
            List<Permission> lstPermission = new List<Permission>();
            var _User = await setup.UserGet(UserID);
            var roles=setup.RoleGetAll().Where(r=>r.Name!="Admin").ToList();
            foreach (var _role in roles)
            {
                _Permission = new Permission();
                _Permission.RoleID = _role.RoleID;
                _Permission.Roles = _role;
                if (_User.Roles.Any(r => r.RoleID == _role.RoleID))
                {
                    _Permission.Status = true;
                }
                else
                {
                    _Permission.Status = false;
                }
                lstPermission.Add(_Permission);
            }
            ViewBag.User = _User;
            return PartialView("_Permission",lstPermission);
        }

        [Authorize(Roles = "Admin,User")]
        [HttpPost]
        public async Task<IActionResult> SetPermission(int[] chk,int UserID=0)
        {
            string msg = "";
            try
            {
                if (UserID==0)
                {
                    ViewBag.Error = "No User Selected!";
                }
                else
                {

                    var _User = await setup.UserGet(UserID);
                    var roles = setup.RoleGetAll().Where(r => r.Name != "Admin").ToList();
                    foreach (var _role in _User.Roles.ToList())
                    {
                        _User.Roles.Remove(_role);
                    }
                    foreach (var roleid in chk)
                    {
                        var _role = roles.Where(r=>r.RoleID==roleid).FirstOrDefault();
                        if (_role!=null) { 
                            _User.Roles.Add(_role);
                        }
                    }

                    await setup.Save();
                    msg= $"Permission set for {_User.DisplayText}" ;
            }
            }
            catch (Exception err)
            {
                return Json(new {flag='n',msg=err.Message.ToString() });
            }
            return Json(new { flag = 'y', msg = msg });
        }
        public IActionResult NoPermission()
        {
            ViewBag.Permission = "You have no permission to access this page!";
            return View();
        }
    }
}
