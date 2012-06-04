using System.Windows.Controls;
using ClientLogics;

namespace SLClient
{
    public partial class MainPage : UserControl
    {
        public MainPage()
        {
            InitializeComponent();
            this.DataContext = new ClientViewModel();
        }
    }
}
