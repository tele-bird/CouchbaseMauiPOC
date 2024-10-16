using System;
using System.Text;

namespace CouchbaseMauiPOC.Extensions;

public static class DictionaryExtensions
{
    public static string ToDebugString(this Dictionary<string, object?> dictionary)
    {
        StringBuilder sbDebugString = new StringBuilder();
        object? keyValue = null;
        List<object>? keyValueList = null;
        foreach(var key in dictionary.Keys)
        {
            if(sbDebugString.Length > 0)
            {
                sbDebugString.Append(", ");
            }
            keyValue = dictionary[key];
            if(keyValue == null)
            {
                sbDebugString.Append($"{key}: (null)");
            }
            else
            {
                keyValueList = keyValue as List<object>;
                if(keyValueList == null)
                {
                    sbDebugString.Append($"{key}: {keyValue}");
                }
                else
                {
                    sbDebugString.Append($"{key}: {keyValueList.ToDebugString()}");
                }
            }
        }
        return $"[{sbDebugString}]";
    }
}
