using System;
using CouchbaseMauiPOC.Models;
using Org.Apache.Http.Authentication;

namespace CouchbaseMauiPOC;

public static class AppInstance
{
    public static UserCredentials? User {get; set;}
}
