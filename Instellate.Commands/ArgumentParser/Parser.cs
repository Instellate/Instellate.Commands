using System.Text.RegularExpressions;

namespace Instellate.Commands.ArgumentParser;

// TODO: Add better support for subcommands

public static class Parser
{
    private static readonly (Token, Regex)[] _tokenRegexes =
    [
        (Token.Argument, Regexes.Argument()),
        (Token.Argument, Regexes.ShortArgument()),
        (Token.Value, Regexes.Value())
    ];

    private enum Token
    {
        Argument,
        Value,
    }

    public record Result(
        List<string> Commands,
        Dictionary<string, string?> Options,
        List<string> PositionalArguments);

    public static Result Parse(string input)
    {
        Dictionary<string, string?> options = new();
        List<string> arguments = [];

        List<string> commands = [];
        Regex commandRegex = Regexes.Command();
        Match commandMatch = commandRegex.Match(input);
        if (!commandMatch.Success)
        {
            throw new NotImplementedException("Proper error message not implemented");
        }

        do
        {
            commands.Add(commandMatch.Groups[1].Value);
            input = input[(commandMatch.Index + commandMatch.Length)..].Trim();
        } while (commandMatch.Success);


        string? lastArgument = null;
        while (!string.IsNullOrWhiteSpace(input))
        {
            foreach ((Token token, Regex regex) in _tokenRegexes)
            {
                Match match = regex.Match(input);
                if (match.Success)
                {
                    switch (token)
                    {
                        case Token.Argument:
                            if (lastArgument is not null)
                            {
                                options.Add(lastArgument, null);
                            }

                            lastArgument = match.Value;
                            break;

                        case Token.Value:
                            if (lastArgument is not null)
                            {
                                options.Add(lastArgument, match.Value);
                                lastArgument = null;
                            }
                            else
                            {
                                arguments.Add(match.Value);
                            }

                            break;
                    }

                    input = input[(match.Index + match.Length)..].Trim();
                    break;
                }
            }
        }

        if (lastArgument is not null)
        {
            options.Add(lastArgument, null);
        }

        return new Result(commands, options, arguments);
    }
}
