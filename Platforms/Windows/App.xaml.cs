﻿using Microsoft.UI.Xaml;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Hosting;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace neatsplit.WinUI;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : MauiWinUIApplication
{
	/// <summary>
	/// Initializes the singleton application object.  This is the first line of authored code
	/// executed, and as such is the logical equivalent of main() or WinMain().
	/// </summary>
	public App()
	{
		InitializeComponent();
	}

	protected override MauiApp CreateMauiApp() => NeatSplit.MauiProgram.CreateMauiApp();
}

