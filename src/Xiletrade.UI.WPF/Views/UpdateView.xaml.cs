using System.Windows.Input;

namespace Xiletrade.UI.WPF.Views
{
    /// <summary>
    /// Logique d'interaction pour UpdateView.xaml
    /// </summary>
    public partial class UpdateView : ViewBase
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
