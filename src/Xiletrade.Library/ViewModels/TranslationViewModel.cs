using System.ComponentModel;
using System.Globalization;

namespace Xiletrade.Library.ViewModels;

public sealed class TranslationViewModel : INotifyPropertyChanged
{
    private static readonly TranslationViewModel instance = new();
    private readonly System.Resources.ResourceManager resManager = Resources.Resources.ResourceManager;
    private CultureInfo currentCulture = null;
    public event PropertyChangedEventHandler PropertyChanged;

    public static TranslationViewModel Instance
    {
        get { return instance; }
    }

    public string this[string key]
    {
        get { return this.resManager.GetString(key, this.currentCulture); }
    }

    public CultureInfo CurrentCulture
    {
        get { return this.currentCulture; }
        set
        {
            if (this.currentCulture != value)
            {
                this.currentCulture = value;
                var @event = this.PropertyChanged;
                @event?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
            }
        }
    }
}
