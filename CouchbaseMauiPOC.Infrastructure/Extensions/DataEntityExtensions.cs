using System.Text.Json;
using CouchbaseMauiPOC.Infrastructure.Models;

namespace CouchbaseMauiPOC.Infrastructure.Extensions;

public static class DataEntityExtensions
{
    public static string ToDebugString<TDataEntity>(this DataEntity? dataEntity)
        where TDataEntity : DataEntity
    {
        return dataEntity != null ? JsonSerializer.Serialize((TDataEntity)dataEntity) : "null";
    }
}
