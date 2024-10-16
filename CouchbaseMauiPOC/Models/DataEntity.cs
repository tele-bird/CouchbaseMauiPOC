using System;

namespace CouchbaseMauiPOC.Models;

public abstract class DataEntity
{
    public abstract string Type {get;}
    public string? Id {get; set;}
}
