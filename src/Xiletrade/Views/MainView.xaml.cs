using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Linq;
using Xiletrade.Library.ViewModels;
using Xiletrade.Library.Models;
using System.Collections.Generic;
using System.Windows.Media;
using Xiletrade.Util.Helper;
using Xiletrade.Library.Shared.Interop;
using Microsoft.Extensions.DependencyInjection;
using Xiletrade.Library.Services;

namespace Xiletrade.Views;

/// <summary>
/// Xiletrade Form : MainWindow.xaml
/// </summary>
public partial class MainView : ViewBase
{
    //internal static readonly DependencyProperty CommandParameterProperty = ButtonBase.CommandParameterProperty.AddOwner(typeof(CommandSlider));

    public MainView(object vm)
    {
        InitializeComponent();
        DataContext = vm;
        Application.Current.MainWindow = this;
        Closing += Window_Closing;
        MouseLeftButtonDown += Window_DragMove;
        listMods.ItemContainerGenerator.StatusChanged += ItemContainerGenerator_StatusChanged;
    }

    // events
    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        e.Cancel = true;
        Keyboard.ClearFocus();
        this.Visibility = Visibility.Hidden;
        GC.Collect();
    }

    private void Window_DragMove(object sender, MouseButtonEventArgs e)
    {
        var mainHwnd = App.ServiceProvider.GetRequiredService<XiletradeService>().MainHwnd;
        if (Native.GetForegroundWindow().Equals(mainHwnd))
        {
            try
            {
                this.DragMove();
            }
            catch (Exception)
            {
                //Debug.Trace("Exception with Window.DragMove : " + ex.Message);
            }
        }
    }
    /// <summary>
    /// Bind mouse gestures for some ListBoxItem contained in ListBox 'listMods'
    /// </summary>
    /// <remarks>
    /// ListBoxItem cannot be binded directly in XAML.
    /// </remarks>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ItemContainerGenerator_StatusChanged(object sender, EventArgs e)
    {
        if (listMods.ItemContainerGenerator.Status is not GeneratorStatus.ContainersGenerated)
        {
            return;
        }
        foreach (TextBox txtChild in FindVisualChildren<TextBox>(listMods).Where(x => x.Name is "min" or "max"))
        {
            txtChild.InputBindings.Clear();
            foreach (MouseGestureCom gesture in (DataContext as MainViewModel).GestureList)
            {
                var bind = new InputBinding(gesture.Command, new MouseWheelGesture(gesture.Modifier, gesture.Direction))
                {
                    CommandParameter = txtChild
                };
                txtChild.InputBindings.Add(bind);
            }
        }
        var lbItems = FindVisualChildren<ListBoxItem>(listMods);
        foreach (var item in lbItems)
        {
            var mod = FindVisualChildren<TextBox>(item).Where(x => x.Name is "mod").FirstOrDefault();
            var modbis = FindVisualChildren<TextBox>(item).Where(x => x.Name is "modbis").FirstOrDefault();
            var select = FindVisualChildren<CheckBox>(item).Where(x => x.Name is "select").FirstOrDefault();

            var bind = new InputBinding((DataContext as MainViewModel).Commands.SelectModCommand, new MouseGesture(MouseAction.LeftDoubleClick))
            {
                CommandParameter = select
            };
            item.InputBindings.Clear();
            item.InputBindings.Add(bind);
            mod.InputBindings.Clear();
            mod.InputBindings.Add(bind);
            modbis.InputBindings.Clear();
            modbis.InputBindings.Add(bind);
        }
    }

    //methods
    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
    {
        if (depObj != null)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                if (child != null && child is T)
                {
                    yield return (T)child;
                }

                foreach (T childOfChild in FindVisualChildren<T>(child))
                {
                    yield return childOfChild;
                }
            }
        }
    }
}
