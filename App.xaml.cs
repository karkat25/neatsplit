namespace NeatSplit;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

	protected override async void OnStart()
	{
		try
		{
			await AppData.InitializeAsync();
		}
		catch
		{
			// Handle initialization error
		}
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AppShell());
	}
}