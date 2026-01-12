using System.Text.RegularExpressions;

namespace Instellate.Commands.ArgumentParser;

internal static partial class Regexes
{
    [GeneratedRegex(@"^--([\w-]+)", RegexOptions.Compiled)]
    public static partial Regex Argument();

    [GeneratedRegex(@"^-([\w-]+)", RegexOptions.Compiled)]
    public static partial Regex ShortArgument();

    [GeneratedRegex("""^("(?:[^\\"]|\\["])*"|[^ ]*)""", RegexOptions.Compiled)]
    public static partial Regex Value();

    [GeneratedRegex(@"^([a-zA-Z0-9_][\w-]*)", RegexOptions.Compiled)]
    public static partial Regex Command();
}
