using System.Reflection;
using DSharpPlus.Entities;
using Instellate.Commands.Actions;
using Instellate.Commands.Attributes;
using Instellate.Commands.Commands;

namespace Instellate.Commands.Converters;

/// <summary>
/// A converter that handles enums
/// </summary>
/// <typeparam name="T"></typeparam>
public class EnumConverter<T> : IConverter<T> where T : struct, Enum
{
    private readonly IReadOnlyDictionary<string, string> _choiceNameMapping;

    public DiscordApplicationCommandOptionType Type => DiscordApplicationCommandOptionType.String;

    public EnumConverter()
    {
        string[] names = Enum.GetNames<T>();
        Type enumType = typeof(T);
        List<MemberInfo> members
            = enumType.GetMembers().Where(m => names.Contains(m.Name)).ToList();

        Dictionary<string, string> choiceNameMapping = new();
        foreach (MemberInfo member in members)
        {
            ChoiceNameAttribute? choiceName = member.GetCustomAttribute<ChoiceNameAttribute>();
            string name = choiceName?.Name ?? member.Name;
            choiceNameMapping.Add(name, member.Name);
        }

        this._choiceNameMapping = choiceNameMapping;
    }

    public CommandOption ConstructOption(CommandOptionMetadata metadata)
    {
        List<DiscordApplicationCommandOptionChoice> choices = new(this._choiceNameMapping.Count);
        foreach ((string name, string value) in this._choiceNameMapping)
        {
            choices.Add(new DiscordApplicationCommandOptionChoice(name, value));
        }

        return new CommandOption(
            metadata.Name,
            metadata.Description,
            metadata.Optional,
            metadata.Positional
        )
        {
            Type = this.Type,
            Choices = choices,
        };
    }

    public ValueTask<object?> ConvertFromObject(object? obj, IActionContext context)
    {
        if (obj is not string name)
        {
            throw new ArgumentException("Object is not string", nameof(obj));
        }

        string value = this._choiceNameMapping[name];
        return ValueTask.FromResult<object?>(Enum.Parse<T>(value));
    }

    public ValueTask<object?> ConvertFromString(Optional<string> input, IActionContext context)
    {
        if (input.TryGetValue(out string? name))
        {
            string value = this._choiceNameMapping[name];
            return ValueTask.FromResult<object?>(Enum.Parse<T>(value));
        }

        throw new ArgumentNullException(nameof(input));
    }
}
