namespace MOAS.Models.VM
{
    public class VMResultData
    {

        public string CustName { get; set; }
        public string CustAddress { get; set;}
        public string CustContact { get; set; }
        public string OilName { get; set; } 
        public string EqpNo { get; set; }   
        public string Meker { get;set; }
        public string Model { get; set;}
        public string Serial { get; set; }  
        public long Req_No { get; set; }
        public string Res_Remaks { get; set;}

        public List<VMItemData>  ItemDatas { get; set; }
        public List<Graph_A_Data>A_Datas { get; set; }
        public List<Graph_B_Data> B_Datas { get; set; }
        public List<Graph_C_Data> C_Datas { get; set; }
        public List<VMGraph_A_Data>Graph_A_Datas { get; set; }
        public List<VMGraph_B_Data> Graph_B_Datas { get; set; }
        public List<VMGraph_C_Data> Graph_C_Datas { get; set; }


    }
}
