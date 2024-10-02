using CommunityToolkit.Maui;
using CouchbaseMauiPOC.Repositories;
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
		builder.Services.AddSingleton<IUserProfileRepository, UserProfileRepository>();

#if ANDROID
		Couchbase.Lite.Support.Android.Activate(Platform.AppContext);
#endif

		return builder.Build();
	}
}
