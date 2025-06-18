using neatsplit.ViewModels;
using NeatSplit.Services;
using Microsoft.Maui.Controls;

namespace neatsplit.Views
{
    public partial class BalancesTab : ContentPage
    {
        private BalancesTabViewModel _viewModel;
        private int _groupId;

        public BalancesTab()
        {
            InitializeComponent();
        }

        protected override async void OnParentSet()
        {
            base.OnParentSet();
            if (Parent is GroupDetailPage groupDetailPage)
            {
                _groupId = groupDetailPage.GroupId;
                _viewModel = new BalancesTabViewModel(App.Current.Services.GetService<NeatSplitDatabase>(), _groupId);
                BindingContext = _viewModel;
                await _viewModel.LoadBalancesAsync();
            }
        }
    }
} 