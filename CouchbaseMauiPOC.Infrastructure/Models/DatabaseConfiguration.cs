using System;

namespace CouchbaseMauiPOC.Infrastructure.Models;

public class DatabaseConfiguration
{
    public string? DatabaseName {get; set;}
    public string? IndexName {get; set;}
    public string[]? Indexes {get; set;}
}
