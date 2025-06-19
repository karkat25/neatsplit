using Microsoft.Maui;
using Microsoft.Maui.Hosting;

namespace NeatSplit;

public static class Main
{
	public static void Main(string[] args)
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		var app = builder.Build();
		app.Run(args);
	}
}
