using NeatSplit.Views;

namespace NeatSplit;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		
		// Register navigation routes
		Routing.RegisterRoute("GroupMembersPage", typeof(GroupMembersPage));
		Routing.RegisterRoute("GroupExpensesPage", typeof(GroupExpensesPage));
		Routing.RegisterRoute("GroupBalancesPage", typeof(GroupBalancesPage));
		Routing.RegisterRoute("ParticipantSelectionPage", typeof(ParticipantSelectionPage));
	}
}
