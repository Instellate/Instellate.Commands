using System.Text.RegularExpressions;

namespace Instellate.Commands.ArgumentParser;

internal static partial class Regexes
{
    [GeneratedRegex(@"^--([\w-]+)")]
    public static partial Regex Argument();

    [GeneratedRegex(@"^-([\w-]+)")]
    public static partial Regex ShortArgument();

    [GeneratedRegex("""^("(?:[^\\"]|\\["])*"|[^ ]*)""")]
    public static partial Regex Value();

    [GeneratedRegex(@"^([a-zA-Z0-9_][\w-]*)")]
    public static partial Regex Command();
}
