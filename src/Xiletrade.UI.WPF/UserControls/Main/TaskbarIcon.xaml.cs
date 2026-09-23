using System.Windows.Controls;
using Xiletrade.Library.Services.Interface.View;

namespace Xiletrade.UI.WPF.UserControls.Main
{
    /// <summary>
    /// Logique d'interaction pour TaskbarIcon.xaml
    /// </summary>
    public partial class TaskbarIcon : UserControl, ITaskbar
    {
        public TaskbarIcon(object vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}
