using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.Input;

namespace Xiletrade.Avalonia.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public IRelayCommand Close { get; }

    public static string Greeting => "Welcome to the new Xiletrade !";

    public MainViewModel()
    {
        Close = new RelayCommand(CloseApplication);
    }

    public static void CloseApplication()
    {
        if(App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime) lifetime.Shutdown();
    }
}
