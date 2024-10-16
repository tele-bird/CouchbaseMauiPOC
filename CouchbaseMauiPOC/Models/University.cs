namespace CouchbaseMauiPOC.Models;

public class University : DataEntity
{
    public override string Type {get => "university";}
    public string? Name {get; set;}
    public string? Country {get; set;}
    public string? AlphaTwoCode {get; set;}
}
