using NeatSplit.ViewModels;
using NeatSplit.Services;
using Microsoft.Maui.Controls;

namespace NeatSplit.Views
{
    public partial class BalancesTab : ContentPage
    {
        private readonly BalancesTabViewModel _viewModel;
        private readonly int _groupId;

        public BalancesTab(int groupId)
        {
            InitializeComponent();
            _groupId = groupId;
            _viewModel = new BalancesTabViewModel(App.Current.Services.GetService<NeatSplitDatabase>(), _groupId);
            BindingContext = _viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.LoadBalances();
        }
    }
} 