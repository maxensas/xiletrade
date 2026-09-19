using System.Windows.Controls;
using System.Windows.Interop;

namespace Xiletrade.UI.WPF.UserControls.Main
{
    /// <summary>
    /// Logique d'interaction pour TaskbarIcon.xaml
    /// </summary>
    public partial class TaskbarIcon : UserControl
    {
        public TaskbarIcon(object vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}
