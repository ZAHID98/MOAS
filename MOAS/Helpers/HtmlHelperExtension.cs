using Microsoft.AspNetCore.Html;
using System.Collections.Generic;
using System.Dynamic;

namespace MOAS
{
    public static class HtmlHelperExtension
    {
        private const string CheckedAttribute = " checked='checked'";

        public static HtmlString CheckedIfMatch(object expected, object actual)
        {
            
            return new HtmlString(Equals(expected, actual) ? CheckedAttribute : string.Empty);
        }
    }

    //public static class Extensions
    //{
    //    public static ExpandoObject ToExpando(this object anonymousObject)
    //    {
    //        IDictionary<string, object> anonymousDictionary = HtmlHelper.AnonymousObjectToHtmlAttributes(anonymousObject);
    //        IDictionary<string, object> expando = new ExpandoObject();
    //        foreach (var item in anonymousDictionary)
    //            expando.Add(item);
    //        return (ExpandoObject)expando;
    //    }
    //}



    
}