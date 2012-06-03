using System.Windows.Controls;
using ClientLogics;

namespace SLClient
{
    public partial class MainPage : UserControl
    {
        ClientViewModel vm;
 
        public MainPage()
        {
            InitializeComponent();
            this.DataContext = vm = new ClientViewModel();
        }
    }
}
