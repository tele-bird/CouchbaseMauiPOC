namespace CouchbaseMauiPOC.Infrastructure.Extensions;

public static class ExceptionExtensions
{
    public static string ToDebugString(this Exception? exception)
    {
        return exception != null ? $"{exception.GetType().FullName}: {exception.Message}" : "null";
    }
}
