using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Xiletrade.Library.Models.Application.Hotkey.Converter;

/// <summary>
/// Provides conversion between key values and their string representations.
/// </summary>
public interface IKeysConverter
{
    /// <summary>
    /// Converts the specified value to a string representation.
    /// </summary>
    string ConvertToString(object value);

    /// <summary>
    /// Converts the specified value to a string representation.
    /// </summary>
    string ConvertToString(ITypeDescriptorContext context, CultureInfo culture, object value);

    /// <summary>
    /// Converts the specified value to a culture-invariant string representation.
    /// </summary>
    string ConvertToInvariantString(object value);

    /// <summary>
    /// Converts the specified text into an object.
    /// </summary>
    object ConvertFromString(string text);

    /// <summary>
    /// Converts the specified text into an object.
    /// </summary>
    object ConvertFromString(ITypeDescriptorContext context, CultureInfo culture, string text);

    /// <summary>
    /// Converts the given string to the converter's native type using the invariant culture.
    /// </summary>
    object ConvertFromInvariantString(string text);

    /// <summary>
    /// Gets a value indicating whether this converter can convert an object to the given
    /// destination type using the context.
    /// </summary>
    bool CanConvertTo([NotNullWhen(true)] Type destinationType);
}
