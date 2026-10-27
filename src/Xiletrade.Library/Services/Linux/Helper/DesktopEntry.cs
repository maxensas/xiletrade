using System.Text;

namespace Xiletrade.Library.Services.Linux.Helper;

/// <summary>
/// Builds the freedesktop.org ".desktop" file used to register the custom URL protocol.
/// </summary>
internal static class DesktopEntry
{
    /// <summary>
    /// Escapes one argument of the Exec key (desktop entry specification, "The Exec key").
    /// </summary>
    internal static string EscapeExecArgument(string argument)
    {
        // Arguments with a reserved character must be double-quoted,
        // and ", `, $ and \ escaped with a backslash inside the quotes.
        var sb = new StringBuilder(argument.Length + 2);
        bool needsQuotes = argument.Length is 0;
        foreach (char c in argument)
        {
            switch (c)
            {
                case '"':
                case '`':
                case '$':
                    sb.Append('\\').Append(c);
                    needsQuotes = true;
                    break;
                case '\\':
                    // the string-value escape rule is applied first: a literal backslash needs four of them
                    sb.Append("\\\\\\\\");
                    needsQuotes = true;
                    break;
                case '%':
                    sb.Append("%%"); // literal percent sign
                    needsQuotes = true;
                    break;
                case ' ':
                case '\t':
                case '\n':
                case '\'':
                case '>':
                case '<':
                case '~':
                case '|':
                case '&':
                case ';':
                case '*':
                case '?':
                case '#':
                case '(':
                case ')':
                    sb.Append(c);
                    needsQuotes = true;
                    break;
                default:
                    sb.Append(c);
                    break;
            }
        }
        return needsQuotes ? "\"" + sb + "\"" : sb.ToString();
    }

    /// <summary>
    /// Content of the ".desktop" file registering <paramref name="scheme"/> to the application.
    /// </summary>
    /// <param name="name">Application name.</param>
    /// <param name="scheme">URL scheme, e.g. "xiletrade".</param>
    /// <param name="execArguments">Command and its leading arguments (the URL placeholder is appended).</param>
    internal static string Build(string name, string scheme, params string[] execArguments)
    {
        var exec = new StringBuilder();
        foreach (var argument in execArguments)
        {
            exec.Append(EscapeExecArgument(argument)).Append(' ');
        }
        exec.Append("%u");

        var sb = new StringBuilder();
        sb.Append("[Desktop Entry]\n");
        sb.Append("Type=Application\n");
        sb.Append("Name=").Append(name).Append('\n');
        sb.Append("Exec=").Append(exec).Append('\n');
        sb.Append("Terminal=false\n");
        sb.Append("NoDisplay=true\n");
        sb.Append("MimeType=x-scheme-handler/").Append(scheme).Append(";\n");
        return sb.ToString();
    }
}
