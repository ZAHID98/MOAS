using MOAS.Interfaces;
using MOAS.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

using System.Net;
using MOAS.Models.VM;
using Castle.Core.Resource;
using System.Security.Cryptography;
using System.Drawing;
using System.Data.SqlClient;

namespace MOAS.Repositories
{
    public class SetupRepository : ISetupRepository
    {
        private MOASContext db { get; set; }

        public SetupRepository(MOASContext _db)
        {
            db = _db;
        }
        public async Task ActiveDeactive(int UserID)
        {
            var user = await UserGet(UserID);
            if (user != null)
            {
                user.ConfirmPassword = user.Password;
                if (user.Exist == true)
                {
                    user.Exist = false;
                }
                else
                {
                    user.Exist = true;
                }
            }
        }

        public async Task<bool> ChangePassword(int UserID, string oldPassword, string newPassword)
        {
            bool flag = false;
            var user = await UserGet(UserID);
            if (user != null && await ValidateUser(user.UserName, oldPassword))
            {
                var encpass = CommonMethod.GetMD5(newPassword.Trim());
                user.Password = encpass;
                flag = true;
            }

            return flag;
        }

        public async Task<bool> CheckRoleExistant(int UserID, string RoleName)
        {
            bool flag = false;

            var user = await UserGet(UserID);
            if (user != null)
            {
                flag = user.Roles.Any(r => r.Name == RoleName);
            }

            return flag;
        }

        public async Task Save()
        {
            await db.SaveChangesAsync();
        }
        public async Task UserAdd(User obj)
        {
            if (UserGetAll().Any(u => u.UserName == obj.UserName.Trim().ToLower()))
            {
                throw new Exception(obj.UserName + " already exist");
            }
            else if ((obj.Email ?? "") != "" &&
                UserGetAll().Any(u => u.Email == obj.Email!.Trim().ToLower()))
            {
                throw new Exception(obj.Email + " already exist");
            }
            else
            {
                obj.UserName = obj.UserName.Trim().ToLower();
                obj = SetPassword(obj, obj.Password);
                await db.User.AddAsync(obj);
            }
        }
        public User SetPassword(User user, string p)
        {
            string str = CommonMethod.GetMD5(p.Trim());
            user.Password = str;
            user.ConfirmPassword = str;
            return user;
        }

        public async Task<User> UserGet(int ID = 0)
        {
            return await db.User.FindAsync(ID) ?? new User();
        }
        public async Task<Role> RoleGet(string RoleName = "")
        {
            return await db.Role.FindAsync(RoleName) ?? new Role();
        }

        public IEnumerable<User> UserGetAll()
        {
            return db.User.Where(u => u.UserName != "admin");
        }

        public IEnumerable<Role> RoleGetAll()
        {
            return db.Role;
        }



        public User UserGetByName(string username = "")
        {
            return db.User.
                Where(u => u.UserName == username.ToLower())
                .FirstOrDefault() ?? new User();
        }

        public SelectList UserGetSelectList()
        {
            return new SelectList(UserGetAll(), "UserID", "DisplayText");
        }

        public async Task UserUpdate(User obj)
        {
            var oldone = await UserGet(obj.UserID);
            oldone.ConfirmPassword = oldone.Password;
            obj.UserName = obj.UserName.Trim().ToLower();
            obj.Email = (obj.Email ?? "").Trim().ToLower();
            if (oldone != null)
            {
                if (oldone.UserName != obj.UserName &&
                    UserGetAll().Any(u => u.UserName == obj.UserName.Trim().ToLower()))
                {
                    throw new Exception(obj.UserName + " already exist");
                }
                else if ((obj.Email ?? "") != "" &&
                    obj.Email != oldone.Email &&
                    UserGetAll().Any(u => u.Email == obj.Email!.Trim().ToLower()))
                {
                    throw new Exception(obj.Email + " already exist");
                }
                else
                {
                    oldone.UserName = obj.UserName;
                    oldone.Email = obj.Email!;
                    oldone.FullName = obj.FullName;
                    //oldone.HostName = obj.HostName;
                    //oldone.LastUpdate = obj.LastUpdate;
                }
            }
        }

        public async Task<bool> ValidateUser(string UserName, string Password)
        {
            bool flag = false;
            var oldone = UserGetByName(UserName);
            if (oldone != null && oldone.Exist == true)
            {
                var hash = CommonMethod.GetMD5(Password.Trim());
                if (hash == oldone.Password)
                {
                    flag = true;
                }
            }
            return flag;
        }



        public IEnumerable<DailySales> GetAllSales()

        {
            return db.DailySales;
        }
        public async Task DSAdd(DailySales DS)
        {
            var obj = new DailySales();
            obj.vbeln = DS.vbeln;
            obj.Posnr = DS.Posnr;
            obj.vgpos = DS.vgpos;
            obj.kunag = DS.kunag;
            obj.matnr = DS.matnr;
            obj.arktx = DS.arktx;
            obj.fkart = DS.fkart;
            obj.FKDat = DS.FKDat;
            obj.name1 = DS.name1;

            obj.spart = DS.spart;
            obj.empcode = DS.empcode;
            obj.aubel = DS.aubel;
            obj.zvgbel = DS.zvgbel;
            obj.year_budat = DS.year_budat;
            obj.TotalQty = DS.TotalQty;
            obj.month_budat = DS.month_budat;
            obj.quarter_budat = DS.quarter_budat;



            db.DailySales.Add(obj);

        }
        public async Task<IEnumerable<DailySales>> SyncSales()
        {
            IEnumerable<DailySales> sales = new List<DailySales>();
            IEnumerable<DailySales> ds = new List<DailySales>();
            string uri = Common1.BaseUrl + "?sap-client=" + Common1.Client;

            var Credential = new NetworkCredential(Common1.UserID, Common1.Password);
            var Handler = new HttpClientHandler { Credentials = Credential };
            HttpClient _client = new HttpClient(Handler);

            IEnumerable<DailySales> existingSalesData = GetAllSales();
            var response = await _client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                sales = JsonConvert.DeserializeObject<IEnumerable<DailySales>>(content);
                if (sales.Count() > 0)
                {
                    var toadd
                         = sales.Where(ns => !existingSalesData.Any(es => es.vbeln == ns.vbeln && es.Posnr == ns.Posnr));


                    ds = DSConvert(toadd);
                }
            }
            return ds;
        }


        private IEnumerable<DailySales> DSConvert(IEnumerable<DailySales> sales)
        {
            var masterlist = new List<DailySales>();
            var ds = (from row in sales
                      select new
                      {




                          vbeln = row.vbeln,
                          Posnr = row.Posnr,
                          vgpos = row.vgpos,
                          kunag = row.kunag,
                          fkart = row.fkart,
                          FKDat = row.FKDat,
                          name1 = row.name1,
                          arktx = row.arktx,
                          matnr = row.matnr,

                          empcode = row.empcode,
                          aubel = row.aubel,
                          zvgbel = row.zvgbel,
                          month_budat = row.month_budat,
                          year_budat = row.year_budat,
                          quarter_budat = row.quarter_budat,
                          TotalQty = row.TotalQty,
                          spart = row.spart,

                      }).Distinct().ToList();
            foreach (var p in ds)
            {
                //var vmpurchases = purchases.Where(c => c.DocNo == p.DocNo);
                var master = new DailySales();
                master.vbeln = p.vbeln;
                master.Posnr = p.Posnr;
                master.vgpos = p.vgpos;
                master.kunag = p.kunag;
                master.matnr = p.matnr;
                master.arktx = p.arktx;
                master.fkart = p.fkart;
                master.FKDat = p.FKDat;
                master.empcode = p.empcode;

                master.aubel = p.aubel;
                master.zvgbel = p.zvgbel;
                master.name1 = p.name1;
                master.TotalQty = p.TotalQty;
                master.month_budat = p.month_budat;
                master.year_budat = p.year_budat;
                master.quarter_budat = p.quarter_budat;
                master.spart = p.spart;



                masterlist.Add(master);
            }
            return masterlist;
        }



        #region Customer Balance
        public IEnumerable<CustomerBalance> GetAllCustBAL()

        {
            return db.CustomerBalance;
        }
        public async Task CustBALLAdd(CustomerBalance DS)
        {
            var obj = new CustomerBalance();
            obj.Kunnr = DS.Kunnr;
            obj.Name1 = DS.Name1;
            obj.CustAddress = DS.CustAddress;
            obj.EmpID = DS.EmpID;
            obj.EMP_Name = DS.EMP_Name;
            obj.t_curr_bal = DS.t_curr_bal;
            obj.loc_currcy = DS.loc_currcy;





            db.CustomerBalance.Add(obj);

        }
        public async Task<IEnumerable<CustomerBalance>> SynGetCustBAl_DATA(User UI)
        {
            IEnumerable<CustomerBalance> sales = new List<CustomerBalance>();
            IEnumerable<CustomerBalance> ds = new List<CustomerBalance>();
            string uri = Common5.BaseUrl + "?sap-client=" + Common5.Client +"&ID="+UI.UserName ;

            var Credential = new NetworkCredential(Common5.UserID, Common5.Password);
            var Handler = new HttpClientHandler { Credentials = Credential };
            HttpClient _client = new HttpClient(Handler);

            IEnumerable<CustomerBalance> existingSalesData = GetAllCustBAL();
            var response = await _client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                sales = JsonConvert.DeserializeObject<IEnumerable<CustomerBalance>>(content);
                if (sales.Count() > 0)
                {
                    var toadd
                         = sales.Where(ns => !existingSalesData.Any(es => es.Kunnr == ns.Kunnr));


                    ds = DSConvertBAL(toadd);
                }
            }
            return ds;
        }


        private IEnumerable<CustomerBalance> DSConvertBAL(IEnumerable<CustomerBalance> sales)
        {
            var masterlist = new List<CustomerBalance>();
            var ds = (from row in sales
                      select new
                      {




                          Kunnr = row.Kunnr,
                          Name1 = row.Name1,
                          CustAddress = row.CustAddress,
                          EmpID = row.EmpID,
                          EMP_Name = row.EMP_Name,
                          t_curr_bal = row.t_curr_bal,
                          loc_currcy = row.loc_currcy,

                      }).Distinct().ToList();
            foreach (var p in ds)
            {
                //var vmpurchases = purchases.Where(c => c.DocNo == p.DocNo);
                var master = new CustomerBalance();
                master.Kunnr = p.Kunnr;
                master.Name1 = p.Name1;
                master.CustAddress = p.CustAddress;
                master.EmpID = p.EmpID;
                master.EMP_Name = p.EMP_Name;
                master.t_curr_bal = p.t_curr_bal;
                master.loc_currcy = p.loc_currcy;




                masterlist.Add(master);
            }
            return masterlist;
        }
    
    #endregion




    #region Get Header Data




    public async Task AddUOAMaster(UOA_Header Md)
        {
            var obj = new UOA_Header();
            obj.REQ_NO = Md.REQ_NO;
            obj.KUNNR = Md.KUNNR;
            obj.CustomerName = Md.CustomerName;
            obj.CUST_ADDRESS = Md.CUST_ADDRESS;
            obj.EQUIPMENT = Md.EQUIPMENT;
            obj.EQUIPMENT_MAKE = Md.EQUIPMENT_MAKE;
            obj.EQUIPMENT_MODEL = Md.EQUIPMENT_MODEL;
            obj.EQUIPMENT_SERIAL = Md.EQUIPMENT_SERIAL;
            obj.INS_LOT = Md.INS_LOT;
            obj.OIL_CODE = Md.OIL_CODE;
            obj.OIL_NAME = Md.OIL_NAME;
            obj.created_on = Md.created_on;
            obj.UserName = Md.UserName;
            obj.Contact1_Name = Md.Contact1_Name;
            obj.Contact1_Phone = Md.Contact1_Phone;
            obj.Contact1_Desig = Md.Contact1_Desig;
            obj.Contact1_Email = Md.Contact1_Email;

            db.UOA_Header.Add(obj);

        }

        public List<UOA_Header> GetAllUOAH()
        {

            var queryResult = (from l in db.UOA_Header
                              select new UOA_Header()
                              {
                                  KUNNR = l.KUNNR,
                                  CustomerName = l.CustomerName
                               
                              }).GroupBy(x => x.CustomerName).Select(z => z.OrderBy(i => i.KUNNR).First()).ToList();

            return queryResult;
        }

        public IEnumerable <UOA_Header>GetHeaderByCustomer( long CID, string EQp)

        {
            return db.UOA_Header.
                Where(u => u.KUNNR == CID && u.EQUIPMENT == EQp);
               

        }
        public IEnumerable<VMENGINSL> GetUOA_HByCustomer(long CID)

        {


            var queryResult = (from l in db.UOA_Header.Where(d => d.KUNNR == CID)
                              
            select new VMENGINSL
            {
                EQUIPMENT_SERIAL = l.EQUIPMENT_SERIAL,
                EQUIPMENT_MODEL = l.EQUIPMENT_MODEL

            }).GroupBy(x => x.EQUIPMENT_SERIAL).Select(z => z.OrderBy(i => i.EQUIPMENT_SERIAL).Distinct().First()).ToList(); ;

            return queryResult;
            


        }

        public SelectList ELGetSelectList(long CID)
        {
            var list = db.Equipment.Where(d => d.Kunnr == CID);
            return new SelectList(list, "EquipmentID", "DisplayText");
        }

        public SelectList GetCustomerSelectlist()
        {
            return new SelectList(GetAllUOAH(), "KUNNR", "CustomerName");
        }

        public  Equipment GETEQP(int ID = 0)
        {
            return  db.Equipment.Find(ID) ?? new Equipment();
        }

        public async Task<IEnumerable<UOA_Header>> SynGetUOA_DATA()
        {
            IEnumerable<UOA_Header> hd = new List<UOA_Header>();
            IEnumerable<UOA_Header> ds = new List<UOA_Header>();
            string uri = Common2.BaseUrl + "?sap-client=" + Common2.Client;

            var Credential = new NetworkCredential(Common2.UserID, Common2.Password);
            var Handler = new HttpClientHandler { Credentials = Credential };
            HttpClient _client = new HttpClient(Handler);

            IEnumerable<UOA_Header> existingSalesData = GetAllUOAH();
            var response = await _client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();



                hd = JsonConvert.DeserializeObject<IEnumerable<UOA_Header>>(content);
                if (hd.Count() > 0)
                {
                    var toadd
                         = hd.Where(ns => !existingSalesData.Any(es => es.REQ_NO == ns.REQ_NO && es.INS_LOT == ns.INS_LOT));


                    ds = UOA_HConvert(toadd);
                }
            }
            return ds;
        }



        private IEnumerable<UOA_Header> UOA_HConvert(IEnumerable<UOA_Header> hd)
        {
            var masterlist = new List<UOA_Header>();
            var ds = (from row in hd
                      select new
                      {
                          REQ_NO = row.REQ_NO,
                          KUNNR = row.KUNNR,
                          CustomerName = row.CustomerName,
                          EQUIPMENT = row.EQUIPMENT,
                          CUST_ADDRESS = row.CUST_ADDRESS,
                          EQUIPMENT_MAKE = row.EQUIPMENT_MAKE,
                          EQUIPMENT_MODEL = row.EQUIPMENT_MODEL,
                          EQUIPMENT_SERIAL = row.EQUIPMENT_SERIAL,
                          OIL_CODE = row.OIL_CODE,
                          OIL_NAME = row.OIL_NAME,
                          INS_LOT = row.INS_LOT,
                          UserName = row.UserName,
                          created_on = row.created_on,
                          Contact1_Name = row.Contact1_Name,
                          Contact1_Desig = row.Contact1_Desig,
                          Contact1_Phone = row.Contact1_Phone,
                          Contact1_Email = row.Contact1_Email,





                      }).Distinct().ToList();
            foreach (var p in ds)
            {
                //var vmpurchases = purchases.Where(c => c.DocNo == p.DocNo);
                var master = new UOA_Header();
                master.REQ_NO = p.REQ_NO;
                master.KUNNR = p.KUNNR;
                master.CustomerName = p.CustomerName;
                master.EQUIPMENT = p.EQUIPMENT;
                master.CUST_ADDRESS = p.CUST_ADDRESS;
                master.EQUIPMENT_MAKE = p.EQUIPMENT_MAKE;
                master.EQUIPMENT_MODEL = p.EQUIPMENT_MODEL;
                master.EQUIPMENT_SERIAL = p.EQUIPMENT_SERIAL;
                master.OIL_CODE = p.OIL_CODE;
                master.OIL_NAME = p.OIL_NAME;
                master.INS_LOT = p.INS_LOT;
                master.UserName = p.UserName;
                master.created_on = p.created_on;
                master.Contact1_Name = p.Contact1_Name;
                master.Contact1_Desig = p.Contact1_Desig;
                master.Contact1_Phone = p.Contact1_Phone;
                master.Contact1_Email = p.Contact1_Email;


                masterlist.Add(master);
            }
            return masterlist;
        }




        #endregion





        #region Get Item Data




        public async Task AddUOAItem(UOA_Item MI)
        {
            var obj = new UOA_Item();
            obj.REQ_NO = MI.REQ_NO;
            obj.INS_LOT = MI.INS_LOT;
            obj.KUNNR = MI.KUNNR;
            obj.CutomerName = MI.CutomerName;
            obj.status = EStatus.pending;
            obj.SAMPLE_NO = MI.SAMPLE_NO;
            obj.EQUIPMENT = MI.EQUIPMENT;
            obj.ITEM_NO = MI.ITEM_NO;
            obj.EQP_RUN_HOUR = MI.EQP_RUN_HOUR;
            obj.OIL_RUN_HOUR = MI.OIL_RUN_HOUR;
            obj.DAILY_OIL_TOP = MI.DAILY_OIL_TOP;
            obj.FILT_RUN_HOUR = MI.FILT_RUN_HOUR;
            obj.LAST_OILCHNG_DT = MI.LAST_OILCHNG_DT;
            obj.SAMPLE_DATE = MI.SAMPLE_DATE;
            obj.SAMPLE_REC_DATE = MI.SAMPLE_REC_DATE;
            obj.SAMP_QTY = MI.SAMP_QTY;
            obj.SAMP_UNIT = MI.SAMP_UNIT;
            obj.REMARKS = MI.REMARKS;
            obj.CREATED_BY = MI.CREATED_BY;
            //obj.CREATED_ON = MI.CREATED_ON;

            db.UOA_Item.Add(obj);

        }



        public async Task  updateItem(UOA_Item item)
        {
            if(item != null)
            {
                var iresult = db.UOA_Item.Where(d => d.SAMPLE_NO == item.SAMPLE_NO && d.REQ_NO == item.REQ_NO).FirstOrDefault();

                if (iresult != null)
                {

               iresult.Comments = item.Comments; 
                    
             iresult.EResultSt = item.EResultSt; 
             iresult.status = EStatus.Approved;

            db.SaveChanges();   

                }

            }
        }

        public IEnumerable<UOA_Item> GetAllUOAI()

        {
            return db.UOA_Item;
        }

        public async Task<IEnumerable<UOA_Item>> SynGetUOA_Item_DATA()
        {
            IEnumerable<UOA_Item> item = new List<UOA_Item>();
            IEnumerable<UOA_Item> ds = new List<UOA_Item>();
            string uri = Common3.BaseUrl + "?sap-client=" + Common3.Client;

            var Credential = new NetworkCredential(Common1.UserID, Common3.Password);
            var Handler = new HttpClientHandler { Credentials = Credential };
            HttpClient _client = new HttpClient(Handler);

            IEnumerable<UOA_Item> existingSalesData = GetAllUOAI();
            var response = await _client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();



                item = JsonConvert.DeserializeObject<IEnumerable<UOA_Item>>(content);
                if (item.Count() > 0)
                {
                    var toadd
                         = item.Where(ns => !existingSalesData.Any(es => es.REQ_NO == ns.REQ_NO && es.SAMPLE_NO == ns.SAMPLE_NO));


                    ds = UOA_IConvert(toadd);
                }
            }
            return ds;
        }



        private IEnumerable<UOA_Item> UOA_IConvert(IEnumerable<UOA_Item> Id)
        {
            var masterlist = new List<UOA_Item>();
            var ds = (from row in Id
                      select new
                      {
                          REQ_NO = row.REQ_NO,
                          INS_LOT = row.INS_LOT,
                          KUNNR = row.KUNNR,
                          CutomerName = row.CutomerName,
                          SAMPLE_NO = row.SAMPLE_NO,
                          EQUIPMENT = row.EQUIPMENT,
                          ITEM_NO = row.ITEM_NO,
                          EQP_RUN_HOUR = row.EQP_RUN_HOUR,
                          OIL_RUN_HOUR = row.OIL_RUN_HOUR,
                          DAILY_OIL_TOP = row.DAILY_OIL_TOP,
                          FILT_RUN_HOUR = row.FILT_RUN_HOUR,
                          LAST_OILCHNG_DT = row.LAST_OILCHNG_DT,
                          SAMPLE_DATE = row.SAMPLE_DATE,
                          SAMPLE_REC_DATE = row.SAMPLE_REC_DATE,
                          SAMP_QTY = row.SAMP_QTY,
                          SAMP_UNIT = row.SAMP_UNIT,
                          REMARKS = row.REMARKS,
                          CREATED_BY = row.CREATED_BY,


                      }).Distinct().ToList();
            foreach (var p in ds)
            {
                //var vmpurchases = purchases.Where(c => c.DocNo == p.DocNo);
                var master = new UOA_Item();
                master.REQ_NO = p.REQ_NO;
                master.INS_LOT = p.INS_LOT;
                master.KUNNR = p.KUNNR;
                master.CutomerName = p.CutomerName;
                master.SAMPLE_NO = p.SAMPLE_NO;
                master.EQUIPMENT = p.EQUIPMENT;
                master.ITEM_NO = p.ITEM_NO;
                master.EQP_RUN_HOUR = p.EQP_RUN_HOUR;
                master.OIL_RUN_HOUR = p.OIL_RUN_HOUR;
                master.DAILY_OIL_TOP = p.DAILY_OIL_TOP;
                master.FILT_RUN_HOUR = p.FILT_RUN_HOUR;
                master.LAST_OILCHNG_DT = p.LAST_OILCHNG_DT;
                master.SAMPLE_DATE = p.SAMPLE_DATE;
                master.SAMPLE_REC_DATE = p.SAMPLE_REC_DATE;
                master.SAMP_QTY = p.SAMP_QTY;
                master.SAMP_UNIT = p.SAMP_UNIT;
                master.CREATED_BY = p.CREATED_BY;
                master.REMARKS = p.REMARKS;
                master.status = EStatus.pending;
                //master.CREATED_ON = p.CREATED_ON;


                masterlist.Add(master);
            }
            return masterlist;
        }



        public IEnumerable<UOA_Item> GetAllPendingUOA()
        {
            return db.UOA_Item.Where(d => d.status == EStatus.pending);
        }

        public IEnumerable<UOA_Item> GetUOAItem(long ID)
        {
            return db.UOA_Item.Where(d => d.SAMPLE_NO == ID);

        }

        #endregion



        #region Get Result Data




        public async Task AddResult(ResultRecord result)
        {
            var obj = new ResultRecord();
            obj.REQ_NO = result.REQ_NO;
            obj.SAMPLE_NO = result.SAMPLE_NO;
            obj.VERWMERKM = result.VERWMERKM;
            obj.KURZTEXT = result.KURZTEXT;
            obj.Erstelldat = result.Erstelldat;
            obj.DUMMY10 = result.DUMMY10;
            obj.MERKNR = result.MERKNR;
            obj.Result = result.Result;
            obj.MBEWERTG = result.MBEWERTG;
            obj.CHAR_GROUP = result.CHAR_GROUP;
            obj.CHAR_PART3 = result.CHAR_PART3;
            obj.GRAPH = result.GRAPH;
            obj.Mresult = result.Result;



            db.ResultRecord.Add(obj);

        }

        public IEnumerable<ResultRecord> GetAllRecord()

        {
            return db.ResultRecord;
        }

        public async Task<IEnumerable<ResultRecord>> SynGetResult_DATA()
        {
            IEnumerable<ResultRecord> item = new List<ResultRecord>();
            IEnumerable<ResultRecord> ds = new List<ResultRecord>();
            string uri = Common4.BaseUrl + "?sap-client=" + Common4.Client;

            var Credential = new NetworkCredential(Common4.UserID, Common4.Password);
            var Handler = new HttpClientHandler { Credentials = Credential };
            HttpClient _client = new HttpClient(Handler);

            IEnumerable<ResultRecord> existingSalesData = GetAllRecord();
            var response = await _client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();



                item = JsonConvert.DeserializeObject<IEnumerable<ResultRecord>>(content);
                if (item.Count() > 0)
                {
                    var toadd
                         = item.Where(ns => !existingSalesData.Any(es => es.REQ_NO == ns.REQ_NO && es.SAMPLE_NO == ns.SAMPLE_NO && es.VERWMERKM == ns.VERWMERKM));


                    ds = UOA_ResultConvert(toadd);
                }
            }
            return ds;
        }



        private IEnumerable<ResultRecord> UOA_ResultConvert(IEnumerable<ResultRecord> Id)
        {
            var masterlist = new List<ResultRecord>();
            var ds = (from row in Id
                      select new
                      {
                          REQ_NO = row.REQ_NO,
                          SAMPLE_NO = row.SAMPLE_NO,
                          VERWMERKM = row.VERWMERKM,
                          KURZTEXT = row.KURZTEXT,
                          Erstelldat = row.Erstelldat,
                          DUMMY10 = row.DUMMY10,
                          MERKNR = row.MERKNR,
                          Result = row.Result,
                          Mresult = row.Mresult,
                          MBEWERTG = row.MBEWERTG,
                          CHAR_GROUP = row.CHAR_GROUP,
                          CHAR_PART3 = row.CHAR_PART3,
                          GRAPH = row.GRAPH,


                          //CREATED_ON = row.CREATED_ON,





                      }).Distinct().ToList();
            foreach (var p in ds)
            {
                //var vmpurchases = purchases.Where(c => c.DocNo == p.DocNo);
                var master = new ResultRecord();
                master.REQ_NO = p.REQ_NO;
                master.SAMPLE_NO = p.SAMPLE_NO;
                master.VERWMERKM = p.VERWMERKM;
                master.KURZTEXT = p.KURZTEXT;
                master.Erstelldat = p.Erstelldat;
                master.DUMMY10 = p.DUMMY10;
                master.MERKNR = p.MERKNR;
                master.Result = p.Result;
                master.Mresult = p.Mresult;
                master.MBEWERTG = p.MBEWERTG;
                master.CHAR_GROUP = p.CHAR_GROUP;
                master.CHAR_PART3 = p.CHAR_PART3;
                master.GRAPH = p.GRAPH;

                //master.CREATED_ON = p.CREATED_ON;


                masterlist.Add(master);
            }
            return masterlist;
        }



        public IEnumerable<ResultRecord> GetResult(long ID)
        {

            return db.ResultRecord.Where(d => d.SAMPLE_NO == ID).ToList();

        }

        public IEnumerable<UOA_Header> GetUOAMaster(long ID)
        {
            return db.UOA_Header.Where(m => m.INS_LOT == ID).ToList();
        }

        //public IEnumerable<UOA_Item> GetAllPendingUOA()
        //{
        //    return db.UOA_Item.Where(d => d.status == EStatus.pending);
        //}



        public async  Task  updateResult(ResultRecord record)
        {

            var oldone = db.ResultRecord.Where(d => d.SAMPLE_NO == record.SAMPLE_NO && d.REQ_NO == record.REQ_NO && d.VERWMERKM == record.VERWMERKM).FirstOrDefault();

           
           
              
            oldone.VERWMERKM = record.VERWMERKM;
            oldone.Result = record.Result;
            oldone.SAMPLE_NO = record.SAMPLE_NO;

            db.SaveChanges();   




        }

        #endregion





        #region Report Data

        public VMResultData GetVMResultData(long IID)
        {


            var result = new VMResultData();

            //get Header Data.

            var hd = from hdta in db.UOA_Header.Where(d => d.INS_LOT == IID).AsEnumerable()
                     select hdta;
            var hdt = hd.FirstOrDefault();

            var last5Item = (from row in db.UOA_Item
                            .OrderByDescending(d => d.SAMPLE_NO).Take(5)
                            select row).ToList();

                            //select new VMItemData
                            //{
                            //    DataReceived = row.SAMPLE_REC_DATE,
                            //    DataSampled = row.SAMPLE_REC_DATE,
                            //    DataReport = row.SAMPLE_DATE,
                            //    Sample = row.SAMPLE_NO,
                            //    OilKmH = row.OIL_RUN_HOUR,
                            //    EQiKMH = row.EQP_RUN_HOUR,
                            //    FilterKMH = row.FILT_RUN_HOUR,
                            //    TopUpD = row.DAILY_OIL_TOP,


                                //};


                var obj = new VMItemData();
            result.ItemDatas = new List<VMItemData>();  

                obj.Description ="Date Sampled";
                obj.Current = last5Item.Count() > 0 ? last5Item[0].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
                obj.Previous1 = last5Item.Count() > 1 ? last5Item[1].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
                obj.Previous2 = last5Item.Count() > 2 ? last5Item[2].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
                obj.Previous3 = last5Item.Count() > 3 ? last5Item[3].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
                obj.Previous4 = last5Item.Count() > 4 ? last5Item[4].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
                result.ItemDatas.Add(obj);

            obj= new VMItemData();
            obj.Description ="Date Received";
            obj.Current = last5Item.Count() > 0 ? last5Item[0].SAMPLE_REC_DATE!.Value.ToString("dd-MMM-yy")   : "";
            obj.Previous1 = last5Item.Count() > 1 ? last5Item[1].SAMPLE_REC_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj.Previous2 = last5Item.Count() > 2 ? last5Item[2].SAMPLE_REC_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj.Previous3 = last5Item.Count() > 3 ? last5Item[3].SAMPLE_REC_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj.Previous4 = last5Item.Count() > 4 ? last5Item[4].SAMPLE_REC_DATE!.Value.ToString("dd-MMM-yy") : "";

            result.ItemDatas.Add(obj);

            obj = new VMItemData();
            obj.Description ="Date Reported";
            obj.Current = last5Item.Count() > 0 ? last5Item[0].LAST_OILCHNG_DT!.Value.ToString("dd-MMM-yy") : "";
            obj.Previous1 = last5Item.Count() > 1 ? last5Item[1].LAST_OILCHNG_DT!.Value.ToString("dd-MMM-yy") : "";
            obj.Previous2 = last5Item.Count() > 2 ? last5Item[2].LAST_OILCHNG_DT!.Value.ToString("dd-MMM-yy") : "";
            obj.Previous3 = last5Item.Count() > 3 ? last5Item[3].LAST_OILCHNG_DT!.Value.ToString("dd-MMM-yy") : "";
            obj.Previous4 = last5Item.Count() > 4 ? last5Item[4].LAST_OILCHNG_DT!.Value.ToString("dd-MMM-yy") : "";
            result.ItemDatas.Add(obj);

            obj = new VMItemData();
            obj.Description = "Lab Number";
            obj.Current = last5Item.Count() > 0 ? last5Item[0].SAMPLE_NO.ToString() : "";
            obj.Previous1 = last5Item.Count() > 1 ? last5Item[1].SAMPLE_NO.ToString() : "";
            obj.Previous2 = last5Item.Count() > 2 ? last5Item[2].SAMPLE_NO.ToString() : "";
            obj.Previous3 = last5Item.Count() > 3 ? last5Item[3].SAMPLE_NO.ToString() : "";
            obj.Previous4 = last5Item.Count() > 4 ? last5Item[4].SAMPLE_NO.ToString() : "";
            result.ItemDatas.Add(obj);

            obj = new VMItemData();
            obj.Description ="Oil Km/Hrs";
            obj.Current = last5Item.Count() > 0 ? last5Item[0].OIL_RUN_HOUR.ToString() : "";
            obj.Previous1 = last5Item.Count() > 1 ? last5Item[1].OIL_RUN_HOUR.ToString() : "";
            obj.Previous2 = last5Item.Count() > 2 ? last5Item[2].OIL_RUN_HOUR.ToString() : "";
            obj.Previous3 = last5Item.Count() > 3 ? last5Item[3].OIL_RUN_HOUR.ToString() : "";
            obj.Previous4 = last5Item.Count() > 4 ? last5Item[4].OIL_RUN_HOUR.ToString() : "";
            result.ItemDatas.Add(obj);


            obj = new VMItemData();
            obj.Description ="Filter Km/Hrs";
            obj.Current = last5Item.Count() > 0 ? last5Item[0].FILT_RUN_HOUR.ToString() : "";
            obj.Previous1 = last5Item.Count() > 1 ? last5Item[1].FILT_RUN_HOUR.ToString() : "";
            obj.Previous2 = last5Item.Count() > 2 ? last5Item[2].FILT_RUN_HOUR.ToString() : "";
            obj.Previous3 = last5Item.Count() > 3 ? last5Item[3].FILT_RUN_HOUR.ToString() : "";
            obj.Previous4 = last5Item.Count() > 4 ? last5Item[4].FILT_RUN_HOUR.ToString() : "";
            result.ItemDatas.Add(obj);

            obj = new VMItemData();
            obj.Description ="Equipment Km/Hrs";
            obj.Current = last5Item.Count() > 0 ? last5Item[0].EQP_RUN_HOUR.ToString() : "";
            obj.Previous1 = last5Item.Count() > 1 ? last5Item[1].EQP_RUN_HOUR.ToString() : "";
            obj.Previous2 = last5Item.Count() > 2 ? last5Item[2].EQP_RUN_HOUR.ToString() : "";
            obj.Previous3 = last5Item.Count() > 3 ? last5Item[3].EQP_RUN_HOUR.ToString() : "";
            obj.Previous4 = last5Item.Count() > 4 ? last5Item[4].EQP_RUN_HOUR.ToString() : "";
            result.ItemDatas.Add(obj);

            obj = new VMItemData();
            obj.Description ="Make-up Liters";
            obj.Current = last5Item.Count() > 0 ? last5Item[0].DAILY_OIL_TOP.ToString() : "";
            obj.Previous1 = last5Item.Count() > 1 ? last5Item[1].DAILY_OIL_TOP.ToString() : "";
            obj.Previous2 = last5Item.Count() > 2 ? last5Item[2].DAILY_OIL_TOP.ToString() : "";
            obj.Previous3 = last5Item.Count() > 3 ? last5Item[3].DAILY_OIL_TOP.ToString() : "";
            obj.Previous4 = last5Item.Count() > 4 ? last5Item[4].DAILY_OIL_TOP.ToString() : "";
            result.ItemDatas.Add(obj);








            

            //get Graph A Data.


            var last5GpAData = from row in db.ResultRecord.Where(d => d.REQ_NO == hdt.INS_LOT && d.CHAR_GROUP == "A" )
  
                               select new VMGraph_A_Data
                               {
                                   SAMPLE_NO = row.SAMPLE_NO,
                                   Erstelldat = row.Erstelldat,
                                   VERWMERKM = row.VERWMERKM,
                                   KURZTEXT = row.KURZTEXT,
                                   CHAR_PART3 = row.CHAR_PART3,
                                   Result = row.Result,
                                   Mresult = row.Mresult,
                                   DUMMY10 = row.DUMMY10,


                               };


            var query = from item in last5Item
                        join res in last5GpAData
                             on item.SAMPLE_NO equals res.SAMPLE_NO
                        select new
                        {
                            res.KURZTEXT,
                            res.Result,
                            res.DUMMY10,
                            res.Erstelldat,
                            res.VERWMERKM,
                            res.CHAR_PART3,
                        };
            query.ToList();
            var rd = (from item in query
                          select new
                          {
                              KURZTEXT = item.KURZTEXT,
                              DUMMY10 = item.DUMMY10,
                              Code = item.VERWMERKM,
                              Chart = item.CHAR_PART3,
                             
                          })
              .ToList()
              .Distinct();


          

            var obj1 = new Graph_A_Data();
            result.A_Datas = new List<Graph_A_Data>();


            obj1 = new Graph_A_Data();
            obj1.Description = "Date Reportd";
            obj1.Current = last5Item.Count() > 0 ? last5Item[0].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj1.Previous1 = last5Item.Count() > 1 ? last5Item[1].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj1.Previous2 = last5Item.Count() > 2 ? last5Item[2].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj1.Previous3 = last5Item.Count() > 3 ? last5Item[3].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj1.Previous4 = last5Item.Count() > 4 ? last5Item[4].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            result.A_Datas.Add(obj1);


            foreach (var dt in rd )
            {
               var list11 = query.Where(d => d.VERWMERKM == dt.Code).OrderByDescending(e => e.Erstelldat).ToList();

                {

                 
                    obj1 = new Graph_A_Data();
                    obj1.Description = dt.KURZTEXT;
                    obj1.Current = list11.Count() > 0 ? list11[0].Result.ToString() : "";
                    obj1.Previous1 = list11.Count() > 1 ? list11[1].Result.ToString("") : "";
                    obj1.Previous2 = list11.Count() > 2 ? list11[2].Result.ToString("") : "";
                    obj1.Previous3 = list11.Count() > 3 ? list11[3].Result.ToString() : "";
                    obj1.Previous4 = list11.Count() > 4 ? list11[4].Result.ToString() : "";
                   
                }

                result.A_Datas.Add(obj1);


            }


           

            //get Graph B List Data.


            var last5GpBData = from row in db.ResultRecord.Where(d => d.REQ_NO == hdt.INS_LOT && d.CHAR_GROUP == "B")

                               select new VMGraph_B_Data
                               {
                                   SAMPLE_NO = row.SAMPLE_NO,
                                   Erstelldat = row.Erstelldat,
                                   VERWMERKM = row.VERWMERKM,
                                   KURZTEXT = row.KURZTEXT,
                                   CHAR_PART3 = row.CHAR_PART3,
                                   Result = row.Result,
                                   Mresult = row.Mresult,
                                   DUMMY10 = row.DUMMY10,


                               };


            var query1 = from item in last5Item
                        join res in last5GpBData
                             on item.SAMPLE_NO equals res.SAMPLE_NO
                        select new
                        {
                            res.KURZTEXT,
                            res.Result,
                            res.DUMMY10,
                            res.Erstelldat,
                            res.VERWMERKM,
                            res.CHAR_PART3,
                        };
            query.ToList();
            var rd1 = (from item in query1
                      select new
                      {
                          KURZTEXT = item.KURZTEXT,
                          DUMMY10 = item.DUMMY10,
                          Code = item.VERWMERKM,
                          Chart = item.CHAR_PART3,

                      })
              .ToList()
              .Distinct();




            var obj2= new Graph_B_Data();
            result.B_Datas = new List<Graph_B_Data>();


            obj2 = new Graph_B_Data();
            obj2.Description = "Date Reportd";
            obj2.Current = last5Item.Count() > 0 ? last5Item[0].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj2.Previous1 = last5Item.Count() > 1 ? last5Item[1].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj2.Previous2 = last5Item.Count() > 2 ? last5Item[2].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj2.Previous3 = last5Item.Count() > 3 ? last5Item[3].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj2.Previous4 = last5Item.Count() > 4 ? last5Item[4].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            result.B_Datas.Add(obj2);


            foreach (var dt in rd1)
            {
                var list11 = query1.Where(d => d.VERWMERKM == dt.Code).OrderByDescending(e => e.Erstelldat).ToList();

                {


                    obj2 = new Graph_B_Data();
                    obj2.Description = dt.KURZTEXT;
                    obj2.Current = list11.Count() > 0 ? list11[0].Result.ToString() : "";
                    obj2.Previous1 = list11.Count() > 1 ? list11[1].Result.ToString("") : "";
                    obj2.Previous2 = list11.Count() > 2 ? list11[2].Result.ToString("") : "";
                    obj2.Previous3 = list11.Count() > 3 ? list11[3].Result.ToString() : "";
                    obj2.Previous4 = list11.Count() > 4 ? list11[4].Result.ToString() : "";

                }

                result.B_Datas.Add(obj2);


            }




            //var last5GpBData = from row in db.ResultRecord.Where(d => d.REQ_NO == hdt.INS_LOT && d.CHAR_GROUP == "B")

            //                   select new VMGraph_B_Data
            //                   {
            //                       SAMPLE_NO = row.SAMPLE_NO,
            //                       Erstelldat = row.Erstelldat,
            //                       VERWMERKM = row.VERWMERKM,
            //                       KURZTEXT = row.KURZTEXT,
            //                       CHAR_PART3 = row.CHAR_PART3,
            //                       Result = row.Result,
            //                       Mresult = row.Mresult,
            //                       DUMMY10 = row.DUMMY10,


            //                   };





            //get Graph C Data.



            //get Graph C List Data.


            var last5CData = from row in db.ResultRecord.Where(d => d.REQ_NO == hdt.INS_LOT && d.CHAR_GROUP == "C")

                               select new VMGraph_C_Data
                               {
                                   SAMPLE_NO = row.SAMPLE_NO,
                                   Erstelldat = row.Erstelldat,
                                   VERWMERKM = row.VERWMERKM,
                                   KURZTEXT = row.KURZTEXT,
                                   CHAR_PART3 = row.CHAR_PART3,
                                   Result = row.Result,
                                   Mresult = row.Mresult,
                                   DUMMY10 = row.DUMMY10,


                               };


            var query2 = from item in last5Item
                         join res in last5CData
                              on item.SAMPLE_NO equals res.SAMPLE_NO
                         select new
                         {
                             res.KURZTEXT,
                             res.Result,
                             res.DUMMY10,
                             res.Erstelldat,
                             res.VERWMERKM,
                             res.CHAR_PART3,
                         };
            query.ToList();
            var rd2 = (from item in query2
                       select new
                       {
                           KURZTEXT = item.KURZTEXT,
                           DUMMY10 = item.DUMMY10,
                           Code = item.VERWMERKM,
                           Chart = item.CHAR_PART3,

                       })
              .ToList()
              .Distinct();




            var obj3 = new Graph_C_Data();
            result.C_Datas = new List<Graph_C_Data>();


            obj3 = new Graph_C_Data();
            obj3.Description = "Date Reportd";
            obj3.Current = last5Item.Count() > 0 ? last5Item[0].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj3.Previous1 = last5Item.Count() > 1 ? last5Item[1].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj3.Previous2 = last5Item.Count() > 2 ? last5Item[2].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj3.Previous3 = last5Item.Count() > 3 ? last5Item[3].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj3.Previous4 = last5Item.Count() > 4 ? last5Item[4].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            result.C_Datas.Add(obj3);


            foreach (var dt in rd2)
            {
                var list11 = query2.Where(d => d.VERWMERKM == dt.Code).OrderByDescending(e => e.Erstelldat).ToList();

                {


                    obj3 = new Graph_C_Data();
                    obj3.Description = dt.KURZTEXT;
                    obj3.Current = list11.Count() > 0 ? list11[0].Result.ToString() : "";
                    obj3.Previous1 = list11.Count() > 1 ? list11[1].Result.ToString("") : "";
                    obj3.Previous2 = list11.Count() > 2 ? list11[2].Result.ToString("") : "";
                    obj3.Previous3 = list11.Count() > 3 ? list11[3].Result.ToString() : "";
                    obj3.Previous4 = list11.Count() > 4 ? list11[4].Result.ToString() : "";

                }

                result.C_Datas.Add(obj3);


            }


            var last5GpCData = from row in db.ResultRecord.Where(d => d.REQ_NO == hdt.INS_LOT && d.CHAR_GROUP == "C")

                               select new VMGraph_C_Data
                               {
                                   SAMPLE_NO = row.SAMPLE_NO,
                                   Erstelldat = row.Erstelldat,
                                   VERWMERKM = row.VERWMERKM,
                                   KURZTEXT = row.KURZTEXT,
                                   CHAR_PART3 = row.CHAR_PART3,
                                   Result = row.Result,
                                   Mresult = row.Mresult,
                                   DUMMY10 = row.DUMMY10,


                               };



            result.EqpNo = hdt.EQUIPMENT;
            result.Req_No = hdt.REQ_NO;
            result.CustAddress = hdt.CUST_ADDRESS;
            result.CustName = hdt.CustomerName;
            result.CustContact = hdt.Contact1_Name;
            result.OilName = hdt.OIL_NAME;
            result.Model = hdt.EQUIPMENT_MODEL;
            result.Serial = hdt.EQUIPMENT_SERIAL;
            result.Meker = hdt.EQUIPMENT_MAKE;
            result.Graph_A_Datas = last5GpAData.ToList();
            //result.Graph_B_Datas = last5GpBData.ToList();
            result.Graph_C_Datas = last5GpCData.ToList();


           


            return result;
        }



        public VMResultData GetVMResultByDateRang(DateTime Start, DateTime End, long ID, int EID)
        {

            var result = new VMResultData();

            var eqp = GETEQP(EID);


            //get Header Data.

            var hd = from hdta in db.UOA_Header.Where(d => d.KUNNR == ID && d.EQUIPMENT == eqp.EQUIPMENT 
                     && d.EQUIPMENT_SERIAL == eqp.EQUIPMENT_SERIAL ).AsEnumerable()
                     select hdta;
            var hdt = hd.FirstOrDefault();

            var last5Item = (from row in db.UOA_Item.Where(d=>d.SAMPLE_DATE >= Start && d.SAMPLE_DATE <= End && d.REQ_NO == hdt.REQ_NO)
                            .OrderByDescending(d => d.SAMPLE_NO)
                             select row).ToList();

            

            var obj = new VMItemData();
            result.ItemDatas = new List<VMItemData>();

            obj.Description = "Date Sampled";
            obj.Current = last5Item.Count() > 0 ? last5Item[0].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj.Previous1 = last5Item.Count() > 1 ? last5Item[1].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj.Previous2 = last5Item.Count() > 2 ? last5Item[2].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj.Previous3 = last5Item.Count() > 3 ? last5Item[3].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj.Previous4 = last5Item.Count() > 4 ? last5Item[4].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj.Previous5 = last5Item.Count() > 5 ? last5Item[5].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj.Previous6 = last5Item.Count() > 6 ? last5Item[6].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj.Previous7 = last5Item.Count() > 7 ? last5Item[7].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj.Previous8 = last5Item.Count() > 8 ? last5Item[8].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj.Previous9 = last5Item.Count() > 9 ? last5Item[9].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj.Previous10 = last5Item.Count() > 10 ? last5Item[10].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj.Previous11 = last5Item.Count() > 11 ? last5Item[11].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj.Previous12 = last5Item.Count() > 12 ? last5Item[12].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj.Previous13 = last5Item.Count() > 13 ? last5Item[13].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj.Previous14 = last5Item.Count() > 14 ? last5Item[14].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            


            result.ItemDatas.Add(obj);

            obj = new VMItemData();
            obj.Description = "Date Received";
            obj.Current = last5Item.Count() > 0 ? last5Item[0].SAMPLE_REC_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj.Previous1 = last5Item.Count() > 1 ? last5Item[1].SAMPLE_REC_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj.Previous2 = last5Item.Count() > 2 ? last5Item[2].SAMPLE_REC_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj.Previous3 = last5Item.Count() > 3 ? last5Item[3].SAMPLE_REC_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj.Previous4 = last5Item.Count() > 4 ? last5Item[4].SAMPLE_REC_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj.Previous5 = last5Item.Count() > 5 ? last5Item[5].SAMPLE_REC_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj.Previous6 = last5Item.Count() > 6 ? last5Item[6].SAMPLE_REC_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj.Previous7 = last5Item.Count() > 7 ? last5Item[7].SAMPLE_REC_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj.Previous8 = last5Item.Count() > 8 ? last5Item[8].SAMPLE_REC_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj.Previous9 = last5Item.Count() > 9 ? last5Item[9].SAMPLE_REC_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj.Previous10 = last5Item.Count() > 10 ? last5Item[10].SAMPLE_REC_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj.Previous11 = last5Item.Count() > 11 ? last5Item[11].SAMPLE_REC_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj.Previous12 = last5Item.Count() > 12 ? last5Item[12].SAMPLE_REC_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj.Previous13 = last5Item.Count() > 13 ? last5Item[13].SAMPLE_REC_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj.Previous14 = last5Item.Count() > 14 ? last5Item[14].SAMPLE_REC_DATE!.Value.ToString("dd-MMM-yy") : "";

            result.ItemDatas.Add(obj);

            obj = new VMItemData();
            obj.Description = "Date Reported";
            obj.Current = last5Item.Count() > 0 ? last5Item[0].LAST_OILCHNG_DT!.Value.ToString("dd-MMM-yy") : "";
            obj.Previous1 = last5Item.Count() > 1 ? last5Item[1].LAST_OILCHNG_DT!.Value.ToString("dd-MMM-yy") : "";
            obj.Previous2 = last5Item.Count() > 2 ? last5Item[2].LAST_OILCHNG_DT!.Value.ToString("dd-MMM-yy") : "";
            obj.Previous3 = last5Item.Count() > 3 ? last5Item[3].LAST_OILCHNG_DT!.Value.ToString("dd-MMM-yy") : "";
            obj.Previous4 = last5Item.Count() > 4 ? last5Item[4].LAST_OILCHNG_DT!.Value.ToString("dd-MMM-yy") : "";
            obj.Previous5 = last5Item.Count() > 5 ? last5Item[5].LAST_OILCHNG_DT!.Value.ToString("dd-MMM-yy") : "";
            obj.Previous6 = last5Item.Count() > 6 ? last5Item[6].LAST_OILCHNG_DT!.Value.ToString("dd-MMM-yy") : "";
            obj.Previous7 = last5Item.Count() > 7 ? last5Item[7].LAST_OILCHNG_DT!.Value.ToString("dd-MMM-yy") : "";
            obj.Previous8 = last5Item.Count() > 8 ? last5Item[8].LAST_OILCHNG_DT!.Value.ToString("dd-MMM-yy") : "";
            obj.Previous9 = last5Item.Count() > 9 ? last5Item[9].LAST_OILCHNG_DT!.Value.ToString("dd-MMM-yy") : "";
            obj.Previous10 = last5Item.Count() > 10 ? last5Item[10].LAST_OILCHNG_DT!.Value.ToString("dd-MMM-yy") : "";
            obj.Previous11 = last5Item.Count() > 11 ? last5Item[11].LAST_OILCHNG_DT!.Value.ToString("dd-MMM-yy") : "";
            obj.Previous12 = last5Item.Count() > 12 ? last5Item[12].LAST_OILCHNG_DT!.Value.ToString("dd-MMM-yy") : "";
            obj.Previous13 = last5Item.Count() > 13? last5Item[13].LAST_OILCHNG_DT!.Value.ToString("dd-MMM-yy") : "";
            obj.Previous14 = last5Item.Count() > 14 ? last5Item[14].LAST_OILCHNG_DT!.Value.ToString("dd-MMM-yy") : "";

            result.ItemDatas.Add(obj);

            obj = new VMItemData();
            obj.Description = "Lab Number";
            obj.Current = last5Item.Count() > 0 ? last5Item[0].SAMPLE_NO.ToString() : "";
            obj.Previous1 = last5Item.Count() > 1 ? last5Item[1].SAMPLE_NO.ToString() : "";
            obj.Previous2 = last5Item.Count() > 2 ? last5Item[2].SAMPLE_NO.ToString() : "";
            obj.Previous3 = last5Item.Count() > 3 ? last5Item[3].SAMPLE_NO.ToString() : "";
            obj.Previous4 = last5Item.Count() > 4 ? last5Item[4].SAMPLE_NO.ToString() : "";
            obj.Previous5 = last5Item.Count() > 5 ? last5Item[5].SAMPLE_NO.ToString() : "";
            obj.Previous6 = last5Item.Count() > 6 ? last5Item[6].SAMPLE_NO.ToString() : "";
            obj.Previous7 = last5Item.Count() > 7 ? last5Item[7].SAMPLE_NO.ToString() : "";
            obj.Previous8 = last5Item.Count() > 8 ? last5Item[8].SAMPLE_NO.ToString() : "";
            obj.Previous9 = last5Item.Count() > 9? last5Item[9].SAMPLE_NO.ToString() : "";
            obj.Previous10 = last5Item.Count() > 10 ? last5Item[10].SAMPLE_NO.ToString() : "";
            obj.Previous11 = last5Item.Count() > 11 ? last5Item[11].SAMPLE_NO.ToString() : "";
            obj.Previous12 = last5Item.Count() > 12 ? last5Item[12].SAMPLE_NO.ToString() : "";
            obj.Previous13 = last5Item.Count() > 13 ? last5Item[13].SAMPLE_NO.ToString() : "";
            obj.Previous14 = last5Item.Count() > 14 ? last5Item[14].SAMPLE_NO.ToString() : "";
            
            result.ItemDatas.Add(obj);

            obj = new VMItemData();
            obj.Description = "Oil Km/Hrs";
            obj.Current = last5Item.Count() > 0 ? last5Item[0].OIL_RUN_HOUR.ToString() : "";
            obj.Previous1 = last5Item.Count() > 1 ? last5Item[1].OIL_RUN_HOUR.ToString() : "";
            obj.Previous2 = last5Item.Count() > 2 ? last5Item[2].OIL_RUN_HOUR.ToString() : "";
            obj.Previous3 = last5Item.Count() > 3 ? last5Item[3].OIL_RUN_HOUR.ToString() : "";
            obj.Previous4 = last5Item.Count() > 4 ? last5Item[4].OIL_RUN_HOUR.ToString() : "";
            obj.Previous5 = last5Item.Count() > 5 ? last5Item[5].OIL_RUN_HOUR.ToString() : "";
            obj.Previous6 = last5Item.Count() > 6 ? last5Item[6].OIL_RUN_HOUR.ToString() : "";
            obj.Previous7 = last5Item.Count() > 7 ? last5Item[7].OIL_RUN_HOUR.ToString() : "";
            obj.Previous8 = last5Item.Count() > 8 ? last5Item[8].OIL_RUN_HOUR.ToString() : "";
            obj.Previous9 = last5Item.Count() > 9 ? last5Item[9].OIL_RUN_HOUR.ToString() : "";
            obj.Previous10 = last5Item.Count() > 10 ? last5Item[10].OIL_RUN_HOUR.ToString() : "";
            obj.Previous11 = last5Item.Count() > 11 ? last5Item[11].OIL_RUN_HOUR.ToString() : "";
            obj.Previous12 = last5Item.Count() > 12 ? last5Item[12].OIL_RUN_HOUR.ToString() : "";
            obj.Previous13 = last5Item.Count() > 13 ? last5Item[13].OIL_RUN_HOUR.ToString() : "";
            obj.Previous14 = last5Item.Count() > 14 ? last5Item[14].OIL_RUN_HOUR.ToString() : "";
            
            result.ItemDatas.Add(obj);


            obj = new VMItemData();
            obj.Description = "Filter Km/Hrs";
            obj.Current = last5Item.Count() > 0 ? last5Item[0].FILT_RUN_HOUR.ToString() : "";
            obj.Previous1 = last5Item.Count() > 1 ? last5Item[1].FILT_RUN_HOUR.ToString() : "";
            obj.Previous2 = last5Item.Count() > 2 ? last5Item[2].FILT_RUN_HOUR.ToString() : "";
            obj.Previous3 = last5Item.Count() > 3 ? last5Item[3].FILT_RUN_HOUR.ToString() : "";
            obj.Previous4 = last5Item.Count() > 4 ? last5Item[4].FILT_RUN_HOUR.ToString() : "";
            obj.Previous5 = last5Item.Count() > 5 ? last5Item[5].FILT_RUN_HOUR.ToString() : "";
            obj.Previous6 = last5Item.Count() > 6 ? last5Item[6].FILT_RUN_HOUR.ToString() : "";
            obj.Previous7 = last5Item.Count() > 7 ? last5Item[7].FILT_RUN_HOUR.ToString() : "";
            obj.Previous8 = last5Item.Count() > 8 ? last5Item[8].FILT_RUN_HOUR.ToString() : "";
            obj.Previous9 = last5Item.Count() > 9 ? last5Item[9].FILT_RUN_HOUR.ToString() : "";
            obj.Previous10 = last5Item.Count() > 10 ? last5Item[10].FILT_RUN_HOUR.ToString() : "";
            obj.Previous11 = last5Item.Count() > 11 ? last5Item[11].FILT_RUN_HOUR.ToString() : "";
            obj.Previous12 = last5Item.Count() > 12? last5Item[12].FILT_RUN_HOUR.ToString() : "";
            obj.Previous13 = last5Item.Count() > 13 ? last5Item[13].FILT_RUN_HOUR.ToString() : "";
            obj.Previous14 = last5Item.Count() > 14 ? last5Item[14].FILT_RUN_HOUR.ToString() : "";
            
            result.ItemDatas.Add(obj);

            obj = new VMItemData();
            obj.Description = "Equip. Km/Hrs";
            obj.Current = last5Item.Count() > 0 ? last5Item[0].EQP_RUN_HOUR.ToString() : "";
            obj.Previous1 = last5Item.Count() > 1 ? last5Item[1].EQP_RUN_HOUR.ToString() : "";
            obj.Previous2 = last5Item.Count() > 2 ? last5Item[2].EQP_RUN_HOUR.ToString() : "";
            obj.Previous3 = last5Item.Count() > 3 ? last5Item[3].EQP_RUN_HOUR.ToString() : "";
            obj.Previous4 = last5Item.Count() > 4 ? last5Item[4].EQP_RUN_HOUR.ToString() : "";
            obj.Previous5 = last5Item.Count() > 5 ? last5Item[5].EQP_RUN_HOUR.ToString() : "";
            obj.Previous6 = last5Item.Count() > 6 ? last5Item[6].EQP_RUN_HOUR.ToString() : "";
            obj.Previous7 = last5Item.Count() > 7 ? last5Item[7].EQP_RUN_HOUR.ToString() : "";
            obj.Previous8 = last5Item.Count() > 8 ? last5Item[8].EQP_RUN_HOUR.ToString() : "";
            obj.Previous9 = last5Item.Count() > 9 ? last5Item[9].EQP_RUN_HOUR.ToString() : "";
            obj.Previous10 = last5Item.Count() > 10 ? last5Item[10].EQP_RUN_HOUR.ToString() : "";
            obj.Previous11 = last5Item.Count() > 11 ? last5Item[11].EQP_RUN_HOUR.ToString() : "";
            obj.Previous12 = last5Item.Count() > 12 ? last5Item[12].EQP_RUN_HOUR.ToString() : "";
            obj.Previous13 = last5Item.Count() > 13 ? last5Item[13].EQP_RUN_HOUR.ToString() : "";
            obj.Previous14 = last5Item.Count() > 14 ? last5Item[14].EQP_RUN_HOUR.ToString() : "";
            
            result.ItemDatas.Add(obj);

            obj = new VMItemData();
            obj.Description = "Make-up Liters";
            obj.Current = last5Item.Count() > 0 ? last5Item[0].DAILY_OIL_TOP.ToString() : "";
            obj.Previous1 = last5Item.Count() > 1 ? last5Item[1].DAILY_OIL_TOP.ToString() : "";
            obj.Previous2 = last5Item.Count() > 2 ? last5Item[2].DAILY_OIL_TOP.ToString() : "";
            obj.Previous3 = last5Item.Count() > 3 ? last5Item[3].DAILY_OIL_TOP.ToString() : "";
            obj.Previous4 = last5Item.Count() > 4 ? last5Item[4].DAILY_OIL_TOP.ToString() : "";
            obj.Previous5 = last5Item.Count() > 5 ? last5Item[5].DAILY_OIL_TOP.ToString() : "";
            obj.Previous6 = last5Item.Count() > 6 ? last5Item[6].DAILY_OIL_TOP.ToString() : "";
            obj.Previous7 = last5Item.Count() > 7 ? last5Item[7].DAILY_OIL_TOP.ToString() : "";
            obj.Previous8 = last5Item.Count() > 8 ? last5Item[8].DAILY_OIL_TOP.ToString() : "";
            obj.Previous9 = last5Item.Count() > 9 ? last5Item[9].DAILY_OIL_TOP.ToString() : "";
            obj.Previous10 = last5Item.Count() > 10 ? last5Item[10].DAILY_OIL_TOP.ToString() : "";
            obj.Previous11 = last5Item.Count() > 11 ? last5Item[11].DAILY_OIL_TOP.ToString() : "";
            obj.Previous12 = last5Item.Count() > 12 ? last5Item[12].DAILY_OIL_TOP.ToString() : "";
            obj.Previous13 = last5Item.Count() > 13 ? last5Item[13].DAILY_OIL_TOP.ToString() : "";
            obj.Previous14 = last5Item.Count() > 14 ? last5Item[14].DAILY_OIL_TOP.ToString() : "";
            result.ItemDatas.Add(obj);










            //get Graph A Data.


            var last5GpAData = from row in db.ResultRecord.Where(d => d.REQ_NO == hdt.INS_LOT && d.CHAR_GROUP == "A" &&  d.Erstelldat >= Start && d.Erstelldat <= End)

                               select new VMGraph_A_Data
                               {
                                   SAMPLE_NO = row.SAMPLE_NO,
                                   Erstelldat = row.Erstelldat,
                                   VERWMERKM = row.VERWMERKM,
                                   KURZTEXT = row.KURZTEXT,
                                   CHAR_PART3 = row.CHAR_PART3,
                                   Result = row.Result,
                                   Mresult = row.Mresult,
                                   DUMMY10 = row.DUMMY10,


                               };


            var query = from item in last5Item
                        join res in last5GpAData
                             on item.SAMPLE_NO equals res.SAMPLE_NO
                        select new
                        {
                            res.KURZTEXT,
                            res.Result,
                            res.DUMMY10,
                            res.Erstelldat,
                            res.VERWMERKM,
                            res.CHAR_PART3,
                        };
            query.ToList();
            var rd = (from item in query
                      select new
                      {
                          KURZTEXT = item.KURZTEXT,
                          DUMMY10 = item.DUMMY10,
                          Code = item.VERWMERKM,
                          Chart = item.CHAR_PART3,

                      })
              .ToList()
              .Distinct();




            var obj1 = new Graph_A_Data();
            result.A_Datas = new List<Graph_A_Data>();


            obj1 = new Graph_A_Data();
            obj1.Description = "Date Reportd";
            obj1.Current = last5Item.Count() > 0 ? last5Item[0].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj1.Previous1 = last5Item.Count() > 1 ? last5Item[1].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj1.Previous2 = last5Item.Count() > 2 ? last5Item[2].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj1.Previous3 = last5Item.Count() > 3 ? last5Item[3].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj1.Previous4 = last5Item.Count() > 4 ? last5Item[4].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj1.Previous5 = last5Item.Count() > 5 ? last5Item[5].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj1.Previous6 = last5Item.Count() > 6 ? last5Item[6].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj1.Previous7 = last5Item.Count() > 7 ? last5Item[7].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj1.Previous8 = last5Item.Count() > 8 ? last5Item[8].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj1.Previous9 = last5Item.Count() > 9 ? last5Item[9].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj1.Previous10 = last5Item.Count() > 10 ? last5Item[10].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj1.Previous11= last5Item.Count() > 11? last5Item[11].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj1.Previous12 = last5Item.Count() > 12 ? last5Item[12].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj1.Previous13 = last5Item.Count() > 13 ? last5Item[13].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj1.Previous14 = last5Item.Count() > 14 ? last5Item[14].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            
            result.A_Datas.Add(obj1);


            foreach (var dt in rd)
            {
                var list11 = query.Where(d => d.VERWMERKM == dt.Code).OrderByDescending(e => e.Erstelldat).ToList();

                {


                    obj1 = new Graph_A_Data();
                    obj1.Description = dt.KURZTEXT;
                    obj1.Current = list11.Count() > 0 ? list11[0].Result.ToString() : "";
                    obj1.Previous1 = list11.Count() > 1 ? list11[1].Result.ToString("") : "";
                    obj1.Previous2 = list11.Count() > 2 ? list11[2].Result.ToString("") : "";
                    obj1.Previous3 = list11.Count() > 3 ? list11[3].Result.ToString() : "";
                    obj1.Previous4 = list11.Count() > 4 ? list11[4].Result.ToString() : "";
                    obj1.Previous5 = list11.Count() > 5 ? list11[4].Result.ToString() : "";
                    obj1.Previous6 = list11.Count() > 6 ? list11[4].Result.ToString() : "";
                    obj1.Previous7 = list11.Count() > 7 ? list11[4].Result.ToString() : "";
                    obj1.Previous8 = list11.Count() > 8 ? list11[4].Result.ToString() : "";
                    obj1.Previous9 = list11.Count() > 9 ? list11[4].Result.ToString() : "";
                    obj1.Previous10 = list11.Count() > 10 ? list11[4].Result.ToString() : "";
                    obj1.Previous11 = list11.Count() > 11 ? list11[4].Result.ToString() : "";
                    obj1.Previous12 = list11.Count() > 12 ? list11[4].Result.ToString() : "";
                    obj1.Previous13 = list11.Count() > 13 ? list11[4].Result.ToString() : "";
                    obj1.Previous14 = list11.Count() > 14 ? list11[4].Result.ToString() : "";
                   

                }

                result.A_Datas.Add(obj1);


            }




            //get Graph B List Data.


            var last5GpBData = from row in db.ResultRecord.Where(d => d.REQ_NO == hdt.INS_LOT && d.CHAR_GROUP == "B")

                               select new VMGraph_B_Data
                               {
                                   SAMPLE_NO = row.SAMPLE_NO,
                                   Erstelldat = row.Erstelldat,
                                   VERWMERKM = row.VERWMERKM,
                                   KURZTEXT = row.KURZTEXT,
                                   CHAR_PART3 = row.CHAR_PART3,
                                   Result = row.Result,
                                   Mresult = row.Mresult,
                                   DUMMY10 = row.DUMMY10,


                               };


            var query1 = from item in last5Item
                         join res in last5GpBData
                              on item.SAMPLE_NO equals res.SAMPLE_NO
                         select new
                         {
                             res.KURZTEXT,
                             res.Result,
                             res.DUMMY10,
                             res.Erstelldat,
                             res.VERWMERKM,
                             res.CHAR_PART3,
                         };
            query.ToList();
            var rd1 = (from item in query1
                       select new
                       {
                           KURZTEXT = item.KURZTEXT,
                           DUMMY10 = item.DUMMY10,
                           Code = item.VERWMERKM,
                           Chart = item.CHAR_PART3,

                       })
              .ToList()
              .Distinct();




            var obj2 = new Graph_B_Data();
            result.B_Datas = new List<Graph_B_Data>();


            obj2 = new Graph_B_Data();
            obj2.Description = "Date Reportd";
            obj2.Current = last5Item.Count() > 0 ? last5Item[0].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj2.Previous1 = last5Item.Count() > 1 ? last5Item[1].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj2.Previous2 = last5Item.Count() > 2 ? last5Item[2].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj2.Previous3 = last5Item.Count() > 3 ? last5Item[3].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj2.Previous4 = last5Item.Count() > 4 ? last5Item[4].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj2.Previous5 = last5Item.Count() > 5 ? last5Item[5].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj2.Previous6 = last5Item.Count() > 6 ? last5Item[6].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj2.Previous7 = last5Item.Count() > 7 ? last5Item[7].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj2.Previous8 = last5Item.Count() > 8 ? last5Item[8].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj2.Previous9 = last5Item.Count() > 9 ? last5Item[9].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj2.Previous10 = last5Item.Count() > 10 ? last5Item[10].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj2.Previous11 = last5Item.Count() > 11 ? last5Item[11].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj2.Previous12 = last5Item.Count() > 12 ? last5Item[12].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj2.Previous13 = last5Item.Count() > 13 ? last5Item[13].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj2.Previous14 = last5Item.Count() > 14 ? last5Item[14].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            result.B_Datas.Add(obj2);


            foreach (var dt in rd1)
            {
                var list11 = query1.Where(d => d.VERWMERKM == dt.Code).OrderByDescending(e => e.Erstelldat).ToList();

                {


                    obj2 = new Graph_B_Data();
                    obj2.Description = dt.KURZTEXT;
                    obj2.Current = list11.Count() > 0 ? list11[0].Result.ToString() : "";
                    obj2.Previous1 = list11.Count() > 1 ? list11[1].Result.ToString("") : "";
                    obj2.Previous2 = list11.Count() > 2 ? list11[2].Result.ToString("") : "";
                    obj2.Previous3 = list11.Count() > 3 ? list11[3].Result.ToString() : "";
                    obj2.Previous4 = list11.Count() > 4 ? list11[4].Result.ToString() : "";
                    obj2.Previous5 = list11.Count() > 5 ? list11[4].Result.ToString() : "";
                    obj2.Previous6 = list11.Count() > 6 ? list11[4].Result.ToString() : "";
                    obj2.Previous7 = list11.Count() > 7 ? list11[4].Result.ToString() : "";
                    obj2.Previous8 = list11.Count() > 8 ? list11[4].Result.ToString() : "";
                    obj2.Previous9 = list11.Count() > 9 ? list11[4].Result.ToString() : "";
                    obj2.Previous10 = list11.Count() > 10 ? list11[4].Result.ToString() : "";
                    obj2.Previous11 = list11.Count() > 11 ? list11[4].Result.ToString() : "";
                    obj2.Previous12 = list11.Count() > 12? list11[4].Result.ToString() : "";
                    obj2.Previous13 = list11.Count() > 13 ? list11[4].Result.ToString() : "";
                    obj2.Previous14 = list11.Count() > 14 ? list11[4].Result.ToString() : "";
                   
                }

                result.B_Datas.Add(obj2);


            }







            //get Graph C Data.



            //get Graph C List Data.


            var last5CData = from row in db.ResultRecord.Where(d => d.REQ_NO == hdt.INS_LOT && d.CHAR_GROUP == "C")

                             select new VMGraph_C_Data
                             {
                                 SAMPLE_NO = row.SAMPLE_NO,
                                 Erstelldat = row.Erstelldat,
                                 VERWMERKM = row.VERWMERKM,
                                 KURZTEXT = row.KURZTEXT,
                                 CHAR_PART3 = row.CHAR_PART3,
                                 Result = row.Result,
                                 Mresult = row.Mresult,
                                 DUMMY10 = row.DUMMY10,


                             };


            var query2 = from item in last5Item
                         join res in last5CData
                              on item.SAMPLE_NO equals res.SAMPLE_NO
                         select new
                         {
                             res.KURZTEXT,
                             res.Result,
                             res.DUMMY10,
                             res.Erstelldat,
                             res.VERWMERKM,
                             res.CHAR_PART3,
                         };
            query.ToList();
            var rd2 = (from item in query2
                       select new
                       {
                           KURZTEXT = item.KURZTEXT,
                           DUMMY10 = item.DUMMY10,
                           Code = item.VERWMERKM,
                           Chart = item.CHAR_PART3,

                       })
              .ToList()
              .Distinct();




            var obj3 = new Graph_C_Data();
            result.C_Datas = new List<Graph_C_Data>();


            obj3 = new Graph_C_Data();
            obj3.Description = "Date Reportd";
            obj3.Current = last5Item.Count() > 0 ? last5Item[0].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj3.Previous1 = last5Item.Count() > 1 ? last5Item[1].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj3.Previous2 = last5Item.Count() > 2 ? last5Item[2].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj3.Previous3 = last5Item.Count() > 3 ? last5Item[3].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj3.Previous4 = last5Item.Count() > 4 ? last5Item[4].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj3.Previous5 = last5Item.Count() > 5 ? last5Item[5].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj3.Previous6 = last5Item.Count() > 6 ? last5Item[6].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj3.Previous7 = last5Item.Count() > 7 ? last5Item[7].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj3.Previous8 = last5Item.Count() > 8 ? last5Item[8].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj3.Previous9 = last5Item.Count() > 9 ? last5Item[9].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj3.Previous10 = last5Item.Count() > 10 ? last5Item[10].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj3.Previous11 = last5Item.Count() > 11 ? last5Item[11].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj3.Previous12 = last5Item.Count() > 12 ? last5Item[12].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj3.Previous13 = last5Item.Count() > 13 ? last5Item[13].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            obj3.Previous14 = last5Item.Count() > 14 ? last5Item[14].SAMPLE_DATE!.Value.ToString("dd-MMM-yy") : "";
            result.C_Datas.Add(obj3);


            foreach (var dt in rd2)
            {
                var list11 = query2.Where(d => d.VERWMERKM == dt.Code).OrderByDescending(e => e.Erstelldat).ToList();

                {


                    obj3 = new Graph_C_Data();
                    obj3.Description = dt.KURZTEXT;
                    obj3.Current = list11.Count() > 0 ? list11[0].Result.ToString() : "";
                    obj3.Previous1 = list11.Count() > 1 ? list11[1].Result.ToString("") : "";
                    obj3.Previous2 = list11.Count() > 2 ? list11[2].Result.ToString("") : "";
                    obj3.Previous3 = list11.Count() > 3 ? list11[3].Result.ToString() : "";
                    obj3.Previous4 = list11.Count() > 4 ? list11[4].Result.ToString() : "";
                    obj3.Previous5 = list11.Count() > 5 ? list11[4].Result.ToString() : "";
                    obj3.Previous6 = list11.Count() > 6 ? list11[4].Result.ToString() : "";
                    obj3.Previous7 = list11.Count() > 7 ? list11[4].Result.ToString() : "";
                    obj3.Previous8 = list11.Count() > 8 ? list11[4].Result.ToString() : "";
                    obj3.Previous9 = list11.Count() > 9 ? list11[4].Result.ToString() : "";
                    obj3.Previous10 = list11.Count() > 10 ? list11[4].Result.ToString() : "";
                    obj3.Previous11 = list11.Count() > 11? list11[4].Result.ToString() : "";
                    obj3.Previous12 = list11.Count() > 12 ? list11[4].Result.ToString() : "";
                    obj3.Previous13 = list11.Count() > 13 ? list11[4].Result.ToString() : "";
                    obj3.Previous14 = list11.Count() > 14 ? list11[4].Result.ToString() : "";
                   

                }

                result.C_Datas.Add(obj3);


            }


            var last5GpCData = from row in db.ResultRecord.Where(d => d.REQ_NO == hdt.INS_LOT && d.CHAR_GROUP == "C")

                               select new VMGraph_C_Data
                               {
                                   SAMPLE_NO = row.SAMPLE_NO,
                                   Erstelldat = row.Erstelldat,
                                   VERWMERKM = row.VERWMERKM,
                                   KURZTEXT = row.KURZTEXT,
                                   CHAR_PART3 = row.CHAR_PART3,
                                   Result = row.Result,
                                   Mresult = row.Mresult,
                                   DUMMY10 = row.DUMMY10,


                               };



            result.EqpNo = hdt.EQUIPMENT;
            result.Req_No = hdt.REQ_NO;
            result.CustAddress = hdt.CUST_ADDRESS;
            result.CustName = hdt.CustomerName;
            result.CustContact = hdt.Contact1_Name;
            result.OilName = hdt.OIL_NAME;
            result.Model = hdt.EQUIPMENT_MODEL;
            result.Serial = hdt.EQUIPMENT_SERIAL;
            result.Meker = hdt.EQUIPMENT_MAKE;
            result.Graph_A_Datas = last5GpAData.ToList();
            //result.Graph_B_Datas = last5GpBData.ToList();
            result.Graph_C_Datas = last5GpCData.ToList();





            return result;
        }

        #endregion
            
        public IEnumerable<DailySalesTTY> GetAllDailySalesTTY()
        {
            return db.DailySalesTTY;
        }

        public async Task AddDailySalesTTY(DailySalesTTY obj)
        {
            
            db.DailySalesTTY.Add(obj);
        }




        public IList<DailySalesTTY> GetDailySalesTTY()
        {

            List<DailySalesTTY> list = new List<DailySalesTTY>();
            SqlCommand command;
            string sql = null;
            //SqlDataReader dataReader;
            string connetionString = null;
            SqlConnection cnn;
            connetionString = @"Data Source=192.168.10.251;Initial Catalog=SalesData;User ID=sa;Password=RplImt0101234";
            cnn = new SqlConnection(connetionString);
            DateTime curtime = DateTime.Now;
            DateTime newtime = curtime.AddMinutes(-60);

            //sql = "Select * from punchlog where bsevtdt between '" + curtime.ToString("MM/dd/yyyy HH:mm:ss") + "' and '" + newtime.ToString("MM/dd/yyyy HH:mm:ss") + "'";
            sql = "SELECT TOP 100 * FROM DailySalesTTY  ORDER BY invdt DESC";
            //try
            //{
            cnn.Open();
            command = new SqlCommand(sql, cnn);
            command.ExecuteNonQuery();

            //dataReader = command.ExecuteReader();

            //cnn.Close();
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {


                    list.Add(new DailySalesTTY()
                    {
                        invdt = Convert.ToDateTime(reader["invdt"]),
                        WorkArea = (reader["WorkArea"]).ToString(),
                        PCd = (reader["PCd"]).ToString(),
                        TP = Convert.ToDecimal(reader["TP"]),
                        Vat =(reader["Vat"]).ToString() ,
                        Sales_Qty = Convert.ToInt32(reader["Sales_Qty"]),
                        Sales_Val = Convert.ToDecimal(reader["Sales_Val"]),
                        Retn_Qty =Convert.ToInt32 (reader["Retn_Qty"]),
                        ActualSales_Qty =Convert.ToInt32 (reader["ActualSales_Qty"]),
                        DeptCd = (reader["DeptCd"]).ToString(),
                        Period = Convert.ToInt32(reader["Period"]),
                        MsrCd = (reader["MsrCd"]).ToString(),
                        Supervisor = (reader["Supervisor"]).ToString(),
                        SSupervisor = (reader["SSupervisor"]).ToString(),
                        SSSupervisor = (reader["SSSupervisor"]).ToString(),
                        SSSSupervisor = (reader["SSSSupervisor"]).ToString(),
                        ProductID = (reader["ProductID"]).ToString(),
                        Company = (reader["Company"]).ToString(),
                        RegionCode = (reader["RegionCode"]).ToString(),
                        ZoneCode = (reader["ZoneCode"]).ToString(),
                        SMAreaCode = (reader["SMAreaCode"]).ToString(),
                        GMAreaCode = (reader["GMAreaCode"]).ToString(),
                    });
                }
            }

            return list;
        }

        public IList<VMOrderData> GetOrderList(DateTime Stdat )
        {

            List<VMOrderData> list = new List<VMOrderData>();
            SqlCommand command;
            string sql = null;
            //SqlDataReader dataReader;
            string connetionString = null;
            SqlConnection cnn;
            connetionString = @"Data Source=192.168.10.251;Initial Catalog=SalesData;User ID=sa;Password=RplImt0101234";
            cnn = new SqlConnection(connetionString);
            DateTime curtime = Stdat;
          

            //sql = "Select * from punchlog where bsevtdt between '" + curtime.ToString("MM/dd/yyyy HH:mm:ss") + "' and '" + newtime.ToString("MM/dd/yyyy HH:mm:ss") + "'";
            sql = "SELECT '300' AS MANDT,om.OrderID AS APP_REF_NO,od.MATNR AS MATERIAL,mt.SalesOrg AS SALESORG,mt.DisChannel AS DISTR_CHAN,mt.Division AS DIVISION,om.OrderType AS SO_TYPE,om.PARTNER AS CUSTOMER,od.OrderQty AS QUANTITY,\r\n'' AS SU,od.DelPlant AS PLANT,right(c.TransPZone,6) AS ROUTE,om.DelDate AS DELVY_DATE,om.WorkAreaT AS ZZTER,om.SaveDateTime AS CREATED_ON,'' AS SO_NUM,'' AS DEL_IND\r\nFROM RDB..OrdersMobile om\r\nINNER JOIN RDB..OrdersMobileDetails od ON om.OrderID=od.OrderID\r\nINNER JOIN RDB..Customer c ON om.PARTNER=c.PARTNER\r\nINNER JOIN RDB..Material mt ON od.MATNR=mt.MATNR AND c.SalesOrg=mt.SalesOrg\r\nWHERE om.DelDate='2023-12-10'\r\nand od.MATNR NOT IN ('15000000','15000001')\r\nUNION\r\nSELECT '300' AS MANDT,om.OrderID AS APP_REF_NO,od.MATNR AS MATERIAL,mt.SalesOrg AS SALESORG,mt.DisChannel AS DISTR_CHAN,mt.Division AS DIVISION,om.OrderType AS SO_TYPE,om.PARTNER AS CUSTOMER,od.OrderQty AS QUANTITY,\r\n'' AS SU,od.DelPlant AS PLANT,right(c.TransPZone,6) AS ROUTE,om.DelDate AS DELVY_DATE,om.WorkAreaT AS ZZTER,om.SaveDateTime AS CREATED_ON,'' AS SO_NUM,'' AS DEL_IND\r\nFROM RDB..OrdersMobile om\r\nINNER JOIN RDB..OrdersMobileDetails od ON om.OrderID=od.OrderID\r\nINNER JOIN RDB..Customer c ON om.PARTNER=c.PARTNER\r\nINNER JOIN RDB..CustMatInc cc ON om.PARTNER=cc.PARTNER and od.MATNR=cc.MATNR\r\nINNER JOIN RDB..Material mt ON od.MATNR=mt.MATNR AND c.SalesOrg=mt.SalesOrg\r\nWHERE om.DelDate='2023-12-10'\r\nand od.MATNR IN ('15000000','15000001')\r\n";
            //try
            //{
            cnn.Open();
            command = new SqlCommand(sql, cnn);
            command.ExecuteNonQuery();

            //dataReader = command.ExecuteReader();

            //cnn.Close();
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {


                    list.Add(new VMOrderData()
                    {
                        MANDT = Convert.ToInt32(reader["MANDT"]),
                        APP_REF_NO = Convert.ToInt64(reader["APP_REF_NO"]),
                        MATERIAL = (reader["MATERIAL"]).ToString(),
                        SalesOrg = (reader["SalesOrg"]).ToString(),
                        DISTR_CHAN = (reader["DISTR_CHAN"]).ToString(),
                        DIVISION = (reader["DIVISION"]).ToString(),
                        SO_TYPE = (reader["SO_TYPE"]).ToString(),
                        CUSTOMER = (reader["CUSTOMER"]).ToString(),
                        QUANTITY = Convert.ToDecimal(reader["QUANTITY"]),
                        SU = (reader["SU"]).ToString(),
                        PLANT = (reader["PLANT"]).ToString(),
                        ROUTE = (reader["ROUTE"]).ToString(),
                        DELVY_DATE = Convert.ToDateTime(reader["DELVY_DATE"]),
                        ZZTER = (reader["ZZTER"]).ToString(),
                        CREATED_ON = Convert.ToDateTime(reader["CREATED_ON"]),
                        SO_NUM = (reader["SO_NUM"]).ToString(),
                        DEL_IND = (reader["DEL_IND"]).ToString(),

                    });
                }
            }

            return list;
        }


    }
}
