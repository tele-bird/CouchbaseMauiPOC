using System;

namespace CouchbaseMauiPOC.Models;

public class UserProfile : DataEntity
{
    public override string Type {get => "user";}
    public string? Name {get; set;}
    public string? Email {get; set;}
    public string? Address {get; set;}
    public byte[]? ImageData {get; set;}
    public string? Description {get; set;}
    public string? University {get; set;}
}
