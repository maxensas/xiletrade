namespace Xiletrade.Library.ViewModels.Command;

public class CompositeCommandParameter
{
    public CompositeCommandParameter(object eventData, object parameter)
    {
        EventData = eventData;
        Parameter = parameter;
    }

    public object EventData { get; }

    public object Parameter { get; }
}