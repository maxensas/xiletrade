using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using Xiletrade.Library.Models.Ninja.Contract.Exchange.Detail;

namespace Xiletrade.UI.WPF.Util.Converters;

public class NinjaPairToOxyPlotConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not IEnumerable<NinjaPair> dataPair || !dataPair.Any())
            return null;

        var firstPair = dataPair.FirstOrDefault();
        var data = firstPair?.History;

        var plotmodel = new PlotModel
        {
            Title = firstPair.Id,
            TextColor = OxyColors.White,
            PlotAreaBorderColor = OxyColors.Transparent
        };
        
        // Date : X axis
        var startDate = data.Min(d => d.Timestamp.DateTime);
        var maxDate = data.Max(d => d.Timestamp.DateTime);
        var minDate = maxDate.AddDays(-7);

        double startX = DateTimeAxis.ToDouble(startDate);
        double minX = DateTimeAxis.ToDouble(minDate);
        double maxX = DateTimeAxis.ToDouble(maxDate);

        var dateAxis = new DateTimeAxis
        {
            TextColor = OxyColors.DarkGray,
            Position = AxisPosition.Bottom,
            StringFormat = "dd/MM",
            IntervalType = DateTimeIntervalType.Days,
            MinorIntervalType = DateTimeIntervalType.Days,
            MajorGridlineStyle = LineStyle.Dot,
            //MinorGridlineStyle = LineStyle.LongDash,
            MajorGridlineColor = OxyColors.DarkGray,
            //MinorGridlineColor = OxyColors.LightGray,
            AbsoluteMinimum = startX,
            AbsoluteMaximum = maxX,
            IsZoomEnabled = true,
            IsPanEnabled = true
        };
        dateAxis.Zoom(minX, maxX);
        plotmodel.Axes.Add(dateAxis);

        // Volume : Y1 Axis
        var valueAxis = new LinearAxis
        {
            Position = AxisPosition.Left,
            Key = "Y1",
            TextColor = OxyColors.DarkOrange,
            MajorGridlineStyle = LineStyle.None,
            MajorGridlineColor = OxyColors.DarkOrange,
            Title = "Volume / Hour",
            AbsoluteMinimum = 0,
            IsZoomEnabled = false,
            LabelFormatter = v =>
            {
                if (v >= 1_000_000) return (v / 1_000_000d).ToString("0.#") + "M";
                if (v >= 1_000) return (v / 1_000d).ToString("0.#") + "k";
                return v.ToString("0");
            }
        };
        plotmodel.Axes.Add(valueAxis);
        
        var series = new StemSeries
        {
            Title = firstPair.Id,
            Color = OxyColors.Orange,
            StrokeThickness = 30,
            TrackerFormatString = "{2:MMMM dd}\n\n{4:0.#} {0}",
            MarkerType = MarkerType.None,
            YAxisKey = "Y1"
        };
        
        foreach (var p in data.OrderBy(d => d.Timestamp))
        {
            series.Points.Add(DateTimeAxis.CreateDataPoint(p.Timestamp.DateTime, p.VolumePrimaryValue));
        }
        plotmodel.Series.Add(series);

        // Rate : Y2 Axis
        var linearAxis = new LinearAxis
        {
            TextColor = OxyColors.DodgerBlue,
            Key = "Y2",
            Position = AxisPosition.Right,
            MajorGridlineStyle = LineStyle.DashDotDot,
            //MinorGridlineStyle = LineStyle.LongDash,
            MajorGridlineColor = OxyColors.DodgerBlue,
            //MinorGridlineColor = OxyColors.LightGray,
            Title = "Rate",
            AbsoluteMinimum = 0,
            IsZoomEnabled = false,
            LabelFormatter = v =>
            {
                if (v >= 1_000_000) return (v / 1_000_000d).ToString("0.#") + "M";
                if (v >= 1_000) return (v / 1_000d).ToString("0.#") + "k";
                return v.ToString("0");
            }
        };

        plotmodel.Axes.Add(linearAxis);

        var seriess = new LineSeries
        {
            Title = firstPair.Id,
            Color = OxyColors.DodgerBlue,
            TrackerFormatString = "{2:MMMM dd}\n\n{4:F1} {0}",
            StrokeThickness = 2,
            MarkerType = MarkerType.None, 
            YAxisKey = "Y2"
        };

        foreach (var p in data.OrderBy(d => d.Timestamp))
        {
            seriess.Points.Add(new DataPoint(DateTimeAxis.ToDouble(p.Timestamp.DateTime), p.Rate));
        }

        plotmodel.Series.Add(seriess);
        plotmodel.InvalidatePlot(true);

        return plotmodel;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}