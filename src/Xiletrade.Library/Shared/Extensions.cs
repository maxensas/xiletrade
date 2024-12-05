using System.Globalization;
using System;

namespace Xiletrade.Library.Shared;

public static class Extensions
{
    public static double ToDoubleEmptyField(this string str)
    {
        return StrToDouble(str, true);
    }

    public static double ToDoubleDefault(this string str)
    {
        return StrToDouble(str, false);
    }

    private static double StrToDouble(string str, bool useEmptyfield = false)
    {
        double value = useEmptyfield ? Modifier.EMPTYFIELD : 0;
        if (str?.Length > 0)
        {
            try
            {
                value = double.Parse(str, CultureInfo.InvariantCulture); // correction
            }
            catch (Exception)
            {
                //Helper.Debug.Trace("Exception using double parsing : " + ex.Message);
            }
        }
        return value;
    }
}
