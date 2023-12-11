namespace MOAS.Models
{
    public enum EUserType
    {
        Executive = 1,Manager,Head
    }

    public enum EStatus
    {
        pending = 1, Approved, Reject
    }


    public enum EResultSt
    {
        Normal = 1, Caution, Alert
    }

    public enum EReportTypes
    {
        PDF, Excel, Word, Image
    }
}
