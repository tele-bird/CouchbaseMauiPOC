using System;
using System.Text;

namespace CouchbaseMauiPOC.Extensions;

public static class ListExtensions
{
    public static string ToDebugString(this List<object> list)
    {
        StringBuilder sbDebugString = new StringBuilder();
        foreach(var obj in list)
        {
            if(sbDebugString.Length > 0)
            {
                sbDebugString.Append(", ");
            }
            if(obj == null)
            {
                sbDebugString.Append("(null)");
            }
            else
            {
                sbDebugString.Append(obj.ToString());
            }
        }
        return $"[{sbDebugString}]";
    }

}
