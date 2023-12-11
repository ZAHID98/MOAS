namespace MOAS.Interfaces
{
    using MOAS.Models;
    using System;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using NuGet.Common;
 
    using MOAS.Models.VM;

    public interface ISetupRepository
    {
        #region User
        Task UserAdd(User obj);
        Task<bool> ChangePassword(int UserID, string oldPassword, string newPassword);
        Task<bool> CheckRoleExistant(int UserID, string RoleName);
        Task ActiveDeactive(int UserID);
        Task<User> UserGet(int ID);
        Task<Role> RoleGet(string RoleName);
        User UserGetByName(string username);
        IEnumerable<User> UserGetAll();
        IEnumerable<Role> RoleGetAll();
        SelectList UserGetSelectList();
        Task UserUpdate(User Obj);
        User SetPassword(User emp, string p);
        Task<bool> ValidateUser(string UserName, string Password);
        #endregion

        Task<IEnumerable<DailySales>> SyncSales();
        Task DSAdd(DailySales DS);
        IEnumerable<DailySales> GetAllSales();

        Task<IEnumerable<UOA_Header>> SynGetUOA_DATA();
        Task AddUOAMaster(UOA_Header Md);
        List<UOA_Header> GetAllUOAH();

        Task<IEnumerable<UOA_Item>> SynGetUOA_Item_DATA();

        IEnumerable<UOA_Item> GetAllPendingUOA();
        Task AddUOAItem(UOA_Item MI);
        Task updateItem(UOA_Item item);

        Task<IEnumerable<ResultRecord>> SynGetResult_DATA();

        Task<IEnumerable<CustomerBalance>> SynGetCustBAl_DATA(User UI);
        IEnumerable<CustomerBalance> GetAllCustBAL();


        IEnumerable<ResultRecord> GetAllRecord();
        Task AddResult(ResultRecord result);
        Task updateResult(ResultRecord record);

        IEnumerable<ResultRecord> GetResult(long ID);

        IEnumerable<UOA_Header> GetUOAMaster(long ID);
        IEnumerable<UOA_Header>GetHeaderByCustomer(long CID, string EQp);
        IEnumerable<VMENGINSL> GetUOA_HByCustomer(long CID);

        SelectList ELGetSelectList(long CID);

        IEnumerable<UOA_Item> GetUOAItem(long ID);

        VMResultData GetVMResultData(long ID);

        Equipment GETEQP(int ID);
        SelectList GetCustomerSelectlist();


        VMResultData GetVMResultByDateRang( DateTime Start,DateTime End, long ID ,int EID);


        Task CustBALLAdd(CustomerBalance BAL);
        //Task updateResult(ResultRecord record);



        IList<DailySalesTTY> GetDailySalesTTY();
      IList<VMOrderData> GetOrderList(DateTime Stdat);
        IEnumerable<DailySalesTTY> GetAllDailySalesTTY();
        Task AddDailySalesTTY(DailySalesTTY obj);

        Task Save();
    }
}

