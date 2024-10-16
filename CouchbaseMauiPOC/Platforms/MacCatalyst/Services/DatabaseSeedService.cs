using Foundation;

namespace CouchbaseMauiPOC.Services;

public partial class DatabaseSeedService : IDatabaseSeedService
{
    public partial async Task CopyDatabaseAsync(string targetDirectoryPath)
    {
        var finalPath = Path.Combine(targetDirectoryPath, "universities.cblite2");
        Directory.CreateDirectory(finalPath);
        var sourcePath = Path.Combine(NSBundle.MainBundle.ResourcePath, "Platforms/iOS/universities.cblite2");
        var dirInfo = new DirectoryInfo(sourcePath);

        foreach (var file in dirInfo.EnumerateFiles())
        {
            using (var inStream = File.OpenRead(file.FullName))
            {
                using (var outStream = File.OpenWrite(Path.Combine(finalPath, file.Name)))
                {
                    await inStream.CopyToAsync(outStream);
                }
            }
        }
     }
}
