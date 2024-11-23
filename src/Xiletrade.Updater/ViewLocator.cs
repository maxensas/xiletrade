using Avalonia.Controls;
using Avalonia.Controls.Templates;
using System;
using System.Diagnostics.CodeAnalysis;
using Xiletrade.Updater.ViewModels;

namespace Xiletrade.Updater
{
    public class ViewLocator : IDataTemplate
    {
        [UnconditionalSuppressMessage("Trimming", "IL2057:Unrecognized value passed to the parameter 'typeName' of method 'System.Type.GetType(String)'. It's not possible to guarantee the availability of the target type.", Justification = "<Pending>")]
        public Control? Build(object? param)
        {
            if (param is null)
                return null;

            var name = param.GetType().FullName!.Replace("ViewModel", "View", StringComparison.Ordinal);
            var type = Type.GetType(name);
            if (type != null)
            {
                return (Control)Activator.CreateInstance(type)!;
            }

            return new TextBlock { Text = "Not Found: " + name };
        }

        public bool Match(object? data)
        {
            return data is ViewModelBase;
        }
    }
}
