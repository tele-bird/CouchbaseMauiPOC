using CommunityToolkit.Maui;
using CouchbaseMauiPOC.Extensions;
using CouchbaseMauiPOC.Infrastructure.Repositories;
using CouchbaseMauiPOC.Infrastructure.Services;
using CouchbaseMauiPOC.Services;
using Microsoft.Extensions.Logging;

namespace CouchbaseMauiPOC;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMauiCommunityToolkit()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		builder.Services.AddSingleton<IAlertService, AlertService>();
		builder.Services.AddSingleton<IMediaService, MediaService>();
		builder.Services.AddSingleton<IDatabaseSeedService, DatabaseSeedService>();
		builder.Services.AddSingleton<IUserProfileRepository, UserProfileRepository>();
		builder.Services.AddSingleton<IUniversityRepository, UniversityRepository>();
		builder.Services.AddNavigation();

#if ANDROID
		Couchbase.Lite.Support.Droid.Activate(Platform.AppContext);
#endif

		return builder.Build();
	}
}
