using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text;


namespace MOAS
{
    public class CommonMethod
    {
        public static bool MatchChar(string input, string tomatch)
        {

            Match m = Regex.Match(input, tomatch,RegexOptions.IgnoreCase);

            return m.Success;
        }
        public static string numToStr(int Num, int StrDigit)
        {
            int SerialLength = Num.ToString().Length;
            int RemainingLength = StrDigit - SerialLength;
            string ZeroPrfix = "";
            for (int i = 0; i < RemainingLength; i++)
            {
                ZeroPrfix = ZeroPrfix + "0";
            }

            return ZeroPrfix + Num.ToString();
        }
        public static string GetMD5(string value)
        {
            MD5 algorithm = MD5.Create();
            byte[] data = algorithm.ComputeHash(Encoding.UTF8.GetBytes(value));
            string sh1 = "";
            for (int i = 0; i < data.Length; i++)
            {
                sh1 += data[i].ToString("x2").ToUpperInvariant();
            }
            return sh1;
        }

        public static String GetMonthName(int MonthNumber=0)
        {
           
            if (MonthNumber == 0)
                return "";
            System.Globalization.DateTimeFormatInfo mfi = new System.Globalization.DateTimeFormatInfo();
            return mfi.GetMonthName(MonthNumber);
        }

       

    }

    //public class NoCache : ActionFilterAttribute
    //{
    //    public override void OnResultExecuting(ResultExecutingContext filterContext)
    //    {
    //        filterContext.HttpContext.Response.Cache.SetExpires(DateTime.UtcNow.AddDays(-1));
    //        filterContext.HttpContext.Response.Cache.SetValidUntilExpires(false);
    //        filterContext.HttpContext.Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
    //        filterContext.HttpContext.Response.Cache.SetCacheability(HttpCacheability.NoCache);
    //        filterContext.HttpContext.Response.Cache.SetNoStore();

    //        base.OnResultExecuting(filterContext);
    //    }
    //}
}