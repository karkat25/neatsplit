using Microsoft.Extensions.Logging;
using neatsplit.Services;
using System.IO;
using NeatSplit.Services;

namespace neatsplit;

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

		// Register NeatSplitDatabase as a singleton
		builder.Services.AddSingleton<NeatSplitDatabase>();

		return builder.Build();
	}
}
