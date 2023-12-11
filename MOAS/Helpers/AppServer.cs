namespace MOAS
{
    public static class AppServer
    {
        public static string MapPath(string path)
        {
            return Path.Combine(
                (string)AppDomain.CurrentDomain.GetData("ContentRootPath"),
                path);
        }
    }
}
