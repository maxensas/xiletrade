using System.IO;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Application.Configuration.DTO.Extension;

internal static class ParserDataExtensions
{
    /// <summary>
    /// Checks the parsing rules once loaded : a mistake in the file (unknown replace, empty old or new) stops the loading
    /// with its location, instead of a rule silently never applied.
    /// </summary>
    /// <remarks>Format of the files : Data/ParsingRules.md.</remarks>
    /// <exception cref="InvalidDataException"></exception>
    internal static void Validate(this ParserData parser, string fileName)
    {
        if (parser.Mods is null)
        {
            throw new InvalidDataException($"{fileName} : 'mods' is missing.");
        }
        for (int i = 0; i < parser.Mods.Length; i++)
        {
            var mod = parser.Mods[i];
            if (mod is null)
            {
                throw new InvalidDataException($"{fileName}, mods[{i}] : empty rule.");
            }
            if (mod.Replace is not Strings.Word.contains and not Strings.Word.equals)
            {
                throw new InvalidDataException($"{fileName}, mods[{i}] (id {mod.Id}) : unknown replace '{mod.Replace}' "
                    + $"(expected '{Strings.Word.contains}' or '{Strings.Word.equals}').");
            }
            if (string.IsNullOrEmpty(mod.Old))
            {
                throw new InvalidDataException($"{fileName}, mods[{i}] (id {mod.Id}) : 'old' is empty.");
            }
            if (string.IsNullOrEmpty(mod.New))
            {
                throw new InvalidDataException($"{fileName}, mods[{i}] (id {mod.Id}) : 'new' is empty.");
            }
        }
    }
}
