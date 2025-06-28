namespace NeatSplit;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

	protected override async void OnStart()
	{
		base.OnStart();
		try
		{
			// Show database path for debugging (console only)
			var dbPath = Path.Combine(FileSystem.AppDataDirectory, "neatsplit.db3");
			System.Diagnostics.Debug.WriteLine($"Database path: {dbPath}");
			
			// Initialize database after app is fully started
			await AppData.InitializeAsync();
		}
		catch (Exception ex)
		{
			// Log the error but don't crash the app
			System.Diagnostics.Debug.WriteLine($"Database initialization error: {ex.Message}");
		}
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AppShell());
	}
}