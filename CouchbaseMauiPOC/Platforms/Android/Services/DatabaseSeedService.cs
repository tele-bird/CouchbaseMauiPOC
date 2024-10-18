using System.IO.Compression;
using Android.Content;
using CouchbaseMauiPOC.Infrastructure.Services;

namespace CouchbaseMauiPOC.Services;

public partial class DatabaseSeedService : IDatabaseSeedService
{
    private readonly Context context;

    public DatabaseSeedService()
    {
        this.context = Platform.AppContext;
    }

    public partial async Task CopyDatabaseAsync(string targetDirectoryPath)
    {
        Directory.CreateDirectory(targetDirectoryPath);
        var assetStream = context.Assets!.Open("universities.zip");
        using (var archive = new ZipArchive(assetStream, ZipArchiveMode.Read))
        {
            foreach (var entry in archive.Entries)
            {
                var entryPath = Path.Combine(targetDirectoryPath, entry.FullName);
                if (entryPath.EndsWith("/"))
                {
                    Directory.CreateDirectory(entryPath);
                }
                else
                {
                    using (var entryStream = entry.Open())
                    {
                        using (var writeStream = File.OpenWrite(entryPath))
                        {
                            await entryStream.CopyToAsync(writeStream).ConfigureAwait(false);
                        }
                    }
                }
            }
        }
    }
}
