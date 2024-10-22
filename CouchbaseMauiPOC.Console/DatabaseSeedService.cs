using System;
using System.Diagnostics;
using CouchbaseMauiPOC.Infrastructure.Services;

namespace CouchbaseMauiPOC.Console;

public class DatabaseSeedService : IDatabaseSeedService
{
    public async Task CopyDatabaseAsync(string targetDirectoryPath)
    {
        // target location:
        var targetFullPath = Path.Combine(targetDirectoryPath, "universities.cblite2");
        var targetFullPathDirectoryInfo = Directory.CreateDirectory(targetFullPath);
        Trace.WriteLine($"Writing universities to: {targetFullPathDirectoryInfo.FullName}");

        // source location:
        var sourcePath = Path.Combine(Environment.CurrentDirectory, "universities.cblite2");
        var sourceFullPathDirectoryInfo = new DirectoryInfo(sourcePath);
        if(!sourceFullPathDirectoryInfo.Exists)
        {
            throw new Exception($"Could not find the universities cblite2 folder in the source location: {sourceFullPathDirectoryInfo.FullName}");
        }
        Trace.WriteLine($"Getting universities from: {sourceFullPathDirectoryInfo.FullName}");

        foreach (var file in sourceFullPathDirectoryInfo.EnumerateFiles())
        {
            using (var inStream = File.OpenRead(file.FullName))
            {
                using (var outStream = File.OpenWrite(Path.Combine(targetFullPath, file.Name)))
                {
                    await inStream.CopyToAsync(outStream);
                }
            }
        }
    }
}
