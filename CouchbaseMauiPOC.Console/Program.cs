using System.Text;
using Couchbase.Lite.DI;
using CouchbaseMauiPOC.Console;
using CouchbaseMauiPOC.Infrastructure.Events;
using CouchbaseMauiPOC.Infrastructure.Extensions;
using CouchbaseMauiPOC.Infrastructure.Models;
using CouchbaseMauiPOC.Infrastructure.Repositories;
using CouchbaseMauiPOC.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

internal class Program
{
    private static async Task Main(string[] args)
    {
        if (args.Count() > 0)
        {
            var universitiesDirectoryInfo = new DirectoryInfo(args[0]);
            if (!universitiesDirectoryInfo.Exists)
            {
                throw new Exception($"Could not find the directory: {args[0]}");
            }
            // var universitiesDbPath = Path.Combine(args[0], "universities.cblite2");
            // if(!Database.Exists("universities", args[0]))
            // {
            //     throw new Exception($"Could not find the database in the path: {args[0]}");
            // }
            Service.Register<IDefaultDirectoryResolver>(new ConsoleDirectoryResolver(args[0]));
        }

        var services = new ServiceCollection();

        services.AddSingleton<IDatabaseSeedService, DatabaseSeedService>();

        services.AddDataServices();

        var serviceProvider = services.BuildServiceProvider();
        var userProfileRepository = serviceProvider.GetRequiredService<IUserProfileRepository>();
        userProfileRepository.UserProfileResultChanged += OnResultChanged;
        var universityRepository = serviceProvider.GetRequiredService<IUniversityRepository>();
        universityRepository.UniversityResultChanged += OnResultChanged;
        IBaseRepository[] baseRepositories = [userProfileRepository, universityRepository];

        int? repositorySelection = null;
        do
        {
            repositorySelection = SelectRepository(baseRepositories);
            if(repositorySelection.HasValue)
            {
                var selectedRepository = baseRepositories[repositorySelection.Value];

                // select by ID:
                string? idToSelect = null;
                do
                {
                    Console.WriteLine($"Enter ID to select from the {selectedRepository.GetType().Name} repository: ");
                    idToSelect = Console.ReadLine();
                }
                while (idToSelect == null || string.IsNullOrEmpty(idToSelect));
                await selectedRepository.GetAsync(idToSelect);

                var universityRepo = selectedRepository as UniversityRepository;
                if(universityRepo != null)
                {
                    // delete by ID:
                    string? idToDelete = null;
                    do
                    {
                        Console.WriteLine($"Enter ID to delete from the {selectedRepository.GetType().Name} repository: ");
                        idToDelete = Console.ReadLine();
                    }
                    while(idToDelete == null || string.IsNullOrEmpty(idToDelete));
                    var universityToDelete = new University { Id = idToDelete };
                    var success = await universityRepo.DeleteAsync(universityToDelete);
                    Console.WriteLine($"Deletion of {universityToDelete.Type} with ID {universityToDelete.Id} completed with success: {success}.");
                }
            }
        }
        while (repositorySelection.HasValue);

        Console.WriteLine("Program terminating...");
    }

    private static void OnResultChanged<TDataEntity>(QueryResultChangedEventArgs<TDataEntity> args)
        where TDataEntity : DataEntity
    {
        Console.WriteLine($"{nameof(Program)}.{nameof(OnResultChanged)}<{typeof(TDataEntity).Name}> >> Error:{args.Exception.ToDebugString()} DataEntity:{args.DataEntity.ToDebugString<TDataEntity>()}");
    }

    private static int? SelectRepository(params IBaseRepository[] baseRepositories)
    {
        // print repositories:
        StringBuilder sbOutput = new StringBuilder();
        sbOutput.AppendLine("Pick a repository:");
        for (int i = 0; i < baseRepositories.Count(); ++i)
        {
            sbOutput.AppendLine($"{i}) {baseRepositories[i].GetType().Name}");
        }
        Console.WriteLine(sbOutput);

        // collect response:
        int? result = null;
        do
        {
            Console.WriteLine("your choice?");
            var keyInfo = Console.ReadKey();
            if(ConsoleKey.Escape.Equals(keyInfo.Key)) break;
            int choice;
            if(Int32.TryParse(keyInfo.KeyChar.ToString(), out choice) && choice >=0 && choice < baseRepositories.Count())
            {
                result = choice;
                Console.WriteLine($" => {baseRepositories[result.Value].GetType().Name}");
            }
            else
            {
                Console.WriteLine($"  ERROR: {keyInfo.KeyChar} is not an option");
            }
        }
        while(!result.HasValue);

        return result;
    }
}