using Microsoft.Maui.Controls;
using System.Threading.Tasks;

namespace neatsplit.Views
{
    [QueryProperty(nameof(GroupId), "GroupId")]
    public partial class GroupDetailPage : TabbedPage
    {
        private int _groupId;
        public int GroupId
        {
            get => _groupId;
            set
            {
                _groupId = value;
                // TODO: Load group details as needed
            }
        }

        public GroupDetailPage()
        {
            InitializeComponent();
        }
    }
} 