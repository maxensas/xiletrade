using System.Windows.Input;
using Xiletrade.Library.Services.Interface.View;

namespace Xiletrade.UI.WPF.Views
{
    /// <summary>
    /// Logique d'interaction pour UpdateView.xaml
    /// </summary>
    public partial class UpdateView : ViewBase, IUpdateView
    {
        public UpdateView()
        {
            InitializeComponent();
            MouseLeftButtonDown += Window_DragWindow;
        }

        private void Window_DragWindow(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
